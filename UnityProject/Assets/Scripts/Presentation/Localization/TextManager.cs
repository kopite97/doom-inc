using System;
using System.Collections.Generic;
using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    /// <summary>Application-owned text facade; the provider remains the language-state owner.</summary>
    public sealed class TextManager : IDisposable
    {
        private readonly ILocalizationService localization;
        private bool disposed;
        private event Action changed;
        public bool IsReady => !disposed && localization.IsReady;
        public TextManager(ILocalizationService localization)
        {
            this.localization = localization ?? throw new ArgumentNullException(nameof(localization));
            localization.LanguageChanged += OnLanguageChanged;
        }

        public string Format(LocalizedMessage message)
        {
            if (disposed) throw new ObjectDisposedException(nameof(TextManager));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (!localization.IsReady) throw new InvalidOperationException("Localization is not ready.");
            // Capture one locale for the whole expression, including translated argument names.
            var language = localization.CurrentLanguage;
            return Resolve(message, HasEntries(message, language) ? language : "ko");
        }

        private bool HasEntries(LocalizedMessage message, string language)
        {
            if (!localization.HasEntry(message.Table, message.Key, language)) return false;
            foreach (var value in message.Arguments.Values)
                if (value is LocalizedMessage nested && !HasEntries(nested, language)) return false;
            return true;
        }

        private string Resolve(LocalizedMessage message, string language)
        {
            var values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var argument in message.Arguments)
                values.Add(argument.Key, argument.Value is LocalizedMessage nested ? Resolve(nested, language) : argument.Value);
            return localization.Format(message.Table, message.Key, language, values);
        }

        public IDisposable Observe(LocalizedMessage message, Action<string> display)
        {
            if (display == null) throw new ArgumentNullException(nameof(display));
            if (disposed) throw new ObjectDisposedException(nameof(TextManager));
            var subscription = new Subscription(this, message, display);
            changed += subscription.Refresh;
            try { subscription.Refresh(); }
            catch { subscription.Dispose(); throw; }
            return subscription;
        }

        private void OnLanguageChanged() { if (!disposed) changed?.Invoke(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            localization.LanguageChanged -= OnLanguageChanged;
            changed = null;
        }

        private sealed class Subscription : IDisposable
        {
            private TextManager owner;
            private readonly LocalizedMessage message;
            private Action<string> display;
            public Subscription(TextManager owner, LocalizedMessage message, Action<string> display)
            { this.owner = owner; this.message = message; this.display = display; }
            public void Refresh() { if (owner != null) display(owner.Format(message)); }
            public void Dispose()
            {
                if (owner == null) return;
                owner.changed -= Refresh; owner = null; display = null;
            }
        }
    }
}
