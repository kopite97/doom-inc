using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    // Reuses rows by semantic key so state refreshes never replace a pressed button or reset scrolling.
    public sealed class UiList
    {
        private sealed class Row
        {
            public RectTransform Rect;
            public TextMeshProUGUI Text;
            public UiButton Button;
            public bool Seen;
        }
        private readonly Dictionary<string, Row> rows = new Dictionary<string, Row>();
        private readonly RectTransform content;
        private readonly Func<int> epoch;
        private readonly Action<UiButton> configureButton;
        private float width;
        private float y;
        private TMP_FontAsset font;
        private float scale = 1;
        private float touchSize = 48;
        public void Style(TMP_FontAsset value, float textScale, float minimumTouch)
        { font = value; scale = textScale; touchSize = minimumTouch; }
        public UiList(RectTransform content, Func<int> epoch, Action<UiButton> configureButton = null)
        {
            this.content = content;
            this.epoch = epoch;
            this.configureButton = configureButton;
        }
        public void Begin(float availableWidth)
        { width = Mathf.Max(80, availableWidth); y = 8; foreach (var row in rows.Values) row.Seen = false; }
        public void Text(string key, string value, int size = 18)
        {
            var row = Get(key, false, size); row.Text.text = value ?? "";
            UiWidgets.Place(row.Rect, 4, y, width - 8, row.Rect.rect.height);
            var height = Mathf.Max(size + 8, row.Text.preferredHeight + 10);
            row.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height); y += height + 4;
        }
        public void Action(string key, string label, Action action, bool enabled, string context, int fontSize = 18)
        {
            var row = Get(key, true, fontSize); row.Text.text = label;
            row.Button.Bind(action, context); row.Button.interactable = enabled;
            var minimum = touchSize;
            UiWidgets.Place(row.Rect, 4, y, width - 8, row.Rect.rect.height);
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
                    var action = UiWidgets.Button(key, content, "", epoch);
                    configureButton?.Invoke(action);
                    row = new Row { Rect = (RectTransform)action.transform, Button = action, Text = action.GetComponentInChildren<TextMeshProUGUI>(true) };
                }
                else
                {
                    var text = UiWidgets.Label(key, content, size);
                    row = new Row { Rect = text.rectTransform, Text = text };
                }
                rows.Add(key, row);
            }
            var style = row.Text.GetComponent<UiText>();
            style.Initialize(size);
            style.Apply(font, scale);
            row.Seen = true; return row;
        }
    }
}
