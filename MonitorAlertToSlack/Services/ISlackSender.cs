using System.Threading.Tasks;

namespace AzureFunctionSlackAlert.Services
{
    public interface ISlackSender
    {
        Task<string> SendAlert(object body, string? slackWebhook = null);
    }
}