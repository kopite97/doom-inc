using System;
using System.Linq;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Temporary PC interaction surface. Rules and state changes are owned by Map.Commands.
    public sealed class MapFoundationView : MonoBehaviour
    {
        private GUIStyle cellStyle;

        public MapFoundationScenario Session { get; private set; }
        public Guid SelectedId { get; private set; }
        public PlacementResult LastResult { get; private set; }
        public int RepaintCount { get; private set; }


        public void Initialize(FacilityData storageDefinition, CreatureData creatureDefinition,
            ResourceData resourceDefinition)
        {
            if (Session != null)
                throw new InvalidOperationException("This view already has a session.");
            Session = new MapFoundationScenario(storageDefinition, creatureDefinition, resourceDefinition);
            SelectedId = Session.Facilities[0].Id;
        }

        public bool SelectThing(Guid id)
        {
            if (Session.Map.Commands.IsEditing || !Session.Registry.TryGet(id, out var thing)
                || thing.Definition.Kind == ThingKind.Resource)
                return false;
            SelectedId = id;
            return true;
        }

        public PlacementResult BeginPlacement() => LastResult = Session.Map.Commands.BeginEdit(SelectedId);
        public PlacementResult PreviewAt(Vector2Int cell) => LastResult = Session.Map.Commands.Preview(cell,
            Session.Map.Commands.Edit?.Rotation ?? QuarterTurn.None);
        public PlacementResult RotatePreview()
        {
            var edit = Session.Map.Commands.Edit;
            return LastResult = edit == null ? PlacementResult.Reject(PlacementError.NoEdit)
                : Session.Map.Commands.Preview(edit.Origin, (QuarterTurn)(((int)edit.Rotation + 1) % 4));
        }
        public PlacementResult ConfirmPlacement() => LastResult = Session.Map.Commands.Confirm();
        public PlacementResult CancelPlacement() => LastResult = Session.Map.Commands.Cancel();

        private void OnGUI()
        {
            if (Session == null)
                return;
            if (Event.current.type == EventType.Repaint)
                RepaintCount++;

            GUI.color = Color.white;
            GUI.Label(new Rect(24, 16, Screen.width - 48, 28), "MAP FOUNDATION / PC VERIFICATION");
            GUI.Label(new Rect(24, 44, Screen.width - 48, 24),
                "Select a facility > Move / Rotate > click a grid cell > Confirm or Cancel.");
            var map = Session.Map;
            var cellSize = Mathf.Max(12, Mathf.Min(52, (Screen.height - 170f) / map.Height,
                (Screen.width - 350f) / map.Width));
            var grid = new Rect(24, 90, cellSize * map.Width, cellSize * map.Height);
            DrawGrid(grid, cellSize);
            DrawPanel(new Rect(grid.xMax + 24, 90, 270, Mathf.Max(420, Screen.height - 100)));
            GUI.Label(new Rect(24, grid.yMax + 12, grid.width, 24),
                "Blue: facility   Orange: creature   I: fixed interaction   P: proposed origin");
            GUI.Label(new Rect(24, grid.yMax + 36, grid.width, 24),
                "Fixture only: 12 x 8, two stores, two creatures, five resources. No production clock.");
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
            var map = Session.Map;
            var placements = map.GetPlacements();
            var edit = map.Commands.Edit;
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                var rect = new Rect(grid.x + x * size, grid.y + (map.Height - 1 - y) * size, size - 2, size - 2);
                var facility = placements.FirstOrDefault(p => p.Kind == ThingKind.Facility && p.Contains(cell));
                var creatures = placements.Count(p => p.Kind == ThingKind.Creature && p.Contains(cell));
                var interaction = placements.FirstOrDefault(p => p.InteractionCell == cell);
                var label = facility != null ? "F" + facility.CreationOrder : creatures > 0 ? "C x" + creatures
                    : interaction != null ? "I" + interaction.CreationOrder : "";
                var tint = facility != null ? new Color(0.18f, 0.42f, 0.62f)
                    : creatures > 0 ? new Color(0.72f, 0.4f, 0.16f) : new Color(0.13f, 0.17f, 0.23f);
                if (edit != null && edit.Origin == cell)
                {
                    tint = edit.Validation.Succeeded ? new Color(0.12f, 0.55f, 0.32f) : new Color(0.65f, 0.15f, 0.18f);
                    label = "P";
                }
                GUI.color = tint;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                if (GUI.Button(rect, label, cellStyle))
                {
                    if (edit != null) PreviewAt(cell);
                    else if (facility != null) SelectThing(facility.ThingId);
                    else if (creatures > 0) SelectThing(placements.First(p => p.Kind == ThingKind.Creature && p.Contains(cell)).ThingId);
                }
            }
        }

        private void DrawPanel(Rect rect)
        {
            var map = Session.Map;
            var commands = map.Commands;
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("FACILITIES");
            GUI.enabled = !commands.IsEditing;
            foreach (var thing in Session.Facilities)
                if (GUILayout.Button("Store " + thing.CreationOrder + (thing.Id == SelectedId ? "  [selected]" : "")))
                    SelectThing(thing.Id);
            GUI.enabled = true;
            if (Session.Registry.TryGet(SelectedId, out var selected))
            {
                GUILayout.Space(12);
                GUILayout.Label("Selected: " + selected.Definition.Kind + " #" + selected.CreationOrder);
                GUILayout.Label("ID: " + selected.Id.ToString("N").Substring(0, 8));
                var placement = selected.Placement;
                GUILayout.Label(placement == null ? "Outside Map" : "Origin: " + placement.Origin
                    + " / " + ((int)placement.Rotation * 90) + " deg");
                if (placement?.InteractionCell is Vector2Int interaction)
                    GUILayout.Label("Interaction: " + interaction + (map.IsWalkable(interaction) ? " / open cell" : " / blocked cell"));
                if (selected.TryGetComponent<ResourceContainerComponent>(out var holder))
                    GUILayout.Label("Stored quantity: " + holder.Contents.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity));
                if (!commands.IsEditing && selected.Definition.Kind == ThingKind.Facility
                    && GUILayout.Button("Move / Rotate"))
                    BeginPlacement();
            }
            if (commands.Edit != null)
            {
                GUILayout.Space(12);
                GUILayout.Label("EDITING: original placement retained");
                GUILayout.Label("Proposed: " + commands.Edit.Origin + " / " + ((int)commands.Edit.Rotation * 90) + " deg");
                GUILayout.Label("Validation: " + commands.Edit.Validation);
                if (GUILayout.Button("Rotate 90 degrees")) RotatePreview();
                if (GUILayout.Button("Confirm")) ConfirmPlacement();
                if (GUILayout.Button("Cancel")) CancelPlacement();
            }
            GUILayout.Space(12);
            GUILayout.Label("Last command: " + LastResult);
            GUILayout.Label("Edit state: " + (commands.IsEditing ? "active" : "idle"));
            GUILayout.Label("Only the marked fixed interaction cell is used. Blocking a corridor is allowed.");
            GUILayout.EndArea();
        }
    }
}
