using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public static class UiWidgets
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
        public static TextMeshProUGUI Label(string name, Transform parent, int fontSize = 18)
        {
            var label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.color = new Color(0.88f, 0.92f, 0.96f); label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.TopLeft; label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Truncate; label.richText = false;
            label.gameObject.AddComponent<UiText>().Initialize(fontSize);
            return label;
        }
        public static UiButton Button(string name, Transform parent, string text, Func<int> epoch)
        {
            var image = Panel(name, parent, new Color(0.16f, 0.23f, 0.31f));
            var button = image.gameObject.AddComponent<UiButton>(); button.targetGraphic = image; button.Epoch = epoch;
            var colors = button.colors; colors.highlightedColor = new Color(0.85f, 0.93f, 1f);
            colors.pressedColor = new Color(0.55f, 0.8f, 1f); colors.disabledColor = new Color(0.42f, 0.45f, 0.5f); button.colors = colors;
            var label = Label("Label", button.transform); label.text = text; label.alignment = TextAlignmentOptions.Center;
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

}
