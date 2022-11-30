using Azure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Xunit;
using static AzureMonitorAlertToSlack.Services.LogQuery.AppInsightsQueryService;
using SlackNet.Interaction.Experimental;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using Shouldly;
using AzureMonitorAlertToSlack.Services.LogQuery;
using System.Data;

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

            var config = CreateConfig();
            var service = new LogAnalyticsQueryService(config["WorkspaceId"]);

            var q = "AppTraces";
            var result = await service.GetQueryAsDataTable(q, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
            result.Columns.OfType<DataColumn>().Count().ShouldNotBe(0);
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
            var client = new ApplicationInsightsClient(ApplicationInsightsClient.ConfigureClient(new HttpClient(), config["ApplicationInsightsAppId"], config["ApplicationInsightsApiKey"]));
            return new AppInsightsQueryService(client);
        }
    }
}
