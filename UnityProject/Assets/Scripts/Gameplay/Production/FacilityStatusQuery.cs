using System;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public enum FacilityStatusKind
    {
        Storage, ConstructionMaterials, ConstructionWorking, ConstructionWaiting,
        GatheringWorking, GatheringWaiting, ProductionWorking, ProductionAutomatic,
        MaterialWaiting, OutputWaiting, Unreachable, TargetRequired
    }

    public enum FacilityMaterialWaitKind { None, DeliveryPending, InsufficientTotal, ReservedElsewhere, SupplyPending }

    public readonly struct FacilityMaterials
    {
        internal FacilityMaterials(ResourceData resource, int required, int delivered, int incoming,
            int inTransit, int totalStock, int reservedElsewhere, bool consumed, bool requested, bool deliveredWasConsumed)
        {
            Resource = resource; Required = required; Delivered = delivered; Incoming = incoming;
            InTransit = inTransit; TotalStock = totalStock; ReservedElsewhere = reservedElsewhere;
            Uncovered = consumed || !requested ? 0 : Math.Max(0, required - delivered - incoming);
            // TotalStock is physical stock across all holders, including incoming material.
            // It proves a global shortage, but never promises reachability or worker availability.
            var outsideTarget = Math.Max(0, totalStock - (consumed || deliveredWasConsumed ? 0 : delivered));
            WaitKind = consumed || !requested || delivered >= required ? FacilityMaterialWaitKind.None
                : Uncovered == 0 ? FacilityMaterialWaitKind.DeliveryPending
                : outsideTarget - incoming < Uncovered ? FacilityMaterialWaitKind.InsufficientTotal
                : outsideTarget - incoming - reservedElsewhere < Uncovered ? FacilityMaterialWaitKind.ReservedElsewhere
                : FacilityMaterialWaitKind.SupplyPending;
        }

        public ResourceData Resource { get; }
        public int Required { get; }
        public int Delivered { get; }
        public int Incoming { get; }
        public int InTransit { get; }
        public int TotalStock { get; }
        public int ReservedElsewhere { get; }
        public int Uncovered { get; }
        public FacilityMaterialWaitKind WaitKind { get; }
    }

    // A disposable observation. Components, reservations and definitions remain the only state owners.
    public readonly struct FacilityStatus
    {
        internal FacilityStatus(Thing facility, FacilityStatusKind kind, bool hasProgress,
            double progress, double duration, int outstanding, int working, int assigned,
            double rate, FacilityMaterials materials, bool consumed = false,
            CultivationTarget current = CultivationTarget.None, CultivationTarget next = CultivationTarget.None)
        {
            FacilityId = facility.Id; Role = ((FacilityData)facility.Definition).Role; Kind = kind;
            HasProgress = hasProgress; Progress = progress; Duration = duration; Outstanding = outstanding;
            WorkingCount = working; AssignedCount = assigned; Rate = rate; Materials = materials;
            InputsConsumed = consumed; CurrentTarget = current; NextTarget = next;
        }

        public Guid FacilityId { get; }
        public FacilityRole Role { get; }
        public FacilityStatusKind Kind { get; }
        public bool HasProgress { get; }
        public double Progress { get; }
        public double Duration { get; }
        public double ProgressFraction => !HasProgress || Duration <= 0 ? 0
            : Outstanding > 0 ? 1 : Math.Max(0, Math.Min(1, Progress / Duration));
        public int Outstanding { get; }
        public int WorkingCount { get; }
        public int AssignedCount { get; }
        // Local effective production speed; global session pause is deliberately a separate UI signal.
        public double Rate { get; }
        public bool IsAutomatic => Kind == FacilityStatusKind.ProductionAutomatic;
        public FacilityMaterials Materials { get; }
        public bool InputsConsumed { get; }
        public CultivationTarget CurrentTarget { get; }
        public CultivationTarget NextTarget { get; }
    }

    public sealed class FacilityStatusQuery
    {
        private readonly LaboratoryManager laboratory;
        public FacilityStatusQuery(LaboratoryManager laboratory)
        { this.laboratory = laboratory ?? throw new ArgumentNullException(nameof(laboratory)); }

        public bool TryRead(Guid facilityId, out FacilityStatus status)
        {
            if (laboratory.Registry.TryGet(facilityId, out var facility) && CanRead(facility))
            { status = Read(facility); return true; }
            status = default; return false;
        }

        public FacilityStatus Read(Thing facility)
        {
            if (!CanRead(facility)) throw new ArgumentException("Select an existing placed laboratory facility.", nameof(facility));
            var definition = (FacilityData)facility.Definition;
            var construction = facility.GetComponent<ConstructionComponent>();
            var assigned = AssignedCount(facility.Id);
            if (!construction.IsComplete)
            {
                var working = WorkingCount(facility.Id, WorkKind.Construction);
                var materials = Materials(facility, laboratory.Organic, definition.BuildCost,
                    construction.Delivered, false, true, true);
                var kind = !laboratory.Production.Accessible(facility) ? FacilityStatusKind.Unreachable
                    : construction.Delivered < definition.BuildCost ? FacilityStatusKind.ConstructionMaterials
                    : working > 0 ? FacilityStatusKind.ConstructionWorking : FacilityStatusKind.ConstructionWaiting;
                return new FacilityStatus(facility, kind, true, construction.Progress, definition.BuildSeconds,
                    0, working, assigned, kind == FacilityStatusKind.ConstructionWorking ? 1 : 0, materials);
            }
            if (facility.TryGetComponent<GatheringComponent>(out var gathering))
            {
                var working = WorkingCount(facility.Id, WorkKind.Gathering);
                var kind = gathering.Outstanding > 0 ? FacilityStatusKind.OutputWaiting
                    : !laboratory.Production.Accessible(facility) ? FacilityStatusKind.Unreachable
                    : working > 0 ? FacilityStatusKind.GatheringWorking : FacilityStatusKind.GatheringWaiting;
                return new FacilityStatus(facility, kind, true, gathering.Progress, definition.GatheringSeconds,
                    gathering.Outstanding, working, assigned, kind == FacilityStatusKind.GatheringWorking ? 1 : 0, default);
            }
            if (facility.TryGetComponent<ProductionComponent>(out var production))
            {
                var working = WorkingCount(facility.Id, WorkKind.Production);
                var needsTarget = !production.InProgress && definition.Role == FacilityRole.Cultivation
                    && production.SelectedTarget == CultivationTarget.None;
                var inputs = laboratory.Production.InputCount(facility);
                var materials = Materials(facility, definition.InputResource, definition.InputAmount, inputs,
                    production.InProgress, !needsTarget && production.Outstanding == 0, false);
                // Same precedence as ProductionManager.WaitReason, with typed presentation values.
                var kind = production.Outstanding > 0 ? FacilityStatusKind.OutputWaiting
                    : needsTarget ? FacilityStatusKind.TargetRequired
                    : !laboratory.Production.Accessible(facility) ? FacilityStatusKind.Unreachable
                    : !production.InProgress && inputs < definition.InputAmount ? FacilityStatusKind.MaterialWaiting
                    : working > 0 ? FacilityStatusKind.ProductionWorking : FacilityStatusKind.ProductionAutomatic;
                var rate = kind == FacilityStatusKind.ProductionWorking ? laboratory.Settings.StaffedRate
                    : kind == FacilityStatusKind.ProductionAutomatic ? laboratory.Settings.UnattendedRate : 0;
                return new FacilityStatus(facility, kind, true, production.Progress, definition.ProductionSeconds,
                    production.Outstanding, working, assigned, rate, materials, production.InProgress,
                    production.InProgress ? production.ActiveTarget : CultivationTarget.None, production.SelectedTarget);
            }
            return new FacilityStatus(facility, FacilityStatusKind.Storage, false, 0, 0, 0, 0, assigned, 0, default);
        }

        private bool CanRead(Thing facility) => facility != null && facility.Exists && facility.IsPlaced
            && ReferenceEquals(facility.Registry, laboratory.Registry) && facility.Definition is FacilityData
            && facility.TryGetComponent<ConstructionComponent>(out _);

        private int AssignedCount(Guid target)
        {
            var count = 0;
            foreach (var thing in laboratory.Registry.Things)
                if (thing.TryGetComponent<WorkerComponent>(out var worker) && worker.AssignedFacilityId == target) count++;
            return count;
        }

        private int WorkingCount(Guid target, WorkKind kind)
        {
            var count = 0;
            foreach (var thing in laboratory.Registry.Things)
                if (thing.IsPlaced && thing.TryGetComponent<WorkerComponent>(out var worker)
                    && worker.Job != null && worker.Job.TargetId == target
                    && worker.Job.Kind == kind && worker.Job.Stage == JobStage.Working) count++;
            return count;
        }

        private FacilityMaterials Materials(Thing target, ResourceData resource, int required, int delivered,
            bool consumed, bool requested, bool construction)
        {
            var total = 0; var incoming = 0; var transit = 0; var elsewhere = 0;
            foreach (var thing in laboratory.Registry.Things)
                if (thing.Definition == resource) total += thing.GetComponent<ResourceStackComponent>().Quantity;
            foreach (var reservation in laboratory.Reservations)
            {
                if (!laboratory.Registry.TryGet(reservation.ResourceId, out var stock) || stock.Definition != resource) continue;
                if (reservation.DestinationId == target.Id)
                {
                    incoming += reservation.Amount;
                    if (reservation.PickedUp) transit += reservation.Amount;
                }
                else elsewhere += reservation.Amount;
            }
            // Construction.Delivered has already been consumed, unlike a production input buffer.
            return new FacilityMaterials(resource, required, delivered, incoming, transit,
                total, elsewhere, consumed, requested, construction);
        }
    }
}
