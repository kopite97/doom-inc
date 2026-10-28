using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileGame.Foundation
{
    public interface ICsvDataSource
    {
        Task<CsvBundle> LoadAsync(CancellationToken cancellationToken);
    }

    public sealed class CsvBundle
    {
        public int SchemaVersion { get; }
        public string Source { get; }
        public IReadOnlyDictionary<string, string> Tables { get; }
        public string ContentHash { get; }
        public CsvBundle(IDictionary<string, string> tables, string source, int schemaVersion = 1)
        {
            if (tables == null || tables.Any(p => string.IsNullOrWhiteSpace(p.Key) || p.Value == null))
                throw new ArgumentException("CSV bundle contains missing names or contents.");
            Tables = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(tables, StringComparer.Ordinal));
            Source = source; SchemaVersion = schemaVersion;
            ContentHash = Hash(string.Concat(Tables.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => Token(p.Key) + Token(p.Value))));
        }
        internal static string Token(string value) => value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
        internal static string Hash(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
    }

    // Input DTO only. Typed reads report the source location; these rows never become runtime definitions.
    public sealed class CsvRow
    {
        private readonly IReadOnlyDictionary<string, string> cells;
        public string Table { get; }
        public int Line { get; }
        public IReadOnlyDictionary<string, string> Cells => cells;
        internal CsvRow(string table, int line, IDictionary<string, string> cells)
        {
            Table = table; Line = line;
            this.cells = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(cells, StringComparer.Ordinal));
        }
        public string Text(string column) => cells.TryGetValue(column, out var value) ? value : throw Error(column, "Missing column.");
        public string Id(string column = "definitionId")
        {
            var id = Text(column);
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim()) throw Error(column, "ID must be nonempty without surrounding whitespace.");
            return id;
        }
        public int Int(string column)
        {
            if (!int.TryParse(Text(column).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw Error(column, "Expected Int32.");
            return value;
        }
        public long Long(string column)
        {
            if (!long.TryParse(Text(column).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw Error(column, "Expected Int64.");
            return value;
        }
        public double Number(string column)
        {
            if (!double.TryParse(Text(column).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                || double.IsNaN(value) || double.IsInfinity(value)) throw Error(column, "Expected finite number.");
            return value;
        }
        public float Float(string column)
        {
            var value = (float)Number(column);
            if (float.IsInfinity(value)) throw Error(column, "Value exceeds Single range.");
            return value;
        }
        public T EnumValue<T>(string column) where T : struct
        {
            if (!Enum.TryParse<T>(Text(column).Trim(), false, out var value) || !Enum.IsDefined(typeof(T), value))
                throw Error(column, "Unknown " + typeof(T).Name);
            return value;
        }
        public ThingComponentKind[] Components()
        {
            var values = Text("components").Split('|').Select(v =>
            {
                if (!Enum.TryParse<ThingComponentKind>(v.Trim(), false, out var kind) || !Enum.IsDefined(typeof(ThingComponentKind), kind))
                    throw Error("components", "Unknown component.");
                return kind;
            }).ToArray();
            if (values.Distinct().Count() != values.Length) throw Error("components", "Duplicate component.");
            return values;
        }
        public FormatException Error(string column, string reason) => new FormatException(
            $"{Table}: line {Line}, column {column}, ID '{(cells.TryGetValue("definitionId", out var id) ? id : "?")}': {reason}");
    }

    public static class CsvParser
    {
        public static IReadOnlyList<CsvRow> Parse(string table, string text, string columns)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var records = new List<(int line, List<string> cells)>();
            var cells = new List<string>(); var field = new StringBuilder();
            var quoted = false; var closed = false; var line = 1; var startLine = 1; var started = false;
            text = text.TrimStart('\uFEFF');
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i]; started = true;
                if (quoted)
                {
                    if (c == '"') { if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; } else { quoted = false; closed = true; } }
                    else { field.Append(c); if (c == '\n') line++; }
                    continue;
                }
                if (c == ',' || c == '\r' || c == '\n')
                {
                    cells.Add(field.ToString()); field.Clear(); closed = false;
                    if (c == ',') continue;
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    records.Add((startLine, cells)); cells = new List<string>(); line++; startLine = line; started = false;
                }
                else if (c == '"' && field.Length == 0 && !closed) quoted = true;
                else
                {
                    if (closed || c == '"') throw new FormatException($"{table}: line {line}: malformed quoted field.");
                    field.Append(c);
                }
            }
            if (quoted) throw new FormatException($"{table}: line {startLine}: unterminated quoted field.");
            if (started || cells.Count > 0) { cells.Add(field.ToString()); records.Add((startLine, cells)); }
            if (records.Count < 2) throw new FormatException(table + ": header and at least one row required.");
            var header = records[0].cells; var expected = columns.Split(',');
            if (header.Count != expected.Length || header.Distinct(StringComparer.Ordinal).Count() != header.Count
                || !new HashSet<string>(header, StringComparer.Ordinal).SetEquals(expected))
                throw new FormatException(table + ": missing, duplicate or unknown header.");
            var result = new List<CsvRow>(); var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in records.Skip(1))
            {
                if (record.cells.Count != header.Count) throw new FormatException($"{table}: line {record.line}: wrong column count.");
                var row = new CsvRow(table, record.line, header.Select((h, i) => (h, v: record.cells[i])).ToDictionary(p => p.h, p => p.v));
                if (!ids.Add(row.Id())) throw row.Error("definitionId", "Duplicate ID.");
                result.Add(row);
            }
            return result.AsReadOnly();
        }
    }
}
