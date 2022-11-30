using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts.LogAlertsV2;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;
using Azure;
using System.Threading;

namespace AzureMonitorAlertToSlack.Services.Implementations
{
    public class DemuxedAlertInfoHandler : IDemuxedAlertHandler
    {
        private readonly ILogQueryServiceFactory? logQueryServiceFactory;
        public List<AlertInfo> Handled { get; private set; } = new List<AlertInfo>();

        public DemuxedAlertInfoHandler(ILogQueryServiceFactory? logQueryServiceFactory)
        {
            this.logQueryServiceFactory = logQueryServiceFactory;
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

                if (logQueryServiceFactory != null && !string.IsNullOrEmpty(criterion.SearchQuery))
                {
                    var service = logQueryServiceFactory.CreateLogQueryService(criterion.TargetResourceTypes);
                    var additional = QueryAI(service, criterion.SearchQuery, ctx.Condition.WindowStartTime, ctx.Condition.WindowEndTime)
                        .Result;
                    if (!string.IsNullOrEmpty(additional))
                        item.Text += $"\n{SlackHelpers.Escape(additional!)}";
                }
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

        protected static async Task<string?> QueryAI(ILogQueryService? logQueryService, string? query, DateTimeOffset start, DateTimeOffset end)
        {
            if (logQueryService == null || string.IsNullOrEmpty(query)) return null;

            var cancelSrc = new CancellationTokenSource();
            // TODO:
            var timeoutString = Environment.GetEnvironmentVariable("LogQueryTimeout");
            if (string.IsNullOrEmpty(timeoutString)) timeoutString = "20";
            cancelSrc.CancelAfter(TimeSpan.FromSeconds(int.Parse(timeoutString)));

            try
            {
                var table = await logQueryService.GetQueryAsDataTable(query!, start, end, cancelSrc.Token);
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
