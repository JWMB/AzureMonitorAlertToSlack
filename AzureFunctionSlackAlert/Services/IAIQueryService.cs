using System;
using System.Data;
using System.Threading.Tasks;

public interface IAIQueryService
{
    Task<DataTable> GetQueryAsDataTable(string query, DateTimeOffset start, DateTimeOffset end);
}
