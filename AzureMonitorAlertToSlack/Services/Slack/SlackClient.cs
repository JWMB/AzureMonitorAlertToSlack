using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SlackNet.WebApi;

namespace AzureMonitorAlertToSlack.Services.Slack
{
    public class SlackClient : ISlackClient
    {
        private readonly HttpClient client;

        public SlackClient(HttpClient client)
        {
            this.client = client;
        }

        private static string Serialize(Message message) => JsonConvert.SerializeObject(message, SlackNet.Default.JsonSettings().SerializerSettings);

        public async Task<string> Send(object body, string? slackWebhook = null)
        {
            slackWebhook = slackWebhook ?? Environment.GetEnvironmentVariable("SlackWebhookUrl");
            if (string.IsNullOrEmpty(slackWebhook))
                throw new ArgumentException($"No Slack webhook speficied");

            var response = await client.PostAsync(slackWebhook, new StringContent(body is Message msg ? Serialize(msg) : JsonConvert.SerializeObject(body)));
            // response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Send error: {response.StatusCode} {response.ReasonPhrase}\nResponse:{response.Content?.ReadAsStringAsync().Result}\n\n{slackWebhook}\n{JsonConvert.SerializeObject(body)}");

            return response.Content.ReadAsStringAsync().Result;
        }

        public static HttpClient Configure(HttpClient client)
        {
            return client;
        }
    }
}