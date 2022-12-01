using AzureMonitorAlertToSlack.Services;
using AzureMonitorAlertToSlack.Services.Slack;

namespace AzureMonitorAlertToSlack
{
    public class AppSettings
    {
        public LogQuerySettings? LogQuery { get; set; }
        public SlackSettings? Slack { get; set; }
    }
}
