using MobileGame.Foundation;
using MobileGame.Gameplay;

namespace MobileGame.Presentation
{
    public static class LaboratoryStatusText
    {
        public static LocalizedMessage Short(FacilityStatus state)
        {
            switch (state.Kind)
            {
                case FacilityStatusKind.ConstructionMaterials: return new LocalizedMessage("Status", "construction_materials");
                case FacilityStatusKind.ConstructionWorking: return new LocalizedMessage("Status", "construction_working");
                case FacilityStatusKind.ConstructionWaiting: return new LocalizedMessage("Status", "construction_waiting");
                case FacilityStatusKind.GatheringWorking: return new LocalizedMessage("Status", "gathering_working");
                case FacilityStatusKind.GatheringWaiting: return new LocalizedMessage("Status", "gathering_waiting");
                case FacilityStatusKind.ProductionWorking: return new LocalizedMessage("Status", "production_working");
                case FacilityStatusKind.ProductionAutomatic: return new LocalizedMessage("Status", "automatic");
                case FacilityStatusKind.MaterialWaiting: return new LocalizedMessage("Status", "material_waiting");
                case FacilityStatusKind.OutputWaiting: return new LocalizedMessage("Status", "output_waiting");
                case FacilityStatusKind.Unreachable: return new LocalizedMessage("Status", "unreachable");
                case FacilityStatusKind.TargetRequired: return new LocalizedMessage("Status", "target_required");
                default: return new LocalizedMessage("Status", "storage");
            }
        }
        public static LaboratoryIcon Icon(FacilityStatus state)
        {
            switch (state.Kind)
            {
                case FacilityStatusKind.ConstructionWorking:
                case FacilityStatusKind.ConstructionWaiting: return LaboratoryIcon.Construction;
                case FacilityStatusKind.ConstructionMaterials:
                case FacilityStatusKind.MaterialWaiting: return LaboratoryIcon.Material;
                case FacilityStatusKind.ProductionAutomatic: return LaboratoryIcon.Automatic;
                case FacilityStatusKind.OutputWaiting: return LaboratoryIcon.Delivery;
                case FacilityStatusKind.Unreachable: return LaboratoryIcon.Blocked;
                case FacilityStatusKind.TargetRequired: return LaboratoryIcon.SelectTarget;
                default: return LaboratoryIcon.Worker;
            }
        }
        public static LocalizedMessage Target(CultivationTarget target) => target == CultivationTarget.Support ? new LocalizedMessage("Status", "support")
            : target == CultivationTarget.Invader ? new LocalizedMessage("Status", "invader") : new LocalizedMessage("Status", "no_target");
        public static LocalizedMessage MaterialReason(FacilityMaterialWaitKind kind)
        {
            switch (kind)
            {
                case FacilityMaterialWaitKind.DeliveryPending: return new LocalizedMessage("Status", "delivery_pending");
                case FacilityMaterialWaitKind.InsufficientTotal: return new LocalizedMessage("Status", "insufficient");
                case FacilityMaterialWaitKind.ReservedElsewhere: return new LocalizedMessage("Status", "reserved_elsewhere");
                case FacilityMaterialWaitKind.SupplyPending: return new LocalizedMessage("Status", "supply_pending");
                default: return new LocalizedMessage("Status", "materials_ready");
            }
        }
    }
}
