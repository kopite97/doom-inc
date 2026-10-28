using MobileGame.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class GridMapTests : FoundationTestFixture
    {
        private FacilityData Facility(int width = 2, int height = 3)
        {
            var definition = CreateStorageDefinition();
            Configure(definition, new Vector2Int(width, height), new Vector2Int(1, -1));
            return definition;
        }

        private static void Configure(FacilityData definition, Vector2Int size, Vector2Int interaction)
        {
            using (var serialized = new DataFixtureEditor(definition))
            {
                serialized.FindProperty("footprint").vector2IntValue = size;
                serialized.FindProperty("interactionOffset").vector2IntValue = interaction;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        [TestCase(QuarterTurn.None, 2, 3, 5, 3)]
        [TestCase(QuarterTurn.Clockwise90, 3, 2, 3, 4)]
        [TestCase(QuarterTurn.Clockwise180, 2, 3, 4, 7)]
        [TestCase(QuarterTurn.Clockwise270, 3, 2, 7, 5)]
        public void RectangularFacilityRotatesFootprintAndFixedInteraction(QuarterTurn rotation,
            int width, int height, int interactionX, int interactionY)
        {
            var registry = new ThingRegistry();
            var thing = registry.Create(Facility());
            var map = new GridMap(registry, 12, 12);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(1, 1)).Succeeded, Is.True);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(4, 4), rotation).Succeeded, Is.True);
            Assert.That(thing.Placement.Footprint, Is.EqualTo(new Vector2Int(width, height)));
            Assert.That(thing.Placement.InteractionCell, Is.EqualTo(new Vector2Int(interactionX, interactionY)));
            Assert.That(map.GetBlockingFacility(new Vector2Int(1, 1)), Is.Null);
            var occupied = 0;
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
                if (map.GetBlockingFacility(new Vector2Int(x, y)) == thing.Id) occupied++;
            Assert.That(occupied, Is.EqualTo(6), "Only the new footprint may block movement.");
        }

        [Test]
        public void InvalidMoveAndRotationAreAtomicIncludingIntegerExtremes()
        {
            var registry = new ThingRegistry();
            var definition = Facility();
            var a = registry.Create(definition);
            var b = registry.Create(definition);
            var map = new GridMap(registry, 8, 8);
            map.Commands.Place(a.Id, new Vector2Int(0, 0));
            map.Commands.Place(b.Id, new Vector2Int(6, 4));
            var before = b.Placement;
            Assert.That(map.Commands.Place(b.Id, Vector2Int.zero).Error, Is.EqualTo(PlacementError.FacilityOverlap));
            Assert.That(map.Commands.Place(b.Id, before.Origin, QuarterTurn.Clockwise90).Error, Is.EqualTo(PlacementError.OutsideBounds));
            Assert.That(map.Commands.Place(b.Id, new Vector2Int(int.MaxValue, 0)).Error, Is.EqualTo(PlacementError.OutsideBounds));
            Assert.That(map.Commands.Place(b.Id, new Vector2Int(-1, 0)).Error, Is.EqualTo(PlacementError.OutsideBounds));
            Assert.That(map.Commands.Place(b.Id, before.Origin, (QuarterTurn)4).Error, Is.EqualTo(PlacementError.InvalidRotation));
            Assert.That(b.Placement, Is.SameAs(before));
            Assert.That(map.GetPlacements().Count, Is.EqualTo(2));
        }

        [Test]
        public void CorridorAndInteractionBlockingDoNotBecomePlacementRestrictions()
        {
            var registry = new ThingRegistry();
            var definition = CreateStorageDefinition();
            Configure(definition, new Vector2Int(3, 1), new Vector2Int(1, -1));
            var wall = registry.Create(definition);
            var other = registry.Create(definition);
            var map = new GridMap(registry, 3, 5);
            Assert.That(map.Commands.Place(wall.Id, new Vector2Int(0, 2)).Succeeded, Is.True);
            Assert.That(map.Commands.Place(other.Id, new Vector2Int(0, 1)).Succeeded, Is.True);
            Assert.That(map.IsWalkable(wall.Placement.InteractionCell.Value), Is.False);
            Assert.That(map.IsWalkable(new Vector2Int(1, 3)), Is.True);
            Assert.That(map.Commands.Place(other.Id, Vector2Int.zero).Succeeded, Is.True);
            Assert.That(other.Placement.InteractionCell.Value.y, Is.EqualTo(-1));
            Assert.That(map.IsWalkable(other.Placement.InteractionCell.Value), Is.False,
                "An out-of-bounds interaction cell does not invalidate an in-bounds footprint.");
        }

        [Test]
        public void CreaturesOverlapWithoutBlockingButCannotBePlacedInsideFacility()
        {
            var registry = new ThingRegistry();
            var creature = CreateDefinition<CreatureData>("fixture.creature");
            var a = registry.Create(creature);
            var b = registry.Create(creature);
            var facility = registry.Create(CreateStorageDefinition());
            var map = new GridMap(registry, 5, 5);
            var cell = new Vector2Int(2, 2);
            Assert.That(map.Commands.Place(a.Id, cell).Succeeded, Is.True);
            Assert.That(map.Commands.Place(b.Id, cell).Succeeded, Is.True);
            Assert.That(map.IsWalkable(cell), Is.True);
            // Approved placement constraints exclude only other facilities, not creatures.
            Assert.That(map.Commands.Place(facility.Id, cell).Succeeded, Is.True);
            Assert.That(a.Placement.Origin, Is.EqualTo(cell));
            Assert.That(map.IsWalkable(cell), Is.False);
            Assert.That(map.Commands.Place(a.Id, new Vector2Int(1, 2)).Succeeded, Is.True);
            var before = a.Placement;
            Assert.That(map.Commands.Place(a.Id, cell).Error, Is.EqualTo(PlacementError.CellBlocked));
            Assert.That(a.Placement, Is.SameAs(before));
            Assert.That(map.IsWalkable(new Vector2Int(5, 0)), Is.False);
        }

        [Test]
        public void MovingOneSharedDefinitionPreservesIdentityInventoryAndOtherInstance()
        {
            var registry = new ThingRegistry();
            var definition = Facility();
            var originalDefinition = DataFixtureSnapshot.ToJson(definition);
            var first = registry.Create(definition);
            var second = registry.Create(definition);
            var held = registry.CreateResource(CreateResourceDefinition(), 5, first.GetComponent<ResourceContainerComponent>());
            var map = new GridMap(registry, 12, 12);
            map.Commands.Place(first.Id, Vector2Int.zero);
            map.Commands.Place(second.Id, new Vector2Int(7, 7));
            var originalId = first.Id;
            var order = first.CreationOrder;
            var secondPlacement = second.Placement;
            Assert.That(map.Commands.Place(first.Id, new Vector2Int(3, 3), QuarterTurn.Clockwise90).Succeeded, Is.True);
            Assert.That(first.Id, Is.EqualTo(originalId));
            Assert.That(first.CreationOrder, Is.EqualTo(order));
            Assert.That(held.HolderId, Is.EqualTo(originalId));
            Assert.That(held.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(held.Map, Is.Null);
            Assert.That(second.Placement, Is.SameAs(secondPlacement));
            Assert.That(DataFixtureSnapshot.ToJson(definition), Is.EqualTo(originalDefinition));
        }

        [Test]
        public void PreviewAndCancelKeepOriginalOccupancyAndSuccessfulConfirmSwitchesOnce()
        {
            var registry = new ThingRegistry();
            var thing = registry.Create(Facility());
            var map = new GridMap(registry, 10, 10);
            map.Commands.Place(thing.Id, Vector2Int.zero);
            var before = thing.Placement;
            Assert.That(map.Commands.BeginEdit(thing.Id).Succeeded, Is.True);
            Assert.That(map.Commands.IsEditing, Is.True);
            Assert.That(map.Commands.Preview(new Vector2Int(4, 4), QuarterTurn.Clockwise90).Succeeded, Is.True);
            Assert.That(thing.Placement, Is.SameAs(before));
            Assert.That(map.IsWalkable(new Vector2Int(4, 4)), Is.True);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(3, 3)).Error, Is.EqualTo(PlacementError.EditInProgress));
            Assert.That(map.Commands.Unplace(thing.Id).Error, Is.EqualTo(PlacementError.EditInProgress));
            Assert.That(map.Commands.BeginEdit(thing.Id).Error, Is.EqualTo(PlacementError.EditInProgress));
            Assert.That(map.Commands.Cancel().Succeeded, Is.True);
            Assert.That(thing.Placement, Is.SameAs(before));
            Assert.That(map.Commands.IsEditing, Is.False);

            map.Commands.BeginEdit(thing.Id);
            map.Commands.Preview(new Vector2Int(4, 4), QuarterTurn.Clockwise90);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
            Assert.That(map.Commands.IsEditing, Is.False);
            Assert.That(map.IsWalkable(Vector2Int.zero), Is.True);
            Assert.That(thing.Placement.Origin, Is.EqualTo(new Vector2Int(4, 4)));
            Assert.That(map.Commands.Confirm().Error, Is.EqualTo(PlacementError.NoEdit));
        }

        [Test]
        public void FailedConfirmationKeepsEditingAndCanBeCorrected()
        {
            var registry = new ThingRegistry();
            var a = registry.Create(Facility());
            var map = new GridMap(registry, 8, 8);
            map.Commands.Place(a.Id, Vector2Int.zero);
            var before = a.Placement;
            map.Commands.BeginEdit(a.Id);
            Assert.That(map.Commands.Preview(new Vector2Int(7, 7), QuarterTurn.None).Error,
                Is.EqualTo(PlacementError.OutsideBounds));
            Assert.That(map.Commands.Confirm().Error, Is.EqualTo(PlacementError.OutsideBounds));
            Assert.That(map.Commands.IsEditing, Is.True);
            Assert.That(a.Placement, Is.SameAs(before));
            map.Commands.Preview(new Vector2Int(3, 3), QuarterTurn.None);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
        }

        [Test]
        public void NewPlacementCanBeCancelledWithoutDestroyingThing()
        {
            var registry = new ThingRegistry();
            var thing = registry.Create(Facility());
            var map = new GridMap(registry, 6, 6);
            map.Commands.BeginEdit(thing.Id);
            map.Commands.Preview(new Vector2Int(2, 2), QuarterTurn.None);
            map.Commands.Cancel();
            Assert.That(thing.IsPlaced, Is.False);
            Assert.That(thing.Exists, Is.True);
            map.Commands.BeginEdit(thing.Id);
            map.Commands.Preview(new Vector2Int(2, 2), QuarterTurn.None);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
            Assert.That(thing.IsPlaced, Is.True);
        }

        [Test]
        public void UnplacementPreservesExistenceAndInventoryAndRemovalCleansMap()
        {
            var registry = new ThingRegistry();
            var holder = registry.Create(CreateStorageDefinition());
            var held = registry.CreateResource(CreateResourceDefinition(), 5, holder.GetComponent<ResourceContainerComponent>());
            var map = new GridMap(registry, 6, 6);
            map.Commands.Place(holder.Id, new Vector2Int(2, 2));
            Assert.That(() => registry.Remove(holder), Throws.InvalidOperationException);
            Assert.That(holder.IsPlaced, Is.True);
            Assert.That(map.Commands.Unplace(holder.Id).Succeeded, Is.True);
            Assert.That(holder.Exists, Is.True);
            Assert.That(holder.Map, Is.Null);
            Assert.That(held.HolderId, Is.EqualTo(holder.Id));
            Assert.That(map.IsWalkable(new Vector2Int(2, 2)), Is.True);
            map.Commands.Place(holder.Id, new Vector2Int(3, 3));
            registry.ConsumeResource(held, 5);
            registry.Remove(holder);
            Assert.That(holder.IsPlaced, Is.False);
            Assert.That(holder.Exists, Is.False);
            Assert.That(map.GetPlacements(), Is.Empty);
        }

        [Test]
        public void RemovedEditedTargetCannotBeResurrectedByConfirm()
        {
            var registry = new ThingRegistry();
            var thing = registry.Create(CreateStorageDefinition());
            var map = new GridMap(registry, 6, 6);
            map.Commands.Place(thing.Id, Vector2Int.zero);
            map.Commands.BeginEdit(thing.Id);
            registry.Remove(thing);
            Assert.That(map.Commands.Confirm().Succeeded, Is.False);
            Assert.That(thing.Exists, Is.False);
            Assert.That(map.GetPlacements(), Is.Empty);
            Assert.That(map.Commands.Cancel().Succeeded, Is.True);
        }

        [Test]
        public void RejectsForeignRegistryOtherMapAndResourcesWithoutSideEffects()
        {
            var registry = new ThingRegistry();
            var facility = CreateStorageDefinition();
            var thing = registry.Create(facility);
            var map = new GridMap(registry, 6, 6);
            var otherMap = new GridMap(registry, 6, 6);
            map.Commands.Place(thing.Id, Vector2Int.zero);
            var foreign = new ThingRegistry().Create(facility);
            Assert.That(map.Commands.Place(foreign.Id, Vector2Int.zero).Error, Is.EqualTo(PlacementError.UnknownThing));
            Assert.That(otherMap.Commands.Place(thing.Id, Vector2Int.zero).Error, Is.EqualTo(PlacementError.OtherMap));
            Assert.That(otherMap.Commands.BeginEdit(thing.Id).Error, Is.EqualTo(PlacementError.OtherMap));
            var resource = registry.CreateResource(CreateResourceDefinition(), 1, thing.GetComponent<ResourceContainerComponent>());
            Assert.That(map.Commands.Place(resource.Id, new Vector2Int(1, 1)).Error, Is.EqualTo(PlacementError.ResourceNotSpatial));
            Assert.That(map.Commands.BeginEdit(resource.Id).Error, Is.EqualTo(PlacementError.FacilityRequired));
            Assert.That(resource.HolderId, Is.EqualTo(thing.Id));
            Assert.That(otherMap.GetPlacements(), Is.Empty);
            map.Commands.Unplace(thing.Id);
            Assert.That(otherMap.Commands.Place(thing.Id, Vector2Int.zero).Succeeded, Is.True);
            Assert.That(thing.Map, Is.SameAs(otherMap));
        }

        [Test]
        public void QueriesAreReadOnlySnapshotsAndInvalidDefinitionIsRejected()
        {
            var registry = new ThingRegistry();
            var definition = Facility();
            var thing = registry.Create(definition);
            var map = new GridMap(registry, 6, 6);
            map.Commands.Place(thing.Id, Vector2Int.zero);
            var snapshot = map.GetPlacements();
            Assert.That(() => ((IList<MapPlacement>)snapshot).Clear(), Throws.TypeOf<NotSupportedException>());
            map.Commands.Place(thing.Id, new Vector2Int(3, 3));
            Assert.That(snapshot[0].Origin, Is.EqualTo(Vector2Int.zero));
            Configure(definition, new Vector2Int(0, 1), new Vector2Int(0, -1));
            Assert.That(map.Commands.Place(thing.Id, Vector2Int.zero).Error, Is.EqualTo(PlacementError.InvalidDefinition));
            Assert.That(() => registry.Create(definition), Throws.ArgumentException);
            Assert.That(thing.Placement.Origin, Is.EqualTo(new Vector2Int(3, 3)));
            Configure(definition, Vector2Int.one, Vector2Int.zero);
            Assert.That(() => definition.Validate(), Throws.ArgumentException);
            Assert.That(() => new GridMap(registry, 0, 1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
