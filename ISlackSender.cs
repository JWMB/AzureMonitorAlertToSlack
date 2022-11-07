using System.Threading.Tasks;

namespace AzureAlerts2Slack
{
    public interface ISlackSender
    {
        Task<string> SendAlert(object body, string? slackWebhook = null);
    }
}