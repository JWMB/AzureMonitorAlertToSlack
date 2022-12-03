using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMonitorAlertToSlack.Alerts;

namespace AzureMonitorAlertToSlack.Slack
{
    public class SlackMessageSender<T, TPart> : IMessageSender<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        private ISlackClient sender;
        private ISlackMessageFactory<T, TPart> messageFactory;

        public SlackMessageSender(ISlackClient sender, ISlackMessageFactory<T, TPart> messageFactory)
        {
            this.sender = sender;
            this.messageFactory = messageFactory;
        }

        public async Task SendMessage(T parts)
        {
            var slackBody = messageFactory.CreateMessage(parts);
            await sender.Send(slackBody);
        }
    }

}
