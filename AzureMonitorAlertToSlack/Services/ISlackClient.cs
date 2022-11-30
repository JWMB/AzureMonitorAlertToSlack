using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Services
{
    public interface ISlackClient
    {
        Task<string> Send(object body, string? slackWebhook = null);
    }
}