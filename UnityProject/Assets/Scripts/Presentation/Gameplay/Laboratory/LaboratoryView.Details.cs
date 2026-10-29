using System;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private void AddSelectedControls(Thing selected)
        {
            var id = selected.Id; var canOperate = CanInteract && !IsEditing;
            controls.Text("SelectedTitle", ThingLabel(id), 22);
            controls.Text("SelectedPosition", selected.Placement == null ? "연구소 밖에서 활동 중" : "위치 " + selected.Placement.Origin);
            if (selected.TryGetComponent<ResourceContainerComponent>(out var container))
                foreach (var group in container.Contents.GroupBy(t => t.Definition))
                    controls.Text("Stored_" + group.Key.DefinitionId, "보유 " + group.Key.DisplayName + " " + group.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity).ToString("N0") + "개");
            if (selected.IsPlaced && selected.TryGetComponent<ConstructionComponent>(out var construction))
                AddFacilityDetails(selected, construction, StatusFor(selected), canOperate);
            if (selected.TryGetComponent<StorageComponent>(out var storageState)) AddStorageDetails(selected, storageState, canOperate);
            if (selected.TryGetComponent<WorkerComponent>(out var workerState)) AddWorkerDetails(selected, workerState, canOperate);
            if (selected.TryGetComponent<InvaderComponent>(out var invaderState))
            {
                controls.Text("InvaderState", invaderState.IsExternal
                    ? "외부 활동 중 · 완료한 활동 " + invaderState.CompletedActivities.ToString("N0") + "회"
                    : "연구소에서 방출 대기 중");
                controls.Text("InvaderHint", "침공·보고에서 대기 중인 침공형을 전원 방출할 수 있습니다. 개별 방출·귀환 기능은 없습니다.", 16);
                UiAction("SelectedInvasion", "침공·보고 보기", () => OpenPanel(LaboratoryPanel.Invasion), canOperate, id);
            }
            AddTransportLinks(id, canOperate);
        }

        private void AddFacilityDetails(Thing selected, ConstructionComponent construction, FacilityStatus status, bool canOperate)
        {
            var id = selected.Id;
            var definition = (FacilityData)selected.Definition;
            controls.Text("ConstructionState", construction.IsComplete ? "시공 완료 · 시설 사용 가능"
                : "건설 중 · 시공 " + ProgressText(status) + "\n시공 완료 전에는 시설을 사용할 수 없습니다.");
            UiAction("Move", "시설 이동·90° 회전", () => BeginPlacement(), construction.IsComplete && canOperate, id);
            if (!construction.IsComplete)
            {
                controls.Text("ConstructionReason", LaboratoryStatusText.Short(status));
                AddMaterialDetails(status, true);
                controls.Text("ConstructionHint", "부지는 확정되었습니다. 자재를 운반·투입한 뒤 보조형이 시공합니다. 자재 부족은 건설 완료를 뜻하지 않습니다.", 16);
            }
            if (selected.TryGetComponent<GatheringComponent>(out _) && construction.IsComplete)
            {
                controls.Text("GatheringState", LaboratoryStatusText.Short(status) + " · " + ProgressText(status));
                controls.Text("GatheringWorkers", "실제 채집 작업자 " + status.WorkingCount.ToString("N0") + "명 · 작업자가 없으면 진행하지 않습니다.");
            }
            if (selected.TryGetComponent<ProductionComponent>(out _))
            {
                if (construction.IsComplete)
                {
                    controls.Text("ProductionState", "현재 회차 · " + ProgressText(status));
                    controls.Text("ProductionReason", LaboratoryStatusText.Short(status)
                        + (status.Rate > 0 ? " · 기준 속도의 " + (status.Rate * 100).ToString("0.#") + "%" : ""));
                    controls.Text("ProductionWorkers", "배정 " + status.AssignedCount.ToString("N0") + "명 / 실제 생산 작업 " + status.WorkingCount.ToString("N0") + "명");
                    AddMaterialDetails(status, false);
                }
                if (definition.Role == FacilityRole.Cultivation)
                {
                    controls.Text("CultivationState", "현재 회차: " + (status.InputsConsumed ? LaboratoryStatusText.Target(status.CurrentTarget) : "진행 중인 회차 없음")
                        + "\n다음 회차: " + LaboratoryStatusText.Target(status.NextTarget));
                    controls.Text("CultivationHint", "진행 중 선택을 바꾸면 다음 회차부터 적용됩니다. 완료 개체는 작업 위치에 자동으로 나타납니다.", 16);
                    UiAction("CultivateSupport", "보급형 배양" + (status.NextTarget == CultivationTarget.Support ? " · 선택됨" : "")
                        + " (" + definition.InputAmount.ToString("N0") + " 배양액)", () => SelectCultivation(id, CultivationTarget.Support), canOperate && construction.IsComplete, id);
                    UiAction("CultivateInvader", "침공형 배양" + (status.NextTarget == CultivationTarget.Invader ? " · 선택됨" : "")
                        + " (" + definition.InputAmount.ToString("N0") + " 배양액)", () => SelectCultivation(id, CultivationTarget.Invader), canOperate && construction.IsComplete, id);
                    if (!construction.IsComplete) controls.Text("CultivationUnavailable", "시공 완료 후 배양 대상을 선택할 수 있습니다.", 16);
                }
                UiAction("ManageFacilityWorkers", "생명체 목록에서 작업자 배정", () => OpenPanel(LaboratoryPanel.Creatures), canOperate, id);
                foreach (var worker in Simulation.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out var component) && component.AssignedFacilityId == id).OrderBy(t => t.CreationOrder))
                    RelatedThing("AssignedWorker_" + worker.Id.ToString("N"), "배정 개체", worker.Id, id, canOperate);
            }
            if (status.Outstanding > 0)
            {
                controls.Text("Outstanding", "실제 입고 대기 " + status.Outstanding.ToString("N0") + "개 · 운반 중인 완료품 포함");
                controls.Text("AdmissionHint", "완료품이 전량 저장소에 들어가면 다음 회차가 시작됩니다. 운반자가 집어 든 것만으로 완료되지 않습니다.", 16);
            }
            if (status.Kind == FacilityStatusKind.Unreachable)
                controls.Text("AccessHint", "보조형이 정면 작업 위치에 접근할 수 없습니다. 시설 이동·회전으로 경로를 확인하세요. 기존 진행과 투입 상태는 보존됩니다.", 16);
            if (status.Kind == FacilityStatusKind.ConstructionMaterials || status.Kind == FacilityStatusKind.MaterialWaiting
                || status.Kind == FacilityStatusKind.OutputWaiting) AddStorageLinks(id, canOperate);
            if (status.Kind == FacilityStatusKind.GatheringWaiting || status.Kind == FacilityStatusKind.ConstructionWaiting)
                UiAction("InspectAvailableWorkers", "생명체 목록에서 작업·우선순위 확인", () => OpenPanel(LaboratoryPanel.Creatures), canOperate, id);
        }

        private void AddMaterialDetails(FacilityStatus status, bool construction)
        {
            var materials = status.Materials;
            if (materials.Resource == null) return;
            if (status.InputsConsumed)
            {
                controls.Text("MaterialState", "현재 회차 재료 " + materials.Resource.DisplayName + " " + materials.Required.ToString("N0") + "개 투입 완료 · 다시 소비하지 않습니다.");
                return;
            }
            if (status.Outstanding > 0) return;
            if (status.Kind == FacilityStatusKind.TargetRequired)
            {
                controls.Text("MaterialState", "배양 대상을 선택하면 현재 한 회차분의 재료를 공급합니다.");
                return;
            }
            controls.Text("MaterialState", materials.Resource.DisplayName + " " + (construction ? "투입" : "도착") + " " + materials.Delivered.ToString("N0")
                + " / 필요 " + materials.Required.ToString("N0") + "개\n공급 예정 " + materials.Incoming.ToString("N0")
                + "개 (운반 중 " + materials.InTransit.ToString("N0") + "개 포함) · 미확보 " + materials.Uncovered.ToString("N0") + "개");
            controls.Text("MaterialReason", LaboratoryStatusText.MaterialReason(materials.WaitKind)
                + "\n전체 재고 " + materials.TotalStock.ToString("N0") + "개 · 다른 작업 예약 " + materials.ReservedElsewhere.ToString("N0") + "개", 16);
        }

        private void AddStorageDetails(Thing selected, StorageComponent storageState, bool canOperate)
        {
            var id = selected.Id;
            var capacity = ((FacilityData)selected.Definition).StorageCapacity;
            var occupied = Simulation.TotalAt(id);
            var incoming = Simulation.Reservations.Where(r => r.DestinationId == id).Sum(r => r.Amount);
            var outgoing = Simulation.Reservations.Where(r => r.SourceId == id && !r.PickedUp).Sum(r => r.Amount);
            controls.Text("StorageState", "실제 재고 " + occupied.ToString("N0") + " / 용량 " + capacity.ToString("N0")
                + "\n입고 예정 " + incoming.ToString("N0") + " · 출고 예약 " + outgoing.ToString("N0")
                + "\n예약을 제외한 입고 여유 " + Math.Max(0, capacity - occupied - incoming).ToString("N0"));
            UiAction("AllowOrganic", "유기물: " + (storageState.AllowsOrganic ? "허용 중 → 해제" : "차단 중 → 허용"), () => SetAllowed(id, organic, !storageState.AllowsOrganic), canOperate, id);
            UiAction("AllowCulture", "배양액: " + (storageState.AllowsCulture ? "허용 중 → 해제" : "차단 중 → 허용"), () => SetAllowed(id, culture, !storageState.AllowsCulture), canOperate, id);
            controls.Text("StorageHint", "허용을 해제해도 재고는 없어지지 않습니다. 다른 허용 저장소로 실제 반출될 때까지 용량을 차지합니다.", 16);
            foreach (var resource in new[] { organic, culture })
            {
                if (resource == null || (resource == organic ? storageState.AllowsOrganic : storageState.AllowsCulture)) continue;
                var held = selected.GetComponent<ResourceContainerComponent>().Contents.Where(t => t.Definition == resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
                if (held == 0) continue;
                var reserved = Simulation.Reservations.Where(r => r.SourceId == id && !r.PickedUp
                    && Simulation.Registry.TryGet(r.ResourceId, out var stock) && stock.Definition == resource).Sum(r => r.Amount);
                controls.Text("Relocating_" + resource.DefinitionId, resource.DisplayName + " 이동 예약 " + reserved.ToString("N0")
                    + "개 · 이동 전 보관 " + Math.Max(0, held - reserved).ToString("N0") + "개\n운반할 곳이 없으면 보관을 유지합니다.", 16);
            }
            if (!storageState.AllowsOrganic || !storageState.AllowsCulture) AddStorageLinks(id, canOperate);
        }

        private void AddWorkerDetails(Thing selected, WorkerComponent workerState, bool canOperate)
        {
            var id = selected.Id;
            var job = workerState.Job;
            controls.Text("WorkerJob", job == null ? "현재 행동: 대기" : "현재 행동: " + WorkKindText(job.Kind) + " · " + JobStageText(job.Stage)
                + "\n작업 대상: " + ThingLabel(job.TargetId));
            if (!string.IsNullOrWhiteSpace(workerState.WaitReason)) controls.Text("WorkerWait", "대기 이유: " + WorkerWaitText(workerState.WaitReason));
            if (job != null) RelatedThing("WorkerTarget", "현재 작업 대상", job.TargetId, id, canOperate);
            if (((CreatureData)selected.Definition).CanOperate)
            {
                controls.Text("Assigned", "배정 시설: " + (workerState.AssignedFacilityId.HasValue ? ThingLabel(workerState.AssignedFacilityId.Value) : "미배정"));
                controls.Text("AssignmentHint", "배정 시설과 실제 행동은 다를 수 있습니다. 시설에서 작업할 수 없으면 배정을 유지한 채 다른 작업을 합니다. 정상 운반은 완료한 뒤 새 배정으로 전환합니다.", 16);
                if (workerState.AssignedFacilityId.HasValue)
                    RelatedThing("AssignedFacility", "배정 시설 보기", workerState.AssignedFacilityId.Value, id, canOperate);
                UiAction("Unassign", "시설 배정 해제", () => AssignWorker(id, null), canOperate, id);
                var facilities = Simulation.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _) && t.GetComponent<ConstructionComponent>().IsComplete).OrderBy(t => t.CreationOrder).ToArray();
                if (facilities.Length == 0) controls.Text("NoAssignableFacility", "시공을 마친 가공·배양시설이 있어야 배정할 수 있습니다.", 16);
                foreach (var facility in facilities)
                {
                    var facilityId = facility.Id;
                    UiAction("Assign_" + facilityId.ToString("N"), "배정: " + ThingLabel(facilityId)
                        + (workerState.AssignedFacilityId == facilityId ? " · 현재 배정" : ""), () => AssignWorker(id, facilityId), canOperate, id);
                }
            }
            else controls.Text("AssignmentUnavailable", "프로토타입은 채집·운반·건설을 수행하며 가공·배양 작업자로 배정할 수 없습니다.", 16);
            controls.Text("WorkerPriorities", workerState.UsesPreset ? "작업 우선순위: 기본 프리셋 사용 중" : "작업 우선순위: 이 개체의 개별 설정");
            AddPriorities("WorkerPriority", workerState.UsesPreset ? Simulation.Preset.ToArray() : workerState.Priorities.ToArray(), values => SetWorkerPriorities(id, values), canOperate, id);
            if (!workerState.UsesPreset) UiAction("UsePreset", "기본 프리셋으로 되돌리기", () => SetWorkerPriorities(id, Simulation.Preset.ToArray(), true), canOperate, id);
        }

        private void AddTransportLinks(Guid owner, bool enabled)
        {
            foreach (var reservation in Simulation.Reservations.Where(r => r.WorkerId == owner || r.SourceId == owner || r.DestinationId == owner || r.BatchOriginId == owner))
            {
                var key = "Transport_" + reservation.WorkerId.ToString("N") + "_" + reservation.ResourceId.ToString("N");
                controls.Text(key, (reservation.PickedUp ? "운반 중 " : "운반 예약 ") + ResourceLabel(reservation.ResourceId)
                    + " " + reservation.Amount.ToString("N0") + "개\n" + ThingLabel(reservation.SourceId) + " → " + ThingLabel(reservation.DestinationId), 16);
                if (reservation.WorkerId != owner) RelatedThing(key + "_Worker", "운반 개체", reservation.WorkerId, owner, enabled);
                if (reservation.SourceId != owner && reservation.SourceId != reservation.WorkerId)
                    RelatedThing(key + "_Source", "공급처", reservation.SourceId, owner, enabled);
                if (reservation.DestinationId != owner) RelatedThing(key + "_Destination", "목적지", reservation.DestinationId, owner, enabled);
            }
        }

        private void RelatedThing(string key, string caption, Guid targetId, Guid owner, bool enabled)
        {
            if (!Simulation.Registry.TryGet(targetId, out var target) || !target.IsPlaced || target.Definition.Kind == ThingKind.Resource) return;
            // A moving job or rerouted reservation changes the row identity, invalidating any old press.
            UiAction(key + "_" + targetId.ToString("N"), caption + ": " + ThingLabel(targetId), () => { if (SelectThing(targetId)) ExpandSelection(); }, enabled, owner);
        }

        private void AddStorageLinks(Guid owner, bool enabled)
        {
            var stores = Simulation.Registry.Things.Where(t => t.Id != owner && t.IsPlaced && t.TryGetComponent<StorageComponent>(out _)
                && Simulation.IsComplete(t)).OrderBy(t => t.CreationOrder).ToArray();
            if (stores.Length == 0)
            {
                controls.Text("NoRelatedStorage", "확인할 다른 완성 저장소가 없습니다. 건설 메뉴에서 저장소를 추가할 수 있습니다.", 16);
                UiAction("BuildForStorage", "건설 목록 보기", () => OpenPanel(LaboratoryPanel.Build), enabled, owner);
                return;
            }
            controls.Text("RelatedStorageHint", "저장소의 재고·빈 공간·허용 자원을 확인하세요. 경로와 운반 작업도 공급에 영향을 줍니다.", 16);
            foreach (var store in stores) RelatedThing("InspectStorage", "저장소 확인", store.Id, owner, enabled);
        }

        private void AddPriorities(string key, int[] values, Action<int[]> apply, bool enabled, Guid? selected = null)
        {
            var names = new[] { "건설", "채집", "운반", "생산" };
            controls.Text(key + "_Hint", "0은 끔 · 작은 숫자부터 선택 · 동률은 건설 → 채집 → 운반 → 생산\n누르면 0~4로 바뀝니다. 진행 중 작업의 전환 규칙은 유지됩니다.", 16);
            for (var index = 0; index < values.Length && index < names.Length; index++)
            {
                var slot = index;
                UiAction(key + "_" + slot, names[slot] + ": " + (values[slot] == 0 ? "끔" : "우선순위 " + values[slot]), () =>
                {
                    if (!CanInteract || IsEditing) return;
                    var changed = Simulation.Preset.ToArray();
                    if (selected.HasValue)
                    {
                        if (!Simulation.Registry.TryGet(selected.Value, out var thing) || !thing.TryGetComponent<WorkerComponent>(out var worker)) return;
                        if (!worker.UsesPreset) changed = worker.Priorities.ToArray();
                    }
                    changed[slot] = (changed[slot] + 1) % 5; apply(changed);
                }, enabled, selected);
            }
        }

        private static string ProgressText(FacilityStatus status) => (status.ProgressFraction * 100).ToString("0.#") + "% ("
            + status.Progress.ToString("0.#") + " / " + status.Duration.ToString("0.#") + "초 작업량)";
        private static string WorkKindText(WorkKind kind) => kind == WorkKind.Construction ? "건설" : kind == WorkKind.Gathering ? "채집" : kind == WorkKind.Hauling ? "운반" : "생산";
        private static string JobStageText(JobStage stage) => stage == JobStage.ToSupply ? "자원 받으러 이동"
            : stage == JobStage.ToDestination ? "목적지로 이동" : stage == JobStage.Working ? "작업 중" : "대기";
        private static string WorkerWaitText(string reason)
        {
            switch (reason)
            {
                case "Target is unavailable":
                case "Work target is unavailable": return "현재 작업 대상을 이용할 수 없습니다.";
                case "Work target is unreachable":
                case "Path is blocked": return "작업 대상까지 이동할 경로가 없습니다.";
                case "All work priorities are disabled": return "모든 작업 우선순위가 꺼져 있습니다.";
                case "No reachable work or valid destination": return "도달할 수 있는 작업 또는 유효한 운반 목적지가 없습니다.";
                case "Another worker is using this facility": return "다른 개체가 이 시설에서 작업하고 있습니다.";
                case "Completed output awaits storage": return "완료품이 저장소에 실제 입고되기를 기다립니다.";
                case "Holding resources; no reachable allowed storage space": return "자원을 보유 중이며 도달 가능한 허용 저장 공간이 없습니다.";
                case "Transport changed; selecting new work": return "운반 조건이 바뀌어 다음 작업을 선택하고 있습니다.";
                default: return "다음 작업을 기다립니다.";
            }
        }
        private string ThingLabel(Guid id) => Simulation != null && Simulation.Registry.TryGet(id, out var thing) ? thing.Definition.DisplayName + " #" + thing.CreationOrder : "대상 없음";
        private string ResourceLabel(Guid id) => Simulation != null && Simulation.Registry.TryGet(id, out var resource) ? resource.Definition.DisplayName : "자원";
        private void InvalidateScreenInput() => screen?.InvalidatePointers();
        private void SetScreenVisible(bool visible) => screen?.SetVisible(visible);
        private void DisposeScreen() { screen?.Dispose(); screen = null; MapViewport = null; }
    }
}
