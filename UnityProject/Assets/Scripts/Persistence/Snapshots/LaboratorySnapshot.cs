using System;
using MobileGame.Gameplay;

namespace MobileGame.Persistence
{
    // Version 2 adds only currently implemented gameplay state. Map/Thing ownership
    // remains in the session DTO; transient placement candidates and path caches are omitted.
    [Serializable]
    public sealed class LaboratorySnapshot
    {
        public string organicDefinitionId;
        public int[] preset;
        public GameplayThingSnapshot[] things;
        public ReservationSnapshot[] reservations;
    }

    [Serializable]
    public sealed class GameplayThingSnapshot
    {
        public string thingId;
        public bool hasConstruction;
        public bool hasGathering;
        public bool hasStorage;
        public bool hasWorker;
        public ConstructionSnapshot construction;
        public GatheringSnapshot gathering;
        public StorageSnapshot storage;
        public WorkerSnapshot worker;
    }

    [Serializable]
    public sealed class ConstructionSnapshot
    {
        public int delivered;
        public double progress;
        public bool isComplete;
    }

    [Serializable]
    public sealed class GatheringSnapshot
    {
        public double progress;
        public int outstanding;
    }

    [Serializable]
    public sealed class StorageSnapshot
    {
        public bool allowsOrganic;
    }

    [Serializable]
    public sealed class WorkerSnapshot
    {
        public WorkerJobSnapshot job;
        public bool hasJob;
        public double moveProgress;
        public int[] priorities;
        public bool usesPreset;
    }

    [Serializable]
    public sealed class WorkerJobSnapshot
    {
        public WorkKind kind;
        public JobStage stage;
        public string targetId;
        public string sourceId;
        public string resourceId;
        public int amount;
        public string batchOriginId;
    }

    [Serializable]
    public sealed class ReservationSnapshot
    {
        public string workerId;
        public string sourceId;
        public string destinationId;
        public string resourceId;
        public int amount;
        public bool pickedUp;
        public string batchOriginId;
        public WorkKind purpose;
    }
}
