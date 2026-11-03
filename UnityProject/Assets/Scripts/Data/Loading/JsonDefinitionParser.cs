using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MobileGame.Foundation
{
    public static class JsonDefinitionParser
    {
        public static readonly IReadOnlyList<string> Topics = Array.AsReadOnly(new[] { "resources", "creatures", "facilities", "invasion", "session" });
        private static readonly Type[] Types = { typeof(ResourceDto), typeof(CreatureDto), typeof(FacilityDto), typeof(InvasionDto), typeof(SessionDto) };
        private static readonly Regex NumberToken = new Regex(@"\G-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?", RegexOptions.CultureInvariant);

        public static IReadOnlyDictionary<string, IReadOnlyList<DefinitionDto>> Parse(GameDataBundle bundle)
        {
            var result = new Dictionary<string, IReadOnlyList<DefinitionDto>>(StringComparer.Ordinal);
            for (var i = 0; i < Topics.Count; i++) result.Add(Topics[i], ParseTopic(Topics[i], bundle.Tables[Topics[i]], Types[i]));
            return result;
        }

        private static IReadOnlyList<DefinitionDto> ParseTopic(string topic, string text, Type type)
        {
            try
            {
                var root = ReadStrict(topic, text) as JObject ?? throw new FormatException("$: expected object.");
                CheckFields(root, new[] { "schemaVersion", "entries" });
                if (Integer(Required(root, "schemaVersion"), false) != 1) throw new FormatException("$.schemaVersion: unsupported version.");
                var entries = Required(root, "entries") as JArray ?? throw new FormatException("$.entries: expected array.");
                if (entries.Count == 0) throw new FormatException("$.entries: at least one definition required.");
                var rows = new List<DefinitionDto>(); var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (var entry in entries)
                {
                    var obj = entry as JObject ?? throw new FormatException(entry.Path + ": expected definition object.");
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    CheckFields(obj, fields.Select(f => f.Name));
                    var dto = (DefinitionDto)Activator.CreateInstance(type); dto.Location = topic + ".json: $." + entry.Path;
                    dto.definitionId = obj["definitionId"]?.Type == JTokenType.String ? (string)obj["definitionId"] : null;
                    foreach (var field in fields)
                    {
                        var value = obj[field.Name];
                        if (value == null && OptionalFacilityField(obj, field.Name))
                        { if (field.FieldType != typeof(string)) dto.NumericText[field.Name] = "0"; continue; }
                        value = Required(obj, field.Name);
                        try
                        {
                            var typed = ReadField(value, field.FieldType);
                            if ((dto is CreatureDto || dto is FacilityDto) && typed is double work
                                && (float.IsInfinity((float)work) || work != 0 && (float)work == 0))
                                throw Invalid(value, "finite non-underflowing Single runtime value");
                            field.SetValue(dto, typed);
                        }
                        catch (FormatException error) { throw dto.Error(field.Name, error.Message); }
                        if (field.FieldType == typeof(int) || field.FieldType == typeof(long) || field.FieldType == typeof(double))
                            dto.NumericText[field.Name] = value.Annotation<ExactNumber>().Canonical;
                    }
                    if (string.IsNullOrWhiteSpace(dto.definitionId) || dto.definitionId != dto.definitionId.Trim()) throw dto.Error("definitionId", "Nonempty ID without surrounding whitespace required.");
                    if (!ids.Add(dto.definitionId)) throw dto.Error("definitionId", "Duplicate ID.");
                    if (dto is ThingDefinitionDto thing && string.IsNullOrWhiteSpace(thing.displayNameKey)) throw dto.Error("displayNameKey", "Nonblank display name required.");
                    rows.Add(dto);
                }
                return rows.AsReadOnly();
            }
            catch (Exception error) when (error is JsonException || error is FormatException || error is OverflowException || error is ArgumentException)
            { throw new FormatException(topic + ".json: " + error.Message, error); }
        }

        private static bool OptionalFacilityField(JObject row, string name)
        {
            // Defaults recreate all former CSV columns, including unused zero/empty cells in the fingerprint.
            var optional = new[] { "storageCapacity", "gatheringAmount", "gatheringSeconds", "inputResource", "outputResource", "supportOutput", "invaderOutput", "inputAmount", "outputAmount", "productionSeconds" };
            if (!optional.Contains(name)) return false;
            var role = (string)row["role"];
            if (role == "Storage") return name != "storageCapacity";
            if (role == "Gathering") return name != "gatheringAmount" && name != "gatheringSeconds";
            if (role == "Processing" || role == "Cultivation")
                return name == "storageCapacity" || name == "gatheringAmount" || name == "gatheringSeconds"
                    || (role == "Processing" ? name == "supportOutput" || name == "invaderOutput" : name == "outputResource");
            return false;
        }
        private static void CheckFields(JObject obj, IEnumerable<string> allowed)
        {
            var names = new HashSet<string>(allowed, StringComparer.Ordinal);
            foreach (var property in obj.Properties()) if (!names.Contains(property.Name)) throw new FormatException(property.Path + ": unknown field.");
        }
        private static JToken Required(JObject obj, string name)
        {
            var value = obj[name];
            if (value == null || value.Type == JTokenType.Null) throw new FormatException(obj.Path + "." + name + ": missing/null required field, ID '" + (string)obj["definitionId"] + "'.");
            return value;
        }
        private static object ReadField(JToken value, Type type)
        {
            if (type == typeof(string))
            { if (value.Type != JTokenType.String) throw Invalid(value, "string"); return (string)value; }
            if (type == typeof(int)) return checked((int)Integer(value, false));
            if (type == typeof(long)) return Integer(value, true);
            if (type == typeof(double))
            {
                var raw = Numeric(value);
                if (!double.TryParse(raw.Canonical, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                    || double.IsInfinity(number) || double.IsNaN(number) || number == 0 && raw.Canonical != "0") throw Invalid(value, "finite non-underflowing number");
                return number;
            }
            if (type == typeof(string[]))
            {
                var ids = value as JArray ?? throw Invalid(value, "array of definition IDs");
                return ids.Select(id => (string)ReadField(id, typeof(string))).ToArray();
            }
            if (type == typeof(ThingComponentKind[]))
            {
                var array = value as JArray ?? throw Invalid(value, "component array");
                var items = array.Select(v => (ThingComponentKind)ReadField(v, typeof(ThingComponentKind))).ToArray();
                if (items.Distinct().Count() != items.Length) throw Invalid(value, "unique components");
                return items;
            }
            if (type.IsEnum)
            {
                if (value.Type != JTokenType.String || !Enum.GetNames(type).Contains((string)value)) throw Invalid(value, type.Name + " name");
                return Enum.Parse(type, (string)value);
            }
            throw Invalid(value, "supported DTO type");
        }
        private static long Integer(JToken value, bool wide)
        {
            var raw = Numeric(value);
            if (!long.TryParse(raw.Canonical, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)
                || !wide && (number < int.MinValue || number > int.MaxValue)) throw Invalid(value, wide ? "Int64" : "Int32");
            return number;
        }
        private static ExactNumber Numeric(JToken value) => value.Annotation<ExactNumber>() ?? throw Invalid(value, "number (quoted numbers are invalid)");
        private static FormatException Invalid(JToken value, string expected) => new FormatException("$." + value.Path + ": expected " + expected + ".");

        private sealed class ExactNumber
        {
            public readonly string Canonical;
            public ExactNumber(string text)
            {
                var negative = text.StartsWith("-", StringComparison.Ordinal); if (negative) text = text.Substring(1);
                var parts = text.Split('e', 'E'); var exponent = 0;
                if (parts.Length == 2 && (!int.TryParse(parts[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out exponent) || Math.Abs((long)exponent) > 10000)) throw new FormatException("Numeric exponent outside supported range.");
                var point = parts[0].IndexOf('.'); var scale = (point < 0 ? 0 : parts[0].Length - point - 1) - exponent;
                var digits = parts[0].Replace(".", "").TrimStart('0');
                if (digits.Length == 0) { Canonical = "0"; return; }
                while (digits.EndsWith("0", StringComparison.Ordinal)) { digits = digits.Substring(0, digits.Length - 1); scale--; }
                if (scale > 1000 || scale < -1000) throw new FormatException("Numeric magnitude outside supported range.");
                Canonical = scale <= 0 ? digits + new string('0', -scale) : scale >= digits.Length
                    ? "0." + new string('0', scale - digits.Length) + digits : digits.Insert(digits.Length - scale, ".");
                if (negative) Canonical = "-" + Canonical;
            }
        }

        // Json.NET handles the object tree/escaping. This lexical gate rejects its non-standard extensions.
        private static JToken ReadStrict(string topic, string text)
        {
            if (text == null) throw new FormatException("Missing JSON text.");
            text = text.TrimStart('\uFEFF'); var numbers = new List<ExactNumber>(); char previous = '\0';
            for (var i = 0; i < text.Length;)
            {
                var c = text[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n') { i++; continue; }
                if (c == '"')
                {
                    i++; var closed = false;
                    while (i < text.Length)
                    {
                        c = text[i++]; if (c < 32) throw new FormatException("Unescaped control character at offset " + (i - 1));
                        if (c == '\\') { i++; continue; }
                        if (c == '"') { closed = true; break; }
                    }
                    if (!closed) throw new FormatException("Unterminated string."); previous = 's'; continue;
                }
                if (c == '-' || c >= '0' && c <= '9')
                {
                    var match = NumberToken.Match(text, i); if (!match.Success) throw new FormatException("Invalid number at offset " + i);
                    numbers.Add(new ExactNumber(match.Value)); i += match.Length; previous = 'n'; continue;
                }
                var literal = new[] { "true", "false", "null" }.FirstOrDefault(v => text.IndexOf(v, i, StringComparison.Ordinal) == i);
                if (literal != null) { i += literal.Length; previous = 'v'; continue; }
                if ("{}[],:".IndexOf(c) < 0) throw new FormatException("Non-standard JSON token at offset " + i);
                if ((c == '}' || c == ']') && previous == ',') throw new FormatException("Trailing comma at offset " + i);
                previous = c; i++;
            }
            using (var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Double, MaxDepth = 64 })
            {
                var tree = JToken.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error, LineInfoHandling = LineInfoHandling.Load });
                if (reader.Read()) throw new FormatException("Additional JSON content.");
                var values = tree is JContainer container ? container.DescendantsAndSelf().Where(v => v.Type == JTokenType.Float || v.Type == JTokenType.Integer).ToArray() : new[] { tree };
                if (values.Length != numbers.Count) throw new FormatException("Non-standard number/property syntax.");
                for (var i = 0; i < values.Length; i++) values[i].AddAnnotation(numbers[i]);
                return tree;
            }
        }
    }
}
