using System;
using System.Linq;
using MobileGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class ProductionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitAssignmentInterruptsGatheringButAutomaticReturnFinishesItsCycle(bool explicitChange)
        {
            var worker = Worker(support, 4, 3, new[] { 0, 2, 0, 1 });
            var gatherer = Facility(gathering, 4, 4);
            var processor = Facility(processing, 9, 4);
            if (!explicitChange) lab.AssignWorker(worker.Id, processor.Id);
            lab.Advance(1);
            var batch = gatherer.GetComponent<GatheringComponent>();
            Assert.That(batch.Progress, Is.EqualTo(1).Within(1e-8));
            Stock(processor, organic, 5);
            if (explicitChange) lab.AssignWorker(worker.Id, processor.Id);
            lab.Advance(0.5);
            Assert.That(batch.Progress, Is.EqualTo(explicitChange ? 1 : 1.5).Within(1e-8));
            if (!explicitChange)
            {
                Until(lab.Advance, () => batch.Outstanding == 5);
                Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null);
                lab.Advance(0.05);
            }
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Production));
            Assert.That(processor.GetComponent<ProductionComponent>().InProgress, Is.True);
            lab.ValidateState();
        }

        [Test]
        public void AutomaticReturnReconsidersMovementButRespectsPlayerPriorities()
        {
            var worker = Worker(support, 0, 1, new[] { 0, 2, 0, 1 });
            Facility(gathering, 4, 4);
            var processor = Facility(processing, 10, 6);
            lab.AssignWorker(worker.Id, processor.Id);
            lab.Advance(0.05);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Gathering));
            Stock(processor, organic, 5);
            lab.Advance(0.05);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Production));
            lab.SetWorkerPriorities(worker.Id, new[] { 0, 1, 0, 2 });
            lab.Advance(0.05);
            Assert.That(worker.GetComponent<WorkerComponent>().Job.Kind, Is.EqualTo(WorkKind.Gathering));
            lab.ValidateState();
        }

        [Test]
        public void BlockedInteractionStopsAutomaticProgressAndRecoveryDoesNotConsumeAgain()
        {
            Worker(prototype, 1, 1, new[] { 0, 0, 0, 0 });
            var processor = Facility(processing, 7, 6); Stock(processor, organic, 5);
            lab.Advance(1);
            var state = processor.GetComponent<ProductionComponent>();
            var blocker = registry.Create(CreateStorageDefinition("production.blocker"));
            Assert.That(map.Commands.Place(blocker.Id, processor.Placement.InteractionCell.Value).Succeeded, Is.True);
            lab.OnMapChanged(); lab.Advance(10);
            Assert.That(state.Progress, Is.EqualTo(0.2).Within(1e-8));
            Assert.That(lab.Production.WaitReason(processor), Is.EqualTo("Interaction cell is unreachable"));
            Assert.That(map.Commands.Place(blocker.Id, new Vector2Int(14, 8)).Succeeded, Is.True);
            lab.OnMapChanged(); lab.Advance(1);
            Assert.That(state.Progress, Is.EqualTo(0.4).Within(1e-8));
            Assert.That(Total(organic), Is.Zero);
            lab.ValidateState();
        }

        [Test]
        public void TwoAssignedWorkersClaimOneSlotWithoutAddingAutomaticSpeed()
        {
            var first = Worker(support, 4, 3, new[] { 0, 0, 0, 1 });
            var second = Worker(support, 4, 3, new[] { 0, 0, 0, 1 });
            var processor = Facility(processing, 4, 4); Stock(processor, organic, 5);
            lab.AssignWorker(first.Id, processor.Id); lab.AssignWorker(second.Id, processor.Id);
            lab.Advance(2);
            Assert.That(processor.GetComponent<ProductionComponent>().Progress, Is.EqualTo(2).Within(1e-8));
            Assert.That(new[] { first, second }.Count(t => t.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Working), Is.EqualTo(1));
            lab.AssignWorker(first.Id, null); lab.Advance(1);
            Assert.That(processor.GetComponent<ProductionComponent>().Progress, Is.EqualTo(3).Within(1e-8));
            lab.ValidateState();
        }

        [Test]
        public void OutputMustVisitStorageBeforeSupplyingCultivation()
        {
            Worker(prototype, 1, 1, new[] { 0, 0, 1, 0 });
            var processor = Facility(processing, 3, 3); Stock(processor, organic, 5);
            var cultivator = Facility(cultivation, 6, 3); lab.SelectCultivation(cultivator.Id, CultivationTarget.Support);
            lab.Advance(55);
            Assert.That(lab.TotalAt(processor.Id), Is.EqualTo(1));
            Assert.That(lab.TotalAt(cultivator.Id), Is.Zero);
            Assert.That(lab.Reservations, Is.Empty, "A nearby cultivator is not a valid output destination.");
            var store = Facility(storage, 10, 3);
            lab.Advance(0.05);
            Assert.That(lab.Reservations.Single().DestinationId, Is.EqualTo(store.Id));
            Until(lab.Advance, () => lab.TotalAt(cultivator.Id) == 1);
            Assert.That(processor.GetComponent<ProductionComponent>().Outstanding, Is.Zero);
            lab.ValidateState();
        }
    }
}
