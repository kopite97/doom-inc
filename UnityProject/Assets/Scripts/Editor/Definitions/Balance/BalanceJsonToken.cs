using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace MobileGame.Foundation.Editor
{
    // Keeps character spans; it never formats or serializes the document tree.
    public sealed class BalanceJsonToken
    {
        public char Kind;
        public int Start, Length;
        public string Raw, StringValue;
        public readonly Dictionary<string, BalanceJsonToken> Fields = new Dictionary<string, BalanceJsonToken>(StringComparer.Ordinal);
        public readonly List<BalanceJsonToken> Items = new List<BalanceJsonToken>();
        public static BalanceJsonToken Parse(string text)
        {
            var reader = new Reader(text); var result = reader.Value(); reader.Space();
            if (reader.Position != text.Length) throw new FormatException("JSON 뒤에 추가 내용이 있습니다.");
            return result;
        }
        private sealed class Reader
        {
            private static readonly Regex Number = new Regex(@"\G-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?", RegexOptions.CultureInvariant);
            private readonly string text;
            public int Position;
            private int depth;
            public Reader(string text) { this.text = text ?? throw new ArgumentNullException(nameof(text)); }
            public void Space() { while (Position < text.Length && " \r\n\t".IndexOf(text[Position]) >= 0) Position++; }
            private char Current => Position < text.Length ? text[Position] : '\0';
            private void Expect(char c) { Space(); if (Current != c) throw new FormatException("JSON 위치 " + Position + ": " + c + " 필요"); Position++; }
            public BalanceJsonToken Value()
            {
                Space(); if (++depth > 64) throw new FormatException("JSON 중첩 깊이 초과");
                var token = new BalanceJsonToken { Start = Position, Kind = Current };
                if (Current == '{')
                {
                    Position++; Space();
                    if (Current != '}') while (true)
                    {
                        if (Current != '"') throw new FormatException("JSON 속성 이름 필요");
                        var key = Value().StringValue; Expect(':');
                        if (token.Fields.ContainsKey(key)) throw new FormatException("중복 JSON 필드: " + key);
                        token.Fields.Add(key, Value()); Space(); if (Current != ',') break; Position++; Space();
                    }
                    Expect('}');
                }
                else if (Current == '[')
                {
                    Position++; Space();
                    if (Current != ']') while (true) { token.Items.Add(Value()); Space(); if (Current != ',') break; Position++; }
                    Expect(']');
                }
                else if (Current == '"')
                {
                    Position++; bool closed = false;
                    while (Position < text.Length)
                    {
                        var c = text[Position++]; if (c == '"') { closed = true; break; }
                        if (c < 32) throw new FormatException("잘못된 JSON 문자열");
                        if (c == '\\') { if (Position >= text.Length) break; Position++; }
                    }
                    if (!closed) throw new FormatException("닫히지 않은 JSON 문자열");
                    token.StringValue = Decode(text.Substring(token.Start + 1, Position - token.Start - 2));
                }
                else
                {
                    var match = Number.Match(text, Position);
                    if (match.Success) { token.Kind = 'n'; Position += match.Length; }
                    else
                    {
                        string literal = null;
                        foreach (var candidate in new[] { "true", "false", "null" })
                            if (text.Length - Position >= candidate.Length && string.CompareOrdinal(text, Position, candidate, 0, candidate.Length) == 0) { literal = candidate; break; }
                        if (literal == null) throw new FormatException("잘못된 JSON 값, 위치 " + Position);
                        token.Kind = literal[0]; Position += literal.Length;
                    }
                }
                token.Length = Position - token.Start; token.Raw = text.Substring(token.Start, token.Length); depth--; return token;
            }
            private static string Decode(string raw)
            {
                var result = new StringBuilder();
                for (var i = 0; i < raw.Length; i++)
                {
                    if (raw[i] != '\\') { result.Append(raw[i]); continue; }
                    if (++i >= raw.Length) throw new FormatException("JSON escape 오류");
                    switch (raw[i])
                    {
                        case '"': result.Append('"'); break;
                        case '\\': result.Append('\\'); break;
                        case '/': result.Append('/'); break;
                        case 'b': result.Append('\b'); break;
                        case 'f': result.Append('\f'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        case 'u':
                            if (i + 4 >= raw.Length || !ushort.TryParse(raw.Substring(i + 1, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var hex)) throw new FormatException("JSON unicode escape 오류");
                            result.Append((char)hex); i += 4; break;
                        default: throw new FormatException("JSON escape 오류");
                    }
                }
                return result.ToString();
            }
        }
    }
}
