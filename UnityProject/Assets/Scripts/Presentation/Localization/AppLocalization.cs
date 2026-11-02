using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Persistence;
using UnityEngine;

namespace MobileGame.Presentation
{
    /// <summary>Owned by the app entry lifetime, or by an isolated diagnostic entry. Never a service locator.</summary>
    public sealed class AppLocalization : IDisposable
    {
        private readonly BootstrapText bootstrap;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private int request;
        private bool disposed;
        public ILocalizationService Service { get; }
        public TextManager Texts { get; }
        public LanguageSettingsStore Preferences { get; }
        public bool IsChanging { get; private set; }
        public string ChangeError { get; private set; }
        public AppLocalization(LocalizationAssets assets, string preferencesPath, ILocalizationService service = null)
        {
            bootstrap = new BootstrapText(assets);
            Preferences = new LanguageSettingsStore(preferencesPath); Preferences.Load();
            Service = service ?? new UnityLocalizationService(assets, Preferences.Language, () => Emergency("generic_error"));
            Texts = new TextManager(Service);
            Service.LanguageChanged += InvalidateInput;
        }
        public Task InitializeAsync() => Service.InitializeAsync(lifetime.Token);
        public string Emergency(string key)
        {
            return bootstrap.Get(key, Service?.CurrentLanguage ?? Preferences?.Language);
        }
        public string Format(string table, string key, params TextArgument[] arguments) => Format(new LocalizedMessage(table, key, arguments));
        public string Format(LocalizedMessage message)
        {
            if (Texts.IsReady) return Texts.Format(message);
            return Emergency(message.Table == "Bootstrap" ? message.Key : "generic_error");
        }
        public async Task SelectLanguageAsync(string code)
        {
            if (disposed) throw new ObjectDisposedException(nameof(AppLocalization));
            var serial = ++request; IsChanging = true; ChangeError = null; AppUiLifetime.Invalidate();
            try
            {
                await Service.ChangeLanguageAsync(code, lifetime.Token);
                if (disposed || serial != request) return;
                Preferences.TrySet(Service.CurrentLanguage);
            }
            catch (OperationCanceledException) when (disposed || lifetime.IsCancellationRequested) { }
            catch (Exception e)
            {
                if (!disposed && serial == request) { ChangeError = e.Message; Debug.LogWarning("Language change failed: " + e.Message); }
            }
            finally { if (!disposed && serial == request) { IsChanging = false; AppUiLifetime.Invalidate(); } }
        }
        private void InvalidateInput() => AppUiLifetime.Invalidate();
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; request++; lifetime.Cancel(); Texts.Dispose();
            Service.LanguageChanged -= InvalidateInput; Service.Dispose(); lifetime.Dispose();
        }
    }
}
