using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Types;
using Types.AlertContexts;
using Types.AlertContexts.LogAlertsV2;

namespace AzureAlerts2Slack
{

   public class AlertInfo
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }

        public static async Task<List<AlertInfo>> Process(string requestBody)
        {
            var slackItems = new List<AlertInfo>();

            var alert = Types.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new Exception($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            if (ctx is LogAlertsV2AlertContext ctxV2)
            {
                await foreach (var item in CreateFromLogAlertsV2(alert, ctxV2))
                    slackItems.Add(item);
                //var items = ctxV2.Condition.AllOf?.ToAsyncEnumerable().SelectAwait(async o => new AlertInfo
                //{
                //    Title = alert.Data.Essentials.AlertRule,
                //    Text = await GetText(ctx, o),
                //    TitleLink = GetTitleLink(o)
                //});

                    //if (items == null)
                    //    slackItems.Add(new AlertInfo { Title = alert.Data.Essentials.AlertRule, Text = ctxV2.Condition.ToUserFriendlyString() });
                    //else
                    //    await foreach (var item in items)
                    //        slackItems.Add(item);
            }
            // In case we want specially tailored info rather than the generic ToUserFriendlyString:
            // else if (ctx is ActivityLogAlertContext ctxAL)
            // else if (ctx is LogAnalyticsAlertContext ctxLA)
            // else if (ctx is SmartAlertContext ctxSA)
            // else if (ctx is ServiceHealthAlertContext ctxSA)
            else
            {
                slackItems.Add(new AlertInfo
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLA ? ctxLA.LinkToFilteredSearchResultsUi?.ToString() : null
                });
            }

            if (!slackItems.Any())
                throw new Exception($"No items produced");
            return slackItems;
        }

        public static async IAsyncEnumerable<AlertInfo> CreateFromLogAlertsV2(Alert alert, LogAlertsV2AlertContext ctxV2)
        {
            var items = ctxV2.Condition.AllOf?.ToAsyncEnumerable().SelectAwait(async o => new AlertInfo
            {
                Title = alert.Data.Essentials.AlertRule,
                Text = await GetText(o),
                TitleLink = GetTitleLink(o)
            });

            if (items == null)
                yield return new AlertInfo { Title = alert.Data.Essentials.AlertRule, Text = ctxV2.Condition.ToUserFriendlyString() };
            else
                await foreach (var item in items)
                    yield return item;


            async Task<string> GetText(IConditionPart cond)
            {
                if (cond is LogQueryCriteria lq)
                {
                    if (!string.IsNullOrEmpty(lq.SearchQuery))
                        await new AIQuery().XI(lq.SearchQuery, ctxV2.Condition.WindowStartTime, ctxV2.Condition.WindowEndTime);
                }
                return "";
            }

            string? GetTitleLink(IConditionPart cond)
            {
                return cond switch
                {
                    LogQueryCriteria lq =>
                        lq.LinkToSearchResultsUi?.ToString(),
                    SingleResourceMultipleMetricCriteria srmm =>
                        null,
                    DynamicThresholdCriteria dt =>
                        null,
                    WebtestLocationAvailabilityCriteria wla =>
                        null,
                    _ =>
                        null
                };
            }
        }
    }
}