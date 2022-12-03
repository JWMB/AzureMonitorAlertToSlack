using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;

namespace AzureMonitorAlertToSlack.Alerts
{
    public class SummarizedAlertFactory<T, TPart> : ISummarizedAlertFactory<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        private readonly Func<IDemuxedAlertHandler<T, TPart>> createDemuxedHandler;

        public SummarizedAlertFactory(Func<IDemuxedAlertHandler<T, TPart>> createDemuxedHandler)
        {
            this.createDemuxedHandler = createDemuxedHandler;
        }

        public Task<T> Process(string requestBody)
        {
            var alert = AzureMonitorCommonAlertSchemaTypes.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new NotImplementedException($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            var demuxedHandler = createDemuxedHandler();
            var demuxer = new AlertDemuxer(demuxedHandler);

            demuxer.Demux(alert);

            var summary = demuxedHandler.Handled;
            if (!summary.Parts.Any())
            {
                var fallback = new TPart
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
                };
                summary.Parts.Add(fallback);
            }

            if (!summary.Parts.Any())
                throw new Exception($"No parts produced");

            return Task.FromResult(summary);
        }
    }
}
