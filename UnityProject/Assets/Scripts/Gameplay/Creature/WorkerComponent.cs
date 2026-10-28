using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class WorkerComponent : ThingComponent
    {
        internal WorkerComponent(Thing owner) : base(owner) { }

        public System.Guid? AssignedFacilityId { get; internal set; }
        public WorkerJob Job { get; internal set; }
        public double MoveProgress { get; internal set; }
        public int[] Priorities { get; internal set; } = new[] { 1, 2, 3, 4 };
        public bool UsesPreset { get; internal set; } = true;
        public string WaitReason { get; internal set; } = string.Empty;
    }
}
