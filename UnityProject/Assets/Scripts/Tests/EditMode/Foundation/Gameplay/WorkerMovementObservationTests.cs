using System;
using System.Reflection;
using MobileGame.Gameplay;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class WorkerMovementObservationTests : FoundationTestFixture
    {
        private ThingRegistry registry;
        private GridMap map;
        private LaboratoryManager laboratory;
        private ResourceData organic;
        private CreatureData creature;
        private int facilityCount;

        [SetUp]
        public void Prepare()
        {
            organic = CreateResourceDefinition("movement.organic");
            creature = CreateDefinition<CreatureData>("movement.worker", ThingComponentKind.ResourceContainer);
            using (var edit = new DataFixtureEditor(creature))
                edit.FindProperty("moveSecondsPerCell").floatValue = 1;
            registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 12);
            laboratory = new LaboratoryManager(registry, map, organic);
            facilityCount = 0;
        }

        [Test]
        public void RepeatedObservationDoesNotChangePositionJobsProgressOrSearches()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            var position = worker.Placement.Origin;
            var job = state.Job;
            var progress = state.MoveProgress;
            var queries = map.PathQueries;
            var visited = map.VisitedCells;
            var decisions = laboratory.Decisions;
            var clock = laboratory.ElapsedSeconds;
            for (int i = 0; i < 100; i++)
            {
                Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var segment), Is.True);
                Assert.That(segment.From, Is.EqualTo(new Vector2Int(1, 1)));
                Assert.That(segment.To, Is.EqualTo(new Vector2Int(2, 1)));
                Assert.That(segment.ProgressSeconds, Is.EqualTo(.25).Within(1e-8));
                Assert.That(segment.DurationSeconds, Is.EqualTo(1));
            }
            Assert.That(worker.Placement.Origin, Is.EqualTo(position));
            Assert.That(state.Job, Is.SameAs(job));
            Assert.That(state.MoveProgress, Is.EqualTo(progress));
            Assert.That(laboratory.ElapsedSeconds, Is.EqualTo(clock));
            Assert.That(laboratory.Decisions, Is.EqualTo(decisions));
            Assert.That(map.PathQueries, Is.EqualTo(queries));
            Assert.That(map.VisitedCells, Is.EqualTo(visited));
            Assert.That(laboratory.Reservations, Is.Empty);
            Assert.That(state.LastMove.Sequence, Is.Zero);
        }

        [Test]
        public void ExactCellBoundaryKeepsTheNextValidSegmentAtZeroProgress()
        {
            var worker = BeginWalk();
            laboratory.Advance(.75);
            var state = worker.GetComponent<WorkerComponent>();
            Assert.That(worker.Placement.Origin, Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(state.MoveProgress, Is.EqualTo(0).Within(1e-8));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var segment), Is.True);
            Assert.That(segment.From, Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(segment.To, Is.EqualTo(new Vector2Int(3, 1)));
            Assert.That(state.NextMoveCell, Is.EqualTo(segment.To));
        }

        [Test]
        public void ReconsideringTheSameWorkKeepsTheObservedProgress()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            var previousJob = state.Job;
            laboratory.Advance(.3);
            Assert.That(state.Job, Is.Not.SameAs(previousJob), "Fixture crosses the normal reconsideration deadline.");
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var segment), Is.True);
            Assert.That(segment.ProgressSeconds, Is.EqualTo(.55).Within(1e-8));
            Assert.That(segment.TargetId, Is.EqualTo(previousJob.TargetId));
        }

        [Test]
        public void ChangingTheTargetInTheSameLayoutRejectsTheOldGoalWithoutSearching()
        {
            var other = CompleteFacility(FacilityRole.Gathering, new Vector2Int(8, 4));
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            var layout = map.LayoutVersion;
            var queries = map.PathQueries;
            Set(state.Job, nameof(WorkerJob.TargetId), other.Id);
            Assert.That(map.LayoutVersion, Is.EqualTo(layout));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(state.NextMoveCell, Is.Null);
            Assert.That(map.PathQueries, Is.EqualTo(queries));
        }

        [Test]
        public void LayoutChangeRejectsTheCachedRouteBeforeTheNextMove()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            CompleteFacility(FacilityRole.Storage, new Vector2Int(2, 1));
            var queries = map.PathQueries;
            Assert.That(state.WaitReason, Is.EqualTo(WorkerWaitReason.None));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(map.PathQueries, Is.EqualTo(queries));
            Assert.That(state.MoveProgress, Is.EqualTo(.25).Within(1e-8));
        }

        [Test]
        public void WaitingWorkerCannotExposeThePreviousRouteAsMovement()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            Set(state, nameof(WorkerComponent.WaitReason), WorkerWaitReason.PathBlocked);
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(state.MoveProgress, Is.EqualTo(.25).Within(1e-8));
        }

        [Test]
        public void RemovedTargetCannotLeaveAVisibleMovingSegment()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            Assert.That(registry.TryGet(state.Job.TargetId, out var target), Is.True);
            registry.Remove(target);
            var queries = map.PathQueries;
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(map.PathQueries, Is.EqualTo(queries));
        }

        [Test]
        public void DirectWorkerPlacementInvalidatesTheOldOriginWithoutRecordingMovement()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            var layout = map.LayoutVersion;
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(10, 8)).Succeeded, Is.True);
            Assert.That(map.LayoutVersion, Is.EqualTo(layout));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(state.LastMove.Sequence, Is.Zero);
        }

        [TestCase(JobStage.Working)]
        [TestCase(JobStage.Waiting)]
        public void NonMovementStageCannotReuseThePreviousMovingRoute(JobStage stage)
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            Set(state.Job, nameof(WorkerJob.Stage), stage);
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
        }

        [TestCase(-.01)]
        [TestCase(1.01)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void InvalidProgressIsRejectedInsteadOfHiddenByClamping(double progress)
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            Set(state, nameof(WorkerComponent.MoveProgress), progress);
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
            Assert.That(state.MoveProgress, Is.EqualTo(progress));
        }

        [TestCase(0f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDurationIsRejected(float duration)
        {
            var worker = BeginWalk();
            using (var edit = new DataFixtureEditor(creature))
                edit.FindProperty("moveSecondsPerCell").floatValue = duration;
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, out _), Is.False);
        }

        [Test]
        public void CachedWorkerBindingMustBelongToTheSameThingAndSession()
        {
            var worker = BeginWalk();
            var other = CreateWorker(new Vector2Int(1, 2));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker,
                other.GetComponent<WorkerComponent>(), out _), Is.False);
            var otherRegistry = new ThingRegistry(GameplayComponentFactory.Attach);
            var otherMap = new GridMap(otherRegistry, 16, 12);
            var otherSession = new LaboratoryManager(otherRegistry, otherMap, organic);
            Assert.That(WorkerManager.TryGetMoveSegment(otherSession, worker,
                worker.GetComponent<WorkerComponent>(), out _), Is.False);
            registry.Remove(worker);
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, out _), Is.False);
        }

        [Test]
        public void EditingFreezesTheSameObservableMovementSegment()
        {
            var worker = BeginWalk();
            var state = worker.GetComponent<WorkerComponent>();
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var before), Is.True);
            Assert.That(map.Commands.BeginEdit(state.Job.TargetId).Succeeded, Is.True);
            laboratory.Advance(30);
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var after), Is.True);
            Assert.That(after.From, Is.EqualTo(before.From));
            Assert.That(after.To, Is.EqualTo(before.To));
            Assert.That(after.ProgressSeconds, Is.EqualTo(before.ProgressSeconds));
            Assert.That(state.LastMove.Sequence, Is.Zero);
            Assert.That(map.Commands.Cancel().Succeeded, Is.True);
        }

        [Test]
        public void LongAdvanceRetainsOnlyTheLatestCommittedStepAndItsSimulationTime()
        {
            var worker = BeginWalk();
            laboratory.Advance(2.25);
            var state = worker.GetComponent<WorkerComponent>();
            var observation = state.LastMove;
            Assert.That(observation.Sequence, Is.EqualTo(2));
            Assert.That(observation.From, Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(observation.To, Is.EqualTo(new Vector2Int(3, 1)));
            Assert.That(observation.AtSeconds, Is.EqualTo(2).Within(1e-8));
            Assert.That(observation.IsWander, Is.False);
            Assert.That(observation.LayoutVersion, Is.EqualTo(map.LayoutVersion));
            Assert.That(worker.Placement.Origin, Is.EqualTo(observation.To));
            Assert.That(state.MoveProgress, Is.EqualTo(.5).Within(1e-8));
            Assert.That(laboratory.ElapsedSeconds, Is.EqualTo(2.5).Within(1e-8));
        }

        [Test]
        public void ActualIdleStepHasAnObservationWhileDirectPlacementDoesNotInventOne()
        {
            var worker = CreateWorker(new Vector2Int(1, 1));
            var state = worker.GetComponent<WorkerComponent>();
            Assert.That(state.LastMove.Sequence, Is.Zero);
            for (int i = 0; i < 200 && state.LastMove.Sequence == 0; i++) laboratory.Advance(.05);
            var observation = state.LastMove;
            Assert.That(observation.Sequence, Is.EqualTo(1));
            Assert.That(observation.IsWander, Is.True);
            Assert.That(observation.From, Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(observation.To, Is.EqualTo(worker.Placement.Origin));
            Assert.That(Math.Abs(observation.To.x - observation.From.x)
                + Math.Abs(observation.To.y - observation.From.y), Is.EqualTo(1));
            Assert.That(observation.AtSeconds, Is.EqualTo(laboratory.ElapsedSeconds).Within(1e-8));
            Assert.That(map.PathQueries, Is.Zero);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(12, 10)).Succeeded, Is.True);
            Assert.That(state.LastMove.Sequence, Is.EqualTo(observation.Sequence));
            Assert.That(state.LastMove.To, Is.EqualTo(observation.To));
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out _), Is.False);
        }

        [Test]
        public void TransportObservationUsesTheSourceBeforePickupAndDestinationAfterPickup()
        {
            var worker = CreateWorker(new Vector2Int(1, 1));
            var source = CompleteFacility(FacilityRole.Gathering, new Vector2Int(4, 2));
            var destination = CompleteFacility(FacilityRole.Storage, new Vector2Int(8, 2));
            registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            var gathering = source.GetComponent<GatheringComponent>();
            Set(gathering, nameof(GatheringComponent.Progress), ((FacilityData)source.Definition).GatheringSeconds);
            Set(gathering, nameof(GatheringComponent.Outstanding), 5);
            Assert.That(laboratory.SetWorkerPriorities(worker.Id, new[] { 0, 0, 1, 0 }), Is.True);
            laboratory.Advance(.25);
            var state = worker.GetComponent<WorkerComponent>();
            Assert.That(WorkerManager.TryGetMoveSegment(laboratory, worker, state, out var supply), Is.True);
            Assert.That(supply.Stage, Is.EqualTo(JobStage.ToSupply));
            Assert.That(supply.TargetId, Is.EqualTo(source.Id));
            WorkerMoveSegment delivery = default;
            bool found = false;
            for (int i = 0; i < 200 && !found; i++)
            {
                laboratory.Advance(.05);
                found = WorkerManager.TryGetMoveSegment(laboratory, worker, state, out delivery)
                    && delivery.Stage == JobStage.ToDestination;
            }
            Assert.That(found, Is.True, "The fixture must have picked up and started its delivery leg.");
            Assert.That(delivery.TargetId, Is.EqualTo(destination.Id));
            Assert.That(laboratory.TotalAt(worker.Id), Is.EqualTo(5));
            laboratory.ValidateState();
        }

        private Thing BeginWalk()
        {
            var worker = CreateWorker(new Vector2Int(1, 1));
            CompleteFacility(FacilityRole.Gathering, new Vector2Int(8, 2));
            laboratory.Advance(.25);
            return worker;
        }

        private Thing CreateWorker(Vector2Int position)
        {
            var worker = registry.Create(creature);
            Assert.That(map.Commands.Place(worker.Id, position).Succeeded, Is.True);
            Assert.That(laboratory.SetWorkerPriorities(worker.Id, new[] { 0, 1, 0, 0 }), Is.True);
            return worker;
        }

        private Thing CompleteFacility(FacilityRole role, Vector2Int position)
        {
            var definition = CreateDefinition<FacilityData>("movement.facility." + ++facilityCount,
                ThingComponentKind.ResourceContainer);
            using (var edit = new DataFixtureEditor(definition))
            {
                edit.FindProperty("role").intValue = (int)role;
                edit.FindProperty("resource").objectReferenceValue = organic;
                edit.FindProperty("footprint").vector2IntValue = Vector2Int.one;
                edit.FindProperty("interactionOffset").vector2IntValue = Vector2Int.down;
            }
            var facility = registry.Create(definition);
            Assert.That(map.Commands.Place(facility.Id, position).Succeeded, Is.True);
            var construction = facility.GetComponent<ConstructionComponent>();
            Set(construction, nameof(ConstructionComponent.IsComplete), true);
            Set(construction, nameof(ConstructionComponent.Delivered), definition.BuildCost);
            Set(construction, nameof(ConstructionComponent.Progress), definition.BuildSeconds);
            return facility;
        }

        private static void Set(object target, string property, object value)
        {
            var member = target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(member, Is.Not.Null, "Fixture preparation property must exist.");
            member.SetValue(target, value);
        }
    }
}
