namespace KIStudy.AlertContextModels.MonitorAI
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AlertContext
    {
        [JsonProperty("SearchQuery")]
        public string SearchQuery { get; set; }

        [JsonProperty("SearchIntervalStartTimeUtc")]
        public string SearchIntervalStartTimeUtc { get; set; }

        [JsonProperty("SearchIntervalEndtimeUtc")]
        public string SearchIntervalEndtimeUtc { get; set; }

        [JsonProperty("ResultCount")]
        public long ResultCount { get; set; }

        [JsonProperty("LinkToSearchResults")]
        public Uri LinkToSearchResults { get; set; }

        [JsonProperty("LinkToFilteredSearchResultsUI")]
        public Uri LinkToFilteredSearchResultsUi { get; set; }

        [JsonProperty("LinkToSearchResultsAPI")]
        public Uri LinkToSearchResultsApi { get; set; }

        [JsonProperty("LinkToFilteredSearchResultsAPI")]
        public Uri LinkToFilteredSearchResultsApi { get; set; }

        [JsonProperty("SeverityDescription")]
        public string SeverityDescription { get; set; }

        [JsonProperty("WorkspaceId")]
        public string WorkspaceId { get; set; }

        [JsonProperty("SearchIntervalDurationMin")]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long SearchIntervalDurationMin { get; set; }

        [JsonProperty("AffectedConfigurationItems")]
        public string[] AffectedConfigurationItems { get; set; }

        [JsonProperty("SearchIntervalInMinutes")]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long SearchIntervalInMinutes { get; set; }

        [JsonProperty("Threshold")]
        public long Threshold { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Dimensions")]
        public Dimension[] Dimensions { get; set; }

        [JsonProperty("SearchResults")]
        public SearchResults SearchResults { get; set; }

        [JsonProperty("IncludedSearchResults")]
        public string IncludedSearchResults { get; set; }

        [JsonProperty("AlertType")]
        public string AlertType { get; set; }
    }

    public partial class Dimension
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class SearchResults
    {
        [JsonProperty("tables")]
        public Table[] Tables { get; set; }

        [JsonProperty("dataSources")]
        public DataSource[] DataSources { get; set; }
    }

    public partial class DataSource
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("tables")]
        public string[] Tables { get; set; }
    }

    public partial class Table
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("columns")]
        public Column[] Columns { get; set; }

        [JsonProperty("rows")]
        public string[][] Rows { get; set; }
    }

    public partial class Column
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
