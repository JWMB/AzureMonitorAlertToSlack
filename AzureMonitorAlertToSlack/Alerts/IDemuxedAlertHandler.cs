using AzureMonitorCommonAlertSchemaTypes;
namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IDemuxedAlertHandler<T, TPart> : IDemuxedAlert
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        T Result { get; }
    }
}
