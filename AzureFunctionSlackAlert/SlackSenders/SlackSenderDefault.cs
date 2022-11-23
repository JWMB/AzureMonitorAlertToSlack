using System.Net.Http;
using System.Threading.Tasks;

namespace AzureAlerts2Slack.SlackSenders
{

    public class SlackSenderDefault : SlackSenderBase
    {
        private readonly IHttpClientFactory factory;

        public SlackSenderDefault(IHttpClientFactory factory)
        {
            this.factory = factory;
        }

        public override async Task<string> SendAlert(object body, string? slackWebhook = null) =>
             await base.Send(factory.CreateClient(), body, slackWebhook);
    }
}