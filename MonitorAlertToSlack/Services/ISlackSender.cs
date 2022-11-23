using System.Threading.Tasks;

namespace MonitorAlertToSlack.Services
{
    public interface ISlackSender
    {
        Task<string> SendAlert(object body, string? slackWebhook = null);
    }
}