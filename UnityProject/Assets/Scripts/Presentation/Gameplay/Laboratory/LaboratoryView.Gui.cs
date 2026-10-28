using System;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private Vector2 panelScroll;
        private LaboratoryUiCanvas screen;
        private RectTransform playRoot;
        private Text titleText;
        private Text hudText;
        private RectTransform worldGauge;
        private RectTransform worldGaugeFill;
        private Text editText;
        private RectTransform toolRoot;
        private ScrollRect controlsScroll;
        private LaboratoryUiList controls;
        private LaboratoryUiButton collapseButton;
        private LaboratoryUiButton rotateButton;
        private LaboratoryUiButton confirmButton;
        private LaboratoryUiButton cancelButton;
        private LaboratoryUiButton zoomInButton;
        private LaboratoryUiButton zoomOutButton;
        private LaboratoryUiButton fitButton;
        private LaboratoryMapView mapScreen;
        private bool controlsCollapsed;
        private int screenGeneration = -1;
        private SessionPhase screenPhase;

        public RectTransform MapViewport { get; private set; }
        public Canvas UiCanvas => screen?.Canvas;
        public RectTransform UiSafeRoot => screen?.SafeRoot;
        public bool ControlsCollapsed => controlsCollapsed;
        public LaboratoryUiButton FindUiButton(string key) => UiCanvas == null ? null
            : UiCanvas.GetComponentsInChildren<LaboratoryUiButton>(true).FirstOrDefault(b => b.name == key);

        private void EnsureScreen()
        {
            if (screen != null) return;
            screen = new LaboratoryUiCanvas(transform, "Temp_LaboratoryCanvas", 10);
            playRoot = LaboratoryUiWidgets.Panel("PlayScreen", screen.SafeRoot, LaboratoryUiWidgets.Background).rectTransform;
            titleText = LaboratoryUiWidgets.Label("Title", playRoot, 24); titleText.text = "DOOM INC / LABORATORY";
            hudText = LaboratoryUiWidgets.Label("Resources", playRoot, 17);
            worldGauge = LaboratoryUiWidgets.Panel("WorldProgress", playRoot, new Color(0.18f, 0.2f, 0.25f)).rectTransform;
            worldGaugeFill = LaboratoryUiWidgets.Panel("Progress", worldGauge, new Color(0.85f, 0.3f, 0.2f)).rectTransform;
            collapseButton = LaboratoryUiWidgets.Button("ToggleControls", playRoot, "Hide controls", () => screen.LayoutEpoch);
            collapseButton.Bind(() => { if (!CanClickScreen()) return; controlsCollapsed = !controlsCollapsed; screen.InvalidatePointers(); }, "screen");
            var mapBackground = LaboratoryUiWidgets.Panel("MapViewport", playRoot, new Color(0.025f, 0.04f, 0.065f));
            MapViewport = mapBackground.rectTransform; MapViewport.gameObject.AddComponent<RectMask2D>();
            // The map owns drawing and gesture interpretation; UI never mutates grid coordinates directly.
            mapScreen = MapViewport.gameObject.AddComponent<LaboratoryMapView>(); mapScreen.Initialize(this);
            toolRoot = LaboratoryUiWidgets.Rect("MapTools", playRoot);
            editText = LaboratoryUiWidgets.Label("PlacementStatus", toolRoot, 16);
            rotateButton = MakeTool("Rotate", "Rotate", () => RotatePreview());
            confirmButton = MakeTool("Confirm", "Confirm", () => ConfirmPlacement());
            cancelButton = MakeTool("Cancel", "Cancel", () => CancelPlacement());
            zoomInButton = MakeTool("ZoomIn", "+", () => mapScreen.ZoomBy(1.3f));
            zoomOutButton = MakeTool("ZoomOut", "−", () => mapScreen.ZoomBy(1 / 1.3f));
            fitButton = MakeTool("FitMap", "Fit", () => mapScreen.Fit());
            controlsScroll = LaboratoryUiWidgets.Scroll("Controls", playRoot, out var content);
            controls = new LaboratoryUiList(content, () => screen.LayoutEpoch);
            CreateEndingScreen();
        }

        private LaboratoryUiButton MakeTool(string key, string label, Action action)
        {
            var button = LaboratoryUiWidgets.Button(key, toolRoot, label, () => screen.LayoutEpoch);
            button.Bind(() => { if (CanInteract) action(); }, "initial");
            return button;
        }
        private bool CanClickScreen() => isActiveAndEnabled && Controller != null && !Controller.IsApplicationPaused;
        private string UiContext => Controller == null ? "none" : Controller.Generation + ":" + Controller.Phase;
        private void BindTool(LaboratoryUiButton button, Action action, bool enabled)
        {
            var generation = Controller.Generation; var selection = SelectedId; var editing = IsEditing;
            button.Bind(() => { if (CanInteract && generation == Controller.Generation && selection == SelectedId && editing == IsEditing) action(); }, UiContext + ":" + SelectedId + ":" + IsEditing);
            button.interactable = enabled && CanInteract;
        }
        private void UpdateScreen()
        {
            if (Controller == null) return;
            EnsureScreen(); screen.RefreshLayout();
            screen.SetVisible(isActiveAndEnabled);
            if (screenGeneration != Controller.Generation || screenPhase != Controller.Phase)
            {
                screen.InvalidatePointers();
                if (screenGeneration != Controller.Generation)
                {
                    controlsScroll.StopMovement(); resultScroll.StopMovement();
                    controlsScroll.content.anchoredPosition = panelScroll;
                    resultScroll.content.anchoredPosition = endingScroll;
                }
                screenGeneration = Controller.Generation; screenPhase = Controller.Phase;
            }
            var ending = Ending?.IsEnding == true;
            playRoot.gameObject.SetActive(!ending);
            endingRoot.gameObject.SetActive(ending);
            if (ending) UpdateEndingScreen();
            else
            {
                LayoutPlayScreen(); UpdateHud(); UpdateTools(); UpdateControls();
            }
            RepaintCount++;
        }

        private void LayoutPlayScreen()
        {
            var size = screen.Size; var w = size.x; var h = size.y; var margin = 12f;
            LaboratoryUiWidgets.Place(playRoot, 0, 0, w, h);
            LaboratoryUiWidgets.Place(titleText.rectTransform, margin, 10, w - 180, 34);
            LaboratoryUiWidgets.Place(collapseButton.GetComponent<RectTransform>(), w - 156, 8, 144, 48);
            collapseButton.GetComponentInChildren<Text>().text = controlsCollapsed ? "Show controls" : "Hide controls";
            collapseButton.interactable = CanClickScreen();
            LaboratoryUiWidgets.Place(hudText.rectTransform, margin, 50, w - 24, 56);
            LaboratoryUiWidgets.Place(worldGauge, margin, 101, w - 24, 6);
            var top = 112f;
            controlsScroll.gameObject.SetActive(!controlsCollapsed);
            if (screen.Portrait)
            {
                var toolbarHeight = ToolbarHeight(w - margin * 2);
                var panelHeight = controlsCollapsed ? 0 : Mathf.Clamp(h * 0.37f, 210, 430);
                var mapHeight = Mathf.Max(100, h - top - toolbarHeight - panelHeight - 32);
                LaboratoryUiWidgets.Place(MapViewport, margin, top, w - margin * 2, mapHeight);
                LaboratoryUiWidgets.Place(toolRoot, margin, top + mapHeight + 4, w - margin * 2, toolbarHeight);
                LaboratoryUiWidgets.Place((RectTransform)controlsScroll.transform, margin, top + mapHeight + toolbarHeight + 12, w - margin * 2, panelHeight);
            }
            else
            {
                var panelWidth = controlsCollapsed ? 0 : Mathf.Clamp(w * 0.39f, 300, 430);
                var mapWidth = w - margin * 2 - (controlsCollapsed ? 0 : panelWidth + margin);
                var toolbarHeight = ToolbarHeight(mapWidth);
                var mapHeight = Mathf.Max(100, h - top - toolbarHeight - 20);
                LaboratoryUiWidgets.Place(MapViewport, margin, top, mapWidth, mapHeight);
                LaboratoryUiWidgets.Place(toolRoot, margin, top + mapHeight + 4, mapWidth, toolbarHeight);
                LaboratoryUiWidgets.Place((RectTransform)controlsScroll.transform, w - margin - panelWidth, top, panelWidth, h - top - margin);
            }
        }

        private int ToolbarColumns(float width)
        {
            // Labels need room inside the 8px horizontal padding, beyond the touch target minimum.
            var columns = Mathf.Max(1, Mathf.Min(IsEditing ? 6 : 3, Mathf.FloorToInt((width + 6) / 96)));
            return IsEditing && columns > 3 && columns < 6 ? 3 : columns;
        }
        private float ToolbarHeight(float width)
        {
            var count = IsEditing ? 6 : 3; var rows = Mathf.CeilToInt((float)count / ToolbarColumns(width));
            return (IsEditing ? 60 : 28) + 4 + rows * 48 + (rows - 1) * 6;
        }

        private void UpdateHud()
        {
            LaboratoryUiWidgets.Place(worldGaugeFill, 0, 0, worldGauge.rect.width * (Invasion == null ? 0
                : (float)Math.Min(1, (double)Invasion.Score / Invasion.Settings.TargetScore)), 6);
            if (Simulation == null) { hudText.text = "The saved run could not be loaded. Retry loading or explicitly start a new run."; return; }
            var registry = Simulation.Registry;
            var organicCount = registry.Things.Where(t => t.Definition == organic).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            var cultureCount = registry.Things.Where(t => t.Definition == culture).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            var creatureCount = registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature);
            hudText.text = "Organic " + organicCount + "   Culture " + cultureCount + "   Creatures " + creatureCount
                + "\nWorld " + Invasion.Score + " / " + Invasion.Settings.TargetScore + "   Waiting " + Simulation.Invasion.WaitingCount
                + " / Outside " + Simulation.Invasion.ExternalCount + (IsEditing ? "   Laboratory paused" : "");
        }

        private void UpdateTools()
        {
            var editing = IsEditing; var width = toolRoot.rect.width;
            var detailHeight = editing ? 60 : 28;
            LaboratoryUiWidgets.Place(editText.rectTransform, 0, 0, width, detailHeight);
            if (editing)
            {
                var edit = Simulation.Map.Commands.Edit;
                editText.text = Simulation.ConstructionDefinition != null
                    ? "Build " + Simulation.ConstructionDefinition.DisplayName + " at " + Simulation.ConstructionOrigin + " / "
                        + (int)Simulation.ConstructionRotation * 90 + "°  " + Simulation.ConstructionValidation
                    : "Move at " + edit.Origin + " / " + (int)edit.Rotation * 90 + "°  " + edit.Validation;
            }
            else editText.text = "Tap to select · Drag to move · Pinch / scroll to zoom";
            var tools = editing ? new[] { rotateButton, confirmButton, cancelButton, zoomInButton, zoomOutButton, fitButton }
                : new[] { zoomInButton, zoomOutButton, fitButton };
            rotateButton.gameObject.SetActive(editing); confirmButton.gameObject.SetActive(editing); cancelButton.gameObject.SetActive(editing);
            var columns = ToolbarColumns(width); var buttonWidth = (width - (columns - 1) * 6) / columns;
            for (var i = 0; i < tools.Length; i++) LaboratoryUiWidgets.Place((RectTransform)tools[i].transform,
                (i % columns) * (buttonWidth + 6), detailHeight + 4 + (i / columns) * 54, buttonWidth, 48);
            BindTool(rotateButton, () => RotatePreview(), editing);
            BindTool(confirmButton, () => ConfirmPlacement(), editing && (Simulation.ConstructionDefinition != null
                ? Simulation.ConstructionValidation.Succeeded : Simulation.Map.Commands.Edit.Validation.Succeeded));
            BindTool(cancelButton, () => CancelPlacement(), editing);
            BindTool(zoomInButton, () => mapScreen.ZoomBy(1.3f), Simulation != null);
            BindTool(zoomOutButton, () => mapScreen.ZoomBy(1 / 1.3f), Simulation != null);
            BindTool(fitButton, () => mapScreen.Fit(), Simulation != null);
        }

        private void UiAction(string key, string label, Action action, bool enabled = true, Guid? selected = null)
        {
            var generation = Controller.Generation; var phase = Controller.Phase;
            controls.Action(key, label, () =>
            {
                if (!CanClickScreen() || Controller.Generation != generation || Controller.Phase != phase
                    || (selected.HasValue && SelectedId != selected.Value)) return;
                action();
            }, enabled && CanClickScreen(), UiContext + (selected.HasValue ? ":" + selected : ""));
        }

        private void UpdateControls()
        {
            controls.Begin(Mathf.Max(100, ((RectTransform)controlsScroll.transform).rect.width - 16));
            if (Simulation != null)
            {
                var canOperate = CanInteract && !IsEditing;
                controls.Text("WorldTitle", "WORLD ACTIVITY", 22);
                UiAction("Release", "Release waiting invaders (" + Simulation.Invasion.WaitingCount + ")", () => ReleaseInvaders(), canOperate && Simulation.Invasion.WaitingCount > 0);
                UiAction("Reports", ReportsExpanded ? "Hide reports" : "Show reports (" + Invasion.Reports.Count + ")", ToggleReports);
                if (ReportsExpanded)
                {
                    controls.Text("ReportMode", Invasion.DetailedReports ? "Detailed field reports" : "Major achievements");
                    if (Invasion.Reports.Count == 0) controls.Text("NoReports", "No field activity yet.");
                    foreach (var report in Invasion.Reports.Reverse()) controls.Text("Report_" + report.Sequence,
                        "#" + report.Sequence + " | " + ThingLabel(report.InvaderId) + "\n" + report.Text + " +" + report.Score + " (" + report.Time.ToString("F1") + "s)");
                }
                if (!string.IsNullOrEmpty(Simulation.Invasion.LastError)) controls.Text("InvasionError", Simulation.Invasion.LastError);
                if (Simulation.Registry.TryGet(SelectedId, out var selected)) AddSelectedControls(selected);
                controls.Text("ConstructionTitle", "CONSTRUCTION", 22);
                UiAction("BuildGathering", "Build " + gathering.DisplayName + " (" + gathering.BuildCost + " organic)", () => BeginGatheringConstruction(), canOperate);
                UiAction("BuildStorage", "Build " + storage.DisplayName + " (" + storage.BuildCost + " organic)", () => BeginStorageConstruction(), canOperate);
                UiAction("BuildProcessing", "Build " + processing.DisplayName + " (" + processing.BuildCost + " organic)", () => BeginProcessingConstruction(), canOperate);
                UiAction("BuildCultivation", "Build " + cultivation.DisplayName + " (" + cultivation.BuildCost + " organic)", () => BeginCultivationConstruction(), canOperate);
                controls.Text("RosterTitle", "FACILITIES / CREATURES", 22);
                controls.Text("RosterHint", "Use this list to select each creature, including those sharing the same cell.", 16);
                foreach (var thing in Simulation.Registry.Things.Where(t => t.Definition.Kind != ThingKind.Resource && t.IsPlaced).OrderBy(t => t.CreationOrder))
                {
                    var id = thing.Id;
                    UiAction("Select_" + id.ToString("N"), thing.Definition.DisplayName + " #" + thing.CreationOrder + (id == SelectedId ? " [selected]" : ""), () => SelectThing(id), canOperate);
                }
                controls.Text("PresetTitle", "DEFAULT PRIORITIES", 22);
                controls.Text("PresetHint", "0 = off; lower = first. Ties: Build > Gather > Haul > Produce.", 16);
                AddPriorities("Preset", Simulation.Preset.ToArray(), values => SetPreset(values), canOperate);
                controls.Text("Reservations", "Reservations: " + Simulation.Reservations.Count);
                foreach (var reservation in Simulation.Reservations)
                    controls.Text("Reservation_" + reservation.WorkerId + "_" + reservation.ResourceId,
                        ThingLabel(reservation.WorkerId) + ": " + reservation.Amount + " " + ResourceLabel(reservation.ResourceId) + " / "
                        + (reservation.PickedUp ? "carrying" : "outgoing + incoming reserved") + "\n"
                        + ThingLabel(reservation.SourceId) + " > " + ThingLabel(reservation.DestinationId));
            }
            controls.Text("SaveTitle", "SAVE / CONTINUE", 22);
            controls.Text("SaveCount", "Successful saves: " + Controller.SuccessfulSaves);
            UiAction("Save", "Save now", () => Save(), Controller.Current != null);
            UiAction("Reload", "Continue last saved state", () => Reload());
            UiAction("Restart", "Start a new run", () => Restart());
            if (!string.IsNullOrEmpty(LastCommand)) controls.Text("LastCommand", LastCommand, 16);
            if (!string.IsNullOrEmpty(Controller.LastError)) controls.Text("LoadError", Controller.LastError);
            if (!string.IsNullOrEmpty(Controller.LastSaveError)) controls.Text("SaveError", "Save failed: " + Controller.LastSaveError);
            controls.End(); panelScroll = controlsScroll.content.anchoredPosition;
        }

        private void AddSelectedControls(Thing selected)
        {
            var id = selected.Id; var canOperate = CanInteract && !IsEditing;
            controls.Text("SelectedTitle", selected.Definition.DisplayName + " #" + selected.CreationOrder, 22);
            controls.Text("SelectedPosition", selected.Placement == null ? "Outside map" : "Position: " + selected.Placement.Origin);
            if (selected.TryGetComponent<ResourceContainerComponent>(out var container))
                foreach (var group in container.Contents.GroupBy(t => t.Definition)) controls.Text("Stored_" + group.Key.DefinitionId, group.Key.DisplayName + ": " + group.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity));
            if (selected.TryGetComponent<ConstructionComponent>(out var construction))
            {
                var definition = (FacilityData)selected.Definition;
                controls.Text("ConstructionState", construction.IsComplete ? "Construction complete" : "Build: " + construction.Progress.ToString("F1") + " / " + definition.BuildSeconds + "s; material " + construction.Delivered + " / " + definition.BuildCost);
                UiAction("Move", "Move / rotate facility", () => BeginPlacement(), construction.IsComplete && canOperate, id);
            }
            if (selected.TryGetComponent<GatheringComponent>(out var gatheringState)) controls.Text("GatheringState", "Gathering: " + gatheringState.Progress.ToString("F1") + " / " + ((FacilityData)selected.Definition).GatheringSeconds + "s\nAwaiting storage admission: " + gatheringState.Outstanding);
            if (selected.TryGetComponent<ProductionComponent>(out var production))
            {
                var definition = (FacilityData)selected.Definition;
                controls.Text("ProductionState", "Cycle: " + production.Progress.ToString("F1") + " / " + definition.ProductionSeconds + "s\nInput: " + Simulation.Production.InputCount(selected) + " / " + definition.InputAmount + " " + definition.InputResource.DisplayName + (production.InProgress ? " (consumed)" : ""));
                var working = Simulation.Registry.Things.Any(t => t.TryGetComponent<WorkerComponent>(out var worker) && worker.Job != null && worker.Job.Kind == WorkKind.Production && worker.Job.Stage == JobStage.Working && worker.Job.TargetId == id);
                var reason = Simulation.Production.WaitReason(selected);
                controls.Text("ProductionReason", !Controller.CanAdvanceLaboratory ? "Paused" : !string.IsNullOrEmpty(reason) ? reason : (working ? "Working: " + (definition.Settings.StaffedRate * 100).ToString("0.#") : "Automatic: " + (definition.Settings.UnattendedRate * 100).ToString("0.#")) + "%");
                if (definition.Role == FacilityRole.Cultivation)
                {
                    controls.Text("CultivationState", "Current: " + (production.InProgress ? production.ActiveTarget.ToString() : "None") + " | Next: " + production.SelectedTarget);
                    UiAction("CultivateSupport", "Cultivate support (" + definition.InputAmount + " culture)", () => SelectCultivation(id, CultivationTarget.Support), canOperate && selected.GetComponent<ConstructionComponent>().IsComplete, id);
                    UiAction("CultivateInvader", "Cultivate invader (" + definition.InputAmount + " culture)", () => SelectCultivation(id, CultivationTarget.Invader), canOperate && selected.GetComponent<ConstructionComponent>().IsComplete, id);
                }
            }
            if (selected.TryGetComponent<StorageComponent>(out var storageState))
            {
                controls.Text("StorageState", "Capacity: " + ((FacilityData)selected.Definition).StorageCapacity + "\nIncoming reserved: " + Simulation.Reservations.Where(r => r.DestinationId == id).Sum(r => r.Amount) + " / Outgoing reserved: " + Simulation.Reservations.Where(r => r.SourceId == id && !r.PickedUp).Sum(r => r.Amount));
                UiAction("AllowOrganic", "Organic matter: " + (storageState.AllowsOrganic ? "allowed" : "blocked"), () => SetAllowed(id, organic, !storageState.AllowsOrganic), canOperate, id);
                UiAction("AllowCulture", "Culture medium: " + (storageState.AllowsCulture ? "allowed" : "blocked"), () => SetAllowed(id, culture, !storageState.AllowsCulture), canOperate, id);
            }
            if (selected.TryGetComponent<WorkerComponent>(out var workerState))
            {
                controls.Text("WorkerJob", workerState.Job == null ? "Job: idle" : "Job: " + workerState.Job.Kind + " / " + workerState.Job.Stage + "\nTarget: " + ThingLabel(workerState.Job.TargetId));
                if (!string.IsNullOrWhiteSpace(workerState.WaitReason)) controls.Text("WorkerWait", "Waiting: " + workerState.WaitReason);
                if (((CreatureData)selected.Definition).CanOperate)
                {
                    controls.Text("Assigned", "Assigned: " + (workerState.AssignedFacilityId.HasValue ? ThingLabel(workerState.AssignedFacilityId.Value) : "None"));
                    UiAction("Unassign", "Unassign", () => AssignWorker(id, null), canOperate, id);
                    foreach (var facility in Simulation.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _) && t.GetComponent<ConstructionComponent>().IsComplete))
                    {
                        var facilityId = facility.Id;
                        UiAction("Assign_" + facilityId.ToString("N"), "Assign: " + ThingLabel(facilityId), () => AssignWorker(id, facilityId), canOperate, id);
                    }
                }
                controls.Text("WorkerPriorities", workerState.UsesPreset ? "Priorities: default preset" : "Priorities: individual override");
                AddPriorities("WorkerPriority", workerState.UsesPreset ? Simulation.Preset.ToArray() : workerState.Priorities.ToArray(), values => SetWorkerPriorities(id, values), canOperate, id);
                if (!workerState.UsesPreset) UiAction("UsePreset", "Use default preset", () => SetWorkerPriorities(id, Simulation.Preset.ToArray(), true), canOperate, id);
            }
        }

        private void AddPriorities(string key, int[] values, Action<int[]> apply, bool enabled, Guid? selected = null)
        {
            var names = new[] { "Build", "Gather", "Haul", "Produce" };
            for (var index = 0; index < values.Length && index < names.Length; index++)
            {
                var slot = index;
                UiAction(key + "_" + slot, names[slot] + ": " + values[slot], () =>
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
        private string ThingLabel(Guid id) => Simulation.Registry.TryGet(id, out var thing) ? thing.Definition.DisplayName + " #" + thing.CreationOrder : "Unassigned";
        private string ResourceLabel(Guid id) => Simulation.Registry.TryGet(id, out var resource) ? resource.Definition.DisplayName : "resource";
        private void InvalidateScreenInput() => screen?.InvalidatePointers();
        private void SetScreenVisible(bool visible) => screen?.SetVisible(visible);
        private void DisposeScreen() { screen?.Dispose(); screen = null; MapViewport = null; }
    }
}
