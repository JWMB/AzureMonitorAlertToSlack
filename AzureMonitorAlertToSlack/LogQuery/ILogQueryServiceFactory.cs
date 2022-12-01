using System.Threading;

namespace AzureMonitorAlertToSlack.LogQuery
{
    public interface ILogQueryServiceFactory
    {
        ILogQueryService? CreateLogQueryService(string targetResourceType);
        CancellationToken GetCancellationToken(); // TODO: not a great design to have this here...
    }
}