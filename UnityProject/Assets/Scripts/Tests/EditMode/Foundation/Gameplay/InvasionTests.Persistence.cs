using System;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class InvasionTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2.35)]
        [TestCase(4.99)]
        [TestCase(5)]
        [TestCase(80)]
        public void FileCheckpointsPreserveWholeGraphAndContinueOnce(double seconds)
        {
            var session = Fresh(); var first = AddInvader(session); var store = new FaultStore(Slot());
            var flow = Flow(session, store);
            if (seconds >= 0)
            {
                Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True);
                flow.Advance(seconds); AddInvader(session); // New waiting + ongoing external coexist.
            }
            Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            var saved = store.DurableRead(); var old = flow.Current;
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            Assert.That(Capture(flow), Is.EqualTo(saved));
            Assert.That(flow.Current.Registry, Is.Not.SameAs(old.Registry));
            Assert.That(flow.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(old.Registry.Things.Select(t => t.Id)));
            var control = Flow(codec.Restore(codec.Deserialize(saved)));
            flow.Advance(0.07); control.Advance(0.07);
            Assert.That(Capture(flow), Is.EqualTo(Capture(control)));
            Assert.That(first.GetComponent<InvaderComponent>().Progress,
                Is.EqualTo(seconds < 0 ? 0 : seconds % 5).Within(1e-7), "Old objects must not receive new-session time.");
        }

        [TestCase(2, SessionPhase.Playing)]
        [TestCase(2, SessionPhase.Destruction)]
        [TestCase(2, SessionPhase.Results)]
        [TestCase(3, SessionPhase.Playing)]
        [TestCase(3, SessionPhase.Destruction)]
        [TestCase(3, SessionPhase.Results)]
        public void LegacyPhaseMigratesAndResavesWithoutInventingAchievements(int version, SessionPhase phase)
        {
            var session = Fresh();
            if (version == 3) AddInvader(session);
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
            var flow = new GameFlowManager(codec, store, () => Fresh());
            Assert.That(flow.StartOrContinue(), Is.True, flow.LastError);
            Assert.That(flow.Phase, Is.EqualTo(phase));
            Assert.That(flow.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(session.Registry.Things.Select(t => t.Id)));
            var state = Lab(flow.Current).Invasion.State;
            Assert.That(state.Score, Is.Zero); Assert.That(state.Reports, Is.Empty);
            Assert.That(Lab(flow.Current).Invasion.ExternalCount, Is.Zero);
            Assert.That(state.LegacyEndVersion, Is.EqualTo(phase == SessionPhase.Playing ? 0 : version));
            Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            Assert.That(codec.Deserialize(store.Read()).version, Is.EqualTo(4));
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            Assert.That(flow.Phase, Is.EqualTo(phase));
            if (phase != SessionPhase.Playing)
            {
                var before = Capture(flow); flow.Advance(1000);
                Assert.That(Capture(flow), Is.EqualTo(before));
                Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.False);
            }
        }

        [TestCase("missing")]
        [TestCase("duplicate")]
        [TestCase("unknown")]
        [TestCase("wrong-model")]
        [TestCase("outside-placement")]
        [TestCase("waiting-outside")]
        [TestCase("progress")]
        [TestCase("score")]
        [TestCase("report")]
        [TestCase("report-round")]
        [TestCase("phase")]
        [TestCase("goal-flag")]
        [TestCase("legacy-forgery")]
        [TestCase("version")]
        public void InvalidCandidateDoesNotPublishOrOverwriteGoodSessionAndFile(string fault)
        {
            var session = Fresh(); AddInvader(session); var store = new FaultStore(Slot()); var flow = Flow(session, store);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True); flow.Advance(7);
            Assert.That(flow.Save(), Is.True);
            var good = store.DurableRead(); var bad = codec.Deserialize(good);
            switch (fault)
            {
                case "missing": bad.invasion.invaders = Array.Empty<InvaderSnapshot>(); break;
                case "duplicate": bad.invasion.invaders = new[] { bad.invasion.invaders[0], bad.invasion.invaders[0] }; break;
                case "unknown": bad.invasion.invaders[0].thingId = Guid.NewGuid().ToString("N"); break;
                case "wrong-model": bad.invasion.invaders[0].thingId = session.Registry.Things.Single(t => t.Definition == prototype).Id.ToString("N"); break;
                case "outside-placement": bad.placements = bad.placements.Concat(new[] { new PlacementSnapshot { thingId = bad.invasion.invaders[0].thingId, x = 1, y = 1 } }).ToArray(); break;
                case "waiting-outside": bad.invasion.invaders[0].isExternal = false; break;
                case "progress": bad.invasion.invaders[0].progress = "NaN"; break;
                case "score": bad.invasion.score++; break;
                case "report": bad.invasion.reports[0].sequence = 99; break;
                case "report-round": bad.invasion.reports[0].activity = 2; break;
                case "phase": bad.phase = SessionPhase.Destruction; break;
                case "goal-flag": bad.invasion.goalReached = true; break;
                case "legacy-forgery": bad.phase = SessionPhase.Results; bad.invasion.legacyEndVersion = 3; break;
                case "version": bad.version = 999; break;
            }
            store.ReadOverride = JsonUtility.ToJson(bad);
            Assert.That(flow.Reload(), Is.False, fault);
            Assert.That(flow.Current, Is.SameAs(session));
            Assert.That(Capture(flow), Is.EqualTo(good));
            Assert.That(store.DurableRead(), Is.EqualTo(good));
        }

        [Test]
        public void PlayingGoalReachedAndIncorrectFlagAreRejectedAfterRealGoal()
        {
            var session = Fresh(Settings(2)); AddInvader(session); var store = new FaultStore(Slot()); var flow = Flow(session, store);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True); flow.Advance(5);
            var good = store.DurableRead(); var bad = codec.Deserialize(good); bad.phase = SessionPhase.Playing;
            store.ReadOverride = JsonUtility.ToJson(bad);
            Assert.That(flow.Reload(), Is.False); Assert.That(flow.Current, Is.SameAs(session));
            bad.phase = SessionPhase.Destruction; bad.invasion.goalReached = false;
            store.ReadOverride = JsonUtility.ToJson(bad);
            Assert.That(flow.Reload(), Is.False); Assert.That(Capture(flow), Is.EqualTo(good));
            Assert.That(store.DurableRead(), Is.EqualTo(good));
        }

        [Test]
        public void FailedGoalSaveStopsAndRetryCommitsWholeActivityWhileFailedRestartPreservesIt()
        {
            var store = new FaultStore(Slot());
            var flow = new GameFlowManager(codec, store, () => Fresh(Settings(2)), 5);
            Assert.That(flow.StartOrContinue(), Is.True);
            AddInvader(flow.Current); Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True);
            Assert.That(flow.Save(), Is.True); var before = store.DurableRead();
            store.FailWrite = true; flow.Advance(5);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(flow.LastSaveError, Is.Not.Null.Or.Empty);
            Assert.That(store.DurableRead(), Is.EqualTo(before));
            var current = flow.Current; var stopped = Capture(flow); flow.Advance(100);
            Assert.That(Capture(flow), Is.EqualTo(stopped));
            Assert.That(flow.Restart(), Is.False); Assert.That(flow.Current, Is.SameAs(current));
            store.FailWrite = false; flow.Advance(5); // Existing timed persistence retry, without simulation.
            Assert.That(flow.LastSaveError, Is.Null);
            Assert.That(store.DurableRead(), Is.EqualTo(stopped));
            Assert.That(flow.Reload(), Is.True); Assert.That(Capture(flow), Is.EqualTo(stopped));
            Assert.That(flow.Restart(), Is.True); Assert.That(flow.Reload(), Is.True);
            Assert.That(flow.Current.Registry.Things.Select(t => t.Id).Intersect(current.Registry.Things.Select(t => t.Id)), Is.Empty);
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.Zero);
            Assert.That(Lab(flow.Current).Invasion.State.Reports, Is.Empty);
            Assert.That(Lab(flow.Current).Invasion.ExternalCount, Is.Zero);
            Assert.That(Lab(flow.Current).Invasion.State.Elapsed, Is.Zero);
            Assert.That(flow.Current.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
            Assert.That(flow.Current.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
        }

        [Test]
        public void PaidInitialProductionReleasesTwoBatchesAndReachesGoal()
        {
            var flow = Flow(Fresh()); var commands = new LaboratoryCommands(flow); var lab = Lab(flow.Current);
            Build(commands, gathering, 3, 3); Build(commands, storage, 8, 3);
            Until(flow, () => lab.Registry.Things.Any(t => t.Definition == storage && lab.TotalAt(t.Id) >= 5), 120);
            Build(commands, processing, 3, 7); Build(commands, cultivation, 8, 7); Build(commands, storage, 12, 3);
            var stores = lab.Registry.Things.Where(t => t.Definition == storage).OrderBy(t => t.CreationOrder).ToArray();
            Assert.That(commands.SetAllowed(stores[0].Id, culture, false), Is.True);
            Assert.That(commands.SetAllowed(stores[1].Id, organic, false), Is.True);
            var processor = lab.Registry.Things.Single(t => t.Definition == processing);
            var cultivator = lab.Registry.Things.Single(t => t.Definition == cultivation);
            Until(flow, () => lab.IsComplete(processor) && lab.IsComplete(cultivator), 150);
            Assert.That(commands.SelectCultivation(cultivator.Id, CultivationTarget.Support), Is.True);
            Until(flow, () => lab.Registry.Things.Any(t => t.Definition == support), 800);
            var helper = lab.Registry.Things.Single(t => t.Definition == support);
            Assert.That(commands.AssignWorker(helper.Id, processor.Id), Is.True);
            Assert.That(commands.SetWorkerPriorities(helper.Id, new[] { 2, 3, 4, 1 }), Is.True);
            Assert.That(commands.SelectCultivation(cultivator.Id, CultivationTarget.Invader), Is.True);
            Until(flow, () => lab.Invasion.WaitingCount == 1, 800);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            Assert.That(lab.Invasion.State.Score, Is.Zero);
            Until(flow, () => lab.Invasion.WaitingCount >= 1, 150);
            Assert.That(lab.Invasion.ExternalCount, Is.EqualTo(1));
            Assert.That(lab.Invasion.State.Score, Is.GreaterThan(0));
            Assert.That(commands.ReleaseInvaders(), Is.True);
            Assert.That(lab.Invasion.ExternalCount, Is.GreaterThanOrEqualTo(2));
            Until(flow, () => flow.Phase == SessionPhase.Destruction, 200);
            Assert.That(flow.LastSaveError, Is.Null.Or.Empty);
            Assert.That(lab.Invasion.State.Reports.Count, Is.GreaterThan(0));
            var stopped = Capture(flow);
            Assert.That(flow.Reload(), Is.True, flow.LastError); Assert.That(Capture(flow), Is.EqualTo(stopped));
            flow.Advance(100); Assert.That(Capture(flow), Is.EqualTo(stopped));
        }

        private static void Build(LaboratoryCommands commands, FacilityDefinition definition, int x, int y)
        {
            Assert.That(commands.BeginConstruction(definition), Is.True);
            Assert.That(commands.PreviewConstruction(new Vector2Int(x, y), QuarterTurn.None), Is.True);
            Assert.That(commands.ConfirmConstruction(), Is.True);
        }
        private static void Until(GameFlowManager flow, Func<bool> condition, double maximum)
        {
            for (var step = 0; step < maximum * 20 && !condition(); step++) flow.Advance(0.05);
            Assert.That(condition(), Is.True, "Invasion scenario exceeded " + maximum + " game seconds.");
        }
    }
}
