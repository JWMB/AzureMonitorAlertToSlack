using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            if (ctx is Types.AlertContexts.LogAlertsV2AlertContext ctxV2) {
                var items = ctxV2.Condition.AllOf?.Select(o => new AlertInfo {
                        Title = alert.Data.Essentials.AlertRule,
                        Text = o.ToUserFriendlyString(),
                        TitleLink = GetTitleLink(o)
                    });
                if (items == null)
                    items = new[] { new AlertInfo{ Title = alert.Data.Essentials.AlertRule, Text = ctxV2.Condition.ToUserFriendlyString() }};
                
                slackItems.AddRange(items);
            }
            // In case we want specially tailored info rather than the generic ToUserFriendlyString:
            // else if (ctx is Types.AlertContexts.ActivityLogAlertContext ctxAL)
            // else if (ctx is Types.AlertContexts.LogAnalyticsAlertContext ctxLA)
            // else if (ctx is Types.AlertContexts.SmartAlertContext ctxSA)
            // else if (ctx is Types.AlertContexts.ServiceHealthAlertContext ctxSA)
            else
            {
                slackItems.Add(new AlertInfo{
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is Types.AlertContexts.LogAnalyticsAlertContext ctxLA ? ctxLA.LinkToFilteredSearchResultsUi?.ToString() : null
                });
            }

            if (!slackItems.Any())
                throw new Exception($"No items produced");
            return slackItems;

            async Task<string> GetText(Types.AlertContexts.LogAlertsV2.IConditionPart cond)
            {
                if (cond is Types.AlertContexts.LogAlertsV2.LogQueryCriteria lq)
                {
                }
                return "";
            }

            string? GetTitleLink(Types.AlertContexts.LogAlertsV2.IConditionPart cond)
            {
                return cond switch 
                {
                    Types.AlertContexts.LogAlertsV2.LogQueryCriteria lq =>
                        lq.LinkToSearchResultsUi?.ToString(),
                    Types.AlertContexts.LogAlertsV2.SingleResourceMultipleMetricCriteria srmm =>
                        null,
                    Types.AlertContexts.LogAlertsV2.DynamicThresholdCriteria dt =>
                        null,
                    Types.AlertContexts.LogAlertsV2.WebtestLocationAvailabilityCriteria wla =>
                        null,
                    _ => 
                        null
                };
            }
        }
    }
}