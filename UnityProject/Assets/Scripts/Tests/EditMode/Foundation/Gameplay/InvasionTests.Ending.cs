using MobileGame.TestSupport;
using System;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class InvasionTests
    {
        private GameFlowManager EndingFlow(out FaultStore store, long target = 2)
        {
            store = new FaultStore(Slot());
            var flow = new GameFlowManager(codec, store, () => Fresh(Settings(target)));
            Assert.That(flow.StartOrContinue(), Is.True);
            AddInvader(flow.Current);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True);
            return flow;
        }

        private static void FinishVisual(RunEndingPresentation visual, GameFlowManager flow)
        {
            for (var i = 0; i < 100 && flow.Phase == SessionPhase.Destruction; i++) visual.Advance(0.1);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Results));
        }

        [Test]
        public void EndingClockHasVisibleEntryBoundaryAndTwoAutomaticFiles()
        {
            var flow = EndingFlow(out var store); using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds);
            flow.Advance(10000); visual.Advance(10000);
            Assert.That(visual.Elapsed, Is.Zero, "The goal frame cannot consume presentation time.");
            Assert.That(codec.Deserialize(store.Read()).phase, Is.EqualTo(SessionPhase.Destruction));
            var frozen = JsonUtility.ToJson(codec.Capture(flow.Current, flow.Phase).laboratory);
            var invasion = JsonUtility.ToJson(codec.Capture(flow.Current, flow.Phase).invasion);
            var saves = flow.SuccessfulSaves;
            for (var i = 0; i < 39; i++) visual.Advance(0.1);
            visual.Advance(0.099);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(visual.Elapsed, Is.EqualTo(3.999).Within(1e-10));
            visual.Advance(0.001);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(flow.SuccessfulSaves, Is.EqualTo(saves + 1));
            var persisted = codec.Deserialize(store.Read()); // No extra Save: actual completion must write Results.
            Assert.That(persisted.phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(JsonUtility.ToJson(persisted.invasion), Is.EqualTo(invasion));
            var restored = codec.Restore(persisted);
            Assert.That(Lab(restored).Invasion.State.Score, Is.EqualTo(2));
            Assert.That(JsonUtility.ToJson(codec.Capture(flow.Current, flow.Phase).laboratory), Is.EqualTo(frozen));
            flow.Advance(1000); visual.Advance(1000);
            Assert.That(JsonUtility.ToJson(codec.Capture(flow.Current, flow.Phase).invasion), Is.EqualTo(invasion));
            Assert.That(new LaboratoryCommands(flow).BeginConstruction(gathering), Is.False);
            Assert.That(flow.FinishDestruction(), Is.False, "The unqualified signal remains diagnostic-only.");
        }

        [Test]
        public void EndingPauseVisibilityAndStallsDoNotCatchUp()
        {
            var flow = EndingFlow(out _); flow.Advance(5);
            using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds);
            visual.Advance(0); visual.Advance(1000);
            Assert.That(visual.Elapsed, Is.EqualTo(0.1));
            var stale = visual.Request;
            visual.SetVisible(false); visual.Advance(10000);
            Assert.That(flow.FinishDestruction(stale), Is.False);
            visual.SetVisible(true); visual.Advance(10000);
            Assert.That(visual.Elapsed, Is.EqualTo(0.1));
            flow.NotifyApplicationPause(true); visual.Advance(10000);
            Assert.That(flow.FinishDestruction(visual.Request), Is.False);
            flow.NotifyApplicationPause(false); visual.Advance(10000);
            Assert.That(visual.Elapsed, Is.EqualTo(0.1));
            visual.Advance(0.1);
            Assert.That(visual.Elapsed, Is.EqualTo(0.2));
            FinishVisual(visual, flow);
        }

        [Test]
        public void OldScreensReloadedSessionsAndDuplicateRequestsCannotFinishOrRestartAnotherRun()
        {
            var flow = EndingFlow(out _);
            Assert.That(flow.AcquireEndingRequest(null), Is.Null);
            Assert.That(flow.FinishDestruction(null), Is.False);
            flow.Advance(5);
            using var first = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds); var original = first.Request;
            using var replacement = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds);
            Assert.That(flow.FinishDestruction(original), Is.False, "Same-session screen replacement revokes old requests.");
            first.Advance(1000); Assert.That(flow.IsCurrentEndingRequest(replacement.Request), Is.True);
            var beforeReload = replacement.Request;
            Assert.That(flow.Reload(), Is.True); replacement.Synchronize();
            first.Advance(1000); first.SetVisible(false); first.SetVisible(true);
            Assert.That(flow.IsCurrentEndingRequest(replacement.Request), Is.True, "A superseded view must not reacquire ownership after Reload.");
            Assert.That(flow.FinishDestruction(beforeReload), Is.False);
            var beforeFinish = replacement.Request;
            FinishVisual(replacement, flow);
            Assert.That(flow.FinishDestruction(beforeFinish), Is.False);
            var results = replacement.Request; var oldIds = flow.Current.Registry.Things.Select(t => t.Id).ToArray();
            Assert.That(replacement.Restart(results), Is.True);
            Assert.That(replacement.Restart(results), Is.False);
            Assert.That(flow.RestartFromResults(results), Is.False);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Playing));
            Assert.That(flow.Current.Registry.Things.Select(t => t.Id).Intersect(oldIds), Is.Empty);
            AddInvader(flow.Current); new LaboratoryCommands(flow).ReleaseInvaders(); flow.Advance(5);
            replacement.Synchronize();
            Assert.That(flow.FinishDestruction(beforeFinish), Is.False, "Even a later Destruction phase rejects the old run.");
            FinishVisual(replacement, flow);
            Assert.That(flow.RestartFromResults(results), Is.False, "Old result input cannot reset a second completed run.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EndingSaveFailuresKeepMemoryPhaseAndGoodFileThenRetry(bool failGoal)
        {
            var flow = EndingFlow(out var store); Assert.That(flow.Save(), Is.True);
            var playing = store.Read(); store.FailWrite = failGoal;
            flow.Advance(5);
            if (failGoal) Assert.That(store.DurableRead(), Is.EqualTo(playing));
            var goalFile = store.DurableRead(); store.FailWrite = true;
            using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds); FinishVisual(visual, flow);
            Assert.That(flow.LastSaveError, Is.Not.Null.Or.Empty);
            Assert.That(store.DurableRead(), Is.EqualTo(goalFile));
            Assert.That(flow.CanAdvanceExternal || flow.CanAdvanceLaboratory, Is.False);
            var current = flow.Current; var request = visual.Request;
            Assert.That(visual.Restart(request), Is.False);
            Assert.That(flow.Current, Is.SameAs(current)); Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Results));
            store.FailWrite = false; flow.Advance(5);
            Assert.That(flow.LastSaveError, Is.Null);
            Assert.That(codec.Deserialize(store.Read()).phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(visual.Restart(request), Is.True);
            Assert.That(flow.Reload(), Is.True);
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.Zero);
            Assert.That(Lab(flow.Current).Invasion.State.Elapsed, Is.Zero);
            Assert.That(Lab(flow.Current).Invasion.State.Reports, Is.Empty);
            Assert.That(flow.Current.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
            Assert.That(flow.Current.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
        }

        [Test]
        public void EndingReloadFailurePreservesScreenClockAndRestartLease()
        {
            var flow = EndingFlow(out var store); flow.Advance(5);
            using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds); visual.Advance(0); visual.Advance(0.1);
            var request = visual.Request; var current = flow.Current; var file = store.DurableRead();
            store.ReadOverride = "broken";
            Assert.That(flow.Reload(), Is.False); visual.Synchronize();
            Assert.That(flow.Current, Is.SameAs(current)); Assert.That(visual.Request, Is.SameAs(request));
            Assert.That(visual.Elapsed, Is.EqualTo(0.1)); Assert.That(store.DurableRead(), Is.EqualTo(file));
            store.ReadOverride = null;
            Assert.That(flow.Reload(), Is.True); visual.Synchronize();
            Assert.That(visual.Elapsed, Is.Zero); Assert.That(visual.Request, Is.Not.SameAs(request));
            FinishVisual(visual, flow); Assert.That(flow.Reload(), Is.True); visual.Synchronize();
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Results));
        }

        [TestCase(2, SessionPhase.Destruction)]
        [TestCase(2, SessionPhase.Results)]
        [TestCase(3, SessionPhase.Destruction)]
        [TestCase(3, SessionPhase.Results)]
        public void LegacyEndingReplaysOrOpensResultsWithoutInventedActivity(int version, SessionPhase phase)
        {
            var session = Fresh();
            if (version == 2)
            {
                Lab(session).SetPreset(new[] { 1, 2, 3 });
                foreach (var worker in session.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out _)))
                    Lab(session).SetWorkerPriorities(worker.Id, new[] { 1, 2, 3 }, true);
            }
            var saved = codec.Capture(session, SessionPhase.Playing);
            saved.version = version; saved.phase = phase; saved.invasion = null;
            if (version == 2) saved.laboratory.cultureDefinitionId = null;
            var store = new FaultStore(Slot()); store.Write(codec.Serialize(saved));
            var flow = new GameFlowManager(codec, store, () => Fresh()); Assert.That(flow.StartOrContinue(), Is.True);
            using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds);
            Assert.That(flow.Phase, Is.EqualTo(phase)); FinishVisual(visual, flow);
            Assert.That(flow.Save(), Is.True); Assert.That(flow.Reload(), Is.True); visual.Synchronize();
            Assert.That(Lab(flow.Current).Invasion.State.LegacyEndVersion, Is.EqualTo(version));
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.Zero);
            Assert.That(Lab(flow.Current).Invasion.State.ActivitySequence, Is.Zero);
            Assert.That(visual.Restart(visual.Request), Is.True);
            Assert.That(Lab(flow.Current).Invasion.State.LegacyEndVersion, Is.Zero);
        }

        [Test]
        public void ResultsRetainOvershootAndFilteredReportTotals()
        {
            var flow = EndingFlow(out var store, 3); AddInvader(flow.Current);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True); flow.Advance(5);
            using var visual = new RunEndingPresentation(flow, DataTestAssets.Catalog.Settings.EndingSeconds); FinishVisual(visual, flow);
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.EqualTo(4));
            var frozen = store.Read();
            for (var i = 0; i < 20; i++) { var ignored = Lab(flow.Current).Invasion.State.Reports.Count; visual.Advance(1); }
            Assert.That(Capture(flow), Is.EqualTo(frozen));
        }
    }
}
