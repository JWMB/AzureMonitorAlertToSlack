﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMonitorAlertToSlack.Alerts;

namespace AzureMonitorAlertToSlack
{
    public interface IMessageSender<T, TPart>
        where T : ISummarizedAlert<TPart>, new()
        where TPart : ISummarizedAlertPart, new()
    {
        Task SendMessage(T parts);
    }
}
