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
using Object = UnityEngine.Object;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest] public IEnumerator SceneViewDestroyedBeforePersistentOwnerCanShutDownCleanly()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            flow.StartNewGame(); yield return Settled();
            Object.DestroyImmediate(flow.Laboratory.gameObject);
            Object.DestroyImmediate(flow.gameObject);
            yield return null;
            Assert.That(SceneFlowManager.Instance, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator OwnerDestroyedAtSceneArrivalDoesNotAutoStartANewRun()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            void CancelOnArrival(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
            {
                if (scene.path == LaboratorySceneAssets.ScenePath) Object.DestroyImmediate(flow.gameObject);
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += CancelOnArrival;
            try
            {
                flow.StartNewGame();
                var deadline = Time.realtimeSinceStartupAsDouble + 20;
                while (flow != null && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                for (var i = 0; i < 5; i++) yield return null;
                Assert.That(SceneFlowManager.Instance, Is.Null);
                Assert.That(File.Exists(path), Is.False);
                var entry = Object.FindFirstObjectByType<GameBootstrap>();
                Assert.That(entry.Error, Does.Contain("canceled"));
                flow = SceneFlowManager.GetOrCreate(configuration, path); flow.StartCoroutine(KeepFocused());
                flow.EnterCurrent(AppScene.Laboratory); yield return Settled();
                Assert.That(flow.Laboratory.Controller.Current, Is.Not.Null);
            }
            finally { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= CancelOnArrival; }
        }

        [UnityTest] public IEnumerator PreviousPressCannotConfirmNewScreenAndPauseKeepsTransitionBlocked()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            File.WriteAllText(path, "preserve until explicit confirm");
            flow.StartNewGame(); yield return WaitPopupStable();
            var button = flow.PopupHost.GetComponentsInChildren<UiButton>().Single(b => b.name == "Confirm");
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = 123 };
            button.OnPointerDown(pointer);
            flow.CancelPrompt(); flow.StartNewGame();
            // The old object is already inactive; dispatch its stale release before deferred destruction.
            button.OnPointerClick(pointer);
            yield return WaitPopupStable();
            Assert.That(flow.IsPrompting, Is.True);
            Assert.That(File.ReadAllText(path), Is.EqualTo("preserve until explicit confirm"));
            flow.CancelPrompt(); File.Delete(path);
            flow.StartNewGame(); flow.SendMessage("OnApplicationPause", true);
            for (var i = 0; i < 20; i++) yield return null;
            Assert.That(flow.IsTransitioning, Is.True); Assert.That(flow.Loading.Visible, Is.True);
            Assert.That(File.Exists(path), Is.False);
            flow.SendMessage("OnApplicationPause", false); yield return Settled();
            Assert.That(flow.Laboratory.Controller.Current, Is.Not.Null);
        }

        [UnityTest] public IEnumerator DestroyedFlowCannotPublishLateSceneEntryAndDuplicateDoesNotReplaceOwner()
        {
            var gate = new TaskCompletionSource<GameDataCatalog>();
            var pending = GameDataStore.GetOrCreate().InitializeAsync(token => gate.Task);
            flow.EnterCurrent(AppScene.Bootstrap); yield return null; yield return null; yield return null;
            var duplicate = new GameObject("DuplicateFlow").AddComponent<SceneFlowManager>();
            yield return null;
            Assert.That(duplicate == null, Is.True); Assert.That(SceneFlowManager.Instance, Is.SameAs(flow));
            Object.Destroy(flow.gameObject); yield return null;
            gate.SetResult(GameDataAssets.LoadCatalog());
            for (var i = 0; i < 5; i++) yield return null;
            Assert.That(pending.IsCompletedSuccessfully, Is.True, "App data can finish independently of its former scene requester.");
            Assert.That(SceneFlowManager.Instance, Is.Null); Assert.That(File.Exists(path), Is.False);
            flow = SceneFlowManager.GetOrCreate(configuration, path); flow.StartCoroutine(KeepFocused());
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator PreparedOperatingAndNearEndingSlotsUseNormalContinueAndRestoreResults()
        {
            var data = GameDataAssets.LoadCatalog();
            AppTestScenario.Prepare(data, path, false);
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled(); flow.Continue(); yield return Settled();
            Assert.That(flow.Laboratory.Simulation.Invasion.WaitingCount, Is.GreaterThanOrEqualTo(2));
            flow.RequestReturnToTitle(); yield return ClickLoading("Retry"); yield return Settled();
            AppTestScenario.Prepare(data, path, true);
            flow.Continue(); yield return Settled();
            var lab = flow.Laboratory; lab.SimulationRate = 0;
            Assert.That(lab.Invasion.Score, Is.GreaterThanOrEqualTo(data.Settings.Invasion.TargetScore * .9));
            for (var i = 0; i < 1000 && lab.Controller.Phase == SessionPhase.Playing; i++) lab.Controller.Advance(.5);
            Assert.That(lab.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(lab.Save(), Is.True);
            // Preparation reloads the scene through the production transition, preserving the stored ending phase.
            flow.Continue(); yield return Settled(); lab = flow.Laboratory;
            Assert.That(lab.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            for (var i = 0; i < (data.Settings.EndingSeconds + 1) * 10; i++) lab.AdvanceFrame(0, .1);
            Assert.That(lab.Controller.Phase, Is.EqualTo(SessionPhase.Results));
            flow.RequestReturnToTitle(); yield return ClickLoading("Retry"); yield return Settled();
            flow.Continue(); yield return Settled();
            Assert.That(flow.Laboratory.Controller.Phase, Is.EqualTo(SessionPhase.Results));
        }
    }
}
#endif
