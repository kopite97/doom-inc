#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        private const string ScenePath = "Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity";

        [UnityTest]
        public IEnumerator RealStartBuildsCollectsAndStoresWithAutomaticSaveAndMidConstructionContinue()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN008-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "session.json");
            LaboratoryView view = null;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                yield return LoadIsolatedScene(path, 0.2, loaded => view = loaded);
                var oldController = view.Controller;
                var start = view.Controller.Current;
                Assert.That(start.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(start.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                var initialWorker = start.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var initialResources = start.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource).ToArray();
                Assert.That(initialResources.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                Assert.That(initialResources.All(t => t.HolderId == initialWorker.Id), Is.True);

                Assert.That(view.BeginGatheringConstruction(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(3, 3)), Is.True);
                Assert.That(view.ConfirmPlacement(), Is.True);
                Assert.That(view.BeginStorageConstruction(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(9, 3)), Is.True);
                Assert.That(view.ConfirmPlacement(), Is.True);
                Assert.That(start.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                    .All(t => !t.GetComponent<ConstructionComponent>().IsComplete), Is.True,
                    "Designation must not be free instant construction.");
                view.SimulationRate = 12;
                yield return WaitFor(() => start.Registry.Things.Any(t => t.TryGetComponent<ConstructionComponent>(out var state)
                    && !state.IsComplete && state.Progress > 0), "The real worker did not begin construction.");
                var building = start.Registry.Things.First(t => t.TryGetComponent<ConstructionComponent>(out var state)
                    && !state.IsComplete && state.Progress > 0);
                var buildProgress = building.GetComponent<ConstructionComponent>().Progress;
                var delivered = building.GetComponent<ConstructionComponent>().Delivered;
                var resourceQuantity = start.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                    .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
                var idsBefore = start.Registry.Things.Select(t => t.Id).ToArray();
                var jobBefore = initialWorker.GetComponent<WorkerComponent>().Job;
                Assert.That(view.BeginStorageConstruction(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(12, 6)), Is.True);
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.False);
                var savesBefore = view.Controller.SuccessfulSaves;
                // No explicit Save or Advance: actual Update must save while placement pauses gameplay.
                yield return WaitFor(() => view.Controller.SuccessfulSaves > savesBefore,
                    "The scene did not automatically save construction progress.");
                Assert.That(building.GetComponent<ConstructionComponent>().Progress, Is.EqualTo(buildProgress));
                Assert.That(view.Controller.LastSaveError, Is.Null.Or.Empty);
                var savedJson = File.ReadAllText(path);
                UnityEngine.Object.Destroy(view.gameObject);
                view = null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.That(File.ReadAllText(path), Is.EqualTo(savedJson), "No closed-game progress or save writes.");

                yield return LoadIsolatedScene(path, 0.2, loaded =>
                {
                    view = loaded;
                    view.SimulationRate = 0;
                });
                var restored = view.Controller.Current;
                Assert.That(view.Controller, Is.Not.SameAs(oldController));
                Assert.That(restored.Registry, Is.Not.SameAs(start.Registry));
                Assert.That(restored.Registry.Things.Select(t => t.Id), Is.EquivalentTo(idsBefore));
                Assert.That(restored.Registry.TryGet(building.Id, out var restoredBuilding), Is.True);
                Assert.That(restoredBuilding, Is.Not.SameAs(building));
                Assert.That(restoredBuilding.GetComponent<ConstructionComponent>().Progress, Is.EqualTo(buildProgress).Within(1e-9));
                Assert.That(restoredBuilding.GetComponent<ConstructionComponent>().Delivered, Is.EqualTo(delivered));
                Assert.That(restored.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                    .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(resourceQuantity));
                Assert.That(restored.Registry.TryGet(initialWorker.Id, out var restoredWorker), Is.True);
                var restoredJob = restoredWorker.GetComponent<WorkerComponent>().Job;
                Assert.That(restoredJob.Kind, Is.EqualTo(jobBefore.Kind));
                Assert.That(restoredJob.Stage, Is.EqualTo(jobBefore.Stage));
                Assert.That(restoredJob.TargetId, Is.EqualTo(jobBefore.TargetId));
                Assert.That(view.IsEditing, Is.False, "Uncommitted construction candidates are transient.");
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));

                view.SimulationRate = 12;
                yield return WaitFor(() => restored.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                    .All(t => t.GetComponent<ConstructionComponent>().IsComplete), "Both facilities were not completed.");
                var store = restored.Registry.Things.Single(t => t.Definition is FacilityDefinition def && def.Role == FacilityRole.Storage);
                yield return WaitFor(() => StoredQuantity(store) >= 10,
                    "The actual worker did not repeat gathering, movement and storage admission.");
                view.SimulationRate = 0;
                Assert.That(view.SelectThing(restoredWorker.Id), Is.True);
                Assert.That(view.SetWorkerPriorities(restoredWorker.Id, new[] { 1, 3, 2 }), Is.True);
                Assert.That(restoredWorker.GetComponent<WorkerComponent>().UsesPreset, Is.False);
                Assert.That(view.Simulation.Reservations.All(r => r.Amount > 0), Is.True);
                yield return WaitFor(() => view.RepaintCount > 0, "The actual gameplay scene did not repaint.");
                var screenshot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/plan008-laboratory-playmode.png"));
                Directory.CreateDirectory(Path.GetDirectoryName(screenshot));
                if (File.Exists(screenshot)) File.Delete(screenshot);
                ScreenCapture.CaptureScreenshot(screenshot);
                yield return WaitFor(() => File.Exists(screenshot) && new FileInfo(screenshot).Length > 0,
                    "The actual gameplay screenshot was not written.");
                Assert.That(view.Restart(), Is.True);
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                Assert.That(view.Simulation.Reservations, Is.Empty);
                Assert.That(view.Controller.Current.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(view.Controller.Current.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                    .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                Assert.That(view.Reload(), Is.True, "Restart must replace the actual saved slot.");
                Assert.That(view.Controller.Current.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(view.Controller.Current.Registry.Things.Select(t => t.Id).Intersect(idsBefore), Is.Empty);
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
        public IEnumerator SceneAndHarnessShareConstructionSettingsAndPlacementRules()
        {
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN008-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                yield return LoadIsolatedScene(Path.Combine(directory, "view.json"), 30, loaded =>
                {
                    view = loaded;
                    view.SimulationRate = 0;
                });
                var organic = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/SO/Resource/OrganicMatter.asset");
                var gathering = AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Gathering.asset");
                var storage = AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Storage.asset");
                var prototype = AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/PrototypeSupport.asset");
                var harness = new GameFlowManager(new SessionSnapshotCodec(new ThingDefinition[] { organic, gathering, storage, prototype }),
                    new JsonFileSessionStore(Path.Combine(directory, "harness.json")),
                    () => LaboratorySessionFactory.Create(organic, gathering, storage, prototype), 30);
                Assert.That(harness.StartOrContinue(), Is.True);
                var commands = new LaboratoryCommands(harness);
                Assert.That(view.BeginStorageConstruction(), Is.EqualTo(commands.BeginConstruction(storage)));
                Assert.That(view.PreviewAt(new Vector2Int(-1, 0)), Is.EqualTo(commands.PreviewConstruction(new Vector2Int(-1, 0), QuarterTurn.None)));
                Assert.That(view.ConfirmPlacement(), Is.False);
                Assert.That(commands.ConfirmConstruction(), Is.False);
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.EqualTo(harness.CanAdvanceLaboratory));
                Assert.That(view.PreviewAt(new Vector2Int(5, 3)), Is.EqualTo(commands.PreviewConstruction(new Vector2Int(5, 3), QuarterTurn.None)));
                Assert.That(view.ConfirmPlacement(), Is.EqualTo(commands.ConfirmConstruction()));
                for (var index = 0; index < 200; index++)
                {
                    view.Controller.Advance(0.1);
                    harness.Advance(0.1);
                }
                var viewStore = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility);
                var harnessSimulation = (LaboratoryManager)harness.Current.Runtime;
                var harnessStore = harnessSimulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility);
                Assert.That(viewStore.GetComponent<ConstructionComponent>().IsComplete, Is.True);
                Assert.That(harnessStore.GetComponent<ConstructionComponent>().IsComplete, Is.True);
                Assert.That(view.SetAllowed(viewStore.Id, false), Is.EqualTo(commands.SetAllowed(harnessStore.Id, false)));
                Assert.That(viewStore.GetComponent<StorageComponent>().AllowsOrganic,
                    Is.EqualTo(harnessStore.GetComponent<StorageComponent>().AllowsOrganic));
                Assert.That(view.SetPreset(new[] { 3, 2, 1 }), Is.EqualTo(commands.SetPreset(new[] { 3, 2, 1 })));
                Assert.That(view.Simulation.Preset, Is.EqualTo(harnessSimulation.Preset));
                var viewWorker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var harnessWorker = harnessSimulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(view.SetWorkerPriorities(viewWorker.Id, new[] { 2, 1, 3 }),
                    Is.EqualTo(commands.SetWorkerPriorities(harnessWorker.Id, new[] { 2, 1, 3 })));
                Assert.That(viewWorker.GetComponent<WorkerComponent>().Priorities,
                    Is.EqualTo(harnessWorker.GetComponent<WorkerComponent>().Priorities));
                Assert.That(view.SelectThing(viewStore.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.EqualTo(harness.BeginPlacement(harnessStore.Id)));
                Assert.That(view.RotatePreview(), Is.EqualTo(harness.PreviewPlacement(new Vector2Int(5, 3), QuarterTurn.Clockwise90)));
                Assert.That(view.PreviewAt(new Vector2Int(10, 4)), Is.EqualTo(harness.PreviewPlacement(new Vector2Int(10, 4), QuarterTurn.Clockwise90)));
                Assert.That(view.ConfirmPlacement(), Is.EqualTo(harness.ConfirmPlacement()));
                Assert.That(viewStore.Placement.Origin, Is.EqualTo(harnessStore.Placement.Origin));
                Assert.That(viewStore.Placement.Rotation, Is.EqualTo(harnessStore.Placement.Rotation));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static int StoredQuantity(Thing holder) => holder.GetComponent<ResourceContainerComponent>()
            .Contents.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);

        private static IEnumerator LoadIsolatedScene(string path, double interval, Action<LaboratoryView> loaded, bool keepFocused = true)
        {
            Exception failure = null;
            LaboratoryView initialized = null;
            void InitializeBeforeStart(Scene scene, LoadSceneMode mode)
            {
                if (scene.path != ScenePath) return;
                try
                {
                    initialized = scene.GetRootGameObjects().Select(root => root.GetComponent<LaboratoryView>())
                        .Single(candidate => candidate != null);
                    using (var serialized = new SerializedObject(initialized))
                    {
                        var organic = serialized.FindProperty("organic").objectReferenceValue as ResourceDefinition;
                        var gathering = serialized.FindProperty("gathering").objectReferenceValue as FacilityDefinition;
                        var storage = serialized.FindProperty("storage").objectReferenceValue as FacilityDefinition;
                        var prototype = serialized.FindProperty("prototype").objectReferenceValue as CreatureDefinition;
                        Assert.That(organic, Is.Not.Null);
                        Assert.That(gathering, Is.Not.Null);
                        Assert.That(storage, Is.Not.Null);
                        Assert.That(prototype, Is.Not.Null);
                        initialized.Initialize(organic, gathering, storage, prototype, path, interval);
                    }
                    loaded(initialized);
                    if (keepFocused) initialized.StartCoroutine(KeepScenarioFocused(initialized));
                }
                catch (Exception error)
                {
                    failure = error;
                    if (initialized != null) initialized.enabled = false;
                }
            }
            SceneManager.sceneLoaded += InitializeBeforeStart;
            try
            {
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                yield return WaitFor(() => load.isDone, "The laboratory scene did not load.");
                Assert.That(failure, Is.Null, failure?.ToString());
                Assert.That(initialized, Is.Not.Null);
                Assert.That(initialized.SavePath, Is.EqualTo(Path.GetFullPath(path)), "Tests must not touch the real gameplay slot.");
                Assert.That(initialized.Simulation, Is.Not.Null, initialized.Controller.LastError);
                yield return null;
            }
            finally { SceneManager.sceneLoaded -= InitializeBeforeStart; }
        }

        // Scenario setup: model an active app even when the host switches away from Game View.
        // The lifecycle scenario opts out and explicitly verifies real pause/focus callbacks.
        private static IEnumerator KeepScenarioFocused(LaboratoryView view)
        {
            while (view != null)
            {
                view.SendMessage("OnApplicationFocus", true);
                yield return null;
            }
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (!condition() && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(condition(), Is.True, message);
        }
    }
}
#endif
