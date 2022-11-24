using AzureMonitorCommonAlertSchemaTypes;
using System.Collections.Generic;

namespace MonitorAlertToSlack.Services
{
    public interface IDemuxedAlertHandler : IDemuxedAlert
    {
        List<AlertInfo> Handled { get; }
    }
}
