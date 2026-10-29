using MobileGame.Foundation;
using MobileGame.Gameplay;

namespace MobileGame.Presentation
{
    public static class LaboratoryStatusText
    {
        public static string Short(FacilityStatus state)
        {
            switch (state.Kind)
            {
                case FacilityStatusKind.ConstructionMaterials: return "건설 · 자재 대기";
                case FacilityStatusKind.ConstructionWorking: return "시공 중";
                case FacilityStatusKind.ConstructionWaiting: return "시공 작업자 대기";
                case FacilityStatusKind.GatheringWorking: return "채집 중";
                case FacilityStatusKind.GatheringWaiting: return "채집 작업자 대기";
                case FacilityStatusKind.ProductionWorking: return "작업 중";
                case FacilityStatusKind.ProductionAutomatic: return "자동";
                case FacilityStatusKind.MaterialWaiting: return "재료 대기";
                case FacilityStatusKind.OutputWaiting: return "입고 대기";
                case FacilityStatusKind.Unreachable: return "접근 불가";
                case FacilityStatusKind.TargetRequired: return "배양 대상 선택";
                default: return "저장소";
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
        public static string Target(CultivationTarget target) => target == CultivationTarget.Support ? "보급형"
            : target == CultivationTarget.Invader ? "침공형" : "미선택";
        public static string MaterialReason(FacilityMaterialWaitKind kind)
        {
            switch (kind)
            {
                case FacilityMaterialWaitKind.DeliveryPending: return "운반 예약됨 · 도착 대기";
                case FacilityMaterialWaitKind.InsufficientTotal: return "전체 자재 부족";
                case FacilityMaterialWaitKind.ReservedElsewhere: return "다른 작업이 자재를 예약 중";
                case FacilityMaterialWaitKind.SupplyPending: return "공급 작업·경로 확인 필요";
                default: return "자재 준비됨";
            }
        }
    }
}
