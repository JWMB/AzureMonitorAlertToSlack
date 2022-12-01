using AzureMonitorAlertToSlack.Services.LogQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureMonitorAlertToSlack.Services
{
    public interface ILogQueryServiceFactory
    {
        ILogQueryService? CreateLogQueryService(string targetResourceType);
    }
}
