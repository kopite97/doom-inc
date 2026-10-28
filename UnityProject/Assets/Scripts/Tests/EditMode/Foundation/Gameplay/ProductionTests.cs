using MobileGame.TestSupport;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    // Explicit fixtures isolate rare boundaries. RealInitialCycle uses only paid commands and real work.
    public sealed partial class ProductionTests : FoundationTestFixture
    {
        private ResourceData organic, culture;
        private FacilityData gathering, storage, processing, cultivation;
        private CreatureData prototype, support, invader;
        private ThingData[] catalog;
        private LaboratoryManager lab;
        private ThingRegistry registry;
        private GridMap map;
        private string directory;

        [SetUp]
        public void Prepare()
        {
            organic = Load<ResourceData>("Resource/OrganicMatter");
            culture = Load<ResourceData>("Resource/CultureMedium");
            gathering = Load<FacilityData>("Facility/Gathering");
            storage = Load<FacilityData>("Facility/Storage");
            processing = Load<FacilityData>("Facility/Processing");
            cultivation = Load<FacilityData>("Facility/Cultivation");
            prototype = Load<CreatureData>("Creature/PrototypeSupport");
            support = Load<CreatureData>("Creature/Support");
            invader = Load<CreatureData>("Creature/Invader");
            catalog = new ThingData[] { organic, culture, gathering, storage, processing, cultivation, prototype, support, invader };
            registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 10);
            lab = new LaboratoryManager(registry, map, organic, culture);
            directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN009-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

        [Test]
        public void RealInitialCycleBuildsFourFacilityTypesCultivatesFirstSupportAndWaitingInvader()
        {
            var flow = Flow(() => LaboratorySessionFactory.Create(organic, gathering, storage, prototype, culture));
            lab = (LaboratoryManager)flow.Current.Runtime; registry = lab.Registry; map = lab.Map;
            var commands = new LaboratoryCommands(flow);
            Assert.That(registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
            Assert.That(registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
            Assert.That(Total(organic), Is.EqualTo(4));
            Build(commands, gathering, 3, 3); Build(commands, storage, 8, 3);
            Until(flow.Advance, () => registry.Things.Any(t => t.Definition == storage && lab.TotalAt(t.Id) >= 5));
            Build(commands, processing, 3, 7); Build(commands, cultivation, 8, 7);
            // Real storage management: organic gathering may fill shared capacity during a 50-second cycle.
            // Build and pay for a culture-only store rather than granting capacity or bypassing admission.
            Build(commands, storage, 12, 3);
            var stores = registry.Things.Where(t => t.Definition == storage).OrderBy(t => t.CreationOrder).ToArray();
            Assert.That(commands.SetAllowed(stores[0].Id, culture, false), Is.True);
            Assert.That(commands.SetAllowed(stores[1].Id, organic, false), Is.True);
            var processor = registry.Things.Single(t => t.Definition == processing);
            var cultivator = registry.Things.Single(t => t.Definition == cultivation);
            Until(flow.Advance, () => lab.IsComplete(processor) && lab.IsComplete(cultivator));
            Assert.That(lab.SelectCultivation(cultivator.Id, CultivationTarget.Support), Is.True);
            Until(flow.Advance, () => registry.Things.Any(t => t.Definition == support), 800);
            var first = registry.Things.Single(t => t.Definition == support);
            Assert.That(first.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
            Assert.That(first.Placement.Origin, Is.EqualTo(cultivator.Placement.InteractionCell));
            Assert.That(lab.AssignWorker(first.Id, processor.Id), Is.True);
            Assert.That(lab.SetWorkerPriorities(first.Id, new[] { 2, 3, 4, 1 }), Is.True);
            Until(flow.Advance, () => first.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Production
                && first.GetComponent<WorkerComponent>().Job.Stage == JobStage.Working);
            var before = processor.GetComponent<ProductionComponent>().Progress;
            flow.Advance(0.5);
            Assert.That(processor.GetComponent<ProductionComponent>().Progress - before, Is.EqualTo(0.5).Within(1e-8));
            Assert.That(commands.SelectCultivation(cultivator.Id, CultivationTarget.Invader), Is.True);
            Until(flow.Advance, () => registry.Things.Any(t => t.Definition == invader), 800);
            Assert.That(registry.Things.Where(t => t.Definition == invader).All(t => !t.TryGetComponent<WorkerComponent>(out _)), Is.True);
            Assert.That(flow.LastSaveError, Is.Null.Or.Empty);
            Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            ((LaboratoryManager)flow.Current.Runtime).ValidateState();
        }

        [TestCase(false, 50)]
        [TestCase(true, 100)]
        public void UnstaffedCyclesConsumeOnceAndFinishAtTwentyPercent(bool cultivate, double seconds)
        {
            Worker(prototype, 1, 1, new[] { 0, 0, 0, 0 });
            var facility = Facility(cultivate ? cultivation : processing, 4, 4);
            if (cultivate) lab.SelectCultivation(facility.Id, CultivationTarget.Support);
            Stock(facility, cultivate ? culture : organic, cultivate ? 2 : 5);
            var state = facility.GetComponent<ProductionComponent>();
            lab.Advance(seconds - 0.05);
            Assert.That(state.InProgress, Is.True);
            Assert.That(lab.TotalAt(facility.Id), Is.Zero, "Inputs consumed once at cycle start.");
            Assert.That(state.Progress, Is.EqualTo((seconds - 0.05) * 0.2).Within(1e-8));
            lab.Advance(0.05);
            Assert.That(state.InProgress, Is.False);
            Assert.That(cultivate ? registry.Things.Count(t => t.Definition == support) : Total(culture), Is.EqualTo(1));
            lab.Advance(10);
            Assert.That(cultivate ? registry.Things.Count(t => t.Definition == support) : Total(culture), Is.EqualTo(1));
            lab.ValidateState();
        }

        [Test]
        public void AssignmentOnlySpeedsActualWorkAndUnassignPreservesConsumedCycle()
        {
            var worker = Worker(support, 1, 1, new[] { 0, 0, 0, 1 });
            var facility = Facility(processing, 10, 6);
            Stock(facility, organic, 5);
            lab.AssignWorker(worker.Id, facility.Id);
            lab.Advance(1);
            var state = facility.GetComponent<ProductionComponent>();
            Assert.That(state.Progress, Is.EqualTo(0.2).Within(1e-8));
            Until(lab.Advance, () => worker.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Working);
            var before = state.Progress;
            lab.Advance(1);
            Assert.That(state.Progress - before, Is.EqualTo(1).Within(1e-8));
            lab.AssignWorker(worker.Id, null);
            before = state.Progress;
            lab.Advance(1);
            Assert.That(state.Progress - before, Is.EqualTo(0.2).Within(1e-8));
            Assert.That(Total(organic), Is.Zero);
            lab.ValidateState();
        }

        [Test]
        public void NoSelectionDoesNotRequestInputsAndMidCycleTargetChangeAppliesNextCycle()
        {
            Worker(prototype, 1, 1, new[] { 0, 0, 1, 0 });
            var store = Facility(storage, 2, 3); Stock(store, culture, 4);
            var facility = Facility(cultivation, 7, 3);
            lab.Advance(10);
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(4));
            Assert.That(lab.Reservations, Is.Empty);
            lab.SelectCultivation(facility.Id, CultivationTarget.Support);
            Until(lab.Advance, () => facility.GetComponent<ProductionComponent>().InProgress);
            lab.SelectCultivation(facility.Id, CultivationTarget.Invader);
            Assert.That(facility.GetComponent<ProductionComponent>().ActiveTarget, Is.EqualTo(CultivationTarget.Support));
            Until(lab.Advance, () => registry.Things.Any(t => t.Definition == support), 110);
            var born = registry.Things.Single(t => t.Definition == support);
            Assert.That(born.Placement.Origin, Is.EqualTo(facility.Placement.InteractionCell));
            lab.SetWorkerPriorities(born.Id, new[] { 0, 0, 0, 0 });
            Until(lab.Advance, () => registry.Things.Any(t => t.Definition == invader), 120);
            var waiting = registry.Things.Single(t => t.Definition == invader);
            Assert.That(waiting.Placement.Origin, Is.EqualTo(born.Placement.Origin));
            Assert.That(waiting.TryGetComponent<WorkerComponent>(out _), Is.False);
            Assert.That(Total(culture), Is.Zero);
            Assert.That(facility.GetComponent<ProductionComponent>().SelectedTarget, Is.EqualTo(CultivationTarget.Invader));
            lab.ValidateState();
        }

        [Test]
        public void CurrentCycleReservationsSubtractPartialInputsAcrossMultipleWorkers()
        {
            Worker(prototype, 0, 1, new[] { 0, 0, 1, 0 });
            Worker(support, 0, 2, new[] { 0, 0, 1, 0 });
            var store = Facility(storage, 4, 3); Stock(store, organic, 10);
            var facility = Facility(processing, 10, 3); Stock(facility, organic, 2);
            lab.Advance(0.05);
            Assert.That(lab.Reservations.Sum(r => r.Amount), Is.EqualTo(3));
            Assert.That(lab.Reservations.Single().DestinationId, Is.EqualTo(facility.Id));
            Until(lab.Advance, () => facility.GetComponent<ProductionComponent>().InProgress);
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(7));
            Assert.That(lab.TotalAt(facility.Id), Is.Zero);
            lab.Advance(3);
            Assert.That(lab.Reservations, Is.Empty, "Production does not stockpile future cycles.");
            lab.ValidateState();
        }

        [Test]
        public void OutputPickupDoesNotReleaseCycleAndHoldingWaitSurvivesFileReload()
        {
            var worker = Worker(prototype, 1, 1, new[] { 0, 0, 1, 0 });
            var facility = Facility(processing, 3, 3); Stock(facility, organic, 5);
            var store = Facility(storage, 10, 3);
            var flow = Flow(() => new SessionState(registry, map, lab));
            Until(flow.Advance, () => lab.Reservations.Any(r => r.BatchOriginId == facility.Id && r.PickedUp));
            Assert.That(lab.SetAllowed(store.Id, culture, false), Is.True);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Stage, Is.EqualTo(JobStage.Waiting));
            Assert.That(facility.GetComponent<ProductionComponent>().Outstanding, Is.EqualTo(1));
            Assert.That(lab.Production.InputNeeded(facility), Is.Zero);
            Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            lab = (LaboratoryManager)flow.Current.Runtime;
            lab.Registry.TryGet(facility.Id, out facility); lab.Registry.TryGet(worker.Id, out worker);
            flow.Advance(20);
            Assert.That(facility.GetComponent<ProductionComponent>().Outstanding, Is.EqualTo(1));
            Assert.That(lab.TotalAt(worker.Id), Is.EqualTo(1));
            lab.SetAllowed(store.Id, culture, true);
            Until(flow.Advance, () => facility.GetComponent<ProductionComponent>().Outstanding == 0);
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(1));
            Assert.That(lab.Production.InputNeeded(facility), Is.EqualTo(5));
            lab.ValidateState();
        }

        [Test]
        public void MixedStorageCapacityExclusionAndExistingOutgoingReservationAreIndependent()
        {
            Worker(prototype, 1, 1, new[] { 0, 0, 1, 0 });
            var source = Facility(storage, 4, 3); Stock(source, organic, 10); Stock(source, culture, 10);
            var destination = Facility(storage, 10, 3); Stock(destination, organic, 18);
            lab.SetAllowed(source.Id, culture, false);
            lab.Advance(0.05);
            var reservation = lab.Reservations.Single();
            Assert.That(reservation.Amount, Is.EqualTo(2));
            Assert.That(reservation.DestinationId, Is.EqualTo(destination.Id));
            lab.SetAllowed(source.Id, culture, true);
            Assert.That(lab.Reservations.Single(), Is.SameAs(reservation), "Changing source allowance preserves outgoing transport.");
            Until(lab.Advance, () => lab.Reservations.Count == 0);
            Assert.That(lab.TotalAt(destination.Id), Is.EqualTo(20));
            Assert.That(Total(culture), Is.EqualTo(10));
            Assert.That(Total(organic), Is.EqualTo(28));
            lab.ValidateState();
        }

        [Test]
        public void AssignmentRejectsWrongModelsAndFacilitiesAndPreservesNormalTransport()
        {
            var original = Worker(prototype, 1, 1, new[] { 0, 0, 0, 0 });
            var worker = Worker(support, 1, 2, new[] { 0, 0, 1, 1 });
            var enemy = registry.Create(invader); map.Commands.Place(enemy.Id, new Vector2Int(1, 3));
            var gatherer = Facility(gathering, 3, 3);
            var store = Facility(storage, 8, 3); Stock(store, organic, 5);
            var facility = Facility(processing, 3, 7);
            Assert.That(lab.AssignWorker(original.Id, facility.Id), Is.False);
            Assert.That(lab.AssignWorker(enemy.Id, facility.Id), Is.False);
            Assert.That(lab.AssignWorker(worker.Id, store.Id), Is.False);
            Assert.That(lab.AssignWorker(worker.Id, gatherer.Id), Is.False);
            lab.Advance(0.05);
            var job = worker.GetComponent<WorkerComponent>().Job;
            var reservation = lab.Reservations.Single();
            Assert.That(job.Stage, Is.EqualTo(JobStage.ToSupply));
            Assert.That(lab.AssignWorker(worker.Id, facility.Id), Is.True);
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.SameAs(job));
            Assert.That(lab.Reservations.Single(), Is.SameAs(reservation));
            Until(lab.Advance, () => worker.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Production);
            lab.ValidateState();
        }

        [Test]
        public void PlacementPauseMovementAndApplicationPausePreserveCycle()
        {
            var worker = Worker(support, 4, 3, new[] { 0, 0, 0, 1 });
            var facility = Facility(processing, 4, 4); Stock(facility, organic, 5);
            lab.AssignWorker(worker.Id, facility.Id);
            var flow = Flow(() => new SessionState(registry, map, lab));
            flow.Advance(1);
            var state = facility.GetComponent<ProductionComponent>();
            Assert.That(state.Progress, Is.EqualTo(1).Within(1e-8));
            flow.NotifyApplicationPause(true); flow.Advance(5);
            Assert.That(state.Progress, Is.EqualTo(1).Within(1e-8));
            flow.NotifyApplicationPause(false);
            Assert.That(flow.BeginPlacement(facility.Id), Is.True); flow.Advance(5);
            Assert.That(state.Progress, Is.EqualTo(1).Within(1e-8));
            Assert.That(flow.PreviewPlacement(new Vector2Int(10, 6), QuarterTurn.Clockwise90), Is.True);
            Assert.That(flow.ConfirmPlacement(), Is.True);
            flow.Advance(1);
            Assert.That(state.Progress, Is.EqualTo(1.2).Within(1e-8));
            Assert.That(Total(organic), Is.Zero);
            lab.ValidateState();
        }

        [Test]
        public void VersionTwoSaveMigratesWithoutChangingIdentityOrOrganicAllowance()
        {
            var worker = Worker(prototype, 1, 1, new[] { 1, 2, 3 });
            lab.SetPreset(new[] { 1, 2, 3 });
            var store = Facility(storage, 4, 4);
            lab.SetAllowed(store.Id, organic, false);
            Stock(store, organic, 2);
            var start = new SessionState(registry, map, lab);
            var codec = new SessionSnapshotCodec(catalog, true);
            var saved = codec.Capture(start, SessionPhase.Playing);
            saved.version = 2; saved.laboratory.cultureDefinitionId = null; saved.invasion = null;
            var restored = codec.Restore(codec.Deserialize(codec.Serialize(saved)));
            Assert.That(restored.Registry.Things.Select(t => t.Id), Is.EquivalentTo(start.Registry.Things.Select(t => t.Id)));
            Assert.That(((LaboratoryManager)restored.Runtime).Culture, Is.SameAs(culture));
            restored.Registry.TryGet(store.Id, out var restoredStore);
            Assert.That(restoredStore.GetComponent<StorageComponent>().AllowsOrganic, Is.False);
            Assert.That(restoredStore.GetComponent<StorageComponent>().AllowsCulture, Is.True);
            Assert.That(((LaboratoryManager)restored.Runtime).TotalAt(store.Id), Is.EqualTo(2));
            Assert.That(codec.Capture(restored, SessionPhase.Playing).version, Is.EqualTo(SessionSnapshotCodec.CurrentVersion));
        }

        [TestCase("assignment")]
        [TestCase("progress")]
        [TestCase("target")]
        [TestCase("stock")]
        [TestCase("version")]
        public void InvalidProductionGraphIsRejectedWithoutReplacingGoodFile(string defect)
        {
            var worker = Worker(prototype, 1, 1, new[] { 0, 0, 0, 0 });
            var facility = Facility(cultivation, 4, 4);
            lab.SelectCultivation(facility.Id, CultivationTarget.Support); Stock(facility, culture, 2); lab.Advance(1);
            var flow = Flow(() => new SessionState(registry, map, lab));
            Assert.That(flow.Save(), Is.True);
            var file = Path.Combine(directory, "session.json"); var valid = File.ReadAllText(file);
            var codec = new SessionSnapshotCodec(catalog, true);
            var snapshot = codec.Deserialize(valid);
            var state = snapshot.laboratory.things.Single(t => t.thingId == facility.Id.ToString("N"));
            if (defect == "assignment") snapshot.laboratory.things.Single(t => t.hasWorker).worker.assignedFacilityId = facility.Id.ToString("N");
            if (defect == "progress") state.production.progress = 20;
            if (defect == "target") state.production.activeTarget = CultivationTarget.None;
            if (defect == "stock") state.production.outstanding = 1;
            if (defect == "version") snapshot.version = 99;
            Assert.Throws<InvalidDataException>(() => codec.Restore(snapshot));
            Assert.That(File.ReadAllText(file), Is.EqualTo(valid));
            if (defect == "assignment")
            {
                Set(worker.GetComponent<WorkerComponent>(), "AssignedFacilityId", (Guid?)facility.Id);
                Assert.That(flow.Save(), Is.False);
                Assert.That(File.ReadAllText(file), Is.EqualTo(valid));
            }
        }

        private GameFlowManager Flow(Func<SessionState> create)
        {
            var flow = new GameFlowManager(new SessionSnapshotCodec(catalog, true),
                new JsonFileSessionStore(Path.Combine(directory, "session.json")), create, 5);
            Assert.That(flow.StartOrContinue(), Is.True, flow.LastError); return flow;
        }
        private Thing Worker(CreatureData definition, int x, int y, int[] priorities)
        {
            var thing = registry.Create(definition);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            Assert.That(lab.SetWorkerPriorities(thing.Id, priorities), Is.True); return thing;
        }
        private Thing Facility(FacilityData definition, int x, int y)
        {
            var thing = registry.Create(definition);
            Assert.That(map.Commands.Place(thing.Id, new Vector2Int(x, y)).Succeeded, Is.True);
            var state = thing.GetComponent<ConstructionComponent>();
            Set(state, "Delivered", definition.BuildCost); Set(state, "Progress", (double)definition.BuildSeconds); Set(state, "IsComplete", true);
            return thing;
        }
        private void Stock(Thing holder, ResourceData definition, int amount) => registry.CreateResource(definition, amount, holder.GetComponent<ResourceContainerComponent>());
        private int Total(ResourceData definition) => registry.Things.Where(t => t.Definition == definition).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
        private static T Load<T>(string path) where T : ThingData => DataTestAssets.LoadAssetAtPath<T>("Assets/SO/" + path + ".asset");
        private static void Set(object target, string property, object value) => target.GetType().GetProperty(property).GetSetMethod(true).Invoke(target, new[] { value });
        private static void Build(LaboratoryCommands commands, FacilityData definition, int x, int y)
        { Assert.That(commands.BeginConstruction(definition), Is.True); Assert.That(commands.PreviewConstruction(new Vector2Int(x,y), QuarterTurn.None), Is.True); Assert.That(commands.ConfirmConstruction(), Is.True); }
        private static void Until(Action<double> advance, Func<bool> condition, double maximum = 120)
        {
            for (var elapsed = 0d; elapsed < maximum && !condition(); elapsed += 0.05) advance(0.05);
            Assert.That(condition(), Is.True, "Scenario exceeded " + maximum + " simulated seconds.");
        }
    }
}
