using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MobileGame.Presentation
{
    // Presentation geometry and device input only. All edits go through LaboratoryView commands.
    public sealed class LaboratoryMapView : MonoBehaviour
    {
        private LaboratoryView view;
        private RectTransform viewport;
        private readonly List<Image> cells = new List<Image>();
        private readonly List<Text> labels = new List<Text>();
        private readonly Dictionary<int, Vector2> pointers = new Dictionary<int, Vector2>();
        private readonly List<RaycastResult> hits = new List<RaycastResult>();
        private LaboratoryGesture gesture;
        private Font font;
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
        public float Zoom => zoom;
        public int AcceptedTaps { get; private set; }
        public RectTransform Viewport => viewport;
        public void CancelInput() => gesture?.Cancel();
        public void Fit() { gesture?.Cancel(); zoom = 1; center = new Vector2(width, height) * .5f; ClampCenter(); }
        public void ZoomBy(float factor) => ZoomAt(factor, RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(viewport.rect.center)));

        public void Initialize(LaboratoryView owner)
        {
            if (view != null) return;
            view = owner; viewport = (RectTransform)transform;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            gesture = new LaboratoryGesture();
            gesture.Tapped += Tap;
            gesture.Panned += delta => Pan(delta);
            gesture.Zoomed += (ratio, point) => ZoomAt(ratio, point);
            SubscribeTouch();
        }
        private void OnEnable() => SubscribeTouch();
        private void SubscribeTouch()
        {
            if (subscribed || gesture == null || !isActiveAndEnabled) return;
            EnhancedTouchSupport.Enable(); subscribed = true;
            EnhancedTouch.onFingerDown += FingerDown; EnhancedTouch.onFingerMove += FingerMove; EnhancedTouch.onFingerUp += FingerUp;
        }
        private void OnDisable()
        {
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
            if (generation != view.Controller.Generation || width != map.Width || height != map.Height)
            {
                gesture.Cancel(); generation = view.Controller.Generation;
                width = map.Width; height = map.Height; center = new Vector2(width, height) * 0.5f; zoom = 1;
            }
            var screenSize = new Vector2(Screen.width, Screen.height);
            if (previousSize != viewport.rect.size || previousScreen != screenSize || previousSafe != Screen.safeArea
                || phase != view.Controller.Phase || wasInteractive != view.CanInteract)
            {
                gesture.Cancel(); previousSize = viewport.rect.size; previousScreen = screenSize;
                previousSafe = Screen.safeArea; phase = view.Controller.Phase; wasInteractive = view.CanInteract;
                ClampCenter();
            }
        }
        private void LateUpdate()
        {
            if (view?.Simulation == null || viewport == null) return;
            // Layout may have been updated after Update (e.g. the panel was collapsed).
            if (previousSize != viewport.rect.size) { gesture.Cancel(); previousSize = viewport.rect.size; ClampCenter(); }
            Draw();
        }
        private float CellSize => Mathf.Max(1, Mathf.Min((viewport.rect.width - 16) / Math.Max(1, width),
            (viewport.rect.height - 16) / Math.Max(1, height))) * zoom;
        private Vector2 Origin => viewport.rect.center - center * CellSize;
        private void ClampCenter()
        {
            if (viewport == null || width == 0 || height == 0) return;
            var half = viewport.rect.size / (2 * CellSize);
            center.x = half.x * 2 >= width ? width * 0.5f : Mathf.Clamp(center.x, half.x, width - half.x);
            center.y = half.y * 2 >= height ? height * 0.5f : Mathf.Clamp(center.y, half.y, height - half.y);
        }
        public Vector2 CellScreenPoint(Vector2 cell)
        {
            var local = Origin + (cell + Vector2.one * 0.5f) * CellSize;
            return RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(local));
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
            if (!view.CanInteract || !RectTransformUtility.RectangleContainsScreenPoint(viewport, point, null)) return false;
            if (EventSystem.current == null) return true;
            hits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            return hits.Count == 0 || hits[0].gameObject.transform == transform || hits[0].gameObject.transform.IsChildOf(transform);
        }
        private void Tap(Vector2 point)
        {
            if (!view.CanInteract || !TryScreenCell(point, out var cell)) return;
            AcceptedTaps++;
            if (view.IsEditing) { view.PreviewAt(cell); return; }
            var choices = view.Simulation.Map.GetPlacements().Where(p => p.Contains(cell)).OrderBy(p =>
                p.Kind == ThingKind.Facility ? 0 : 1).ThenBy(p => view.Simulation.Registry.Things.First(t => t.Id == p.ThingId).CreationOrder).ToArray();
            if (choices.Length == 0) return;
            var index = Array.FindIndex(choices, p => p.ThingId == view.SelectedId);
            view.SelectThing(choices[(index + 1) % choices.Length].ThingId);
        }
        private void Pan(Vector2 pixels)
        {
            if (!view.CanInteract) return;
            center -= pixels / Mathf.Max(0.001f, view.UiCanvas.scaleFactor) / CellSize; ClampCenter();
        }
        private void ZoomAt(float ratio, Vector2 point)
        {
            if (!view.CanInteract || !float.IsFinite(ratio) || ratio <= 0) return;
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
            while (cells.Count < width * height)
            {
                var go = new GameObject("Temp_MapCell", typeof(RectTransform), typeof(Image)); go.transform.SetParent(transform, false);
                var graphic = go.GetComponent<Image>(); graphic.raycastTarget = false; cells.Add(graphic);
                var labelObject = new GameObject("Temp_CellLabel", typeof(RectTransform), typeof(Text)); labelObject.transform.SetParent(go.transform, false);
                var text = labelObject.GetComponent<Text>(); text.font = font; text.alignment = TextAnchor.MiddleCenter;
                text.raycastTarget = false; text.color = Color.white; text.horizontalOverflow = HorizontalWrapMode.Overflow;
                var rect = text.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                labels.Add(text);
            }
            var placements = map.GetPlacements(); var edit = map.Commands.Edit;
            var constructing = view.Simulation.ConstructionDefinition != null;
            var previewOrigin = constructing ? view.Simulation.ConstructionOrigin : edit?.Origin;
            var previewDefinition = view.Simulation.ConstructionDefinition;
            if (previewDefinition == null && edit != null && view.Simulation.Registry.TryGet(edit.ThingId, out var moving)) previewDefinition = moving.Definition as FacilityData;
            var rotation = constructing ? view.Simulation.ConstructionRotation : edit?.Rotation ?? QuarterTurn.None;
            var valid = constructing ? view.Simulation.ConstructionValidation.Succeeded : edit?.Validation.Succeeded ?? false;
            var area = previewDefinition != null && previewOrigin.HasValue ? new RectInt(previewOrigin.Value, FacilityGeometry.GetSize(previewDefinition, rotation)) : new RectInt();
            var interaction = previewDefinition != null && previewOrigin.HasValue ? (Vector2Int?)FacilityGeometry.GetInteractionCell(previewDefinition, previewOrigin.Value, rotation) : null;
            var size = CellSize; var origin = Origin;
            for (var i = 0; i < cells.Count; i++)
            {
                var active = i < width * height; cells[i].gameObject.SetActive(active); if (!active) continue;
                var point = new Vector2Int(i % width, i / width); var rect = cells[i].rectTransform;
                rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = Vector2.zero;
                rect.anchoredPosition = origin - viewport.rect.min + (Vector2)point * size; rect.sizeDelta = Vector2.one * Mathf.Max(1, size - 2);
                var facility = placements.FirstOrDefault(p => p.Kind == ThingKind.Facility && p.Contains(point));
                var creatures = placements.Where(p => p.Kind == ThingKind.Creature && p.Contains(point)).ToArray();
                var label = placements.Any(p => p.InteractionCell == point) ? "I" : "";
                var color = new Color(.12f, .16f, .21f);
                if (facility != null && view.Simulation.Registry.TryGet(facility.ThingId, out var thing))
                {
                    label = ((FacilityData)thing.Definition).Role.ToString().Substring(0, 1) + thing.CreationOrder;
                    color = thing.GetComponent<ConstructionComponent>().IsComplete ? new Color(.18f, .42f, .62f) : new Color(.56f, .43f, .12f);
                    if (view.SelectedId == thing.Id) color *= 1.5f;
                }
                else if (creatures.Length > 0)
                {
                    var creatureThing = view.Simulation.Registry.Things.First(t => t.Id == creatures[0].ThingId);
                    label = ((CreatureData)creatureThing.Definition).Role.ToString().Substring(0, 1) + creatureThing.CreationOrder + (creatures.Length > 1 ? "+" : "");
                    color = new Color(.7f, .4f, .15f); if (creatures.Any(p => p.ThingId == view.SelectedId)) color *= 1.4f;
                }
                if (previewOrigin.HasValue && area.Contains(point)) { label = "+"; color = valid ? new Color(.12f, .55f, .32f) : new Color(.65f, .15f, .18f); }
                if (interaction == point) { label = "I"; color = new Color(.5f, .4f, .65f); }
                cells[i].color = color; labels[i].text = label; labels[i].fontSize = Mathf.Clamp((int)(size * .32f), 9, 24);
            }
        }
    }
}
