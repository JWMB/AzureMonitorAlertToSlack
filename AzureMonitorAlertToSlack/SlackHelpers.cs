namespace AzureMonitorAlertToSlack
{
    public class SlackHelpers
    {
        public static string Escape(string str)
        {
            return str
                .Replace("&", "&amp;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");
        }
    }
}
