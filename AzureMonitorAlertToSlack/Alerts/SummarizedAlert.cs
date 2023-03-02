using System;
using System.Collections.Generic;

namespace AzureMonitorAlertToSlack.Alerts
{
    public class SummarizedAlert : ISummarizedAlert<SummarizedAlertPart>
    {
        public string Title { get; set; } = "";
        public string? TitleLink { get; set; }
        public List<SummarizedAlertPart> Parts { get; set; } = new List<SummarizedAlertPart>();
        public Dictionary<string, string>? CustomProperties { get; set; }
        public List<Uri> ImageUrls { get; set; } = new List<Uri>();

        //public SummarizedAlertPart AddPartWithFallback(SummarizedAlertPart part)
        //{
        //    if (part.Title == string.Empty) part.Title = Title;
        //    if (part.TitleLink == string.Empty) part.Title = Title;
        //}
    }

    public class SummarizedAlertPart : ISummarizedAlertPart
    {
        public string? Title { get; set; }
        public string? TitleLink { get; set; }
        public string Text { get; set; } = "";
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
}
