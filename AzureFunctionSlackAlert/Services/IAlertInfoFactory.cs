﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionSlackAlert.Services
{
    public interface IAlertInfoFactory
    {
        Task<List<AlertInfo>> Process(string requestBody);
    }
}