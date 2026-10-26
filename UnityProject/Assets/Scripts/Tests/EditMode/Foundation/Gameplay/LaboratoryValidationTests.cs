using System;
using System.IO;
using System.Linq;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using SessionState = MobileGame.Foundation.SessionState;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryValidationTests : FoundationTestFixture
    {
        private SessionState state;
        private LaboratorySimulation simulation;
        private SessionSnapshotCodec codec;
        private JsonFileSessionStore store;
        private CreatureDefinition prototype;
        private string saveDirectory;
        private Thing worker;
        private Thing gathering;

        [SetUp]
        public void BuildAndSaveActualGatheringCheckpoint()
        {
            var organic = CreateResourceDefinition("validation.organic");
            prototype = CreateDefinition<CreatureDefinition>("validation.prototype", ThingComponentKind.ResourceContainer);
            var gatheringDefinition = Facility("validation.gathering", FacilityRole.Gathering, organic);
            var storageDefinition = Facility("validation.storage", FacilityRole.Storage, organic);
            state = LaboratorySessionFactory.Create(organic, gatheringDefinition, storageDefinition, prototype);
            simulation = (LaboratorySimulation)state.Runtime;
            codec = new SessionSnapshotCodec(new ThingDefinition[] { organic, prototype, gatheringDefinition, storageDefinition }, true);
            Build(gatheringDefinition, new Vector2Int(4, 4));
            Build(storageDefinition, new Vector2Int(7, 4));
            worker = state.Registry.Things.Single(t => t.Definition is CreatureDefinition);
            gathering = state.Registry.Things.Single(t => ReferenceEquals(t.Definition, gatheringDefinition));
            for (var elapsed = 0; elapsed < 2400 && gathering.GetComponent<GatheringComponent>().Progress < 1; elapsed++)
                simulation.Advance(0.05);
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.InRange(1.0, 1.1));
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.Working));
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero, "Both initial construction costs were actually delivered and consumed.");
            saveDirectory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN008-Validation-" + Guid.NewGuid().ToString("N"));
            store = new JsonFileSessionStore(Path.Combine(saveDirectory, "session.json"));
            SaveCheckpoint();
        }

        [TearDown]
        public void RemoveIsolatedSave()
        {
            if (saveDirectory != null && Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        [TestCase("working-away")]
        [TestCase("worker-unplaced")]
        [TestCase("idle-movement")]
        public void RejectsWorkerPlacementOrMovementThatDisagreesWithItsWork(string corruption)
        {
            var saved = codec.Deserialize(store.Read());
            var workerId = worker.Id.ToString("N");
            if (corruption == "working-away")
            {
                var placement = saved.placements.Single(p => p.thingId == workerId);
                placement.x = 1; placement.y = 1;
            }
            else if (corruption == "worker-unplaced")
                saved.placements = saved.placements.Where(p => p.thingId != workerId).ToArray();
            else
            {
                var workerState = saved.laboratory.things.Single(t => t.thingId == workerId).worker;
                workerState.hasJob = false;
                workerState.job = null;
                workerState.moveProgress = 0.1;
            }
            AssertRejectedWithoutChangingLiveSessionOrSavedFile(saved);
        }

        [Test]
        public void RejectsTwoWorkersClaimingOneFacilityEvenWhenBothStandAtItsInteractionCell()
        {
            // A second actor is an explicit malformed-graph fixture, not a change to the actual start.
            var other = state.Registry.Create(prototype);
            Assert.That(state.Map.Commands.Place(other.Id, worker.Placement.Origin).Succeeded, Is.True);
            SaveCheckpoint();
            var saved = codec.Deserialize(store.Read());
            var original = saved.laboratory.things.Single(t => t.thingId == worker.Id.ToString("N")).worker.job;
            var copied = saved.laboratory.things.Single(t => t.thingId == other.Id.ToString("N")).worker;
            copied.hasJob = true;
            copied.job = new WorkerJobSnapshot
            {
                kind = original.kind, stage = original.stage, targetId = original.targetId,
                sourceId = original.sourceId, resourceId = original.resourceId,
                amount = original.amount, batchOriginId = original.batchOriginId
            };
            AssertRejectedWithoutChangingLiveSessionOrSavedFile(saved);
        }

        [Test]
        public void RejectsWorkWhoseTargetExistsButHasNoMapPlacement()
        {
            var saved = codec.Deserialize(store.Read());
            saved.placements = saved.placements.Where(p => p.thingId != gathering.Id.ToString("N")).ToArray();
            AssertRejectedWithoutChangingLiveSessionOrSavedFile(saved);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FacilityCoveringAWorkerRemainsLegalForIdleOrReachableMovingState(bool idle)
        {
            var storage = state.Registry.Things.Single(t => t.Definition is FacilityDefinition facility
                && facility.Role == FacilityRole.Storage);
            Assert.That(state.Map.Commands.Place(gathering.Id, new Vector2Int(9, 4)).Succeeded, Is.True);
            Assert.That(state.Map.Commands.Place(storage.Id, worker.Placement.Origin).Succeeded, Is.True);
            simulation.OnMapChanged();
            if (idle)
            {
                simulation.SetWorkerPriorities(worker.Id, new[] { 0, 0, 0 });
                simulation.Advance(0.05);
                Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null);
            }
            else Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.ToDestination));
            Assert.That(state.Map.IsWalkable(worker.Placement.Origin), Is.False,
                "This verifies legal facility-over-creature placement, not an ordinary free cell.");

            SaveCheckpoint();
            var restored = codec.Restore(codec.Deserialize(store.Read()));
            Assert.That(restored.Registry.TryGet(worker.Id, out var restoredWorker), Is.True);
            Assert.That(restored.Map.IsWalkable(restoredWorker.Placement.Origin), Is.False);
            Assert.That(restored.Registry.TryGet(gathering.Id, out var restoredGathering), Is.True);
            Assert.That(restoredGathering.GetComponent<GatheringComponent>().Progress,
                Is.EqualTo(gathering.GetComponent<GatheringComponent>().Progress).Within(1e-9),
                "JSON number parsing may differ by a floating-point ULP without changing elapsed work time.");
            if (!idle)
            {
                restored.Runtime.Advance(0.25);
                Assert.That(restored.Map.IsWalkable(restoredWorker.Placement.Origin), Is.True,
                    "The worker can leave through an adjacent walkable cell after restoration.");
            }
        }

        private void AssertRejectedWithoutChangingLiveSessionOrSavedFile(SessionSnapshot corrupted)
        {
            var savedFile = store.Read();
            var originalJob = worker.GetComponent<WorkerComponent>().Job;
            var progress = gathering.GetComponent<GatheringComponent>().Progress;
            Assert.Throws<InvalidDataException>(() => codec.Restore(corrupted));
            Assert.That(store.Read(), Is.EqualTo(savedFile));
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.SameAs(originalJob));
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.EqualTo(progress));
            Assert.That(codec.Restore(codec.Deserialize(store.Read())).Registry.TryGet(worker.Id, out _), Is.True);
        }

        private void SaveCheckpoint() => store.Write(codec.Serialize(codec.Capture(state, SessionPhase.Playing)));

        private void Build(FacilityDefinition definition, Vector2Int position)
        {
            Assert.That(simulation.BeginConstruction(definition), Is.True);
            Assert.That(simulation.PreviewConstruction(position, QuarterTurn.None), Is.True);
            Assert.That(simulation.ConfirmConstruction(), Is.True);
        }

        private FacilityDefinition Facility(string id, FacilityRole role, ResourceDefinition organic)
        {
            var definition = CreateStorageDefinition(id);
            using (var serialized = new SerializedObject(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return definition;
        }
    }
}
