using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private sealed class StatusMarker
        {
            public RectTransform Root;
            public UnityEngine.UI.Image Background, Icon, Track, Fill, Connection;
            public UnityEngine.UI.Text Caption;
            public Rect Bounds;
            public bool Visible;
            // Transient map observation preference; null retains automatic selected/nearby captions.
            public bool? CaptionOverride;
        }

        private readonly Dictionary<Guid, StatusMarker> statusMarkers = new Dictionary<Guid, StatusMarker>();
        private readonly List<Guid> removedStatusIds = new List<Guid>();
        private readonly List<Rect> occupiedStatusBounds = new List<Rect>();
        private readonly List<Thing> statusFacilities = new List<Thing>();
        private RectTransform statusLayer;
        private int statusGeneration = -1;
        private bool statusLayoutOverflow;
        public int VisibleStatusCount { get; private set; }

        public bool TryStatusScreenPoint(Guid facilityId, out Vector2 point)
        {
            point = default;
            if (view == null || !view.ShowFacilityStatus || statusGeneration != view.Controller.Generation
                || !statusMarkers.TryGetValue(facilityId, out var marker) || !marker.Visible || !marker.Root.gameObject.activeInHierarchy)
                return false;
            point = RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(marker.Bounds.center));
            return true;
        }

        private void RefreshStatusOverlays()
        {
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
            if (!view.ShowFacilityStatus)
            {
                if (statusLayer != null) statusLayer.gameObject.SetActive(false);
                return;
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
            statusLayer.SetAsLastSibling();
            occupiedStatusBounds.Clear();
            statusFacilities.Clear();
            foreach (var thing in simulation.Registry.Things)
                if (thing.IsPlaced && thing.Definition.Kind == ThingKind.Facility && thing.TryGetComponent<ConstructionComponent>(out _))
                    statusFacilities.Add(thing);
            // A selected caption gets first claim on space; other identities retain creation order.
            statusFacilities.Sort(CompareStatusFacilities);
            statusLayoutOverflow = false;
            foreach (var thing in statusFacilities) RefreshStatusMarker(thing, false);
            if (statusLayoutOverflow)
            {
                // If the remaining icons cannot fit around existing bars, repack non-selected markers as icons.
                occupiedStatusBounds.Clear(); VisibleStatusCount = 0;
                foreach (var marker in statusMarkers.Values) marker.Visible = false;
                foreach (var thing in statusFacilities) RefreshStatusMarker(thing, true);
            }
            foreach (var marker in statusMarkers.Values)
                if (!marker.Visible)
                {
                    marker.Root.gameObject.SetActive(false);
                    marker.Connection.gameObject.SetActive(false);
                }
        }

        private int CompareStatusFacilities(Thing left, Thing right)
        {
            if (left.Id == view.SelectedId) return right.Id == left.Id ? 0 : -1;
            if (right.Id == view.SelectedId) return 1;
            return left.CreationOrder.CompareTo(right.CreationOrder);
        }

        private void RefreshStatusMarker(Thing thing, bool compact)
        {
            var status = view.StatusFor(thing);
            // Completed storage has no production cycle. A storage construction site still has a construction bar.
            if (status.Kind == FacilityStatusKind.Storage)
            {
                if (statusMarkers.TryGetValue(thing.Id, out var completedStorage))
                {
                    RemoveStatusMarker(completedStorage);
                    statusMarkers.Remove(thing.Id);
                }
                return;
            }
            var placement = thing.Placement;
            var footprint = FacilityGeometry.GetSize((FacilityData)thing.Definition, placement.Rotation);
            var facilityRect = new Rect(Origin + (Vector2)placement.Origin * CellSize, (Vector2)footprint * CellSize);
            if (!facilityRect.Overlaps(viewport.rect)) return;
            var selected = thing.Id == view.SelectedId;
            var showProgress = status.HasProgress && (selected || !compact);
            if (!statusMarkers.TryGetValue(thing.Id, out var marker))
            {
                marker = CreateStatusMarker(thing.Id);
                statusMarkers.Add(thing.Id, marker);
            }
            var caption = marker.CaptionOverride ?? (selected || (!compact && zoom >= 1.65f));
            var markerWidth = caption ? 100f : showProgress ? 72f : 26f;
            marker.Caption.font = view.UiFont;
            marker.Caption.fontSize = Mathf.RoundToInt(12 * view.UiTextScale);
            marker.Caption.text = LaboratoryStatusText.Short(status) + (status.HasProgress
                ? "\n진행 " + Mathf.RoundToInt((float)status.ProgressFraction * 100) + "%" : "");
            PlaceStatusRect(marker.Caption.rectTransform, new Rect(4, 0, markerWidth - 8, 20));
            var captionHeight = caption ? Mathf.Ceil(marker.Caption.preferredHeight) + 4 : 0;
            var markerHeight = 24 + captionHeight;
            var anchor = new Vector2(Mathf.Clamp(facilityRect.center.x, viewport.rect.xMin, viewport.rect.xMax),
                Mathf.Clamp(facilityRect.yMax, viewport.rect.yMin, viewport.rect.yMax));
            var desired = new Rect(anchor.x - markerWidth * .5f, anchor.y + 3, markerWidth, markerHeight);
            var fits = FindFreeStatusBounds(desired, out var bounds);
            if (!fits && caption && !selected)
            {
                // At density, discard extra words before sacrificing the progress bar or state icon.
                caption = false; captionHeight = 0; markerHeight = 24;
                markerWidth = showProgress ? 72 : 26;
                desired = new Rect(anchor.x - markerWidth * .5f, anchor.y + 3, markerWidth, markerHeight);
                fits = FindFreeStatusBounds(desired, out bounds);
            }
            if (!fits && !selected)
            {
                // Final density fallback keeps a readable state icon; selection restores its bar and exact progress.
                caption = false; captionHeight = 0; markerHeight = 24; markerWidth = 26; showProgress = false;
                desired = new Rect(anchor.x - markerWidth * .5f, anchor.y + 3, markerWidth, markerHeight);
                fits = FindFreeStatusBounds(desired, out bounds);
            }
            if (!fits)
            {
                statusLayoutOverflow = true;
                if (!selected) return;
            }
            marker.Bounds = bounds;
            occupiedStatusBounds.Add(bounds);
            marker.Visible = true;
            marker.Root.gameObject.SetActive(true);
            PlaceStatusRect(marker.Root, new Rect(bounds.position - viewport.rect.min, bounds.size));
            marker.Background.color = selected ? new Color(.14f, .24f, .34f, .98f) : new Color(.035f, .065f, .095f, .96f);
            marker.Icon.sprite = view.UiAssets?.GetIcon(LaboratoryStatusText.Icon(status));
            marker.Icon.color = Color.white;
            marker.Icon.preserveAspect = true;
            PlaceStatusRect(marker.Icon.rectTransform, new Rect(4, markerHeight - 21, 18, 18));
            marker.Track.gameObject.SetActive(showProgress);
            if (showProgress)
            {
                var barWidth = markerWidth - 28;
                PlaceStatusRect(marker.Track.rectTransform, new Rect(24, markerHeight - 14, barWidth, 4));
                PlaceStatusRect(marker.Fill.rectTransform, new Rect(0, 0, barWidth * (float)status.ProgressFraction, 4));
                marker.Fill.color = StatusProgressColor(status);
            }
            marker.Caption.gameObject.SetActive(caption);
            PlaceStatusRect(marker.Caption.rectTransform, new Rect(4, 2, markerWidth - 8, captionHeight));
            UpdateStatusConnection(marker, anchor, bounds, selected);
            VisibleStatusCount++;
        }

        private bool FindFreeStatusBounds(Rect desired, out Rect result)
        {
            result = ClampStatusRect(desired);
            if (!StatusOverlaps(result)) return true;
            var bestDistance = float.PositiveInfinity;
            var found = false;
            var stepX = desired.width + 3;
            var stepY = desired.height + 3;
            var rows = Mathf.CeilToInt(viewport.rect.height / stepY);
            var columns = Mathf.CeilToInt(viewport.rect.width / stepX);
            // Bounded, stable nearest-slot search keeps dense bars readable during pans and rotation.
            for (var row = -rows; row <= rows; row++)
            for (var column = -columns; column <= columns; column++)
            {
                var candidate = ClampStatusRect(new Rect(desired.position + new Vector2(column * stepX, row * stepY), desired.size));
                var distance = (candidate.center - desired.center).sqrMagnitude;
                if (distance >= bestDistance || StatusOverlaps(candidate)) continue;
                result = candidate; bestDistance = distance; found = true;
            }
            return found;
        }

        private Rect ClampStatusRect(Rect rect)
        {
            var area = viewport.rect;
            rect.x = Mathf.Clamp(rect.x, area.xMin + 2, Mathf.Max(area.xMin + 2, area.xMax - rect.width - 2));
            rect.y = Mathf.Clamp(rect.y, area.yMin + 2, Mathf.Max(area.yMin + 2, area.yMax - rect.height - 2));
            return rect;
        }

        private bool StatusOverlaps(Rect candidate)
        {
            var padding = new Rect(candidate.x - 1, candidate.y - 1, candidate.width + 2, candidate.height + 2);
            foreach (var occupied in occupiedStatusBounds) if (padding.Overlaps(occupied)) return true;
            return false;
        }

        private void UpdateStatusConnection(StatusMarker marker, Vector2 anchor, Rect bounds, bool selected)
        {
            var end = new Vector2(Mathf.Clamp(anchor.x, bounds.xMin + 2, bounds.xMax - 2),
                Mathf.Clamp(anchor.y, bounds.yMin, bounds.yMax));
            var delta = end - anchor;
            marker.Connection.gameObject.SetActive(delta.sqrMagnitude > 4);
            if (delta.sqrMagnitude <= 4) return;
            var rect = marker.Connection.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = new Vector2(0, .5f);
            rect.anchoredPosition = anchor - viewport.rect.min;
            rect.sizeDelta = new Vector2(delta.magnitude, selected ? 2 : 1);
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            marker.Connection.color = selected ? new Color(.65f, .9f, 1, .95f) : new Color(.48f, .64f, .75f, .8f);
        }

        private bool TrySelectStatus(Vector2 screenPoint)
        {
            if (view == null || !view.CanUseMap || view.IsEditing || !view.ShowFacilityStatus || statusGeneration != view.Controller.Generation
                || !RectTransformUtility.RectangleContainsScreenPoint(viewport, screenPoint, null)) return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, screenPoint, null, out var point);
            Guid selected = Guid.Empty;
            var distance = float.PositiveInfinity;
            foreach (var pair in statusMarkers)
            {
                var marker = pair.Value;
                if (!marker.Visible || !marker.Bounds.Contains(point)) continue;
                var candidateDistance = (marker.Bounds.center - point).sqrMagnitude;
                if (candidateDistance >= distance) continue;
                selected = pair.Key; distance = candidateDistance;
            }
            if (selected == Guid.Empty) return false;
            var selectedMarker = statusMarkers[selected];
            if (view.SelectedId == selected)
            {
                // Repeated marker taps affect only its caption, preserving the panel and its scroll context.
                selectedMarker.CaptionOverride = !(selectedMarker.CaptionOverride ?? selectedMarker.Caption.gameObject.activeSelf);
                return true;
            }
            if (!view.SelectThing(selected)) return false;
            selectedMarker.CaptionOverride = true;
            return true;
        }

        private StatusMarker CreateStatusMarker(Guid id)
        {
            var background = CreateStatusImage("Temp_FacilityStatus_" + id.ToString("N"), statusLayer, Color.black);
            var caption = new GameObject("Caption", typeof(RectTransform), typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();
            caption.transform.SetParent(background.transform, false);
            caption.raycastTarget = false; caption.color = Color.white; caption.alignment = TextAnchor.MiddleCenter;
            caption.horizontalOverflow = HorizontalWrapMode.Wrap; caption.verticalOverflow = VerticalWrapMode.Truncate;
            caption.supportRichText = false;
            var track = CreateStatusImage("Track", background.transform, new Color(.2f, .27f, .33f));
            var connection = CreateStatusImage("Temp_StatusConnection_" + id.ToString("N"), statusLayer, Color.white);
            connection.transform.SetAsFirstSibling();
            return new StatusMarker
            {
                Root = background.rectTransform, Background = background,
                Icon = CreateStatusImage("Icon", background.transform, Color.white), Track = track,
                Fill = CreateStatusImage("Progress", track.transform, Color.cyan), Caption = caption,
                Connection = connection
            };
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

        private static Color StatusProgressColor(FacilityStatus status) => status.Kind == FacilityStatusKind.Unreachable
            ? new Color(1, .52f, .4f) : status.Outstanding > 0 ? new Color(.94f, .8f, .35f)
            : status.IsAutomatic ? new Color(.65f, .78f, 1) : new Color(.38f, .88f, .7f);

        private static void RemoveStatusMarker(StatusMarker marker)
        {
            marker.Root.gameObject.SetActive(false); marker.Connection.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(marker.Root.gameObject); UnityEngine.Object.Destroy(marker.Connection.gameObject);
        }
    }
}
