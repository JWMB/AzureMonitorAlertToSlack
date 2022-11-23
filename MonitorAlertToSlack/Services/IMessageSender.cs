using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureFunctionSlackAlert.Services
{
    public interface IMessageSender
    {
        Task SendMessage(IEnumerable<AlertInfo> parts);
    }
}
