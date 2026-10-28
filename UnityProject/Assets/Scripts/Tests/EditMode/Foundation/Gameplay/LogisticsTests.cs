using System;
using System.Linq;
using System.Reflection;
using MobileGame.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    // These explicit preparation states exercise general logistics. They are not the game's starting state.
    public sealed class LogisticsTests : FoundationTestFixture
    {
        private ThingRegistry registry;
        private GridMap map;
        private LaboratoryManager simulation;
        private ResourceDefinition organic;
        private CreatureDefinition prototype;
        private FacilityDefinition gathering;
        private int definitions;

        [SetUp]
        public void PrepareLaboratory()
        {
            organic = CreateResourceDefinition("logistics.organic");
            prototype = CreateDefinition<CreatureDefinition>("logistics.prototype", ThingComponentKind.ResourceContainer);
            registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 12);
            simulation = new LaboratoryManager(registry, map, organic);
            gathering = FacilityDefinition(FacilityRole.Gathering);
        }

        [Test]
        public void SelectsNearestSupplyFirstEvenWhenAnotherTotalTripIsShorter()
        {
            var worker = Worker(3, 1);
            var nearSupply = CompletedGathering(4, 2);
            var fartherSupply = CompletedGathering(2, 5);
            var destination = Store(1, 5);
            Assert.That(simulation.Distance(worker, nearSupply), Is.LessThan(simulation.Distance(worker, fartherSupply)));
            Assert.That(simulation.Distance(worker, nearSupply) + simulation.Distance(nearSupply, destination),
                Is.GreaterThan(simulation.Distance(worker, fartherSupply) + simulation.Distance(fartherSupply, destination)),
                "The fixture must distinguish source-first selection from minimizing both trip legs together.");
            simulation.Advance(0.01);
            var reservation = simulation.Reservations.Single();
            Assert.That(reservation.SourceId, Is.EqualTo(nearSupply.Id));
            Assert.That(reservation.DestinationId, Is.EqualTo(destination.Id));
            Assert.That(reservation.Amount, Is.EqualTo(5));
            Assert.That(simulation.TotalAt(nearSupply.Id), Is.EqualTo(5), "Reservations do not move actual inventory.");
        }

        [Test]
        public void UsesObstacleAvoidingPathInsteadOfStraightLineDistance()
        {
            var worker = Worker(2, 3);
            var straightNear = CompletedGathering(4, 4);
            var pathNear = CompletedGathering(0, 7);
            Store(8, 8);
            var wallDefinition = CreateStorageDefinition("logistics.wall");
            using (var serialized = new SerializedObject(wallDefinition))
            {
                serialized.FindProperty("footprint").vector2IntValue = new Vector2Int(1, 6);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var wall = registry.Create(wallDefinition);
            Assert.That(map.Commands.Place(wall.Id, new Vector2Int(3, 0)).Succeeded, Is.True);
            Assert.That(Vector2Int.Distance(worker.Placement.Origin, straightNear.Placement.InteractionCell.Value),
                Is.LessThan(Vector2Int.Distance(worker.Placement.Origin, pathNear.Placement.InteractionCell.Value)));
            Assert.That(simulation.Distance(worker, straightNear), Is.GreaterThan(simulation.Distance(worker, pathNear)));
            simulation.Advance(0.01);
            Assert.That(simulation.Reservations.Single().SourceId, Is.EqualTo(pathNear.Id));
        }

        [Test]
        public void SupplyAndDestinationTiesKeepOriginalCreationOrderAfterMovement()
        {
            var worker = Worker(5, 1);
            var olderSupply = CompletedGathering(0, 0);
            var newerSupply = CompletedGathering(7, 3);
            var olderDestination = Store(10, 8);
            var newerDestination = Store(1, 5);
            var originalSupplyOrder = olderSupply.CreationOrder;
            var originalDestinationOrder = olderDestination.CreationOrder;
            Assert.That(map.Commands.Place(olderSupply.Id, new Vector2Int(3, 3)).Succeeded, Is.True);
            Assert.That(map.Commands.Place(olderDestination.Id, new Vector2Int(5, 5)).Succeeded, Is.True);
            simulation.OnMapChanged();
            Assert.That(simulation.Distance(worker, olderSupply), Is.EqualTo(simulation.Distance(worker, newerSupply)));
            Assert.That(simulation.Distance(olderSupply, olderDestination), Is.EqualTo(simulation.Distance(olderSupply, newerDestination)));
            simulation.Advance(0.01);
            var reservation = simulation.Reservations.Single();
            Assert.That(reservation.SourceId, Is.EqualTo(olderSupply.Id));
            Assert.That(reservation.DestinationId, Is.EqualTo(olderDestination.Id));
            Assert.That(olderSupply.CreationOrder, Is.EqualTo(originalSupplyOrder));
            Assert.That(olderDestination.CreationOrder, Is.EqualTo(originalDestinationOrder));
        }

        [Test]
        public void MultipleWorkersReserveOnlyRemainingStockAndCapacity()
        {
            using (var serialized = new SerializedObject(prototype))
            {
                serialized.FindProperty("carryCapacity").intValue = 3;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Worker(0, 0);
            Worker(0, 1);
            Worker(1, 0);
            var source = CompletedGathering(4, 4);
            var destination = Store(8, 4, 4);
            simulation.Advance(0.01);
            Assert.That(simulation.Reservations.Count, Is.EqualTo(2));
            Assert.That(simulation.Reservations.Select(r => r.Amount), Is.EquivalentTo(new[] { 3, 1 }));
            Assert.That(simulation.Reservations.Sum(r => r.Amount), Is.EqualTo(4));
            Assert.That(simulation.Reservations.All(r => r.SourceId == source.Id && r.DestinationId == destination.Id), Is.True);
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(5));
            AdvanceUntil(() => simulation.TotalAt(destination.Id) == 4, "Reserved shipments did not arrive.");
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(1));
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(1));
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            simulation.Advance(8);
            Assert.That(simulation.TotalAt(destination.Id), Is.EqualTo(4), "A full destination cannot be overfilled.");
            Assert.That(TotalOrganic(), Is.EqualTo(5), "The unfinished batch prevents another production cycle.");
        }

        [Test]
        public void PickupAndPartialIntakeDoNotFinishBatchBeforeEveryUnitIsStored()
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(2, 2);
            var firstDestination = Store(4, 2, 2);
            var secondDestination = Store(8, 2, 3);
            AdvanceUntil(() => simulation.TotalAt(worker.Id) > 0, "The worker never picked up output.");
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(2), "Trip quantity is limited by destination capacity.");
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(5));
            Assert.That(source.GetComponent<GatheringComponent>().Progress, Is.EqualTo(gathering.GatheringSeconds));
            AdvanceUntil(() => simulation.TotalAt(firstDestination.Id) == 2, "First partial shipment did not arrive.");
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(3));
            Assert.That(source.GetComponent<GatheringComponent>().Progress, Is.EqualTo(gathering.GatheringSeconds));
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            AdvanceUntil(() => simulation.TotalAt(secondDestination.Id) == 3, "Remaining output did not reach the second store.");
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
        }

        [Test]
        public void AllowanceChangeKeepsOutgoingConstructionReservation()
        {
            var worker = Worker(0, 1);
            var source = Store(3, 3);
            registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            var site = registry.Create(FacilityDefinition(FacilityRole.Storage));
            Assert.That(map.Commands.Place(site.Id, new Vector2Int(8, 3)).Succeeded, Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 1, 0, 2 }), Is.True);
            simulation.Advance(0.01);
            var reservation = simulation.Reservations.Single();
            Assert.That(reservation.SourceId, Is.EqualTo(source.Id));
            Assert.That(reservation.DestinationId, Is.EqualTo(site.Id));
            Assert.That(simulation.SetAllowed(source.Id, false), Is.True);
            Assert.That(simulation.Reservations.Single(), Is.SameAs(reservation), "Changed allowance must not cancel outgoing work.");
            Assert.That(reservation.PickedUp, Is.False);
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(5));
        }

        [Test]
        public void PickedConstructionMaterialsReturnToStorageWhenSiteBecomesUnreachable()
        {
            var worker = Worker(0, 1);
            var source = Store(3, 3);
            registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            var site = registry.Create(FacilityDefinition(FacilityRole.Storage));
            Assert.That(map.Commands.Place(site.Id, new Vector2Int(8, 3)).Succeeded, Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 1, 0, 2 }), Is.True);
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 2, "Construction materials were never picked up.");
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(3));
            Assert.That(simulation.Reservations.Single().DestinationId, Is.EqualTo(site.Id));
            // A legal placement may make the fixed interaction position inaccessible.
            Assert.That(map.Commands.Place(site.Id, new Vector2Int(8, 0)).Succeeded, Is.True);
            simulation.OnMapChanged();
            Assert.That(simulation.Reservations.Any(r => r.DestinationId == site.Id), Is.False,
                "Unreachable construction must release its incoming reservation after pickup.");
            Assert.That(site.GetComponent<ConstructionComponent>().Delivered, Is.Zero);
            AdvanceUntil(() => simulation.TotalAt(source.Id) == 5, "Carried construction materials were not recovered to storage.");
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
            Assert.That(site.GetComponent<ConstructionComponent>().Delivered, Is.Zero);
            Assert.That(site.GetComponent<ConstructionComponent>().IsComplete, Is.False);
            Assert.That(TotalOrganic(), Is.EqualTo(5), "Rerouting must not consume or duplicate construction materials.");
        }

        [Test]
        public void DisablingWorkFinishesConstructionDeliveryWithoutForcingConstructionWork()
        {
            var worker = Worker(0, 1);
            var source = Store(3, 3);
            registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            var site = registry.Create(FacilityDefinition(FacilityRole.Storage));
            Assert.That(map.Commands.Place(site.Id, new Vector2Int(8, 3)).Succeeded, Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 1, 0, 2 }), Is.True);
            simulation.Advance(0.01);
            Assert.That(simulation.Reservations.Single().DestinationId, Is.EqualTo(site.Id));
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 0, 0, 0 }), Is.True);
            AdvanceUntil(() => site.GetComponent<ConstructionComponent>().Delivered == 2,
                "Changing priorities must not interrupt an already assigned construction delivery.");
            Assert.That(site.GetComponent<ConstructionComponent>().Progress, Is.Zero);
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null,
                "Delivery completion must return to normal work selection instead of forcing construction work.");
            simulation.Advance(1);
            Assert.That(site.GetComponent<ConstructionComponent>().Progress, Is.Zero);
            Assert.That(site.GetComponent<ConstructionComponent>().IsComplete, Is.False);
            Assert.That(TotalOrganic(), Is.EqualTo(3), "Only the two delivered construction materials are consumed.");
        }

        [Test]
        public void ExcludingDestinationCancelsBothUnpickedReservationsAndSelectsAnotherStore()
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(3, 3);
            var firstDestination = Store(6, 3);
            var secondDestination = Store(10, 3);
            simulation.Advance(0.01);
            Assert.That(simulation.Reservations.Single().DestinationId, Is.EqualTo(firstDestination.Id));
            Assert.That(simulation.SetAllowed(firstDestination.Id, false), Is.True);
            Assert.That(simulation.Reservations.Any(r => r.DestinationId == firstDestination.Id), Is.False,
                "The changed allowance applies immediately to existing incoming reservations.");
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(5));
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
            simulation.Advance(0.01);
            var replacement = simulation.Reservations.Single();
            Assert.That(replacement.SourceId, Is.EqualTo(source.Id));
            Assert.That(replacement.DestinationId, Is.EqualTo(secondDestination.Id));
            Assert.That(replacement.Amount, Is.EqualTo(5), "Released outgoing quantities remain available to the replacement work.");
        }

        [Test]
        public void PickedOutputWaitsWithoutLossThenSplitsAcrossRecoveredStores()
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(2, 2);
            var originalDestination = Store(7, 2, 5);
            var partialDestination = Store(4, 4, 2, false);
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 5, "The worker never picked up the completed batch.");
            Assert.That(simulation.SetAllowed(originalDestination.Id, false), Is.True);
            var waitingCell = worker.Placement.Origin;
            simulation.Advance(3);
            Assert.That(worker.Placement.Origin, Is.EqualTo(waitingCell));
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(5));
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(5));
            Assert.That(worker.GetComponent<WorkerComponent>().WaitReason, Is.Not.Null.And.Not.Empty);
            Assert.That(simulation.Reservations.Any(r => r.DestinationId == originalDestination.Id), Is.False);
            Assert.That(simulation.SetAllowed(partialDestination.Id, true), Is.True);
            AdvanceUntil(() => simulation.TotalAt(partialDestination.Id) == 2, "The recovered partial store was not used.");
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(3));
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(3));
            simulation.Advance(2);
            Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(3));
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            Assert.That(simulation.SetAllowed(originalDestination.Id, true), Is.True);
            AdvanceUntil(() => simulation.TotalAt(originalDestination.Id) == 3, "The remaining carried output never resumed.");
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
        }

        [Test]
        public void NormalHaulingKeepsItsTargetWhenCloserSupplyAndHigherPriorityWorkAppear()
        {
            var worker = Worker(0, 1);
            var originalSupply = CompletedGathering(6, 3);
            var destination = Store(10, 3);
            simulation.Advance(0.01);
            var original = simulation.Reservations.Single();
            Assert.That(original.SourceId, Is.EqualTo(originalSupply.Id));
            CompletedGathering(1, 3);
            var construction = registry.Create(FacilityDefinition(FacilityRole.Storage));
            Assert.That(map.Commands.Place(construction.Id, new Vector2Int(2, 5)).Succeeded, Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 1, 2, 3 }), Is.True);
            simulation.OnMapChanged();
            simulation.Advance(0.1);
            Assert.That(simulation.Reservations.Single(r => r.WorkerId == worker.Id), Is.SameAs(original));
            Assert.That(original.SourceId, Is.EqualTo(originalSupply.Id));
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 5, "The original shipment was interrupted before pickup.");
            Assert.That(simulation.Reservations.Single(r => r.WorkerId == worker.Id).DestinationId, Is.EqualTo(destination.Id));
            AdvanceUntil(() => simulation.TotalAt(destination.Id) >= 5, "Higher-priority work interrupted normal hauling after pickup.");
            Assert.That(originalSupply.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
        }

        [Test]
        public void EditingFreezesMovementJobsReservationsAndBatchUntilCancelled()
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(5, 3);
            var destination = Store(10, 3);
            simulation.Advance(0.1);
            var position = worker.Placement.Origin;
            var workerState = worker.GetComponent<WorkerComponent>();
            var moveProgress = workerState.MoveProgress;
            var job = workerState.Job;
            var reservation = simulation.Reservations.Single();
            var outstanding = source.GetComponent<GatheringComponent>().Outstanding;
            Assert.That(map.Commands.BeginEdit(destination.Id).Succeeded, Is.True);
            Assert.That(map.Commands.Preview(new Vector2Int(11, 5), QuarterTurn.Clockwise90).Succeeded, Is.True);
            simulation.Advance(30);
            Assert.That(worker.Placement.Origin, Is.EqualTo(position));
            Assert.That(workerState.MoveProgress, Is.EqualTo(moveProgress));
            Assert.That(workerState.Job, Is.SameAs(job));
            Assert.That(simulation.Reservations.Single(), Is.SameAs(reservation));
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(outstanding));
            Assert.That(map.Commands.Cancel().Succeeded, Is.True);
            AdvanceUntil(() => simulation.TotalAt(destination.Id) == 5, "Cancelling an edit did not resume the original work.");
        }

        [Test]
        public void ExcludedStoredStacksRelocateAsOneFullLoadAndKeepOutgoingWorkWhenAllowedAgain()
        {
            var worker = Worker(0, 1);
            var source = Store(3, 3);
            var destination = Store(8, 3);
            var container = source.GetComponent<ResourceContainerComponent>();
            registry.CreateResource(organic, 2, container);
            registry.CreateResource(organic, 3, container);
            Assert.That(container.Contents.Count(), Is.EqualTo(2));
            simulation.Advance(0.1);
            Assert.That(simulation.Reservations, Is.Empty, "Allowed stored inventory should stay in its current store.");
            Assert.That(simulation.SetAllowed(source.Id, false), Is.True);
            simulation.Advance(0.01);
            var reservation = simulation.Reservations.Single();
            Assert.That(reservation.SourceId, Is.EqualTo(source.Id));
            Assert.That(reservation.DestinationId, Is.EqualTo(destination.Id));
            Assert.That(reservation.Amount, Is.EqualTo(5), "Unreserved chunks of one resource must fill the available carrying capacity.");
            Assert.That(simulation.Logistics.OutgoingReserved(reservation.ResourceId), Is.EqualTo(5));
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(5));
            AssertSingleOwnership();
            Assert.That(simulation.SetAllowed(source.Id, true), Is.True);
            Assert.That(simulation.Reservations.Single(), Is.SameAs(reservation),
                "An allowance change at the outgoing source must not preempt its normal reserved transport.");
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 5, "Excluded stored inventory was not collected.");
            Assert.That(simulation.TotalAt(source.Id), Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            AssertSingleOwnership();
            AdvanceUntil(() => simulation.TotalAt(destination.Id) == 5, "Stored inventory relocation did not finish.");
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            AssertSingleOwnership();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BlockingUnpickedInteractionReleasesBothReservationsAndMovingBlockerRecovers(bool blockSource)
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(3, 3);
            var destination = Store(8, 3);
            simulation.Advance(0.01);
            var reservation = simulation.Reservations.Single();
            var resourceId = reservation.ResourceId;
            Assert.That(reservation.PickedUp, Is.False);
            var blocker = registry.Create(CreateStorageDefinition("logistics.interaction-blocker"));
            var blockedCell = (blockSource ? source : destination).Placement.InteractionCell.Value;
            Assert.That(map.Commands.Place(blocker.Id, blockedCell).Succeeded, Is.True);
            simulation.OnMapChanged();
            Assert.That(simulation.Reservations, Is.Empty, "An impossible unpicked route must cancel both promises immediately.");
            Assert.That(simulation.Logistics.OutgoingReserved(resourceId), Is.Zero);
            Assert.That(simulation.Logistics.IncomingReserved(destination.Id), Is.Zero);
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null);
            Assert.That(simulation.TotalAt(source.Id), Is.EqualTo(5));
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
            simulation.Advance(1);
            Assert.That(simulation.Reservations, Is.Empty, "A blocked interaction must not be selected again.");
            Assert.That(map.Commands.BeginEdit(blocker.Id).Succeeded, Is.True);
            Assert.That(map.Commands.Preview(new Vector2Int(12, 8), QuarterTurn.None).Succeeded, Is.True);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
            simulation.OnMapChanged();
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 5, "Moving the obstruction did not recover pickup.");
            Assert.That(worker.Placement.Origin, Is.EqualTo(source.Placement.InteractionCell.Value));
            Assert.That(simulation.Reservations.Single().PickedUp, Is.True);
            AssertSingleOwnership();
            AdvanceUntil(() => simulation.TotalAt(destination.Id) == 5, "The recovered shipment never reached storage.");
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
        }

        [Test]
        public void MovingAndRotatingReachableDestinationKeepsPickedJobAndReservationUntilNewInteraction()
        {
            var worker = Worker(0, 1);
            var source = CompletedGathering(2, 2);
            var destination = Store(8, 3);
            AdvanceUntil(() => simulation.TotalAt(worker.Id) == 5, "The completed batch was never picked up.");
            var job = worker.GetComponent<WorkerComponent>().Job;
            var reservation = simulation.Reservations.Single();
            var previousInteraction = destination.Placement.InteractionCell.Value;
            Assert.That(map.Commands.BeginEdit(destination.Id).Succeeded, Is.True);
            Assert.That(map.Commands.Preview(new Vector2Int(11, 6), QuarterTurn.Clockwise90).Succeeded, Is.True);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
            simulation.OnMapChanged();
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.SameAs(job));
            Assert.That(simulation.Reservations.Single(), Is.SameAs(reservation));
            Assert.That(reservation.DestinationId, Is.EqualTo(destination.Id));
            Assert.That(reservation.PickedUp, Is.True);
            Assert.That(reservation.Amount, Is.EqualTo(5));
            Assert.That(destination.Placement.InteractionCell.Value, Is.EqualTo(new Vector2Int(10, 6)));
            Assert.That(destination.Placement.InteractionCell.Value, Is.Not.EqualTo(previousInteraction));
            AdvanceUntil(() => simulation.TotalAt(destination.Id) == 5, "The existing shipment did not follow the moved facility.");
            Assert.That(worker.Placement.Origin, Is.EqualTo(destination.Placement.InteractionCell.Value),
                "Actual intake requires reaching the new rotated interaction position.");
            Assert.That(source.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(TotalOrganic(), Is.EqualTo(5));
            AssertSingleOwnership();
        }

        private Thing Worker(int x, int y)
        {
            var worker = registry.Create(prototype);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 0, 0, 1 }), Is.True);
            return worker;
        }

        private Thing CompletedGathering(int x, int y)
        {
            var source = CompleteFacility(gathering, x, y);
            registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            SetPreparedState(source.GetComponent<GatheringComponent>(), nameof(GatheringComponent.Progress), gathering.GatheringSeconds);
            SetPreparedState(source.GetComponent<GatheringComponent>(), nameof(GatheringComponent.Outstanding), 5);
            return source;
        }

        private Thing Store(int x, int y, int capacity = 20, bool allowed = true)
        {
            var store = CompleteFacility(FacilityDefinition(FacilityRole.Storage, capacity), x, y);
            Assert.That(simulation.SetAllowed(store.Id, allowed), Is.True);
            return store;
        }

        private Thing CompleteFacility(FacilityDefinition definition, int x, int y)
        {
            var facility = registry.Create(definition);
            Assert.That(map.Commands.Place(facility.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            var construction = facility.GetComponent<ConstructionComponent>();
            SetPreparedState(construction, nameof(ConstructionComponent.IsComplete), true);
            SetPreparedState(construction, nameof(ConstructionComponent.Delivered), definition.BuildCost);
            SetPreparedState(construction, nameof(ConstructionComponent.Progress), definition.BuildSeconds);
            return facility;
        }

        private FacilityDefinition FacilityDefinition(FacilityRole role, int capacity = 20)
        {
            var definition = CreateDefinition<FacilityDefinition>("logistics.facility." + ++definitions, ThingComponentKind.ResourceContainer);
            using (var serialized = new SerializedObject(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.FindProperty("footprint").vector2IntValue = Vector2Int.one;
                serialized.FindProperty("interactionOffset").vector2IntValue = Vector2Int.down;
                serialized.FindProperty("storageCapacity").intValue = capacity;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            definition.Validate();
            return definition;
        }

        private int TotalOrganic() => registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
            .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);

        private void AssertSingleOwnership()
        {
            foreach (var resource in registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource))
            {
                Assert.That(resource.HolderId.HasValue, Is.True);
                Assert.That(registry.TryGet(resource.HolderId.Value, out var holder), Is.True);
                Assert.That(holder.GetComponent<ResourceContainerComponent>().Contents.Count(t => t.Id == resource.Id), Is.EqualTo(1));
                Assert.That(registry.Things.Where(t => t.TryGetComponent<ResourceContainerComponent>(out _))
                    .Sum(t => t.GetComponent<ResourceContainerComponent>().Contents.Count(item => item.Id == resource.Id)), Is.EqualTo(1),
                    "Each live resource may appear in exactly one container.");
            }
        }

        private void AdvanceUntil(Func<bool> condition, string message)
        {
            for (var step = 0; step < 3000 && !condition(); step++) simulation.Advance(0.025);
            Assert.That(condition(), Is.True, message + "\n" + string.Join("; ", registry.Things
                .Where(t => t.TryGetComponent<WorkerComponent>(out _)).Select(t =>
                {
                    var state = t.GetComponent<WorkerComponent>();
                    return t.Id + " @ " + t.Placement?.Origin + " " + state.Job?.Kind + "/" + state.Job?.Stage + " " + state.WaitReason;
                })));
        }

        private static void SetPreparedState(object target, string property, object value)
        {
            var member = target.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(member, Is.Not.Null, "Fixture preparation field must exist.");
            member.SetValue(target, value);
        }
    }
}
