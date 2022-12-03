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
        private readonly IDemuxedAlertHandler<T, TPart> demuxedHandler;

        public SummarizedAlertFactory(IDemuxedAlertHandler<T, TPart> demuxedHandler)
        {
            this.demuxedHandler = demuxedHandler;
        }

        private T Create(ISummarizedAlertPart info)
        {
            var result = new T();
            result.Parts.Add(
                new TPart
                {
                    Title = info.Title,
                    Text = info.Text,
                    TitleLink = info.TitleLink
                }
            );
            return result;
        }

        public Task<T> Process(string requestBody)
        {
            var alert = AzureMonitorCommonAlertSchemaTypes.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new NotImplementedException($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            var demuxer = new AlertDemuxer(demuxedHandler);

            demuxer.Demux(alert);

            var items = demuxedHandler.Handled;
            if (!items.Parts.Any())
            {
                var fallback = new TPart
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
                };
                items.Parts.Add(fallback);
            }

            if (!items.Parts.Any())
                throw new Exception($"No items produced");

            return Task.FromResult(items);
        }
    }
}
