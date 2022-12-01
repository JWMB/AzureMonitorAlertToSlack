using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMonitorAlertToSlack.Alerts;

namespace AzureMonitorAlertToSlack.Slack
{
    public class SlackMessageSender : IMessageSender
    {
        private ISlackClient sender;
        private ISlackMessageFactory messageFactory;

        public SlackMessageSender(ISlackClient sender, ISlackMessageFactory messageFactory)
        {
            this.sender = sender;
            this.messageFactory = messageFactory;
        }

        public async Task SendMessage(IEnumerable<AlertInfo> parts)
        {
            var slackBody = messageFactory.CreateMessage(parts);
            await sender.Send(slackBody);
        }
    }

}
