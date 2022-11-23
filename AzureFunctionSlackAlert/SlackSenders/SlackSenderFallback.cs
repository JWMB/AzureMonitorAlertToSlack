using System.Net.Http;
using System.Threading.Tasks;

namespace AzureAlerts2Slack.SlackSenders
{
    public class SlackSenderFallback : SlackSenderBase
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
}