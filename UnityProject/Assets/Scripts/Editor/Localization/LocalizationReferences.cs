using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    /// <summary>Checks the project's code-owned bindings and JSON definition-name references.</summary>
    public static class LocalizationReferences
    {
        private static readonly Regex Calls = new Regex(@"\b(LocalizedMessage|Format|L|M|R|C|SetLabel|Emergency)\s*\(");
        private static readonly Regex Strings = new Regex("\"((?:\\\\.|[^\"\\\\])*)\"");
        public static HashSet<string> Validate(IEnumerable<LocalizationCsvDocument> documents)
        {
            var keys = documents.ToDictionary(d => d.Topic, d => new HashSet<string>(d.Rows.Select(r => r.Key)), StringComparer.Ordinal);
            var used = new HashSet<string>(StringComparer.Ordinal);
            void Check(string table, string key, string source)
            {
                if (!keys.TryGetValue(table, out var entries) || !entries.Contains(key))
                    throw new InvalidDataException(source + ": missing translation " + table + "/" + key);
                used.Add(table + "/" + key);
            }
            foreach (var path in Directory.GetFiles("Assets/Scripts/Presentation", "*.cs", SearchOption.AllDirectories))
            {
                var code = File.ReadAllText(path);
                foreach (Match call in Calls.Matches(code))
                {
                    var args = Arguments(code, call.Index + call.Length);
                    if (args.Count == 0) continue;
                    var method = call.Groups[1].Value;
                    string table; string expression;
                    if (method == "Format" || method == "LocalizedMessage")
                    {
                        if (args.Count < 2 || !Regex.IsMatch(args[0].Trim(), "^\"[^\"]+\"$")) continue;
                        table = Strings.Match(args[0]).Groups[1].Value; expression = args[1];
                    }
                    else
                    {
                        table = method == "C" ? "Common" : method == "R" ? "Results" : method == "Emergency" ? "Bootstrap" : "Laboratory";
                        if (method == "SetLabel" && args.Count < 2) continue;
                        expression = args[method == "SetLabel" ? 1 : 0];
                    }
                    // Keys are literals or conditional literal branches. Dynamic definition keys are checked below.
                    var conditional = expression.IndexOf('?');
                    if (conditional >= 0) expression = expression.Substring(conditional + 1);
                    foreach (Match literal in Strings.Matches(expression)) Check(table, Regex.Unescape(literal.Groups[1].Value), path);
                }
            }
            foreach (var path in Directory.GetFiles("Assets/JSON/Balance", "*.json"))
            {
                var doc = JsonUtility.FromJson<DefinitionNames>(File.ReadAllText(path));
                if (doc?.entries == null) continue;
                foreach (var entry in doc.entries)
                    if (!string.IsNullOrEmpty(entry.displayNameKey)) Check("Definitions", entry.displayNameKey, path);
            }
            return used;
        }

        private static List<string> Arguments(string code, int start)
        {
            var result = new List<string>(); var depth = 0; var quoted = false; var escape = false;
            for (var i = start; i < code.Length; i++)
            {
                var c = code[i];
                if (quoted)
                {
                    if (escape) escape = false;
                    else if (c == '\\') escape = true;
                    else if (c == '"') quoted = false;
                    continue;
                }
                if (c == '"') { quoted = true; continue; }
                if (c == '(' || c == '[' || c == '{') depth++;
                else if (c == ')' && depth == 0) { result.Add(code.Substring(start, i - start)); break; }
                else if (c == ')' || c == ']' || c == '}') depth--;
                else if (c == ',' && depth == 0) { result.Add(code.Substring(start, i - start)); start = i + 1; }
            }
            return result;
        }
        [Serializable] private sealed class DefinitionNames { public DefinitionName[] entries; }
        [Serializable] private sealed class DefinitionName { public string displayNameKey; }
    }
}
