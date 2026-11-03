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
            controls.Text("SelectedPosition", selected.Placement == null ? L("outside_detail") : L("position", A("position", selected.Placement.Origin.ToString())));
            if (selected.TryGetComponent<ResourceContainerComponent>(out var container))
                foreach (var group in container.Contents.GroupBy(t => t.Definition))
                    controls.Text("Stored_" + group.Key.DefinitionId, L("stock", A("resource", DefinitionName(group.Key)), A("count", group.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity))));
            if (selected.IsPlaced && selected.TryGetComponent<ConstructionComponent>(out var construction))
                AddFacilityDetails(selected, construction, StatusFor(selected), canOperate);
            if (selected.TryGetComponent<StorageComponent>(out var storageState)) AddStorageDetails(selected, storageState, canOperate);
            if (selected.TryGetComponent<WorkerComponent>(out var workerState)) AddWorkerDetails(selected, workerState, canOperate);
            if (selected.TryGetComponent<InvaderComponent>(out var invaderState))
            {
                controls.Text("InvaderState", invaderState.IsExternal
                    ? L("invader_active", A("count", invaderState.CompletedActivities))
                    : L("waiting_release_detail"));
                controls.Text("InvaderHint", L("invader_hint"), 16);
                UiAction("SelectedInvasion", L("view_invasion"), () => OpenPanel(LaboratoryPanel.Invasion), canOperate, id);
            }
            AddTransportLinks(id, canOperate);
        }

        private void AddFacilityDetails(Thing selected, ConstructionComponent construction, FacilityStatus status, bool canOperate)
        {
            var id = selected.Id;
            var definition = (FacilityData)selected.Definition;
            controls.Text("ConstructionState", construction.IsComplete ? L("construction_complete")
                : L("construction_progress", A("progress", ProgressMessage(status))));
            UiAction("Move", L("move"), () => BeginPlacement(), construction.IsComplete && canOperate, id);
            if (!construction.IsComplete)
            {
                controls.Text("ConstructionReason", FormatMessage(LaboratoryStatusText.Short(status)));
                AddMaterialDetails(status, true);
                controls.Text("ConstructionHint", L("construction_hint"), 16);
            }
            if (selected.TryGetComponent<GatheringComponent>(out _) && construction.IsComplete)
            {
                controls.Text("GatheringState", L("work_progress", A("state", LaboratoryStatusText.Short(status)), A("progress", ProgressMessage(status))));
                controls.Text("GatheringWorkers", L("gatherers", A("count", status.WorkingCount)));
            }
            if (selected.TryGetComponent<ProductionComponent>(out _))
            {
                if (construction.IsComplete)
                {
                    controls.Text("ProductionState", L("current_progress", A("progress", ProgressMessage(status))));
                    controls.Text("ProductionReason", status.Rate > 0 ? L("production_rate", A("state", LaboratoryStatusText.Short(status)), A("rate", status.Rate * 100)) : FormatMessage(LaboratoryStatusText.Short(status)));
                    controls.Text("ProductionWorkers", L("production_workers", A("assigned", status.AssignedCount), A("working", status.WorkingCount)));
                    AddMaterialDetails(status, false);
                }
                if (definition.Role == FacilityRole.Cultivation)
                {
                    controls.Text("CultivationState", L("cultivation_state", A("current", status.InputsConsumed ? LaboratoryStatusText.Target(definition.FindOutput(status.CurrentTarget)) : M("no_cycle")), A("next", LaboratoryStatusText.Target(definition.FindOutput(status.NextTarget)))));
                    controls.Text("CultivationHint", L("cultivation_hint"), 16);
                    foreach (var model in definition.CultivationOutputs)
                    {
                        var modelId = model.DefinitionId;
                        var action = model == definition.SupportOutput ? "CultivateSupport" : model == definition.InvaderOutput ? "CultivateInvader" : "Cultivate_" + modelId;
                        UiAction(action, L(status.NextTarget == modelId ? "cultivate_selected" : "cultivate",
                            A("target", DefinitionName(model)), A("resource", DefinitionName(culture)), A("count", definition.InputAmount)),
                            () => SelectCultivation(id, modelId), canOperate && construction.IsComplete, id);
                    }
                    controls.Text("RaiderHint", L("raider_hint"), 16);
                    if (!construction.IsComplete) controls.Text("CultivationUnavailable", L("cultivation_unavailable"), 16);
                }
                UiAction("ManageFacilityWorkers", L("manage_workers"), () => OpenPanel(LaboratoryPanel.Creatures), canOperate, id);
                foreach (var worker in Simulation.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out var component) && component.AssignedFacilityId == id).OrderBy(t => t.CreationOrder))
                    RelatedThing("AssignedWorker_" + worker.Id.ToString("N"), M("assigned_creature"), worker.Id, id, canOperate);
            }
            if (status.Outstanding > 0)
            {
                controls.Text("Outstanding", L("outstanding", A("count", status.Outstanding)));
                controls.Text("AdmissionHint", L("admission_hint"), 16);
            }
            if (status.Kind == FacilityStatusKind.Unreachable)
                controls.Text("AccessHint", L("access_hint"), 16);
            if (status.Kind == FacilityStatusKind.ConstructionMaterials || status.Kind == FacilityStatusKind.MaterialWaiting
                || status.Kind == FacilityStatusKind.OutputWaiting) AddStorageLinks(id, canOperate);
            if (status.Kind == FacilityStatusKind.GatheringWaiting || status.Kind == FacilityStatusKind.ConstructionWaiting)
                UiAction("InspectAvailableWorkers", L("inspect_workers"), () => OpenPanel(LaboratoryPanel.Creatures), canOperate, id);
        }

        private void AddMaterialDetails(FacilityStatus status, bool construction)
        {
            var materials = status.Materials;
            if (materials.Resource == null) return;
            if (status.InputsConsumed)
            {
                controls.Text("MaterialState", L("inputs_consumed", A("resource", DefinitionName(materials.Resource)), A("count", materials.Required)));
                return;
            }
            if (status.Outstanding > 0) return;
            if (status.Kind == FacilityStatusKind.TargetRequired)
            {
                controls.Text("MaterialState", L("choose_material_target"));
                return;
            }
            controls.Text("MaterialState", L("materials", A("resource", DefinitionName(materials.Resource)), A("action", M(construction ? "material_inserted" : "material_arrived")), A("delivered", materials.Delivered), A("required", materials.Required), A("incoming", materials.Incoming), A("transit", materials.InTransit), A("uncovered", materials.Uncovered)));
            controls.Text("MaterialReason", L("material_stock", A("reason", LaboratoryStatusText.MaterialReason(materials.WaitKind)), A("total", materials.TotalStock), A("reserved", materials.ReservedElsewhere)), 16);
        }

        private void AddStorageDetails(Thing selected, StorageComponent storageState, bool canOperate)
        {
            var id = selected.Id;
            var capacity = ((FacilityData)selected.Definition).StorageCapacity;
            var occupied = Simulation.TotalAt(id);
            var incoming = Simulation.Reservations.Where(r => r.DestinationId == id).Sum(r => r.Amount);
            var outgoing = Simulation.Reservations.Where(r => r.SourceId == id && !r.PickedUp).Sum(r => r.Amount);
            controls.Text("StorageState", L("storage_state", A("stock", occupied), A("capacity", capacity), A("incoming", incoming), A("outgoing", outgoing), A("free", Math.Max(0, capacity - occupied - incoming))));
            UiAction("AllowOrganic", L(storageState.AllowsOrganic ? "resource_allowed" : "resource_blocked", A("resource", DefinitionName(organic))), () => SetAllowed(id, organic, !storageState.AllowsOrganic), canOperate, id);
            UiAction("AllowCulture", L(storageState.AllowsCulture ? "resource_allowed" : "resource_blocked", A("resource", DefinitionName(culture))), () => SetAllowed(id, culture, !storageState.AllowsCulture), canOperate, id);
            controls.Text("StorageHint", L("storage_hint"), 16);
            foreach (var resource in new[] { organic, culture })
            {
                if (resource == null || (resource == organic ? storageState.AllowsOrganic : storageState.AllowsCulture)) continue;
                var held = selected.GetComponent<ResourceContainerComponent>().Contents.Where(t => t.Definition == resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
                if (held == 0) continue;
                var reserved = Simulation.Reservations.Where(r => r.SourceId == id && !r.PickedUp
                    && Simulation.Registry.TryGet(r.ResourceId, out var stock) && stock.Definition == resource).Sum(r => r.Amount);
                controls.Text("Relocating_" + resource.DefinitionId, L("relocating", A("resource", DefinitionName(resource)), A("reserved", reserved), A("held", Math.Max(0, held - reserved))), 16);
            }
            if (!storageState.AllowsOrganic || !storageState.AllowsCulture) AddStorageLinks(id, canOperate);
        }

        private void AddWorkerDetails(Thing selected, WorkerComponent workerState, bool canOperate)
        {
            var id = selected.Id;
            var job = workerState.Job;
            controls.Text("WorkerJob", job == null ? L("worker_idle") : L("worker_job", A("kind", WorkKindText(job.Kind)), A("stage", JobStageText(job.Stage)), A("target", ThingMessage(job.TargetId))));
            if (workerState.WaitReason != WorkerWaitReason.None) controls.Text("WorkerWait", L("worker_wait", A("reason", WorkerWaitText(workerState.WaitReason))));
            if (job != null) RelatedThing("WorkerTarget", M("current_target"), job.TargetId, id, canOperate);
            if (((CreatureData)selected.Definition).CanOperate)
            {
                controls.Text("Assigned", L("assigned", A("facility", workerState.AssignedFacilityId.HasValue ? ThingMessage(workerState.AssignedFacilityId.Value) : M("unassigned"))));
                controls.Text("AssignmentHint", L("assignment_hint"), 16);
                if (workerState.AssignedFacilityId.HasValue)
                    RelatedThing("AssignedFacility", M("view_assigned"), workerState.AssignedFacilityId.Value, id, canOperate);
                UiAction("Unassign", L("unassign"), () => AssignWorker(id, null), canOperate, id);
                var facilities = Simulation.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _) && t.GetComponent<ConstructionComponent>().IsComplete).OrderBy(t => t.CreationOrder).ToArray();
                if (facilities.Length == 0) controls.Text("NoAssignableFacility", L("no_assignable"), 16);
                foreach (var facility in facilities)
                {
                    var facilityId = facility.Id;
                    UiAction("Assign_" + facilityId.ToString("N"), L(workerState.AssignedFacilityId == facilityId ? "assign_current" : "assign", A("facility", ThingMessage(facilityId))), () => AssignWorker(id, facilityId), canOperate, id);
                }
            }
            else controls.Text("AssignmentUnavailable", L("prototype_assignment"), 16);
            controls.Text("WorkerPriorities", workerState.UsesPreset ? L("using_preset") : L("individual_priority"));
            AddPriorities("WorkerPriority", workerState.UsesPreset ? Simulation.Preset.ToArray() : workerState.Priorities.ToArray(), values => SetWorkerPriorities(id, values), canOperate, id);
            if (!workerState.UsesPreset) UiAction("UsePreset", L("use_preset"), () => SetWorkerPriorities(id, Simulation.Preset.ToArray(), true), canOperate, id);
        }

        private void AddTransportLinks(Guid owner, bool enabled)
        {
            foreach (var reservation in Simulation.Reservations.Where(r => r.WorkerId == owner || r.SourceId == owner || r.DestinationId == owner || r.BatchOriginId == owner))
            {
                var key = "Transport_" + reservation.WorkerId.ToString("N") + "_" + reservation.ResourceId.ToString("N");
                controls.Text(key, L("transport", A("state", M(reservation.PickedUp ? "in_transit" : "transport_reserved")), A("resource", ResourceMessage(reservation.ResourceId)), A("count", reservation.Amount), A("source", ThingMessage(reservation.SourceId)), A("destination", ThingMessage(reservation.DestinationId))), 16);
                if (reservation.WorkerId != owner) RelatedThing(key + "_Worker", M("carrier"), reservation.WorkerId, owner, enabled);
                if (reservation.SourceId != owner && reservation.SourceId != reservation.WorkerId)
                    RelatedThing(key + "_Source", M("source"), reservation.SourceId, owner, enabled);
                if (reservation.DestinationId != owner) RelatedThing(key + "_Destination", M("destination"), reservation.DestinationId, owner, enabled);
            }
        }

        private void RelatedThing(string key, LocalizedMessage caption, Guid targetId, Guid owner, bool enabled)
        {
            if (!Simulation.Registry.TryGet(targetId, out var target) || !target.IsPlaced || target.Definition.Kind == ThingKind.Resource) return;
            // A moving job or rerouted reservation changes the row identity, invalidating any old press.
            UiAction(key + "_" + targetId.ToString("N"), L("related_thing", A("caption", caption), A("thing", ThingMessage(targetId))), () => { if (SelectThing(targetId)) ExpandSelection(); }, enabled, owner);
        }

        private void AddStorageLinks(Guid owner, bool enabled)
        {
            var stores = Simulation.Registry.Things.Where(t => t.Id != owner && t.IsPlaced && t.TryGetComponent<StorageComponent>(out _)
                && Simulation.IsComplete(t)).OrderBy(t => t.CreationOrder).ToArray();
            if (stores.Length == 0)
            {
                controls.Text("NoRelatedStorage", L("no_related_storage"), 16);
                UiAction("BuildForStorage", L("view_build"), () => OpenPanel(LaboratoryPanel.Build), enabled, owner);
                return;
            }
            controls.Text("RelatedStorageHint", L("related_storage_hint"), 16);
            foreach (var store in stores) RelatedThing("InspectStorage", M("inspect_storage"), store.Id, owner, enabled);
        }

        private void AddPriorities(string key, int[] values, Action<int[]> apply, bool enabled, Guid? selected = null)
        {
            var names = new[] { M("work_construction"), M("work_gathering"), M("work_hauling"), M("work_production") };
            controls.Text(key + "_Hint", L("priority_hint"), 16);
            for (var index = 0; index < values.Length && index < names.Length; index++)
            {
                var slot = index;
                UiAction(key + "_" + slot, L(values[slot] == 0 ? "priority_off" : "priority", A("work", names[slot]), A("value", values[slot])), () =>
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

        private static LocalizedMessage ProgressMessage(FacilityStatus status) => M("progress", A("percent", status.ProgressFraction * 100), A("work", status.Progress), A("duration", status.Duration));
        private static LocalizedMessage WorkKindText(WorkKind kind) => kind == WorkKind.Construction ? M("work_construction") : kind == WorkKind.Gathering ? M("work_gathering") : kind == WorkKind.Hauling ? M("work_hauling") : M("work_production");
        private static LocalizedMessage JobStageText(JobStage stage) => stage == JobStage.ToSupply ? M("to_supply")
            : stage == JobStage.ToDestination ? M("to_destination") : stage == JobStage.Working ? M("job_working") : M("job_idle");
        private static LocalizedMessage WorkerWaitText(WorkerWaitReason reason)
        {
            switch (reason)
            {
                case WorkerWaitReason.TargetUnavailable: return M("wait_unavailable");
                case WorkerWaitReason.TargetUnreachable:
                case WorkerWaitReason.PathBlocked: return M("wait_path");
                case WorkerWaitReason.PrioritiesDisabled: return M("wait_disabled");
                case WorkerWaitReason.NoWork: return M("wait_no_work");
                case WorkerWaitReason.FacilityClaimed: return M("wait_claimed");
                case WorkerWaitReason.OutputAdmission: return M("wait_admission");
                case WorkerWaitReason.NoStorage: return M("wait_storage");
                case WorkerWaitReason.TransportChanged: return M("wait_transport");
                default: return M("wait_next");
            }
        }
        private string ThingLabel(Guid id) => FormatMessage(ThingMessage(id));
        private LocalizedMessage ResourceMessage(Guid id) => Simulation != null && Simulation.Registry.TryGet(id, out var resource) ? DefinitionName(resource.Definition) : M("resource");
        private void InvalidateScreenInput() => screen?.InvalidatePointers();
        private void SetScreenVisible(bool visible) => screen?.SetVisible(visible);
        private void DisposeScreen() { screen?.Dispose(); screen = null; MapViewport = null; }
    }
}
