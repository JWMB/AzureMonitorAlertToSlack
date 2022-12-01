using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Slack
{
    public interface ISlackClient
    {
        Task<string> Send(object body, string? slackWebhook = null);
    }
}