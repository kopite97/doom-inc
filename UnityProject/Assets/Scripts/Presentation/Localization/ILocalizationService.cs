using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MobileGame.Presentation
{
    public sealed class LanguageOption
    {
        public string Code { get; }
        public string NativeName { get; }
        public LanguageOption(string code, string nativeName) { Code = code; NativeName = nativeName; }
    }

    /// <summary>Formatting is synchronous only after the service has prepared its tables.</summary>
    public interface ILocalizationService : IDisposable
    {
        bool IsReady { get; }
        string CurrentLanguage { get; }
        IReadOnlyList<LanguageOption> Languages { get; }
        event Action LanguageChanged;
        bool HasEntry(string table, string key, string language);
        Task InitializeAsync(CancellationToken cancellationToken);
        Task ChangeLanguageAsync(string code, CancellationToken cancellationToken);
        string Format(string table, string key, string language, IReadOnlyDictionary<string, object> arguments);
    }
}
