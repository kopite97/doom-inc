#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        private static readonly Vector2Int[] ScreenStateSizes = { new Vector2Int(1280, 720), new Vector2Int(720, 1280) };

        [UnityTest]
        public IEnumerator LoadingAndRestoreFailuresKeepRecoveryControlsReachableInBothOrientations()
        {
            var previousBackground = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN013-Errors-" + Guid.NewGuid().ToString("N"));
            GameObject owner = null; ScreenStateSource source = null;
            try
            {
                if (GameDataStore.Instance != null) UnityEngine.Object.DestroyImmediate(GameDataStore.Instance.gameObject);
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, "session.json"); const string original = "invalid saved run; preserve these bytes";
                File.WriteAllText(path, original);
                owner = new GameObject("PLAN013_LoadingFixture"); var view = owner.AddComponent<LaboratoryView>();
                GameDataAssets.Configure(owner); var bootstrap = owner.GetComponent<GameBootstrap>();
                bootstrap.SetSaveSlotBeforeStart(path, 30); view.SimulationRate = 0;
                source = new ScreenStateSource();
                var loading = bootstrap.PrepareAsync(source); var starting = bootstrap.StartSessionAsync();
                yield return null; yield return null;
                Assert.That(view.Controller, Is.Null); Assert.That(bootstrap.Ready, Is.False);
                Assert.That(owner.GetComponentsInChildren<LaboratoryUiButton>(true).Single(b => b.name == "RetryLoading").gameObject.activeInHierarchy, Is.False);
                var error = string.Join("\n", Enumerable.Range(1, 48).Select(i => "Row " + i + ": definitions could not be prepared; keep the saved run and retry."));
                source.Gate.SetException(new FormatException(error));
                yield return WaitFor(() => loading.IsCompleted && starting.IsCompleted, "The loading failure did not complete.");
                Assert.That(loading.IsFaulted, Is.True); Assert.That(loading.Exception, Is.Not.Null);
                Assert.That(bootstrap.Error, Is.EqualTo(error)); Assert.That(view.Controller, Is.Null);
                var store = GameDataStore.Instance;
                foreach (var size in ScreenStateSizes)
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(GameDataStore.Instance, Is.SameAs(store)); Assert.That(store.Catalog, Is.Null);
                    Assert.That(File.ReadAllText(path), Is.EqualTo(original));
                    var retry = owner.GetComponentsInChildren<LaboratoryUiButton>().Single(b => b.name == "RetryLoading");
                    AssertScreenStateButtonReachable(retry);
                    var scroll = owner.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "LoadingDetails");
                    Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height), "Long loading errors need scrolling.");
                    scroll.StopMovement(); scroll.verticalNormalizedPosition = 0;
                    yield return null; Canvas.ForceUpdateCanvases();
                    Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(0));
                    var hint = scroll.content.GetComponentsInChildren<Text>().Single(t => t.name == "Hint");
                    Assert.That(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport,
                        RectTransformUtility.WorldToScreenPoint(null, hint.rectTransform.TransformPoint(hint.rectTransform.rect.center))), Is.True,
                        "The error recovery explanation must be reachable at the bottom of the scroll.");
                    AssertScreenStateButtonReachable(retry);
                    yield return CaptureScreenState("loading-error-scroll", null, bootstrap.Error);
                }

                // Shared bootstrap retry path, not a substitute for the separate device-input tests.
                var retryLoad = bootstrap.StartSessionAsync();
                yield return WaitFor(() => retryLoad.IsCompleted, "The corrected local bundle did not finish loading.");
                Assert.That(bootstrap.Ready, Is.True); Assert.That(view.Controller, Is.Not.Null);
                Assert.That(view.Controller.Current, Is.Null, "An invalid save cannot silently become a new run.");
                Assert.That(view.Controller.LastError, Is.Not.Null.Or.Empty);
                var controller = view.Controller; var catalog = store.Catalog;
                view.StartCoroutine(KeepScenarioFocused(view));
                foreach (var size in ScreenStateSizes)
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(view.Controller, Is.SameAs(controller)); Assert.That(controller.Current, Is.Null);
                    Assert.That(GameDataStore.Instance, Is.SameAs(store)); Assert.That(store.Catalog, Is.SameAs(catalog));
                    Assert.That(File.ReadAllText(path), Is.EqualTo(original));
                    Assert.That(owner.GetComponentsInChildren<Canvas>().Any(c => c.name == "Temp_LoadingCanvas"), Is.False);
                    var scroll = view.UiCanvas.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "Controls");
                    foreach (var key in new[] { "Reload", "Restart" })
                    {
                        var button = view.FindUiButton(key);
                        RevealScreenStateButton(scroll, button); yield return null; Canvas.ForceUpdateCanvases();
                        AssertScreenStateButtonReachable(button);
                    }
                    Assert.That(view.FindUiButton("Save").interactable, Is.False);
                    yield return CaptureScreenState("restore-error", view, controller.LastError);
                }
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                source?.Gate.TrySetCanceled();
                if (owner != null) UnityEngine.Object.DestroyImmediate(owner);
                if (GameDataStore.Instance != null) UnityEngine.Object.DestroyImmediate(GameDataStore.Instance.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [UnityTest]
        public IEnumerator EndingAndResultsRotationPreserveClockRequestAndRunWithSaveRecoveryVisible()
        {
            var previousBackground = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN013-Ending-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                var path = Path.Combine(directory, "session.json"); var definitions = EndingCatalog();
                var codec = new SessionSnapshotCodec(definitions, true);
                var fixture = new GameFlowManager(codec, new JsonFileSessionStore(path), () => LaboratorySessionFactory.Create(
                    (ResourceData)definitions[0], (FacilityData)definitions[2], (FacilityData)definitions[3],
                    (CreatureData)definitions[6], (ResourceData)definitions[1]));
                Assert.That(fixture.StartOrContinue(), Is.True);
                // Existing ending boundary fixture; paid production is covered by the two-run regression.
                var invader = fixture.Current.Registry.Create(definitions[8]);
                Assert.That(fixture.Current.Map.Commands.Place(invader.Id, Vector2Int.one).Succeeded, Is.True);
                Assert.That(new LaboratoryCommands(fixture).ReleaseInvaders(), Is.True); fixture.Advance(1000);
                Assert.That(fixture.Phase, Is.EqualTo(SessionPhase.Destruction));
                yield return LoadIsolatedScene(path, 30, loaded => { view = loaded; view.SimulationRate = 0; });
                view.SendMessage("OnApplicationFocus", true);
                view.AdvanceFrame(0, 0); view.AdvanceFrame(0, 0.1); view.AdvanceFrame(0, 0.1);
                Assert.That(view.Ending.Elapsed, Is.GreaterThan(0));
                // Freeze the shared clock without changing visibility/request, isolating rotation from elapsed real time.
                view.Controller.NotifyApplicationPause(true);
                var elapsed = view.Ending.Elapsed; var request = view.Ending.Request; var session = view.Controller.Current;
                var store = GameDataStore.Instance; var catalog = store.Catalog;
                Assert.That(request, Is.Not.Null);
                var playCodec = new SessionSnapshotCodec(catalog.Definitions.Values, true);
                var stoppedInvasion = JsonUtility.ToJson(playCodec.Capture(session, view.Controller.Phase).invasion);
                foreach (var size in ScreenStateSizes)
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
                    Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed)); Assert.That(view.Ending.Request, Is.SameAs(request));
                    Assert.That(view.Controller.Current, Is.SameAs(session)); Assert.That(GameDataStore.Instance, Is.SameAs(store));
                    Assert.That(store.Catalog, Is.SameAs(catalog));
                    Assert.That(view.FindUiButton("RestartFromResults").gameObject.activeInHierarchy, Is.False);
                    Assert.That(view.UiCanvas.GetComponentsInChildren<Text>().Single(t => t.name == "EndingTitle").text, Does.Contain("FINAL SIGNAL"));
                    AssertWorldGraphicRenders(view);
                    yield return CaptureScreenState("destruction-rotation", view, null);
                }
                view.Controller.NotifyApplicationPause(false);
                for (var frame = 0; frame < 60 && view.Controller.Phase != SessionPhase.Results; frame++) view.AdvanceFrame(0, 0.1);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                Assert.That(view.Controller.Current, Is.SameAs(session));
                request = view.Ending.Request; elapsed = view.Ending.Elapsed;
                var resultFile = File.ReadAllText(path);
                using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Assert.That(view.Save(), Is.False); Assert.That(view.Controller.LastSaveError, Is.Not.Null.Or.Empty);
                    var saveError = view.Controller.LastSaveError;
                    foreach (var size in ScreenStateSizes)
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return SettleScreenStateSize(size);
                        Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                        Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed)); Assert.That(view.Ending.Request, Is.SameAs(request));
                        Assert.That(view.Controller.Current, Is.SameAs(session)); Assert.That(store.Catalog, Is.SameAs(catalog));
                        Assert.That(view.Controller.IsCurrentEndingRequest(request), Is.True);
                        Assert.That(JsonUtility.ToJson(playCodec.Capture(session, view.Controller.Phase).invasion), Is.EqualTo(stoppedInvasion));
                        Assert.That(view.Controller.LastSaveError, Is.EqualTo(saveError)); Assert.That(File.ReadAllText(path), Is.EqualTo(resultFile));
                        AssertScreenStateButtonReachable(view.FindUiButton("RestartFromResults"));
                        AssertScreenStateButtonReachable(view.FindUiButton("RetrySave"));
                        var errorText = view.UiCanvas.GetComponentsInChildren<Text>().Single(t => t.name == "EndingError");
                        Assert.That(errorText.text, Does.Contain(saveError));
                        AssertWorldGraphicRenders(view);
                        yield return CaptureScreenState("results-save-error", view, saveError);
                    }
                }
                Assert.That(view.Save(), Is.True); yield return null; yield return null;
                Assert.That(view.FindUiButton("RetrySave").gameObject.activeInHierarchy, Is.False);
                Assert.That(view.Controller.Current, Is.SameAs(session)); Assert.That(view.Ending.Request, Is.SameAs(request));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static void AssertWorldGraphicRenders(LaboratoryView view)
        {
            Canvas.ForceUpdateCanvases();
            var world = view.UiCanvas.GetComponentInChildren<LaboratoryUiWorld>();
            Assert.That(world, Is.Not.Null);
            var renderer = world.GetComponent<CanvasRenderer>();
            Assert.That(renderer, Is.Not.Null, "The ending artwork needs a renderer.");
            Assert.That(renderer.cull, Is.False, "The world must be inside the visible canvas.");
            Assert.That(renderer.materialCount, Is.GreaterThan(0));
            var mesh = renderer.GetMesh();
            Assert.That(mesh, Is.Not.Null);
            Assert.That(mesh.vertexCount, Is.GreaterThan(100), "The world mesh must reach its CanvasRenderer.");
        }

        private static IEnumerator SettleScreenStateSize(Vector2Int size)
        {
            yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y, "Game View did not adopt " + size);
            yield return null; yield return null; Canvas.ForceUpdateCanvases();
        }

        private static void RevealScreenStateButton(ScrollRect scroll, LaboratoryUiButton button)
        {
            Assert.That(button, Is.Not.Null); scroll.StopMovement();
            var rect = (RectTransform)button.transform;
            var top = -scroll.content.InverseTransformPoint(rect.TransformPoint(new Vector2(rect.rect.center.x, rect.rect.yMax))).y;
            var position = scroll.content.anchoredPosition;
            position.y = Mathf.Clamp(top - (scroll.viewport.rect.height - rect.rect.height) * 0.5f,
                0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height));
            scroll.content.anchoredPosition = position;
        }

        private static void AssertScreenStateButtonReachable(LaboratoryUiButton button)
        {
            Assert.That(button, Is.Not.Null); Assert.That(button.isActiveAndEnabled && button.IsInteractable(), Is.True, button.name);
            var rect = (RectTransform)button.transform; var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var safe = Screen.safeArea.width > 0 && Screen.safeArea.height > 0 ? Screen.safeArea : new Rect(0, 0, Screen.width, Screen.height);
            foreach (var corner in corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(null, corner);
                Assert.That(point.x, Is.InRange(safe.xMin - 1, safe.xMax + 1), button.name);
                Assert.That(point.y, Is.InRange(safe.yMin - 1, safe.yMax + 1), button.name);
            }
            Assert.That(rect.rect.width, Is.GreaterThanOrEqualTo(44)); Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(44));
            Assert.That(EventSystem.current, Is.Not.Null);
            var hits = new List<RaycastResult>();
            var center = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = center }, hits);
            Assert.That(hits.Count, Is.GreaterThan(0), button.name + " has no input hit.");
            Assert.That(hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform), Is.True,
                button.name + " is covered or clipped.");
        }

        private static IEnumerator CaptureScreenState(string state, LaboratoryView view, string error)
        {
            var name = "plan013-screen-" + state + "-" + Screen.width + "x" + Screen.height + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/" + name + ".png"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
            yield return WaitFor(() => File.Exists(path) && new FileInfo(path).Length > 0, "Screen evidence was not written.");
            File.WriteAllText(Path.ChangeExtension(path, ".json"), JsonUtility.ToJson(new ScreenStateEvidence
            {
                state = state, width = Screen.width, height = Screen.height, safeArea = Screen.safeArea,
                phase = view?.Controller?.Phase.ToString() ?? "DataLoading", endingElapsed = view?.Ending?.Elapsed ?? 0,
                currentRequest = view?.Ending?.Request != null && view.Controller.IsCurrentEndingRequest(view.Ending.Request), error = error
            }, true));
        }

        [Serializable] private sealed class ScreenStateEvidence
        {
            public string state, phase, error;
            public int width, height;
            public Rect safeArea;
            public double endingElapsed;
            public bool currentRequest;
        }

        private sealed class ScreenStateSource : ICsvDataSource
        {
            public readonly TaskCompletionSource<CsvBundle> Gate = new TaskCompletionSource<CsvBundle>();
            public Task<CsvBundle> LoadAsync(CancellationToken token) => Gate.Task;
        }
    }
}
#endif
