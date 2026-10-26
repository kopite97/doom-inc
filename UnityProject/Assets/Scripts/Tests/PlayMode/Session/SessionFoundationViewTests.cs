#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.GameFlow;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using SessionState = MobileGame.Foundation.SessionState;

namespace MobileGame.SpatialTests
{
    public sealed class SessionFoundationViewTests
    {
        private const string ScenePath = "Assets/Scenes/Diagnostics/Session/Temp_SessionFoundation.unity";

        [UnityTest]
        public IEnumerator SceneUpdateAutoSavesAndRestoresNewGraphThenRestartsSlot()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN007-" + Guid.NewGuid().ToString("N"));
            var savePath = Path.Combine(directory, "session.json");
            SessionFoundationView view = null;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                yield return LoadIsolatedScene(savePath, 0.2, loaded => view = loaded);
                var originalController = view.Controller;
                var originalState = originalController.Current;
                var facilities = originalState.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                    .OrderBy(t => t.CreationOrder).ToArray();
                var first = facilities[0];
                var second = facilities[1];
                var resource = originalState.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Resource);
                var creature = originalState.Registry.Things.First(t => t.Definition.Kind == ThingKind.Creature);
                var definitionBefore = EditorJsonUtility.ToJson(first.Definition);
                var oldIds = originalState.Registry.Things.Select(t => t.Id).ToArray();
                Assert.That(view.SelectThing(first.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.True);
                Assert.That(view.RotatePreview(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(4, 3)), Is.True);
                Assert.That(view.ConfirmPlacement(), Is.True);
                originalState.Registry.ConsumeResource(resource, 2);
                Assert.That(originalState.Map.Commands.Unplace(creature.Id).Succeeded, Is.True);

                Assert.That(view.SelectThing(second.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(8, 6)), Is.True);
                var laboratoryBefore = view.LaboratorySecondsObserved;
                var externalBefore = view.ExternalSecondsObserved;
                var savesBefore = originalController.SuccessfulSaves;
                // No Save() or Advance() call: real MonoBehaviour.Update must trigger the write.
                yield return WaitFor(() => File.Exists(savePath) && originalController.SuccessfulSaves > savesBefore,
                    "Update did not automatically write the isolated slot.");
                Assert.That(view.LaboratorySecondsObserved, Is.EqualTo(laboratoryBefore), "Editing stops laboratory updates.");
                Assert.That(view.ExternalSecondsObserved, Is.GreaterThan(externalBefore), "Editing does not stop external updates.");
                Assert.That(new FileInfo(savePath).Length, Is.GreaterThan(0));
                Assert.That(originalController.LastSaveError, Is.Null.Or.Empty);
                var savedFile = File.ReadAllText(savePath);

                UnityEngine.Object.Destroy(view.gameObject);
                view = null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.That(File.ReadAllText(savePath), Is.EqualTo(savedFile), "No live view means no progress/save update.");
                double observedAtRestore = -1;
                yield return LoadIsolatedScene(savePath, 0.2, loaded =>
                {
                    view = loaded;
                    observedAtRestore = loaded.LaboratorySecondsObserved;
                });
                var restored = view.Controller.Current;
                Assert.That(view.Controller, Is.Not.SameAs(originalController));
                Assert.That(restored.Registry, Is.Not.SameAs(originalState.Registry));
                Assert.That(restored.Map, Is.Not.SameAs(originalState.Map));
                Assert.That(restored.Registry.Things.Select(t => t.Id), Is.EquivalentTo(oldIds));
                Assert.That(restored.Registry.TryGet(first.Id, out var restoredFirst), Is.True);
                Assert.That(restoredFirst, Is.Not.SameAs(first));
                Assert.That(restoredFirst.Placement.Origin, Is.EqualTo(new Vector2Int(4, 3)));
                Assert.That(restoredFirst.Placement.Rotation, Is.EqualTo(QuarterTurn.Clockwise90));
                Assert.That(restored.Registry.TryGet(second.Id, out var restoredSecond), Is.True);
                Assert.That(restoredSecond.Placement.Origin, Is.EqualTo(new Vector2Int(7, 3)),
                    "The unconfirmed preview must not become a saved placement.");
                Assert.That(restored.Registry.TryGet(resource.Id, out var restoredResource), Is.True);
                Assert.That(restoredResource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
                Assert.That(restoredResource.HolderId, Is.EqualTo(first.Id));
                Assert.That(restored.Registry.TryGet(creature.Id, out var restoredCreature), Is.True);
                Assert.That(restoredCreature.IsPlaced, Is.False);
                Assert.That(restored.Map.Commands.IsEditing, Is.False);
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                Assert.That(observedAtRestore, Is.Zero, "Restore must not emit catch-up time from wall-clock elapsed time.");

                var savesBeforeResult = view.Controller.SuccessfulSaves;
                Assert.That(view.ReachGoal(), Is.True);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
                Assert.That(view.FinishDestruction(), Is.True);
                yield return WaitFor(() => view.Controller.SuccessfulSaves > savesBeforeResult,
                    "The result phase was not automatically saved.");
                Assert.That(view.Reload(), Is.True);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.False);
                Assert.That(view.Controller.CanAdvanceExternal, Is.False);

                Assert.That(view.Restart(), Is.True);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Playing));
                Assert.That(view.Controller.Current.Registry.Things.Select(t => t.Id).Intersect(oldIds), Is.Empty);
                Assert.That(view.Controller.Current.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Resource)
                    .GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
                var restartedIds = view.Controller.Current.Registry.Things.Select(t => t.Id).ToArray();
                Assert.That(view.Reload(), Is.True);
                Assert.That(view.Controller.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(restartedIds),
                    "Restart must replace the actual file, not just the in-memory session.");

                var selected = view.Controller.Current.Registry.Things.First(t => t.Definition.Kind == ThingKind.Facility);
                Assert.That(view.SelectThing(selected.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.True);
                Assert.That(view.Restart(), Is.True);
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                Assert.That(view.Controller.Current.Map.Commands.IsEditing, Is.False);
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.True);
                Assert.That(EditorJsonUtility.ToJson(first.Definition), Is.EqualTo(definitionBefore));
                yield return WaitFor(() => view.RepaintCount > 0, "The actual scene did not repaint.");
                var screenshot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/session-foundation-playmode.png"));
                Directory.CreateDirectory(Path.GetDirectoryName(screenshot));
                if (File.Exists(screenshot)) File.Delete(screenshot);
                ScreenCapture.CaptureScreenshot(screenshot);
                yield return WaitFor(() => File.Exists(screenshot) && new FileInfo(screenshot).Length > 0,
                    "The actual Game View screenshot was not written.");
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [UnityTest]
        public IEnumerator SceneButtonsAndHarnessUseSameFlowAndPlacementCommands()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN007-" + Guid.NewGuid().ToString("N"));
            SessionFoundationView view = null;
            try
            {
                yield return LoadIsolatedScene(Path.Combine(directory, "view.json"), 30, loaded => view = loaded);
                var storage = AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Storage.asset");
                var creature = AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/PrototypeSupport.asset");
                var resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/SO/Resource/OrganicMatter.asset");
                var harness = new SessionController(new SessionSnapshotCodec(new ThingDefinition[] { storage, creature, resource }),
                    new JsonFileSessionStore(Path.Combine(directory, "harness.json")), () =>
                    {
                        var fixture = new MapFoundationScenario(storage, creature, resource);
                        return new SessionState(fixture.Registry, fixture.Map);
                    }, 30);
                Assert.That(harness.StartOrContinue(), Is.True);
                var viewFacility = view.Controller.Current.Registry.Things.First(t => t.Definition.Kind == ThingKind.Facility);
                var harnessFacility = harness.Current.Registry.Things.First(t => t.Definition.Kind == ThingKind.Facility);
                Assert.That(view.SelectThing(viewFacility.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.EqualTo(harness.BeginPlacement(harnessFacility.Id)));
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.EqualTo(harness.CanAdvanceLaboratory));
                Assert.That(view.PreviewAt(new Vector2Int(7, 3)),
                    Is.EqualTo(harness.PreviewPlacement(new Vector2Int(7, 3), QuarterTurn.None)));
                Assert.That(view.ConfirmPlacement(), Is.EqualTo(harness.ConfirmPlacement()));
                Assert.That(view.Controller.Current.Map.Commands.IsEditing, Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(4, 3)),
                    Is.EqualTo(harness.PreviewPlacement(new Vector2Int(4, 3), QuarterTurn.None)));
                Assert.That(view.ConfirmPlacement(), Is.EqualTo(harness.ConfirmPlacement()));
                Assert.That(viewFacility.Placement.Origin, Is.EqualTo(harnessFacility.Placement.Origin));
                Assert.That(view.BeginPlacement(), Is.EqualTo(harness.BeginPlacement(harnessFacility.Id)));
                Assert.That(view.ReachGoal(), Is.EqualTo(harness.ReachGoal()));
                Assert.That(view.CancelPlacement(), Is.EqualTo(harness.CancelPlacement()));
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.False, "Cancel cannot restart an ended round.");
                Assert.That(view.Controller.CanAdvanceExternal, Is.EqualTo(harness.CanAdvanceExternal));
                Assert.That(view.FinishDestruction(), Is.EqualTo(harness.FinishDestruction()));
                Assert.That(view.Controller.Phase, Is.EqualTo(harness.Phase));
                Assert.That(view.BeginPlacement(), Is.False);
                Assert.That(view.Controller.LastError, Is.Not.Null.And.Not.Empty);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static IEnumerator LoadIsolatedScene(string savePath, double autoSaveSeconds,
            Action<SessionFoundationView> loaded)
        {
            Exception initializationError = null;
            SessionFoundationView initialized = null;
            void InitializeBeforeStart(Scene scene, LoadSceneMode mode)
            {
                if (scene.path != ScenePath) return;
                try
                {
                    initialized = scene.GetRootGameObjects().Select(root => root.GetComponent<SessionFoundationView>())
                        .Single(view => view != null);
                    using (var serialized = new SerializedObject(initialized))
                    {
                        var storage = serialized.FindProperty("storage").objectReferenceValue as FacilityDefinition;
                        var creature = serialized.FindProperty("creature").objectReferenceValue as CreatureDefinition;
                        var resource = serialized.FindProperty("resource").objectReferenceValue as ResourceDefinition;
                        Assert.That(storage, Is.Not.Null, "The saved scene must retain its facility SO reference.");
                        Assert.That(creature, Is.Not.Null, "The saved scene must retain its creature SO reference.");
                        Assert.That(resource, Is.Not.Null, "The saved scene must retain its resource SO reference.");
                        initialized.Initialize(storage, creature, resource, savePath, autoSaveSeconds);
                    }
                    loaded(initialized);
                }
                catch (Exception error)
                {
                    initializationError = error;
                    // Failed test injection must never fall back to the normal diagnostic slot in Start.
                    if (initialized != null) initialized.enabled = false;
                }
            }
            SceneManager.sceneLoaded += InitializeBeforeStart;
            try
            {
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                yield return WaitFor(() => load.isDone, "The diagnostic scene did not load.");
                Assert.That(initializationError, Is.Null, initializationError?.ToString());
                Assert.That(initialized, Is.Not.Null);
                Assert.That(initialized.SavePath, Is.EqualTo(Path.GetFullPath(savePath)), "Never use the player's diagnostic slot in tests.");
                Assert.That(initialized.Controller.Current, Is.Not.Null, initialized.Controller.LastError);
                yield return null;
            }
            finally { SceneManager.sceneLoaded -= InitializeBeforeStart; }
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (!condition() && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(condition(), Is.True, message);
        }
    }
}
#endif
