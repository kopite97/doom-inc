using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    public sealed class WorkerManager
    {
        private readonly LaboratoryManager simulation;

        public WorkerManager(LaboratoryManager simulation)
        {
            this.simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }

        private int[] preset = { 1, 2, 3, 4 };
        public IReadOnlyList<int> Preset => Array.AsReadOnly(preset);
        public string LastError { get; private set; }
        private bool Reject(string message) { LastError = message; return false; }
        public bool SetPreset(int[] priorities)
        {
            if (!LaboratoryManager.ValidPriorities(priorities)) return Reject("Provide nonnegative priorities for each work kind.");
            preset = priorities.ToArray();
            foreach (var thing in simulation.Registry.Things)
                if (thing.TryGetComponent<WorkerComponent>(out var worker) && worker.UsesPreset)
                    worker.Priorities = preset.ToArray();
            LastError = null;
            return true;
        }

        public bool SetWorkerPriorities(Guid workerId, int[] priorities, bool usePreset = false)
        {
            if (!LaboratoryManager.ValidPriorities(priorities) || !simulation.Registry.TryGet(workerId, out var thing) ||
                !thing.TryGetComponent<WorkerComponent>(out var worker))
                return Reject("Select a worker and provide nonnegative work priorities.");
            worker.UsesPreset = usePreset;
            worker.Priorities = (usePreset ? preset : priorities).ToArray();
            LastError = null;
            return true;
        }

        public ISet<Guid> ProducingFacilities { get; } = new HashSet<Guid>();
        public bool Assign(Guid workerId, Guid? facilityId)
        {
            if (!simulation.Registry.TryGet(workerId, out var thing) || !(thing.Definition is CreatureData definition)
                || !definition.CanOperate || !thing.TryGetComponent<WorkerComponent>(out var worker))
                return Reject("Only support creatures can be assigned.");
            if (facilityId.HasValue && (!simulation.Registry.TryGet(facilityId.Value, out var facility)
                || !simulation.IsComplete(facility) || !facility.TryGetComponent<ProductionComponent>(out _)))
                return Reject("Choose a completed processing or cultivation facility.");
            if (worker.AssignedFacilityId != facilityId)
            {
                worker.AssignedFacilityId = facilityId;
                if (!IsTransport(worker.Job)) Clear(worker, string.Empty);
            }
            LastError = null;
            return true;
        }

        public void Tick(double seconds)
        {
            ValidateTime(seconds);
            ProducingFacilities.Clear();
            if (seconds == 0 || simulation.IsEditing || simulation.Map.Commands.IsEditing) return;
            Reconcile();
            foreach (var thing in Workers())
            {
                var worker = thing.GetComponent<WorkerComponent>();
                if (!thing.IsPlaced) continue;
                if (IsTransport(worker.Job))
                {
                    simulation.Logistics.AdvanceTransport(thing, seconds);
                    continue;
                }

                // Actual work finishes its current operation. Non-transport movement may
                // reconsider new candidates/priorities without losing progress on the same route.
                if (worker.Job == null || worker.Job.Stage != JobStage.Working)
                {
                    var previous = worker.Job;
                    var progress = worker.MoveProgress;
                    worker.Job = null;
                    SelectWork(thing);
                    if (previous != null && worker.Job != null && previous.Kind == worker.Job.Kind
                        && previous.TargetId == worker.Job.TargetId && !IsTransport(worker.Job))
                        worker.MoveProgress = progress;
                }

                if (worker.Job == null) continue;
                if (IsTransport(worker.Job))
                {
                    simulation.Logistics.AdvanceTransport(thing, seconds);
                    continue;
                }
                if (!simulation.Registry.TryGet(worker.Job.TargetId, out var target))
                {
                    Clear(worker, "Target is unavailable");
                    continue;
                }
                var cell = simulation.TargetCell(target);
                if (!cell.HasValue || thing.Placement.Origin != cell.Value)
                {
                    worker.Job.Stage = JobStage.ToDestination;
                    Move(simulation, thing, target, seconds);
                    // Even if the last step arrived, this delta already paid for movement.
                    continue;
                }

                worker.Job.Stage = JobStage.Working;
                worker.MoveProgress = 0;
                worker.WaitReason = string.Empty;
                AdvanceWork(thing, target, seconds);
            }
        }

        public void Reconcile()
        {
            foreach (var thing in Workers())
            {
                var worker = thing.GetComponent<WorkerComponent>();
                var job = worker.Job;
                if (job == null || IsTransport(job)) continue;
                if (!thing.IsPlaced || !simulation.Registry.TryGet(job.TargetId, out var target)
                    || !target.IsPlaced || !ValidWorkTarget(target, job.Kind))
                {
                    Clear(worker, "Work target is unavailable");
                    continue;
                }
                if (!simulation.Distance(thing, target).HasValue)
                {
                    Clear(worker, "Work target is unreachable");
                    continue;
                }
                // Moving a facility ends actual work until the worker reaches it again.
                // Arrival alone is not work: a ToDestination Job must still reconsider
                // current priorities before its first actual work delta is spent.
                if (thing.Placement.Origin != simulation.TargetCell(target))
                    worker.Job.Stage = JobStage.ToDestination;
            }
        }

        public static bool Move(LaboratoryManager simulation, Thing thing, Thing target, double seconds)
        {
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            ValidateTime(seconds);
            if (thing == null || !thing.IsPlaced || target == null || simulation.IsEditing
                || simulation.Map.Commands.IsEditing) return false;
            var worker = thing.GetComponent<WorkerComponent>();
            var goal = simulation.TargetCell(target);
            if (!goal.HasValue)
            {
                worker.WaitReason = "Target is unavailable";
                return false;
            }
            var path = GridPathfinder.FindPath(simulation.Map, thing.Placement.Origin, goal.Value);
            if (path == null)
            {
                worker.WaitReason = "Path is blocked";
                return false;
            }
            if (path.Count == 0)
            {
                worker.MoveProgress = 0;
                return true;
            }

            worker.WaitReason = string.Empty;
            var stepSeconds = ((CreatureData)thing.Definition).MoveSecondsPerCell;
            worker.MoveProgress += seconds;
            foreach (var cell in path)
            {
                if (worker.MoveProgress + 1e-9 < stepSeconds) return false;
                var result = simulation.Map.Commands.Place(thing.Id, cell);
                if (!result.Succeeded)
                {
                    worker.WaitReason = "Path is blocked";
                    return false;
                }
                worker.MoveProgress = Math.Max(0, worker.MoveProgress - stepSeconds);
            }
            worker.MoveProgress = 0;
            worker.WaitReason = string.Empty;
            return true;
        }

        private void SelectWork(Thing thing)
        {
            var worker = thing.GetComponent<WorkerComponent>();
            var priorities = worker.UsesPreset ? simulation.Preset : worker.Priorities;
            worker.MoveProgress = 0;
            foreach (var kind in new[] { WorkKind.Construction, WorkKind.Gathering, WorkKind.Hauling, WorkKind.Production }
                .Where(kind => Priority(priorities, kind) > 0).OrderBy(kind => Priority(priorities, kind)).ThenBy(kind => (int)kind))
            {
                if (kind == WorkKind.Production && SelectProduction(thing)) return;
                if (kind == WorkKind.Construction && SelectConstruction(thing)) return;
                if (kind == WorkKind.Gathering && SelectGathering(thing)) return;
                if (kind == WorkKind.Hauling && simulation.Logistics.TryCreateHaulingJob(thing)) return;
            }
            worker.WaitReason = priorities.All(value => value == 0)
                ? "All work priorities are disabled" : "No reachable work or valid destination";
        }

        private static int Priority(IReadOnlyList<int> priorities, WorkKind kind) =>
            (int)kind < priorities.Count ? priorities[(int)kind] : 4;

        private bool SelectProduction(Thing thing)
        {
            var worker = thing.GetComponent<WorkerComponent>();
            if (!worker.AssignedFacilityId.HasValue || !simulation.Registry.TryGet(worker.AssignedFacilityId.Value, out var target)
                || !simulation.Production.CanOperate(target) || Claimed(target, thing)) return false;
            AssignWork(thing, target, WorkKind.Production);
            return true;
        }

        private bool SelectConstruction(Thing worker)
        {
            foreach (var target in ReachableTargets(worker).Where(thing =>
                thing.TryGetComponent<ConstructionComponent>(out var construction) && !construction.IsComplete))
            {
                var definition = (FacilityData)target.Definition;
                var construction = target.GetComponent<ConstructionComponent>();
                if (construction.Delivered < definition.BuildCost)
                {
                    if (simulation.Logistics.TryCreateConstructionJob(worker, target)) return true;
                }
                else if (!Claimed(target, worker))
                {
                    AssignWork(worker, target, WorkKind.Construction);
                    return true;
                }
            }
            return false;
        }

        private bool SelectGathering(Thing worker)
        {
            foreach (var target in ReachableTargets(worker).Where(thing =>
                ValidWorkTarget(thing, WorkKind.Gathering) && !Claimed(thing, worker)))
            {
                AssignWork(worker, target, WorkKind.Gathering);
                return true;
            }
            return false;
        }

        private void AssignWork(Thing thing, Thing target, WorkKind kind)
        {
            var worker = thing.GetComponent<WorkerComponent>();
            worker.Job = new WorkerJob
            {
                Kind = kind, TargetId = target.Id,
                Stage = thing.Placement.Origin == simulation.TargetCell(target)
                    ? JobStage.Working : JobStage.ToDestination
            };
            worker.WaitReason = string.Empty;
        }

        private void AdvanceWork(Thing thing, Thing target, double seconds)
        {
            var worker = thing.GetComponent<WorkerComponent>();
            if (ClaimedByEarlierWorker(target, thing))
            {
                Clear(worker, "Another worker is using this facility");
                return;
            }
            var definition = (FacilityData)target.Definition;
            if (worker.Job.Kind == WorkKind.Production)
            { ProducingFacilities.Add(target.Id); return; }
            if (worker.Job.Kind == WorkKind.Construction)
            {
                var construction = target.GetComponent<ConstructionComponent>();
                construction.Progress = Math.Min(definition.BuildSeconds, construction.Progress + seconds);
                if (construction.Progress + 1e-9 < definition.BuildSeconds) return;
                construction.Progress = definition.BuildSeconds;
                construction.IsComplete = true;
                Clear(worker, string.Empty);
            }
            else if (worker.Job.Kind == WorkKind.Gathering)
            {
                var gathering = target.GetComponent<GatheringComponent>();
                if (gathering.Outstanding > 0) { Clear(worker, "Completed output awaits storage"); return; }
                // The completed previous batch no longer blocks the next actual work cycle.
                if (gathering.Progress >= definition.GatheringSeconds) gathering.Progress = 0;
                gathering.Progress = Math.Min(definition.GatheringSeconds, gathering.Progress + seconds);
                if (gathering.Progress + 1e-9 < definition.GatheringSeconds) return;
                simulation.Registry.CreateResource(definition.Resource, definition.GatheringAmount,
                    target.GetComponent<ResourceContainerComponent>());
                gathering.Progress = definition.GatheringSeconds;
                gathering.Outstanding = definition.GatheringAmount;
                Clear(worker, string.Empty);
            }
        }

        private IEnumerable<Thing> ReachableTargets(Thing worker) => simulation.Registry.Things
            .Where(thing => thing.IsPlaced && thing.Definition is FacilityData)
            .Select(thing => new { Thing = thing, Distance = simulation.Distance(worker, thing) })
            .Where(item => item.Distance.HasValue).OrderBy(item => item.Distance.Value)
            .ThenBy(item => item.Thing.CreationOrder).Select(item => item.Thing);

        private bool ValidWorkTarget(Thing target, WorkKind kind)
        {
            if (!(target.Definition is FacilityData definition)) return false;
            if (kind == WorkKind.Production) return simulation.Production.CanOperate(target);
            if (kind == WorkKind.Construction)
                return target.TryGetComponent<ConstructionComponent>(out var construction)
                    && !construction.IsComplete && construction.Delivered >= definition.BuildCost;
            return kind == WorkKind.Gathering && simulation.IsComplete(target)
                && target.TryGetComponent<GatheringComponent>(out var gathering) && gathering.Outstanding == 0;
        }

        private bool Claimed(Thing target, Thing worker) => Workers().Any(other => other.Id != worker.Id
            && HasWorkClaim(other.GetComponent<WorkerComponent>().Job, target.Id));

        private bool ClaimedByEarlierWorker(Thing target, Thing worker) => Workers().Any(other =>
            other.CreationOrder < worker.CreationOrder && HasWorkClaim(other.GetComponent<WorkerComponent>().Job, target.Id));

        private static bool HasWorkClaim(WorkerJob job, Guid targetId) => job != null
            && !IsTransport(job) && job.TargetId == targetId;

        private IEnumerable<Thing> Workers() => simulation.Registry.Things
            .Where(thing => thing.TryGetComponent<WorkerComponent>(out _))
            .OrderBy(thing => thing.CreationOrder).ToArray();

        private static bool IsTransport(WorkerJob job) => job != null
            && (job.Kind == WorkKind.Hauling || (job.Amount > 0 && job.ResourceId.HasValue));

        private static void Clear(WorkerComponent worker, string reason)
        {
            worker.Job = null;
            worker.MoveProgress = 0;
            worker.WaitReason = reason;
        }

        private static void ValidateTime(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
        }
    }
}
