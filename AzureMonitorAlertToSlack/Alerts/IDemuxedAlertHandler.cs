using AzureMonitorCommonAlertSchemaTypes;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IDemuxedAlertHandler : IDemuxedAlert
    {
        List<AlertInfo> Handled { get; }
    }
}
