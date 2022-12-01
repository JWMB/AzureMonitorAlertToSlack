using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IAlertInfoFactory
    {
        Task<List<AlertInfo>> Process(string requestBody);
    }
}
