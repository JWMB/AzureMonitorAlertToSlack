using AzureMonitorAlertToSlack.Alerts;
using SlackNet.WebApi;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Slack
{
    public interface ISlackMessageFactory
    {
        Message CreateMessage(IEnumerable<IAlertInfo> items);
    }
}
