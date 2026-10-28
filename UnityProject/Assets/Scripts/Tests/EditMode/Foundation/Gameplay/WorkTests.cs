using MobileGame.TestSupport;
using System;
using System.Linq;
using System.Reflection;
using MobileGame.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    // Prepared completed/paid facilities isolate work rules; actual initial building is
    // covered separately through the laboratory commands and resource delivery path.
    public sealed class WorkTests : FoundationTestFixture
    {
        private ResourceData organic;
        private CreatureData prototype;
        private ThingRegistry registry;
        private GridMap map;
        private LaboratoryManager simulation;
        private int definitionNumber;

        [SetUp]
        public void SetUpLaboratory()
        {
            organic = CreateResourceDefinition("work.organic");
            prototype = CreateDefinition<CreatureData>("work.prototype", ThingComponentKind.ResourceContainer);
            registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 10);
            simulation = new LaboratoryManager(registry, map, organic);
        }

        [Test]
        public void GatheringRequiresFiveActualSecondsAndWaitsForEveryOutputToBeStored()
        {
            var facility = CompletedGathering(4, 4);
            simulation.Advance(20);
            Assert.That(simulation.TotalAt(facility.Id), Is.Zero, "An unstaffed gathering facility never produces automatically.");
            Worker(4, 3);
            simulation.Advance(4.9);
            Assert.That(facility.GetComponent<GatheringComponent>().Progress, Is.EqualTo(4.9).Within(1e-6));
            Assert.That(simulation.TotalAt(facility.Id), Is.Zero);
            simulation.Advance(0.1);
            Assert.That(simulation.TotalAt(facility.Id), Is.EqualTo(5));
            Assert.That(facility.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(5));
            simulation.Advance(20);
            Assert.That(simulation.TotalAt(facility.Id), Is.EqualTo(5), "No destination exists, so the complete batch remains pending.");
            Assert.That(facility.GetComponent<GatheringComponent>().Progress, Is.EqualTo(5));
            simulation.ValidateState();
        }

        [Test]
        public void EditingAndMovingToChangedInteractionCellDoNotCountAsGatheringTime()
        {
            var facility = CompletedGathering(4, 4);
            var worker = Worker(4, 3);
            simulation.Advance(2);
            Assert.That(map.Commands.BeginEdit(facility.Id).Succeeded, Is.True);
            simulation.Advance(30);
            Assert.That(facility.GetComponent<GatheringComponent>().Progress, Is.EqualTo(2).Within(1e-6));
            Assert.That(map.Commands.Preview(new Vector2Int(8, 4), QuarterTurn.None).Succeeded, Is.True);
            Assert.That(map.Commands.Confirm().Succeeded, Is.True);
            simulation.OnMapChanged();
            simulation.Advance(1);
            Assert.That(worker.Placement.Origin, Is.EqualTo(new Vector2Int(8, 3)));
            Assert.That(facility.GetComponent<GatheringComponent>().Progress, Is.EqualTo(2).Within(1e-6),
                "The movement delta cannot also pay for production, including the arrival step.");
            simulation.Advance(2.9);
            Assert.That(simulation.TotalAt(facility.Id), Is.Zero);
            simulation.Advance(0.1);
            Assert.That(simulation.TotalAt(facility.Id), Is.EqualTo(5));
            simulation.ValidateState();
        }

        [Test]
        public void OneWorkerUsesFacilityAndReplacementContinuesItsSavedProgress()
        {
            var facility = CompletedGathering(4, 4);
            var first = Worker(4, 3);
            var second = Worker(4, 3);
            simulation.Advance(2);
            Assert.That(facility.GetComponent<GatheringComponent>().Progress, Is.EqualTo(2).Within(1e-6));
            Assert.That(first.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Gathering));
            Assert.That(second.GetComponent<WorkerComponent>().Job, Is.Null);
            registry.Remove(first);
            simulation.Advance(3);
            Assert.That(simulation.TotalAt(facility.Id), Is.EqualTo(5));
            Assert.That(facility.GetComponent<GatheringComponent>().Outstanding, Is.EqualTo(5));
            simulation.ValidateState();
        }

        [Test]
        public void PresetDisabledWorkOverrideAndEqualPriorityOrderUseCommonSettings()
        {
            var gathering = CompletedGathering(4, 4);
            var construction = Facility(FacilityRole.Storage, 8, 4);
            Set(construction.GetComponent<ConstructionComponent>(), "Delivered", 2);
            var worker = Worker(4, 3);
            Assert.That(simulation.SetPreset(new[] { 0, 0, 0 }), Is.True);
            simulation.Advance(0.1);
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null);
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.Zero);

            Assert.That(simulation.SetPreset(new[] { 1, 1, 1 }), Is.True);
            simulation.Advance(0.1);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Construction),
                "At equal priority the first listed work kind wins, even when gathering is closer.");
            Assert.That(worker.GetComponent<WorkerComponent>().Job.TargetId, Is.EqualTo(construction.Id));

            Assert.That(simulation.SetWorkerPriorities(worker.Id, new[] { 2, 1, 0 }), Is.True);
            simulation.Advance(0.1);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Gathering),
                "Movement to non-transport work reconsiders the new per-creature preference immediately.");
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.EqualTo(0.1).Within(1e-6));
            Assert.That(construction.GetComponent<ConstructionComponent>().Progress, Is.Zero);
            simulation.SetPreset(new[] { 0, 0, 0 });
            Assert.That(worker.GetComponent<WorkerComponent>().Priorities, Is.EqualTo(new[] { 2, 1, 0 }),
                "A changed preset must not replace an individual override.");
            simulation.ValidateState();
        }

        [Test]
        public void ConstructionChoosesNearestReachableSiteAcrossMaterialAndPaidWork()
        {
            var near = Facility(FacilityRole.Storage, 3, 4);
            var paid = Facility(FacilityRole.Storage, 8, 4);
            Set(paid.GetComponent<ConstructionComponent>(), "Delivered", 2);
            var worker = Worker(4, 3);
            registry.CreateResource(organic, 2, worker.GetComponent<ResourceContainerComponent>());
            simulation.Advance(0.01);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Construction));
            Assert.That(worker.GetComponent<WorkerComponent>().Job.TargetId, Is.EqualTo(near.Id),
                "A nearby available material-delivery job must not lose to farther already-paid construction.");
            Assert.That(simulation.Reservations.Single().DestinationId, Is.EqualTo(near.Id));
            Assert.That(paid.GetComponent<ConstructionComponent>().Progress, Is.Zero);
            simulation.ValidateState();
        }

        [Test]
        public void ArrivalStillReconsidersChangedPriorityBeforeActualWorkStarts()
        {
            var gathering = CompletedGathering(4, 4);
            var construction = Facility(FacilityRole.Storage, 8, 4);
            Set(construction.GetComponent<ConstructionComponent>(), "Delivered", 2);
            var worker = Worker(3, 3);
            simulation.SetWorkerPriorities(worker.Id, new[] { 2, 1, 0 });
            simulation.Advance(0.25);
            Assert.That(worker.Placement.Origin, Is.EqualTo(new Vector2Int(4, 3)));
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.ToDestination));
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.Zero);

            simulation.SetWorkerPriorities(worker.Id, new[] { 1, 2, 0 });
            simulation.Advance(0.05);

            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Construction));
            Assert.That(worker.GetComponent<WorkerComponent>().Job.TargetId, Is.EqualTo(construction.Id));
            Assert.That(gathering.GetComponent<GatheringComponent>().Progress, Is.Zero,
                "Reconciliation must not turn arrival into a committed gathering cycle before selecting current priorities.");
            simulation.ValidateState();
        }

        private Thing Worker(int x, int y)
        {
            var worker = registry.Create(prototype);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            return worker;
        }

        private Thing CompletedGathering(int x, int y)
        {
            var facility = Facility(FacilityRole.Gathering, x, y);
            var definition = (FacilityData)facility.Definition;
            var construction = facility.GetComponent<ConstructionComponent>();
            Set(construction, "Delivered", definition.BuildCost);
            Set(construction, "Progress", definition.BuildSeconds);
            Set(construction, "IsComplete", true);
            return facility;
        }

        private Thing Facility(FacilityRole role, int x, int y)
        {
            var definition = CreateStorageDefinition("work.facility." + definitionNumber++);
            using (var serialized = new DataFixtureEditor(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var thing = registry.Create(definition);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            return thing;
        }

        private static void Set<T>(T component, string property, object value)
        {
            typeof(T).GetProperty(property, BindingFlags.Instance | BindingFlags.Public).SetValue(component, value);
        }
    }
}
