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
        private static ThingData[] EndingCatalog() => new[] { "Resource/OrganicMatter", "Resource/CultureMedium", "Facility/Gathering", "Facility/Storage",
            "Facility/Processing", "Facility/Cultivation", "Creature/PrototypeSupport", "Creature/Support", "Creature/Invader" }
            .Select(path => DataTestAssets.LoadAssetAtPath<ThingData>("Assets/SO/" + path + ".asset")).ToArray();

        [UnityTest]
        public IEnumerator EndingSceneCompletesTwoPaidRunsAndPreservesFileOnFailedRestart()
        {
            var previousBackground = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN011-Play-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                var path = Path.Combine(directory, "view.json");
                yield return LoadIsolatedScene(path, 5, loaded => { view = loaded; view.SimulationRate = 0; });
                view.SendMessage("OnApplicationFocus", true);
                var catalog = EndingCatalog(); var codec = new SessionSnapshotCodec(catalog, true);
                Guid[] previousIds = Array.Empty<Guid>();
                for (var run = 1; run <= 2; run++)
                {
                    Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Playing));
                    Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                    Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                    Assert.That(view.Simulation.Registry.Things.Select(t => t.Id).Intersect(previousIds), Is.Empty);
                    yield return ProducePaidInvader(view, catalog);
                    Assert.That(view.ReleaseInvaders(), Is.True);
                    // Exercise both kinds of transient edit at the real activity goal.
                    if (run == 1) Assert.That(view.BeginGatheringConstruction(), Is.True);
                    else
                    {
                        Assert.That(view.SelectThing(view.Simulation.Registry.Things.First(t => t.Definition == catalog[4]).Id), Is.True);
                        Assert.That(view.BeginPlacement(), Is.True); Assert.That(view.RotatePreview(), Is.True);
                    }
                    view.Ending.SetVisible(false); // Hold only the cinematic for a stable entry capture.
                    yield return AdvanceUntil(view, () => view.Controller.Phase == SessionPhase.Destruction, 300);
                    view.AdvanceFrame(0, 0);
                    Assert.That(view.IsEditing, Is.False); Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                    Assert.That(view.BeginGatheringConstruction() || view.ReleaseInvaders(), Is.False);
                    var goal = codec.Deserialize(File.ReadAllText(path));
                    Assert.That(goal.phase, Is.EqualTo(SessionPhase.Destruction));
                    var stopped = JsonUtility.ToJson(goal.invasion);
                    var lab = JsonUtility.ToJson(codec.Capture(view.Controller.Current, view.Controller.Phase).laboratory);
                    yield return CaptureInvasion(view, codec, "plan011-run" + run + "-start");
                    view.Ending.SetVisible(true);
                    if (run == 1)
                    {
                        // Actual Update / unscaled time proceeds while simulation speed is zero.
                        yield return WaitFor(() => view.Ending.Progress >= 0.45, "The cinematic did not progress in real frames.");
                        yield return CaptureInvasion(view, codec, "plan011-run1-middle");
                        yield return WaitFor(() => view.Controller.Phase == SessionPhase.Results, "The actual cinematic did not complete.");
                    }
                    else
                    {
                        // A new graph with exactly the same state receives the same presentation inputs.
                        Assert.That(view.Reload(), Is.True);
                        lab = JsonUtility.ToJson(codec.Capture(view.Controller.Current, view.Controller.Phase).laboratory);
                        var harness = new GameFlowManager(codec, new JsonFileSessionStore(Path.Combine(directory, "harness.json")), () => codec.Restore(goal));
                        File.WriteAllText(Path.Combine(directory, "harness.json"), codec.Serialize(goal));
                        Assert.That(harness.StartOrContinue(), Is.True);
                        using var visual = new RunEndingPresentation(harness, DataTestAssets.Catalog.Settings.EndingSeconds);
                        for (var frame = 0; frame < 50; frame++)
                        {
                            view.AdvanceFrame(0, 0.1); visual.Advance(0.1);
                            Assert.That(view.Ending.Progress, Is.EqualTo(visual.Progress).Within(1e-10));
                            Assert.That(view.Controller.Phase, Is.EqualTo(harness.Phase));
                        }
                        Assert.That(harness.Phase, Is.EqualTo(SessionPhase.Results));
                        Assert.That(JsonUtility.ToJson(codec.Capture(harness.Current, harness.Phase).invasion), Is.EqualTo(stopped));
                    }
                    Assert.That(JsonUtility.ToJson(codec.Capture(view.Controller.Current, view.Controller.Phase).laboratory), Is.EqualTo(lab));
                    Assert.That(JsonUtility.ToJson(codec.Capture(view.Controller.Current, view.Controller.Phase).invasion), Is.EqualTo(stopped));
                    var resultFile = File.ReadAllText(path); // No extra Save: verifies the real completion auto-save.
                    Assert.That(codec.Deserialize(resultFile).phase, Is.EqualTo(SessionPhase.Results));
                    Assert.That(codec.Restore(codec.Deserialize(resultFile)), Is.Not.SameAs(view.Controller.Current));
                    yield return CaptureInvasion(view, codec, "plan011-run" + run + "-results");
                    previousIds = view.Simulation.Registry.Things.Select(t => t.Id).ToArray();
                    if (run == 1)
                    {
                        Assert.That(view.Reload(), Is.True); Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                        var resultRequest = view.Ending.Request; var current = view.Controller.Current;
                        using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            Assert.That(view.RestartFromResults(resultRequest), Is.False);
                            Assert.That(view.Controller.Current, Is.SameAs(current));
                            Assert.That(File.ReadAllText(path), Is.EqualTo(resultFile));
                            Assert.That(view.Controller.LastSaveError, Is.Not.Null.Or.Empty);
                            yield return CaptureInvasion(view, codec, "plan011-restart-error");
                        }
                        Assert.That(view.RestartFromResults(resultRequest), Is.True);
                        var fresh = File.ReadAllText(path);
                        Assert.That(codec.Deserialize(fresh).phase, Is.EqualTo(SessionPhase.Playing));
                        Assert.That(view.RestartFromResults(resultRequest), Is.False);
                        Assert.That(File.ReadAllText(path), Is.EqualTo(fresh));
                        Assert.That(view.Invasion.Score, Is.Zero); Assert.That(view.Invasion.Elapsed, Is.Zero);
                        Assert.That(view.Invasion.Reports, Is.Empty); Assert.That(view.ReportsExpanded, Is.False);
                        Assert.That(view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                            .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                        yield return CaptureInvasion(view, codec, "plan011-restarted");
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static IEnumerator ProducePaidInvader(LaboratoryView view, ThingData[] catalog)
        {
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
        }

        [UnityTest]
        public IEnumerator EndingViewPauseDisableReloadAndRecreationPreserveStoppedRun()
        {
            var previousBackground = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN011-Lifecycle-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                var path = Path.Combine(directory, "view.json"); var catalog = EndingCatalog();
                var codec = new SessionSnapshotCodec(catalog, true);
                var flow = new GameFlowManager(codec, new JsonFileSessionStore(path), () => LaboratorySessionFactory.Create(
                    (ResourceData)catalog[0], (FacilityData)catalog[2], (FacilityData)catalog[3], (CreatureData)catalog[6], (ResourceData)catalog[1]));
                Assert.That(flow.StartOrContinue(), Is.True);
                // Rare-boundary fixture, separate from the two actual paid runs above.
                var invader = flow.Current.Registry.Create(catalog[8]);
                Assert.That(flow.Current.Map.Commands.Place(invader.Id, Vector2Int.one).Succeeded, Is.True);
                Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True); flow.Advance(1000);
                Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
                yield return LoadIsolatedScene(path, 5, loaded => { view = loaded; view.SimulationRate = 0; }, keepFocused: false);
                view.SendMessage("OnApplicationFocus", true); view.AdvanceFrame(0, 0); view.AdvanceFrame(0, 0.1);
                var elapsed = view.Ending.Elapsed; var old = view.Ending.Request;
                view.enabled = false; view.AdvanceFrame(10000, 10000);
                Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed));
                Assert.That(view.Controller.FinishDestruction(old), Is.False);
                view.enabled = true; view.AdvanceFrame(10000, 10000);
                Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed));
                view.SendMessage("OnApplicationPause", true); view.AdvanceFrame(10000, 10000);
                view.SendMessage("OnApplicationFocus", false); view.SendMessage("OnApplicationPause", false);
                view.AdvanceFrame(10000, 10000); Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed));
                view.SendMessage("OnApplicationFocus", true); view.AdvanceFrame(10000, 10000);
                Assert.That(view.Ending.Elapsed, Is.EqualTo(elapsed));
                var current = view.Controller.Current; var request = view.Ending.Request;
                using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    Assert.That(view.Reload(), Is.False); Assert.That(view.Controller.Current, Is.SameAs(current));
                    Assert.That(view.Ending.Request, Is.SameAs(request));
                }
                Assert.That(view.Reload(), Is.True); Assert.That(view.Ending.Elapsed, Is.Zero);
                var oldFlow = view.Controller; var stale = view.Ending.Request;
                UnityEngine.Object.DestroyImmediate(view.gameObject); view = null;
                yield return LoadIsolatedScene(path, 5, loaded => { view = loaded; view.SimulationRate = 0; });
                view.SendMessage("OnApplicationFocus", true);
                Assert.That(oldFlow.FinishDestruction(stale), Is.False);
                Assert.That(view.Controller.FinishDestruction(stale), Is.False);
                yield return WaitFor(() => view.Controller.Phase == SessionPhase.Results, "Restored cinematic did not finish.");
                Assert.That(view.Reload(), Is.True); Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
#endif
