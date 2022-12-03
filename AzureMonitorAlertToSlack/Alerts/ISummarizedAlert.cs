using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{
    public interface ISummarizedAlert<T> 
        where T : ISummarizedAlertPart, new()
    {
        string Title { get; set; }
        string? TitleLink { get; set; }

        List<T> Parts { get; }
        Dictionary<string, string>? CustomProperties { get; set; }
    }

    public interface ISummarizedAlertPart
    {
        string? Title { get; set; }
        string? TitleLink { get; set; }
        string Text { get; set; }
        string? Color { get; set; }
        string? Icon { get; set; }
    }
}
