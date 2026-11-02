#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed class LocalizationProviderTests
    {
        private sealed class Held<T> : AsyncOperationBase<T>
        {
            protected override void Execute() { }
            public void Finish(T result) => Complete(result, true, null);
        }
        [Serializable]
        private sealed class Provider : ITableProvider
        {
            private readonly LocalizationAssets assets;
            public bool FailNext, HoldNext;
            public Action Release;
            public readonly List<StringTable> Created = new List<StringTable>();
            public Provider(LocalizationAssets assets) { this.assets = assets; }
            public AsyncOperationHandle<T> ProvideTableAsync<T>(string name, Locale locale) where T : LocalizationTable
            {
                if (locale.Identifier.Code != "fr" || typeof(T) != typeof(StringTable)) return default;
                if (FailNext) { FailNext = false; return Addressables.ResourceManager.CreateCompletedOperation<T>(null, null); }
                var table = ScriptableObject.CreateInstance<StringTable>(); Created.Add(table);
                table.SharedData = assets.tables.Single(t => t.TableCollectionName == name); table.LocaleIdentifier = locale.Identifier;
                if (name == "Title") table.AddEntry("new_game", "Nouvelle partie");
                if (HoldNext)
                {
                    HoldNext = false; var held = new Held<T>();
                    var handle = Addressables.ResourceManager.StartOperation(held, default);
                    Release = () => { Release = null; held.Finish(table as T); }; return handle;
                }
                return Addressables.ResourceManager.CreateCompletedOperation(table as T, null);
            }
        }

        [UnityTest]
        public IEnumerator FailedLocaleRetriesLastRequestWinsAndThirdLocaleNeedsNoProductBranch()
        {
            var originalSettings = LocalizationSettings.Instance;
            var testSettings = UnityEngine.Object.Instantiate(originalSettings);
            testSettings.hideFlags = HideFlags.HideAndDontSave;
            LocalizationSettings.Instance = testSettings;
            try
            {
                var assets = AssetDatabase.LoadAssetAtPath<LocalizationAssets>("Assets/SO/Localization/LocalizationAssets.asset");
                var service = new UnityLocalizationService(assets, "ko", () => "Unavailable");
                yield return Wait(service.InitializeAsync(CancellationToken.None));
                var database = LocalizationSettings.StringDatabase; var previousProvider = database.TableProvider;
                var locale = Locale.CreateLocale("fr"); var provider = new Provider(assets);
                LocalizationSettings.AvailableLocales.AddLocale(locale); database.TableProvider = provider;
                // Recreate after registration: production support lists are built during initialization.
                service.Dispose(); service = new UnityLocalizationService(assets, "ko", () => "Unavailable");
                try
                {
                    yield return Wait(service.InitializeAsync(CancellationToken.None));
                    Assert.That(service.Languages.Any(l => l.Code == "fr"), Is.True);
                    provider.FailNext = true;
                    var failed = service.ChangeLanguageAsync("fr", CancellationToken.None);
                    yield return UntilDone(failed);
                    Assert.That(failed.IsFaulted, Is.True); Assert.That(service.CurrentLanguage, Is.EqualTo("ko"));
                    provider.HoldNext = true;
                    var stale = service.ChangeLanguageAsync("fr", CancellationToken.None);
                    yield return WaitForRelease(provider);
                    var latest = service.ChangeLanguageAsync("en", CancellationToken.None);
                    provider.Release(); yield return Wait(latest); yield return Wait(stale);
                    Assert.That(service.CurrentLanguage, Is.EqualTo("en"), "Late French preparation must not override the last request.");
                    yield return Wait(service.ChangeLanguageAsync("fr", CancellationToken.None));
                    using (var texts = new TextManager(service))
                    {
                        Assert.That(texts.Format(new LocalizedMessage("Title", "new_game")), Is.EqualTo("Nouvelle partie"));
                        Assert.That(texts.Format(new LocalizedMessage("Title", "heading")), Does.Contain("연구소"), "Missing third-locale entry falls back to Korean.");
                    }
                    yield return Wait(service.ChangeLanguageAsync("ko", CancellationToken.None));
                    service.Dispose();
                    foreach (var table in assets.tables) database.ReleaseTable(table.TableCollectionName, locale);
                    service = new UnityLocalizationService(assets, "ko", () => "Unavailable");
                    yield return Wait(service.InitializeAsync(CancellationToken.None));
                    provider.HoldNext = true; var changes = 0; service.LanguageChanged += () => changes++;
                    var abandoned = service.ChangeLanguageAsync("fr", CancellationToken.None);
                    yield return WaitForRelease(provider); service.Dispose(); provider.Release(); yield return UntilDone(abandoned);
                    Assert.That(abandoned.IsFaulted || abandoned.IsCanceled, Is.True);
                    Assert.That(changes, Is.Zero, "Disposed owner must not publish a late language change.");
                }
                finally
                {
                    provider.Release?.Invoke(); service.Dispose();
                    foreach (var table in assets.tables) database.ReleaseTable(table.TableCollectionName, locale);
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales.Single(l => l.Identifier.Code == "ko");
                    database.TableProvider = previousProvider; LocalizationSettings.AvailableLocales.RemoveLocale(locale);
                    foreach (var table in provider.Created) UnityEngine.Object.Destroy(table);
                    UnityEngine.Object.Destroy(locale);
                }
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                testSettings.ResetState();
                LocalizationSettings.Instance = originalSettings;
                UnityEngine.Object.Destroy(testSettings);
            }
        }
        private static IEnumerator WaitForRelease(Provider provider)
        {
            var until = Time.realtimeSinceStartup + 15;
            while (provider.Release == null && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(provider.Release, Is.Not.Null, "The failed cached table was not retried.");
        }
        private static IEnumerator UntilDone(Task task)
        {
            var until = Time.realtimeSinceStartup + 20;
            while (!task.IsCompleted && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(task.IsCompleted, Is.True, "Localization operation timed out.");
        }
        private static IEnumerator Wait(Task task)
        {
            yield return UntilDone(task);
            if (task.IsFaulted) throw task.Exception.GetBaseException();
            Assert.That(task.IsCanceled, Is.False);
        }
    }
}
#endif
