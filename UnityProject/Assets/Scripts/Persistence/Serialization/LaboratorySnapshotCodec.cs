using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;

namespace MobileGame.Persistence
{
    internal static class LaboratorySnapshotCodec
    {
        public static LaboratorySnapshot Capture(LaboratoryManager simulation)
        {
            simulation.ValidateState();
            var things = new List<GameplayThingSnapshot>();
            foreach (var thing in simulation.Registry.Things.OrderBy(t => t.CreationOrder))
            {
                var saved = new GameplayThingSnapshot { thingId = Id(thing.Id) };
                if (thing.TryGetComponent<ConstructionComponent>(out var construction))
                {
                    saved.hasConstruction = true;
                    saved.construction = new ConstructionSnapshot
                    {
                        delivered = construction.Delivered, progress = construction.Progress,
                        isComplete = construction.IsComplete
                    };
                }
                if (thing.TryGetComponent<GatheringComponent>(out var gathering))
                {
                    saved.hasGathering = true;
                    saved.gathering = new GatheringSnapshot
                    { progress = gathering.Progress, outstanding = gathering.Outstanding };
                }
                if (thing.TryGetComponent<ProductionComponent>(out var production))
                {
                    saved.hasProduction = true;
                    saved.production = new ProductionSnapshot { selectedTarget = production.SelectedTarget,
                        activeTarget = production.ActiveTarget, inProgress = production.InProgress,
                        progress = production.Progress, outstanding = production.Outstanding };
                }
                if (thing.TryGetComponent<StorageComponent>(out var storage))
                {
                    saved.hasStorage = true;
                    saved.storage = new StorageSnapshot { allowsOrganic = storage.AllowsOrganic, allowsCulture = storage.AllowsCulture };
                }
                if (thing.TryGetComponent<WorkerComponent>(out var worker))
                {
                    saved.hasWorker = true;
                    saved.worker = new WorkerSnapshot
                    {
                        assignedFacilityId = Id(worker.AssignedFacilityId),
                        moveProgress = worker.MoveProgress, usesPreset = worker.UsesPreset,
                        priorities = worker.Priorities?.ToArray(), hasJob = worker.Job != null,
                        job = worker.Job == null ? null : new WorkerJobSnapshot
                        {
                            kind = worker.Job.Kind, stage = worker.Job.Stage,
                            targetId = Id(worker.Job.TargetId), sourceId = Id(worker.Job.SourceId),
                            resourceId = Id(worker.Job.ResourceId), amount = worker.Job.Amount,
                            batchOriginId = Id(worker.Job.BatchOriginId)
                        }
                    };
                }
                if (saved.hasConstruction || saved.hasGathering || saved.hasStorage || saved.hasWorker)
                    things.Add(saved);
            }
            return new LaboratorySnapshot
            {
                organicDefinitionId = simulation.Organic.DefinitionId, cultureDefinitionId = simulation.Culture?.DefinitionId,
                preset = simulation.Preset.ToArray(), things = things.ToArray(),
                reservations = simulation.Reservations.Select(reservation => new ReservationSnapshot
                {
                    workerId = Id(reservation.WorkerId), sourceId = Id(reservation.SourceId),
                    destinationId = Id(reservation.DestinationId), resourceId = Id(reservation.ResourceId),
                    amount = reservation.Amount, pickedUp = reservation.PickedUp,
                    batchOriginId = Id(reservation.BatchOriginId), purpose = reservation.Purpose
                }).ToArray()
            };
        }

        public static LaboratoryManager Restore(LaboratorySnapshot saved, ThingRegistry registry,
            GridMap map, Func<string, ThingDefinition> getDefinition, int version, ResourceDefinition defaultCulture,
            InvasionSettings invasionSettings = null)
        {
            ValidateStructure(saved);
            if (!(getDefinition(saved.organicDefinitionId) is ResourceDefinition organic))
                throw Invalid("The laboratory resource definition is not a resource.");
            var byId = saved.things.ToDictionary(state => ParseId(state.thingId));
            foreach (var thing in registry.Things)
            {
                var hasProduction = thing.TryGetComponent<ProductionComponent>(out var production);
                var hasConstruction = thing.TryGetComponent<ConstructionComponent>(out var construction);
                var hasGathering = thing.TryGetComponent<GatheringComponent>(out var gathering);
                var hasStorage = thing.TryGetComponent<StorageComponent>(out var storage);
                var hasWorker = thing.TryGetComponent<WorkerComponent>(out var worker);
                var hasState = byId.TryGetValue(thing.Id, out var state);
                if (!hasConstruction && !hasGathering && !hasStorage && !hasWorker)
                {
                    if (hasState) throw Invalid("Unexpected gameplay state for Thing " + thing.Id);
                    continue;
                }
                if (!hasState || state.hasConstruction != hasConstruction || state.hasGathering != hasGathering ||
                    state.hasStorage != hasStorage || state.hasWorker != hasWorker || state.hasProduction != hasProduction)
                    throw Invalid("Missing or incompatible component state for Thing " + thing.Id);
                byId.Remove(thing.Id);
                if (hasConstruction)
                {
                    construction.Delivered = state.construction.delivered;
                    construction.Progress = state.construction.progress;
                    construction.IsComplete = state.construction.isComplete;
                }
                if (hasGathering)
                {
                    gathering.Progress = state.gathering.progress;
                    gathering.Outstanding = state.gathering.outstanding;
                }
                if (hasProduction)
                {
                    production.SelectedTarget = state.production.selectedTarget;
                    production.ActiveTarget = state.production.activeTarget;
                    production.InProgress = state.production.inProgress;
                    production.Progress = state.production.progress;
                    production.Outstanding = state.production.outstanding;
                }
                if (hasStorage)
                {
                    storage.AllowsOrganic = state.storage.allowsOrganic;
                    storage.AllowsCulture = version == 2 || state.storage.allowsCulture;
                }
                if (hasWorker)
                {
                    worker.AssignedFacilityId = ParseOptionalId(state.worker.assignedFacilityId);
                    worker.MoveProgress = state.worker.moveProgress;
                    worker.Priorities = state.worker.priorities.ToArray();
                    worker.UsesPreset = state.worker.usesPreset;
                    worker.WaitReason = string.Empty; // Derived by the next work selection/path check.
                    var job = state.worker.job;
                    worker.Job = !state.worker.hasJob ? null : new WorkerJob
                    {
                        Kind = job.kind, Stage = job.stage, TargetId = ParseId(job.targetId, true),
                        SourceId = ParseOptionalId(job.sourceId), ResourceId = ParseOptionalId(job.resourceId),
                        Amount = job.amount, BatchOriginId = ParseOptionalId(job.batchOriginId)
                    };
                }
            }
            if (byId.Count != 0) throw Invalid("Gameplay state references a missing Thing.");
            var reservations = saved.reservations.Select(reservation => new TransportReservation
            {
                WorkerId = ParseId(reservation.workerId), SourceId = ParseId(reservation.sourceId),
                DestinationId = ParseId(reservation.destinationId), ResourceId = ParseId(reservation.resourceId),
                Amount = reservation.amount, PickedUp = reservation.pickedUp,
                BatchOriginId = ParseOptionalId(reservation.batchOriginId), Purpose = reservation.purpose
            }).ToArray();
            var culture = string.IsNullOrEmpty(saved.cultureDefinitionId) ? (version == 2 ? defaultCulture : null)
                : getDefinition(saved.cultureDefinitionId) as ResourceDefinition ?? throw Invalid("Invalid culture definition.");
            var runtime = new LaboratoryManager(registry, map, organic, culture, invasionSettings);
            runtime.RestoreState(saved.preset.ToArray(), reservations);
            // Validate the whole graph only after external component state is restored.
            return runtime;
        }

        public static void ValidateStructure(LaboratorySnapshot saved)
        {
            if (saved == null || string.IsNullOrWhiteSpace(saved.organicDefinitionId))
                throw Invalid("Version 2 requires laboratory state and a resource definition.");
            ValidatePriorities(saved.preset);
            if (saved.things == null || saved.reservations == null)
                throw Invalid("Laboratory component and reservation arrays are required.");
            var ids = new HashSet<Guid>();
            foreach (var thing in saved.things)
            {
                if (thing == null || !ids.Add(ParseId(thing.thingId)))
                    throw Invalid("Missing or duplicate gameplay Thing state.");
                if (!thing.hasConstruction && !thing.hasGathering && !thing.hasStorage && !thing.hasWorker)
                    throw Invalid("Empty gameplay Thing state.");
                if (thing.hasConstruction && (thing.construction == null || thing.construction.delivered < 0 ||
                    !NonNegativeFinite(thing.construction.progress)))
                    throw Invalid("Invalid construction state.");
                if (thing.hasGathering && (thing.gathering == null || thing.gathering.outstanding < 0 ||
                    !NonNegativeFinite(thing.gathering.progress)))
                    throw Invalid("Invalid gathering state.");
                if (thing.hasProduction && (thing.production == null || !NonNegativeFinite(thing.production.progress)
                    || thing.production.outstanding < 0)) throw Invalid("Invalid production state.");
                if (thing.hasStorage && thing.storage == null)
                    throw Invalid("Storage settings are missing.");
                if (thing.hasWorker)
                {
                    if (thing.worker == null || !NonNegativeFinite(thing.worker.moveProgress))
                        throw Invalid("Invalid worker movement state.");
                    ParseOptionalId(thing.worker.assignedFacilityId);
                    ValidatePriorities(thing.worker.priorities);
                    if (thing.worker.hasJob) ValidateJob(thing.worker.job);
                }
            }
            foreach (var reservation in saved.reservations)
            {
                if (reservation == null || reservation.amount <= 0 ||
                    !Enum.IsDefined(typeof(WorkKind), reservation.purpose))
                    throw Invalid("Invalid transport reservation.");
                ParseId(reservation.workerId);
                ParseId(reservation.sourceId);
                ParseId(reservation.destinationId);
                ParseId(reservation.resourceId);
                ParseOptionalId(reservation.batchOriginId);
            }
        }

        private static void ValidateJob(WorkerJobSnapshot job)
        {
            if (job == null || !Enum.IsDefined(typeof(WorkKind), job.kind) ||
                !Enum.IsDefined(typeof(JobStage), job.stage) || job.amount < 0)
                throw Invalid("Invalid worker Job.");
            ParseId(job.targetId, true);
            ParseOptionalId(job.sourceId);
            ParseOptionalId(job.resourceId);
            ParseOptionalId(job.batchOriginId);
        }

        private static void ValidatePriorities(int[] priorities)
        {
            if (priorities == null || (priorities.Length != 3 && priorities.Length != 4) || priorities.Any(priority => priority < 0))
                throw Invalid("Each work kind requires a nonnegative priority.");
        }

        private static bool NonNegativeFinite(double value) => value >= 0 && !double.IsInfinity(value);
        private static string Id(Guid id) => id.ToString("N");
        private static string Id(Guid? id) => id?.ToString("N");
        private static Guid ParseId(string value, bool allowEmpty = false)
        {
            if (!Guid.TryParse(value, out var id) || (!allowEmpty && id == Guid.Empty))
                throw Invalid("Invalid gameplay Thing ID: " + value);
            return id;
        }
        private static Guid? ParseOptionalId(string value)
            => string.IsNullOrEmpty(value) ? (Guid?)null : ParseId(value);
        private static InvalidDataException Invalid(string message) => new InvalidDataException(message);
    }
}
