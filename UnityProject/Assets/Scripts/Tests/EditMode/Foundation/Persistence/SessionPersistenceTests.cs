using MobileGame.TestSupport;
using System;
using System.IO;
using System.Linq;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class SessionPersistenceTests : FoundationTestFixture
    {
        private string directory;

        [SetUp]
        public void PrepareSaveDirectory()
        {
            directory = Path.Combine(Path.GetTempPath(), "DoomInc-PersistenceTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void RemoveTestSaveDirectory()
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        [Test]
        public void ActualFileRoundTripRestoresIndependentObjectsAndSingleOwnership()
        {
            var storage = CreateStorageDefinition();
            using (var serialized = new DataFixtureEditor(storage))
            {
                serialized.FindProperty("footprint").vector2IntValue = new Vector2Int(2, 1);
                serialized.FindProperty("interactionOffset").vector2IntValue = new Vector2Int(1, -1);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var creature = CreateDefinition<CreatureData>("fixture.carrier", ThingComponentKind.ResourceContainer);
            var resource = CreateResourceDefinition();
            var definitionsBefore = DataFixtureSnapshot.ToJson(storage);
            var codec = new SessionSnapshotCodec(new ThingData[] { storage, creature, resource });
            var registry = new ThingRegistry();
            var first = registry.Create(storage);
            var movedResource = registry.CreateResource(resource, 7, first.GetComponent<ResourceContainerComponent>());
            var second = registry.Create(storage);
            registry.TransferResource(movedResource, second.GetComponent<ResourceContainerComponent>());
            var carrier = registry.Create(creature);
            var outside = registry.Create(creature);
            var carried = registry.CreateResource(resource, 3, carrier.GetComponent<ResourceContainerComponent>());
            var map = new GridMap(registry, 10, 8);
            Require(map.Commands.Place(first.Id, new Vector2Int(2, 2), QuarterTurn.Clockwise90));
            Require(map.Commands.Place(second.Id, new Vector2Int(7, 4)));
            Require(map.Commands.Place(carrier.Id, new Vector2Int(5, 1)));
            Require(map.Commands.Place(outside.Id, new Vector2Int(5, 1)));
            Require(map.Commands.Unplace(outside.Id));
            var source = new SessionState(registry, map);
            var saved = codec.Capture(source, SessionPhase.Destruction);
            var store = new JsonFileSessionStore(Path.Combine(directory, "session.json"));
            Assert.That(store.Exists, Is.False);
            store.Write(codec.Serialize(saved));
            Assert.That(store.Exists, Is.True);

            var parsed = codec.Deserialize(new JsonFileSessionStore(Path.Combine(directory, "session.json")).Read());
            var restored = codec.Restore(parsed);
            Assert.That(parsed.phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(restored.Registry, Is.Not.SameAs(registry));
            Assert.That(restored.Map, Is.Not.SameAs(map));
            Assert.That(codec.Serialize(codec.Capture(restored, parsed.phase)), Is.EqualTo(codec.Serialize(saved)));
            var restoredResource = Find(restored, movedResource.Id);
            var restoredFirst = Find(restored, first.Id);
            var restoredSecond = Find(restored, second.Id);
            Assert.That(restoredResource, Is.Not.SameAs(movedResource));
            Assert.That(restoredResource.HolderId, Is.EqualTo(second.Id));
            Assert.That(restoredResource.CreationOrder, Is.LessThan(restoredSecond.CreationOrder),
                "Restoration must not assume a holder was created before its resource.");
            Assert.That(restoredFirst.Definition, Is.SameAs(restoredSecond.Definition));
            Assert.That(restoredFirst.GetComponent<ResourceContainerComponent>(),
                Is.Not.SameAs(restoredSecond.GetComponent<ResourceContainerComponent>()));
            Assert.That(restoredFirst.GetComponent<ResourceContainerComponent>().Contents, Is.Empty);
            Assert.That(restoredSecond.GetComponent<ResourceContainerComponent>().Contents.Single(), Is.SameAs(restoredResource));
            Assert.That(Find(restored, carried.Id).HolderId, Is.EqualTo(carrier.Id));
            Assert.That(Find(restored, carried.Id).Map, Is.Null);
            Assert.That(Find(restored, outside.Id).Exists, Is.True);
            Assert.That(Find(restored, outside.Id).Map, Is.Null);
            Assert.That(restoredFirst.Placement.Rotation, Is.EqualTo(QuarterTurn.Clockwise90));
            Assert.That(restoredFirst.Placement.InteractionCell, Is.EqualTo(first.Placement.InteractionCell));
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
                Assert.That(restored.Map.IsWalkable(new Vector2Int(x, y)), Is.EqualTo(map.IsWalkable(new Vector2Int(x, y))));

            restored.Registry.ConsumeResource(restoredResource, 2);
            Assert.That(restoredResource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(movedResource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(7));
            Assert.That(Find(restored, carried.Id).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
            Assert.That(DataFixtureSnapshot.ToJson(storage), Is.EqualTo(definitionsBefore));
        }

        [Test]
        public void RemovedHighestOrderAndConsumedResourcesStayRemovedAfterRestore()
        {
            var storage = CreateStorageDefinition();
            var resource = CreateResourceDefinition();
            var codec = new SessionSnapshotCodec(new ThingData[] { storage, resource });
            var registry = new ThingRegistry();
            var holder = registry.Create(storage);
            var consumed = registry.CreateResource(resource, 5, holder.GetComponent<ResourceContainerComponent>());
            registry.ConsumeResource(consumed, 5);
            var removed = registry.Create(storage);
            var highest = removed.CreationOrder;
            registry.Remove(removed);
            var restored = codec.Restore(codec.Deserialize(codec.Serialize(codec.Capture(
                new SessionState(registry, new GridMap(registry, 3, 3)), SessionPhase.Playing))));
            Assert.That(restored.Registry.TryGet(consumed.Id, out _), Is.False);
            Assert.That(restored.Registry.TryGet(removed.Id, out _), Is.False);
            Assert.That(restored.Registry.LastCreationOrder, Is.EqualTo(highest));
            Assert.That(restored.Registry.Create(storage).CreationOrder, Is.GreaterThan(highest));
        }

        [Test]
        public void LegalCreatureOverlapAndFacilityOverCreatureSurviveRestoreOrdering()
        {
            var storage = CreateStorageDefinition();
            var creature = CreateDefinition<CreatureData>("fixture.creature");
            var codec = new SessionSnapshotCodec(new ThingData[] { storage, creature });
            var registry = new ThingRegistry();
            var facility = registry.Create(storage);
            var a = registry.Create(creature);
            var b = registry.Create(creature);
            var map = new GridMap(registry, 3, 3);
            var cell = new Vector2Int(1, 0);
            Require(map.Commands.Place(a.Id, cell));
            Require(map.Commands.Place(b.Id, cell));
            Require(map.Commands.Place(facility.Id, cell));
            var restored = codec.Restore(codec.Capture(new SessionState(registry, map), SessionPhase.Results));
            Assert.That(restored.Map.GetPlacements().Count, Is.EqualTo(3));
            Assert.That(Find(restored, a.Id).Placement.Origin, Is.EqualTo(cell));
            Assert.That(Find(restored, b.Id).Placement.Origin, Is.EqualTo(cell));
            Assert.That(restored.Map.GetBlockingFacility(cell), Is.EqualTo(facility.Id));
            Assert.That(Find(restored, facility.Id).Placement.InteractionCell.Value.y, Is.EqualTo(-1),
                "An out-of-bounds interaction cell is legal when the footprint is in bounds.");
        }

        [Test]
        public void EditingSavesOnlyCommittedPlacementAndDoesNotRestoreTransientPause()
        {
            var state = CreateSimpleSession(out var codec, out var facility, out _);
            var committed = facility.Placement;
            Require(state.Map.Commands.BeginEdit(facility.Id));
            Require(state.Map.Commands.Preview(new Vector2Int(3, 3), QuarterTurn.Clockwise90));
            var restored = codec.Restore(codec.Capture(state, SessionPhase.Playing));
            Assert.That(state.Map.Commands.IsEditing, Is.True, "Saving must not commit or cancel the active edit.");
            Assert.That(restored.Map.Commands.IsEditing, Is.False);
            Assert.That(Find(restored, facility.Id).Placement.Origin, Is.EqualTo(committed.Origin));
            Assert.That(Find(restored, facility.Id).Placement.Rotation, Is.EqualTo(committed.Rotation));
        }

        [TestCase("duplicate-id")]
        [TestCase("duplicate-order")]
        [TestCase("low-high-water")]
        [TestCase("missing-definition")]
        [TestCase("missing-holder")]
        [TestCase("resource-holder")]
        [TestCase("zero-quantity")]
        [TestCase("non-resource-quantity")]
        [TestCase("missing-placed-thing")]
        [TestCase("resource-placement")]
        [TestCase("outside-map")]
        [TestCase("duplicate-placement")]
        [TestCase("unknown-rotation")]
        public void InvalidSnapshotCannotMutateTheOriginalGraph(string invalidCase)
        {
            var state = CreateSimpleSession(out var codec, out var facility, out var resource);
            var valid = codec.Capture(state, SessionPhase.Playing);
            var before = codec.Serialize(valid);
            var snapshot = codec.Deserialize(before);
            var holderRecord = snapshot.things.Single(t => t.id == facility.Id.ToString("N"));
            var resourceRecord = snapshot.things.Single(t => t.id == resource.Id.ToString("N"));
            switch (invalidCase)
            {
                case "duplicate-id": resourceRecord.id = holderRecord.id; break;
                case "duplicate-order": resourceRecord.creationOrder = holderRecord.creationOrder; break;
                case "low-high-water": snapshot.lastCreationOrder = 0; break;
                case "missing-definition": holderRecord.definitionId = "missing.definition"; break;
                case "missing-holder": resourceRecord.holderId = Guid.NewGuid().ToString("N"); break;
                case "resource-holder": resourceRecord.holderId = resourceRecord.id; break;
                case "zero-quantity": resourceRecord.quantity = 0; break;
                case "non-resource-quantity": holderRecord.quantity = 1; break;
                case "missing-placed-thing": snapshot.placements[0].thingId = Guid.NewGuid().ToString("N"); break;
                case "resource-placement": snapshot.placements[0].thingId = resourceRecord.id; break;
                case "outside-map": snapshot.placements[0].x = int.MaxValue; break;
                case "duplicate-placement": snapshot.placements = new[] { snapshot.placements[0], snapshot.placements[0] }; break;
                case "unknown-rotation": snapshot.placements[0].rotation = 4; break;
            }
            Assert.That(() => codec.Restore(snapshot), Throws.TypeOf<InvalidDataException>());
            Assert.That(codec.Serialize(codec.Capture(state, SessionPhase.Playing)), Is.EqualTo(before));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("null")]
        [TestCase("{ not json")]
        [TestCase("{}")]
        [TestCase("{\"version\":999}")]
        public void MissingMalformedOrUnsupportedSaveIsNotAnEmptyNewGame(string json)
        {
            CreateSimpleSession(out var codec, out _, out _);
            Assert.That(() => codec.Deserialize(json), Throws.TypeOf<InvalidDataException>());
        }

        [TestCase(SessionPhase.Preparing)]
        [TestCase((SessionPhase)99)]
        public void UnpreparedAndUnknownPhasesCannotBeSaved(SessionPhase phase)
        {
            var state = CreateSimpleSession(out var codec, out _, out _);
            Assert.That(() => codec.Capture(state, phase), Throws.TypeOf<InvalidDataException>());
        }

        [Test]
        public void CaptureRejectsAnUntrackedMapRatherThanSilentlyLosingPlacement()
        {
            var state = CreateSimpleSession(out var codec, out var facility, out _);
            Require(state.Map.Commands.Unplace(facility.Id));
            var otherMap = new GridMap(state.Registry, 4, 4);
            Require(otherMap.Commands.Place(facility.Id, Vector2Int.zero));
            Assert.That(() => codec.Capture(state, SessionPhase.Playing), Throws.TypeOf<InvalidDataException>());
        }

        [Test]
        public void DuplicateDefinitionIdsAcrossDistinctAssetsAreRejected()
        {
            var first = CreateStorageDefinition();
            var duplicate = CreateStorageDefinition();
            Assert.That(() => new SessionSnapshotCodec(new ThingData[] { first, duplicate }),
                Throws.ArgumentException);
        }

        [Test]
        public void FileOverwriteAndLockedFileFailurePreserveTheLastGoodSave()
        {
            var state = CreateSimpleSession(out var codec, out _, out var resource);
            var path = Path.Combine(directory, "session.json");
            var store = new JsonFileSessionStore(path);
            var original = codec.Serialize(codec.Capture(state, SessionPhase.Playing));
            store.Write(original);
            state.Registry.ConsumeResource(resource, 1);
            var newer = codec.Serialize(codec.Capture(state, SessionPhase.Results));
            store.Write(newer);
            Assert.That(store.Read(), Is.EqualTo(newer));
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                Assert.That(() => store.Write(original), Throws.InstanceOf<IOException>());
            Assert.That(store.Read(), Is.EqualTo(newer));
            Assert.That(Directory.GetFiles(directory), Has.Length.EqualTo(1), "A failed write must clean its temporary file.");
            var loaded = codec.Deserialize(store.Read());
            Assert.That(loaded.phase, Is.EqualTo(SessionPhase.Results));
            Assert.That(Find(codec.Restore(loaded), resource.Id).GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(4));
        }

        private SessionState CreateSimpleSession(out SessionSnapshotCodec codec, out Thing facility, out Thing resource)
        {
            var storageDefinition = CreateStorageDefinition();
            var resourceDefinition = CreateResourceDefinition();
            codec = new SessionSnapshotCodec(new ThingData[] { storageDefinition, resourceDefinition });
            var registry = new ThingRegistry();
            facility = registry.Create(storageDefinition);
            resource = registry.CreateResource(resourceDefinition, 5, facility.GetComponent<ResourceContainerComponent>());
            var map = new GridMap(registry, 5, 5);
            Require(map.Commands.Place(facility.Id, new Vector2Int(1, 1)));
            return new SessionState(registry, map);
        }

        private static Thing Find(SessionState state, Guid id)
        {
            Assert.That(state.Registry.TryGet(id, out var thing), Is.True, "Missing restored Thing " + id);
            return thing;
        }

        private static void Require(PlacementResult result)
            => Assert.That(result.Succeeded, Is.True, result.ToString());
    }
}
