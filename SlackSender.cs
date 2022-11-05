using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SlackNet.WebApi;

namespace KIStudy
{
    public interface ISlackSender
    {
        Task<string> SendAlert(object body, string? slackWebhook = null);
    }

    public abstract class SlackSenderBase : ISlackSender
    {
        private static string Serialize(Message message)
        {
            return JsonConvert.SerializeObject(message, SlackNet.Default.JsonSettings().SerializerSettings);
        }

        protected async Task<string> Send(HttpClient client, object body, string? slackWebhook = null)
        {
            slackWebhook = slackWebhook ?? Environment.GetEnvironmentVariable("SlackWebhookUrl");
            if (string.IsNullOrEmpty(slackWebhook))
                throw new ArgumentException($"No Slack webhook speficied");

            HttpResponseMessage response;
            if (body is Message msg)
                response = await client.PostAsync(slackWebhook, new StringContent(Serialize(msg)));
            else
                response = await client.PostAsJsonAsync(slackWebhook, body);

            // response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Send error: {response.StatusCode} {response.ReasonPhrase}\n{slackWebhook}\n{JsonConvert.SerializeObject(body)}\n{response.Content?.ReadAsStringAsync().Result}");
            }
            // return response;
            return response.Content.ReadAsStringAsync().Result;
        }

        public abstract Task<string> SendAlert(object body, string? slackWebhook = null);
    }

    public class SlackSenderPlain : SlackSenderBase
    {
        private static HttpClient? client;
        public override async Task<string> SendAlert(object body, string? slackWebhook = null)
        {
            if (client == null)
            {
                client = new HttpClient();
            }
            return await base.Send(client, body, slackWebhook);
        }
    }

    public class SlackSender : SlackSenderBase
    {
        private readonly IHttpClientFactory factory;

        public SlackSender(IHttpClientFactory factory)
        {
            this.factory = factory;
        }

        public override async Task<string> SendAlert(object body, string? slackWebhook = null) =>
             await base.Send(factory.CreateClient(), body, slackWebhook);
    }
}