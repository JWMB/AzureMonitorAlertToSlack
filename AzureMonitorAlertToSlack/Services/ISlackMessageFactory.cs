using SlackNet.WebApi;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Services
{
    public interface ISlackMessageFactory
    {
        Message CreateMessage(IEnumerable<AlertInfo> items);
    }
}
