#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        private string directory;
        private string path;
        private SceneFlowManager flow;
        private AppConfiguration configuration;
        private bool background;
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            if (SceneFlowManager.Instance != null) Object.Destroy(SceneFlowManager.Instance.gameObject);
            if (GameDataStore.Instance != null) Object.Destroy(GameDataStore.Instance.gameObject);
            yield return null;
            AppUiSettings.Reset();
            directory = Path.Combine(Path.GetTempPath(), "PLAN016-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); path = Path.Combine(directory, "session.json");
            configuration = Object.Instantiate(AppSceneAssets.EnsureConfiguration());
            flow = SceneFlowManager.GetOrCreate(configuration, path);
            flow.StartCoroutine(KeepFocused());
        }
        private IEnumerator KeepFocused()
        {
            while (flow != null)
            {
                flow.SendMessage("OnApplicationFocus", true);
                if (flow.Laboratory != null) flow.Laboratory.SendMessage("OnApplicationFocus", true);
                yield return null;
            }
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (flow != null) Object.Destroy(flow.gameObject);
            if (GameDataStore.Instance != null) Object.Destroy(GameDataStore.Instance.gameObject);
            yield return null;
            if (configuration != null) Object.Destroy(configuration);
            AppUiSettings.Reset(); Application.runInBackground = background;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        private IEnumerator Settled()
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(flow.IsTransitioning, Is.False, "Transition timeout");
            Assert.That(flow.Error, Is.Null, flow.Error);
            yield return null; yield return null;
        }
        private IEnumerator ClickLoading(string name)
        {
            yield return WaitPopupStable();
            var popup = !flow.Loading.Visible && flow.Popups?.Top != null;
            var target = popup ? flow.PopupHost.gameObject : flow.Loading.gameObject;
            var key = popup ? name == "Retry" ? "Confirm" : "Cancel" : name;
            var button = target.GetComponentsInChildren<UiButton>().Single(b => b.name == key);
            Assert.That(button.interactable, Is.True);
            button.OnSubmit(new BaseEventData(EventSystem.current));
            yield return WaitPopupStable();
        }
        private IEnumerator WaitPopupStable()
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 5;
            do { yield return null; }
            while (Time.realtimeSinceStartupAsDouble < deadline &&
                (flow.Popups?.Top?.State == PopupState.Opening || flow.Popups?.Top?.State == PopupState.Closing || !AppUiLifetime.AcceptInput));
            Assert.That(Time.realtimeSinceStartupAsDouble, Is.LessThan(deadline), "Popup/input did not settle.");
            yield return null;
        }
        [UnityTest] public IEnumerator BootstrapTitleNewContinueAndReturnUseOneCatalogAndStopOldSession()
        {
            Assert.That(flow.EnterCurrent(AppScene.Bootstrap), Is.True);
            Assert.That(flow.EnterCurrent(AppScene.Bootstrap), Is.False);
            yield return Settled();
            Assert.That(Object.FindFirstObjectByType<TitleView>().HasSave, Is.False);
            Assert.That(File.Exists(path), Is.False, "Title must not create a run.");
            var catalog = GameDataStore.Instance.Catalog;
            Assert.That(flow.StartNewGame(), Is.True); yield return Settled();
            var first = flow.Laboratory.Controller; var id = first.Current.Registry.Things.First().Id;
            flow.Laboratory.SimulationRate = 0;
            flow.Settings.TrySet(false, true);
            Assert.That(flow.RequestReturnToTitle(), Is.True);
            yield return ClickLoading("Retry"); yield return Settled();
            var saves = first.SuccessfulSaves;
            for (var i = 0; i < 5; i++) yield return null;
            Assert.That(first.SuccessfulSaves, Is.EqualTo(saves));
            Assert.That(GameDataStore.Instance.Catalog, Is.SameAs(catalog));
            Assert.That(Object.FindFirstObjectByType<TitleView>().HasSave, Is.True);
            Assert.That(flow.Continue(), Is.True); yield return Settled();
            Assert.That(flow.Laboratory.Controller.Current.Registry.TryGet(id, out _), Is.True);
            Assert.That(flow.Laboratory.LargeText, Is.True); Assert.That(flow.Laboratory.ShowFacilityStatus, Is.False);
            Assert.That(Object.FindObjectsByType<SceneFlowManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CorruptSaveAndFailedNewRunKeepBytesAndRecoveryScreen()
        {
            File.WriteAllText(path, "broken saved session");
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            LogAssert.Expect(LogType.Warning, "Scene preparation failed: The save is not valid JSON.");
            flow.Continue();
            var deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(flow.Error, Is.Not.Null); Assert.That(flow.Loading.Visible, Is.True);
            Assert.That(File.ReadAllText(path), Is.EqualTo("broken saved session"));
            yield return ClickLoading("Recover"); yield return Settled();
            Assert.That(flow.StartNewGame(), Is.True); Assert.That(flow.IsPrompting, Is.True);
            flow.CancelPrompt(); Assert.That(File.ReadAllText(path), Is.EqualTo("broken saved session"));
            flow.StartNewGame(); yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(flow.Laboratory.Controller.Current, Is.Not.Null);
            Assert.That(File.ReadAllText(path), Is.Not.EqualTo("broken saved session"));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator DirtyPreferencesSurviveSceneChangesAndRetryLatestValue()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var settingsPath = MobileGame.Persistence.UiSettingsStore.PathForGameSave(path);
            Directory.CreateDirectory(settingsPath); // force settings write failure, without affecting game save
            Assert.That(flow.Settings.TrySet(false, true), Is.False);
            var settings = flow.Settings;
            flow.StartNewGame(); yield return Settled();
            Assert.That(flow.Settings, Is.SameAs(settings)); Assert.That(flow.Laboratory.LargeText, Is.True);
            Assert.That(flow.Settings.HasUnsavedChanges, Is.True);
            Assert.That(flow.Laboratory.Restart(), Is.True);
            flow.RequestReturnToTitle(); yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(flow.Settings.TrySet(true, true), Is.False);
            Directory.Delete(settingsPath);
            Assert.That(flow.Settings.TrySave(), Is.True);
            Assert.That(flow.Settings.HasUnsavedChanges, Is.False);
            var check = new MobileGame.Persistence.UiSettingsStore(settingsPath); Assert.That(check.Load(), Is.True);
            Assert.That(check.Current.ShowFacilityStatus, Is.True); Assert.That(check.Current.LargeText, Is.True);
            Assert.That(AppUiSettings.ForSlot(Path.Combine(directory, "other.json")).HasUnsavedChanges, Is.False);
            Assert.That(AppUiSettings.ForSlot(Path.Combine(directory, "other.json")).Current.LargeText, Is.False);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator DataFailureBeforeAnySceneCanRetryAndDoesNotCreateSave()
        {
            var gate = new TaskCompletionSource<GameDataCatalog>();
            var failed = GameDataStore.GetOrCreate().InitializeAsync(token => gate.Task);
            flow.EnterCurrent(AppScene.Bootstrap);
            yield return null; yield return null; yield return null;
            LogAssert.Expect(LogType.Warning, "Scene preparation failed: test data failure");
            gate.SetException(new IOException("test data failure"));
            var deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(flow.Error, Does.Contain("test data failure")); Assert.That(flow.Loading.Visible, Is.True);
            Assert.That(File.Exists(path), Is.False); Assert.That(failed.IsFaulted, Is.True);
            yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(Object.FindFirstObjectByType<TitleView>(), Is.Not.Null);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ReturnSaveFailurePreservesSessionAndCanCancel()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            flow.StartNewGame(); yield return Settled();
            var session = flow.Laboratory.Controller.Current;
            var bytes = File.ReadAllText(path); File.Delete(path); Directory.CreateDirectory(path);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("^Save before title failed: .+"));
            flow.RequestReturnToTitle(); yield return ClickLoading("Retry");
            Assert.That(flow.IsPrompting, Is.True); Assert.That(flow.Error, Is.Not.Null);
            Assert.That(flow.Laboratory.Controller.Current, Is.SameAs(session));
            flow.CancelPrompt(); Assert.That(flow.Laboratory.CanInteract, Is.True);
            Directory.Delete(path); File.WriteAllText(path, bytes);
            flow.RequestReturnToTitle(); yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(Object.FindFirstObjectByType<TitleView>(), Is.Not.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator DestinationPreparationFailureKeepsSharedRecoveryAfterSceneReplacement()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            configuration.laboratoryScene = AppSceneAssets.BootstrapPath; // loadable scene, deliberately missing Laboratory entry
            LogAssert.Expect(LogType.Warning, "Scene preparation failed: Laboratory view is missing.");
            flow.StartNewGame();
            var deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(flow.Error, Does.Contain("Laboratory view is missing"));
            Assert.That(Object.FindFirstObjectByType<TitleView>(), Is.Null, "Previous scene was actually unloaded.");
            Assert.That(flow.Loading.Visible, Is.True);
            Assert.That(flow.Loading.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            configuration.laboratoryScene = LaboratorySceneAssets.ScenePath;
            yield return ClickLoading("Retry"); yield return Settled();
            Assert.That(flow.Laboratory.Controller.Current, Is.Not.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator TitleConfirmationAndLoadingFitBothOrientationsWithLargeText()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled(); flow.Settings.TrySet(true, true);
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
            using (new LaboratoryGameViewSize(size.x, size.y))
            {
                for (var i = 0; i < 5; i++) yield return null;
                Canvas.ForceUpdateCanvases();
                var title = Object.FindFirstObjectByType<TitleView>();
                Assert.That(title.GetComponentsInChildren<UiButton>().Where(b => b.interactable).All(b =>
                    ((RectTransform)b.transform).rect.height >= 48), Is.True);
                yield return Capture("title-" + size.x);
                yield return ClickTitle("Settings");
                yield return WaitPopupStable();
                yield return Capture("settings-" + size.x);
                yield return ClickLoading("Recover");
                File.WriteAllText(path, "confirmation sample");
                flow.StartNewGame();
                yield return WaitPopupStable();
                yield return Capture("confirmation-" + size.x);
                flow.CancelPrompt(); File.Delete(path);
                flow.Loading.Show(LoadingScreenKind.Loading, "게임 데이터를 준비하고 있습니다…");
                for (var i = 0; i < 3; i++) yield return null;
                yield return Capture("loading-" + size.x);
                flow.Loading.Show(LoadingScreenKind.Error, "화면을 준비하지 못했습니다. 마지막으로 성공한 저장은 유지됩니다.", () => { }, () => { });
                for (var i = 0; i < 4; i++) yield return null;
                var buttons = flow.Loading.GetComponentsInChildren<UiButton>();
                Assert.That(buttons.Length, Is.EqualTo(2));
                var viewport = flow.Loading.GetComponentInChildren<ScrollRect>().viewport;
                foreach (var button in buttons)
                    Assert.That(RectTransformUtility.RectangleContainsScreenPoint(viewport,
                        RectTransformUtility.WorldToScreenPoint(null, button.transform.position)), Is.True);
                yield return Capture("loading-error-" + size.x);
                flow.Loading.Hide();
            }
            LogAssert.NoUnexpectedReceived();
        }
        private static IEnumerator ClickTitle(string name)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 5;
            UiButton button = null;
            while (Time.realtimeSinceStartupAsDouble < deadline)
            {
                button = Object.FindFirstObjectByType<TitleView>().GetComponentsInChildren<UiButton>()
                    .SingleOrDefault(b => b.name == name && b.interactable);
                if (button != null && AppUiLifetime.AcceptInput) break;
                yield return null;
            }
            Assert.That(button, Is.Not.Null, "Title action did not become usable: " + name);
            Assert.That(AppUiLifetime.AcceptInput, Is.True);
            button.OnSubmit(new BaseEventData(EventSystem.current));
            yield return null;
        }
        internal static IEnumerator Capture(string name, string destination = null)
        {
            var target = destination ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/AppSceneFlowScreens/" + name + ".png"));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            // ScreenCapture uses the host window backbuffer in this Editor. Read GameView's
            // render target instead to preserve the chosen portrait/landscape aspect ratio.
            var type = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.PlayModeView");
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            var window = type.GetMethod("GetMainPlayModeView", flags | System.Reflection.BindingFlags.Static).Invoke(null, null);
            // Focus this exact view: the menu may select another Game window with a different size.
            ((UnityEditor.EditorWindow)window).Focus();
            yield return null;
            yield return new WaitForEndOfFrame();
            var targetTexture = (RenderTexture)type.GetField("m_TargetTexture", flags | System.Reflection.BindingFlags.Instance).GetValue(window);
            Assert.That(targetTexture, Is.Not.Null);
            Assert.That(targetTexture.width, Is.EqualTo(Screen.width));
            Assert.That(targetTexture.height, Is.EqualTo(Screen.height));
            var previous = RenderTexture.active;
            var upright = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 0, RenderTextureFormat.ARGB32);
            if (SystemInfo.graphicsUVStartsAtTop) Graphics.Blit(targetTexture, upright, new Vector2(1, -1), new Vector2(0, 1));
            else Graphics.Blit(targetTexture, upright);
            RenderTexture.active = upright;
            var texture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0); texture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(upright);
            try { File.WriteAllBytes(target, texture.EncodeToPNG()); }
            finally { Object.Destroy(texture); }
        }
    }
}
#endif
