using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public static class LaboratoryUiWidgets
    {
        public static readonly Color Background = new Color(0.035f, 0.055f, 0.08f);
        public static readonly Color PanelColor = new Color(0.075f, 0.105f, 0.15f);
        public static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            return rect;
        }
        public static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(Mathf.Max(0, width), Mathf.Max(0, height));
        }
        public static Image Panel(string name, Transform parent, Color color)
        { var image = Rect(name, parent).gameObject.AddComponent<Image>(); image.color = color; return image; }
        public static Text Label(string name, Transform parent, int fontSize = 18)
        {
            var label = Rect(name, parent).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize; label.color = new Color(0.88f, 0.92f, 0.96f); label.raycastTarget = false;
            label.alignment = TextAnchor.UpperLeft; label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate; label.supportRichText = false;
            label.gameObject.AddComponent<LaboratoryUiText>().Initialize(fontSize);
            return label;
        }
        public static LaboratoryUiButton Button(string name, Transform parent, string text, Func<int> epoch)
        {
            var image = Panel(name, parent, new Color(0.16f, 0.23f, 0.31f));
            var button = image.gameObject.AddComponent<LaboratoryUiButton>(); button.targetGraphic = image; button.Epoch = epoch;
            var colors = button.colors; colors.highlightedColor = new Color(0.85f, 0.93f, 1f);
            colors.pressedColor = new Color(0.55f, 0.8f, 1f); colors.disabledColor = new Color(0.42f, 0.45f, 0.5f); button.colors = colors;
            var label = Label("Label", button.transform); label.text = text; label.alignment = TextAnchor.MiddleCenter;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(8, 4); label.rectTransform.offsetMax = new Vector2(-8, -4);
            return button;
        }
        public static ScrollRect Scroll(string name, Transform parent, out RectTransform content)
        {
            var background = Panel(name, parent, PanelColor);
            var scroll = background.gameObject.AddComponent<ScrollRect>();
            var viewport = Rect("Viewport", background.transform);
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(8, 4); viewport.offsetMax = new Vector2(-8, -4);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", viewport);
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36;
            scroll.decelerationRate = 0.08f;
            return scroll;
        }
    }

    // Reuses rows by semantic key so state refreshes never replace a pressed button or reset scrolling.
    public sealed class LaboratoryUiList
    {
        private sealed class Row
        {
            public RectTransform Rect;
            public Text Text;
            public LaboratoryUiButton Button;
            public bool Seen;
        }
        private readonly Dictionary<string, Row> rows = new Dictionary<string, Row>();
        private readonly RectTransform content;
        private readonly Func<int> epoch;
        private float width;
        private float y;
        public LaboratoryUiList(RectTransform content, Func<int> epoch) { this.content = content; this.epoch = epoch; }
        public void Begin(float availableWidth)
        { width = Mathf.Max(80, availableWidth); y = 8; foreach (var row in rows.Values) row.Seen = false; }
        public void Text(string key, string value, int size = 18)
        {
            var row = Get(key, false, size); row.Text.text = value ?? "";
            LaboratoryUiWidgets.Place(row.Rect, 4, y, width - 8, 30);
            var height = Mathf.Max(size + 8, row.Text.preferredHeight + 10);
            row.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height); y += height + 4;
        }
        public void Action(string key, string label, Action action, bool enabled, string context)
        {
            var row = Get(key, true, 18); row.Text.text = label;
            row.Button.Bind(action, context); row.Button.interactable = enabled;
            var minimum = content.GetComponentInParent<LaboratoryView>()?.MinimumTouchSize ?? 48;
            LaboratoryUiWidgets.Place(row.Rect, 4, y, width - 8, minimum);
            var height = Mathf.Max(minimum, row.Text.preferredHeight + 14);
            row.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height); y += height + 8;
        }
        public void End()
        {
            foreach (var key in rows.Where(p => !p.Value.Seen).Select(p => p.Key).ToArray())
            {
                var row = rows[key]; row.Rect.gameObject.SetActive(false); UnityEngine.Object.Destroy(row.Rect.gameObject); rows.Remove(key);
            }
            content.sizeDelta = new Vector2(width, y + 8);
        }
        public string CaptureAnchor(string prefix, float scrollY, out float offset)
        {
            offset = 0;
            if (scrollY < 1) return null;
            var candidate = rows.Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)
                && -pair.Value.Rect.anchoredPosition.y + pair.Value.Rect.rect.height > scrollY)
                .OrderBy(pair => -pair.Value.Rect.anchoredPosition.y).FirstOrDefault();
            if (candidate.Value == null) return null;
            offset = scrollY + candidate.Value.Rect.anchoredPosition.y;
            return candidate.Key;
        }
        public bool TryRestoreAnchor(string key, float offset, out float scrollY)
        {
            scrollY = 0;
            if (key == null || !rows.TryGetValue(key, out var row)) return false;
            scrollY = Mathf.Max(0, -row.Rect.anchoredPosition.y + offset); return true;
        }
        private Row Get(string key, bool button, int size)
        {
            if (!rows.TryGetValue(key, out var row))
            {
                if (button)
                {
                    var action = LaboratoryUiWidgets.Button(key, content, "", epoch);
                    row = new Row { Rect = (RectTransform)action.transform, Button = action, Text = action.GetComponentInChildren<Text>() };
                }
                else
                {
                    var text = LaboratoryUiWidgets.Label(key, content, size);
                    row = new Row { Rect = text.rectTransform, Text = text };
                }
                rows.Add(key, row);
            }
            row.Seen = true; return row;
        }
    }
}
