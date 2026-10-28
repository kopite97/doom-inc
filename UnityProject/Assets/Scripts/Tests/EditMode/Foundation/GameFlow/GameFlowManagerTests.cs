using MobileGame.TestSupport;
using System;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class GameFlowManagerTests : FoundationTestFixture
    {
        private FacilityData storage;
        private CreatureData creature;
        private ResourceData resource;
        private SessionSnapshotCodec codec;
        private string directory;
        private string slot;

        [SetUp]
        public void SetUp()
        {
            storage = CreateStorageDefinition();
            creature = CreateDefinition<CreatureData>("fixture.creature", ThingComponentKind.ResourceContainer);
            resource = CreateResourceDefinition();
            codec = new SessionSnapshotCodec(new ThingData[] { storage, creature, resource });
            directory = Path.Combine(Path.GetTempPath(), "DoomInc.SessionTests", Guid.NewGuid().ToString("N"));
            slot = Path.Combine(directory, "session.json");
        }

        [TearDown]
        public void RemoveTestSlot()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        private SessionState Initial()
        {
            var registry = new ThingRegistry();
            var map = new GridMap(registry, 8, 8);
            var first = registry.Create(storage);
            var second = registry.Create(storage);
            var worker = registry.Create(creature);
            registry.Create(creature); // An existing Thing intentionally outside the Map.
            registry.CreateResource(resource, 7, first.GetComponent<ResourceContainerComponent>());
            Assert.That(map.Commands.Place(first.Id, new Vector2Int(1, 2)).Succeeded, Is.True);
            Assert.That(map.Commands.Place(second.Id, new Vector2Int(5, 2)).Succeeded, Is.True);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(3, 0)).Succeeded, Is.True);
            return new SessionState(registry, map);
        }

        private GameFlowManager Controller(ISessionStore store = null, Func<SessionState> factory = null)
            => new GameFlowManager(codec, store ?? new JsonFileSessionStore(slot), factory ?? Initial, 2);

        private static Thing FirstFacility(SessionState state) => state.Registry.Things.First(t => t.Definition.Kind == ThingKind.Facility);
        private static Thing Stack(SessionState state) => state.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Resource);

        [Test]
        public void PreparationDoesNotAdvanceAndFreshStartPublishesOnlyAfterSaving()
        {
            var controller = Controller();
            var advanced = 0d;
            controller.LaboratoryAdvanced += delta => advanced += delta;
            controller.Advance(100);
            Assert.That(controller.Current, Is.Null);
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Preparing));
            Assert.That(advanced, Is.Zero);
            Assert.That(controller.ReachGoal(), Is.False);
            Assert.That(controller.BeginPlacement(Guid.NewGuid()), Is.False);
            Assert.That(controller.StartOrContinue(), Is.True, controller.LastError);
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Playing));
            Assert.That(File.Exists(slot), Is.True);
            Assert.That(controller.SuccessfulSaves, Is.EqualTo(1));
            Assert.That(controller.CanAdvanceLaboratory, Is.True);
        }

        [Test]
        public void EditingPausesOnlyLaboratoryAndSaveRestoresCommittedPlacementWithoutEdit()
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var lab = 0d;
            var outside = 0d;
            controller.LaboratoryAdvanced += delta => lab += delta;
            controller.ExternalAdvanced += delta => outside += delta;
            var facility = FirstFacility(controller.Current);
            var original = facility.Placement.Origin;
            Assert.That(controller.BeginPlacement(facility.Id), Is.True);
            Assert.That(controller.PreviewPlacement(new Vector2Int(2, 5), QuarterTurn.Clockwise90), Is.True);
            controller.Advance(2); // Actual time-triggered automatic save, with an uncommitted candidate.
            Assert.That(lab, Is.Zero);
            Assert.That(outside, Is.EqualTo(2));
            var restored = Controller();
            Assert.That(restored.StartOrContinue(), Is.True, restored.LastError);
            Assert.That(restored.Current.Map.Commands.IsEditing, Is.False);
            Assert.That(restored.Current.Map.GetPlacements().Single(p => p.ThingId == facility.Id).Origin, Is.EqualTo(original));
            Assert.That(restored.CanAdvanceLaboratory, Is.True);
            Assert.That(controller.ConfirmPlacement(), Is.True);
            controller.Advance(1);
            Assert.That(lab, Is.EqualTo(1));
            Assert.That(outside, Is.EqualTo(3));
        }

        [Test]
        public void GoalStopsBothSystemsAndCancellingEditCannotResumeResults()
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var lab = 0d;
            var outside = 0d;
            controller.LaboratoryAdvanced += delta => lab += delta;
            controller.ExternalAdvanced += delta => outside += delta;
            Assert.That(controller.FinishDestruction(), Is.False);
            Assert.That(controller.BeginPlacement(FirstFacility(controller.Current).Id), Is.True);
            Assert.That(controller.ReachGoal(), Is.True);
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(controller.ConfirmPlacement(), Is.False);
            Assert.That(controller.CancelPlacement(), Is.True);
            controller.Advance(10);
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(controller.FinishDestruction(), Is.True);
            controller.Advance(10);
            Assert.That(controller.ReachGoal(), Is.False);
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(lab, Is.Zero);
            Assert.That(outside, Is.Zero);
        }

        [TestCase(SessionPhase.Playing)]
        [TestCase(SessionPhase.Destruction)]
        [TestCase(SessionPhase.Results)]
        public void ContinueRebuildsIndependentObjectsInSavedPhaseWithoutOfflineProgress(SessionPhase phase)
        {
            var first = Controller();
            Assert.That(first.StartOrContinue(), Is.True);
            first.Current.Registry.ConsumeResource(Stack(first.Current), 2);
            if (phase != SessionPhase.Playing) first.ReachGoal();
            if (phase == SessionPhase.Results) first.FinishDestruction();
            Assert.That(first.Save(), Is.True);
            File.SetLastWriteTimeUtc(slot, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var restored = Controller(factory: () => throw new InvalidOperationException("Saved sessions must not use the initial factory."));
            var elapsed = 0d;
            restored.LaboratoryAdvanced += delta => elapsed += delta;
            restored.ExternalAdvanced += delta => elapsed += delta;
            Assert.That(restored.StartOrContinue(), Is.True, restored.LastError);
            Assert.That(restored.Phase, Is.EqualTo(phase));
            Assert.That(elapsed, Is.Zero);
            Assert.That(Stack(restored.Current).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(restored.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(first.Current.Registry.Things.Select(t => t.Id)));
            Assert.That(Stack(restored.Current), Is.Not.SameAs(Stack(first.Current)));
            Assert.That(restored.Current.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature && !t.IsPlaced), Is.EqualTo(1));
        }

        [Test]
        public void CorruptAndInvalidSavesDoNotBecomeFreshGamesOrReplaceCurrentState()
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var old = controller.Current;
            var valid = File.ReadAllText(slot);
            var invalidGraph = codec.Deserialize(valid);
            invalidGraph.things.Single(t => t.quantity > 0).holderId = Guid.NewGuid().ToString("N");
            foreach (var invalid in new[] { "broken JSON", "{}", codec.Serialize(invalidGraph) })
            {
                File.WriteAllText(slot, invalid);
                var fresh = Controller(factory: () => throw new Exception("Factory must not run for corrupt saves."));
                Assert.That(fresh.StartOrContinue(), Is.False);
                Assert.That(fresh.Current, Is.Null);
                Assert.That(fresh.Phase, Is.EqualTo(SessionPhase.Preparing));
                Assert.That(controller.Reload(), Is.False);
                Assert.That(controller.Current, Is.SameAs(old));
                Assert.That(File.ReadAllText(slot), Is.EqualTo(invalid));
            }
        }

        [Test]
        public void AutomaticAndLifecycleSavingUseActualFilesAndPauseDoesNotAccumulateTime()
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var firstSave = File.ReadAllText(slot);
            controller.Current.Registry.ConsumeResource(Stack(controller.Current), 1);
            controller.Advance(1);
            Assert.That(File.ReadAllText(slot), Is.EqualTo(firstSave));
            controller.Advance(1);
            Assert.That(File.ReadAllText(slot), Is.Not.EqualTo(firstSave));
            var continued = Controller();
            Assert.That(continued.StartOrContinue(), Is.True);
            Assert.That(Stack(continued.Current).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(6));
            controller.Current.Registry.ConsumeResource(Stack(controller.Current), 1);
            controller.NotifyApplicationPause(true);
            var savedAtPause = controller.SuccessfulSaves;
            var elapsed = 0d;
            controller.LaboratoryAdvanced += delta => elapsed += delta;
            controller.Advance(10000);
            Assert.That(elapsed, Is.Zero);
            Assert.That(controller.SuccessfulSaves, Is.EqualTo(savedAtPause));
            Assert.That(continued.Reload(), Is.True);
            Assert.That(Stack(continued.Current).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            controller.NotifyApplicationPause(false);
            controller.Advance(1);
            Assert.That(elapsed, Is.EqualTo(1));
            controller.Current.Registry.ConsumeResource(Stack(controller.Current), 1);
            controller.NotifyApplicationQuit();
            Assert.That(continued.Reload(), Is.True);
            Assert.That(Stack(continued.Current).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(4));
        }

        [Test]
        public void FailedRestartPreservesSessionAndFileWhileLaterSuccessClearsOldIdsAndEdits()
        {
            var store = new FailingStore(new JsonFileSessionStore(slot));
            var controller = Controller(store);
            Assert.That(controller.StartOrContinue(), Is.True);
            var old = controller.Current;
            var oldIds = old.Registry.Things.Select(t => t.Id).ToArray();
            Assert.That(controller.BeginPlacement(FirstFacility(old).Id), Is.True);
            var before = File.ReadAllText(slot);
            store.FailWrites = true;
            Assert.That(controller.Restart(), Is.False);
            Assert.That(controller.Current, Is.SameAs(old));
            Assert.That(controller.Current.Map.Commands.IsEditing, Is.True);
            Assert.That(File.ReadAllText(slot), Is.EqualTo(before));
            Assert.That(controller.LastSaveError, Is.Not.Empty);
            store.FailWrites = false;
            Assert.That(controller.Restart(), Is.True);
            Assert.That(controller.Current, Is.Not.SameAs(old));
            Assert.That(controller.Current.Map.Commands.IsEditing, Is.False);
            Assert.That(controller.Current.Registry.Things.Select(t => t.Id).Intersect(oldIds), Is.Empty);
            var restored = Controller();
            Assert.That(restored.StartOrContinue(), Is.True);
            Assert.That(restored.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(controller.Current.Registry.Things.Select(t => t.Id)));
        }

        [Test]
        public void FailedAutoSaveKeepsSessionAndLastGoodFileAndRetriesOnNextInterval()
        {
            var store = new FailingStore(new JsonFileSessionStore(slot));
            var controller = Controller(store);
            Assert.That(controller.StartOrContinue(), Is.True);
            var current = controller.Current;
            var before = File.ReadAllText(slot);
            current.Registry.ConsumeResource(Stack(current), 1);
            store.FailWrites = true;
            controller.Advance(2);
            Assert.That(controller.Current, Is.SameAs(current));
            Assert.That(File.ReadAllText(slot), Is.EqualTo(before));
            Assert.That(controller.LastSaveError, Is.Not.Empty);
            Assert.That(controller.ReachGoal(), Is.True);
            Assert.That(controller.CanAdvanceExternal, Is.False, "Disk failure must not let the finished world advance.");
            store.FailWrites = false;
            controller.Advance(2);
            Assert.That(controller.LastSaveError, Is.Null);
            var restored = Controller();
            Assert.That(restored.StartOrContinue(), Is.True);
            Assert.That(restored.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(Stack(restored.Current).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(6));
        }

        [Test]
        public void PostCommitObserverFailureIsNotReportedAsFailedPersistence()
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var old = controller.Current;
            controller.SessionChanged += () => throw new InvalidOperationException("Broken presentation observer.");
            Assert.That(() => controller.Restart(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(controller.Current, Is.Not.SameAs(old));
            Assert.That(controller.LastSaveError, Is.Null);
            Assert.That(controller.LastError, Is.Null);
            var restored = Controller();
            Assert.That(restored.StartOrContinue(), Is.True);
            Assert.That(restored.Current.Registry.Things.Select(t => t.Id), Is.EquivalentTo(controller.Current.Registry.Things.Select(t => t.Id)));
            Assert.That(() => controller.StartOrContinue(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(controller.LastSaveError, Is.Null);
            Assert.That(controller.LastError, Is.Null);
        }

        [TestCase(-1d)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void InvalidTimeInputDoesNotAdvanceOrSave(double seconds)
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True);
            var before = File.ReadAllText(slot);
            Assert.That(() => controller.Advance(seconds), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(File.ReadAllText(slot), Is.EqualTo(before));
        }

        private sealed class FailingStore : ISessionStore
        {
            private readonly ISessionStore inner;
            public FailingStore(ISessionStore inner) { this.inner = inner; }
            public bool FailWrites { get; set; }
            public bool Exists => inner.Exists;
            public string Read() => inner.Read();
            public void Write(string json)
            {
                if (FailWrites) throw new IOException("Injected unavailable storage.");
                inner.Write(json);
            }
        }
    }
}
