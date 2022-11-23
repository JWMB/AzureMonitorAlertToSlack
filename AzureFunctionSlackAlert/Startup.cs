using AzureFunctionSlackAlert.Services;
using AzureFunctionSlackAlert.Services.SlackSenders;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureFunctionSlackAlert.Startup))]

namespace AzureFunctionSlackAlert
{
    public class Startup : FunctionsStartup
    {
        // TODO: is this being run at all? Doesn't seem like it...
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ISlackSender>(sp => new SlackSenderFallback());
            builder.Services.AddLogging();
        }
    }
}
