using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{

    public class AlertInfo : IAlertInfo
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
}
