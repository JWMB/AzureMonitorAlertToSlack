using System;
using System.Collections.Generic;
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
            if (string.IsNullOrEmpty(requestBody))
                throw new ArgumentNullException(nameof(requestBody));
        
            Alert input;
            try
            {
                var tempTest = JsonConvert.DeserializeObject<Alert>(requestBody);
                if (tempTest == null)
                    throw new ArgumentException($"Deserialized body is null");
                input = tempTest;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Could not deserialize body: {ex.Message}");
            }

            var slackItems = new List<AlertInfo>();

            switch (input.SchemaId)
            {
                case "azureMonitorCommonAlertSchema":
                    if (input.Data == null)
                        throw new ArgumentException($"body.data is null");
                    var essentials = input.Data.Essentials;
                    if (essentials == null)
                        throw new ArgumentException($"body.data.essentials is null");

                    if (input.Data.AlertContext == null)
                        throw new ArgumentException($"body.data.alertContext is null");

                    dynamic alertContext = input.Data.AlertContext;
                    var properties = alertContext.properties;
                    //var color = properties?.color;
                    // var description = essentials.Description;

                    switch (essentials.SignalType)
                    {
                        case "Metric":
                            if (essentials.MonitoringService != "Log Analytics")
                                throw new NotImplementedException($"Metric / monitoringService not supported: {essentials.MonitoringService}\n{requestBody}");
                            KIStudy.AlertContextModels.MonitorAI.AlertContext ctxAI = ParseOrThrow<KIStudy.AlertContextModels.MonitorAI.AlertContext>(alertContext);
                            slackItems.Add(new AlertInfo{
                                Title = essentials.AlertRule,
                                Text = $"{ctxAI.AlertType} {ctxAI.ResultCount} {ctxAI.Operator} {ctxAI.Threshold}",
                                TitleLink = $"{ctxAI.LinkToFilteredSearchResultsUi}"
                            });
                            break;

                        case "Activity Log":
                            var ctxAL = ParseOrThrow<KIStudy.AlertContextModels.ActivityLog.AlertContext>(alertContext);
                            slackItems.Add(new AlertInfo{ Title = essentials.AlertRule, Text = $"{ctxAL.OperationName} {ctxAL.Status}" });
                            break;

                        case "Log":
                            if (new List<string> { "Log Alerts V2", "Platform" }.Contains(essentials.MonitoringService) == false)
                                throw new NotImplementedException($"Log / monitoringService not supported: {essentials.MonitoringService}\n{requestBody}");
                            var ctxLog = ParseOrThrow<KIStudy.AlertContextModels.LogAlertsV2.AlertContext>(alertContext);

                            var condition = ctxLog.Condition; //alertContext.condition;
                            //windowSize = condition.windowSize;
                            // "windowStartTime": "2022-11-04T15:10:45.286Z",
                            // "windowEndTime": "2022-11-04T15:10:45.286Z"
                            //"conditionType": "LogQueryCriteria",

                            foreach (var cond in condition.AllOf)
                                slackItems.Add(new AlertInfo
                                {
                                    Title = essentials.AlertRule,
                                    TitleLink = $"{cond.LinkToSearchResultsUi}",
                                    Text = $"{properties?["msg"]} {cond.SearchQuery}"
                                });
                            //"@{triggerBody()?['data']?['alertContext']?['properties']?['msg']} \nCount @{items('For_each')['metricValue']} @{items('For_each')['operator']} @{items('For_each')['threshold']} (time window: @{triggerBody()['data']['alertContext']['condition']['windowStartTime']} - @{triggerBody()['data']['alertContext']['condition']['windowEndTime']})",
                            break;
                        default:
                            throw new NotImplementedException($"signalType not supported: {essentials.SignalType}\n{requestBody}");
                    }
                    break;

                default:
                    throw new NotImplementedException($"schemaId not supported: {input.SchemaId}\n{requestBody}");
            }

            return slackItems;
        }

        private static T ParseOrThrow<T>(object data)
        {
            var asString = data is string str ? str : JsonConvert.SerializeObject(data);
            try
            {
                var ctx = JsonConvert.DeserializeObject<T>(asString);
                if (ctx == null)
                    throw new Exception($"couldn't deserialize {asString}");
                return ctx;
            }
            catch (Exception ex)
            {
                throw new Exception($"couldn't deserialize {asString} {ex.Message}");
            }
        }
    }
}