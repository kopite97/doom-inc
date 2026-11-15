using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private bool initializePreview;
        private Vector2 previewGrabOffset;
        private readonly List<SpriteRenderer> previewCells = new List<SpriteRenderer>();
        private readonly List<Vector2Int> invalidPreviewCells = new List<Vector2Int>();
        private GridMap previewDiagnosticMap;
        private long previewLayoutVersion;
        private FacilityData previewDiagnosticDefinition;
        private Guid? previewDiagnosticId;
        private Vector2Int previewDiagnosticOrigin;
        private QuarterTurn previewDiagnosticRotation;

        public void BeginPlacementPreview()
        {
            StopSelectionPan();
            gesture?.Cancel();
            initializePreview = true;
        }
        public void PreservePlacementPreview() => initializePreview = false;

        private bool TryPreview(out FacilityData definition, out Vector2Int origin, out QuarterTurn rotation, out Guid? id)
        {
            definition = view?.Simulation?.ConstructionDefinition;
            origin = default; rotation = default; id = null;
            if (view == null || !view.IsEditing) return false;
            if (definition != null)
            {
                origin = view.Simulation.ConstructionOrigin; rotation = view.Simulation.ConstructionRotation;
                return true;
            }
            var edit = view.Simulation.Map.Commands.Edit;
            if (edit == null || !view.Simulation.Registry.TryGet(edit.ThingId, out var thing)) return false;
            definition = thing.Definition as FacilityData; origin = edit.Origin; rotation = edit.Rotation; id = edit.ThingId;
            return definition != null;
        }

        private void InitializePlacementPreview()
        {
            if (!initializePreview) return;
            initializePreview = false;
            if (!TryPreview(out var definition, out var origin, out var rotation, out var id)) return;
            var size = FacilityGeometry.GetSize(definition, rotation);
            if (!id.HasValue)
            {
                origin = Vector2Int.RoundToInt(center - (Vector2)size * .5f);
                origin.x = Mathf.Clamp(origin.x, 0, Math.Max(0, width - size.x));
                origin.y = Mathf.Clamp(origin.y, 0, Math.Max(0, height - size.y));
                view.PreviewAt(origin);
            }
            // Reveal once, after the editing layout settles. Later layout changes preserve the candidate.
            var half = viewport.rect.size / (2 * CellSize) - Vector2.one * .2f;
            if (size.x > half.x * 2 || size.y > half.y * 2)
            {
                zoom = Mathf.Max(1, zoom * Mathf.Min(half.x * 2 / size.x, half.y * 2 / size.y));
                half = viewport.rect.size / (2 * CellSize) - Vector2.one * .2f;
            }
            var max = origin + size;
            if (origin.x < center.x - half.x || max.x > center.x + half.x
                || origin.y < center.y - half.y || max.y > center.y + half.y)
                center = (Vector2)origin + (Vector2)size * .5f;
            ClampCenter();
        }

        private Vector2 ScreenMapPoint(Vector2 point)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, point, null, out var local);
            return (local - Origin) / CellSize;
        }

        private bool CapturePreviewDrag(Vector2 point)
        {
            if (initializePreview || !PublishedGeometryCurrent || !TryPreview(out var definition, out var origin, out var rotation, out _)) return false;
            var size = FacilityGeometry.GetSize(definition, rotation);
            var min = CellScreenPoint((Vector2)origin - Vector2.one * .5f);
            var max = CellScreenPoint((Vector2)(origin + size) - Vector2.one * .5f);
            var bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            if (previewArt != null && previewArt.Root.gameObject.activeSelf)
            {
                var body = previewArt.Body.bounds;
                var bodyMin = world.Camera.WorldToScreenPoint(body.min);
                var bodyMax = world.Camera.WorldToScreenPoint(body.max);
                bounds = Rect.MinMaxRect(Mathf.Min(bounds.xMin, bodyMin.x), Mathf.Min(bounds.yMin, bodyMin.y),
                    Mathf.Max(bounds.xMax, bodyMax.x), Mathf.Max(bounds.yMax, bodyMax.y));
            }
            var minimum = 44 * view.UiCanvas.scaleFactor;
            var padding = new Vector2(Mathf.Max(0, minimum - bounds.width), Mathf.Max(0, minimum - bounds.height)) * .5f;
            bounds.min -= padding; bounds.max += padding;
            if (!bounds.Contains(point)) return false;
            StopSelectionPan();
            previewGrabOffset = ScreenMapPoint(point) - origin;
            return true;
        }

        private void DragPreview(Vector2 point)
        {
            if (!OverMap(point) || !TryPreview(out _, out var origin, out _, out _)) return;
            var next = Vector2Int.RoundToInt(ScreenMapPoint(point) - previewGrabOffset);
            if (next != origin) view.PreviewAt(next);
        }

        private void DrawPlacementCells(FacilityData definition, Vector2Int origin, QuarterTurn rotation)
        {
            var map = view.Simulation.Map;
            var id = view.Simulation.ConstructionDefinition == null ? map.Commands.Edit?.ThingId : null;
            var size = FacilityGeometry.GetSize(definition, rotation);
            if (previewDiagnosticMap != map || previewLayoutVersion != map.LayoutVersion || previewDiagnosticDefinition != definition
                || previewDiagnosticId != id || previewDiagnosticOrigin != origin || previewDiagnosticRotation != rotation)
            {
                if (id.HasValue) map.CheckPlacement(id.Value, origin, rotation, invalidPreviewCells);
                else map.CheckPlacement(definition, origin, rotation, invalidPreviewCells);
                previewDiagnosticMap = map; previewLayoutVersion = map.LayoutVersion; previewDiagnosticDefinition = definition;
                previewDiagnosticId = id; previewDiagnosticOrigin = origin; previewDiagnosticRotation = rotation;
            }
            var count = size.x * size.y;
            while (previewCells.Count < count)
            {
                var cell = world.CreateSprite("PlacementCell_" + previewCells.Count, previewArt.Root, 30001);
                cell.sprite = world.Solid;
                cell.transform.localScale = new Vector3(1.88f, 1.88f, 1);
                previewCells.Add(cell);
            }
            for (var i = 0; i < previewCells.Count; i++)
            {
                var cell = previewCells[i]; cell.gameObject.SetActive(i < count);
                if (i >= count) continue;
                var offset = new Vector2Int(i % size.x, i / size.x);
                cell.transform.localPosition = ((Vector2)offset + Vector2.one * .5f - (Vector2)size * .5f) * 2;
                cell.color = invalidPreviewCells.Contains(origin + offset)
                    ? new Color(1, .12f, .16f, .55f) : new Color(.2f, 1, .5f, .16f);
            }
        }
    }
}
