using AzureMonitorAlertToSlack.Alerts;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace AzureMonitorAlertToSlack.Slack
{
    public class SlackMessageFactory<T, TPart>: ISlackMessageFactory<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        public Message CreateMessage(T summary)
        {
            return new Message
            {
                Attachments = summary.Parts.Select(o => CreateSlackAttachment(o, summary)).ToList(),
            };
        }

        private static Attachment CreateSlackAttachment(TPart part, T summary)
        {
            return new Attachment
            {
                //Title = info.Title,
                //TitleLink = info.TitleLink,
                //Text = info.Text,
                Color = string.IsNullOrEmpty(part.Color) ? "#FF5500" : part.Color,
                Fallback = ConvertToString.Truncate(part.Text, 100),
                // Goddamnit, Slack! Width is fixed when using blocks... https://stackoverflow.com/questions/57438097/how-to-make-slack-api-block-kit-take-up-the-entire-width-of-the-slack-window
                Blocks = CreateSlackBlocks(part, summary)
            };
        }

        private static List<Block> CreateSlackBlocks(TPart part, T summary)
        {
            // https://api.slack.com/block-kit
            var blocks = new List<Block>
            {
                // Note: seems like JSON in Markdown causes BadRequest/invalid_attachment?
                new SectionBlock
                {
                    Text = new Markdown
                    { 
                        Text = $"{MakeLink($"*{FallbackIfEmpty(part.Title, summary.Title)}*", FallbackIfEmpty(part.TitleLink, summary.TitleLink))}\n{part.Text}"
                    }
                }
            };
            if (summary.ImageUrls.Any())
            {
                blocks.AddRange(summary.ImageUrls.Select(o => new ImageBlock { ImageUrl = o.AbsoluteUri, AltText = "n/a" }));
            }

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

            string FallbackIfEmpty(string? value, string? fallback) => (string.IsNullOrEmpty(value) ? fallback : value!) ?? string.Empty;
            string MakeLink(string text, string? url) => string.IsNullOrEmpty(url) ? text : $"<{url}|{text}>";
        }
    }
}
