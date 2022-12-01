using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SlackNet.WebApi;
using static AzureMonitorAlertToSlack.AppSettings;

namespace AzureMonitorAlertToSlack.Services.Slack
{
    public class SlackClient : ISlackClient
    {
        private readonly HttpClient client;
        private readonly string? defaultWebhook;

        public SlackClient(HttpClient client, SlackSettings settings)
        {
            this.client = client;
            defaultWebhook = settings.DefaultWebhook;
        }

        private static string Serialize(Message message) => JsonConvert.SerializeObject(message, SlackNet.Default.JsonSettings().SerializerSettings);

        public async Task<string> Send(object body, string? slackWebhook = null)
        {
            slackWebhook = slackWebhook ?? defaultWebhook;
            if (string.IsNullOrEmpty(slackWebhook))
                throw new ArgumentException($"No Slack webhook specified");

            var response = await client.PostAsync(slackWebhook, new StringContent(body is Message msg ? Serialize(msg) : JsonConvert.SerializeObject(body)));

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Send error: {response.StatusCode} {response.ReasonPhrase}\nResponse:{response.Content?.ReadAsStringAsync().Result}\n\n{slackWebhook}\n{JsonConvert.SerializeObject(body)}");

            return response.Content.ReadAsStringAsync().Result;
        }

        public static HttpClient Configure(HttpClient client)
        {
            return client;
        }
    }

    public class SlackSettings
    {
        public string? DefaultWebhook { get; set; }
    }

}