using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileGame.Presentation
{
    public sealed class PopupHost : MonoBehaviour, IPopupHost
    {
        private UiCanvas screen;
        private Dictionary<string, GameObject> prefabs;
        private PopupRegistry common;
        private Func<bool> large;
        private bool blocked;
        private GameObject previousSelection;
        private GameObject currentSelectionRoot;
        public PopupManager Manager { get; private set; }
        public AppLocalization Texts { get; private set; }
        public TMP_FontAsset Font { get; private set; }
        public float TextScale => large?.Invoke() == true ? 1.25f : 1;
        public Vector2 Size { get { screen.RefreshLayout(); return screen.Size; } }
        public int LayoutEpoch => screen.LayoutEpoch;
        public float MinimumTouch => screen.MinimumTouchSize;
        public void Configure(PopupRegistry registry, AppLocalization texts, TMP_FontAsset font, Func<bool> largeText)
        {
            if (Manager != null) throw new InvalidOperationException("Popup host already configured.");
            common = registry; prefabs = Validate(registry, null); Texts = texts; Font = font; large = largeText;
            screen = new UiCanvas(transform, "Temp_PopupCanvas", 80);
            var shield = UiWidgets.Panel("ModalShield", screen.Canvas.transform, new Color(0, 0, 0, .65f)).rectTransform;
            shield.anchorMin = Vector2.zero; shield.anchorMax = Vector2.one; shield.offsetMin = shield.offsetMax = Vector2.zero;
            shield.SetAsFirstSibling();
            Manager = new PopupManager(this, error => Debug.LogWarning("Popup operation failed: " + error.Message));
            screen.SetVisible(false);
        }
        public static Dictionary<string, GameObject> Validate(PopupRegistry common, PopupRegistry scene)
        {
            if (common == null) throw new InvalidOperationException("Missing common popup registry.");
            var result = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (var registry in new[] { common, scene })
            {
                if (registry == null) continue;
                foreach (var item in registry.Entries)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Id) || item.Prefab == null
                        || item.Prefab.GetComponents<PopupBase>().Length != 1 || item.Prefab.activeSelf || !item.Prefab.GetComponent<PopupBase>().HasBindings)
                        throw new InvalidOperationException("Invalid popup resource; use an inactive prefab with one PopupBase.");
                    if (result.ContainsKey(item.Id)) throw new InvalidOperationException("Duplicate popup ID: " + item.Id);
                    result.Add(item.Id, item.Prefab);
                }
            }
            Require<ConfirmPopup>(result, "Confirm"); Require<NoticePopup>(result, "Notice"); Require<SettingsPopup>(result, "Settings");
            return result;
        }
        private static void Require<T>(Dictionary<string, GameObject> map, string id) where T : PopupBase
        { if (!map.TryGetValue(id, out var prefab) || prefab.GetComponent<T>() == null) throw new InvalidOperationException("Missing or incompatible popup: " + id); }
        public void RegisterScene(PopupRegistry registry) { var next = Validate(common, registry); prefabs = next; }
        public IPopupView Create(PopupRequest request, Action<PopupResult> choose)
        {
            if (!prefabs.TryGetValue(request.Id, out var prefab)) throw new InvalidOperationException("Unknown popup: " + request.Id);
            var instance = Instantiate(prefab, screen.SafeRoot);
            try { var popup = instance.GetComponent<PopupBase>(); popup.Initialize(this, request, choose); Invalidate(); return popup; }
            catch { Destroy(instance); throw; }
        }
        public void Remove(IPopupView view)
        {
            if (!(view is PopupBase popup) || popup == null) return;
            try { popup.Release(); } finally { Destroy(popup.gameObject); Invalidate(); }
        }
        public void SetBlocked(bool value)
        {
            if (screen == null || blocked == value) return;
            if (value) previousSelection = EventSystem.current?.currentSelectedGameObject;
            blocked = value; screen.SetVisible(value); Invalidate();
            if (!value && previousSelection != null && previousSelection.activeInHierarchy)
                EventSystem.current?.SetSelectedGameObject(previousSelection);
            if (!value) { previousSelection = null; currentSelectionRoot = null; }
        }
        public void Invalidate() { screen?.InvalidatePointers(); AppUiLifetime.Invalidate(); }
        private void LateUpdate()
        {
            if (!blocked || screen == null) return;
            if (screen.RefreshLayout()) Invalidate();
            if (!AppUiLifetime.AcceptInput || Manager.Top?.State != PopupState.Open || !Manager.Foreground || Manager.Suspended) return;
            var top = screen.SafeRoot.GetComponentsInChildren<PopupBase>().LastOrDefault();
            if (top == null) return;
            var selected = EventSystem.current?.currentSelectedGameObject;
            if (currentSelectionRoot != top.gameObject || selected == null || !selected.transform.IsChildOf(top.transform))
            {
                currentSelectionRoot = top.gameObject;
                var first = top.GetComponentsInChildren<UiButton>().FirstOrDefault(b => b.IsInteractable());
                if (first != null) EventSystem.current?.SetSelectedGameObject(first.gameObject);
            }
        }
        private void OnDestroy() { Manager?.Dispose(); Manager = null; screen?.Dispose(); screen = null; }
    }
}
