using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace MobileGame.Presentation
{
    /// <summary>Small generated CSV subset, available before the package or its tables are ready.</summary>
    public sealed class BootstrapText
    {
        private readonly BootstrapTextDocument document;
        public BootstrapText(LocalizationAssets assets)
        {
            if (assets == null || assets.bootstrapText == null) throw new ArgumentException("Missing localization recovery text.");
            document = JsonUtility.FromJson<BootstrapTextDocument>(assets.bootstrapText.text);
            if (document?.entries == null) throw new ArgumentException("Invalid localization recovery text.");
        }
        public string Get(string key, string language = null)
        {
            var entry = document.entries.Single(e => e.key == key);
            var code = language ?? CultureInfo.CurrentUICulture.Name;
            while (!string.IsNullOrEmpty(code))
            {
                var translation = entry.translations.FirstOrDefault(t => string.Equals(t.language, code, StringComparison.OrdinalIgnoreCase));
                if (translation != null) return translation.text;
                var split = code.LastIndexOf('-'); code = split < 0 ? null : code.Substring(0, split);
            }
            return entry.translations.Single(t => t.language == "ko").text;
        }
    }
}
