using AzureMonitorAlertToSlack.Services.LogQuery;

namespace AzureMonitorAlertToSlack.Services
{
    public class LogQueryServiceFactory : ILogQueryServiceFactory
    {
        private readonly ILogAnalyticsQueryService? logAnalytics;
        private readonly IAppInsightsQueryService? appInsights;

        public LogQueryServiceFactory(LogQuerySettings settings, ILogAnalyticsQueryService? logAnalytics, IAppInsightsQueryService? appInsights)
        {
            if (settings.Enabled)
            {
                this.logAnalytics = logAnalytics;
                this.appInsights = appInsights;
            }
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

    public class LogQuerySettings
    {
        public int Timeout { get; set; } = 20;
        public bool Enabled { get; set; }

        public LogAnalyticsQuerySettings? LogAnalytics { get; set; }
        public ApplicationInsightsQuerySettings? ApplicationInsights { get; set; }
    }

}
