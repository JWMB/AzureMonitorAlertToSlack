using AzureMonitorAlertToSlack.Alerts;
using SlackNet.WebApi;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Slack
{
    public interface ISlackMessageFactory<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        List<Message> CreateMessages(T summary);
    }
}
