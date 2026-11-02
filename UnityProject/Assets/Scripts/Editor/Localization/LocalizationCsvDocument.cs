using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using MobileGame.Foundation;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.SmartFormat.Core.Formatting;

namespace MobileGame.Foundation.Editor
{
    public sealed class LocalizationCsvRow
    {
        public string Key, Context, Contract;
        public long Id;
        public bool Smart;
        public int Line;
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.Ordinal);
        public readonly Dictionary<string, string> Reviews = new Dictionary<string, string>(StringComparer.Ordinal);
        public string Revision => TranslationReview.SourceRevision(Values["ko"], Context, Contract, Smart);
    }

    /// <summary>The package CSV reader supplies quoting/newline handling; project validation runs before asset mutations.</summary>
    public sealed class LocalizationCsvDocument
    {
        public string Topic { get; }
        public List<LocalizationCsvRow> Rows { get; } = new List<LocalizationCsvRow>();
        public IReadOnlyList<string> Languages { get; }
        public LocalizationCsvDocument(string topic, IEnumerable<string> languages)
        { Topic = topic; Languages = languages.OrderBy(c => c, StringComparer.Ordinal).ToArray(); }

        public static LocalizationCsvDocument Read(string topic, string contents, IEnumerable<string> languages, bool requireReviewed = true)
        {
            var doc = new LocalizationCsvDocument(topic, languages);
            if (!doc.Languages.Contains("ko") || !doc.Languages.Contains("en")) throw new FormatException("ko/en are required.");
            var expected = new[] { "Key", "Id", "Shared Comments", "Smart", "Arguments" }
                .Concat(doc.Languages).Concat(doc.Languages.Where(l => l != "ko").Select(l => "Reviewed:" + l)).ToArray();
            using (var reader = new CsvReader(new StringReader(contents.TrimStart('\uFEFF')), CultureInfo.InvariantCulture))
            {
                if (!reader.Read()) throw new FormatException(topic + ": empty CSV.");
                reader.ReadHeader();
                var headers = reader.Context.HeaderRecord;
                if (headers.Length != expected.Length || headers.Distinct(StringComparer.Ordinal).Count() != headers.Length
                    || !new HashSet<string>(headers, StringComparer.Ordinal).SetEquals(expected))
                    throw new FormatException(topic + ": unexpected, duplicate or missing columns. Expected " + string.Join(", ", expected));
                var keys = new HashSet<string>(StringComparer.Ordinal); var ids = new HashSet<long>();
                var line = 1;
                while (reader.Read())
                {
                    line++;
                    var row = new LocalizationCsvRow { Line = line, Key = reader.GetField("Key"),
                        Context = reader.GetField("Shared Comments"), Contract = reader.GetField("Arguments") };
                    try
                    {
                        if (string.IsNullOrWhiteSpace(row.Key) || row.Key != row.Key.Trim() || !keys.Add(row.Key))
                            throw new FormatException("Blank, padded or duplicate key.");
                        // Keep IDs spreadsheet-safe. Existing stable IDs are not regenerated on import.
                        if (!long.TryParse(reader.GetField("Id"), NumberStyles.None, CultureInfo.InvariantCulture, out row.Id)
                            || row.Id <= 0 || row.Id > 999999999999999L || !ids.Add(row.Id))
                            throw new FormatException("ID must be unique, positive and at most 15 decimal digits.");
                        if (!bool.TryParse(reader.GetField("Smart"), out row.Smart)) throw new FormatException("Smart must be true or false.");
                        var arguments = ParseArguments(row.Contract);
                        if (!row.Smart && arguments.Count != 0) throw new FormatException("Arguments require Smart=true.");
                        foreach (var code in doc.Languages)
                        {
                            var value = reader.GetField(code);
                            if (string.IsNullOrWhiteSpace(value)) throw new FormatException(code + ": translation is blank.");
                            row.Values.Add(code, value);
                            if (row.Smart)
                            {
                                var formatter = Smart.CreateDefaultSmartFormat();
                                var parsed = formatter.Parser.ParseFormat(value, formatter.GetNotEmptyFormatterExtensionNames());
                                var used = new HashSet<string>(StringComparer.Ordinal);
                                CollectArguments(parsed, used);
                                if (!used.SetEquals(arguments.Keys)) throw new FormatException(code + ": placeholders differ from Arguments contract.");
                                // Exercise common plural branches, and fail missing selectors rather than shipping error text.
                                foreach (var count in new[] { 0, 1, 2, 5 })
                                {
                                    var sample = arguments.ToDictionary(p => p.Key, p => p.Value == "number" ? (object)count
                                        : p.Value == "bool" ? (object)(count != 0) : "sample", StringComparer.Ordinal);
                                    formatter.Format(CultureInfo.GetCultureInfo(code), value, sample);
                                }
                            }
                            if (code != "ko") row.Reviews.Add(code, reader.GetField("Reviewed:" + code));
                        }
                        if (requireReviewed)
                            foreach (var code in doc.Languages.Where(l => l != "ko"))
                                if (!TranslationReview.IsReviewed(row.Revision, code, row.Values[code], row.Reviews[code]))
                                    throw new FormatException(code + ": translation review required (source, context, arguments or translation changed).");
                        doc.Rows.Add(row);
                    }
                    catch (Exception e) when (e is FormatException || e is ArgumentException || e is ParsingErrors || e is FormattingException)
                    { throw new FormatException($"{topic}.csv row {line}, key '{row.Key}': {e.Message}", e); }
                }
            }
            if (doc.Rows.Count == 0) throw new FormatException(topic + ": no entries.");
            return doc;
        }

        public static Dictionary<string, string> ParseArguments(string contract)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(contract)) return result;
            foreach (var part in contract.Split(';'))
            {
                var pieces = part.Split(':');
                if (pieces.Length != 2 || string.IsNullOrWhiteSpace(pieces[0]) || pieces[0] != pieces[0].Trim()
                    || (pieces[1] != "text" && pieces[1] != "number" && pieces[1] != "bool") || result.ContainsKey(pieces[0]))
                    throw new FormatException("Arguments must be unique name:text|number|bool entries separated by semicolons.");
                result.Add(pieces[0], pieces[1]);
            }
            return result;
        }

        private static void CollectArguments(Format format, HashSet<string> used)
        {
            foreach (var placeholder in format.Items.OfType<Placeholder>())
            {
                // Message arguments are named scalar values, not arbitrary object/member traversal.
                if (placeholder.Selectors.Count > 1) throw new FormatException("Use named scalar arguments, not member paths.");
                if (placeholder.Selectors.Count == 1 && !string.IsNullOrEmpty(placeholder.Selectors[0].RawText))
                    used.Add(placeholder.Selectors[0].RawText);
                if (placeholder.Format != null) CollectArguments(placeholder.Format, used);
            }
        }

        public void ConfirmReview(string key, string language)
        {
            if (language == "ko" || !Languages.Contains(language)) throw new ArgumentException("Choose a target language.");
            var row = Rows.Single(r => r.Key == key);
            row.Reviews[language] = TranslationReview.ReviewStamp(row.Revision, language, row.Values[language]);
        }

        public string Write()
        {
            using (var output = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new CsvWriter(output, CultureInfo.InvariantCulture))
            {
                foreach (var header in new[] { "Key", "Id", "Shared Comments", "Smart", "Arguments" }
                    .Concat(Languages).Concat(Languages.Where(l => l != "ko").Select(l => "Reviewed:" + l))) writer.WriteField(header);
                writer.NextRecord();
                foreach (var row in Rows.OrderBy(r => r.Key, StringComparer.Ordinal))
                {
                    writer.WriteField(row.Key); writer.WriteField(row.Id.ToString(CultureInfo.InvariantCulture));
                    writer.WriteField(row.Context); writer.WriteField(row.Smart ? "true" : "false"); writer.WriteField(row.Contract);
                    foreach (var code in Languages) writer.WriteField(row.Values[code]);
                    foreach (var code in Languages.Where(l => l != "ko")) writer.WriteField(row.Reviews[code]);
                    writer.NextRecord();
                }
                writer.Flush(); return output.ToString();
            }
        }
    }
}
