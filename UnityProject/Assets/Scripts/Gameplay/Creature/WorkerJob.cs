using System;

namespace MobileGame.Gameplay
{
    public enum WorkKind { Construction = 0, Gathering = 1, Hauling = 2, Production = 3 }
    public enum JobStage { ToSupply = 0, ToDestination = 1, Working = 2, Waiting = 3 }

    public sealed class WorkerJob
    {
        public WorkKind Kind { get; internal set; }
        public JobStage Stage { get; internal set; }
        public Guid TargetId { get; internal set; }
        public Guid? SourceId { get; internal set; }
        public Guid? ResourceId { get; internal set; }
        public int Amount { get; internal set; }
        public Guid? BatchOriginId { get; internal set; }
    }
}
