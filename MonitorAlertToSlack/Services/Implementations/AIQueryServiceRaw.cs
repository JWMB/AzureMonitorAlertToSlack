using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using Newtonsoft.Json;

public class AIQueryServiceRaw : IAIQueryService
{
    private readonly string workspaceId;
    private static HttpClient client; // TODO: until we get DI to work...
    private static AccessToken? token; // TODO: real caching

    public AIQueryServiceRaw(string workspaceId)
    {
        this.workspaceId = workspaceId;

        if (client == null)
        {
            client = new HttpClient();
            ConfigureClient();
        }
    }

    public async Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end)
    {
        // Note: set up Managed Identity so Azure Function can access Application Insights
        // In Function, Enable System assigned Identity
        // In AI, Access control (IAM), add role Reader, assign the Function's Managed Identity
        // (Also added reader to the Workspace IAM, not sure which one is needed)

        if (token == null || token.Value.ExpiresOn < DateTimeOffset.UtcNow)
        {
            var tokenRequestContext = new TokenRequestContext(); //new[] { scope }
            token = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext);
        }

        var body = new
        {
            Query = query,
            Options = new { TruncationMaxSize = 67108864 },
            MaxRows = 30001,
            WorkspaceFilters = new { Regions = Array.Empty<string>() }
        };
        //var options = new LogsQueryOptions
        //{
        //    IncludeVisualization = false, // result.Value.GetVisualization()
        //    AllowPartialErrors = true,
        //    IncludeStatistics = false,
        //    ServerTimeout = null
        //};

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value.Token);

        // https://api.loganalytics.io/v1/workspaces/c4ee0cba-337c-4e67-add9-3dd60c0cc81e/query?timespan=2022-11-24T13:00:53.000Z/2022-11-24T13:30:56.644Z

        var timespan = $"{UrlParamFormattedDateTime(start)}/{UrlParamFormattedDateTime(end)}"; //"P1D";
        var result = await client.PostAsync($"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query?timespan={timespan}", new StringContent(JsonConvert.SerializeObject(body)));

        result.EnsureSuccessStatusCode();

        var content = await result.Content.ReadAsStringAsync();

        var typed = JsonConvert.DeserializeObject<LogAnalyticsResponse>(content);
        return MonitorAlertToSlack.Services.Implementations.DemuxedAlertInfoHandler.TableToDataTable(typed?.Tables.FirstOrDefault() ?? new Table());

        string UrlParamFormattedDateTime(DateTimeOffset date) => date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    private void ConfigureClient()
    {
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); //text/plain, */*
        /*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: https://sandbox-32-3.reactblade.portal.azure.net/
Prefer: wait=600, ai.include-statistics=true, ai.include-render=true, include-datasources=true
Cache-Control: no-cache
x-ms-client-request-info: Query
Content-Type: application/json
x-ms-app: AppAnalytics
x-ms-user-id: 
Access-Control-Expose-Headers: x-ms-client-request-id
x-ms-client-request-id: 11c37fab-10cf-47bb-8959-5f3a1813bd63
Authorization: Bearer ...
Content-Length: 178
Origin: https://sandbox-32-3.reactblade.portal.azure.net
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site
Pragma: no-cache
TE: trailers
*/
    }

    public class LogAnalyticsResponse
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public dynamic? Render { get; set; }
        public dynamic? Statistics { get; set;}
        public List<dynamic>? DataSources { get; set; }
    }
}