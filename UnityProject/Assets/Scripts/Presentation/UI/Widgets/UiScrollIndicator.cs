using UnityEngine;
using UnityEngine.UI;
namespace MobileGame.Presentation
{
    // Passive position indicator; dragging remains owned by the existing ScrollRect.
    public sealed class UiScrollIndicator : MonoBehaviour
    {
        private ScrollRect scroll; private Image thumb;
        public static void Attach(ScrollRect owner, Sprite sprite)
        {
            if (sprite == null || owner.GetComponent<UiScrollIndicator>() != null) return;
            var indicator = owner.gameObject.AddComponent<UiScrollIndicator>(); indicator.scroll = owner;
            indicator.thumb = UiWidgets.Panel("ScrollPosition", owner.transform, Color.white);
            UiWidgets.ApplySkin(indicator.thumb, sprite); indicator.thumb.raycastTarget = false;
            owner.onValueChanged.AddListener(indicator.Refresh);
            indicator.Refresh(Vector2.zero);
        }
        private void OnRectTransformDimensionsChange()
        { if (scroll != null && thumb != null) Refresh(Vector2.zero); }
        private void Refresh(Vector2 ignored)
        {
            var available = scroll.viewport.rect.height;
            var total = scroll.content.rect.height;
            thumb.enabled = total > available + 1 && available > 12;
            if (!thumb.enabled) return;
            var length = Mathf.Clamp(available * available / total, 12, available);
            var progress = 1 - Mathf.Clamp01(scroll.verticalNormalizedPosition);
            UiWidgets.Place(thumb.rectTransform, ((RectTransform)scroll.transform).rect.width - 7,
                4 + progress * (available - length), 6, length);
        }
        private void OnDestroy() { if (scroll != null) scroll.onValueChanged.RemoveListener(Refresh); }
    }
}
