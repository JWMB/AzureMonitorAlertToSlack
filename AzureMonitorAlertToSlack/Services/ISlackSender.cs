using System.Threading.Tasks;

namespace AzureMonitorAlertToSlack.Services
{
    public interface ISlackSender
    {
        Task<string> SendAlert(object body, string? slackWebhook = null);
    }
}