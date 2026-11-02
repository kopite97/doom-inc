#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest] public IEnumerator PopupLongBodyKeepsScrollAndCapturedActionAcrossLanguageAndRotation()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var preferencePath = Path.Combine(Application.temporaryCachePath, "Preferences", "language.json");
            var originalBytes = File.Exists(preferencePath) ? File.ReadAllBytes(preferencePath) : null;
            var snapshot = string.Join("\n", Enumerable.Range(1, 48).Select(i => "Captured detail " + i));
            var message = new LocalizedMessage("Common", "status_setting", new TextArgument("value", snapshot));
            var calls = 0;
            var handle = flow.Popups.ShowConfirm(flow.ScenePopupScope, "long-body", new LocalizedMessage("Loading", "confirmation"), message, () => calls++);
            yield return WaitPopupStable();
            try
            {
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return WaitPopupStable();
                    var popup = flow.PopupHost.GetComponentInChildren<ConfirmPopup>();
                    Canvas.ForceUpdateCanvases();
                    Assert.That(popup.Scroll.content.rect.height, Is.GreaterThan(popup.Scroll.viewport.rect.height));
                    popup.Scroll.verticalNormalizedPosition = .5f;
                    var button = popup.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm");
                    var pointer = new PointerEventData(EventSystem.current) { pointerId = 901 };
                    button.OnPointerDown(pointer);
                    var change = flow.Localization.SelectLanguageAsync(flow.Localization.Service.CurrentLanguage == "ko" ? "en" : "ko");
                    while (!change.IsCompleted) yield return null;
                    change.GetAwaiter().GetResult(); yield return WaitPopupStable();
                    button.OnPointerClick(pointer);
                    Assert.That(calls, Is.Zero); Assert.That(handle.State, Is.EqualTo(PopupState.Open));
                    Assert.That(popup.Scroll.content.anchoredPosition.y, Is.GreaterThan(1), "Language change must not reset the reading position.");
                    Assert.That(popup.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t => t.name == "Message").text, Does.EndWith(snapshot));
                    var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                    Assert.That(corners.All(p => p.x >= 0 && p.x <= Screen.width && p.y >= 0 && p.y <= Screen.height));
                }
                yield return WaitPopupStable();
                var confirm = flow.PopupHost.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm");
                var deadline = Time.realtimeSinceStartupAsDouble + 5;
                while ((!confirm.IsInteractable() || !AppUiLifetime.AcceptInput || !flow.Popups.Foreground) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Assert.That(confirm.IsInteractable() && AppUiLifetime.AcceptInput && flow.Popups.Foreground, Is.True, "Restored Game View input must settle.");
                confirm.OnSubmit(new BaseEventData(EventSystem.current));
                Assert.That(handle.State, Is.EqualTo(PopupState.Closing));
                yield return WaitPopupStable(); Assert.That(calls, Is.EqualTo(1)); Assert.That(handle.Result, Is.EqualTo(PopupResult.Confirmed));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (originalBytes != null) File.WriteAllBytes(preferencePath, originalBytes);
                else if (File.Exists(preferencePath)) File.Delete(preferencePath);
            }
        }
        [UnityTest] public IEnumerator PopupSceneExtensionAndFailedRegistrationPreserveUsableRegistry()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var root = new GameObject("Test popup template", typeof(RectTransform), typeof(CanvasGroup));
            root.SetActive(false); root.AddComponent<PopupExtensionFixture>().CreateTemplate();
            var resource = ScriptableObject.CreateInstance<PopupResource>(); resource.Configure("TestExtension", root);
            var scene = ScriptableObject.CreateInstance<PopupRegistry>(); scene.Configure(new[] { resource });
            var invalid = ScriptableObject.CreateInstance<PopupRegistry>(); invalid.Configure(new[] { configuration.popups.Entries[0] });
            try
            {
                flow.PopupHost.RegisterScene(scene);
                Assert.Throws<InvalidOperationException>(() => flow.PopupHost.RegisterScene(invalid));
                var calls = 0; var handle = flow.Popups.Show(new PopupExtensionRequest(flow.ScenePopupScope), () => calls++);
                yield return WaitPopupStable();
                Assert.That(flow.PopupHost.GetComponentsInChildren<PopupExtensionFixture>().Length, Is.EqualTo(1));
                flow.PopupHost.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm").OnSubmit(new BaseEventData(EventSystem.current));
                yield return WaitPopupStable(); Assert.That(calls, Is.EqualTo(1)); Assert.That(handle.Result, Is.EqualTo(PopupResult.Confirmed));
                flow.PopupHost.RegisterScene(null);
                LogAssert.Expect(LogType.Warning, "Popup operation failed: Unknown popup: TestExtension");
                handle = flow.Popups.Show(new PopupExtensionRequest(flow.ScenePopupScope));
                Assert.That(handle.Result, Is.EqualTo(PopupResult.Failed)); Assert.That(flow.Popups.BlocksInput, Is.False);
                var common = flow.Popups.ShowConfirm(flow.ScenePopupScope, "still-valid", new LocalizedMessage("Common", "confirm"),
                    new LocalizedMessage("Laboratory", "map_help"), null);
                yield return WaitPopupStable(); Assert.That(common.State, Is.EqualTo(PopupState.Open)); common.Cancel();
                yield return null; Assert.That(flow.PopupHost.GetComponentsInChildren<PopupBase>(true), Is.Empty);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                flow.PopupHost.RegisterScene(null);
                UnityEngine.Object.Destroy(root); UnityEngine.Object.Destroy(resource);
                UnityEngine.Object.Destroy(scene); UnityEngine.Object.Destroy(invalid);
            }
        }
        [UnityTest] public IEnumerator PopupAcceptedConfirmationWaitsForResumeAndRunsOnce()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            File.WriteAllText(path, "old save must survive until confirm handoff");
            flow.StartNewGame(); yield return WaitPopupStable();
            var handle = flow.Popups.Top;
            flow.PopupHost.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm").OnSubmit(new BaseEventData(EventSystem.current));
            flow.SendMessage("OnApplicationPause", true);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(handle.State, Is.EqualTo(PopupState.WaitingForForeground));
            Assert.That(flow.Popups.BlocksInput, Is.True); Assert.That(flow.IsTransitioning, Is.False);
            Assert.That(File.ReadAllText(path), Does.StartWith("old save"));
            flow.SendMessage("OnApplicationPause", false); yield return Settled();
            Assert.That(handle.Result, Is.EqualTo(PopupResult.Confirmed));
            var session = flow.Laboratory.Controller.Current;
            flow.SendMessage("OnApplicationPause", false); yield return null;
            Assert.That(flow.Laboratory.Controller.Current, Is.SameAs(session));
            Assert.That(flow.Popups.Count, Is.Zero); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ClosingSettingsDoesNotCancelAppOwnedLanguageChange()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var preferencePath = Path.Combine(Application.temporaryCachePath, "Preferences", "language.json");
            var originalBytes = File.Exists(preferencePath) ? File.ReadAllBytes(preferencePath) : null;
            try
            {
                var scope = new PopupScope(flow.ScenePopupScope);
                var handle = flow.Popups.Show(new SettingsPopupRequest(scope, flow.Settings)); yield return WaitPopupStable();
                var target = flow.Localization.Service.CurrentLanguage == "ko" ? "en" : "ko";
                var change = flow.Localization.SelectLanguageAsync(target);
                scope.Dispose();
                while (!change.IsCompleted) yield return null;
                change.GetAwaiter().GetResult(); yield return null;
                Assert.That(handle.Result, Is.EqualTo(PopupResult.OwnerEnded));
                Assert.That(flow.Localization.Service.CurrentLanguage, Is.EqualTo(target));
                Assert.That(flow.Localization.Preferences.Language, Is.EqualTo(target));
                Assert.That(flow.Popups.Count, Is.Zero); Assert.That(flow.Popups.BlocksInput, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (originalBytes != null) File.WriteAllBytes(preferencePath, originalBytes);
                else if (File.Exists(preferencePath)) File.Delete(preferencePath);
            }
        }
        [UnityTest] public IEnumerator PopupAcceptedIntentIsInvalidatedWhenOwnerEndsInBackground()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var owner = new PopupScope(); var calls = 0;
            var handle = flow.Popups.ShowConfirm(owner, "intent", new LocalizedMessage("Common", "confirm"),
                new LocalizedMessage("Loading", "return_prompt"), () => calls++);
            yield return WaitPopupStable(); flow.Popups.Choose(handle, PopupResult.Confirmed);
            flow.SendMessage("OnApplicationPause", true); yield return new WaitForSecondsRealtime(.3f);
            owner.Dispose(); flow.SendMessage("OnApplicationPause", false); yield return null;
            Assert.That(calls, Is.Zero); Assert.That(handle.Result, Is.EqualTo(PopupResult.OwnerEnded));
            Assert.That(flow.Popups.BlocksInput, Is.False); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator PopupSettingsAndChildCloseOnceAtDestructionButResultsPromptRemains()
        {
            AppTestScenario.Prepare(GameDataAssets.LoadCatalog(), path, true);
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled(); flow.Continue(); yield return Settled();
            var view = flow.Laboratory; view.SimulationRate = 0;
            var settings = view.ShowSettings(); yield return WaitPopupStable();
            flow.Settings.TrySet(false, true);
            var cancelCalls = 0;
            var child = flow.Popups.Show(new DialogPopupRequest(settings.Children, "child", new LocalizedMessage("Common", "settings"),
                new LocalizedMessage("Laboratory", "map_help"), new LocalizedMessage("Common", "back"), parent: settings), onCancel: () => cancelCalls++);
            yield return WaitPopupStable();
            for (var i = 0; i < 1000 && view.Controller.Phase == SessionPhase.Playing; i++) view.AdvanceFrame(1, 0);
            Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(settings.Result, Is.EqualTo(PopupResult.OwnerEnded)); Assert.That(child.Result, Is.EqualTo(PopupResult.OwnerEnded));
            Assert.That(cancelCalls, Is.Zero); Assert.That(flow.Settings.Current.LargeText, Is.True);
            for (var i = 0; i < 100 && view.Controller.Phase != SessionPhase.Results; i++) view.AdvanceFrame(0, .1);
            Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(flow.RequestReturnToTitle(), Is.True); yield return WaitPopupStable();
            var prompt = flow.Popups.Top;
            for (var i = 0; i < 10; i++) { view.AdvanceFrame(0, .1); yield return null; }
            Assert.That(prompt.IsCompleted, Is.False); flow.CancelPrompt();
            Assert.That(flow.Popups.BlocksInput, Is.False); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator PopupRegistryFailureUsesIndependentLoadingRecovery()
        {
            var registry = configuration.popups; configuration.popups = null;
            LogAssert.Expect(LogType.Warning, "Scene preparation failed: Missing common popup registry.");
            flow.EnterCurrent(AppScene.Bootstrap);
            while (flow.IsTransitioning) yield return null;
            Assert.That(flow.Error, Does.Contain("popup registry")); Assert.That(flow.Loading.Visible);
            Assert.That(flow.Popups, Is.Null); Assert.That(File.Exists(path), Is.False);
            configuration.popups = registry;
            yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(flow.Popups.Ready); Assert.That(flow.PopupHost, Is.Not.Null); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator PopupLocalizationRotationAndSettingsPreserveTheOpenStack()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var preferencePath = Path.Combine(Application.temporaryCachePath, "Preferences", "language.json");
            var originalBytes = File.Exists(preferencePath) ? File.ReadAllBytes(preferencePath) : null;
            var settings = flow.Popups.Show(new SettingsPopupRequest(flow.ScenePopupScope, flow.Settings));
            yield return WaitPopupStable();
            flow.Settings.TrySet(false, true);
            var child = flow.Popups.Show(new DialogPopupRequest(settings.Children, "reading", new LocalizedMessage("Loading", "confirmation"),
                new LocalizedMessage("Laboratory", "map_help"), new LocalizedMessage("Common", "back"), parent: settings));
            yield return WaitPopupStable();
            try
            {
                foreach (var language in new[] { "ko", "en" })
                {
                    var change = flow.Localization.SelectLanguageAsync(language); while (!change.IsCompleted) yield return null;
                    change.GetAwaiter().GetResult();
                    foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        for (var i = 0; i < 6; i++) yield return null;
                        Assert.That(child.IsCompleted || settings.IsCompleted, Is.False); Assert.That(flow.Popups.Top, Is.SameAs(child));
                        var popup = flow.PopupHost.GetComponentsInChildren<PopupBase>().Last();
                        Assert.That(popup.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t => t.name == "Message").text,
                            Is.EqualTo(flow.Localization.Format("Laboratory", "map_help")));
                        var button = popup.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm");
                        var bounds = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(bounds);
                        Assert.That(bounds.All(p => p.x >= 0 && p.x <= Screen.width && p.y >= 0 && p.y <= Screen.height));
                        var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/plans/evidence/PLAN-019"));
                        yield return Capture("popup-" + language, Path.Combine(folder, "popup-" + language + "-" + size.x + "x" + size.y + ".png"));
                    }
                }
                child.Cancel(); yield return WaitPopupStable(); Assert.That(flow.Popups.Top, Is.SameAs(settings));
                settings.Cancel(); Assert.That(flow.Popups.BlocksInput, Is.False); LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (originalBytes != null) File.WriteAllBytes(preferencePath, originalBytes);
                else if (File.Exists(preferencePath)) File.Delete(preferencePath);
            }
        }
    }
}
#endif
