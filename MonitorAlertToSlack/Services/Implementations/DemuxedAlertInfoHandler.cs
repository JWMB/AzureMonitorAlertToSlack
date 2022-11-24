using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts.LogAlertsV2;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;

namespace MonitorAlertToSlack.Services.Implementations
{
    public class DemuxedAlertInfoHandler : IDemuxedAlertHandler
    {
        private readonly IAIQueryService? aiQueryService;
        public List<AlertInfo> Handled { get; private set; } = new List<AlertInfo>();

        public DemuxedAlertInfoHandler(IAIQueryService? aiQueryService)
        {
            this.aiQueryService = aiQueryService;
        }

        public virtual void ActivityLogAlertContext(Alert alert, ActivityLogAlertContext ctx) => HandleGeneric(alert);
        public virtual void ResourceHealthAlertContext(Alert alert, ResourceHealthAlertContext ctx) => HandleGeneric(alert);
        public virtual void ServiceHealthAlertContext(Alert alert, ServiceHealthAlertContext ctx) => HandleGeneric(alert);
        public virtual void SmartAlertContext(Alert alert, SmartAlertContext ctx) => HandleGeneric(alert);

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx) => HandleGeneric(alert);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, DynamicThresholdCriteria[] criteria) => HandleGeneric(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, SingleResourceMultipleMetricCriteria[] criteria) => HandleGeneric(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, WebtestLocationAvailabilityCriteria[] criteria) => HandleGeneric(alert, ctx, criteria);

        public virtual void HandleGeneric(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart[]? conditions)
        {
            if (conditions?.Any() == true)
            {
                foreach (var item in conditions)
                {
                    Push(CreateFromV2ConditionPart(alert, ctx, item));
                }
            }
            else
            {
                Push(CreateFromV2ConditionPart(alert, ctx, null));
            }
        }

        protected virtual AlertInfo CreateFromV2ConditionPart(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart? conditionPart)
        {
            return new AlertInfo
            {
                Title = alert.Data.Essentials.AlertRule,
                Text = conditionPart == null ? ctx.Condition.ToUserFriendlyString() : $"{conditionPart.ToUserFriendlyString()} ({ctx.Condition.GetUserFriendlyTimeWindowString()})",
                TitleLink = null
            };
        }

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, LogQueryCriteria[] criteria)
        {
            foreach (var criterion in criteria)
            {
                string? additional = null;
                if (!string.IsNullOrEmpty(criterion.SearchQuery) && aiQueryService != null)
                    additional = QueryAI(aiQueryService, criterion.SearchQuery, ctx.Condition.WindowStartTime, ctx.Condition.WindowEndTime).Result;

                var item = CreateFromV2ConditionPart(alert, ctx, criterion);
                if (!string.IsNullOrEmpty(additional))
                    item.Text += $"\n{additional}";
                item.TitleLink = (criterion.LinkToFilteredSearchResultsUi ?? criterion.LinkToSearchResultsUi)?.ToString();

                Push(item);
            }
        }

        public virtual void LogAnalyticsAlertContext(Alert alert, LogAnalyticsAlertContext ctx)
        {
            var dataTables = ctx.SearchResults.Tables.Select(o =>
            {
                var dt = new DataTable(o.Name);
                foreach (var col in o.Columns)
                    dt.Columns.Add(new DataColumn(col.Name, typeof(string))); // TODO: parse col.Type to actual types

                foreach (var row in o.Rows)
                {
                    var dr = dt.NewRow();
                    dr.ItemArray = row;
                    dt.Rows.Add(dr);
                }
                return dt;
            });
            var renderedTable = dataTables.Any() ? RenderDataTable(dataTables.First()) : null;

            Push(new AlertInfo
            {
                Title = alert.Data.Essentials.AlertRule,
                Text = $"{ctx.ResultCount} {ctx.OperatorToken} {ctx.Threshold}{(renderedTable == null ? "" : $"\n{renderedTable}")}",
                TitleLink = ctx.LinkToFilteredSearchResultsUi?.ToString()
            });
        }

        public virtual void HandleGeneric(Alert alert)
        {
            Push(new AlertInfo
            {
                Title = alert.Data.Essentials.AlertRule,
                Text = $"{alert.Data.AlertContext?.ToUserFriendlyString()}",
                TitleLink = alert.Data.AlertContext is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
            });
        }

        protected virtual void Push(AlertInfo alert)
        {
            Handled.Add(alert);
        }

        protected static async Task<string> QueryAI(IAIQueryService aiQueryService, string query, DateTimeOffset start, DateTimeOffset end)
        {
            return RenderDataTable(await aiQueryService.GetQueryAsDataTable(query, start, end));
        }

        protected static string RenderDataTable(DataTable dt)
        {
            var stringifyer = new ConvertToString(40);
            return $"```\n{TableHelpers.TableToMarkdown(dt, (obj, type) => stringifyer.Convert(obj, type), 10)}\n```";
        }
    }
}
