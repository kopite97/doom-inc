#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        internal static void AssertSharedProductFont(Component root, TMP_FontAsset font)
        {
            Assert.That(root.GetComponentsInChildren<UnityEngine.UI.Text>(true), Is.Empty, root.name);
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.That(labels, Is.Not.Empty, root.name);
            foreach (var label in labels)
            {
                Assert.That(label.font, Is.SameAs(font), root.name + "/" + label.name);
                Assert.That(label.richText, Is.False, label.name);
            }
        }

        [UnityTest] public IEnumerator ColdLoadingCopySurvivesLocalizationInitializationFailure()
        {
            var original = LocalizationSettings.Instance;
            var isolated = UnityEngine.Object.Instantiate(original);
            isolated.hideFlags = HideFlags.HideAndDontSave;
            LocalizationSettings.Instance = isolated;
            try
            {
                var package = LocalizationSettings.InitializationOperation;
                yield return package;
                Assert.That(package.Status, Is.EqualTo(AsyncOperationStatus.Succeeded));
                var locales = LocalizationSettings.AvailableLocales;
                var missing = LocalizationSettings.SelectedLocale.Identifier.Code == "ko" ? "en" : "ko";
                Assert.That(locales.RemoveLocale(locales.Locales.Single(l => l.Identifier.Code == missing)), Is.True);

                Assert.That(flow.Localization.Texts.IsReady, Is.False);
                const string failure = "Korean and English locales are required.";
                LogAssert.Expect(LogType.Warning, "Scene preparation failed: " + failure);
                Assert.That(flow.EnterCurrent(AppScene.Bootstrap), Is.True);
                var overlay = flow.Loading;
                var wave = overlay.GetComponentInChildren<LoadingTextWave>();
                var title = wave.GetComponent<TextMeshProUGUI>();
                var track = overlay.GetComponentsInChildren<RectTransform>(true).Single(r => r.name == "SceneProgress");
                var percent = overlay.GetComponentsInChildren<TextMeshProUGUI>(true).Single(t => t.name == "ScenePercent");
                Assert.That(overlay.Visible && wave.isActiveAndEnabled, Is.True);
                Assert.That(title.text, Is.EqualTo("Loading..."), "The startup label must resolve from the embedded Bootstrap copy.");
                Assert.That(track.gameObject.activeInHierarchy || percent.isActiveAndEnabled, Is.False);

                var deadline = Time.realtimeSinceStartupAsDouble + 15;
                while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Assert.That(flow.Error, Is.EqualTo(failure));
                Assert.That(flow.Localization.Texts.IsReady, Is.False);
                Assert.That(overlay.Kind, Is.EqualTo(LoadingScreenKind.Error));
                Assert.That(overlay.Visible, Is.True);
                var retry = overlay.GetComponentsInChildren<UiButton>().Single(b => b.name == "Retry");
                var fallback = new BootstrapText(configuration.localization);
                var language = flow.Localization.Preferences.Language;
                Assert.That(title.text, Is.EqualTo(fallback.Get("error_title", language)));
                Assert.That(retry.GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo(fallback.Get("retry", language)));
                Assert.That(wave.isActiveAndEnabled || track.gameObject.activeInHierarchy || percent.isActiveAndEnabled, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                isolated.ResetState();
                LocalizationSettings.Instance = original;
                UnityEngine.Object.Destroy(isolated);
            }
        }

        [UnityTest] public IEnumerator ColdMulmaruAtlasRendersKoreanLatinAndSymbolsWithoutFallback()
        {
            var shared = configuration.uiAssets.Font;
            Assert.That(shared.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic));
            Assert.That(shared.sourceFontFile.name, Is.EqualTo("Mulmaru"));
            // Use the same source and settings without clearing the shared project's live atlas.
            var cold = TMP_FontAsset.CreateFontAsset(shared.sourceFontFile, Mathf.RoundToInt(shared.faceInfo.pointSize),
                shared.atlasPadding, shared.atlasRenderMode, shared.atlasWidth, shared.atlasHeight,
                AtlasPopulationMode.Dynamic, shared.isMultiAtlasTexturesEnabled);
            try
            {
                Assert.That(cold.HasCharacter('준'), Is.False);
                using (var canvas = new UiCanvas(flow.transform, "Temp_ColdFont", 10))
                {
                    var label = UiWidgets.Label("Glyphs", canvas.SafeRoot, 24);
                    label.font = cold;
                    UiWidgets.Place(label.rectTransform, 0, 0, 500, 100);
                    const string sample = "Loading...●준비중…-+0123456789%";
                    label.text = sample;
                    yield return null;
                    label.ForceMeshUpdate();
                    Assert.That(label.textInfo.characterCount, Is.EqualTo(sample.Length));
                    for (var i = 0; i < sample.Length; i++)
                    {
                        var letter = label.textInfo.characterInfo[i];
                        Assert.That(letter.fontAsset, Is.SameAs(cold), sample[i].ToString());
                        Assert.That(letter.textElement.unicode, Is.EqualTo((uint)sample[i]), sample[i].ToString());
                        Assert.That(letter.isVisible, Is.True, sample[i].ToString());
                    }
                }
                var copy = MobileGame.Foundation.Editor.LocalizationCsvTools.ReadAll()
                    .SelectMany(doc => doc.Rows).SelectMany(row => row.Values.Values)
                    .Append(configuration.uiAssets.FontLicense.text);
                var required = string.Concat(copy.SelectMany(value => value).Where(c => !char.IsWhiteSpace(c)).Distinct());
                var allAdded = cold.TryAddCharacters(required, out var missing);
                Assert.That(allAdded, Is.True, "Missing Mulmaru glyphs in localized copy or license: " + missing);
            }
            finally { UnityEngine.Object.Destroy(cold); }
        }

        [UnityTest] public IEnumerator LanguageChangeKeepsProductionReportsEndingAndRestartState()
        {
            var preferences = Path.Combine(Application.temporaryCachePath, "Preferences", "language.json");
            var original = File.Exists(preferences) ? File.ReadAllBytes(preferences) : null;
            try
            {
                MobileGame.Foundation.Editor.AppTestScenario.Prepare(MobileGame.Foundation.Editor.GameDataAssets.LoadCatalog(), path, true);
                flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
                var change = flow.Localization.SelectLanguageAsync("ko");
                while (!change.IsCompleted) yield return null;
                change.GetAwaiter().GetResult();
                flow.Continue(); yield return Settled();
                var view = flow.Laboratory; view.SimulationRate = 0;
                var session = view.Controller.Current;
                var codec = new MobileGame.Persistence.SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                var before = JsonUtility.ToJson(codec.Capture(session, view.Controller.Phase));
                var report = view.Invasion.Reports.Last();
                var korean = view.FormatMessage(view.ReportMessage(report));
                view.OpenPanel(LaboratoryPanel.Invasion);
                change = flow.Localization.SelectLanguageAsync("en");
                while (!change.IsCompleted) yield return null;
                change.GetAwaiter().GetResult(); yield return null;
                Assert.That(JsonUtility.ToJson(codec.Capture(session, view.Controller.Phase)), Is.EqualTo(before), "Language changes cannot advance production, cultivation, or reports.");
                Assert.That(view.FormatMessage(view.ReportMessage(report)), Is.Not.EqualTo(korean));
                Assert.That(view.Invasion.Reports.Last(), Is.SameAs(report));
                for (var i = 0; i < 1000 && view.Controller.Phase == MobileGame.Foundation.SessionPhase.Playing; i++) view.AdvanceFrame(1, 0);
                Assert.That(view.Controller.Phase, Is.EqualTo(MobileGame.Foundation.SessionPhase.Destruction));
                view.Controller.NotifyApplicationPause(true);
                var request = view.Ending.Request; var elapsed = view.Ending.Elapsed;
                change = flow.Localization.SelectLanguageAsync("ko");
                while (!change.IsCompleted) yield return null;
                change.GetAwaiter().GetResult(); yield return null;
                Assert.That(view.Ending.Request, Is.SameAs(request)); Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed));
                view.Controller.NotifyApplicationPause(false);
                for (var i = 0; i < 100 && view.Controller.Phase != MobileGame.Foundation.SessionPhase.Results; i++) view.AdvanceFrame(0, .1);
                Assert.That(view.Controller.Phase, Is.EqualTo(MobileGame.Foundation.SessionPhase.Results));
                request = view.Ending.Request; var generation = view.Controller.Generation;
                change = flow.Localization.SelectLanguageAsync("en");
                while (!change.IsCompleted) yield return null;
                change.GetAwaiter().GetResult(); yield return null;
                Assert.That(view.Ending.Request, Is.SameAs(request)); Assert.That(view.Controller.Generation, Is.EqualTo(generation));
                Assert.That(view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "EndingTitle").text, Is.EqualTo(flow.Localization.Format("Results", "ended")));
                Assert.That(view.Restart(), Is.True);
                Assert.That(flow.Localization.Service.CurrentLanguage, Is.EqualTo("en"));
                Assert.That(view.Controller.Generation, Is.EqualTo(generation + 1));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (original == null) { if (File.Exists(preferences)) File.Delete(preferences); }
                else File.WriteAllBytes(preferences, original);
            }
        }
        [UnityTest] public IEnumerator RepeatedVisibleLoadingRequestsInvalidatePressEvenWithIdenticalText()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var oldCalls = 0; var newCalls = 0;
            flow.Loading.Show(LoadingScreenKind.Confirmation, "same message", () => oldCalls++, null, "custom label");
            for (var i = 0; i < 4; i++) yield return null;
            var button = flow.Loading.GetComponentsInChildren<UiButton>().Single(b => b.name == "Retry");
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            var press = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = 17 };
            button.OnPointerDown(press);
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++, null, "새 게임 시작");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            Assert.That(flow.Loading.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "LoadingTitle").text,
                Is.EqualTo("준비 중 문제가 발생했습니다"), "A button caption cannot choose the screen kind.");
            button.OnPointerClick(press);
            Assert.That(oldCalls + newCalls, Is.Zero);
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerDown(press);
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++, null, "새 게임 시작");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerClick(press);
            Assert.That(oldCalls + newCalls, Is.Zero, "Even an identical error is a new input lifetime.");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerDown(press); button.OnPointerClick(press); button.OnPointerClick(press);
            Assert.That(newCalls, Is.EqualTo(1)); Assert.That(oldCalls, Is.Zero);
            flow.Loading.Show(LoadingScreenKind.Loading, "same message");
            yield return null;
            Assert.That(flow.Loading.GetComponentsInChildren<UiButton>(), Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => flow.Loading.Show((LoadingScreenKind)99, "invalid"));
            Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Loading));
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++);
            yield return ClickLoading("Retry");
            Assert.That(newCalls, Is.EqualTo(2));
            flow.Loading.Hide(); LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator SharedTextStyleKeepsBaseSizeForReusedNewAndHiddenRowsWithoutAView()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            using (var canvas = new UiCanvas(flow.transform, "Temp_SharedStyleTest", 20))
            {
                var content = UiWidgets.Rect("Rows", canvas.SafeRoot);
                var rows = new UiList(content, () => canvas.LayoutEpoch);
                var staticLabel = UiWidgets.Label("Static", canvas.SafeRoot, 24);
                var style = staticLabel.GetComponent<UiText>();
                foreach (var scale in new[] { 1f, 1.25f, 1.25f, 1f, 1.25f, 1f })
                {
                    style.Apply(null, scale);
                    rows.Style(null, scale, 64);
                    rows.Begin(400); rows.Text("Existing", "Test", 20); rows.Action("Action", "Action", () => { }, true, "test"); rows.End();
                    canvas.SetVisible(false); canvas.SetVisible(true);
                    yield return null;
                    Assert.That(staticLabel.fontSize, Is.EqualTo(Mathf.RoundToInt(24 * scale)));
                    Assert.That(staticLabel.font, Is.Not.Null);
                    Assert.That(content.Find("Existing").GetComponent<TextMeshProUGUI>().fontSize, Is.EqualTo(Mathf.RoundToInt(20 * scale)));
                    Assert.That(((RectTransform)content.Find("Action")).rect.height, Is.GreaterThanOrEqualTo(64));
                }
                rows.Style(configuration.uiAssets.Font, 1.25f, 70);
                rows.Begin(400); rows.Text("Existing", "Existing", 20); rows.Text("New", "New", 20); rows.End();
                yield return null;
                foreach (var text in content.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    Assert.That(text.fontSize, Is.EqualTo(25));
                    Assert.That(text.font, Is.EqualTo(configuration.uiAssets.Font));
                }
            }
            flow.Loading.Configure(null, () => true);
            flow.Loading.Show(LoadingScreenKind.Error, "Fallback text", () => { });
            yield return null;
            Assert.That(flow.Loading.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "LoadingTitle").fontSize, Is.EqualTo(30));
            Assert.That(flow.Loading.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "Status").fontSize, Is.EqualTo(Mathf.RoundToInt(18 * 1.25f)));
            flow.Loading.Hide(); LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator UnreadableSaveRemainsAnErrorAndCannotSilentlyStartANewRun()
        {
            Directory.CreateDirectory(path);
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var title = UnityEngine.Object.FindFirstObjectByType<TitleView>();
            Assert.That(title.HasSave, Is.True);
            Assert.That(title.SaveError, Is.Not.Null);
            LogAssert.Expect(LogType.Warning, "Scene preparation failed: " + title.SaveError);
            Assert.That(flow.StartNewGame(), Is.False);
            Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Error));
            Assert.That(Directory.Exists(path), Is.True);
            Directory.Delete(path); title.RefreshSave();
            Assert.That(title.HasSave, Is.False); Assert.That(title.SaveError, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
