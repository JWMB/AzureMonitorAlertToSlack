using AzureMonitorCommonAlertSchemaTypes;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IDemuxedAlertHandler : IDemuxedAlert
    {
        List<IAlertInfo> Handled { get; }
    }
}
