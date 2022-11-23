namespace AzureFunctionSlackAlert.Services
{
    public class AlertInfo
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }
    }
}
