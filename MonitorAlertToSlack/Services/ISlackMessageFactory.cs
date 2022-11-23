using SlackNet.WebApi;
using System.Collections.Generic;

namespace AzureFunctionSlackAlert.Services
{
    public interface ISlackMessageFactory
    {
        Message CreateMessage(IEnumerable<AlertInfo> items);
    }
}
