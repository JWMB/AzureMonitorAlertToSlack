using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Types.AlertContexts.LogAlertsV2;
using Types.AlertContexts;
using Types;

namespace MonitorAlertToSlack.Services
{
    public class AlertInfoFactory : IAlertInfoFactory
    {
        private readonly IAIQueryService? aiQueryService;

        public AlertInfoFactory(IAIQueryService? aiQueryService)
        {
            this.aiQueryService = aiQueryService;
        }

        public async Task<List<AlertInfo>> Process(string requestBody)
        {
            var slackItems = new List<AlertInfo>();

            var alert = Types.Serialization.AlertJsonSerializerSettings.DeserializeOrThrow(requestBody);
            var ctx = alert?.Data.AlertContext;
            if (alert == null || ctx == null)
                throw new Exception($"Not supported: {alert?.Data?.Essentials?.MonitoringService}");

            try
            {
                if (ctx is LogAlertsV2AlertContext ctxV2)
                {
                    await foreach (var item in CreateFromLogAlertsV2(alert, ctxV2, aiQueryService))
                        slackItems.Add(item);
                }
                else if (ctx is LogAnalyticsAlertContext ctxLA)
                {
                    var dataTables = ctxLA.SearchResults.Tables.Select(o =>
                    {
                        var dt = new DataTable(o.Name);
                        foreach (var col in o.Columns)
                            dt.Columns.Add(new DataColumn(col.Name, typeof(string))); // TODO: parse col.Type to actual types

                        foreach (var row in o.Rows)
                        {
                            var dr = dt.NewRow();
                            dr.ItemArray = row;
                            dt.Rows.Add(dr);
                        }
                        return dt;
                    });
                    var renderedTable = dataTables.Any() ? RenderDataTable(dataTables.First()) : null;

                    slackItems.Add(new AlertInfo
                    {
                        Title = alert.Data.Essentials.AlertRule,
                        Text = $"{ctxLA.ResultCount} {ctxLA.OperatorToken} {ctxLA.Threshold}{(renderedTable == null ? "" : $"\n{renderedTable}")}",
                        TitleLink = ctxLA.LinkToFilteredSearchResultsUi?.ToString()
                    });
                }
                // In case we want specially tailored info rather than the generic ToUserFriendlyString:
                // else if (ctx is ActivityLogAlertContext ctxAL)
                // else if (ctx is SmartAlertContext ctxSA)
                // else if (ctx is ServiceHealthAlertContext ctxSA)
            }
            catch (Exception ex)
            {
                // TODO: log error
            }

            if (!slackItems.Any())
            {
                slackItems.Add(new AlertInfo
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = $"{ctx.ToUserFriendlyString()}",
                    TitleLink = ctx is LogAnalyticsAlertContext ctxLAx ? ctxLAx.LinkToFilteredSearchResultsUi?.ToString() : null
                });
            }

            if (!slackItems.Any())
                throw new Exception($"No items produced");

            if (Environment.GetEnvironmentVariable("DebugPayload") == "1") // TODO: change when DI problem solved
            {
                slackItems.Last().Text += $"\\n{requestBody}";
            }

            return slackItems;
        }

        private static async Task<string> QueryAI(IAIQueryService aiQueryService, string query, DateTimeOffset start, DateTimeOffset end)
        {
            return RenderDataTable(await aiQueryService.GetQueryAsDataTable(query, start, end));
        }

        private static string RenderDataTable(DataTable dt)
        {
            var stringifyer = new ConvertToString(40);
            return $"```\n{TableHelpers.TableToMarkdown(dt, (obj, type) => stringifyer.Convert(obj, type), 10)}\n```";
        }

        public static async IAsyncEnumerable<AlertInfo> CreateFromLogAlertsV2(Alert alert, LogAlertsV2AlertContext ctxV2, IAIQueryService? aiQueryService)
        {
            if (ctxV2.Condition.AllOf?.Any() == true)
            {
                foreach (var item in ctxV2.Condition.AllOf)
                {
                    yield return new AlertInfo
                    {
                        Title = alert.Data.Essentials.AlertRule,
                        Text = await GetText(item),
                        TitleLink = GetTitleLink(item)
                    };
                }
            }
            else
            {
                yield return new AlertInfo
                {
                    Title = alert.Data.Essentials.AlertRule,
                    Text = ctxV2.Condition.ToUserFriendlyString(),
                    TitleLink = GetTitleLink(ctxV2.Condition.AllOf?.FirstOrDefault())
                };
            }

            // ToAsyncEnumerable not available in .netstandard 2.0
            //var items = ctxV2.Condition.AllOf?.ToAsyncEnumerable().SelectAwait(async o => new AlertInfo
            //{
            //    Title = alert.Data.Essentials.AlertRule,
            //    Text = await GetText(o),
            //    TitleLink = GetTitleLink(o)
            //});

            //if (items == null)
            //    yield return new AlertInfo { Title = alert.Data.Essentials.AlertRule, Text = ctxV2.Condition.ToUserFriendlyString(), TitleLink = GetTitleLink(ctxV2.Condition.AllOf?.FirstOrDefault()) };
            //else
            //    await foreach (var item in items)
            //        yield return item;


            async Task<string> GetText(IConditionPart cond)
            {
                string? additional = null;
                if (cond is LogQueryCriteria lq)
                {
                    if (!string.IsNullOrEmpty(lq.SearchQuery) && aiQueryService != null)
                    {
                        additional = await QueryAI(aiQueryService, lq.SearchQuery, ctxV2.Condition.WindowStartTime, ctxV2.Condition.WindowEndTime);
                    }
                }
                return cond.ToUserFriendlyString() + (additional == null ? "" : $"\n{additional}");
            }

            string? GetTitleLink(IConditionPart? cond)
            {
                return cond switch
                {
                    LogQueryCriteria lq =>
                        lq.LinkToSearchResultsUi?.ToString(),
                    SingleResourceMultipleMetricCriteria srmm =>
                        null,
                    DynamicThresholdCriteria dt =>
                        null,
                    WebtestLocationAvailabilityCriteria wla =>
                        null,
                    _ =>
                        null
                };
            }
        }
    }
}
