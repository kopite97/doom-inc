#if UNITY_EDITOR
using MobileGame.TestSupport;
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
        public IEnumerator InvasionSceneRunsPaidCycleReleasesReportsStopsAndRebinds()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN010-Play-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                yield return LoadIsolatedScene(Path.Combine(directory, "view.json"), 5, loaded => { view = loaded; view.SimulationRate = 0; });
                Assert.That(GameDataStore.Instance.State, Is.EqualTo(GameDataLoadState.Ready));
                Assert.That(view.Simulation.Invasion.State.Settings.TargetScore, Is.EqualTo(GameDataStore.Instance.Catalog.Settings.Invasion.TargetScore));
                var catalog = new[] { "Resource/OrganicMatter", "Resource/CultureMedium", "Facility/Gathering", "Facility/Storage",
                    "Facility/Processing", "Facility/Cultivation", "Creature/PrototypeSupport", "Creature/Support", "Creature/Invader" }
                    .Select(path => DataTestAssets.LoadAssetAtPath<ThingData>("Assets/SO/" + path + ".asset")).ToArray();
                var codec = new SessionSnapshotCodec(catalog, true);
                var organic = (ResourceData)catalog[0]; var culture = (ResourceData)catalog[1];
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
                yield return AdvanceUntil(view, () => view.Simulation.Registry.Things.Any(t => t.Definition == catalog[7]), 800);
                var support = view.Simulation.Registry.Things.Single(t => t.Definition == catalog[7]);
                Assert.That(view.AssignWorker(support.Id, processor.Id), Is.True);
                Assert.That(view.SetWorkerPriorities(support.Id, new[] { 2, 3, 4, 1 }), Is.True);
                Assert.That(view.SelectCultivation(cultivator.Id, CultivationTarget.Invader), Is.True);
                yield return AdvanceUntil(view, () => view.Simulation.Invasion.WaitingCount >= 1, 800);
                var waiting = view.Simulation.Registry.Things.First(t => t.Definition == catalog[8]);
                Assert.That(view.SelectThing(waiting.Id), Is.True);
                yield return CaptureInvasion(view, codec, "plan010-waiting");

                // Identical graph, command and time through View and headless flow.
                var checkpoint = codec.Capture(view.Controller.Current, SessionPhase.Playing);
                var harness = new GameFlowManager(codec, new JsonFileSessionStore(Path.Combine(directory, "harness.json")), () => codec.Restore(checkpoint));
                Assert.That(harness.StartOrContinue(), Is.True);
                Assert.That(view.ReleaseInvaders(), Is.EqualTo(new LaboratoryCommands(harness).ReleaseInvaders()));
                Assert.That(view.SelectedId, Is.EqualTo(waiting.Id), "The selected invader changes to its external-activity details.");
                Assert.That(view.SelectThing(waiting.Id), Is.False);
                Assert.That(view.Invasion.Score, Is.Zero);
                Assert.That(waiting.IsPlaced, Is.False);
                Assert.That(view.Simulation.Map.GetPlacements().Any(placement => placement.ThingId == waiting.Id), Is.False);
                view.ExpandSelection(); yield return null;
                Assert.That(VisibleUiText(view), Does.Contain("외부 활동 중"));
                var beforeToggle = codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase));
                for (var index = 0; index < 9; index++) view.ToggleReports();
                Assert.That(view.ReportsExpanded, Is.True);
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)), Is.EqualTo(beforeToggle));
                view.Controller.Advance(5); harness.Advance(5);
                var viewSnapshot = codec.Capture(view.Controller.Current, view.Controller.Phase);
                var harnessSnapshot = codec.Capture(harness.Current, harness.Phase);
                Assert.That(JsonUtility.ToJson(viewSnapshot.invasion), Is.EqualTo(JsonUtility.ToJson(harnessSnapshot.invasion)));
                // Independently produced resource stacks legitimately receive fresh GUIDs.
                // Compare their definition/holder/quantity, while existing invader IDs stay exact above.
                Assert.That(ResourceInventory(viewSnapshot), Is.EqualTo(ResourceInventory(harnessSnapshot)));
                var harnessLab = (LaboratoryManager)harness.Current.Runtime;
                foreach (var facility in view.Simulation.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _)))
                {
                    Assert.That(harnessLab.Registry.TryGet(facility.Id, out var other), Is.True);
                    Assert.That(facility.GetComponent<ProductionComponent>().Progress,
                        Is.EqualTo(other.GetComponent<ProductionComponent>().Progress).Within(1e-8));
                }
                Assert.That(view.Invasion.Score, Is.EqualTo(2));
                yield return CaptureInvasion(view, codec, "plan010-detailed-report");

                // Actual Update advances outside activity; rendering and closed reports do not gate it.
                view.ToggleReports();
                var elapsed = view.Invasion.Elapsed;
                view.SimulationRate = 1;
                yield return WaitFor(() => view.Invasion.Elapsed > elapsed + 0.15, "Actual Update did not advance external activity.");
                view.SimulationRate = 0;
                yield return AdvanceUntil(view, () => view.Simulation.Invasion.WaitingCount >= 1, 150);
                Assert.That(view.Simulation.Invasion.ExternalCount, Is.EqualTo(1));
                Assert.That(view.ReleaseInvaders(), Is.True);
                Assert.That(view.Simulation.Invasion.ExternalCount, Is.GreaterThanOrEqualTo(2));
                view.ToggleReports();
                yield return CaptureInvasion(view, codec, "plan010-later-reports");
                yield return AdvanceUntil(view, () => view.Controller.Phase == SessionPhase.Destruction, 200);
                yield return CaptureInvasion(view, codec, "plan010-goal-stopped");
                Assert.That(view.Invasion.GoalReached, Is.True);
                Assert.That(view.Controller.CanAdvanceExternal || view.Controller.CanAdvanceLaboratory, Is.False);
                Assert.That(view.ReleaseInvaders(), Is.False);
                var stoppedScore = view.Invasion.Score;
                var stoppedActivities = view.Invasion.ActivitySequence;
                var stoppedReports = view.Invasion.Reports.Select(r => r.Sequence).ToArray();
                var oldState = view.Invasion;
                // Uses the automatic goal file; no extra Save before Continue.
                Assert.That(view.Reload(), Is.True, view.Controller.LastError);
                Assert.That(view.Invasion, Is.Not.SameAs(oldState));
                Assert.That(view.Invasion.Score, Is.EqualTo(stoppedScore));
                Assert.That(view.Invasion.ActivitySequence, Is.EqualTo(stoppedActivities));
                Assert.That(view.Invasion.Reports.Select(r => r.Sequence), Is.EqualTo(stoppedReports));
                Assert.That(view.ReportsExpanded, Is.False);
                view.Controller.Advance(100);
                Assert.That(oldState.Score, Is.EqualTo(stoppedScore));
                Assert.That(view.Invasion.Score, Is.EqualTo(stoppedScore));
                Assert.That(view.Restart(), Is.True); Assert.That(view.Reload(), Is.True);
                Assert.That(view.Invasion.Score, Is.Zero); Assert.That(view.Invasion.Reports, Is.Empty);
                Assert.That(view.Simulation.Invasion.ExternalCount + view.Simulation.Invasion.WaitingCount, Is.Zero);
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                Assert.That(view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                    .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static IEnumerator CaptureInvasion(LaboratoryView view, SessionSnapshotCodec codec, string name)
        {
            var repaints = view.RepaintCount;
            yield return WaitFor(() => view.RepaintCount > repaints, "Invasion panel did not repaint.");
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/" + name + ".png"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return WaitFor(() => File.Exists(path) && new FileInfo(path).Length > 0, "Invasion screenshot was not written.");
            File.WriteAllText(Path.ChangeExtension(path, ".json"), codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)));
        }

        private static string[] ResourceInventory(SessionSnapshot snapshot) => snapshot.things.Where(t => t.quantity > 0)
            .GroupBy(t => t.definitionId + "/" + t.holderId)
            .OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => g.Key + "=" + g.Sum(t => t.quantity)).ToArray();
    }
}
#endif
