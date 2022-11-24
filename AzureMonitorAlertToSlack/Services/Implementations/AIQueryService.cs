using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

public class AIQueryService : IAIQueryService
{
    private readonly string workspaceId;

    public AIQueryService(string workspaceId)
    {
        this.workspaceId = workspaceId;
    }
    public async Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end)
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
        var result = await logClient.QueryWorkspaceAsync(workspaceId, query, new QueryTimeRange(start, end), options);

        return ConvertToDataTable(result.Value.Table);
    }

    public static DataTable ConvertToDataTable(LogsTable table)
    {
        var dt = new DataTable(table.Name);

        dt.Columns.AddRange(table.Columns.Select(o => new DataColumn(o.Name, ConvertType(o.Type))).ToArray());

        foreach (var row in table.Rows)
        {
            var dtRow = dt.NewRow();
            dt.Rows.Add(row);

            dtRow.ItemArray = row.Select(o => o).ToArray();
        }

        return dt;
    }

    private static Type ConvertType(LogsColumnType type)
    {
        var columnTypes = new Dictionary<LogsColumnType, Type>
        {
            { LogsColumnType.Bool, typeof(bool) },
            { LogsColumnType.Datetime, typeof(DateTime) }, // TODO: DateTimeOffset?
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