namespace KIStudy.AlertContextModels.LogAlertsV2
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AlertContext
    {
        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("conditionType")]
        public string ConditionType { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }
    }

    public partial class Condition
    {
        [JsonProperty("windowSize")]
        public string WindowSize { get; set; }

        [JsonProperty("allOf")]
        public AllOf[] AllOf { get; set; }

        [JsonProperty("windowStartTime")]
        public DateTimeOffset WindowStartTime { get; set; }

        [JsonProperty("windowEndTime")]
        public DateTimeOffset WindowEndTime { get; set; }
    }

    public partial class AllOf
    {
        [JsonProperty("searchQuery")]
        public string SearchQuery { get; set; }

        [JsonProperty("metricMeasureColumn")]
        public object MetricMeasureColumn { get; set; }

        [JsonProperty("targetResourceTypes")]
        public string TargetResourceTypes { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("threshold")]
        public long Threshold { get; set; }

        [JsonProperty("timeAggregation")]
        public string TimeAggregation { get; set; }

        [JsonProperty("dimensions")]
        public Dimension[] Dimensions { get; set; }

        [JsonProperty("metricValue")]
        public long MetricValue { get; set; }

        [JsonProperty("failingPeriods")]
        public FailingPeriods FailingPeriods { get; set; }

        [JsonProperty("linkToSearchResultsUI")]
        public Uri LinkToSearchResultsUi { get; set; }

        [JsonProperty("linkToFilteredSearchResultsUI")]
        public Uri LinkToFilteredSearchResultsUi { get; set; }

        [JsonProperty("linkToSearchResultsAPI")]
        public Uri LinkToSearchResultsApi { get; set; }

        [JsonProperty("linkToFilteredSearchResultsAPI")]
        public Uri LinkToFilteredSearchResultsApi { get; set; }
    }

    public partial class Dimension
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class FailingPeriods
    {
        [JsonProperty("numberOfEvaluationPeriods")]
        public long NumberOfEvaluationPeriods { get; set; }

        [JsonProperty("minFailingPeriodsToAlert")]
        public long MinFailingPeriodsToAlert { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("customKey1")]
        public string CustomKey1 { get; set; }

        [JsonProperty("customKey2")]
        public string CustomKey2 { get; set; }
    }
}
