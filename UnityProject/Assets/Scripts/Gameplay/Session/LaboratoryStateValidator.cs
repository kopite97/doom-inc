using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    // Validate a candidate graph before publishing a restored session. Range checks
    // alone are insufficient: component roles and references must describe real work.
    internal static class LaboratoryStateValidator
    {
        public static void Validate(LaboratoryManager simulation)
        {
            if (!LaboratoryManager.ValidPriorities(simulation.Preset.ToArray()))
                throw Invalid("Invalid laboratory priorities.");
            var workClaims = new HashSet<Guid>();
            foreach (var thing in simulation.Registry.Things)
            {
                if (thing.Definition is ResourceData)
                {
                    if (!simulation.Supports(thing.Definition as ResourceData))
                        throw Invalid("Unsupported laboratory resource.");
                    continue;
                }
                if (thing.Definition is FacilityData facility && facility.Role != FacilityRole.None)
                    ValidateFacility(simulation, thing, facility);
                if (thing.Definition is CreatureData creature)
                {
                    if (creature.CanWork) ValidateWorker(simulation, thing, creature, workClaims);
                }
            }
            simulation.Logistics.Validate();
            simulation.Invasion.ValidateState();
        }

        private static void ValidateFacility(LaboratoryManager simulation, Thing thing, FacilityData definition)
        {
            if (!ReferenceEquals(definition.Resource, simulation.Organic) ||
                !thing.TryGetComponent<ConstructionComponent>(out var construction))
                throw Invalid("Facility has no compatible construction state.");
            if (construction.Delivered < 0 || construction.Delivered > definition.BuildCost ||
                !InRange(construction.Progress, definition.BuildSeconds) ||
                (construction.Progress > 0 && construction.Delivered != definition.BuildCost) ||
                construction.IsComplete != (construction.Progress >= definition.BuildSeconds))
                throw Invalid("Construction material/progress/completion state is inconsistent.");
            if (definition.Role == FacilityRole.Gathering)
            {
                if (!thing.TryGetComponent<GatheringComponent>(out var gathering) ||
                    !InRange(gathering.Progress, definition.GatheringSeconds) ||
                    gathering.Outstanding < 0 || gathering.Outstanding > definition.GatheringAmount ||
                    (!construction.IsComplete && (gathering.Progress > 0 || gathering.Outstanding > 0)) ||
                    (gathering.Outstanding > 0) != (gathering.Progress >= definition.GatheringSeconds))
                    throw Invalid("Gathering progress and the outstanding batch disagree.");
            }
            if (definition.HasProduction) ValidateProduction(simulation, thing, definition, construction);
            if (definition.Role == FacilityRole.Storage)
            {
                if (!thing.TryGetComponent<StorageComponent>(out _) ||
                    simulation.TotalAt(thing.Id) > definition.StorageCapacity ||
                    (!construction.IsComplete && simulation.TotalAt(thing.Id) > 0))
                    throw Invalid("Invalid storage inventory or capacity.");
            }
        }

        private static void ValidateWorker(LaboratoryManager simulation, Thing thing,
            CreatureData definition, HashSet<Guid> workClaims)
        {
            if (!thing.TryGetComponent<WorkerComponent>(out var worker) ||
                !LaboratoryManager.ValidPriorities(worker.Priorities) ||
                !InRange(worker.MoveProgress, definition.MoveSecondsPerCell) ||
                worker.MoveProgress >= definition.MoveSecondsPerCell ||
                simulation.TotalAt(thing.Id) > definition.CarryCapacity)
                throw Invalid("Invalid worker state or carrying capacity.");
            if (worker.AssignedFacilityId.HasValue && (!definition.CanOperate ||
                !simulation.Registry.TryGet(worker.AssignedFacilityId.Value, out var assigned) ||
                !simulation.IsComplete(assigned) || !assigned.TryGetComponent<ProductionComponent>(out _)))
                throw Invalid("Invalid worker production assignment.");
            var job = worker.Job;
            if (job == null)
            {
                if (worker.MoveProgress != 0)
                    throw Invalid("An idle worker cannot retain movement progress.");
                return;
            }
            if (!Enum.IsDefined(typeof(WorkKind), job.Kind) || !Enum.IsDefined(typeof(JobStage), job.Stage) || job.Amount < 0)
                throw Invalid("Unsupported worker Job.");
            if (job.Kind == WorkKind.Hauling || job.ResourceId.HasValue || job.Amount > 0)
                return; // Transport links, held batches and reservations are validated together below.
            if (job.SourceId.HasValue || job.BatchOriginId.HasValue ||
                !simulation.Registry.TryGet(job.TargetId, out var target) ||
                !(target.Definition is FacilityData targetDefinition) ||
                !target.TryGetComponent<ConstructionComponent>(out var construction))
                throw Invalid("Worker Job references an invalid facility.");
            if (job.Stage != JobStage.Working && job.Stage != JobStage.ToDestination && job.Stage != JobStage.Waiting)
                throw Invalid("A facility work Job has an invalid phase.");
            if (!thing.IsPlaced || !target.IsPlaced || !simulation.Distance(thing, target).HasValue)
                throw Invalid("Facility work requires a placed worker, a placed target and a reachable interaction cell.");
            if (job.Stage == JobStage.Working &&
                (thing.Placement.Origin != simulation.TargetCell(target) || worker.MoveProgress != 0))
                throw Invalid("Actual facility work requires its fixed interaction cell and no pending movement.");
            if (!workClaims.Add(target.Id))
                throw Invalid("Multiple workers claim the same facility work slot.");
            if (job.Kind == WorkKind.Construction &&
                (construction.IsComplete || construction.Delivered != targetDefinition.BuildCost))
                throw Invalid("Construction work requires delivered materials and an unfinished site.");
            if (job.Kind == WorkKind.Production && (worker.AssignedFacilityId != target.Id ||
                !simulation.Production.CanOperate(target))) throw Invalid("Invalid production work target.");
            if (job.Kind == WorkKind.Gathering &&
                (!construction.IsComplete || !target.TryGetComponent<GatheringComponent>(out var gathering) || gathering.Outstanding != 0))
                throw Invalid("Gathering work requires an available completed gathering facility.");
        }

        private static void ValidateProduction(LaboratoryManager simulation, Thing thing, FacilityData definition,
            ConstructionComponent construction)
        {
            var state = thing.GetComponent<ProductionComponent>();
            if (!simulation.Supports(definition.InputResource) ||
                (definition.Role == FacilityRole.Processing && !simulation.Supports(definition.OutputResource)) ||
                !Enum.IsDefined(typeof(CultivationTarget), state.SelectedTarget) ||
                !Enum.IsDefined(typeof(CultivationTarget), state.ActiveTarget) ||
                !InRange(state.Progress, definition.ProductionSeconds) || state.Outstanding < 0 || state.Outstanding > definition.OutputAmount)
                throw Invalid("Invalid production definition or progress.");
            if (definition.Role == FacilityRole.Processing && (state.SelectedTarget != CultivationTarget.None || state.ActiveTarget != CultivationTarget.None))
                throw Invalid("Processing cannot select a creature.");
            if (definition.Role == FacilityRole.Cultivation && (state.Outstanding != 0 ||
                state.InProgress != (state.ActiveTarget != CultivationTarget.None)))
                throw Invalid("Cultivation active target and cycle disagree.");
            if ((state.InProgress && (state.Outstanding != 0 || state.Progress >= definition.ProductionSeconds)) ||
                (!state.InProgress && state.Outstanding == 0 && state.Progress != 0) ||
                (state.Outstanding > 0 && state.Progress != definition.ProductionSeconds))
                throw Invalid("Production lifecycle is inconsistent.");
            var inputs = simulation.Production.InputCount(thing);
            if (inputs > definition.InputAmount || ((state.InProgress || state.Outstanding > 0) && inputs != 0) ||
                (definition.Role == FacilityRole.Cultivation && state.SelectedTarget == CultivationTarget.None && inputs != 0))
                throw Invalid("Production input stock exceeds its current cycle.");
            foreach (var resource in thing.GetComponent<ResourceContainerComponent>().Contents)
                if (resource.Definition != definition.InputResource &&
                    !(definition.Role == FacilityRole.Processing && state.Outstanding > 0 && resource.Definition == definition.OutputResource))
                    throw Invalid("Facility holds an incompatible production resource.");
            if (!construction.IsComplete && (state.SelectedTarget != CultivationTarget.None || state.InProgress ||
                state.Progress != 0 || state.Outstanding != 0 || simulation.TotalAt(thing.Id) != 0))
                throw Invalid("Unfinished facility cannot hold production state.");
        }

        private static bool InRange(double value, double maximum) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0 && value <= maximum;
        private static InvalidDataException Invalid(string message) => new InvalidDataException(message);
    }
}
