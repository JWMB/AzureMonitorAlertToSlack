using AzureMonitorCommonAlertSchemaTypes;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Services
{
    public interface IDemuxedAlertHandler : IDemuxedAlert
    {
        List<AlertInfo> Handled { get; }
    }
}
