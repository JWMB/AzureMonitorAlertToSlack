using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts.LogAlertsV2;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;
using Azure;
using AzureMonitorAlertToSlack.LogQuery;
using AzureMonitorAlertToSlack.Slack;

namespace AzureMonitorAlertToSlack.Alerts
{
    public class DemuxedAlertHandler<T, TPart> : IDemuxedAlertHandler<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        private readonly ILogQueryServiceFactory? logQueryServiceFactory;
        public T Handled { get; private set; } = new T();

        public DemuxedAlertHandler(ILogQueryServiceFactory? logQueryServiceFactory = null)
        {
            this.logQueryServiceFactory = logQueryServiceFactory;
        }

        public virtual void ActivityLogAlertContext(Alert alert, ActivityLogAlertContext ctx) => HandleGeneric(alert);
        public virtual void ResourceHealthAlertContext(Alert alert, ResourceHealthAlertContext ctx) => HandleGeneric(alert);
        public virtual void ServiceHealthAlertContext(Alert alert, ServiceHealthAlertContext ctx) => HandleGeneric(alert);
        public virtual void SmartAlertContext(Alert alert, SmartAlertContext ctx) => HandleGeneric(alert);

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx) => HandleGeneric(alert);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, DynamicThresholdCriteria[] criteria) => HandleGenericV2(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, SingleResourceMultipleMetricCriteria[] criteria) => HandleGenericV2(alert, ctx, criteria);
        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, WebtestLocationAvailabilityCriteria[] criteria) => HandleGenericV2(alert, ctx, criteria);

        public virtual void LogAlertsV2AlertContext(Alert alert, LogAlertsV2AlertContext ctx, LogQueryCriteria[] criteria)
        {
            UpdateGeneric(alert, null);
            foreach (var criterion in criteria)
            {
                var item = CreatePartFromV2ConditionPart(alert, ctx, criterion);
                // $"{alert.Data.AlertContext?.ToUserFriendlyString()}"
                var additional = QueryAI(criterion.TargetResourceTypes, criterion.SearchQuery, ctx.Condition.WindowStartTime, ctx.Condition.WindowEndTime)
                    .Result;
                if (!string.IsNullOrEmpty(additional))
                    item.Text += $"\n{SlackHelpers.Escape(additional!)}";

                item.TitleLink = (criterion.LinkToFilteredSearchResultsUi ?? criterion.LinkToSearchResultsUi)?.ToString();

                Handled.Parts.Add(item);
            }
        }

        public virtual void LogAnalyticsAlertContext(Alert alert, LogAnalyticsAlertContext ctx)
        {
            var dataTables = ctx.SearchResults.Tables.Select(TableHelpers.TableToDataTable);
            var renderedTable = dataTables.Any() ? RenderDataTable(dataTables.First()) : null;

            UpdateGeneric(alert, $"{ctx.ResultCount} {ctx.OperatorToken} {ctx.Threshold}{(renderedTable == null ? "" : $"\n{renderedTable}")}");
            Handled.TitleLink = ctx.LinkToFilteredSearchResultsUi?.ToString();
        }

        public virtual void HandleGenericV2(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart[]? conditions)
        {
            UpdateGeneric(alert, null);
            var items = conditions?.Select(o => CreatePartFromV2ConditionPart(alert, ctx, o)) ?? new[] { CreatePartFromV2ConditionPart(alert, ctx, null) };
            Handled.Parts.AddRange(items);
        }

        protected virtual TPart CreatePartFromV2ConditionPart(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart? conditionPart)
        {
            var part = new TPart();
            part.Text = conditionPart == null ? ctx.Condition.ToUserFriendlyString() : $"{conditionPart.ToUserFriendlyString()} ({ctx.Condition.GetUserFriendlyTimeWindowString()})";
            return part;
        }

        protected virtual void UpdateGeneric(Alert alert, string? createPartWithText)
        {
            Handled.CustomProperties = alert.Data.CustomProperties;
            Handled.Title = alert.Data.Essentials.AlertRule;

            if (createPartWithText != null)
            {
                Handled.Parts.Add(
                    new TPart
                    {
                        Text = createPartWithText,
                    }
                );
            }
        }

        protected virtual void HandleGeneric(Alert alert)
        {
            UpdateGeneric(alert, $"{alert.Data.AlertContext?.ToUserFriendlyString()}");
            Handled.TitleLink = alert.Data.AlertContext is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null;
        }

        protected async Task<string?> QueryAI(string targetResourceTypes, string? query, DateTimeOffset start, DateTimeOffset end)
        {
            if (logQueryServiceFactory == null || query == null)
                return null;

            var logQueryService = logQueryServiceFactory.CreateLogQueryService(targetResourceTypes);
            if (logQueryService == null || string.IsNullOrEmpty(query))
                return null;

            var cancellation = logQueryServiceFactory.GetCancellationToken();

            try
            {
                var table = await logQueryService.GetQueryAsDataTable(query, start, end, cancellation);
                return RenderDataTable(table);
            }
            catch (Exception ex)
            {
                var errorCode = ex is RequestFailedException rfEx ? rfEx.ErrorCode : null;
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
