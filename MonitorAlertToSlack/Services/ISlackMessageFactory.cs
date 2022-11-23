using SlackNet.WebApi;
using System.Collections.Generic;

namespace MonitorAlertToSlack.Services
{
    public interface ISlackMessageFactory
    {
        Message CreateMessage(IEnumerable<AlertInfo> items);
    }
}
