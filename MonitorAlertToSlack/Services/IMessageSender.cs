using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Services
{
    public interface IMessageSender
    {
        Task SendMessage(IEnumerable<AlertInfo> parts);
    }
}
