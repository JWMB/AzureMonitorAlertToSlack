using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SlackNet.WebApi;

namespace AzureMonitorAlertToSlack.Slack
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

            var parts = body is System.Collections.IEnumerable enumerable
                ? enumerable
                : new[] { body };

            var responses = new List<string>();
            foreach (var part in parts)
            {
                var response = await client.PostAsync(slackWebhook, new StringContent(part is Message msg ? Serialize(msg) : JsonConvert.SerializeObject(part)));
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Send error: {response.StatusCode} {response.ReasonPhrase}\nResponse:{response.Content?.ReadAsStringAsync().Result}\n\n{slackWebhook}\n{JsonConvert.SerializeObject(body)}");
                responses.Add(await response.Content.ReadAsStringAsync());
            }

            return string.Join("\n", responses);
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