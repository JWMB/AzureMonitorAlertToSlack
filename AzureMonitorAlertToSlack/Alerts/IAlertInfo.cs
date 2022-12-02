namespace AzureMonitorAlertToSlack.Alerts
{
    public interface IAlertInfo
    {
        string Title { get; set; }
        string? TitleLink { get; set; }
        string Text { get; set; }
        string? Color { get; set; }
        string? Icon { get; set; }
    }
}
