using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MobileGame.Foundation.Editor
{
    public sealed class BalanceFieldInfo
    {
        public FieldInfo Field { get; }
        public BalanceFieldAttribute Attribute { get; }
        public string Error { get; }
        public string Signature => Field.DeclaringType.FullName + "|" + Field.FieldType.FullName + "|" + Attribute?.ConditionField + "|"
            + string.Join("|", Attribute?.ConditionValues ?? Array.Empty<string>()) + "|" + (Attribute != null);
        public BalanceFieldInfo(FieldInfo field)
        {
            Field = field; Attribute = field.GetCustomAttribute<BalanceFieldAttribute>();
            if (Attribute == null) return;
            if (string.IsNullOrWhiteSpace(Attribute.Name) || string.IsNullOrWhiteSpace(Attribute.Description)) Error = "메타데이터 이름/설명 누락";
            else if (field.FieldType != typeof(int) && field.FieldType != typeof(long) && field.FieldType != typeof(double)) Error = "미지원 자료형";
            else if (!string.IsNullOrEmpty(Attribute.ConditionField))
            {
                var condition = field.DeclaringType.GetField(Attribute.ConditionField);
                if (condition == null || Attribute.ConditionValues == null || Attribute.ConditionValues.Length == 0
                    || (condition.FieldType != typeof(string) && !condition.FieldType.IsEnum)) Error = "메타데이터 조건 참조 오류";
                else if (condition.FieldType.IsEnum && Attribute.ConditionValues.Any(v => !Enum.GetNames(condition.FieldType).Contains(v))) Error = "메타데이터 조건 값 오류";
            }
        }
        public string Diagnostic(BalanceJsonToken entry)
        {
            if (Attribute == null) return "속성 없음";
            if (Error != null) return Error;
            var reasons = new List<string>();
            if (!entry.Fields.ContainsKey(Field.Name)) reasons.Add("JSON 값 없음");
            if (!string.IsNullOrEmpty(Attribute.ConditionField)
                && (!entry.Fields.TryGetValue(Attribute.ConditionField, out var condition) || !Attribute.ConditionValues.Contains(condition.StringValue)))
                reasons.Add("역할·행동 조건 불일치");
            return reasons.Count == 0 ? "편집 가능" : string.Join(" / ", reasons);
        }
        public void ValidateNumber(string raw)
        {
            var token = BalanceJsonToken.Parse(raw);
            if (token.Kind != 'n') throw new FormatException("숫자를 입력하세요: " + Field.Name);
            if (Field.FieldType == typeof(int)) { if (!int.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)) throw new FormatException("Int32 정수가 필요합니다."); }
            else if (Field.FieldType == typeof(long)) { if (!long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)) throw new FormatException("Int64 정수가 필요합니다."); }
            else if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || double.IsNaN(value) || double.IsInfinity(value))
                throw new FormatException("유한 소수가 필요합니다.");
        }
    }

    public static class BalanceMetadataReader
    {
        public static IReadOnlyList<BalanceFieldInfo> Read(Type dto) => dto.GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(f => new BalanceFieldInfo(f)).OrderBy(f => f.Attribute?.Order ?? int.MaxValue).ThenBy(f => f.Field.Name, StringComparer.Ordinal).ToArray();
    }
}
