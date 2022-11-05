namespace KIStudy.AlertContextModels.ActivityLog
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AlertContext
    {
        [JsonProperty("authorization")]
        public Authorization Authorization { get; set; }

        [JsonProperty("channels")]
        public string Channels { get; set; }

        [JsonProperty("claims")]
        public string Claims { get; set; }

        [JsonProperty("caller")]
        public string Caller { get; set; }

        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }

        [JsonProperty("eventSource")]
        public string EventSource { get; set; }

        [JsonProperty("eventTimestamp")]
        public DateTimeOffset EventTimestamp { get; set; }

        [JsonProperty("eventDataId")]
        public Guid EventDataId { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("subStatus")]
        public string SubStatus { get; set; }

        [JsonProperty("submissionTimestamp")]
        public DateTimeOffset SubmissionTimestamp { get; set; }

        [JsonProperty("Activity Log Event Description")]
        public string ActivityLogEventDescription { get; set; }
    }

    public partial class Authorization
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("eventCategory")]
        public string EventCategory { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("hierarchy")]
        public string Hierarchy { get; set; }
    }
}
