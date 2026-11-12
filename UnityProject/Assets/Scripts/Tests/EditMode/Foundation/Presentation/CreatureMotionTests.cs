using System;
using System.Reflection;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    // Prepared movement states isolate the display contract; commands and real Advance are covered separately.
    public sealed class CreatureMotionTests : FoundationTestFixture
    {
        private LaboratoryManager simulation;
        private Thing worker, target;
        private WorkerComponent state;
        private CreatureData creature;
        private ResourceData organic;
        private FacilityData facility;
        private GridMap map;
        private CreatureMotion motion;

        [SetUp]
        public void Prepare()
        {
            organic = CreateResourceDefinition("motion.organic");
            creature = CreateDefinition<CreatureData>("motion.creature", ThingComponentKind.ResourceContainer);
            using (var edit = new DataFixtureEditor(creature))
            {
                edit.FindProperty("moveSecondsPerCell").floatValue = 1;
                edit.ApplyModifiedPropertiesWithoutUndo();
            }
            facility = CreateStorageDefinition("motion.target");
            using (var edit = new DataFixtureEditor(facility))
            {
                edit.FindProperty("footprint").vector2IntValue = Vector2Int.one;
                edit.FindProperty("interactionOffset").vector2IntValue = Vector2Int.down;
                edit.ApplyModifiedPropertiesWithoutUndo();
            }
            var registry = new ThingRegistry(GameplayComponentFactory.Attach);
            map = new GridMap(registry, 16, 16);
            simulation = new LaboratoryManager(registry, map, organic);
            worker = registry.Create(creature); target = registry.Create(facility);
            state = worker.GetComponent<WorkerComponent>(); motion = new CreatureMotion();
        }

        [TestCase(1, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        public void FourDirectionsUseSecondsAndKeepLogicalPosition(int x, int y)
        {
            var start = new Vector2Int(5, 5); var direction = new Vector2Int(x, y);
            Route(start, start + direction * 3, 0, 0);
            foreach (var fraction in new[] { 0.0, .25, .5, .75 })
            {
                Set(state, nameof(WorkerComponent.MoveProgress), fraction); Clock(fraction);
                motion.Evaluate(simulation, worker);
                Position(start + (Vector2)direction * (float)fraction);
                Assert.That(worker.Placement.Origin, Is.EqualTo(start));
                Assert.That(state.MoveProgress, Is.EqualTo(fraction));
                Assert.That(motion.Direction, Is.EqualTo(direction));
                Assert.That(motion.IsMoving, Is.True);
            }
        }

        [TestCase(.5)]
        [TestCase(2.0)]
        public void MovementDurationScalesTheSameFraction(double duration)
        {
            using (var edit = new DataFixtureEditor(creature))
            {
                edit.FindProperty("moveSecondsPerCell").floatValue = (float)duration;
                edit.ApplyModifiedPropertiesWithoutUndo();
            }
            var start = new Vector2Int(5, 5);
            Route(start, start + Vector2Int.right * 3, duration * .5, duration * .5);
            motion.Evaluate(simulation, worker);
            Position(start + Vector2.right * .5f);
        }

        [Test]
        public void EquivalentJobReplacementAndZeroProgressCornerKeepContinuity()
        {
            var start = new Vector2Int(5, 5); var goal = new Vector2Int(6, 8);
            Route(start, goal, .75, .75); motion.Evaluate(simulation, worker);
            Position(new Vector2(5.75f, 5));
            var previousJob = state.Job;
            Set(state, nameof(WorkerComponent.Job), Job(target.Id));
            motion.Evaluate(simulation, worker);
            Position(new Vector2(5.75f, 5));
            Assert.That(state.Job, Is.Not.SameAs(previousJob));
            Assert.That(motion.IsCorrecting, Is.False);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(6, 5)).Succeeded, Is.True);
            Observe(1, start, new Vector2Int(6, 5), 1, false);
            Route(new Vector2Int(6, 5), goal, 0, 1);
            motion.Evaluate(simulation, worker);
            Position(new Vector2(6, 5));
            Assert.That(motion.Direction, Is.EqualTo(Vector2Int.up));
            Assert.That(motion.IsCorrecting, Is.False);
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void NormalPositionMatchesTheSameProgressAcrossTimePartitions(int rate)
        {
            var start = new Vector2Int(5, 5);
            Route(start, start + Vector2Int.right * 3, 0, 0);
            for (var i = 0; i <= rate * 3 / 4; i++)
            {
                var at = (double)i / rate;
                Set(state, nameof(WorkerComponent.MoveProgress), at); Clock(at);
                motion.Evaluate(simulation, worker);
                Position(start + Vector2.right * (float)at);
            }
            Set(state, nameof(WorkerComponent.MoveProgress), .75); Clock(.75);
            motion.Evaluate(simulation, worker); Position(start + Vector2.right * .75f);
        }

        [Test]
        public void WanderUsesTheCommittedSimulationTimestampAndPauseRepeatsIt()
        {
            var from = new Vector2Int(4, 4); var to = from + Vector2Int.right;
            Idle(from, 0); motion.Evaluate(simulation, worker);
            Assert.That(map.Commands.Place(worker.Id, to).Succeeded, Is.True);
            Observe(1, from, to, 1, true); Clock(1.25);
            motion.Evaluate(simulation, worker); Position(new Vector2(4.25f, 4));
            for (var i = 0; i < 20; i++) motion.Evaluate(simulation, worker);
            Position(new Vector2(4.25f, 4));
            Clock(2); motion.Evaluate(simulation, worker); Position(to);
            Assert.That(motion.IsMoving, Is.False);
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1.0)]
        public void InvalidSimulationTimeUsesTheSafeLogicalOrigin(double at)
        {
            var origin = new Vector2Int(5, 5);
            Route(origin, origin + Vector2Int.right * 3, .5, .5);
            motion.Evaluate(simulation, worker);
            Clock(at); motion.Evaluate(simulation, worker);
            Position(origin); Assert.That(motion.IsMoving, Is.False);
        }

        [TestCase(1, 0)]
        [TestCase(0, 1)]
        [TestCase(-1, 0)]
        public void WanderToJobUsesOrthogonalLegsAndMeetsOneCellDeadline(int x, int y)
        {
            var from = new Vector2Int(4, 4); var origin = from + Vector2Int.right;
            Idle(from, 0); motion.Evaluate(simulation, worker);
            Assert.That(map.Commands.Place(worker.Id, origin).Succeeded, Is.True);
            Observe(1, from, origin, 1, true); Clock(1.25);
            motion.Evaluate(simulation, worker); var before = motion.Position;
            var direction = new Vector2Int(x, y); var goal = origin + direction * 3;
            Route(origin, goal, 0, 1.25);
            motion.Evaluate(simulation, worker);
            Assert.That(motion.IsCorrecting, Is.True); Position(before);
            motion.Evaluate(simulation, worker); Position(before);
            for (var i = 1; i <= 20; i++)
            {
                var elapsed = i * .05;
                if (i < 20) Set(state, nameof(WorkerComponent.MoveProgress), elapsed);
                else
                {
                    Assert.That(map.Commands.Place(worker.Id, origin + direction).Succeeded, Is.True);
                    Observe(2, origin, origin + direction, 2.25, false);
                    Route(origin + direction, goal, 0, 2.25);
                }
                Clock(1.25 + elapsed); motion.Evaluate(simulation, worker);
                var position = motion.Position;
                if (x == 0)
                    Assert.That(Math.Abs(position.y - origin.y) < .0001 || Math.Abs(position.x - origin.x) < .0001,
                        Is.True, "A right-angle transition must stay on its two grid edges.");
                else Assert.That(position.y, Is.EqualTo(origin.y).Within(.0001));
            }
            Position(origin + direction);
            Assert.That(motion.IsCorrecting, Is.False, "The deadline must not restart as the target advances.");
        }

        [Test]
        public void ChangedGoalRetractsContinuouslyAndDoesNotRestoreThroughAStaleCache()
        {
            var origin = new Vector2Int(5, 5);
            Route(origin, new Vector2Int(8, 5), .5, .5); motion.Evaluate(simulation, worker);
            Position(new Vector2(5.5f, 5));
            Assert.That(map.Commands.Place(target.Id, new Vector2Int(5, 9)).Succeeded, Is.True);
            var queries = map.PathQueries;
            motion.Evaluate(simulation, worker);
            Position(new Vector2(5.5f, 5)); Assert.That(motion.IsCorrecting, Is.True);
            Clock(1.5); motion.Evaluate(simulation, worker); Position(origin);
            Assert.That(map.PathQueries, Is.EqualTo(queries));
            Assert.That(state.MoveProgress, Is.EqualTo(.5));
        }

        [Test]
        public void ARealProgressResetOnTheSameEdgeDoesNotJumpTheDisplayBackwards()
        {
            var origin = new Vector2Int(5, 5);
            Route(origin, new Vector2Int(8, 5), .75, .75); motion.Evaluate(simulation, worker);
            Set(state, nameof(WorkerComponent.MoveProgress), .1); Clock(.8);
            motion.Evaluate(simulation, worker); Position(new Vector2(5.75f, 5));
            Assert.That(motion.IsCorrecting, Is.True);
        }

        [Test]
        public void ANewBlockingFacilitySnapsToTheCurrentSafeLogicalCell()
        {
            var origin = new Vector2Int(5, 5);
            Route(origin, new Vector2Int(8, 5), .5, .5); motion.Evaluate(simulation, worker);
            var wall = simulation.Registry.Create(facility);
            Assert.That(map.Commands.Place(wall.Id, origin + Vector2Int.right).Succeeded, Is.True);
            motion.Evaluate(simulation, worker); Position(origin);
            Assert.That(motion.IsCorrecting, Is.False);
        }

        [Test]
        public void LongAdvancePublishesTheLatestRealSegmentWithoutReplayingOldCorners()
        {
            var origin = new Vector2Int(5, 5);
            Route(origin, new Vector2Int(8, 5), .25, .25); motion.Evaluate(simulation, worker);
            Clock(2.25); WorkerManager.Move(simulation, worker, target, 2);
            motion.Evaluate(simulation, worker);
            Position(new Vector2(7.25f, 5)); Assert.That(motion.IsCorrecting, Is.False);
            Assert.That(worker.Placement.Origin, Is.EqualTo(new Vector2Int(7, 5)));
        }

        [Test]
        public void CommandRelocationAndSessionReplacementNeverBorrowAnOldDisplayPosition()
        {
            Route(new Vector2Int(5, 5), new Vector2Int(8, 5), .75, .75);
            motion.Evaluate(simulation, worker);
            Assert.That(map.Commands.Place(worker.Id, new Vector2Int(2, 2)).Succeeded, Is.True);
            motion.Evaluate(simulation, worker); Position(new Vector2(2, 2));
            var otherRegistry = new ThingRegistry(GameplayComponentFactory.Attach);
            var otherMap = new GridMap(otherRegistry, 16, 16);
            var other = new LaboratoryManager(otherRegistry, otherMap, organic);
            var replacement = otherRegistry.Create(creature);
            Assert.That(otherMap.Commands.Place(replacement.Id, new Vector2Int(9, 9)).Succeeded, Is.True);
            motion.Evaluate(other, replacement); Position(new Vector2(9, 9));
            Assert.That(motion.IsCorrecting, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RestoredProgressQueriesOnceAndCachesSuccessOrFailure(bool blocked)
        {
            var origin = new Vector2Int(5, 5); var goal = new Vector2Int(8, 5);
            Route(origin, goal, .5, .5);
            Field(state, "Route", null);
            if (blocked)
            {
                var wall = simulation.Registry.Create(facility);
                Assert.That(map.Commands.Place(wall.Id, goal).Succeeded, Is.True);
            }
            var queries = map.PathQueries;
            var job = state.Job; var logical = worker.Placement.Origin;
            for (var i = 0; i < 20; i++) motion.Evaluate(simulation, worker);
            Assert.That(map.PathQueries - queries, Is.EqualTo(1));
            Position(origin + (blocked ? Vector2.zero : Vector2.right * .5f));
            Assert.That(state.Job, Is.SameAs(job)); Assert.That(state.MoveProgress, Is.EqualTo(.5));
            Assert.That(worker.Placement.Origin, Is.EqualTo(logical));
            Assert.That(state.HasCachedMoveRoute, Is.False, "Display reconstruction must not prepare a gameplay route.");
        }

        [Test]
        public void NormalDisplayReadsLeaveTheRouteJobAndResourcesUnchanged()
        {
            Route(new Vector2Int(5, 5), new Vector2Int(8, 5), .5, .5);
            var job = state.Job; var queryCount = map.PathQueries;
            var route = typeof(WorkerComponent).GetField("Route", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state);
            for (var i = 0; i < 1000; i++) motion.Evaluate(simulation, worker);
            Assert.That(state.Job, Is.SameAs(job)); Assert.That(state.MoveProgress, Is.EqualTo(.5));
            Assert.That(map.PathQueries, Is.EqualTo(queryCount));
            Assert.That(simulation.TotalAt(worker.Id), Is.Zero); Assert.That(simulation.Reservations.Count, Is.Zero);
            Assert.That(typeof(WorkerComponent).GetField("Route", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state), Is.SameAs(route));
        }

        private void Route(Vector2Int from, Vector2Int goal, double progress, double at)
        {
            if (!worker.IsPlaced || worker.Placement.Origin != from)
                Assert.That(map.Commands.Place(worker.Id, from).Succeeded, Is.True);
            // Keep the facility behind its interaction cell so it does not obstruct the tested straight segment.
            var rotation = goal.y < from.y ? QuarterTurn.Clockwise180 : QuarterTurn.None;
            var targetOrigin = goal + (rotation == QuarterTurn.Clockwise180 ? Vector2Int.down : Vector2Int.up);
            if (!target.IsPlaced || target.Placement.Origin != targetOrigin || target.Placement.Rotation != rotation)
                Assert.That(map.Commands.Place(target.Id, targetOrigin, rotation).Succeeded, Is.True);
            Set(state, nameof(WorkerComponent.Job), Job(target.Id));
            Set(state, nameof(WorkerComponent.MoveProgress), progress);
            Set(state, nameof(WorkerComponent.WaitReason), WorkerWaitReason.None);
            Field(state, "Route", GridPathfinder.FindPath(map, from, goal));
            Field(state, "RouteOrigin", from); Field(state, "RouteGoal", goal);
            Field(state, "RouteIndex", 0); Field(state, "RouteVersion", map.LayoutVersion);
            Clock(at);
        }
        private void Idle(Vector2Int origin, double at)
        {
            Assert.That(map.Commands.Place(worker.Id, origin).Succeeded, Is.True);
            Set(state, nameof(WorkerComponent.Job), null); Field(state, "Route", null);
            Set(state, nameof(WorkerComponent.MoveProgress), 0.0);
            Set(state, nameof(WorkerComponent.WaitReason), WorkerWaitReason.NoWork); Clock(at);
        }
        private void Observe(long sequence, Vector2Int from, Vector2Int to, double at, bool wander)
            => Set(state, nameof(WorkerComponent.LastMove), new WorkerMoveObservation(sequence, from, to, at, wander, map.LayoutVersion));
        private static WorkerJob Job(Guid targetId)
        {
            var job = new WorkerJob(); Set(job, nameof(WorkerJob.TargetId), targetId);
            Set(job, nameof(WorkerJob.Kind), WorkKind.Gathering); Set(job, nameof(WorkerJob.Stage), JobStage.ToDestination);
            return job;
        }
        private void Clock(double at) => typeof(LaboratoryManager).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(simulation, at);
        private void Position(Vector2 expected)
            => Assert.That(Vector2.Distance(motion.Position, expected), Is.LessThan(.0001), "Display cell origin differs.");
        private static void Set(object targetObject, string property, object value)
            => targetObject.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).SetValue(targetObject, value);
        private static void Field(object targetObject, string name, object value)
            => targetObject.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(targetObject, value);
    }
}
