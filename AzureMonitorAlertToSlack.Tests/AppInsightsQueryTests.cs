using Microsoft.Extensions.Configuration;
using static AzureMonitorAlertToSlack.LogQuery.AppInsightsQueryService;
using Shouldly;
using System.Data;
using AzureMonitorAlertToSlack.LogQuery;
using SlackNet;

namespace AzureMonitorAlertToSlack.Tests
{
    public class AppInsightsQueryTests
    {
        [SkippableFact]
        public async Task AppInsightsQuery_ActualServiceCall_Success()
        {
            var service = CreateRealServiceSkippable();

            var projection = new[] { "timestamp", "message" };
            var q = $@"
traces
| where severityLevel >= 2
| where timestamp > ago(2d)
| project {string.Join(",", projection)}
";
            var result = await service.GetQueryAsDataTable(q, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);

            result.Columns.OfType<DataColumn>().Select(o => o.ColumnName).ShouldBe(projection);
        }

        [SkippableFact]
        public async Task AppInsightsQuery_ActualServiceCall_Error()
        {
            var service = CreateRealServiceSkippable();

            var q = "tracesx";
            (await Should.ThrowAsync<Exception>(async () => await service.GetQueryAsDataTable(q, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow)))
                .Message.ShouldContain("BadArgumentError");
        }

        [SkippableFact]
        public async Task LogAnalyticsQuery_ActualServiceCall_Success()
        {
            // Just for local testing of the client...
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);

            var settings = PrepareLogAnalyticsQuery();
            var service = new LogAnalyticsQueryService(settings);

            var q = "AppTraces | project TimeGenerated, Message";
            var result = await service.GetQueryAsDataTable(q, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
            result.Columns.OfType<DataColumn>().Count().ShouldNotBe(0);
        }

        [SkippableFact]
        public async Task LogAnalyticsQueryRaw_ActualServiceCall_Success()
        {
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);

            var settings = PrepareLogAnalyticsQuery();
            var service = new LogAnalyticsQueryServiceRaw(new LogAnalyticsQueryServiceRaw.LogAnalyticsClient(new HttpClient()), settings);

            var q = "AppTraces | project TimeGenerated, Message";
            var result = await service.GetQueryAsDataTable(q, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
            result.Columns.OfType<DataColumn>().Count().ShouldNotBe(0);
        }

        private LogAnalyticsQuerySettings PrepareLogAnalyticsQuery()
        {
            var config = CreateConfig();
            // TODO: Auth problems from VS
            // https://stackoverflow.com/questions/62398069/defaultazurecredential-in-visual-studio-dev-fails-to-find-a-suitable-user
            // https://github.com/Azure/azure-sdk-for-net/issues/10816
            var envVarName = "AZURE_USERNAME";
            var azureUsername = config[envVarName];
            if (envVarName != null)
                Environment.SetEnvironmentVariable(envVarName, azureUsername);
            return new LogAnalyticsQuerySettings { WorkspaceId = config["WorkspaceId"] };
        }

        private IConfigurationRoot CreateConfig()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<AppInsightsQueryTests>();
            return builder.Build();
        }

        private AppInsightsQueryService CreateRealServiceSkippable()
        {
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);
            var config = CreateConfig();

            var client = new ApplicationInsightsClient(ApplicationInsightsClient.ConfigureClient(
                new HttpClient(), new ApplicationInsightsQuerySettings { AppId = config["ApplicationInsightsAppId"], ApiKey = config["ApplicationInsightsApiKey"] }));
            return new AppInsightsQueryService(client);
        }
    }
}
