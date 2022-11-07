using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AzureAlerts2Slack
{

   public class AlertInfo
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }

        public static List<AlertInfo> Process(string requestBody)
        {
            var slackItems = new List<AlertInfo>();

            var alert = Types.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new Exception($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            if (ctx is Types.AlertContexts.LogAlertsV2AlertContext ctxV2) {
                if (ctxV2.Condition.AllOf is Types.AlertContexts.LogAlertsV2.LogQueryCriteriaCondition[] condLQ) {
                    slackItems.AddRange(condLQ.Select(o => new AlertInfo {
                        Title = alert.Data.Essentials.AlertRule,
                        Text = $"{ctxV2.Condition.ToUserFriendlyString()}",
                        TitleLink = $"{o.LinkToFilteredSearchResultsUi}"
                    }));
                }
            }
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
        }
    }
}