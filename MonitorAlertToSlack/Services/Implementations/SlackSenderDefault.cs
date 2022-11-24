using System.Net.Http;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Services.Implementations
{
    public class SlackSenderDefault : SlackSenderBase
    {
        private readonly IHttpClientFactory factory;

        public SlackSenderDefault(IHttpClientFactory factory)
        {
            this.factory = factory;
        }

        public override async Task<string> SendAlert(object body, string? slackWebhook = null) =>
             await Send(factory.CreateClient(), body, slackWebhook);
    }
}