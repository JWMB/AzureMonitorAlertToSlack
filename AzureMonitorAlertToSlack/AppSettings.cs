using AzureMonitorAlertToSlack.LogQuery;
using AzureMonitorAlertToSlack.Slack;

namespace AzureMonitorAlertToSlack
{
    public class AppSettings
    {
        public LogQuerySettings? LogQuery { get; set; }
        public SlackSettings? Slack { get; set; }
    }
}
