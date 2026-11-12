using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    public sealed class WorkerComponent : ThingComponent
    {
        internal WorkerComponent(Thing owner) : base(owner) { }

        // Disposable search state. Only the authoritative job, position and cargo are restored.
        internal long EvaluatedStep = -1;
        internal long ObservedWorkVersion = -1;
        internal double NextSearch;
        internal WorkerJob EvaluatedJob;
        internal Vector2Int? IdleOrigin;
        internal double NextWander;
        internal int WanderSequence;
        internal System.Collections.Generic.IReadOnlyList<Vector2Int> Route;
        internal int RouteIndex;
        internal long RouteVersion;
        internal Vector2Int RouteOrigin;
        internal Vector2Int RouteGoal;

        public Guid? AssignedFacilityId { get; internal set; }
        public WorkerJob Job { get; internal set; }
        public double MoveProgress { get; internal set; }
        public bool HasCachedMoveRoute => Route != null;
        public WorkerMoveObservation LastMove { get; internal set; }

        // Keep the existing observation on the same validation path as the full segment.
        public Vector2Int? NextMoveCell => TryGetMoveSegment(Owner.Map, out var segment)
            ? segment.To : (Vector2Int?)null;

        internal bool TryGetMoveSegment(GridMap map, out WorkerMoveSegment segment)
        {
            segment = default;
            if (map == null || !ReferenceEquals(Owner.Map, map) || !Owner.Exists || !Owner.IsPlaced
                || !(Owner.Definition is CreatureData definition) || Job == null
                || (Job.Stage != JobStage.ToSupply && Job.Stage != JobStage.ToDestination)
                || WaitReason != WorkerWaitReason.None || (int)Job.Kind < 0 || (int)Job.Kind > 3
                || Route == null || RouteIndex < 0 || RouteIndex >= Route.Count
                || RouteVersion != map.LayoutVersion || RouteOrigin != Owner.Placement.Origin)
                return false;

            var duration = definition.MoveSecondsPerCell;
            if (duration <= 0 || double.IsNaN(duration) || double.IsInfinity(duration)
                || MoveProgress < 0 || double.IsNaN(MoveProgress) || double.IsInfinity(MoveProgress)
                || MoveProgress > duration)
                return false;

            var targetId = Job.Stage == JobStage.ToSupply ? Job.SourceId : Job.TargetId;
            if (!targetId.HasValue || targetId.Value == Guid.Empty
                || !map.Registry.TryGet(targetId.Value, out var target) || !target.Exists
                || !ReferenceEquals(target.Map, map) || !target.IsPlaced)
                return false;
            var targetPlacement = target.Placement;
            var goal = targetPlacement.InteractionCell ?? targetPlacement.Origin;
            if (RouteGoal != goal) return false;

            var from = Owner.Placement.Origin;
            var to = Route[RouteIndex];
            if (Math.Abs((long)to.x - from.x) + Math.Abs((long)to.y - from.y) != 1
                || !map.IsWalkable(from) || !map.IsWalkable(to))
                return false;

            segment = new WorkerMoveSegment(from, to, MoveProgress, duration,
                targetId.Value, Job.Kind, Job.Stage, map.LayoutVersion);
            return true;
        }

        public int[] Priorities { get; internal set; } = new[] { 1, 2, 3, 4 };
        public bool UsesPreset { get; internal set; } = true;
        public WorkerWaitReason WaitReason { get; internal set; }
    }
}
