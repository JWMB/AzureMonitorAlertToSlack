using AzureMonitorCommonAlertSchemaTypes.AlertContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AzureMonitorAlertToSlack
{
    public class ConvertToString
    {
        private readonly int maxColumnLength;

        public ConvertToString(int maxColumnLength = 100)
        {
            this.maxColumnLength = maxColumnLength;
        }

        public string Convert(object obj, Type type)
        {
            if (obj.GetType() == type)
            {
                var str = obj switch
                {
                    bool val => Convert(val),
                    DateTime val => Convert(val),
                    DateTimeOffset val => Convert(val),
                    decimal val => Convert(val),
                    Guid val => Convert(val),
                    int val => Convert(val),
                    long val => Convert(val),
                    string val => Convert(val),
                    TimeSpan val => Convert(val),
                    object val => val.ToString()
                };
                return PostProcess(str ?? obj.ToString() ?? "", obj, type);
            }
            return PostProcess(obj.ToString() ?? "", obj, type);
        }

        public virtual string Convert(bool obj) => obj ? "1" : "0";
        public virtual string Convert(DateTime obj) => obj.ToString("yyyy-MM-dd HH:mm:ss");
        public virtual string Convert(DateTimeOffset obj) => obj.ToString("yyyy-MM-dd HH:mm:ss");
        public virtual string Convert(decimal obj) => obj.ToString("", System.Globalization.CultureInfo.InvariantCulture);
        public virtual string Convert(Guid obj) => obj.ToString();
        public virtual string Convert(int obj) => obj.ToString();
        public virtual string Convert(long obj) => obj.ToString();
        public virtual string Convert(string obj) => obj;
        public virtual string Convert(TimeSpan obj) => obj.ToString();

        public virtual string PostProcess(string converted, object obj, Type type) => Truncate(converted, maxColumnLength);
        public static string Truncate(string str, int maxLength, string? ellipsis = null) => str.Length > maxLength ? str.Remove(maxLength) : str;
    }

    public class TableHelpers
    {
        public static string TableToMarkdown(DataTable dt, Func<object, Type, string> stringify, int maxRows = 100)
        {
            var cols = new List<DataColumn>();
            foreach (DataColumn col in dt.Columns)
                cols.Add(col);

            var rows = new List<DataRow>();
            foreach (DataRow row in dt.Rows)
                rows.Add(row);

            var allStrings = new[] { cols.Select(o => o.ColumnName).ToList() }
                .Concat(rows.Take(maxRows).Select(row => cols.Select(col => stringify(row[col], col.DataType))));

            return TableToMarkdown(allStrings);
        }

        public static string TableToMarkdown(IEnumerable<IEnumerable<string>> table)
        {
            if (table.FirstOrDefault()?.Any() != true)
                return string.Empty;

            var lengthsPerColumn = table.SelectMany(row => row.Select((c, i) => new { Index = i, Length = c?.Length ?? 0 }))
                .GroupBy(o => o.Index)
                .Select(grp => new { Index = grp.Key, Lengths = grp.Select(x => x.Length).ToList() });

            var useLengths = lengthsPerColumn.ToDictionary(o => o.Index, o => o.Lengths.Max()); //.Select(o => new { o.Index, Length = o.Lengths.Max() });
            var delim = "|";

            //{CreateString('-', useLengths.Values.Sum() + (useLengths.Count + 1) * delim.Length)}

            return $@"
{CreateRow(table.First())}
{CreateRow(table.First().Select((o, i) => RenderCell("", useLengths[i], '-')))}
{string.Join("\n", table.Skip(1).Select(CreateRow))}
".Trim().Replace("\r", "");


            string CreateRow(IEnumerable<string> row) =>
                delim + string.Join(delim, row.Select((o, i) => RenderCell(o, useLengths[i]))) + delim;

            string RenderCell(string value, int maxLength, char fillChar = ' ') =>
                $"{(value.Length > maxLength ? value.Remove(maxLength) : value)}{CreateString(fillChar, maxLength - value.Length)}";

            string CreateString(char c, int length) =>
                length > 0 ? string.Join("", Enumerable.Range(0, length).Select(o => c)) : string.Empty;
        }

        public static DataTable TableToDataTable(Table table)
        {
            // TODO: Should this Table actually be Azure.Monitor.Query.Models.LogTable..? Looks similar
            // Otherwise, we should create a deserializer that handles typing
            var dt = new DataTable(table.Name);
            foreach (var col in table.Columns)
                dt.Columns.Add(new DataColumn(col.Name, TypenameToType(col.Type)));

            foreach (var row in table.Rows)
            {
                var dr = dt.NewRow();
                dr.ItemArray = row.Select((o, i) => ConvertTo(o, dt.Columns[i].DataType)).ToArray();
                dt.Rows.Add(dr);
            }
            return dt;

            object? ConvertTo(string input, Type type)
            {
                if (type == typeof(DateTimeOffset))
                    return DateTimeOffset.TryParse(input, out var result) ? (DateTimeOffset?)result : null;
                if (type == typeof(Guid))
                    return Guid.TryParse(input, out var result) ? (Guid?)result : null;
                if (type == typeof(TimeSpan))
                    return TimeSpan.TryParse(input, out var result) ? (TimeSpan?)result : null;
                return Convert.ChangeType(input, type, System.Globalization.CultureInfo.InvariantCulture);
            }

            Type TypenameToType(string typename)
            {
                switch (typename.ToLower())
                {
                    case "bool": return typeof(bool);
                    case "datetime": return typeof(DateTimeOffset);
                    case "decimal": return typeof(decimal);
                    case "dynamic": return typeof(object);
                    case "guid": return typeof(Guid);
                    case "int": return typeof(int);
                    case "long": return typeof(long);
                    case "string": return typeof(string);
                    case "timespan": return typeof(TimeSpan);
                    default: return typeof(string);
                }
            }
        }
    }
}
