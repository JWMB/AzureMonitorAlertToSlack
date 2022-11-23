using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace MonitorAlertToSlack.Services.Implementations
{
    public class SlackMessageFactory : ISlackMessageFactory
    {
        public Message CreateMessage(IEnumerable<AlertInfo> items)
        {
            return new Message
            {
                Attachments = items.Select(CreateSlackAttachment).ToList(),
            };
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
