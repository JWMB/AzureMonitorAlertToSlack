using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;

namespace AzureMonitorAlertToSlack.Alerts
{
    public class AlertInfoFactory<T> : IAlertInfoFactory<T>
        where T : IAlertInfo, new()
    {
        private readonly IDemuxedAlertHandler<T> demuxedHandler;

        public AlertInfoFactory(IDemuxedAlertHandler<T> demuxedHandler)
        {
            this.demuxedHandler = demuxedHandler;
        }

        private T Create(IAlertInfo info)
        {
            return new T
            {
                Title = info.Title,
                Text = info.Text,
                TitleLink = info.TitleLink
            };
        }

        public Task<List<T>> Process(string requestBody)
        {
            var alert = AzureMonitorCommonAlertSchemaTypes.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new NotImplementedException($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            var demuxer = new AlertDemuxer(demuxedHandler);

            demuxer.Demux(alert);

            var items = demuxedHandler.Handled;
            if (!items.Any())
            {
                items.Add(Create(new AlertInfo
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
                }));
            }

            if (!items.Any())
                throw new Exception($"No items produced");

            return Task.FromResult(items);
        }
    }
}
