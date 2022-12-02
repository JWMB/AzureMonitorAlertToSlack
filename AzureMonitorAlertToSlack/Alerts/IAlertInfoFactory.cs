using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IAlertInfoFactory<T> where T : IAlertInfo, new()
    {
        Task<List<T>> Process(string requestBody);
    }
}
