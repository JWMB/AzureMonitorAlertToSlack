using AzureMonitorAlertToSlack.Alerts;
using SlackNet.WebApi;

namespace AzureMonitorAlertToSlack.Slack
{
    public interface ISlackMessageFactory<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        Message CreateMessage(T summary);
    }
}
