using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SlackNet.Blocks;
using SlackNet;
using System.Linq;
using SlackNet.WebApi;
using System.Runtime.CompilerServices;

namespace AzureAlerts2Slack
{
    public static class HttpAlertToSlack
    {
        [FunctionName("HttpAlertToSlack")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            // TODO: for some reason, DI for ISlackSender doesn't work - causes 500 on startup with no further information
            // Creating it explicitly instead :()
            //ISlackSender sender,
            Microsoft.Extensions.Logging.ILogger log)
        {
            ISlackSender sender = new SlackSenders.SlackSenderFallback();
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            if (requestBody == null)
            {
                return new BadRequestObjectResult($"Body was null");
            }

            List<AlertInfo> items;
            Exception? parseException = null;
            try 
            {
                items = await AlertInfo.Process(requestBody, new AIQueryService());
            }
            catch (Exception ex)
            {
                parseException = ex;
                items = new List<AlertInfo>{ 
                    new AlertInfo{ Title = "Unknown alert", Text = ex.Message },
                    new AlertInfo{ Title = "Body", Text = requestBody }
                };
                log.LogError(ex.Message);
            }

            var slackBody = new Message {
                Attachments = items.Select(CreateSlackAttachment).ToList(),
                // TODO: should we use Blocks instead?
                // Blocks = new Block[] {
                //         new HeaderBlock { Text = "My header" }
                //     }.Concat(
                //         items.SelectMany(CreateSlackBlocks)
                //     ).ToList()
            };

            try
            {
                await sender.SendAlert(slackBody);
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

        private static Attachment CreateSlackAttachment(AlertInfo info)
        {
            return new Attachment
            {
                 //Title = info.Title,
                 //TitleLink = info.TitleLink,
                 //Text = info.Text,
                 Color = string.IsNullOrEmpty(info.Color) ? "#FF5500" : info.Color,
                 Fallback = info.Text,
                 Blocks = CreateSlackBlocks(info)
            };
        }

        private static List<Block> CreateSlackBlocks(AlertInfo info)
        {
            // https://api.slack.com/block-kit
            return new List<Block>{
                new SectionBlock { Text = new Markdown{ Text = $"{MakeLink($"*{info.Title}*", info.TitleLink)}\n{info.Text}" } }
            };

            string MakeLink(string text, string? url) => string.IsNullOrEmpty(url) ? text : $"<{url}|{text}>";
        }
    }
}
