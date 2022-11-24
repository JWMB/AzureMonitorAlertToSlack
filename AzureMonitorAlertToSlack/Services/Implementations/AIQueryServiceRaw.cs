using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorAlertToSlack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    /*
var client = HttpClientFactory.Create();
var URL = "https://api.applicationinsights.io/v1/apps/{0}/query";
client.DefaultRequestHeaders.Add("x-api-key", config.GetSetting("APPINSIGHTS_API_KEY"));
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
var requestUrl = string.Format(URL, config.GetSetting("APPINSIGHTS_APP_KEY"));
     */
    public async Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end)
    {
        // Note: set up Managed Identity so Azure Function can access logs:
        // In Function, Enable System assigned Identity
        // Workspace IAM: add Log Analytics Reader to the new Managed Identity
        // ...Also added reader, then even Owner - still no success
        // https://stackoverflow.com/a/64237716 says Log Analytics Reader is enough

        // Access mode / Access control mode...? https://learn.microsoft.com/en-us/azure/azure-monitor/logs/manage-access?tabs=portal

        // IMPORTANT
        // https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp
        // The back-end services for managed identities maintain a cache per resource URI for around 24 hours.
        // If you update the access policy of a particular target resource and immediately retrieve a token for that resource,
        // you may continue to get a cached token with outdated permissions until that token expires.
        // There's currently no way to force a token refresh.

        if (token == null || token.Value.ExpiresOn < DateTimeOffset.UtcNow)
        {
            try
            {
                var resourceId = "https://management.azure.com";
                var tokenRequestContext = new TokenRequestContext(new[] { $"{resourceId}/.default" });
                token = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext);
                //token = await new ManagedIdentityCredential(workspaceId).GetTokenAsync(tokenRequestContext);

                // When using resourceId = <Resource ID url path from Properties page): seems GetTokenAsync never terminates
                //    explore resource ids here: https://resources.azure.com/
                // When using resourceId = "https://management.azure.com{Resource ID url path}": Azure.Identity.CredentialDiagnosticScope.FailWrapAndThrow 
                // When using resourceId = "https://management.azure.com": we do get token, but for actual request: Response status code does not indicate success: 403 (Forbidden).
                //    (best outcome so far...)

                // Does "api://<commonly-api-client-id-uuid>/.default" work?
                // https://stackoverflow.com/questions/72992376/azure-function-get-token-from-defaultcredentials-managed-identity
            }
            catch (AuthenticationFailedException afEx)
            {
                throw new Exception($"Token auth error {afEx.StackTrace}", afEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Token retrieval problem", ex);
            }
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

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.Token);

        // https://api.loganalytics.io/v1/workspaces/c4ee0cba-337c-4e67-add9-3dd60c0cc81e/query?timespan=2022-11-24T13:00:53.000Z/2022-11-24T13:30:56.644Z

        var timespan = $"{UrlParamFormattedDateTime(start)}/{UrlParamFormattedDateTime(end)}"; //"P1D";
        var url = $"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query?timespan={timespan}";
        var serialized = JsonConvert.SerializeObject(body, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        HttpResponseMessage result;
        try
        {
            var content = new StringContent(serialized);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = serialized.Length;
            result = await client.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            throw new Exception($"{url} {ex.GetType().Name} {ex.Message}\n{serialized}", ex);
        }

        result.EnsureSuccessStatusCode();

        var reponseContent = await result.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(reponseContent))
            throw new Exception("Result content was null");

        LogAnalyticsResponse? typed;
        try
        {
            typed = JsonConvert.DeserializeObject<LogAnalyticsResponse>(reponseContent);
        }
        catch
        {
            typed = JsonConvert.DeserializeObject<LogAnalyticsResponse>(reponseContent, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
        return TableHelpers.TableToDataTable(typed?.Tables.FirstOrDefault() ?? new Table());

        string UrlParamFormattedDateTime(DateTimeOffset date) => date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    private void ConfigureClient()
    {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); //text/plain, */*
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