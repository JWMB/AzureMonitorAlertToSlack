using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

namespace AzureMonitorAlertToSlack.LogQuery
{
    public class LogAnalyticsQueryService : ILogAnalyticsQueryService
    {
        private readonly string workspaceId;

        public LogAnalyticsQueryService(LogAnalyticsQuerySettings settings)
        {
            workspaceId = settings.WorkspaceId;
        }

        public async Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end, CancellationToken? cancellationToken = null)
        {
            // Note: set up Managed Identity so Azure Function can access Application Insights
            // In Function, Enable System assigned Identity
            // In AI, Access control (IAM), add role Reader, assign the Function's Managed Identity

            // TODO: is LogsQueryClient broken? Getting error:
            // Unable to cast object of type 'Azure.Monitor.Query.Models.LogsTableRow' to type 'System.IConvertible'.
            // Couldn't store <["2022-11-24T12:49:21.6480202Z","Here is a Error"]> in TimeGenerated Column.  Expected type is DateTime.

            // Wait - this is for querying the Workspace, and those queries look different! AppTraces vs traces, property capitalization etc
            var logClient = new LogsQueryClient(new DefaultAzureCredential());
            var options = new LogsQueryOptions
            {
                IncludeVisualization = false, // result.Value.GetVisualization()
                AllowPartialErrors = true,
                IncludeStatistics = false,
                ServerTimeout = null
            };
            var result = await logClient.QueryWorkspaceAsync(workspaceId, query, new QueryTimeRange(start, end), options, cancellationToken: cancellationToken ?? default);

            return ConvertToDataTable(result.Value.Table);
        }

        public static DataTable ConvertToDataTable(LogsTable table)
        {
            var dt = new DataTable(table.Name);

            try
            {
                dt.Columns.AddRange(table.Columns.Select(o => new DataColumn(o.Name, ConvertType(o.Type))).ToArray());
                var errors = new List<string>();

                foreach (var row in table.Rows)
                {
                    var dtRow = dt.NewRow();

                    var values = row.Select((o, i) => {
                        if (o == null)
                            return null;
                        try
                        {
                            return Convert.ChangeType(o, dt.Columns[i].DataType);
                        }
                        catch (Exception x)
                        {
                            errors.Add($"{o}/{dt.Columns[i].DataType}: {x.Message}");
                            return null;
                        }
                    });
                    dt.Rows.Add(values);
                    //dtRow.ItemArray = row.Select(o => o).ToArray();
                }
                if (errors.Any())
                    throw new Exception($"Convert errors: {string.Join(",", errors)}");
            }
            catch (Exception ex)
            {
                // ArgumentException Unable to cast object of type 'Azure.Monitor.Query.Models.LogsTableRow' to type 'System.DateTimeOffset'
                // Couldn't store <["2023-02-27T17:22:26.8360448Z","We currently allow max 50 trainings per account. You have 0 left.","TrainingApi.ErrorHandling.HttpException at TrainingApi.Controllers.TrainingsController+<PostGroup>d__14.MoveNext"]>
                // in TimeGenerated Column. Expected type is DateTimeOffset
                var cols = string.Join(",", table.Columns.Select(o => $"{o.Name}/{o.Type}"));
                var row1 = table.Rows.FirstOrDefault().Select(o => $"'{o}'/{o.GetType().Name}");
                throw new Exception($"Problem converting to datatable: {cols} {(row1 == null ? "NULL" : string.Join(",", row1))}", ex);
            }
            return dt;
        }

        private static Type ConvertType(LogsColumnType type)
        {
            var columnTypes = new Dictionary<LogsColumnType, Type>
        {
            { LogsColumnType.Bool, typeof(bool) },
            { LogsColumnType.Datetime, typeof(DateTimeOffset) },
            { LogsColumnType.Decimal, typeof(decimal) },
            { LogsColumnType.Dynamic, typeof(object) },
            { LogsColumnType.Guid, typeof(Guid) },
            { LogsColumnType.Int, typeof(int) },
            { LogsColumnType.Long, typeof(long) },
            { LogsColumnType.String, typeof(string) },
            //{ LogsColumnType.Real, typeof(Real) },
            { LogsColumnType.Timespan, typeof(TimeSpan) },
        };
            return columnTypes[type];
        }
    }

    public class LogAnalyticsQuerySettings
    {
        public string WorkspaceId { get; set; } = string.Empty;
    }
}