using AzureMonitorAlertToSlack;
using AzureMonitorAlertToSlack.Alerts;
using AzureMonitorAlertToSlack.Slack;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace AzureMonitorAlertToSlack.Tests
{
    public class SlackTests
    {
        private IConfigurationRoot config;

        public SlackTests()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<AppInsightsQueryTests>();
            config = builder.Build();
        }

        [SkippableFact]
        public async Task Slack_ComplexLink()
        {
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);

            var item = new SummarizedAlertPart
            {
                Title = "UNIT TEST - ignore",
                TitleLink = "https://portal.azure.com/#@aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/blade/Microsoft_Azure_Monitoring_Logs/LogsBlade/source/Alerts.EmailLinks/scope/{\"resources\"%3A%5B{\"resourceId\"%3A\"%2Fsubscriptions%2Faaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa%2FresourceGroups%2Faaaaaaa%2Fproviders%2Fmicrosoft.operationalinsights%2Fworkspaces%2FDefaultWorkspace-aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa-NEU\"}%5D}/q/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa%3D%3D/prettify/1/timespan/2022-11-25T09%3a25%3a37.0000000Z%2f2022-11-25T09%3a30%3a37.0000000Z",
                Text = "AppTraces\n| where SeverityLevel >= 2\n| project TimeGenerated, Message\n\n/MMC: 3 > 0 (09:25:37 - 09:30:37 UTC:+00:00)\nAIQuery error - ArgumentException Unable to cast object of type 'Azure.Monitor.Query.Models.LogsTableRow' to type 'System.IConvertible'.Couldn't store <[\"2022-11-25T09:25:54.1391113Z\",\"Here is a Error\"]> in TimeGenerated Column. Expected type is DateTime.\n at System.Data.DataColumn.set_Item(Int32 record, Object value)\r\n at System.Data.DataTable.NewRecordFromArray(Object[] value)\r\n at System.Data.DataRowCollection.Add(Object[] values)\r\n at LogAnalyticsQueryService.ConvertToDataTable(LogsTable table) in D:\\a\\AzureAlerts2Slack\\AzureAlerts2Slack\\AzureMonitorAlertToSlack\\Services\\LogQuery\\LogAnalyticsQueryService.cs:line 50\r\n at LogAnalyticsQueryService.GetQueryAsDataTable(String query, DateTimeOffset start, DateTimeOffset end) in D:\\a\\AzureAlerts2Slack\\AzureAlerts2Slack\\AzureMonitorAlertToSlack\\Services\\LogQuery\\LogAnalyticsQueryService.cs:line 37\r\n at AzureMonitorAlertToSlack.Implementations.DemuxedAlertInfoHandler.QueryAI(ILogQueryService logQueryService, String query, DateTimeOffset start, DateTimeOffset end) in D:\\a\\AzureAlerts2Slack\\AzureAlerts2Slack\\AzureMonitorAlertToSlack\\Services\\Implementations\\DemuxedAlertInfoHandler.cs:line 114\n--InvalidCastException Unable to cast object of type 'Azure.Monitor.Query.Models.LogsTableRow' to type 'System.IConvertible'.\\n{\"schemaId\":\"azureMonitorCommonAlertSchema\",\"data\":{\"essentials\":{\"alertId\":\"/subscriptions/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/providers/Microsoft.AlertsManagement/alerts/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"alertRule\":\"WS Log query problems\",\"severity\":\"Sev1\",\"signalType\":\"Log\",\"monitorCondition\":\"Fired\",\"monitoringService\":\"Log Alerts V2\",\"alertTargetIDs\":[\"/subscriptions/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/resourcegroups/kistudy/providers/microsoft.operationalinsights/workspaces/defaultworkspace-aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa-neu\"],\"configurationItems\":[\"/subscriptions/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/resourceGroups/KIStudy/providers/microsoft.operationalinsights/workspaces/DefaultWorkspace-aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa-NEU\"],\"originAlertId\":\"aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"firedDateTime\":\"2022-11-25T09:31:09.1507473Z\",\"description\":\"\",\"essentialsVersion\":\"1.0\",\"alertContextVersion\":\"1.0\"},\"alertContext\":{\"properties\":{\"JonasTest\":\"123\"},\"conditionType\":\"LogQueryCriteria\",\"condition\":{\"windowSize\":\"PT5M\",\"allOf\":[{\"searchQuery\":\"AppTraces\\n| where SeverityLevel >= 2\\n| project TimeGenerated, Message\\n\\n\",\"metricMeasureColumn\":null,\"targetResourceTypes\":\"['microsoft.operationalinsights/workspaces']\",\"operator\":\"GreaterThan\",\"threshold\":\"0\",\"timeAggregation\":\"Count\",\"dimensions\":[],\"metricValue\":3.0,\"failingPeriods\":{\"numberOfEvaluationPeriods\":1,\"minFailingPeriodsToAlert\":1},\"linkToSearchResultsUI\":\"https://portal.azure.com#@aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/blade/Microsoft_Azure_Monitoring_Logs/LogsBlade/source/Alerts.EmailLinks/scope/%7B%22resources%22%3A%5B%7B%22resourceId%22%3A%22%2Fsubscriptions%2Faaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa%2FresourceGroups%2Faaaaaa%2Fproviders%2Fmicrosoft.operationalinsights%2Fworkspaces%2FDefaultWorkspace-aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa-NEU%22%7D%5D%7D/q/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaxaaaaaaaaaaaaaaaaaaaaaaaaaaa%3D%3D/prettify/1/timespan/2022-11-25T09%3a25%3a37.0000000Z%2f2022-11-25T09%3a30%3a37.0000000Z\",\"linkToFilteredSearchResultsUI\":\"https://portal.azure.com#@aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/blade/Microsoft_Azure_Monitoring_Logs/LogsBlade/source/Alerts.EmailLinks/scope/%7B%22resources%22%3A%5B%7B%22resourceId%22%3A%22%2Fsubscriptions%2Fe5d4ca12-e670-4255-84a4-78223ece667a%2FresourceGroups%2FKIStudy%2Fproviders%2Fmicrosoft.operationalinsights%2Fworkspaces%2FDefaultWorkspace-aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa-NEU%22%7D%5D%7D/q/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa%3D%3D/prettify/1/timespan/2022-11-25T09%3a25%3a37.0000000Z%2f2022-11-25T09%3a30%3a37.0000000Z\",\"linkToSearchResultsAPI\":\"https://api.loganalytics.io/v1/workspaces/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/query?query=AppTraces%0A%7C%20where%20SeverityLevel%20%3E%3D%202%0A%7C%20project%20TimeGenerated%2C%20Message&timespan=2022-11-25T09%3a25%3a37.0000000Z%2f2022-11-25T09%3a30%3a37.0000000Z\",\"linkToFilteredSearchResultsAPI\":\"https://api.loganalytics.io/v1/workspaces/aaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/query?query=AppTraces%0A%7C%20where%20SeverityLevel%20%3E%3D%202%0A%7C%20project%20TimeGenerated%2C%20Message&timespan=2022-11-25T09%3a25%3a37.0000000Z%2f2022-11-25T09%3a30%3a37.0000000Z\"}],\"windowStartTime\":\"2022-11-25T09:25:37Z\",\"windowEndTime\":\"2022-11-25T09:30:37Z\"}},\"customProperties\":null}}"
            };

            item.Text = SlackHelpers.Escape(item.Text);
            var messageFactory = new SlackMessageFactory<SummarizedAlert, SummarizedAlertPart>();
            var msg = messageFactory.CreateMessage(new SummarizedAlert { Parts = new[] { item }.ToList() });

            ISlackClient sender = new SlackClient(SlackClient.Configure(new HttpClient()), new SlackSettings { DefaultWebhook = config["SlackWebhookUrl"] });
            await Should.NotThrowAsync(async () => await sender.Send(msg));
        }

        [SkippableFact]
        public async Task Slack_Images()
        {
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);

            var item = new SummarizedAlertPart
            {
                Title = "UNIT TEST - ignore (with images)",
            };

            item.Text = SlackHelpers.Escape(item.Text);
            var messageFactory = new SlackMessageFactory<SummarizedAlert, SummarizedAlertPart>();
            var msg = messageFactory.CreateMessage(new SummarizedAlert
            {
                Parts = new[] { item }.ToList(),
                ImageUrls = new List<Uri> {
                    new Uri("https://th.bing.com/th/id/R.2784f9ce44da8c6c6567a5c5cace464c?rik=igh8f%2bM1h8VPkw&pid=ImgRaw&r=0"),
                    new Uri("https://i.pinimg.com/originals/7f/f8/3e/7ff83e001e859b9b9ddb4ec4388b8e1c.jpg")
                }
            });

            ISlackClient sender = new SlackClient(SlackClient.Configure(new HttpClient()), new SlackSettings { DefaultWebhook = config["SlackWebhookUrl"] });
            await Should.NotThrowAsync(async () => await sender.Send(msg));
        }

    }
}
