using System;

namespace MobileGame.Foundation
{
    // Describes editing only; never participates in JSON or gameplay validation.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class BalanceFieldAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Unit { get; set; } = "";
        public string Group { get; set; } = "";
        public int Order { get; set; }
        public string ConditionField { get; set; } = "";
        public string[] ConditionValues { get; set; } = Array.Empty<string>();
        public BalanceFieldAttribute(string name, string description) { Name = name; Description = description; }
    }
}
