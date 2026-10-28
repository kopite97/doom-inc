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
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator ProductionSceneRunsPaidInitialCycleMatchesHarnessAndRestartsCleanly()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN009-Play-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                yield return LoadIsolatedScene(Path.Combine(directory, "view.json"), 5, loaded => { view = loaded; view.SimulationRate = 0; });
                var catalog = new[] { "Resource/OrganicMatter", "Resource/CultureMedium", "Facility/Gathering", "Facility/Storage",
                    "Facility/Processing", "Facility/Cultivation", "Creature/PrototypeSupport", "Creature/Support", "Creature/Invader" }
                    .Select(path => AssetDatabase.LoadAssetAtPath<ThingDefinition>("Assets/SO/" + path + ".asset")).ToArray();
                Assert.That(catalog.All(d => d != null), Is.True);
                var organic = (ResourceDefinition)catalog[0]; var culture = (ResourceDefinition)catalog[1];
                Assert.That(view.BeginGatheringConstruction(), Is.True); Place(view, 3, 3);
                Assert.That(view.BeginStorageConstruction(), Is.True); Place(view, 8, 3);
                yield return AdvanceUntil(view, () => view.Simulation.Registry.Things.Any(t => t.Definition == catalog[3] && view.Simulation.TotalAt(t.Id) >= 5), 120);
                Assert.That(view.BeginProcessingConstruction(), Is.True); Place(view, 3, 7);
                Assert.That(view.BeginCultivationConstruction(), Is.True); Place(view, 8, 7);
                Assert.That(view.BeginStorageConstruction(), Is.True); Place(view, 12, 3);
                var stores = view.Simulation.Registry.Things.Where(t => t.Definition == catalog[3]).OrderBy(t => t.CreationOrder).ToArray();
                Assert.That(view.SetAllowed(stores[0].Id, culture, false), Is.True);
                Assert.That(view.SetAllowed(stores[1].Id, organic, false), Is.True);
                yield return AdvanceUntil(view, () => view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                    .All(t => t.GetComponent<ConstructionComponent>().IsComplete), 150);
                var processor = view.Simulation.Registry.Things.Single(t => t.Definition == catalog[4]);
                var cultivator = view.Simulation.Registry.Things.Single(t => t.Definition == catalog[5]);
                Assert.That(view.SelectCultivation(cultivator.Id, CultivationTarget.Support), Is.True);
                yield return AdvanceUntil(view, () => view.Simulation.Registry.Things.Any(t => t.Definition == catalog[7]), 500);
                var support = view.Simulation.Registry.Things.Single(t => t.Definition == catalog[7]);
                Assert.That(support.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
                Assert.That(view.AssignWorker(support.Id, processor.Id), Is.True);
                Assert.That(view.SetWorkerPriorities(support.Id, new[] { 2, 3, 4, 1 }), Is.True);
                yield return AdvanceUntil(view, () => support.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Production
                    && support.GetComponent<WorkerComponent>().Job.Stage == JobStage.Working, 150);
                Assert.That(view.SelectThing(processor.Id), Is.True);
                var progress = processor.GetComponent<ProductionComponent>().Progress;
                view.Controller.Advance(0.5);
                Assert.That(processor.GetComponent<ProductionComponent>().Progress - progress, Is.EqualTo(0.5).Within(1e-8));
                var beforeUpdate = processor.GetComponent<ProductionComponent>().Progress;
                view.SimulationRate = 1;
                yield return WaitFor(() => processor.GetComponent<ProductionComponent>().Progress > beforeUpdate + 0.15,
                    "Actual Update did not advance production.");
                view.SimulationRate = 0;
                Assert.That(view.Save(), Is.True, view.Controller.LastSaveError);
                var codec = new SessionSnapshotCodec(catalog, true);
                var checkpoint = codec.Capture(view.Controller.Current, SessionPhase.Playing);
                var harness = new GameFlowManager(codec, new JsonFileSessionStore(Path.Combine(directory, "harness.json")), () => codec.Restore(checkpoint));
                Assert.That(harness.StartOrContinue(), Is.True);
                var commands = new LaboratoryCommands(harness);
                Assert.That(view.SelectCultivation(cultivator.Id, CultivationTarget.Invader), Is.EqualTo(commands.SelectCultivation(cultivator.Id, CultivationTarget.Invader)));
                Assert.That(view.AssignWorker(support.Id, null), Is.EqualTo(commands.AssignWorker(support.Id, null)));
                view.Controller.Advance(0.5); harness.Advance(0.5);
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, SessionPhase.Playing)),
                    Is.EqualTo(codec.Serialize(codec.Capture(harness.Current, SessionPhase.Playing))), "UI and harness execute identical commands and state transitions.");
                Assert.That(view.AssignWorker(support.Id, processor.Id), Is.True);
                yield return AdvanceUntil(view, () => support.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Production
                    && support.GetComponent<WorkerComponent>().Job.Stage == JobStage.Working, 60);
                yield return CaptureProduction(view, "plan009-production-working.png");
                yield return AdvanceUntil(view, () => view.Simulation.Registry.Things.Any(t => t.Definition == catalog[8]), 500);
                var waiting = view.Simulation.Registry.Things.First(t => t.Definition == catalog[8]);
                Assert.That(waiting.TryGetComponent<WorkerComponent>(out _), Is.False);
                Assert.That(view.SelectThing(cultivator.Id), Is.True);
                yield return CaptureProduction(view, "plan009-cultivation-waiting.png");
                // File reload retains the generated graph; restart clears it through the real factory.
                var beforeIds = view.Simulation.Registry.Things.Select(t => t.Id).ToArray();
                Assert.That(view.Save(), Is.True, view.Controller.LastSaveError);
                Assert.That(view.Reload(), Is.True, view.Controller.LastError);
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id), Is.EquivalentTo(beforeIds));
                Assert.That(view.Restart(), Is.True);
                Assert.That(view.Reload(), Is.True);
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                Assert.That(view.Simulation.Reservations, Is.Empty);
                Assert.That(view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id).Intersect(beforeIds), Is.Empty);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static void Place(LaboratoryView view, int x, int y)
        { Assert.That(view.PreviewAt(new Vector2Int(x,y)), Is.True); Assert.That(view.ConfirmPlacement(), Is.True); }

        private static IEnumerator AdvanceUntil(LaboratoryView view, Func<bool> condition, double maximum)
        {
            for (var steps = 0; steps < maximum * 20 && !condition(); steps++)
            {
                view.Controller.Advance(0.05);
                if (steps % 40 == 0) yield return null;
            }
            Assert.That(condition(), Is.True, "Production scenario exceeded " + maximum + " simulated seconds.");
            Assert.That(view.Controller.LastSaveError, Is.Null.Or.Empty);
        }

        private static IEnumerator CaptureProduction(LaboratoryView view, string name)
        {
            var repaints = view.RepaintCount;
            yield return WaitFor(() => view.RepaintCount > repaints, "Production panel did not repaint.");
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/" + name));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return WaitFor(() => File.Exists(path) && new FileInfo(path).Length > 0, "Production screenshot was not written.");
            File.WriteAllText(Path.ChangeExtension(path, ".json"), JsonUtility.ToJson(new ProductionTrace
            {
                lastCommand = view.LastCommand, selectedId = view.SelectedId.ToString("N"),
                workers = view.Simulation.Registry.Things.Count(t => t.TryGetComponent<WorkerComponent>(out _)),
                waitingInvaders = view.Simulation.Registry.Things.Count(t => t.Definition is CreatureDefinition d && d.Role == CreatureRole.Invader),
                reservations = view.Simulation.Reservations.Count
            }, true));
        }

        [Serializable]
        private sealed class ProductionTrace { public string lastCommand; public string selectedId; public int workers; public int waitingInvaders; public int reservations; }
    }
}
#endif
