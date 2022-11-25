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

        private AppInsightsQueryService CreateRealServiceSkippable()
        {
            Skip.IfNot(System.Diagnostics.Debugger.IsAttached);
            var builder = new ConfigurationBuilder().AddUserSecrets<AppInsightsQueryTests>();
            var config = builder.Build();
            return new AppInsightsQueryService(config["ApplicationInsightsAppId"], config["ApplicationInsightsApiKey"]);
        }

        [Fact]
        public void Y()
        {
            var content = "";

            var typed = AppInsightsResponse.Deserialize(content);
            var dt = TableHelpers.TableToDataTable(typed?.Tables.FirstOrDefault() ?? new Table());
        }
    }
}
