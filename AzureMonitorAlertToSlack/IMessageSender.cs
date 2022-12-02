using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMonitorAlertToSlack.Alerts;

namespace AzureMonitorAlertToSlack
{
    public interface IMessageSender
    {
        Task SendMessage(IEnumerable<IAlertInfo> parts);
    }
}
