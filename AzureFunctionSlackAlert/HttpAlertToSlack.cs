using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MonitorAlertToSlack.Services;
using MonitorAlertToSlack.Services.Implementations;

namespace AzureFunctionSlackAlert
{
    public static class HttpAlertToSlack
    {
        [FunctionName("HttpAlertToSlack")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // TODO: for some reason, DI doesn't work when deployed - causes 500 on startup with no further information
            // Creating them explicitly instead:
            IDemuxedAlertHandler demuxedHandler = new DemuxedAlertInfoHandler(new AIQueryService());
            IAlertInfoFactory alertInfoFactory = new AlertInfoFactory(demuxedHandler);
            IMessageSender sender = new SlackMessageSender(new SlackSenderFallback(), new SlackMessageFactory());

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //log.LogInformation(requestBody);

            if (requestBody == null)
            {
                return new BadRequestObjectResult($"Body was null");
            }

            List<AlertInfo> items;
            Exception? parseException = null;
            try 
            {
                items = await alertInfoFactory.Process(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);

                // Don't throw immediately - let this error message be sent first
                parseException = ex;
                items = new List<AlertInfo>{ 
                    new AlertInfo{ Title = "Unknown alert", Text = ex.Message },
                    new AlertInfo{ Title = "Body", Text = requestBody }
                };
            }

            try
            {
                await sender.SendMessage(items);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult($"Failed to send message: {ex.Message} ({ex.GetType().Name})"); // TODO: some other response type
            }

            return parseException != null
                ? new BadRequestObjectResult($"Could not read body: {parseException.Message}")
                : new OkObjectResult("");
        }
    }
}
