using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;
using Unity.Profiling;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private static readonly ProfilerMarker statusMarker = new ProfilerMarker("Laboratory.Status.Layout");
        private const float StatusWidth = 36, StatusHeight = 4, StatusGap = 2;
        private sealed class StatusMarker
        {
            public RectTransform Root;
            public UnityEngine.UI.Image Track, Fill;
            public Rect Bounds;
            public bool Visible;
        }

        private readonly Dictionary<Guid, StatusMarker> statusMarkers = new Dictionary<Guid, StatusMarker>();
        private readonly List<Guid> removedStatusIds = new List<Guid>();
        private RectTransform statusLayer;
        private int statusGeneration = -1;
        public int VisibleStatusCount { get; private set; }

        // Observation for diagnostics; the bar has no input handler or hit region.
        public bool TryStatusScreenPoint(Guid facilityId, out Vector2 point)
        {
            point = default;
            if (view == null || statusGeneration != view.Controller.Generation
                || !statusMarkers.TryGetValue(facilityId, out var marker) || !marker.Visible || !marker.Root.gameObject.activeInHierarchy)
                return false;
            point = RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(marker.Bounds.center));
            return true;
        }

        private void RefreshStatusOverlays()
        {
            using var layout = statusMarker.Auto();
            VisibleStatusCount = 0;
            var simulation = view?.Simulation;
            if (simulation == null || viewport == null)
            {
                if (statusLayer != null) statusLayer.gameObject.SetActive(false);
                return;
            }
            if (statusGeneration != view.Controller.Generation)
            {
                foreach (var marker in statusMarkers.Values) RemoveStatusMarker(marker);
                statusMarkers.Clear();
                statusGeneration = view.Controller.Generation;
            }
            removedStatusIds.Clear();
            foreach (var pair in statusMarkers)
            {
                if (!simulation.Registry.TryGet(pair.Key, out var thing) || !thing.IsPlaced)
                    removedStatusIds.Add(pair.Key);
                pair.Value.Visible = false;
            }
            foreach (var id in removedStatusIds)
            {
                RemoveStatusMarker(statusMarkers[id]);
                statusMarkers.Remove(id);
            }
            if (statusLayer == null)
            {
                statusLayer = new GameObject("Temp_FacilityStatusLayer", typeof(RectTransform)).GetComponent<RectTransform>();
                statusLayer.SetParent(viewport, false);
                statusLayer.anchorMin = statusLayer.anchorMax = statusLayer.pivot = Vector2.zero;
                statusLayer.anchoredPosition = Vector2.zero;
            }
            statusLayer.sizeDelta = viewport.rect.size;
            statusLayer.gameObject.SetActive(true);
            if (statusLayer.GetSiblingIndex() != statusLayer.parent.childCount - 1) statusLayer.SetAsLastSibling();
            foreach (var thing in simulation.Registry.Things)
                if (thing.IsPlaced && thing.Definition.Kind == ThingKind.Facility && thing.TryGetComponent<ConstructionComponent>(out _))
                    RefreshStatusMarker(thing);
            foreach (var marker in statusMarkers.Values)
                if (!marker.Visible) marker.Root.gameObject.SetActive(false);
        }

        private void RefreshStatusMarker(Thing thing)
        {
            var status = view.StatusFor(thing);
            if (!status.HasProgress)
            {
                if (statusMarkers.TryGetValue(thing.Id, out var completed))
                {
                    RemoveStatusMarker(completed);
                    statusMarkers.Remove(thing.Id);
                }
                return;
            }
            var placement = thing.Placement;
            var footprint = FacilityGeometry.GetSize((FacilityData)thing.Definition, placement.Rotation);
            var facilityRect = new Rect(LaboratoryWorldMap.WorldToLocal(LaboratoryWorldMap.CellToWorld(placement.Origin), Origin, CellSize), (Vector2)footprint * CellSize);
            if (!facilityRect.Overlaps(viewport.rect)) return;
            // Snap the anchor to a physical pixel while keeping size independent of map zoom and text scale.
            var position = new Vector2(facilityRect.center.x - StatusWidth * .5f, facilityRect.yMax + StatusGap);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(position));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport,
                new Vector2(Mathf.Round(screenPoint.x), Mathf.Round(screenPoint.y)), null, out position);
            var bounds = new Rect(position, new Vector2(StatusWidth, StatusHeight));
            // The viewport mask clips partial bars. Never relocate offscreen anchors to an edge.
            if (!bounds.Overlaps(viewport.rect)) return;
            if (!statusMarkers.TryGetValue(thing.Id, out var marker))
            {
                marker = CreateStatusMarker(thing.Id);
                statusMarkers.Add(thing.Id, marker);
            }
            marker.Bounds = bounds;
            marker.Visible = true;
            marker.Root.gameObject.SetActive(true);
            PlaceStatusRect(marker.Root, new Rect(bounds.position - viewport.rect.min, bounds.size));
            var working = status.Kind == FacilityStatusKind.ConstructionWorking || status.Kind == FacilityStatusKind.GatheringWorking
                || status.Kind == FacilityStatusKind.ProductionWorking || status.Kind == FacilityStatusKind.ProductionAutomatic;
            marker.Track.color = working ? new Color(.08f, .11f, .14f, .9f) : new Color(.34f, .23f, .10f, .95f);
            marker.Fill.color = working ? new Color(.38f, .88f, .7f) : new Color(.95f, .68f, .30f);
            PlaceStatusRect(marker.Track.rectTransform, new Rect(0, 0, StatusWidth, StatusHeight));
            PlaceStatusRect(marker.Fill.rectTransform, new Rect(1, 1, (StatusWidth - 2) * (float)status.ProgressFraction, StatusHeight - 2));
            VisibleStatusCount++;
        }

        private StatusMarker CreateStatusMarker(Guid id)
        {
            var root = new GameObject("Temp_FacilityStatus_" + id.ToString("N"), typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(statusLayer, false);
            var track = CreateStatusImage("Track", root, Color.black);
            return new StatusMarker { Root = root, Track = track, Fill = CreateStatusImage("Progress", track.transform, Color.white) };
        }

        private static UnityEngine.UI.Image CreateStatusImage(string name, Transform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            image.transform.SetParent(parent, false); image.color = color; image.raycastTarget = false;
            return image;
        }

        private static void PlaceStatusRect(RectTransform transform, Rect rect)
        {
            transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
            transform.anchoredPosition = rect.position; transform.sizeDelta = rect.size;
        }

        private static void RemoveStatusMarker(StatusMarker marker)
        {
            marker.Root.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(marker.Root.gameObject);
        }
    }
}
