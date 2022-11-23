using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorAlertToSlack.Services
{
    public interface IMessageSender
    {
        Task SendMessage(IEnumerable<AlertInfo> parts);
    }
}
