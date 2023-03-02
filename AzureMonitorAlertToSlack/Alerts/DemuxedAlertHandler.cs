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
    /// <summary>
    /// Not thread safe - instantiate for each use (transient), can e.g. be registered as Func<>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPart"></typeparam>
    public class DemuxedAlertHandler<T, TPart> : IDemuxedAlertHandler<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        protected readonly ILogQueryServiceFactory? logQueryServiceFactory;

        // TODO: we'd like this to be T? but difficult with .netstandard 2.0 (C# version >= 9)
        public T Result { get; private set; } = new T();

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
            CreateResult(alert, null);

            foreach (var criterion in criteria)
            {
                var item = CreatePartFromV2ConditionPart(alert, ctx, criterion);
                var additional = QueryAIToText(criterion.TargetResourceTypes, criterion.SearchQuery, ctx.Condition.WindowStartTime, ctx.Condition.WindowEndTime)
                    .Result;
                if (!string.IsNullOrEmpty(additional))
                    item.Text += $"\n{SlackHelpers.Escape(additional!)}";

                item.TitleLink = (criterion.LinkToFilteredSearchResultsUi ?? criterion.LinkToSearchResultsUi)?.ToString();

                Result.Parts.Add(item);
            }

            PostProcess();
        }

        public virtual void LogAnalyticsAlertContext(Alert alert, LogAnalyticsAlertContext ctx)
        {
            var dataTables = ctx.SearchResults.Tables.Select(TableHelpers.TableToDataTable);
            var renderedTable = dataTables.Any() ? RenderDataTable(dataTables.First()) : null;

            CreateResult(alert, $"{ctx.ResultCount} {ctx.OperatorToken} {ctx.Threshold}{(renderedTable == null ? "" : $"\n{renderedTable}")}");
            Result.TitleLink = ctx.LinkToFilteredSearchResultsUi?.ToString();

            PostProcess();
        }

        public virtual void HandleGenericV2(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart[]? conditions)
        {
            CreateResult(alert, null);

            var parts = conditions?.Select(o => CreatePartFromV2ConditionPart(alert, ctx, o)) ?? new[] { CreatePartFromV2ConditionPart(alert, ctx, null) };
            Result.Parts.AddRange(parts);

            PostProcess();
        }

        protected virtual TPart CreatePart() => new TPart();

        protected virtual TPart CreatePartFromV2ConditionPart(Alert alert, LogAlertsV2AlertContext ctx, IConditionPart? conditionPart)
        {
            var part = CreatePart();
            part.Text = conditionPart == null ? ctx.Condition.ToUserFriendlyString() : $"{conditionPart.ToUserFriendlyString()} ({ctx.Condition.GetUserFriendlyTimeWindowString()})";
            return part;
        }

        protected virtual void CreateResult(Alert alert, string? createPartWithText)
        {
            var result = new T();
            result.CustomProperties = alert.Data.CustomProperties;
            result.Title = alert.Data.Essentials.AlertRule;

            if (createPartWithText != null)
            {
                var part = CreatePart();
                part.Text = createPartWithText;
                result.Parts.Add(part);
            }
            Result = result;
        }

        protected virtual void HandleGeneric(Alert alert)
        {
            CreateResult(alert, $"{alert.Data.AlertContext?.ToUserFriendlyString()}");
            Result.TitleLink = alert.Data.AlertContext is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null;
            PostProcess();
        }

        protected virtual void PostProcess() { }

        protected virtual async Task<string?> QueryAIToText(string targetResourceTypes, string? query, DateTimeOffset start, DateTimeOffset end)
        {
            try
            {
                var table = await QueryAI(targetResourceTypes, query, start, end);
                return table == null ? null : RenderDataTable(table);
            }
            catch (TaskCanceledException tcEx)
            {
                return "AIQuery - timeout";
            }
            catch (Exception ex)
            {
                var errorCode = ex is RequestFailedException rfEx ? rfEx.ErrorCode : null;
                if (ex.Message.Contains("403 (Forbidden)"))
                    errorCode = "403";
                return $"AIQuery error - {errorCode} {ex.GetType().Name} {ex.Message}\n{ex.StackTrace}\n--{ex.InnerException?.GetType().Name} {ex.InnerException?.Message}";
            }
        }

        protected virtual async Task QueryAIToImage()
        {

        }

        protected virtual async Task<DataTable?> QueryAI(string targetResourceTypes, string? query, DateTimeOffset start, DateTimeOffset end)
        {
            if (logQueryServiceFactory == null || query == null)
                return null;

            var logQueryService = logQueryServiceFactory.CreateLogQueryService(targetResourceTypes);
            if (logQueryService == null || string.IsNullOrEmpty(query))
                return null;

            var cancellation = logQueryServiceFactory.GetCancellationToken();

            return await logQueryService.GetQueryAsDataTable(query, start, end, cancellation);
        }

        protected virtual string RenderDataTable(DataTable dt)
        {
            var stringifyer = new ConvertToString(40);
            return $"```\n{TableHelpers.TableToMarkdown(dt, (obj, type) => stringifyer.Convert(obj, type), 10)}\n```";
        }
    }
}
