using AzureMonitorCommonAlertSchemaTypes;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IDemuxedAlertHandler<T> : IDemuxedAlert
        where T : IAlertInfo, new()
    {
        List<T> Handled { get; }
    }
}
