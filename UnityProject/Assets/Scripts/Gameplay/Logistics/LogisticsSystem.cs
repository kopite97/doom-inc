using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    // Reservations are promises, never a second inventory. Resource ownership changes
    // only at pickup/admission, using the same Registry as every other gameplay system.
    public sealed partial class LogisticsSystem
    {
        private readonly LaboratorySimulation simulation;
        private readonly List<TransportReservation> reservations = new List<TransportReservation>();
        private readonly IReadOnlyList<TransportReservation> reservationView;

        public LogisticsSystem(LaboratorySimulation simulation)
        {
            this.simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            reservationView = reservations.AsReadOnly();
        }

        public IReadOnlyList<TransportReservation> Reservations => reservationView;
        public int OutgoingReserved(Guid resourceId) => reservations.Where(r => !r.PickedUp && r.ResourceId == resourceId).Sum(r => r.Amount);
        public int IncomingReserved(Guid destinationId) => reservations.Where(r => r.DestinationId == destinationId).Sum(r => r.Amount);

        public bool TryCreateConstructionJob(Thing worker)
        {
            foreach (var target in Reachable(worker, simulation.Registry.Things.Where(t =>
                t.TryGetComponent<ConstructionComponent>(out var c) && !c.IsComplete)))
                if (TryCreateConstructionJob(worker, target)) return true;
            return false;
        }

        public bool TryCreateConstructionJob(Thing worker, Thing target)
        {
            if (!CanChoose(worker) || !IsUnfinished(target) || !simulation.Distance(worker, target).HasValue)
                return false;
            var missing = ConstructionFree(target);
            if (missing <= 0) return false;
            // Bootstrap resources already belong to the initial worker. Later construction
            // draws from completed storage without making a nonexistent ground inventory.
            var sources = CompletedStores().Concat(new[] { worker });
            foreach (var source in Reachable(worker, sources))
            {
                if (!simulation.Distance(source, target).HasValue) continue;
                NormalizeStock(source);
                foreach (var stock in Stocks(source))
                {
                    var available = Quantity(stock) - OutgoingReserved(stock.Id);
                    var limit = ((CreatureDefinition)worker.Definition).CarryCapacity;
                    if (source.Id != worker.Id) limit -= simulation.TotalAt(worker.Id);
                    var amount = Math.Min(missing, Math.Min(limit, available));
                    if (amount <= 0) continue;
                    Assign(worker, source, target, stock, amount, source.Id == worker.Id, null, WorkKind.Construction);
                    return true;
                }
            }
            return false;
        }

        public bool TryCreateHaulingJob(Thing worker)
        {
            if (!CanChoose(worker)) return false;
            var limit = ((CreatureDefinition)worker.Definition).CarryCapacity - simulation.TotalAt(worker.Id);
            if (limit <= 0) return false;
            foreach (var source in Reachable(worker, simulation.Registry.Things.Where(IsHaulingSource)))
            {
                var destination = NearestStore(source, source.Id);
                if (destination == null) continue;
                NormalizeStock(source);
                foreach (var stock in Stocks(source))
                {
                    var amount = Math.Min(limit, Math.Min(Quantity(stock) - OutgoingReserved(stock.Id), StorageFree(destination)));
                    if (amount <= 0) continue;
                    var batchOrigin = source.TryGetComponent<GatheringComponent>(out _) ? source.Id : (Guid?)null;
                    Assign(worker, source, destination, stock, amount, false, batchOrigin, WorkKind.Hauling);
                    return true;
                }
            }
            return false;
        }

        // True means this is transport (including held waiting), so the worker cannot
        // spend the same delta on another job. Arrival does not also pay for interaction.
        public bool AdvanceTransport(Thing worker, double seconds)
        {
            var state = worker.GetComponent<WorkerComponent>();
            if (!IsTransport(state.Job)) return false;
            ReconcileWorker(worker);
            var reservation = ReservationFor(worker.Id);
            if (reservation == null) return true;
            if (state.Job.Stage == JobStage.Waiting) return true;
            var targetId = reservation.PickedUp ? reservation.DestinationId : reservation.SourceId;
            if (!simulation.Registry.TryGet(targetId, out var target)) return true;
            var cell = simulation.TargetCell(target);
            if (!cell.HasValue || !worker.IsPlaced || worker.Placement.Origin != cell.Value)
            {
                WorkerSystem.Move(simulation, worker, target, seconds);
                return true;
            }
            state.MoveProgress = 0;
            if (!reservation.PickedUp) PickUp(worker, reservation);
            else Deliver(worker, reservation);
            return true;
        }

        public void Reconcile()
        {
            foreach (var worker in simulation.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out _)).ToArray())
                ReconcileWorker(worker);
        }

        public void Restore(IEnumerable<TransportReservation> saved)
        {
            if (saved == null) throw Invalid("Reservation collection is missing.");
            var copy = saved.Select(r => r == null ? throw Invalid("A reservation is missing.") : new TransportReservation
            {
                WorkerId = r.WorkerId, SourceId = r.SourceId, DestinationId = r.DestinationId,
                ResourceId = r.ResourceId, Amount = r.Amount, PickedUp = r.PickedUp,
                BatchOriginId = r.BatchOriginId, Purpose = r.Purpose
            }).ToArray();
            reservations.Clear();
            reservations.AddRange(copy);
        }

        private void Assign(Thing worker, Thing source, Thing target, Thing stock, int amount,
            bool pickedUp, Guid? batchOrigin, WorkKind purpose)
        {
            // Bootstrap material can share the worker's holder without belonging to
            // this delivery. Give only the reserved subset its own identity so later
            // rerouting never commandeers the worker's unrelated initial remainder.
            if (purpose == WorkKind.Construction && pickedUp && source.Id == worker.Id && Quantity(stock) > amount)
                stock = simulation.Registry.TransferResource(stock, worker.GetComponent<ResourceContainerComponent>(), amount);
            var reservation = new TransportReservation
            {
                WorkerId = worker.Id, SourceId = source.Id, DestinationId = target.Id,
                ResourceId = stock.Id, Amount = amount, PickedUp = pickedUp,
                BatchOriginId = batchOrigin, Purpose = purpose
            };
            reservations.Add(reservation);
            var state = worker.GetComponent<WorkerComponent>();
            state.Job = new WorkerJob
            {
                Kind = purpose, Stage = pickedUp ? JobStage.ToDestination : JobStage.ToSupply,
                TargetId = target.Id, SourceId = source.Id, ResourceId = stock.Id,
                Amount = amount, BatchOriginId = batchOrigin
            };
            state.MoveProgress = 0;
            state.WaitReason = string.Empty;
        }

        private void PickUp(Thing worker, TransportReservation reservation)
        {
            if (!ValidUnpicked(worker, reservation)) { CancelUnpicked(worker, reservation); return; }
            simulation.Registry.TryGet(reservation.ResourceId, out var resource);
            var held = simulation.Registry.TransferResource(resource, worker.GetComponent<ResourceContainerComponent>(), reservation.Amount);
            reservation.ResourceId = held.Id;
            reservation.PickedUp = true;
            var state = worker.GetComponent<WorkerComponent>();
            state.Job.ResourceId = held.Id;
            state.Job.Stage = JobStage.ToDestination;
            state.MoveProgress = 0;
        }

        private void Deliver(Thing worker, TransportReservation reservation)
        {
            simulation.Registry.TryGet(reservation.ResourceId, out var held);
            simulation.Registry.TryGet(reservation.DestinationId, out var destination);
            if (reservation.Purpose == WorkKind.Construction)
            {
                if (!IsUnfinished(destination) || !HeldBy(held, worker) || ConstructionFree(destination, reservation) < reservation.Amount)
                { ReconcileWorker(worker); return; }
                var amount = reservation.Amount;
                simulation.Registry.ConsumeResource(held, amount);
                destination.GetComponent<ConstructionComponent>().Delivered += amount;
                reservations.Remove(reservation);
                var state = worker.GetComponent<WorkerComponent>();
                // Delivery finishes this transport. Re-select against current priorities;
                // disabling construction during travel must not force construction work.
                state.Job = null;
                state.MoveProgress = 0;
                state.WaitReason = string.Empty;
                return;
            }

            var accepted = destination == null ? 0 : Math.Min(reservation.Amount, StorageFree(destination, reservation));
            if (!HeldBy(held, worker) || !Accepts(destination) || accepted <= 0)
            { Reroute(worker); return; }
            var before = Quantity(held);
            simulation.Registry.TransferResource(held, destination.GetComponent<ResourceContainerComponent>(), accepted);
            reservations.Remove(reservation);
            if (reservation.BatchOriginId.HasValue)
            {
                var origin = Require(reservation.BatchOriginId.Value);
                var gathering = origin.GetComponent<GatheringComponent>();
                gathering.Outstanding -= accepted;
                if (gathering.Outstanding == 0) gathering.Progress = 0;
            }
            NormalizeStock(destination);
            var workerState = worker.GetComponent<WorkerComponent>();
            if (accepted < before)
            {
                workerState.Job.Amount = before - accepted;
                Reroute(worker);
            }
            else Clear(workerState, string.Empty);
        }

        private void ReconcileWorker(Thing worker)
        {
            var state = worker.GetComponent<WorkerComponent>();
            if (!IsTransport(state.Job)) return;
            var reservation = ReservationFor(worker.Id);
            if (reservation == null)
            {
                if (state.Job.Kind == WorkKind.Hauling && state.Job.Stage == JobStage.Waiting)
                    Reroute(worker);
                return;
            }
            if (!reservation.PickedUp)
            {
                if (!ValidUnpicked(worker, reservation)) CancelUnpicked(worker, reservation);
                return;
            }
            if (reservation.Purpose == WorkKind.Construction)
            {
                if (!simulation.Registry.TryGet(reservation.DestinationId, out var target) || !IsUnfinished(target) ||
                    ConstructionFree(target, reservation) < reservation.Amount)
                {
                    // Logical removal is not a player demolition feature. Preserve real
                    // carried materials if an owning test/system invalidates the site.
                    state.Job.Kind = WorkKind.Hauling;
                    Reroute(worker);
                    return;
                }
                if (!simulation.Distance(worker, target).HasValue)
                {
                    state.Job.Kind = WorkKind.Hauling;
                    Reroute(worker);
                }
                else
                {
                    state.Job.Stage = JobStage.ToDestination;
                    state.WaitReason = string.Empty;
                }
                return;
            }
            if (!simulation.Registry.TryGet(reservation.DestinationId, out var destination) || !Accepts(destination) ||
                StorageFree(destination, reservation) < reservation.Amount || !simulation.Distance(worker, destination).HasValue)
                Reroute(worker);
        }

        private bool ValidUnpicked(Thing worker, TransportReservation reservation)
        {
            if (!simulation.Registry.TryGet(reservation.SourceId, out var source) || !source.IsPlaced ||
                !simulation.Registry.TryGet(reservation.DestinationId, out var destination) ||
                !simulation.Registry.TryGet(reservation.ResourceId, out var resource) ||
                resource.HolderId != source.Id || Quantity(resource) - OutgoingReserved(resource.Id) + reservation.Amount < reservation.Amount ||
                !simulation.Distance(worker, source).HasValue || !simulation.Distance(source, destination).HasValue)
                return false;
            return reservation.Purpose == WorkKind.Construction
                ? IsUnfinished(destination) && ConstructionFree(destination, reservation) >= reservation.Amount
                : Accepts(destination) && StorageFree(destination, reservation) >= reservation.Amount;
        }

        private void CancelUnpicked(Thing worker, TransportReservation reservation)
        {
            reservations.Remove(reservation);
            Clear(worker.GetComponent<WorkerComponent>(), "Transport changed; selecting new work");
        }

        private void Reroute(Thing worker)
        {
            var state = worker.GetComponent<WorkerComponent>();
            var job = state.Job;
            var old = ReservationFor(worker.Id);
            if (old != null) reservations.Remove(old);
            if (job?.ResourceId == null || !simulation.Registry.TryGet(job.ResourceId.Value, out var held) || !HeldBy(held, worker))
                throw Invalid("A carried transport Job has lost its resource.");
            var destination = NearestStore(worker);
            if (destination != null)
            {
                var amount = Math.Min(Quantity(held), StorageFree(destination));
                Assign(worker, worker, destination, held, amount, true, job.BatchOriginId, WorkKind.Hauling);
                return;
            }
            job.Kind = WorkKind.Hauling;
            job.Stage = JobStage.Waiting;
            job.TargetId = Guid.Empty;
            job.SourceId = worker.Id;
            job.Amount = Quantity(held);
            state.MoveProgress = 0;
            state.WaitReason = "Holding resources; no reachable allowed storage space";
        }

        private Thing NearestStore(Thing source, Guid? excluded = null) => Reachable(source,
            CompletedStores().Where(t => t.Id != excluded && Accepts(t) && StorageFree(t) > 0)).FirstOrDefault();
        private IEnumerable<Thing> CompletedStores() => simulation.Registry.Things.Where(t =>
            simulation.IsComplete(t) && t.TryGetComponent<StorageComponent>(out _));
        private IEnumerable<Thing> Reachable(Thing source, IEnumerable<Thing> targets) => targets
            .Select(t => new { Thing = t, Distance = simulation.Distance(source, t) })
            .Where(t => t.Distance.HasValue).OrderBy(t => t.Distance.Value)
            .ThenBy(t => t.Thing.CreationOrder).Select(t => t.Thing).ToArray();
        private IEnumerable<Thing> Stocks(Thing holder) => holder.GetComponent<ResourceContainerComponent>().Contents
            .Where(t => ReferenceEquals(t.Definition, simulation.Organic)).OrderBy(t => t.CreationOrder).ToArray();
        private bool IsHaulingSource(Thing thing) => simulation.IsComplete(thing) &&
            ((thing.TryGetComponent<GatheringComponent>(out var gathering) && gathering.Outstanding > 0) ||
                (thing.TryGetComponent<StorageComponent>(out var storage) && !storage.AllowsOrganic)) &&
            Stocks(thing).Any(t => Quantity(t) - OutgoingReserved(t.Id) > 0);
        private bool Accepts(Thing thing) => thing != null && simulation.IsComplete(thing) &&
            thing.TryGetComponent<StorageComponent>(out var storage) && storage.AllowsOrganic;
        private bool IsUnfinished(Thing thing) => thing != null && thing.Exists && thing.IsPlaced &&
            thing.TryGetComponent<ConstructionComponent>(out var state) && !state.IsComplete;
        private bool CanChoose(Thing worker) => worker != null && worker.IsPlaced &&
            worker.TryGetComponent<WorkerComponent>(out var state) && state.Job == null && ReservationFor(worker.Id) == null;
        private int StorageFree(Thing destination, TransportReservation excluding = null) => !Accepts(destination) ? 0 :
            Math.Max(0, ((FacilityDefinition)destination.Definition).StorageCapacity - simulation.TotalAt(destination.Id) -
                reservations.Where(r => r.DestinationId == destination.Id && !ReferenceEquals(r, excluding)).Sum(r => r.Amount));
        private int ConstructionFree(Thing target, TransportReservation excluding = null) => !IsUnfinished(target) ? 0 :
            Math.Max(0, ((FacilityDefinition)target.Definition).BuildCost - target.GetComponent<ConstructionComponent>().Delivered -
                reservations.Where(r => r.DestinationId == target.Id && !ReferenceEquals(r, excluding)).Sum(r => r.Amount));
        private TransportReservation ReservationFor(Guid worker) => reservations.FirstOrDefault(r => r.WorkerId == worker);

        private void NormalizeStock(Thing holder)
        {
            // Preserve any stack ID already named by an outgoing reservation. Newly
            // admitted/unreserved chunks can merge into that canonical stack safely.
            if (holder.Definition.Kind == ThingKind.Creature) return;
            var stocks = Stocks(holder).ToArray();
            var destination = stocks.FirstOrDefault(t => OutgoingReserved(t.Id) > 0) ?? stocks.FirstOrDefault();
            if (destination == null) return;
            foreach (var source in stocks)
                if (source.Id != destination.Id && OutgoingReserved(source.Id) == 0)
                    simulation.Registry.MergeResources(destination, source);
        }

        private static bool IsTransport(WorkerJob job) => job != null &&
            (job.Kind == WorkKind.Hauling || job.ResourceId.HasValue || job.Amount > 0);
        private static int Quantity(Thing resource) => resource.GetComponent<ResourceStackComponent>().Quantity;
        private static bool HeldBy(Thing resource, Thing holder) => resource != null && resource.Exists && resource.HolderId == holder.Id;
        private Thing Require(Guid id) => simulation.Registry.TryGet(id, out var thing) ? thing : throw Invalid("Missing Thing " + id);
        private static void Clear(WorkerComponent state, string reason)
        { state.Job = null; state.MoveProgress = 0; state.WaitReason = reason; }
        private static InvalidDataException Invalid(string message) => new InvalidDataException(message);
    }
}
