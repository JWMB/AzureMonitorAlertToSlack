using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using AzureMonitorCommonAlertSchemaTypes;

namespace AzureMonitorAlertToSlack.Services.Implementations
{
    public class AlertInfoFactory : IAlertInfoFactory
    {
        private readonly IDemuxedAlertHandler demuxedHandler;

        public AlertInfoFactory(IDemuxedAlertHandler demuxedHandler)
        {
            this.demuxedHandler = demuxedHandler;
        }

        public Task<List<AlertInfo>> Process(string requestBody)
        {
            var alert = AzureMonitorCommonAlertSchemaTypes.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new Exception($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            var demuxer = new AlertDemuxer(demuxedHandler);

            try
            {
                demuxer.Demux(alert);
            }
            catch (Exception ex)
            {
                // TODO: log error
            }

            var items = demuxedHandler.Handled;
            if (!items.Any())
            {
                items.Add(new AlertInfo
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
                });
            }

            if (!items.Any())
                throw new Exception($"No items produced");

            if (Environment.GetEnvironmentVariable("DebugPayload") == "1") // TODO: change when DI problem solved
            {
                items.Last().Text += $"\\n{requestBody}";
            }

            return Task.FromResult(items);
        }
    }
}
