using AzureMonitorAlertToSlack.Services.LogQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureMonitorAlertToSlack.Services
{
    public interface ILogQueryServiceFactory
    {
        ILogQueryService? CreateLogQueryService(string targetResourceType);
    }

    public class LogQueryServiceFactory : ILogQueryServiceFactory
    {
        private readonly LogAnalyticsQueryServiceRaw? logAnalytics;
        private readonly AppInsightsQueryService? appInsights;

        public LogQueryServiceFactory(LogAnalyticsQueryServiceRaw? logAnalytics, AppInsightsQueryService? appInsights)
        {
            this.logAnalytics = logAnalytics;
            this.appInsights = appInsights;
        }

        public ILogQueryService? CreateLogQueryService(string targetResourceType)
        {
            // different APIs for querying depending on provider - microsoft.insights/components vs microsoft.operationalinsights/workspaces - e.g. traces vs AppTraces
            if (targetResourceType.Contains("/workspaces")) //microsoft.operationalinsights/workspaces
            {
                // This is Workspace (e.g. AppTraces)
                return logAnalytics;
            }
            else if (targetResourceType.Contains("microsoft.insights")) //microsoft.insights/components
            {
                // This is application insights (e.g. traces)
                return appInsights;
            }
            return null;
        }
    }
}
