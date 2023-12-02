# AzureMonitorAlertToSlack

A library for summarizing Azure alerts (using [JWMB.AzureMonitorCommonAlertSchemaTypes](https://www.nuget.org/packages/JWMB.AzureMonitorCommonAlertSchemaTypes/)) and posting them to Slack (using [SlackNet](https://www.nuget.org/packages/SlackNet)).

Real-world usage example here: https://github.com/JWMB/AzureFunctionAlert2Slack/blob/main/AzureFunctionAlert2Slack/RequestToSlackFunction.cs

Notes
* Provide a custom Azure message summarizer by injecting your own implementation of `IDemuxedAlertHandler` into `AlertInfoFactory`
  * The design ensures that when `JWMB.AzureMonitorCommonAlertSchemaTypes` is extended with more signal types, the `IDemuxedAlertHandler` must implement those additions (or it will not compile)
* (In progress) For Log Alerts V2 where `SearchQuery` is present, fetch query results from the corresponding Azure service (Log Analytics or Application Insights)
  * Currently relies on Managed Identity authorization
  * `Azure.Monitor.Query.LogsQueryClient` doesn't parse the response properly

Simplified usage (error handling omitted for readability)
```CSharp
[FunctionName("HttpAlertToSlack")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    IAlertInfoFactory alertInfoFactory, IMessageSender sender,
    ILogger log)
{
    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    var summary = await alertInfoFactory.Process(requestBody);
    var messages = messageFactory.CreateMessages(summary);

    await sender.SendMessage(messages);

    return new OkObjectResult("");
}
```