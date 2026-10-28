using System;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private GUIStyle cellStyle;
        private GUIStyle wrappedStyle;
        private Vector2 panelScroll;

        private void OnGUI()
        {
            if (Controller == null) return;
            if (Event.current.type == EventType.Repaint) RepaintCount++;
            if (Ending?.IsEnding == true) { DrawEnding(); return; }
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 10, Screen.width - 40, 25), "DOOM INC / LABORATORY");
            GUI.Label(new Rect(20, 36, Screen.width - 40, 25),
                "Build, process, cultivate. Release waiting invaders and follow their achievements.");
            var panelWidth = Mathf.Min(390, Screen.width * 0.43f);
            var gridWidth = Mathf.Max(100, Screen.width - panelWidth - 60);
            if (Simulation != null)
            {
                var map = Simulation.Map;
                var size = Mathf.Max(8, Mathf.Min(52, gridWidth / map.Width, (Screen.height - 200f) / map.Height));
                var grid = new Rect(20, 90, size * map.Width, size * map.Height);
                DrawGrid(grid, size);
                GUI.Label(new Rect(20, grid.yMax + 10, gridWidth, 22), "G: gatherer  S: storage  F: processing  C: cultivation");
                GUI.Label(new Rect(20, grid.yMax + 34, gridWidth, 22), "P: prototype  W: support  V: invader  I: interaction | Yellow: building");
                GUI.Label(new Rect(20, grid.yMax + 60, gridWidth, 42),
                    "Organic matter: " + Simulation.Registry.Things.Where(t => t.Definition == organic)
                        .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity)
                    + " | Culture: " + Simulation.Registry.Things.Where(t => t.Definition == culture)
                        .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity) + " | Reservations: " + Simulation.Reservations.Count
                    + "\nLaboratory: " + (Controller.CanAdvanceLaboratory ? "running" : "paused")
                    + " | Phase: " + Controller.Phase);
            }
            DrawPanel(new Rect(Screen.width - panelWidth - 20, 72, panelWidth, Mathf.Max(120, Screen.height - 86)));
        }

        private void DrawGrid(Rect grid, float size)
        {
            if (cellStyle == null)
            {
                cellStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                cellStyle.normal.textColor = Color.white;
                cellStyle.hover.textColor = Color.white;
                cellStyle.active.textColor = Color.white;
            }
            var placements = Simulation.Map.GetPlacements();
            var edit = Simulation.Map.Commands.Edit;
            var constructing = Simulation.ConstructionDefinition != null;
            var previewOrigin = constructing ? Simulation.ConstructionOrigin : edit?.Origin;
            var previewValid = constructing ? Simulation.ConstructionValidation.Succeeded : edit?.Validation.Succeeded ?? false;
            var previewDefinition = Simulation.ConstructionDefinition;
            if (previewDefinition == null && edit != null && Simulation.Registry.TryGet(edit.ThingId, out var moving))
                previewDefinition = moving.Definition as FacilityData;
            var previewRotation = constructing ? Simulation.ConstructionRotation : edit?.Rotation ?? QuarterTurn.None;
            var previewArea = previewDefinition != null && previewOrigin.HasValue
                ? new RectInt(previewOrigin.Value, FacilityGeometry.GetSize(previewDefinition, previewRotation))
                : new RectInt();
            var previewInteraction = previewDefinition != null && previewOrigin.HasValue
                ? (Vector2Int?)FacilityGeometry.GetInteractionCell(previewDefinition, previewOrigin.Value, previewRotation) : null;
            for (var y = 0; y < Simulation.Map.Height; y++)
            for (var x = 0; x < Simulation.Map.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                var rect = new Rect(grid.x + x * size, grid.y + (Simulation.Map.Height - 1 - y) * size, size - 2, size - 2);
                var facility = placements.FirstOrDefault(p => p.Kind == ThingKind.Facility && p.Contains(cell));
                var worker = placements.FirstOrDefault(p => p.Kind == ThingKind.Creature && p.Contains(cell));
                var interaction = placements.FirstOrDefault(p => p.InteractionCell == cell);
                var tint = new Color(0.13f, 0.17f, 0.23f);
                var label = interaction != null ? "I" : "";
                Guid selected = Guid.Empty;
                if (facility != null && Simulation.Registry.TryGet(facility.ThingId, out var thing))
                {
                    var definition = (FacilityData)thing.Definition;
                    label = (definition.Role == FacilityRole.Gathering ? "G" : definition.Role == FacilityRole.Storage ? "S"
                        : definition.Role == FacilityRole.Processing ? "F" : "C") + thing.CreationOrder;
                    tint = thing.GetComponent<ConstructionComponent>().IsComplete
                        ? new Color(0.18f, 0.42f, 0.62f) : new Color(0.56f, 0.43f, 0.12f);
                    selected = thing.Id;
                }
                else if (worker != null)
                {
                    var role = ((CreatureData)Simulation.Registry.Things.First(t => t.Id == worker.ThingId).Definition).Role;
                    label = role == CreatureRole.Prototype ? "P" : role == CreatureRole.Support ? "W" : "V";
                    tint = new Color(0.72f, 0.4f, 0.16f);
                    selected = worker.ThingId;
                }
                if (selected != Guid.Empty && selected == SelectedId) tint *= 1.4f;
                if (previewOrigin.HasValue && previewArea.Contains(cell))
                {
                    tint = previewValid ? new Color(0.12f, 0.55f, 0.32f) : new Color(0.65f, 0.15f, 0.18f);
                    label = "+";
                }
                else if (previewInteraction.HasValue && previewInteraction.Value == cell)
                {
                    tint = new Color(0.38f, 0.24f, 0.55f);
                    label = "I+";
                }
                GUI.color = tint;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                if (GUI.Button(rect, label, cellStyle))
                {
                    if (IsEditing) PreviewAt(cell);
                    else if (selected != Guid.Empty) SelectThing(selected);
                }
            }
        }

        private void DrawPanel(Rect rect)
        {
            if (wrappedStyle == null) wrappedStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.BeginArea(rect, GUI.skin.box);
            panelScroll = GUILayout.BeginScrollView(panelScroll);
            if (Simulation != null) DrawInvasion();
            GUILayout.Label("CONSTRUCTION");
            GUI.enabled = Simulation != null && Controller.Phase == SessionPhase.Playing && !IsEditing;
            if (GUILayout.Button("Build gatherer (" + gathering.BuildCost + " organic)")) BeginGatheringConstruction();
            if (GUILayout.Button("Build storage (" + storage.BuildCost + " organic)")) BeginStorageConstruction();
            if (processing != null && GUILayout.Button("Build processing (" + processing.BuildCost + " organic)")) BeginProcessingConstruction();
            if (cultivation != null && GUILayout.Button("Build cultivation (" + cultivation.BuildCost + " organic)")) BeginCultivationConstruction();
            GUI.enabled = true;
            if (Simulation != null)
            {
                if (IsEditing) DrawEdit();
                GUILayout.Space(8);
                if (Simulation.Registry.TryGet(SelectedId, out var selected)) DrawSelected(selected);
                GUILayout.Space(8);
                GUILayout.Label("FACILITIES / CREATURES");
                foreach (var thing in Simulation.Registry.Things.Where(t => t.Definition.Kind != ThingKind.Resource && t.IsPlaced)
                    .OrderBy(t => t.CreationOrder))
                {
                    GUI.enabled = !IsEditing;
                    if (GUILayout.Button(thing.Definition.DisplayName + " #" + thing.CreationOrder
                        + (SelectedId == thing.Id ? " [selected]" : ""))) SelectThing(thing.Id);
                }
                GUI.enabled = true;
                GUILayout.Space(10);
                GUILayout.Label("DEFAULT PRIORITIES (0 = off; lower = first)");
                DrawPriorities(Simulation.Preset.ToArray(), values => SetPreset(values));
                GUILayout.Label("Ties: Build > Gather > Haul > Produce.", wrappedStyle);
                GUILayout.Label("RESERVATIONS: " + Simulation.Reservations.Count);
                foreach (var reservation in Simulation.Reservations)
                    GUILayout.Label(ThingLabel(reservation.WorkerId) + ": " + reservation.Amount + " " + ResourceLabel(reservation.ResourceId) + " / "
                        + (reservation.PickedUp ? "carrying" : "outgoing + incoming reserved") + "\n"
                        + ThingLabel(reservation.SourceId) + " > " + ThingLabel(reservation.DestinationId), wrappedStyle);
            }
            GUILayout.Space(10);
            GUILayout.Label("SAVE / CONTINUE");
            GUILayout.Label("Automatic saves: " + Controller.SuccessfulSaves);
            if (GUILayout.Button("Save now")) Save();
            if (GUILayout.Button("Continue last saved state")) Reload();
            if (GUILayout.Button("Restart laboratory")) Restart();
            GUILayout.Label(LastCommand ?? "", wrappedStyle);
            if (!string.IsNullOrEmpty(Controller.LastError)) GUILayout.Label("Command: " + Controller.LastError, wrappedStyle);
            if (!string.IsNullOrEmpty(Controller.LastSaveError)) GUILayout.Label("Save: " + Controller.LastSaveError, wrappedStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawEdit()
        {
            var edit = Simulation.Map.Commands.Edit;
            GUILayout.Label("Placement pauses laboratory work.", wrappedStyle);
            GUILayout.Label(Simulation.ConstructionDefinition != null
                ? "Construction at " + Simulation.ConstructionOrigin + " / " + ((int)Simulation.ConstructionRotation * 90)
                    + " degrees / " + Simulation.ConstructionValidation
                : "Move to " + edit.Origin + " / " + ((int)edit.Rotation * 90) + " degrees / " + edit.Validation, wrappedStyle);
            if (GUILayout.Button("Rotate 90 degrees")) RotatePreview();
            if (GUILayout.Button("Confirm location")) ConfirmPlacement();
            if (GUILayout.Button("Cancel")) CancelPlacement();
        }

        private void DrawInvasion()
        {
            var manager = Simulation.Invasion;
            var state = manager.State;
            GUILayout.Label("WORLD DESTRUCTION: " + state.Score + " / " + state.Settings.TargetScore);
            var bar = GUILayoutUtility.GetRect(80, 14, GUILayout.ExpandWidth(true));
            GUI.color = new Color(0.18f, 0.2f, 0.25f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = new Color(0.85f, 0.3f, 0.2f);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * (float)Math.Min(1, (double)state.Score / state.Settings.TargetScore), bar.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUILayout.Label("Waiting: " + manager.WaitingCount + " | Outside: " + manager.ExternalCount);
            if (Controller.Phase != SessionPhase.Playing)
                GUILayout.Label(state.GoalReached ? "GOAL REACHED - laboratory and outside activity stopped."
                    : "Saved world ended - all activity stopped.", wrappedStyle);
            else if (IsEditing) GUILayout.Label("Placement paused. Outside activity continues.", wrappedStyle);
            GUI.enabled = Controller.CanAdvanceExternal && !IsEditing && manager.WaitingCount > 0;
            if (GUILayout.Button("Release all waiting invaders")) ReleaseInvaders();
            GUI.enabled = true;
            if (GUILayout.Button((ReportsExpanded ? "Hide" : "Show") + " reports (" + state.Reports.Count + ")")) ToggleReports();
            if (ReportsExpanded)
            {
                GUILayout.Label(state.DetailedReports ? "Detailed field reports" : "Major achievements", wrappedStyle);
                if (state.Reports.Count == 0) GUILayout.Label("No field activity yet.");
                foreach (var report in state.Reports.Reverse())
                    GUILayout.Label("#" + report.Sequence + " | " + ThingLabel(report.InvaderId) + "\n"
                        + report.Text + " +" + report.Score + " (" + report.Time.ToString("F1") + "s)", wrappedStyle);
            }
            if (!string.IsNullOrEmpty(manager.LastError)) GUILayout.Label(manager.LastError, wrappedStyle);
            GUILayout.Space(10);
        }

        private void DrawSelected(Thing selected)
        {
            GUILayout.Space(8);
            GUILayout.Label("SELECTED: " + selected.Definition.DisplayName + " #" + selected.CreationOrder, wrappedStyle);
            GUILayout.Label(selected.Placement == null ? "Outside map" : "Position: " + selected.Placement.Origin);
            if (selected.TryGetComponent<ResourceContainerComponent>(out var container))
                foreach (var group in container.Contents.GroupBy(t => t.Definition))
                    GUILayout.Label(group.Key.DisplayName + ": " + group.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity));
            if (selected.TryGetComponent<ConstructionComponent>(out var construction))
            {
                var definition = (FacilityData)selected.Definition;
                GUILayout.Label(construction.IsComplete ? "Construction complete" : "Build: "
                    + construction.Progress.ToString("F1") + " / " + definition.BuildSeconds.ToString("F1") + "s; material "
                    + construction.Delivered + " / " + definition.BuildCost, wrappedStyle);
                GUI.enabled = construction.IsComplete && !IsEditing;
                if (GUILayout.Button("Move / rotate facility")) BeginPlacement();
                GUI.enabled = true;
            }
            if (selected.TryGetComponent<GatheringComponent>(out var gatheringState))
            {
                GUILayout.Label("Gathering: " + gatheringState.Progress.ToString("F1") + " / "
                    + ((FacilityData)selected.Definition).GatheringSeconds.ToString("F1") + "s");
                GUILayout.Label("Awaiting actual storage admission: " + gatheringState.Outstanding, wrappedStyle);
            }
            if (selected.TryGetComponent<ProductionComponent>(out var productionState)) DrawProduction(selected, productionState);
            if (selected.TryGetComponent<StorageComponent>(out var storageState))
            {
                GUILayout.Label("Capacity: " + ((FacilityData)selected.Definition).StorageCapacity);
                GUILayout.Label("Incoming reserved: " + Simulation.Reservations.Where(r => r.DestinationId == selected.Id).Sum(r => r.Amount)
                    + " / Outgoing reserved: " + Simulation.Reservations.Where(r => r.SourceId == selected.Id && !r.PickedUp).Sum(r => r.Amount), wrappedStyle);
                var allowed = GUILayout.Toggle(storageState.AllowsOrganic, "Allow organic matter");
                if (allowed != storageState.AllowsOrganic) SetAllowed(selected.Id, allowed);
                if (culture != null)
                {
                    var allowCulture = GUILayout.Toggle(storageState.AllowsCulture, "Allow culture medium");
                    if (allowCulture != storageState.AllowsCulture) SetAllowed(selected.Id, culture, allowCulture);
                }
            }
            if (selected.TryGetComponent<WorkerComponent>(out var worker))
            {
                GUILayout.Label(worker.Job == null ? "Job: idle" : "Job: " + worker.Job.Kind + " / " + worker.Job.Stage, wrappedStyle);
                if (worker.Job != null) GUILayout.Label("Target: " + ThingLabel(worker.Job.TargetId), wrappedStyle);
                if (!string.IsNullOrWhiteSpace(worker.WaitReason)) GUILayout.Label("Waiting: " + worker.WaitReason, wrappedStyle);
                if (((CreatureData)selected.Definition).CanOperate)
                {
                    GUILayout.Label("Assigned: " + (worker.AssignedFacilityId.HasValue ? ThingLabel(worker.AssignedFacilityId.Value) : "None"), wrappedStyle);
                    if (GUILayout.Button("Unassign")) AssignWorker(selected.Id, null);
                    foreach (var facility in Simulation.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _) && t.GetComponent<ConstructionComponent>().IsComplete))
                        if (GUILayout.Button("Assign: " + ThingLabel(facility.Id))) AssignWorker(selected.Id, facility.Id);
                }
                GUILayout.Label(worker.UsesPreset ? "Priorities: default preset" : "Priorities: individual override");
                DrawPriorities(worker.UsesPreset ? Simulation.Preset.ToArray() : worker.Priorities.ToArray(),
                    values => SetWorkerPriorities(selected.Id, values));
                if (!worker.UsesPreset && GUILayout.Button("Use default preset"))
                    SetWorkerPriorities(selected.Id, Simulation.Preset.ToArray(), true);
            }
        }

        private void DrawProduction(Thing facility, ProductionComponent state)
        {
            var definition = (FacilityData)facility.Definition;
            var working = Simulation.Registry.Things.Any(t => t.TryGetComponent<WorkerComponent>(out var w)
                && w.Job != null && w.Job.Kind == WorkKind.Production && w.Job.Stage == JobStage.Working && w.Job.TargetId == facility.Id);
            GUILayout.Label("Cycle: " + state.Progress.ToString("F1") + " / " + definition.ProductionSeconds + "s");
            GUILayout.Label("Input: " + Simulation.Production.InputCount(facility) + " / " + definition.InputAmount
                + " " + definition.InputResource.DisplayName + (state.InProgress ? " (consumed for current cycle)" : ""), wrappedStyle);
            var reason = Simulation.Production.WaitReason(facility);
            GUILayout.Label(!Controller.CanAdvanceLaboratory ? "Paused" : !string.IsNullOrEmpty(reason) ? reason
                : working ? "Working: 100%" : "Automatic: 20%", wrappedStyle);
            if (definition.Role == FacilityRole.Cultivation)
            {
                GUILayout.Label("Current: " + (state.InProgress ? state.ActiveTarget.ToString() : "None")
                    + " | Next: " + state.SelectedTarget, wrappedStyle);
                GUI.enabled = facility.GetComponent<ConstructionComponent>().IsComplete && !IsEditing;
                if (GUILayout.Button("Cultivate support (2 culture)")) SelectCultivation(facility.Id, CultivationTarget.Support);
                if (GUILayout.Button("Cultivate invader (2 culture)")) SelectCultivation(facility.Id, CultivationTarget.Invader);
                GUI.enabled = true;
            }
        }

        private string ResourceLabel(Guid id) => Simulation.Registry.TryGet(id, out var resource) ? resource.Definition.DisplayName : "resource";

        private string ThingLabel(Guid id) => Simulation.Registry.TryGet(id, out var thing)
            ? thing.Definition.DisplayName + " #" + thing.CreationOrder : "Unassigned";

        private static void DrawPriorities(int[] values, Action<int[]> apply)
        {
            GUILayout.BeginHorizontal();
            var names = new[] { "Build", "Gather", "Haul", "Produce" };
            if (values.Length == 3) values = values.Concat(new[] { 4 }).ToArray();
            for (var index = 0; index < values.Length; index++)
            {
                if (!GUILayout.Button(names[index] + ": " + values[index])) continue;
                var changed = (int[])values.Clone();
                changed[index] = (values[index] + 1) % 5;
                apply(changed);
            }
            GUILayout.EndHorizontal();
        }
    }
}
