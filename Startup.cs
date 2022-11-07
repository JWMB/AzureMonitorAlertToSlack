using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureAlerts2Slack.Startup))]

namespace AzureAlerts2Slack
{
    public class Startup : FunctionsStartup
    {
        // TODO: is this being run at all? Doesn't seem like it...
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ISlackSender>(sp => new SlackSenders.SlackSenderFallback());
            builder.Services.AddLogging();
        }
    }
}
