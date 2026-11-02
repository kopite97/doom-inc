using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MobileGame.Foundation
{
    /// <summary>A message identity and captured values, independent of the selected language.</summary>
    public sealed class LocalizedMessage
    {
        public string Table { get; }
        public string Key { get; }
        public IReadOnlyDictionary<string, object> Arguments { get; }

        public LocalizedMessage(string table, string key, params TextArgument[] arguments)
        {
            Table = RequireIdentifier(table, nameof(table));
            Key = RequireIdentifier(key, nameof(key));
            var values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var argument in arguments ?? Array.Empty<TextArgument>())
            {
                var name = RequireIdentifier(argument.Name, nameof(arguments));
                if (values.ContainsKey(name)) throw new ArgumentException("Repeated message argument: " + name);
                if (!IsCapturedValue(argument.Value))
                    throw new ArgumentException("Message arguments must be captured values or localized messages: " + name);
                values.Add(name, argument.Value);
            }
            Arguments = new ReadOnlyDictionary<string, object>(values);
        }

        private static string RequireIdentifier(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
                throw new ArgumentException("A nonblank identifier without surrounding whitespace is required.", name);
            return value;
        }

        private static bool IsCapturedValue(object value) => value == null || value is string || value is bool
            || value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint
            || value is long || value is ulong || value is float || value is double || value is decimal
            || value is DateTime || value is TimeSpan || value is Guid || value is LocalizedMessage;

        public override string ToString() => Table + "/" + Key;
    }

    public readonly struct TextArgument
    {
        public string Name { get; }
        public object Value { get; }
        public TextArgument(string name, object value) { Name = name; Value = value; }
    }
}
