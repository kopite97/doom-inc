using System;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryPersistenceTests : FoundationTestFixture
    {
        private ResourceDefinition organic;
        private CreatureDefinition prototype;
        private FacilityDefinition gathering;
        private FacilityDefinition storage;
        private SessionSnapshotCodec codec;
        private string directory;
        private string slot;

        [SetUp]
        public void Prepare()
        {
            organic = CreateResourceDefinition("persistence.organic");
            prototype = CreateDefinition<CreatureDefinition>("persistence.prototype", ThingComponentKind.ResourceContainer);
            gathering = GameplayFacility("persistence.gathering", FacilityRole.Gathering);
            storage = GameplayFacility("persistence.storage", FacilityRole.Storage);
            codec = new SessionSnapshotCodec(new ThingDefinition[] { organic, prototype, gathering, storage });
            directory = Path.Combine(Path.GetTempPath(), "DoomInc-LaboratoryPersistence", Guid.NewGuid().ToString("N"));
            slot = Path.Combine(directory, "session.json");
        }

        [TearDown]
        public void CleanSave()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        private FacilityDefinition GameplayFacility(string id, FacilityRole role)
        {
            var definition = CreateDefinition<FacilityDefinition>(id, ThingComponentKind.ResourceContainer);
            using (var serialized = new SerializedObject(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return definition;
        }

        // The two designated, unfinished sites are test setup. All delivery, work,
        // production and transportation below run through the production runtime.
        private SessionState PendingConstruction()
        {
            var registry = new ThingRegistry(GameplayComponentFactory.Attach);
            var map = new GridMap(registry, 10, 8);
            var worker = registry.Create(prototype);
            var gatherer = registry.Create(gathering);
            var store = registry.Create(storage);
            Require(map.Commands.Place(worker.Id, Vector2Int.zero));
            Require(map.Commands.Place(gatherer.Id, new Vector2Int(2, 2)));
            Require(map.Commands.Place(store.Id, new Vector2Int(6, 2)));
            registry.CreateResource(organic, gathering.BuildCost + storage.BuildCost,
                worker.GetComponent<ResourceContainerComponent>());
            return new SessionState(registry, map, new LaboratoryManager(registry, map, organic));
        }

        private GameFlowManager Controller(Func<SessionState> factory = null)
            => new GameFlowManager(codec, new JsonFileSessionStore(slot), factory ?? PendingConstruction, 10000);

        private SessionState RoundTrip(SessionState state)
        {
            var captured = codec.Capture(state, SessionPhase.Playing);
            Assert.That(captured.version, Is.EqualTo(SessionSnapshotCodec.CurrentVersion));
            var json = codec.Serialize(captured);
            new JsonFileSessionStore(slot).Write(json);
            var restored = codec.Restore(codec.Deserialize(new JsonFileSessionStore(slot).Read()));
            Assert.That(restored.Runtime, Is.TypeOf<LaboratoryManager>());
            Assert.That(restored.Registry, Is.Not.SameAs(state.Registry));
            Assert.That(restored.Map, Is.Not.SameAs(state.Map));
            Assert.That(restored.Runtime, Is.Not.SameAs(state.Runtime));
            Assert.That(codec.Serialize(codec.Capture(restored, SessionPhase.Playing)), Is.EqualTo(json),
                "Restore must preserve every authoritative state value without advancing it.");
            foreach (var thing in state.Registry.Things)
            {
                Assert.That(restored.Registry.TryGet(thing.Id, out var other), Is.True);
                Assert.That(other, Is.Not.SameAs(thing));
                Assert.That(other.Definition, Is.SameAs(thing.Definition));
            }
            return restored;
        }

        [TestCase("construction")]
        [TestCase("gathering")]
        [TestCase("reserved")]
        [TestCase("carried")]
        public void ActualWorkSnapshotsRestoreNewObjectsAndFinishWithoutDuplicateResources(string checkpoint)
        {
            var state = PendingConstruction();
            var simulation = (LaboratoryManager)state.Runtime;
            if (checkpoint == "reserved") PrepareRemoteCarrier(state);
            RunUntil(state, () => AtCheckpoint(state, checkpoint), checkpoint);
            var restored = RoundTrip(state);
            var loaded = (LaboratoryManager)restored.Runtime;
            RunUntil(state, () => Stored(state) >= 5, "original first admission");
            RunUntil(restored, () => Stored(restored) >= 5, "restored first admission");
            Assert.That(Stored(restored), Is.EqualTo(5));
            Assert.That(Stored(state), Is.EqualTo(Stored(restored)));
            Assert.That(TotalResource(restored), Is.EqualTo(5),
                "The two construction costs are consumed once and the first batch is produced once.");
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(loaded.Reservations, Is.Empty);
            loaded.ValidateState();
        }

        [Test]
        public void LegacyDiagnosticSaveRemainsVersionOneAndDoesNotGainGameplayComponents()
        {
            var definition = CreateStorageDefinition("legacy.storage");
            var registry = new ThingRegistry();
            var map = new GridMap(registry, 4, 4);
            var facility = registry.Create(definition);
            Require(map.Commands.Place(facility.Id, Vector2Int.one));
            var legacyCodec = new SessionSnapshotCodec(new[] { definition });
            var snapshot = legacyCodec.Capture(new SessionState(registry, map), SessionPhase.Results);
            Assert.That(snapshot.version, Is.EqualTo(1));
            var loaded = legacyCodec.Restore(legacyCodec.Deserialize(legacyCodec.Serialize(snapshot)));
            Assert.That(loaded.Runtime, Is.Null);
            Assert.That(loaded.Registry.TryGet(facility.Id, out var restored), Is.True);
            Assert.That(restored.TryGetComponent<ConstructionComponent>(out _), Is.False);
        }

        [Test]
        public void HeldBatchWithoutDestinationAndCustomSettingsSurviveUntilActualAdmission()
        {
            var state = PendingConstruction();
            RunUntil(state, () => AtCheckpoint(state, "carried"), "picked-up first batch");
            var simulation = (LaboratoryManager)state.Runtime;
            var worker = state.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
            var storageId = Facility(state, FacilityRole.Storage).Id;
            Assert.That(simulation.SetAllowed(storageId, false), Is.True);
            Assert.That(simulation.SetPreset(new[] { 3, 1, 2 }), Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 2, 3, 1 }), Is.True);
            Assert.That(simulation.Reservations, Is.Empty);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.Waiting));
            var restored = RoundTrip(state);
            var loaded = (LaboratoryManager)restored.Runtime;
            var restoredWorker = restored.Registry.Things.Single(t => t.Id == worker.Id).GetComponent<WorkerComponent>();
            Assert.That(restoredWorker.Priorities, Is.EqualTo(new[] { 2, 3, 1 }));
            Assert.That(restoredWorker.UsesPreset, Is.False);
            Assert.That(restoredWorker.Job.TargetId, Is.EqualTo(Guid.Empty));
            Assert.That(restoredWorker.Job.BatchOriginId, Is.EqualTo(Facility(restored, FacilityRole.Gathering).Id));
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(5));
            Assert.That(loaded.SetAllowed(storageId, true), Is.True);
            RunUntil(restored, () => Stored(restored) == 5, "held batch after restoring allowed storage");
            Assert.That(TotalResource(restored), Is.EqualTo(5));
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.Zero);
        }

        [Test]
        public void PartialAdmissionRestoresHeldRemainderAndCompletesTheSameBatchExactlyOnce()
        {
            var smallDefinition = GameplayFacility("persistence.small-storage", FacilityRole.Storage);
            using (var serialized = new SerializedObject(smallDefinition))
            {
                serialized.FindProperty("storageCapacity").intValue = 2;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            codec = new SessionSnapshotCodec(new ThingDefinition[] { organic, prototype, gathering, storage, smallDefinition });
            var state = PendingConstruction();
            var simulation = (LaboratoryManager)state.Runtime;
            var mainStore = Facility(state, FacilityRole.Storage);
            var worker = state.Registry.Things.Single(t => t.Definition == prototype);
            // Only this extra completed, initially disabled store is fixture setup.
            // The first two facilities, entire batch and every shipment use real work.
            var smallStore = state.Registry.Create(smallDefinition);
            Require(state.Map.Commands.Place(smallStore.Id, new Vector2Int(8, 6)));
            var construction = smallStore.GetComponent<ConstructionComponent>();
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Delivered))
                .SetValue(construction, smallDefinition.BuildCost);
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Progress))
                .SetValue(construction, smallDefinition.BuildSeconds);
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.IsComplete))
                .SetValue(construction, true);
            Assert.That(simulation.SetAllowed(smallStore.Id, false), Is.True);

            RunUntil(state, () => AtCheckpoint(state, "carried"), "whole first batch picked up");
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(5));
            Assert.That(simulation.SetAllowed(mainStore.Id, false), Is.True);
            Assert.That(simulation.SetAllowed(smallStore.Id, true), Is.True);
            RunUntil(state, () => simulation.TotalAt(smallStore.Id) == 2, "two-unit partial admission");
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(3));
            Assert.That(Facility(state, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(3));
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.Waiting));
            Assert.That(TotalResource(state), Is.EqualTo(5));

            var restored = RoundTrip(state);
            var loaded = (LaboratoryManager)restored.Runtime;
            Assert.That(loaded.TotalAt(smallStore.Id), Is.EqualTo(2));
            Assert.That(loaded.TotalAt(worker.Id), Is.EqualTo(3));
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(3));
            Assert.That(loaded.SetAllowed(mainStore.Id, true), Is.True);
            RunUntil(restored, () => loaded.TotalAt(mainStore.Id) == 3, "restored three-unit remainder admission");
            Assert.That(loaded.TotalAt(smallStore.Id), Is.EqualTo(2));
            Assert.That(loaded.TotalAt(worker.Id), Is.Zero);
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(TotalResource(restored), Is.EqualTo(5));
            Assert.That(loaded.Reservations, Is.Empty);
            loaded.ValidateState();
        }

        [Test]
        public void TransientConstructionPreviewIsNotSavedAndGameFlowGatesActualWork()
        {
            var invaderDefinition = CreateDefinition<CreatureDefinition>("persistence.invader", ThingComponentKind.ResourceContainer);
            using (var serialized = new SerializedObject(invaderDefinition))
            {
                serialized.FindProperty("role").intValue = (int)CreatureRole.Invader;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            codec = new SessionSnapshotCodec(new ThingDefinition[] { organic, prototype, gathering, storage, invaderDefinition });
            var controller = Controller(() => LaboratorySessionFactory.Create(organic, gathering, storage, prototype));
            Assert.That(controller.StartOrContinue(), Is.True, controller.LastError);
            var state = controller.Current;
            var simulation = (LaboratoryManager)state.Runtime;
            var commands = new LaboratoryCommands(controller);
            var initialIds = state.Registry.Things.Select(t => t.Id).ToArray();
            Assert.That(commands.BeginConstruction(gathering), Is.True);
            Assert.That(commands.PreviewConstruction(new Vector2Int(3, 3), QuarterTurn.None), Is.True);
            Assert.That(controller.CanAdvanceLaboratory, Is.False);
            controller.Advance(10);
            Assert.That(state.Registry.Things.Select(t => t.Id), Is.EquivalentTo(initialIds));
            var restored = RoundTrip(state);
            Assert.That(restored.Runtime.IsEditing, Is.False);
            Assert.That(restored.Registry.Things.Select(t => t.Id), Is.EquivalentTo(initialIds));
            Assert.That(commands.ConfirmConstruction(), Is.True);
            var site = state.Registry.Things.Single(t => t.Definition == gathering);
            controller.NotifyApplicationPause(true);
            var paused = codec.Serialize(codec.Capture(state, SessionPhase.Playing));
            controller.Advance(1000);
            Assert.That(codec.Serialize(codec.Capture(state, SessionPhase.Playing)), Is.EqualTo(paused));
            controller.NotifyApplicationPause(false);
            controller.Advance(0.05);
            Assert.That(state.Registry.Things.Single(t => t.Definition == prototype).GetComponent<WorkerComponent>().Job, Is.Not.Null);
            var invader = state.Registry.Create(invaderDefinition);
            Require(state.Map.Commands.Place(invader.Id, Vector2Int.one));
            Assert.That(commands.ReleaseInvaders(), Is.True);
            Assert.That(commands.BeginConstruction(storage), Is.True);
            Assert.That(controller.ReachGoal(), Is.False, "A real session cannot end before actual achievements.");
            controller.Advance(200);
            Assert.That(commands.CancelConstruction(), Is.True, "Cancel a transient preview without resuming the ended session.");
            Assert.That(controller.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(controller.CanAdvanceLaboratory, Is.False);
            var stopped = codec.Serialize(codec.Capture(state, SessionPhase.Destruction));
            controller.Advance(1000);
            Assert.That(codec.Serialize(codec.Capture(state, SessionPhase.Destruction)), Is.EqualTo(stopped));
            Assert.That(site.GetComponent<ConstructionComponent>().IsComplete, Is.False);
        }

        [Test]
        public void BlockedBootstrapDeliveryPreservesOnlyItsReservedSubsetAsTheHeldJob()
        {
            var controller = Controller(() => LaboratorySessionFactory.Create(organic, gathering, storage, prototype));
            Assert.That(controller.StartOrContinue(), Is.True, controller.LastError);
            var commands = new LaboratoryCommands(controller);
            Assert.That(commands.BeginConstruction(gathering), Is.True);
            Assert.That(commands.PreviewConstruction(new Vector2Int(3, 3), QuarterTurn.None), Is.True);
            Assert.That(commands.ConfirmConstruction(), Is.True);
            controller.Advance(0.05);
            var state = controller.Current;
            var simulation = (LaboratoryManager)state.Runtime;
            var worker = state.Registry.Things.Single(t => t.Definition == prototype);
            var site = state.Registry.Things.Single(t => t.Definition == gathering);
            var reservation = simulation.Reservations.Single();
            Assert.That(reservation.PickedUp, Is.True);
            Assert.That(reservation.Amount, Is.EqualTo(2));
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(4));
            Assert.That(worker.GetComponent<ResourceContainerComponent>().Contents.Select(Quantity),
                Is.EquivalentTo(new[] { 2, 2 }));
            Assert.That(controller.BeginPlacement(site.Id), Is.True);
            Assert.That(controller.PreviewPlacement(new Vector2Int(3, 0), QuarterTurn.None), Is.True);
            Assert.That(controller.ConfirmPlacement(), Is.True);
            Assert.That(simulation.Reservations, Is.Empty);
            var job = worker.GetComponent<WorkerComponent>().Job;
            Assert.That(job.Kind, Is.EqualTo(WorkKind.Hauling));
            Assert.That(job.Stage, Is.EqualTo(JobStage.Waiting));
            Assert.That(job.Amount, Is.EqualTo(2), "Uncommitted bootstrap resources are not part of the interrupted shipment.");
            var restored = RoundTrip(state);
            Assert.That(TotalResource(restored), Is.EqualTo(4));
            Assert.That(Facility(restored, FacilityRole.Gathering).GetComponent<ConstructionComponent>().Delivered, Is.Zero);
        }

        [TestCase("no-laboratory")]
        [TestCase("no-worker-state")]
        [TestCase("duplicate-component-state")]
        [TestCase("missing-component-flag")]
        [TestCase("unknown-component-owner")]
        [TestCase("unknown-job-target")]
        [TestCase("invalid-priorities")]
        [TestCase("nan-progress")]
        [TestCase("unsupported-version")]
        [TestCase("downgrade")]
        public void MalformedGameplayStateIsRejectedWithoutReplacingTheActiveSession(string corruption)
        {
            var controller = Controller();
            Assert.That(controller.StartOrContinue(), Is.True, controller.LastError);
            var previous = controller.Current;
            var snapshot = codec.Capture(previous, SessionPhase.Playing);
            var worker = snapshot.laboratory.things.Single(t => t.hasWorker);
            switch (corruption)
            {
                case "no-laboratory": snapshot.laboratory = null; break;
                case "no-worker-state": snapshot.laboratory.things = snapshot.laboratory.things.Where(t => !t.hasWorker).ToArray(); break;
                case "duplicate-component-state": snapshot.laboratory.things = snapshot.laboratory.things.Concat(new[] { worker }).ToArray(); break;
                case "missing-component-flag": snapshot.laboratory.things.First(t => t.hasGathering).hasGathering = false; break;
                case "unknown-component-owner": worker.thingId = Guid.NewGuid().ToString("N"); break;
                case "unknown-job-target":
                    worker.worker.hasJob = true;
                    worker.worker.job = new WorkerJobSnapshot { kind = WorkKind.Construction, stage = JobStage.Working,
                        targetId = Guid.NewGuid().ToString("N") };
                    break;
                case "invalid-priorities": worker.worker.priorities = new[] { 1, -1, 3 }; break;
                case "nan-progress": snapshot.laboratory.things.First(t => t.hasGathering).gathering.progress = double.NaN; break;
                case "unsupported-version": snapshot.version = 999; break;
                case "downgrade": snapshot.version = 1; snapshot.laboratory = null; break;
            }
            Assert.That(() => codec.Restore(snapshot), Throws.InstanceOf<InvalidDataException>());
            // Serialize via Unity only to inject damaged input, never via the production writer.
            var corruptedJson = JsonUtility.ToJson(snapshot);
            File.WriteAllText(slot, corruptedJson);
            Assert.That(controller.Reload(), Is.False, corruption);
            Assert.That(controller.Current, Is.SameAs(previous));
            Assert.That(File.ReadAllText(slot), Is.EqualTo(corruptedJson));
            Assert.That(controller.LastError, Is.Not.Empty);
        }

        [Test]
        public void DuplicatedOutboundReservationCannotDoubleBookTheSameResourceAfterReload()
        {
            var state = PendingConstruction();
            var simulation = (LaboratoryManager)state.Runtime;
            PrepareRemoteCarrier(state);
            RunUntil(state, () => AtCheckpoint(state, "reserved"), "outbound reservation");
            var snapshot = codec.Capture(state, SessionPhase.Playing);
            snapshot.laboratory.reservations = snapshot.laboratory.reservations.Concat(
                new[] { snapshot.laboratory.reservations[0] }).ToArray();
            Assert.That(() => codec.Restore(snapshot), Throws.InstanceOf<InvalidDataException>());
            simulation.ValidateState();
        }

        private static bool AtCheckpoint(SessionState state, string checkpoint)
        {
            var simulation = (LaboratoryManager)state.Runtime;
            var gathering = Facility(state, FacilityRole.Gathering).GetComponent<GatheringComponent>();
            switch (checkpoint)
            {
                case "construction": return state.Registry.Things.Any(t => t.TryGetComponent<ConstructionComponent>(out var c)
                    && c.Progress > 0 && !c.IsComplete);
                case "gathering": return gathering.Progress > 0 && gathering.Outstanding == 0;
                case "reserved": return simulation.Reservations.Any(r => r.Purpose == WorkKind.Hauling && !r.PickedUp);
                case "carried": return simulation.Reservations.Any(r => r.Purpose == WorkKind.Hauling && r.PickedUp);
                default: throw new ArgumentException(checkpoint);
            }
        }

        private void PrepareRemoteCarrier(SessionState state)
        {
            var simulation = (LaboratoryManager)state.Runtime;
            var builder = state.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
            Assert.That(simulation.SetWorkerPriorities(builder.Id, new[] { 1, 2, 0 }), Is.True);
            // Explicit multi-worker fixture: the collector remains at its facility,
            // while another worker must travel to the reserved completed batch.
            var carrier = state.Registry.Create(prototype);
            Require(state.Map.Commands.Place(carrier.Id, new Vector2Int(0, 1)));
            Assert.That(simulation.SetWorkerPriorities(carrier.Id, new[] { 0, 0, 1 }), Is.True);
        }

        private void RunUntil(SessionState state, Func<bool> condition, string description)
        {
            var controller = new GameFlowManager(codec, new MemoryStore(), () => state, 10000);
            Assert.That(controller.StartOrContinue(), Is.True, controller.LastError);
            for (var step = 0; step < 20000 && !condition(); step++) controller.Advance(0.05);
            Assert.That(condition(), Is.True, "Timed out: " + description);
        }

        private sealed class MemoryStore : ISessionStore
        {
            private string json;
            public bool Exists => json != null;
            public string Read() => json;
            public void Write(string value) => json = value;
        }

        private static Thing Facility(SessionState state, FacilityRole role) => state.Registry.Things.Single(t =>
            t.Definition is FacilityDefinition definition && definition.Role == role);
        private static int Stored(SessionState state) => Facility(state, FacilityRole.Storage)
            .GetComponent<ResourceContainerComponent>().Contents.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
        private static int TotalResource(SessionState state) => state.Registry.Things
            .Where(t => t.Definition.Kind == ThingKind.Resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
        private static int Quantity(Thing thing) => thing.GetComponent<ResourceStackComponent>().Quantity;
        private static void Require(PlacementResult result) => Assert.That(result.Succeeded, Is.True, result.ToString());
    }
}
