﻿using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.LogQuery
{
    public class AppInsightsQueryService : IAppInsightsQueryService
    {
        private readonly ApplicationInsightsClient client;

        public AppInsightsQueryService(ApplicationInsightsClient client)
        {
            this.client = client;
        }

        public async Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end, CancellationToken? cancellationToken = null)
        {
            // https://learn.microsoft.com/en-us/rest/api/application-insights/query/execute?tabs=HTTP

            var result = await client.Send(query, start, end, cancellationToken);
            return TableHelpers.TableToDataTable(result.Tables.FirstOrDefault() ?? new Table());
        }

        public class AppInsightsResponse
        {
            public List<Table> Tables { get; set; } = new List<Table>();

            public ErrorInfo? Error { get; set; }

            public class ErrorInfo
            {
                public string Message { get; set; } = string.Empty;
                public string Code { get; set; } = string.Empty;
                public string CorrelationId { get; set; } = string.Empty;
                public int? Line { get; set; }
                public int? Pos { get; set; }
                public string? Token { get; set; }

                public ErrorInfo? InnerError { get; set; }
            }

            public static AppInsightsResponse Deserialize(string json)
            {
                var result = JsonConvert.DeserializeObject<AppInsightsResponse>(json, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                if (result == null) throw new Exception($"Failed to deserialize {nameof(AppInsightsResponse)}");
                if (result.Error != null)
                {
                    throw new Exception(JsonConvert.SerializeObject(result.Error));
                }
                return result;
            }
        }

        public class ApplicationInsightsClient
        {
            private readonly HttpClient client;

            public ApplicationInsightsClient(HttpClient client)
            {
                this.client = client;
            }

            public async Task<AppInsightsResponse> Send(string query, DateTimeOffset start, DateTimeOffset end, CancellationToken? cancellationToken = null)
            {
                var timespan = "PT1H"; // TODO: can't find any specification
                var body = new
                {
                    timespan = timespan,
                    query = query.Replace("\n", "").Replace("\r", ""),
                    //applications
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("", content, cancellationToken: cancellationToken ?? default);

                return AppInsightsResponse.Deserialize(await response.Content.ReadAsStringAsync());
            }

            public static HttpClient ConfigureClient(HttpClient client, ApplicationInsightsQuerySettings settings)
            {
                client.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = $"https://api.applicationinsights.io/v1/apps/{settings.AppId}/query";
                client.BaseAddress = new Uri(url);
                return client;
            }
        }
    }

    public class ApplicationInsightsQuerySettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
    }
}
