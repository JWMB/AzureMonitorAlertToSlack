using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

public class AIQuery
{
    public async Task<string> XI(string query, DateTimeOffset start, DateTimeOffset end)
    {
        // Note: set up Managed Identity so Azure Function can access Application Insights
        // In Function, Enable System assigned Identity
        // In AI, Access control (IAM), add role Reader, assign the Function's Managed Identity 
        var logClient = new LogsQueryClient(new DefaultAzureCredential());
        var result = await logClient.QueryWorkspaceAsync("workspace", query, new QueryTimeRange(start, end), new LogsQueryOptions());
        // result.Value.GetVisualization()

        return @"
```
```
";
    }

    private static List<List<string>> TableToStrings(LogsTable table)
    {
        return new[] { table.Columns.Select(o => o.Name).ToList() }
            .ToList()
            .Concat(table.Rows.Select(row => row.Select(item => item.ToString() ?? "").ToList())).ToList();
    }
}