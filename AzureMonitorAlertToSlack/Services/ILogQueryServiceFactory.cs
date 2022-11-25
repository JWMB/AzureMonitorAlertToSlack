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
        public ILogQueryService? CreateLogQueryService(string targetResourceType)
        {
            if (targetResourceType.Contains("/workspaces")) //microsoft.operationalinsights/workspaces
            {
                // This is Workspace (e.g. AppTraces)
                var workspaceId = Environment.GetEnvironmentVariable("LogAnalyticsWorkspaceId");
                if (!string.IsNullOrWhiteSpace(workspaceId))
                    return new LogAnalyticsQueryService(workspaceId);
            }
            else if (targetResourceType.Contains("microsoft.insights")) //microsoft.insights/components
            {
                // This is application insights (e.g. traces)
            }

            return null;
        }
    }
}
