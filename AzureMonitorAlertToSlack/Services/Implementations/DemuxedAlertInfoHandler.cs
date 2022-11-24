using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts.LogAlertsV2;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;
using Azure;

namespace AzureMonitorAlertToSlack.Services.Implementations
{
    public class DemuxedAlertInfoHandler : IDemuxedAlertHandler
    {
        private readonly IAIQueryService? aiQueryService;
        public List<AlertInfo> Handled { get; private set; } = new List<AlertInfo>();

        public DemuxedAlertInfoHandler(IAIQueryService? aiQueryService)
        {
            this.aiQueryService = aiQueryService;
        }

        public virtual void ActivityLogAlertContext(Alert alert, ActivityLogAlertContext ctx)
            => HandleGeneric(alert);
        public virtual void ResourceHealthAlertContext(Alert alert, ResourceHealthAlertContext ctx)
            => HandleGeneric(alert);
        public virtual void ServiceHealthAlertContext(Alert alert, ServiceHealthAlertContext ctx)
            => HandleGeneric(alert);
        public virtual void SmartAlertContext(Alert alert, SmartAlertContext ctx)
            => HandleGeneric(alert);

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx)
            => HandleGeneric(alert);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, DynamicThresholdCriteria[] criteria)
            => HandleGenericV2(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, SingleResourceMultipleMetricCriteria[] criteria)
            => HandleGenericV2(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, WebtestLocationAvailabilityCriteria[] criteria)
            => HandleGenericV2(alert, ctx, criteria);

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, LogQueryCriteria[] criteria)
        {
            foreach (var criterion in criteria)
            {
                var item = CreateFromV2ConditionPart(alert, ctx, criterion);

                // TODO: different APIs for querying depending on provider - microsoft.insights/components vs microsoft.operationalinsights/workspaces
                // e.g. traces vs AppTraces
                // maybe by checking criterion.TargetResourceTypes?
                // log analytics [...]ResultsApi links be like https://api.loganalytics.io/v1/workspaces/{workspaceId}/query?query={query}&timespan={start}Z%2f{end}Z
                // For debugging:
                // item.Text += $"\n{criterion.TargetResourceTypes} {criterion.LinkToFilteredSearchResultsApi}";

                // TODO: we need a AIQueryServiceFactory that checks TargetResourceTypes
                if (criterion.TargetResourceTypes.Contains("/workspaces")) //microsoft.operationalinsights/workspaces
                {
                    // This is Workspace (e.g. AppTraces)
                }
                else if (criterion.TargetResourceTypes.Contains("microsoft.insights")) //microsoft.insights/components'
                {
                    // This is application insights (e.g. traces)
                }

                var additional = QueryAI(aiQueryService, criterion.SearchQuery, ctx.Condition.WindowStartTime, ctx.Condition.WindowEndTime)
                    .Result;
                if (!string.IsNullOrEmpty(additional))
                    item.Text += $"\n{additional}";

                item.TitleLink = (criterion.LinkToFilteredSearchResultsUi ?? criterion.LinkToSearchResultsUi)?.ToString();

                Push(item);
            }
        }

        public virtual void LogAnalyticsAlertContext(Alert alert, LogAnalyticsAlertContext ctx)
        {
            var dataTables = ctx.SearchResults.Tables.Select(TableHelpers.TableToDataTable);
            var renderedTable = dataTables.Any() ? RenderDataTable(dataTables.First()) : null;

            var item = CreateGeneric(alert);
            item.Text = $"{ctx.ResultCount} {ctx.OperatorToken} {ctx.Threshold}{(renderedTable == null ? "" : $"\n{renderedTable}")}";
            item.TitleLink = ctx.LinkToFilteredSearchResultsUi?.ToString();

            Push(item);
        }

        public virtual void HandleGenericV2(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart[]? conditions)
        {
            var items = conditions?.Select(o => CreateFromV2ConditionPart(alert, ctx, o)) ?? new[] { CreateFromV2ConditionPart(alert, ctx, null) };
            items.ToList().ForEach(o => Push(o));
        }

        protected virtual AlertInfo CreateFromV2ConditionPart(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart? conditionPart)
        {
            var item = CreateGeneric(alert);
            item.Text = conditionPart == null ? ctx.Condition.ToUserFriendlyString() : $"{conditionPart.ToUserFriendlyString()} ({ctx.Condition.GetUserFriendlyTimeWindowString()})";
            return item;
        }

        protected virtual AlertInfo CreateGeneric(Alert alert)
        {
            return new AlertInfo
            {
                Title = alert.Data.Essentials.AlertRule,
                Text = $"{alert.Data.AlertContext?.ToUserFriendlyString()}",
            };
        }

        protected virtual void HandleGeneric(Alert alert)
        {
            var item = CreateGeneric(alert);
            item.TitleLink = alert.Data.AlertContext is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null;
            Push(item);
        }

        protected virtual void Push(AlertInfo alert)
        {
            Handled.Add(alert);
        }

        protected static async Task<string?> QueryAI(IAIQueryService? aiQueryService, string? query, DateTimeOffset start, DateTimeOffset end)
        {
            if (aiQueryService == null || string.IsNullOrEmpty(query)) return null;
            try
            {
                return RenderDataTable(await aiQueryService.GetQueryAsDataTable(query!, start, end));
            }
            catch (Exception ex)
            {
                var errorCode = ex is RequestFailedException rfEx ? rfEx.ErrorCode : null;
                // Query:{query}\n{ex.Source}
                if (ex.Message.Contains("403 (Forbidden)"))
                    errorCode = "403";
                return $"AIQuery error - {errorCode} {ex.GetType().Name} {ex.Message}\n{ex.StackTrace}\n--{ex.InnerException?.GetType().Name} {ex.InnerException?.Message}";
            }
        }

        protected static string RenderDataTable(DataTable dt)
        {
            var stringifyer = new ConvertToString(40);
            return $"```\n{TableHelpers.TableToMarkdown(dt, (obj, type) => stringifyer.Convert(obj, type), 10)}\n```";
        }
    }
}
