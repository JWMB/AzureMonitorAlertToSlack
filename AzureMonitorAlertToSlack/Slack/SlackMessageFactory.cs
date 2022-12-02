using AzureMonitorAlertToSlack.Alerts;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace AzureMonitorAlertToSlack.Slack
{
    public class SlackMessageFactory : ISlackMessageFactory
    {
        public Message CreateMessage(IEnumerable<IAlertInfo> items)
        {
            return new Message
            {
                Attachments = items.Select(CreateSlackAttachment).ToList(),
            };
        }

        private static Attachment CreateSlackAttachment(IAlertInfo info)
        {
            return new Attachment
            {
                //Title = info.Title,
                //TitleLink = info.TitleLink,
                //Text = info.Text,
                Color = string.IsNullOrEmpty(info.Color) ? "#FF5500" : info.Color,
                Fallback = ConvertToString.Truncate(info.Text, 100),
                Blocks = CreateSlackBlocks(info)
            };
        }

        private static List<Block> CreateSlackBlocks(IAlertInfo info)
        {
            // https://api.slack.com/block-kit
            var blocks = new List<Block>
            {
                // Note: seems like JSON in Markdown causes BadRequest/invalid_attachment?
                new SectionBlock { Text = new Markdown{ Text = $"{MakeLink($"*{info.Title}*", info.TitleLink)}\n{info.Text}" } }
            };

            foreach (var item in blocks)
            {
                // TODO: no common interface for e.g. those with Text..?
                if (item is SectionBlock s)
                {
                    // TODO: add a second block instead of truncating?
                    var maxLength = 3000;
                    if (s.Text.Text.Length > maxLength)
                        s.Text.Text = s.Text.Text.Remove(maxLength);
                }
            }
            return blocks;

            string MakeLink(string text, string? url) => string.IsNullOrEmpty(url) ? text : $"<{url}|{text}>";
        }
    }
}
