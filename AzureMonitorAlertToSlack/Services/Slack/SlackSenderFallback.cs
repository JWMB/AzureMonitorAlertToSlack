using System.Net.Http;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Services.Slack
{
    public class SlackSenderFallback : SlackSenderBase
    {
        private static HttpClient? client;
        public override async Task<string> SendAlert(object body, string? slackWebhook = null)
        {
            if (client == null)
                client = new HttpClient();

            return await Send(client, body, slackWebhook);
        }
    }
}