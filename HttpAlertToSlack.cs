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

namespace KIStudy
{
    public static class HttpAlertToSlack
    {
        [FunctionName("HttpAlertToSlack")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            //ISlackSender sender,
            Microsoft.Extensions.Logging.ILogger log)
        {
            // TODO: for some reason, DI for ISlackSender doesn't work - causes 500 on startup with no further information
            ISlackSender sender = new SlackSenderPlain();
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            if (requestBody == null)
            {
                return new BadRequestObjectResult($"Body was null");
            }

            List<AlertInfo> items;
            try 
            {
                items = AlertPayloadParser.Parse(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult($"Could not read body: {ex.Message}");
            }

            var slackBody = new Message {
                Attachments = items.Select(CreateSlackAttachment).ToList(),
            };
            // var slackBody = new Message {
            //     Blocks = new Block[] {
            //         new HeaderBlock { Text = "My header" }
            //     }.Concat(
            //         items.SelectMany(CreateSlackBlocks)
            //     )
            //     .ToList()
            // };

            try
            {
                await sender.SendAlert(slackBody);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult($"Failed to send message: {ex.Message} ({ex.GetType().Name})"); // TODO: some other response type
            }

            return new OkObjectResult("");
        }

        private static object CreateSlackAttachmentObject(AlertInfo info)
        {
            return new 
            {
                 title = info.Title,
                 title_link = info.TitleLink,
                 text = info.Text,
                 color = string.IsNullOrEmpty(info.Color) ? "#FF5500" : info.Color,
                 fallback = info.Text
            };
        }

        private static Attachment CreateSlackAttachment(AlertInfo info)
        {
            return new Attachment
            {
                 Title = info.Title,
                 TitleLink = info.TitleLink,
                 Text = info.Text,
                 Color = string.IsNullOrEmpty(info.Color) ? "#FF5500" : info.Color,
                 Fallback = info.Text
            };
        }

        private static List<Block> CreateSlackBlocks(AlertInfo info)
        {
            var blocks = new List<Block>();
            blocks.Add(new SectionBlock { Text = new Markdown{ Text = $"<{info.TitleLink}|*{info.Title}*>\n{info.Text}" } });
            // https://api.slack.com/block-kit
            return blocks;
        }
    }
}
