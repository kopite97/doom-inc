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
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 10, Screen.width - 40, 25), "DOOM INC / INITIAL LABORATORY");
            GUI.Label(new Rect(20, 36, Screen.width - 40, 25),
                "Temp presentation: build a gatherer and storage, then watch the prototype work. Select a grid cell or an item below.");
            var panelWidth = Mathf.Min(390, Screen.width * 0.43f);
            var gridWidth = Mathf.Max(100, Screen.width - panelWidth - 60);
            if (Simulation != null)
            {
                var map = Simulation.Map;
                var size = Mathf.Max(8, Mathf.Min(52, gridWidth / map.Width, (Screen.height - 200f) / map.Height));
                var grid = new Rect(20, 90, size * map.Width, size * map.Height);
                DrawGrid(grid, size);
                GUI.Label(new Rect(20, grid.yMax + 10, gridWidth, 22), "G: gatherer   S: storage   P: prototype   I: interaction");
                GUI.Label(new Rect(20, grid.yMax + 34, gridWidth, 22), "Yellow: construction   Green / red: valid / invalid preview");
                GUI.Label(new Rect(20, grid.yMax + 60, gridWidth, 42),
                    "Organic matter: " + Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                        .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity)
                    + " total | Reservations: " + Simulation.Reservations.Count
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
                previewDefinition = moving.Definition as FacilityDefinition;
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
                    var definition = (FacilityDefinition)thing.Definition;
                    label = (definition.Role == FacilityRole.Gathering ? "G" : "S") + thing.CreationOrder;
                    tint = thing.GetComponent<ConstructionComponent>().IsComplete
                        ? new Color(0.18f, 0.42f, 0.62f) : new Color(0.56f, 0.43f, 0.12f);
                    selected = thing.Id;
                }
                else if (worker != null)
                {
                    label = "P";
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
            GUILayout.Label("CONSTRUCTION");
            GUI.enabled = Simulation != null && Controller.Phase == SessionPhase.Playing && !IsEditing;
            if (GUILayout.Button("Build gatherer (" + gathering.BuildCost + " organic)")) BeginGatheringConstruction();
            if (GUILayout.Button("Build storage (" + storage.BuildCost + " organic)")) BeginStorageConstruction();
            GUI.enabled = true;
            if (Simulation != null)
            {
                if (IsEditing) DrawEdit();
                GUILayout.Space(8);
                GUILayout.Label("FACILITIES / CREATURES");
                foreach (var thing in Simulation.Registry.Things.Where(t => t.Definition.Kind != ThingKind.Resource)
                    .OrderBy(t => t.CreationOrder))
                {
                    GUI.enabled = !IsEditing;
                    if (GUILayout.Button(thing.Definition.DisplayName + " #" + thing.CreationOrder
                        + (SelectedId == thing.Id ? " [selected]" : ""))) SelectThing(thing.Id);
                }
                GUI.enabled = true;
                if (Simulation.Registry.TryGet(SelectedId, out var selected)) DrawSelected(selected);
                GUILayout.Space(10);
                GUILayout.Label("DEFAULT PRIORITIES (0 = off; lower = first)");
                DrawPriorities(Simulation.Preset.ToArray(), values => SetPreset(values));
                GUILayout.Label("Ties follow Construction > Gathering > Hauling.", wrappedStyle);
                GUILayout.Label("RESERVATIONS: " + Simulation.Reservations.Count);
                foreach (var reservation in Simulation.Reservations)
                    GUILayout.Label(ThingLabel(reservation.WorkerId) + ": " + reservation.Amount + " organic / "
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

        private void DrawSelected(Thing selected)
        {
            GUILayout.Space(8);
            GUILayout.Label("SELECTED: " + selected.Definition.DisplayName + " #" + selected.CreationOrder, wrappedStyle);
            GUILayout.Label(selected.Placement == null ? "Outside map" : "Position: " + selected.Placement.Origin);
            if (selected.TryGetComponent<ResourceContainerComponent>(out var container))
                GUILayout.Label("Held organic: " + container.Contents.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity));
            if (selected.TryGetComponent<ConstructionComponent>(out var construction))
            {
                var definition = (FacilityDefinition)selected.Definition;
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
                    + ((FacilityDefinition)selected.Definition).GatheringSeconds.ToString("F1") + "s");
                GUILayout.Label("Awaiting actual storage admission: " + gatheringState.Outstanding, wrappedStyle);
            }
            if (selected.TryGetComponent<StorageComponent>(out var storageState))
            {
                GUILayout.Label("Capacity: " + ((FacilityDefinition)selected.Definition).StorageCapacity);
                GUILayout.Label("Incoming reserved: " + Simulation.Reservations.Where(r => r.DestinationId == selected.Id).Sum(r => r.Amount)
                    + " / Outgoing reserved: " + Simulation.Reservations.Where(r => r.SourceId == selected.Id && !r.PickedUp).Sum(r => r.Amount), wrappedStyle);
                var allowed = GUILayout.Toggle(storageState.AllowsOrganic, "Allow organic matter");
                if (allowed != storageState.AllowsOrganic) SetAllowed(selected.Id, allowed);
            }
            if (selected.TryGetComponent<WorkerComponent>(out var worker))
            {
                GUILayout.Label(worker.Job == null ? "Job: idle" : "Job: " + worker.Job.Kind + " / " + worker.Job.Stage, wrappedStyle);
                if (worker.Job != null) GUILayout.Label("Target: " + ThingLabel(worker.Job.TargetId), wrappedStyle);
                if (!string.IsNullOrWhiteSpace(worker.WaitReason)) GUILayout.Label("Waiting: " + worker.WaitReason, wrappedStyle);
                GUILayout.Label(worker.UsesPreset ? "Priorities: default preset" : "Priorities: individual override");
                DrawPriorities(worker.UsesPreset ? Simulation.Preset.ToArray() : worker.Priorities.ToArray(),
                    values => SetWorkerPriorities(selected.Id, values));
                if (!worker.UsesPreset && GUILayout.Button("Use default preset"))
                    SetWorkerPriorities(selected.Id, Simulation.Preset.ToArray(), true);
            }
        }

        private string ThingLabel(Guid id) => Simulation.Registry.TryGet(id, out var thing)
            ? thing.Definition.DisplayName + " #" + thing.CreationOrder : "Unassigned";

        private static void DrawPriorities(int[] values, Action<int[]> apply)
        {
            GUILayout.BeginHorizontal();
            var names = new[] { "Build", "Gather", "Haul" };
            for (var index = 0; index < values.Length; index++)
            {
                if (!GUILayout.Button(names[index] + ": " + values[index])) continue;
                var changed = (int[])values.Clone();
                changed[index] = (values[index] + 1) % 4;
                apply(changed);
            }
            GUILayout.EndHorizontal();
        }
    }
}
