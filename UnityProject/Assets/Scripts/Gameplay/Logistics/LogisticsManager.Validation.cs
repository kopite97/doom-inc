using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed partial class LogisticsManager
    {
        // This is a graph check, not recovery. Corrupt saves must not silently cancel
        // Jobs, drop reservations, finish a batch, or synthesize replacement inventory.
        public void Validate()
        {
            var reservedWorkers = new HashSet<Guid>();
            foreach (var reservation in reservations)
            {
                if (reservation == null || !reservedWorkers.Add(reservation.WorkerId) || reservation.Amount <= 0 ||
                    (reservation.Purpose != WorkKind.Construction && reservation.Purpose != WorkKind.Hauling))
                    throw Invalid("Duplicate worker or invalid transport reservation.");
                var worker = Require(reservation.WorkerId);
                var source = Require(reservation.SourceId);
                var destination = Require(reservation.DestinationId);
                var resource = Require(reservation.ResourceId);
                if (!worker.TryGetComponent<WorkerComponent>(out var state) || !(worker.Definition is CreatureDefinition definition) ||
                    !simulation.Supports(resource.Definition as ResourceDefinition) || !resource.TryGetComponent<ResourceStackComponent>(out _) ||
                    reservation.SourceId == reservation.DestinationId || reservation.Amount > definition.CarryCapacity ||
                    Quantity(resource) < reservation.Amount)
                    throw Invalid("Reservation endpoints or resource quantity are invalid.");
                var job = state.Job;
                if (job == null || job.Kind != reservation.Purpose || job.TargetId != destination.Id ||
                    job.SourceId != source.Id || job.ResourceId != resource.Id || job.Amount != reservation.Amount ||
                    job.BatchOriginId != reservation.BatchOriginId)
                    throw Invalid("A reservation and its worker Job disagree.");
                if (!reservation.PickedUp)
                {
                    if (job.Stage != JobStage.ToSupply || resource.HolderId != source.Id ||
                        simulation.TotalAt(worker.Id) + reservation.Amount > definition.CarryCapacity ||
                        !simulation.IsComplete(source) || !simulation.Distance(worker, source).HasValue ||
                        !simulation.Distance(source, destination).HasValue)
                        throw Invalid("Outbound reservation has invalid ownership, phase, capacity or route.");
                    if (reservation.Purpose == WorkKind.Construction && !source.TryGetComponent<StorageComponent>(out _))
                        throw Invalid("Construction supply must come from storage or already held bootstrap resources.");
                    if (reservation.Purpose == WorkKind.Hauling && !source.TryGetComponent<StorageComponent>(out _) &&
                        !source.TryGetComponent<GatheringComponent>(out _) && !source.TryGetComponent<ProductionComponent>(out _))
                        throw Invalid("Hauling source cannot supply output or relocation inventory.");
                }
                else
                {
                    if (!HeldBy(resource, worker) || job.Stage != JobStage.ToDestination)
                        throw Invalid("Picked-up reservation has invalid ownership or phase.");
                    if (!simulation.Distance(worker, destination).HasValue)
                        throw Invalid("A moving transport has no route to its destination.");
                }
                if (reservation.Purpose == WorkKind.Construction)
                {
                    if (resource.Definition != simulation.Organic || reservation.BatchOriginId.HasValue || !IsUnfinished(destination) ||
                        ConstructionFree(destination, reservation) < reservation.Amount)
                        throw Invalid("Construction delivery exceeds the site's unreserved cost.");
                }
                else
                {
                    if (destination.TryGetComponent<ProductionComponent>(out _) &&
                        (reservation.BatchOriginId.HasValue || !source.TryGetComponent<StorageComponent>(out _)))
                        throw Invalid("Production input must come from actual storage admission.");
                    if (!Accepts(destination, (ResourceDefinition)resource.Definition) || DestinationFree(destination, (ResourceDefinition)resource.Definition, reservation) < reservation.Amount)
                        throw Invalid("Storage incoming reservations exceed allowed space.");
                    if ((source.TryGetComponent<GatheringComponent>(out _) || source.TryGetComponent<ProductionComponent>(out _)) && reservation.BatchOriginId != source.Id)
                        throw Invalid("Gathering output lost its batch origin.");
                }
            }
            foreach (var group in reservations.Where(r => !r.PickedUp).GroupBy(r => r.ResourceId))
                if (group.Sum(r => r.Amount) > Quantity(Require(group.Key)))
                    throw Invalid("Multiple outgoing reservations exceed the available resource stack.");

            var heldBatchAmounts = new Dictionary<Guid, int>();
            foreach (var worker in simulation.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out _)))
            {
                var state = worker.GetComponent<WorkerComponent>();
                var job = state.Job;
                if (!IsTransport(job))
                {
                    if (reservedWorkers.Contains(worker.Id)) throw Invalid("An idle worker owns a reservation.");
                    continue;
                }
                if ((job.Kind != WorkKind.Construction && job.Kind != WorkKind.Hauling) ||
                    !job.ResourceId.HasValue || !job.SourceId.HasValue || job.Amount <= 0)
                    throw Invalid("Transport Job has missing resource relationships.");
                var resource = Require(job.ResourceId.Value);
                if (!simulation.Supports(resource.Definition as ResourceDefinition) || !resource.TryGetComponent<ResourceStackComponent>(out _))
                    throw Invalid("Transport Job references a non-resource Thing.");
                if (!reservedWorkers.Contains(worker.Id))
                {
                    if (job.Kind != WorkKind.Hauling || job.Stage != JobStage.Waiting || job.TargetId != Guid.Empty ||
                        job.SourceId != worker.Id || !HeldBy(resource, worker) || job.Amount != Quantity(resource))
                        throw Invalid("Only a held waiting shipment may lack reservations.");
                }
                if (job.BatchOriginId.HasValue)
                {
                    var origin = Require(job.BatchOriginId.Value);
                    if (job.Kind != WorkKind.Hauling || !simulation.IsComplete(origin) ||
                        (!origin.TryGetComponent<GatheringComponent>(out _) && !origin.TryGetComponent<ProductionComponent>(out _)))
                        throw Invalid("A carried batch has no completed gathering origin.");
                    var originDefinition = (FacilityDefinition)origin.Definition;
                    if (resource.Definition != (originDefinition.Role == FacilityRole.Gathering
                        ? originDefinition.Resource : originDefinition.OutputResource))
                        throw Invalid("Carried output does not match its origin's recipe.");
                    if (HeldBy(resource, worker))
                    {
                        heldBatchAmounts.TryGetValue(origin.Id, out var amount);
                        heldBatchAmounts[origin.Id] = checked(amount + Quantity(resource));
                    }
                    else if (resource.HolderId != origin.Id)
                        throw Invalid("An unpicked batch is not owned by its gathering origin.");
                }
            }
            foreach (var thing in simulation.Registry.Things)
            {
                if (thing.TryGetComponent<GatheringComponent>(out var gathering))
                {
                    heldBatchAmounts.TryGetValue(thing.Id, out var held);
                    if (gathering.Outstanding != checked(simulation.TotalAt(thing.Id) + held))
                        throw Invalid("Outstanding gathering output does not match unadmitted real resources.");
                }
                if (thing.TryGetComponent<ProductionComponent>(out var production))
                {
                    heldBatchAmounts.TryGetValue(thing.Id, out var held);
                    var definition = (FacilityDefinition)thing.Definition;
                    var unadmitted = thing.GetComponent<ResourceContainerComponent>().Contents
                        .Where(t => t.Definition == definition.OutputResource).Sum(Quantity);
                    if (production.Outstanding != checked(unadmitted + held))
                        throw Invalid("Processing output does not match unadmitted real resources.");
                }
                if (thing.Definition.Kind == ThingKind.Resource)
                {
                    var holder = Require(thing.HolderId ?? Guid.Empty);
                    if (!holder.TryGetComponent<WorkerComponent>(out _) &&
                        !holder.TryGetComponent<StorageComponent>(out _) && !holder.TryGetComponent<GatheringComponent>(out _) &&
                        !holder.TryGetComponent<ProductionComponent>(out _))
                        throw Invalid("A resource has no supported gameplay holder.");
                    if (holder.TryGetComponent<GatheringComponent>(out _) && thing.Definition != simulation.Organic)
                        throw Invalid("A gathering facility cannot hold production inputs.");
                }
            }
        }
    }
}
