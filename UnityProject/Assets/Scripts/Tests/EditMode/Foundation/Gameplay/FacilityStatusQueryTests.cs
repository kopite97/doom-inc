using System;
using System.Linq;
using MobileGame.Gameplay;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    // Completed buildings and stock are explicit fixtures. Transitions use actual jobs and commands.
    public sealed class FacilityStatusQueryTests : FoundationTestFixture
    {
        private ResourceData organic, culture;
        private FacilityData storage, gathering, processing, cultivation;
        private CreatureData prototype, support;
        private ThingRegistry registry;
        private GridMap map;
        private LaboratoryManager laboratory;
        private FacilityStatusQuery query;

        [SetUp]
        public void Prepare()
        {
            organic = Load<ResourceData>("Resource/OrganicMatter");
            culture = Load<ResourceData>("Resource/CultureMedium");
            storage = Load<FacilityData>("Facility/Storage");
            gathering = Load<FacilityData>("Facility/Gathering");
            processing = Load<FacilityData>("Facility/Processing");
            cultivation = Load<FacilityData>("Facility/Cultivation");
            prototype = Load<CreatureData>("Creature/PrototypeSupport");
            support = Load<CreatureData>("Creature/Support");
            registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 10);
            laboratory = new LaboratoryManager(registry, map, organic, culture);
            query = new FacilityStatusQuery(laboratory);
        }

        [Test]
        public void CompletedStorageHasNoProductionBarButItsUnfinishedConstructionDoes()
        {
            Worker(prototype, 1, 1, 0, 0, 0, 0);
            var complete = Facility(storage, 4, 4);
            var site = Site(storage, 10, 4);
            var stored = query.Read(complete);
            Assert.That(stored.Kind, Is.EqualTo(FacilityStatusKind.Storage));
            Assert.That(stored.HasProgress, Is.False);
            Assert.That(stored.ProgressFraction, Is.Zero);
            var building = query.Read(site);
            Assert.That(building.Kind, Is.EqualTo(FacilityStatusKind.ConstructionMaterials));
            Assert.That(building.HasProgress, Is.True);
            Assert.That(building.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.InsufficientTotal));
        }

        [Test]
        public void ValidSiteWithNoMaterialsCanWaitForDeliveryAndThenConstruction()
        {
            var worker = Worker(prototype, 4, 3, 1, 0, 0, 0);
            var site = Site(storage, 4, 4);
            Assert.That(query.Read(site).Materials.Uncovered, Is.EqualTo(storage.BuildCost));
            Stock(worker, organic, storage.BuildCost);
            laboratory.Advance(0.05);
            var delivering = query.Read(site);
            Assert.That(delivering.Materials.Incoming, Is.EqualTo(storage.BuildCost));
            Assert.That(delivering.Materials.InTransit, Is.EqualTo(storage.BuildCost));
            Assert.That(delivering.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.DeliveryPending));
            Assert.That(delivering.ProgressFraction, Is.Zero, "Incoming material is not actual construction work.");
            Until(() => site.GetComponent<ConstructionComponent>().Delivered == storage.BuildCost);
            var delivered = query.Read(site);
            Assert.That(delivered.Materials.Delivered, Is.EqualTo(storage.BuildCost));
            Assert.That(delivered.Materials.TotalStock, Is.Zero, "Delivered construction material was consumed, not retained as stock.");
            Assert.That(delivered.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.None));
            Until(() => query.Read(site).Kind == FacilityStatusKind.ConstructionWorking);
            Assert.That(query.Read(site).WorkingCount, Is.EqualTo(1));
            Until(() => laboratory.IsComplete(site));
            Assert.That(query.Read(site).HasProgress, Is.False);
            laboratory.ValidateState();
        }

        [Test]
        public void AnotherSitesReservationIsDistinctFromTotalMaterialShortage()
        {
            Worker(prototype, 1, 1, 1, 0, 0, 0);
            var source = Facility(storage, 8, 7);
            Stock(source, organic, storage.BuildCost);
            var first = Site(storage, 4, 4);
            var second = Site(storage, 10, 4);
            laboratory.Advance(0.05);
            Assert.That(query.Read(first).Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.DeliveryPending));
            var waiting = query.Read(second);
            Assert.That(waiting.Materials.TotalStock, Is.EqualTo(storage.BuildCost));
            Assert.That(waiting.Materials.Incoming, Is.Zero);
            Assert.That(waiting.Materials.ReservedElsewhere, Is.EqualTo(storage.BuildCost));
            Assert.That(waiting.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.ReservedElsewhere));
            laboratory.ValidateState();
        }

        [Test]
        public void AssignedWorkerInTransitDoesNotDisplayStaffedProduction()
        {
            var worker = Worker(support, 1, 1, 0, 0, 0, 1);
            var facility = Facility(processing, 10, 6);
            Stock(facility, organic, processing.InputAmount);
            Assert.That(laboratory.AssignWorker(worker.Id, facility.Id), Is.True);
            laboratory.Advance(0.05);
            var automatic = query.Read(facility);
            Assert.That(automatic.AssignedCount, Is.EqualTo(1));
            Assert.That(automatic.WorkingCount, Is.Zero);
            Assert.That(automatic.IsAutomatic, Is.True);
            Assert.That(automatic.Rate, Is.EqualTo(laboratory.Settings.UnattendedRate));
            Assert.That(automatic.InputsConsumed, Is.True);
            Assert.That(automatic.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.None));
            Until(() => worker.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Working);
            var staffed = query.Read(facility);
            Assert.That(staffed.Kind, Is.EqualTo(FacilityStatusKind.ProductionWorking));
            Assert.That(staffed.WorkingCount, Is.EqualTo(1));
            Assert.That(staffed.Rate, Is.EqualTo(laboratory.Settings.StaffedRate));
            Assert.That(staffed.ProgressFraction, Is.LessThan(1), "Production speed is not the bar's completion percentage.");
            var progress = staffed.Progress;
            Assert.That(laboratory.AssignWorker(worker.Id, null), Is.True);
            Assert.That(query.Read(facility).IsAutomatic, Is.True);
            Assert.That(query.Read(facility).Progress, Is.EqualTo(progress));
            laboratory.ValidateState();
        }

        [Test]
        public void GatheringNeverBecomesAutomaticAndInterruptedProgressIsPreserved()
        {
            var worker = Worker(support, 4, 3, 0, 1, 0, 0);
            var facility = Facility(gathering, 4, 4);
            laboratory.Advance(1);
            var before = query.Read(facility);
            Assert.That(before.Kind, Is.EqualTo(FacilityStatusKind.GatheringWorking));
            Assert.That(before.Progress, Is.GreaterThan(0));
            laboratory.SetWorkerPriorities(worker.Id, new[] { 0, 0, 0, 0 });
            laboratory.Advance(0.05);
            // Priority changes preserve active gathering; explicit assignment change cancels it.
            var processor = Facility(processing, 10, 6);
            Assert.That(laboratory.AssignWorker(worker.Id, processor.Id), Is.True);
            var waiting = query.Read(facility);
            Assert.That(waiting.Kind, Is.EqualTo(FacilityStatusKind.GatheringWaiting));
            Assert.That(waiting.IsAutomatic, Is.False);
            Assert.That(waiting.Rate, Is.Zero);
            Assert.That(waiting.Progress, Is.GreaterThanOrEqualTo(before.Progress));
        }

        [Test]
        public void PartialProductionInputsAndReservedDeliveryAreNotCountedTwice()
        {
            Worker(prototype, 1, 1, 0, 0, 1, 0);
            var source = Facility(storage, 4, 4);
            Stock(source, organic, 10);
            var facility = Facility(processing, 10, 4);
            Stock(facility, organic, 2);
            laboratory.Advance(0.05);
            var waiting = query.Read(facility);
            Assert.That(waiting.Kind, Is.EqualTo(FacilityStatusKind.MaterialWaiting));
            Assert.That(waiting.Materials.Delivered, Is.EqualTo(2));
            Assert.That(waiting.Materials.Incoming, Is.EqualTo(processing.InputAmount - 2));
            Assert.That(waiting.Materials.Uncovered, Is.Zero);
            Assert.That(waiting.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.DeliveryPending));
            Assert.That(waiting.Materials.TotalStock, Is.EqualTo(12), "Reservations are not extra physical stock.");
            Until(() => laboratory.Reservations.Any(r => r.DestinationId == facility.Id && r.PickedUp));
            Assert.That(query.Read(facility).Materials.InTransit, Is.EqualTo(processing.InputAmount - 2));
            Until(() => facility.GetComponent<ProductionComponent>().InProgress);
            var started = query.Read(facility);
            Assert.That(started.InputsConsumed, Is.True);
            Assert.That(started.Materials.Delivered, Is.Zero);
            Assert.That(started.Materials.Incoming, Is.Zero);
            Assert.That(started.Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.None));
            laboratory.ValidateState();
        }

        [Test]
        public void CompletedProcessingOutputKeepsFullBarEvenWhenItsWorkTileBecomesBlocked()
        {
            Worker(prototype, 1, 1, 0, 0, 0, 0);
            var facility = Facility(processing, 7, 6);
            Stock(facility, organic, processing.InputAmount);
            Until(() => facility.GetComponent<ProductionComponent>().Outstanding > 0);
            var blocker = registry.Create(CreateStorageDefinition("status.output-blocker"));
            Assert.That(map.Commands.Place(blocker.Id, facility.Placement.InteractionCell.Value).Succeeded, Is.True);
            laboratory.OnMapChanged();
            var completed = query.Read(facility);
            Assert.That(completed.Kind, Is.EqualTo(FacilityStatusKind.OutputWaiting));
            Assert.That(completed.Outstanding, Is.EqualTo(processing.OutputAmount));
            Assert.That(completed.ProgressFraction, Is.EqualTo(1));
            Assert.That(completed.Rate, Is.Zero);
            Assert.That(completed.Materials.Uncovered, Is.Zero);
            laboratory.ValidateState();
        }

        [Test]
        public void PickupAndPartialAdmissionKeepCompletedBarUntilLastActualAdmission()
        {
            var worker = Worker(prototype, 4, 3, 0, 1, 2, 0);
            var facility = Facility(gathering, 4, 4);
            var store = Facility(storage, 10, 4);
            Stock(store, organic, storage.StorageCapacity - 2);
            Until(() => laboratory.Reservations.Any(r => r.BatchOriginId == facility.Id && r.PickedUp));
            var picked = query.Read(facility);
            Assert.That(picked.Kind, Is.EqualTo(FacilityStatusKind.OutputWaiting));
            Assert.That(picked.Outstanding, Is.EqualTo(gathering.GatheringAmount));
            Assert.That(picked.ProgressFraction, Is.EqualTo(1));
            Until(() => facility.GetComponent<GatheringComponent>().Outstanding == gathering.GatheringAmount - 2);
            Assert.That(query.Read(facility).ProgressFraction, Is.EqualTo(1));
            Assert.That(query.Read(facility).Kind, Is.EqualTo(FacilityStatusKind.OutputWaiting));
            laboratory.SetWorkerPriorities(worker.Id, new[] { 0, 0, 1, 0 });
            Facility(storage, 10, 7);
            Until(() => facility.GetComponent<GatheringComponent>().Outstanding == 0);
            Assert.That(query.Read(facility).ProgressFraction, Is.Zero);
            laboratory.ValidateState();
        }

        [Test]
        public void BlockedWorkTilePreservesConsumedCycleAndTargetChangeRemainsNextCycle()
        {
            Worker(prototype, 1, 1, 0, 0, 0, 0);
            var facility = Facility(cultivation, 7, 6);
            Assert.That(query.Read(facility).Kind, Is.EqualTo(FacilityStatusKind.TargetRequired));
            laboratory.SelectCultivation(facility.Id, CultivationTarget.Support);
            Stock(facility, culture, cultivation.InputAmount);
            laboratory.Advance(1);
            laboratory.SelectCultivation(facility.Id, CultivationTarget.Invader);
            var before = query.Read(facility);
            var blocker = registry.Create(CreateStorageDefinition("status.blocker"));
            Assert.That(map.Commands.Place(blocker.Id, facility.Placement.InteractionCell.Value).Succeeded, Is.True);
            laboratory.OnMapChanged();
            laboratory.Advance(2);
            var blocked = query.Read(facility);
            Assert.That(blocked.Kind, Is.EqualTo(FacilityStatusKind.Unreachable));
            Assert.That(blocked.Progress, Is.EqualTo(before.Progress));
            Assert.That(blocked.InputsConsumed, Is.True);
            Assert.That(blocked.Materials.Uncovered, Is.Zero);
            Assert.That(blocked.CurrentTarget, Is.EqualTo(CultivationTarget.Support));
            Assert.That(blocked.NextTarget, Is.EqualTo(CultivationTarget.Invader));
            Assert.That(blocked.Rate, Is.Zero);
            laboratory.ValidateState();
        }

        [Test]
        public void SameDefinitionFacilitiesAreIndependentAndReadingDoesNotAdvanceOrReserve()
        {
            Worker(prototype, 1, 1, 0, 0, 0, 0);
            var first = Facility(processing, 4, 4);
            var second = Facility(processing, 10, 4);
            Stock(first, organic, processing.InputAmount);
            laboratory.Advance(1);
            var before = query.Read(first);
            var count = registry.Things.Count();
            for (var i = 0; i < 20; i++)
            {
                Assert.That(query.Read(first).Progress, Is.EqualTo(before.Progress));
                Assert.That(query.Read(first).IsAutomatic, Is.True);
                Assert.That(query.Read(second).Kind, Is.EqualTo(FacilityStatusKind.MaterialWaiting));
                Assert.That(query.Read(second).ProgressFraction, Is.Zero);
            }
            Assert.That(registry.Things.Count(), Is.EqualTo(count));
            Assert.That(laboratory.Reservations, Is.Empty);
            laboratory.ValidateState();
        }

        [Test]
        public void MissingRemovedAndCreatureIdsDoNotProduceFacilityStatus()
        {
            var worker = Worker(prototype, 1, 1, 0, 0, 0, 0);
            var facility = Facility(storage, 4, 4);
            Assert.That(query.TryRead(facility.Id, out var valid), Is.True);
            Assert.That(valid.FacilityId, Is.EqualTo(facility.Id));
            Assert.That(query.TryRead(worker.Id, out _), Is.False);
            Assert.That(query.TryRead(Guid.NewGuid(), out _), Is.False);
            registry.Remove(facility);
            Assert.That(query.TryRead(facility.Id, out _), Is.False);
        }

        private Thing Worker(CreatureData definition, int x, int y, params int[] priorities)
        {
            var thing = registry.Create(definition);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            Assert.That(laboratory.SetWorkerPriorities(thing.Id, priorities), Is.True);
            return thing;
        }

        private Thing Site(FacilityData definition, int x, int y)
        {
            Assert.That(laboratory.BeginConstruction(definition), Is.True);
            Assert.That(laboratory.PreviewConstruction(new Vector2Int(x, y), QuarterTurn.None), Is.True);
            Assert.That(laboratory.ConfirmConstruction(), Is.True);
            return registry.Things.Where(t => t.Definition == definition).OrderBy(t => t.CreationOrder).Last();
        }

        private Thing Facility(FacilityData definition, int x, int y)
        {
            var thing = registry.Create(definition);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            var state = thing.GetComponent<ConstructionComponent>();
            Set(state, "Delivered", definition.BuildCost);
            Set(state, "Progress", (double)definition.BuildSeconds);
            Set(state, "IsComplete", true);
            return thing;
        }

        private void Stock(Thing holder, ResourceData resource, int amount)
            => registry.CreateResource(resource, amount, holder.GetComponent<ResourceContainerComponent>());
        private static T Load<T>(string path) where T : ThingData
            => DataTestAssets.LoadAssetAtPath<T>("Assets/SO/" + path + ".asset");
        private static void Set(object target, string property, object value)
            => target.GetType().GetProperty(property).GetSetMethod(true).Invoke(target, new[] { value });
        private void Until(Func<bool> condition)
        {
            for (var elapsed = 0d; elapsed < 120 && !condition(); elapsed += 0.05) laboratory.Advance(0.05);
            Assert.That(condition(), Is.True, "State was not reached within 120 simulated seconds.");
        }
    }
}
