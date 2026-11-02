using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MobileGame.Presentation
{
    public sealed class UnityLocalizationService : ILocalizationService
    {
        private readonly string[] tableNames;
        private readonly string preferredLanguage;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly Dictionary<string, Dictionary<string, StringTable>> prepared = new Dictionary<string, Dictionary<string, StringTable>>();
        private readonly Dictionary<string, Task<Dictionary<string, StringTable>>> pendingLocales = new Dictionary<string, Task<Dictionary<string, StringTable>>>();
        private readonly HashSet<string> missing = new HashSet<string>();
        private readonly SemaphoreSlim loadGate = new SemaphoreSlim(1, 1);
        private readonly List<AsyncOperationHandle<StringTable>> retained = new List<AsyncOperationHandle<StringTable>>();
        private bool resetFailedDatabase;
        private bool packageInitializationFailed;
        private readonly Func<string> emergency;
        private Task initialization;
        private int request;
        private bool disposed;
        public bool IsReady { get; private set; }
        public string CurrentLanguage => IsReady ? LocalizationSettings.SelectedLocale.Identifier.Code : null;
        public IReadOnlyList<LanguageOption> Languages { get; private set; } = Array.Empty<LanguageOption>();
        public event Action LanguageChanged;

        public UnityLocalizationService(LocalizationAssets assets, string preferredLanguage, Func<string> emergency)
        {
            if (assets == null || assets.tables == null || assets.tables.Length == 0) throw new ArgumentException("Missing localization table registry.");
            tableNames = assets.tables.Select(t => t != null ? t.TableCollectionName : throw new ArgumentException("Missing table binding.")).ToArray();
            if (tableNames.Distinct(StringComparer.Ordinal).Count() != tableNames.Length) throw new ArgumentException("Repeated localization table.");
            this.preferredLanguage = preferredLanguage; this.emergency = emergency;
        }

        public Task InitializeAsync(CancellationToken token)
        {
            ThrowIfDisposed(); token.ThrowIfCancellationRequested();
            if (IsReady) return Task.CompletedTask;
            if (packageInitializationFailed) { LocalizationSettings.Instance.ResetState(); packageInitializationFailed = false; }
            if (initialization == null || initialization.IsFaulted || initialization.IsCanceled) initialization = Initialize(token);
            return initialization;
        }
        private async Task Initialize(CancellationToken token)
        {
            try
            {
                var operation = LocalizationSettings.InitializationOperation;
                await operation.Task;
                if (operation.Status != AsyncOperationStatus.Succeeded) throw new InvalidOperationException("Localization package initialization failed.");
            }
            catch { packageInitializationFailed = true; throw; }
            Check(token);
            var locales = LocalizationSettings.AvailableLocales.Locales;
            Languages = locales.Select(l => new LanguageOption(l.Identifier.Code, l.Identifier.CultureInfo?.NativeName ?? l.LocaleName)).ToArray();
            if (!Languages.Any(l => l.Code == "ko") || !Languages.Any(l => l.Code == "en")) throw new InvalidOperationException("Korean and English locales are required.");
            var selected = FindSupported(preferredLanguage) ?? FindSupported(CultureInfo.CurrentUICulture.Name) ?? "ko";
            await PrepareLocale("ko");
            await PrepareLocale(selected);
            Check(token);
            LocalizationSettings.SelectedLocale = locales.Single(l => l.Identifier.Code == selected);
            IsReady = true;
        }

        private string FindSupported(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            while (!string.IsNullOrEmpty(code))
            {
                var match = Languages.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match.Code;
                var split = code.LastIndexOf('-'); if (split < 0) break; code = code.Substring(0, split);
            }
            return null;
        }

        public async Task ChangeLanguageAsync(string code, CancellationToken token)
        {
            ThrowIfDisposed(); token.ThrowIfCancellationRequested();
            if (!IsReady) throw new InvalidOperationException("Localization is not ready.");
            if (!Languages.Any(l => l.Code == code)) throw new ArgumentException("Unsupported language: " + code);
            var serial = ++request;
            // Do not set SelectedLocale until every required table is available. Failed changes keep the old locale.
            await PrepareLocale(code);
            Check(token);
            if (serial != request) return;
            if (CurrentLanguage == code) return;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales.Single(l => l.Identifier.Code == code);
            LanguageChanged?.Invoke();
        }

        private Task<Dictionary<string, StringTable>> PrepareLocale(string code)
        {
            if (prepared.TryGetValue(code, out var tables)) return Task.FromResult(tables);
            if (!pendingLocales.TryGetValue(code, out var task) || task.IsFaulted || task.IsCanceled)
                pendingLocales[code] = task = LoadLocale(code);
            return task;
        }
        private async Task<Dictionary<string, StringTable>> LoadLocale(string code)
        {
            await loadGate.WaitAsync(lifetime.Token);
            var acquired = new List<AsyncOperationHandle<StringTable>>();
            try
            {
                Check(CancellationToken.None);
                var database = LocalizationSettings.StringDatabase;
                // Localization 1.5.13 ReleaseTable returns early for a cached null result.
                // Reset only between loads. Acquired successful handles keep the visible language alive.
                if (resetFailedDatabase) { database.ResetState(); resetFailedDatabase = false; }
                var locale = LocalizationSettings.AvailableLocales.Locales.Single(l => l.Identifier.Code == code);
                var tables = new Dictionary<string, StringTable>(StringComparer.Ordinal);
                foreach (var name in tableNames)
                {
                    var operation = database.GetTableAsync(name, locale);
                    acquired.Add(Addressables.ResourceManager.Acquire(operation));
                    var table = await operation.Task;
                    Check(CancellationToken.None);
                    if (table == null) throw new InvalidOperationException("Missing localization table: " + name + "/" + code);
                    tables.Add(name, table);
                }
                prepared[code] = tables;
                retained.AddRange(acquired); acquired.Clear();
                return tables;
            }
            catch { if (!disposed) resetFailedDatabase = true; throw; }
            finally
            {
                foreach (var operation in acquired) if (operation.IsValid()) Addressables.Release(operation);
                loadGate.Release();
            }
        }

        public string Format(string table, string key, string language, IReadOnlyDictionary<string, object> arguments)
        {
            ThrowIfDisposed();
            if (!IsReady) throw new InvalidOperationException("Localization is not ready.");
            var entry = Entry(language, table, key);
            if (entry == null || string.IsNullOrWhiteSpace(entry.LocalizedValue)) return Missing(table, key);
            try
            {
                // All tables are already prepared. This overload avoids SelectedLocaleAsync.Result and any blocking load.
                return entry.GetLocalizedString(CultureInfo.GetCultureInfo(language), new object[] { arguments }, null);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentException || e is ParsingErrors || e is FormattingException)
            { return Missing(table, key); }
        }
        private StringTableEntry Entry(string language, string table, string key)
            => prepared.TryGetValue(language, out var tables) && tables.TryGetValue(table, out var value) ? value.GetEntry(key) : null;
        public bool HasEntry(string table, string key, string language)
            => !disposed && IsReady && !string.IsNullOrWhiteSpace(Entry(language, table, key)?.LocalizedValue);
        private string Missing(string table, string key)
        {
            if (missing.Add(table + "/" + key)) UnityEngine.Debug.LogWarning("Localization entry unavailable: " + table + "/" + key);
            return emergency();
        }
        private void ThrowIfDisposed() { if (disposed) throw new ObjectDisposedException(nameof(UnityLocalizationService)); }
        private void Check(CancellationToken token) { ThrowIfDisposed(); lifetime.Token.ThrowIfCancellationRequested(); token.ThrowIfCancellationRequested(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; IsReady = false; request++; lifetime.Cancel(); lifetime.Dispose(); LanguageChanged = null;
            // Only release the references explicitly acquired by this owner; the database keeps its own references.
            foreach (var operation in retained) if (operation.IsValid()) Addressables.Release(operation);
            retained.Clear();
            prepared.Clear(); pendingLocales.Clear();
        }
    }
}
