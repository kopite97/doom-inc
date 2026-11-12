using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MobileGame.Presentation
{
    // Presentation geometry and device input only. All edits go through LaboratoryView commands.
    [DefaultExecutionOrder(100)]
    public sealed partial class LaboratoryMapView : MonoBehaviour
    {
        private LaboratoryView view;
        private RectTransform viewport;
        private LaboratoryWorldMap world;
        public LaboratoryWorldMap World => world;
        private readonly Dictionary<int, Vector2> pointers = new Dictionary<int, Vector2>();
        private readonly List<RaycastResult> hits = new List<RaycastResult>();
        private LaboratoryGesture gesture;
        private TMP_FontAsset font;
        private Vector2 center;
        private float zoom = 1;
        private Vector2 previousSize;
        private Rect previousSafe;
        private Vector2 previousScreen;
        private int generation = -1;
        private SessionPhase phase;
        private bool wasInteractive;
        private bool subscribed;
        private int touchFrame = -1;
        private int width, height;
        private bool ensureSelectionVisible;
        private Tween selectionPan;

        public void RequestSelectionVisibility()
        {
            StopSelectionPan();
            ensureSelectionVisible = true;
        }

        private void StopSelectionPan()
        {
            ensureSelectionVisible = false;
            selectionPan?.Kill();
            selectionPan = null;
        }
        public float Zoom => zoom;
        public int AcceptedTaps { get; private set; }
        public RectTransform Viewport => viewport;
        public void CancelInput()
        {
            gesture?.Cancel();
            if (view == null || !view.CanUseMap || view.OpenedPanel != LaboratoryPanel.Selection)
                StopSelectionPan();
        }
        public void Fit() { StopSelectionPan(); gesture?.Cancel(); zoom = 1; center = new Vector2(width, height) * .5f; ClampCenter(); }
        public void ZoomBy(float factor) => ZoomAt(factor, RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(viewport.rect.center)));

        public void Initialize(LaboratoryView owner)
        {
            if (view != null) return;
            view = owner; viewport = (RectTransform)transform;
            BindArtSimulation();
            compareSelectionChoices = CompareSelectionChoices;
            font = owner.UiFont;
            gesture = new LaboratoryGesture();
            gesture.Tapped += Tap;
            gesture.Panned += delta => Pan(delta);
            gesture.Zoomed += (ratio, point) => ZoomAt(ratio, point);
            SubscribeTouch();
        }
        private void OnEnable() { SubscribeTouch(); if (world != null) world.SetVisible(true); }
        private void SubscribeTouch()
        {
            if (subscribed || gesture == null || !isActiveAndEnabled) return;
            EnhancedTouchSupport.Enable(); subscribed = true;
            EnhancedTouch.onFingerDown += FingerDown; EnhancedTouch.onFingerMove += FingerMove; EnhancedTouch.onFingerUp += FingerUp;
        }
        private void OnDisable()
        {
            publishedGeometry = false;
            if (view != null && (view.Simulation == null || view.Ending?.IsEnding == true)) ReleaseArtSimulation();
            if (world != null) world.SetVisible(false);
            StopSelectionPan();
            gesture?.Cancel();
            foreach (var p in pointers) gesture?.End(p.Key, p.Value, true);
            pointers.Clear();
            if (!subscribed) return;
            EnhancedTouch.onFingerDown -= FingerDown; EnhancedTouch.onFingerMove -= FingerMove; EnhancedTouch.onFingerUp -= FingerUp;
            subscribed = false; EnhancedTouchSupport.Disable();
        }
        private void FingerDown(Finger finger)
        {
            CheckLayout(); touchFrame = Time.frameCount;
            var touch = finger.currentTouch; var id = touch.touchId;
            pointers[id] = touch.screenPosition; gesture.Begin(id, touch.screenPosition, OverMap(touch.screenPosition));
        }
        private void FingerMove(Finger finger)
        {
            CheckLayout(); touchFrame = Time.frameCount; var touch = finger.currentTouch;
            if (!pointers.ContainsKey(touch.touchId)) return;
            pointers[touch.touchId] = touch.screenPosition; gesture.Move(touch.touchId, touch.screenPosition);
        }
        private void FingerUp(Finger finger)
        {
            CheckLayout(); touchFrame = Time.frameCount; var touch = finger.currentTouch;
            if (pointers.Remove(touch.touchId)) gesture.End(touch.touchId, touch.screenPosition,
                touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled || !view.CanInteract);
        }
        private void Update()
        {
            if (view?.Simulation == null || gesture == null) return;
            CheckLayout();
            gesture.DragThreshold = Mathf.Max(10, (view.UiCanvas != null ? view.UiCanvas.scaleFactor : 1) * 12);
            PollDevices();
        }
        private void CheckLayout()
        {
            if (view?.Simulation == null || viewport == null) { gesture?.Cancel(); return; }
            var map = view.Simulation.Map;
            BindArtSimulation();
            if (generation != view.Controller.Generation || width != map.Width || height != map.Height)
            {
                StopSelectionPan(); gesture.Cancel(); generation = view.Controller.Generation;
                width = map.Width; height = map.Height; center = new Vector2(width, height) * 0.5f; zoom = 1;
            }
            var screenSize = new Vector2(Screen.width, Screen.height);
            if (previousSize != viewport.rect.size || previousScreen != screenSize || previousSafe != Screen.safeArea
                || phase != view.Controller.Phase || wasInteractive != view.CanInteract)
            {
                if (!view.CanUseMap) StopSelectionPan();
                else if (previousSize != viewport.rect.size && view.OpenedPanel == LaboratoryPanel.Selection)
                    RequestSelectionVisibility();
                gesture.Cancel(); previousSize = viewport.rect.size; previousScreen = screenSize;
                previousSafe = Screen.safeArea; phase = view.Controller.Phase; wasInteractive = view.CanInteract;
                ClampCenter();
            }
        }
        public void PublishFrame()
        {
            if (!isActiveAndEnabled) return;
            if (view?.Simulation == null || viewport == null) return;
            // Layout may have been updated after Update (e.g. the panel was collapsed).
            if (previousSize != viewport.rect.size)
            {
                gesture.Cancel(); previousSize = viewport.rect.size; ClampCenter();
                if (view.CanUseMap && view.OpenedPanel == LaboratoryPanel.Selection) RequestSelectionVisibility();
            }
            EnsureSelectionVisible();
            Draw();
            RefreshStatusOverlays();
            publishedOrigin = Origin; publishedCellSize = CellSize;
            publishedViewport = viewport.rect; publishedScreen = new Vector2(Screen.width, Screen.height);
            publishedMatrix = viewport.localToWorldMatrix; publishedSafeArea = Screen.safeArea;
            publishedCanvasScale = view.UiCanvas.scaleFactor;
            publishedGeneration = view.Controller.Generation; publishedGeometry = true;
        }
        private float CellSize => Mathf.Max(1, Mathf.Min((viewport.rect.width - 16) / Math.Max(1, width),
            (viewport.rect.height - 16) / Math.Max(1, height))) * zoom;
        private Vector2 Origin => viewport.rect.center - center * CellSize;
        private void EnsureSelectionVisible()
        {
            if (!ensureSelectionVisible) return;
            ensureSelectionVisible = false;
            if (!view.Simulation.Registry.TryGet(view.SelectedId, out var selected) || !selected.IsPlaced) return;
            var size = selected.Definition is FacilityData definition
                ? (Vector2)FacilityGeometry.GetSize(definition, selected.Placement.Rotation) : Vector2.one;
            var min = (Vector2)selected.Placement.Origin; var max = min + size;
            if (view.UiAssets?.Art != null && view.UiAssets.Art.HasPrototype(selected.Definition)) max.y += .5f;
            var half = viewport.rect.size / (2 * CellSize) - Vector2.one * .2f;
            var target = center;
            if (size.x > half.x * 2) target.x = (min.x + max.x) * .5f;
            else if (min.x < target.x - half.x) target.x = min.x + half.x;
            else if (max.x > target.x + half.x) target.x = max.x - half.x;
            if (size.y > half.y * 2) target.y = (min.y + max.y) * .5f;
            else if (min.y < target.y - half.y) target.y = min.y + half.y;
            else if (max.y > target.y + half.y) target.y = max.y - half.y;
            target = ClampedCenter(target);
            if (target == center) return;
            selectionPan = DOTween.To(() => center, value => center = value, target, .2f)
                .SetEase(Ease.OutQuad).SetUpdate(true).OnKill(() => selectionPan = null);
        }
        private void ClampCenter() => center = ClampedCenter(center);
        private Vector2 ClampedCenter(Vector2 point)
        {
            if (viewport == null || width == 0 || height == 0) return point;
            var half = viewport.rect.size / (2 * CellSize);
            return new Vector2(
                half.x * 2 >= width ? width * .5f : Mathf.Clamp(point.x, half.x, width - half.x),
                half.y * 2 >= height ? height * .5f : Mathf.Clamp(point.y, half.y, height - half.y));
        }
        public Vector2 CellScreenPoint(Vector2 cell)
        {
            var local = LaboratoryWorldMap.WorldToLocal(LaboratoryWorldMap.CellToWorld(cell + Vector2.one * .5f), Origin, CellSize);
            return RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(local));
        }
        private readonly List<Thing> selectionChoices = new List<Thing>();
        private readonly List<Guid> selectionIds = new List<Guid>();
        private Comparison<Thing> compareSelectionChoices;
        private Vector2 publishedOrigin, publishedScreen;
        private float publishedCellSize;
        private Rect publishedViewport;
        private Rect publishedSafeArea;
        private Matrix4x4 publishedMatrix;
        private float publishedCanvasScale;
        private int publishedGeneration;
        private bool publishedGeometry;
        private bool PublishedGeometryCurrent => publishedGeometry && view?.Simulation != null
            && publishedGeneration == view.Controller.Generation && publishedViewport == viewport.rect
            && publishedOrigin == Origin && publishedCellSize == CellSize
            && publishedMatrix == viewport.localToWorldMatrix && publishedSafeArea == Screen.safeArea
            && publishedCanvasScale == view.UiCanvas.scaleFactor
            && publishedScreen == new Vector2(Screen.width, Screen.height);

        private static int CompareSelectionChoices(Thing a, Thing b)
        {
            var kind = (a.Definition.Kind == ThingKind.Facility ? 0 : 1).CompareTo(b.Definition.Kind == ThingKind.Facility ? 0 : 1);
            return kind != 0 ? kind : a.CreationOrder.CompareTo(b.CreationOrder);
        }
        public bool TryDisplayedPosition(Guid id, out Vector2 position)
        {
            position = default;
            if (!PublishedGeometryCurrent || !thingArt.TryGetValue(id, out var item)
                || !view.Simulation.Registry.TryGet(id, out var current) || !current.IsPlaced) return false;
            position = item.Ground; return true;
        }
        public bool TryThingScreenPoint(Guid id, out Vector2 point)
        {
            point = default;
            if (!TryDisplayedPosition(id, out _) || !thingArt.TryGetValue(id, out var item) || !item.HitVisible) return false;
            point = item.HitBounds.center; return true;
        }
        private void PublishHitBounds(ThingArt item)
        {
            var bounds = item.Body.bounds;
            if (item.Cargo.gameObject.activeSelf) bounds.Encapsulate(item.Cargo.bounds);
            var min = world.Camera.WorldToScreenPoint(bounds.min);
            var max = world.Camera.WorldToScreenPoint(bounds.max);
            item.HitBounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            item.HitVisible = world.Camera.enabled && item.HitBounds.Overlaps(world.Camera.pixelRect);
        }
        public bool TryScreenCell(Vector2 point, out Vector2Int cell)
        {
            cell = default;
            if (!RectTransformUtility.RectangleContainsScreenPoint(viewport, point, null)) return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, point, null, out var local);
            var mapPoint = (local - Origin) / CellSize;
            cell = new Vector2Int(Mathf.FloorToInt(mapPoint.x), Mathf.FloorToInt(mapPoint.y));
            return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
        }
        private bool OverMap(Vector2 point)
        {
            if (!view.CanUseMap || !RectTransformUtility.RectangleContainsScreenPoint(viewport, point, null)) return false;
            if (EventSystem.current == null) return true;
            hits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            return hits.Count == 0 || hits[0].gameObject.transform == transform || hits[0].gameObject.transform.IsChildOf(transform);
        }
        private void Tap(Vector2 point)
        {
            if (!view.CanUseMap || !PublishedGeometryCurrent || !OverMap(point)) return;
            if (!view.IsEditing && TrySelectStatus(point)) { AcceptedTaps++; return; }
            var hasCell = TryScreenCell(point, out var cell);
            if (view.IsEditing && !hasCell) return;
            AcceptedTaps++;
            if (view.IsEditing) { view.PreviewAt(cell); return; }
            selectionChoices.Clear();
            foreach (var thing in placedArt)
            {
                if (!view.Simulation.Registry.TryGet(thing.Id, out var current) || !current.IsPlaced) continue;
                if (thing.Definition.Kind == ThingKind.Facility)
                {
                    if (hasCell && thingArt[thing.Id].PublishedPlacement.Contains(cell)) selectionChoices.Add(thing);
                }
                else if (thingArt.TryGetValue(thing.Id, out var item) && item.HitVisible && item.HitBounds.Contains(point))
                    selectionChoices.Add(thing);
            }
            if (selectionChoices.Count == 0) return;
            selectionChoices.Sort(compareSelectionChoices);
            selectionIds.Clear();
            foreach (var thing in selectionChoices) selectionIds.Add(thing.Id);
            view.ShowSelectionCandidates(selectionIds);
        }
        private void Pan(Vector2 pixels)
        {
            if (!view.CanUseMap) return;
            StopSelectionPan();
            center -= pixels / Mathf.Max(0.001f, view.UiCanvas.scaleFactor) / CellSize; ClampCenter();
        }
        private void ZoomAt(float ratio, Vector2 point)
        {
            if (!view.CanUseMap || !float.IsFinite(ratio) || ratio <= 0) return;
            StopSelectionPan();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, point, null, out var local);
            var before = (local - Origin) / CellSize;
            zoom = Mathf.Clamp(zoom * ratio, 1, 8);
            center = before - (local - viewport.rect.center) / CellSize; ClampCenter();
        }
        private void PollDevices()
        {
            if (subscribed)
            {
                var active = EnhancedTouch.activeTouches;
                foreach (var id in pointers.Keys.Where(id => id >= 0).ToArray())
                    if (!active.Any(t => t.touchId == id))
                    { var point = pointers[id]; pointers.Remove(id); gesture.End(id, point, true); }
            }
            if (touchFrame == Time.frameCount || pointers.Keys.Any(id => id >= 0)
                || (subscribed && EnhancedTouch.activeTouches.Count > 0))
            {
                if (pointers.TryGetValue(-1, out var last)) { pointers.Remove(-1); gesture.End(-1, last, true); }
                return;
            }
            var mouse = Mouse.current;
            if (mouse == null)
            {
                if (pointers.TryGetValue(-1, out var removed)) { pointers.Remove(-1); gesture.End(-1, removed, true); }
                return;
            }
            var mousePoint = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            { pointers[-1] = mousePoint; gesture.Begin(-1, mousePoint, OverMap(mousePoint)); }
            else if (mouse.leftButton.isPressed && pointers.ContainsKey(-1))
            { pointers[-1] = mousePoint; gesture.Move(-1, mousePoint); }
            if (!mouse.leftButton.isPressed && pointers.Remove(-1)) gesture.End(-1, mousePoint, !view.CanInteract);
            var wheel = mouse.scroll.ReadValue().y;
            if (wheel != 0 && !gesture.HasPointers && OverMap(mousePoint)) ZoomAt(Mathf.Exp(wheel * 0.001f), mousePoint);
        }
        private void Draw()
        {
            var map = view.Simulation.Map; if (width != map.Width || height != map.Height) return;
            if (world == null) world = new LaboratoryWorldMap(view.transform, view.UiAssets.WorldMaterial, view.UiCanvas);
            world.SyncCamera(viewport, center, CellSize);
            world.UpdateFloor(width, height, view.UiAssets);
            var edit = map.Commands.Edit;
            var constructing = view.Simulation.ConstructionDefinition != null;
            var previewOrigin = constructing ? view.Simulation.ConstructionOrigin : edit?.Origin;
            var previewDefinition = view.Simulation.ConstructionDefinition;
            if (previewDefinition == null && edit != null && view.Simulation.Registry.TryGet(edit.ThingId, out var moving)) previewDefinition = moving.Definition as FacilityData;
            var rotation = constructing ? view.Simulation.ConstructionRotation : edit?.Rotation ?? QuarterTurn.None;
            var valid = constructing ? view.Simulation.ConstructionValidation.Succeeded : edit?.Validation.Succeeded ?? false;
            DrawArt(previewDefinition, previewOrigin, rotation, valid);
        }
    }
}
