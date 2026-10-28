using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    // A small runtime-owned screen shell. It does not own a game session.
    public sealed class LaboratoryUiCanvas : IDisposable
    {
        public Canvas Canvas { get; }
        public RectTransform SafeRoot { get; }
        // EventSystem processes releases before LateUpdate. Read current geometry at the event boundary.
        public int LayoutEpoch { get { RefreshLayout(); return layoutEpoch; } }
        public Vector2 Size => SafeRoot.sizeDelta;
        public bool Portrait => Size.y > Size.x;
        private Rect lastSafe;
        private Vector2 lastScreen;
        private bool initialized;
        private readonly CanvasScaler scaler;
        private static GameObject eventOwner;
        private static int users;
        private bool disposed;
        private int layoutEpoch;

        public LaboratoryUiCanvas(Transform parent, string name, int order)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            host.transform.SetParent(parent, false);
            Canvas = host.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceOverlay; Canvas.sortingOrder = order;
            scaler = host.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            SafeRoot = LaboratoryUiWidgets.Rect("SafeArea", host.transform);
            users++;
            EnsureEventSystem();
            RefreshLayout();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                eventOwner = new GameObject("LaboratoryEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                var module = eventOwner.GetComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();
            }
        }

        public bool RefreshLayout()
        {
            if (disposed) return false;
            EnsureEventSystem();
            var safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            var screen = new Vector2(Screen.width, Screen.height);
            if (initialized && lastSafe == safe && lastScreen == screen) return false;
            initialized = true; lastSafe = safe; lastScreen = screen;
            var area = CalculateSafeRect(safe, screen, out var scale);
            scaler.scaleFactor = scale; Canvas.scaleFactor = scale;
            SafeRoot.anchorMin = SafeRoot.anchorMax = new Vector2(0, 1);
            SafeRoot.pivot = new Vector2(0, 1);
            SafeRoot.anchoredPosition = new Vector2(area.x, -area.y);
            SafeRoot.sizeDelta = area.size;
            InvalidatePointers();
            return true;
        }

        public static Rect CalculateSafeRect(Rect safe, Vector2 screen, out float scale)
        {
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(Vector2.zero, screen);
            scale = Mathf.Clamp(Mathf.Min(safe.width, safe.height) / 600f, 0.5f, 3f);
            return new Rect(new Vector2(safe.xMin, screen.y - safe.yMax) / scale, safe.size / scale);
        }
        public void InvalidatePointers() { layoutEpoch++; }
        public void SetVisible(bool visible)
        {
            if (Canvas == null || Canvas.gameObject.activeSelf == visible) return;
            InvalidatePointers(); Canvas.gameObject.SetActive(visible);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; users = Mathf.Max(0, users - 1);
            if (Canvas != null) { Canvas.gameObject.SetActive(false); UnityEngine.Object.Destroy(Canvas.gameObject); }
            if (users == 0 && eventOwner != null) { eventOwner.SetActive(false); UnityEngine.Object.Destroy(eventOwner); eventOwner = null; }
        }
    }
}
