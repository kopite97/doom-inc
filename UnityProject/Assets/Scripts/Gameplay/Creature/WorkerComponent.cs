using MobileGame.Foundation;

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
        internal UnityEngine.Vector2Int? IdleOrigin;
        internal double NextWander;
        internal int WanderSequence;
        internal System.Collections.Generic.IReadOnlyList<UnityEngine.Vector2Int> Route;
        internal int RouteIndex;
        internal long RouteVersion;
        internal UnityEngine.Vector2Int RouteOrigin;
        internal UnityEngine.Vector2Int RouteGoal;

        public System.Guid? AssignedFacilityId { get; internal set; }
        public WorkerJob Job { get; internal set; }
        public double MoveProgress { get; internal set; }
        // Read-only observation of the actual cached path, never a second path calculation.
        public UnityEngine.Vector2Int? NextMoveCell => Owner.IsPlaced && Job != null
            && Job.Stage != JobStage.Working && Job.Stage != JobStage.Waiting
            && WaitReason == WorkerWaitReason.None && MoveProgress > 0 && Route != null
            && RouteOrigin == Owner.Placement.Origin && RouteIndex < Route.Count ? Route[RouteIndex] : (UnityEngine.Vector2Int?)null;
        public int[] Priorities { get; internal set; } = new[] { 1, 2, 3, 4 };
        public bool UsesPreset { get; internal set; } = true;
        public WorkerWaitReason WaitReason { get; internal set; }
    }
}
