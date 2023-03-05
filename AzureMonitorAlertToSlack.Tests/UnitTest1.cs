using AzureMonitorAlertToSlack.LogQuery;
using Moq;
using Shouldly;
using System.Data;
using AzureMonitorAlertToSlack.Alerts;

namespace AzureMonitorAlertToSlack.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task LogSearchAlerts()
        {
            var requestBody = File.ReadAllText(@"Payloads\Log alert V1 - Metric.json");
            var summary = await new SummarizedAlertFactory<SummarizedAlert, SummarizedAlertPart>(() => new DemuxedAlertHandler<SummarizedAlert, SummarizedAlertPart>(null)).Process(requestBody);

            var expected = @"
2 > 0
```
|TimeGenerated      |AggregatedValue|
|-------------------|---------------|
|2022-11-23 16:31:12|11             |
|2022-11-23 16:31:12|11             |
```
";
            summary.Parts.Single().Text.ShouldBe(TrimTable(expected));
            summary.TitleLink.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task LogQueryCriteria()
        {
            var requestBody = File.ReadAllText(@"Payloads\Log alert V2.json");

            var mockedLogQuery = new Mock<ILogQueryService>();
            var dt = CreateDataTable(new[] { new { Title = "A", Value = 1 }, new { Title = "B", Value = 2 } }.Cast<object>().ToList());
            mockedLogQuery.Setup(o => o.GetQueryAsDataTable(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => dt);

            var mockedFactory = new Mock<ILogQueryServiceFactory>();
            mockedFactory.Setup(o => o.CreateLogQueryService(It.IsAny<string>())).Returns(mockedLogQuery.Object);

            var demuxedHandler = new DemuxedAlertHandler<SummarizedAlert, SummarizedAlertPart>(mockedFactory.Object);
            var summary = await new SummarizedAlertFactory<SummarizedAlert, SummarizedAlertPart>(() => demuxedHandler).Process(requestBody);

            var expected = @"
Heartbeat/MMC: 3 > 0 (16:21:24 UTC:+00:00)
```
|Title|Value|
|-----|-----|
|A    |1    |
|B    |2    |
```
";
            summary.Parts.Single().Text.ShouldBe(TrimTable(expected));
            summary.Parts.Single().TitleLink.ShouldNotBeEmpty();
        }

        private static string TrimTable(string str) => str.Trim().Replace("\r", "");

        [Fact]
        public void TableToMarkdown()
        {
            var localNow = DateTime.Now;
            var table = CreateTable(localNow);

            var stringifyer = new CustomConverter();
            var rendered = TableHelpers.TableToMarkdown(table, (o, col) => stringifyer.Convert(o, col.DataType));

            var expected = $@"
|Id   |Name           |Timespan   |DateTime |dynamic        |
|-----|---------------|-----------|---------|---------------|
|1    |abc            |00:00:10   |22:00 UTC|{{ A = 1, B = 2 |
|19999|1              |00:00:01   |00:00 UTC|{{ X = 1 }}      |
|2    |123456789012345|10.00:00:00|{stringifyer.Convert(localNow)}|{{ }}            |
";
            rendered.ShouldBe(TrimTable(expected));
        }

        private DataTable CreateDataTable(List<object> items)
        {
            var dt = new DataTable();
            var type = items.First().GetType();
            //var type = typeof(T);
            var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
                dt.Columns.Add(prop.Name, prop.PropertyType);

            foreach (var item in items)
            {
                var cells = props.Select(p => p.GetValue(item));
                var dr = dt.NewRow();
                dr.ItemArray = cells.ToArray();
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private DataTable CreateTable(DateTime dateTime)
        {
            var table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("Id", typeof(int)),
                new DataColumn("Name", typeof(string)),
                new DataColumn("Timespan", typeof(TimeSpan)),
                new DataColumn("DateTime", typeof(DateTime)),
                new DataColumn("dynamic", typeof(object)),
            });

            var rows = new[]
            {
                new object[]{ 1, "abc", TimeSpan.FromSeconds(10), DateTime.Parse("1990-01-10T22:00:00Z"), new { A = 1, B = 2 } },
                new object[]{ 19999, "1", TimeSpan.FromSeconds(1), DateTime.MinValue, new { X = 1 } },
                new object[]{ 2, "12345678901234567890", TimeSpan.FromDays(10), dateTime, new { } },
            };
            foreach (var row in rows)
            {
                var dr = table.NewRow();
                dr.ItemArray = row;
                table.Rows.Add(dr);
            }
            return table;
        }
    }

    public class CustomConverter : ConvertToString
    {
        public CustomConverter(): base(15) { }

        public override string Convert(DateTime obj) => obj.ToUniversalTime().ToString("HH:mm") + " UTC";
    }
}