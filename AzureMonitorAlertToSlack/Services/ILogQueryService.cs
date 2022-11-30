using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public interface ILogQueryService
{
    Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end, CancellationToken? cancellationToken = null);
}

public interface ILogAnalyticsQueryService : ILogQueryService { }
public interface IAppInsightsQueryService : ILogQueryService { }
