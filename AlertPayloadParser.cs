using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KIStudy
{

   public class AlertInfo
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }
    }
    public class AlertPayloadParser
    {
        public static List<AlertInfo> Parse(string requestBody)
        {
            var slackItems = new List<AlertInfo>();

            var alertx = Types.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alertx?.Data.AlertContext;
            if (alertx == null || ctx == null)
                throw new Exception($"Not supported: {alertx?.Data?.Essentials?.MonitoringService}");

            if (ctx is Types.AlertContexts.LogAlertsV2AlertContext ctxV2) {
                if (ctxV2.Condition.AllOf is Types.AlertContexts.LogAlertsV2.LogQueryCriteriaCondition[] condLQ) {
                    slackItems.AddRange(condLQ.Select(o => new AlertInfo {
                        Title = alertx.Data.Essentials.AlertRule,
                        Text = $"{ctxV2.Condition.ToUserFriendlyString()}", //o.ToUserFriendlyString()
                        TitleLink = $"{o.LinkToFilteredSearchResultsUi}"
                    }));
                }
            }
            else if (ctx is Types.AlertContexts.ActivityLogAlertContext ctxAL)
            {
                slackItems.Add(new AlertInfo{
                    Title = alertx.Data.Essentials.AlertRule,
                    Text = $"{ctxAL.ToUserFriendlyString()}",
                });
            }
            else if (ctx is Types.AlertContexts.LogAnalyticsAlertContext ctxLA)
            {
                slackItems.Add(new AlertInfo{
                    Title = alertx.Data.Essentials.AlertRule,
                    Text = $"{ctxLA.ToUserFriendlyString()}",
                    TitleLink = $"{ctxLA.LinkToFilteredSearchResultsUi}"

                });
            }
            // else if (ctx is Types.AlertContexts.SmartAlertContext ctxSA)
            // { }
            // else if (ctx is Types.AlertContexts.ServiceHealthAlertContext ctxSA)
            // { }
            else
            {
                slackItems.Add(new AlertInfo{
                    Title = alertx.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                });
            }

            if (!slackItems.Any())
                throw new Exception($"No items produced");
            return slackItems;
        }
    }
}