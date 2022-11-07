using System;
using Newtonsoft.Json.Linq;

namespace KIStudy
{
    public class Alert
    {
        public string SchemaId { get; set; } = string.Empty;
        public Data Data { get; set; } = new();
    }

    public class Data
    {
        public Essentials Essentials { get; set; } = new();
        public JObject? AlertContext { get; set; }
    }

    public class Essentials
    {
        public string AlertId { get; set; } = string.Empty;
        public string AlertRule { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string SignalType { get; set; } = string.Empty;
        public string MonitorCondition { get; set; } = string.Empty;
        public string MonitoringService { get; set; } = string.Empty;
        public string[] AlertTargetIDs { get; set; } = new string[0];
        public string OriginAlertId { get; set; } = string.Empty;
        public DateTimeOffset? FiredDateTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string EssentialsVersion { get; set; } = string.Empty;
        public string AlertContextVersion { get; set; } = string.Empty;
        public string? FormattedFiredDateTime => FiredDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
    }
}
