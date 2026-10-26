using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class WorkerComponent : ThingComponent
    {
        internal WorkerComponent(Thing owner) : base(owner) { }

        public WorkerJob Job { get; internal set; }
        public double MoveProgress { get; internal set; }
        public int[] Priorities { get; internal set; } = new[] { 1, 2, 3 };
        public bool UsesPreset { get; internal set; } = true;
        public string WaitReason { get; internal set; } = string.Empty;
    }
}
