using TMPro;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Remembers the intended text size, so preference changes never multiply an already scaled size.
    public sealed class UiText : MonoBehaviour
    {
        public int BaseSize { get; private set; }
        private TMP_FontAsset font;
        private float scale = 1;
        public void Initialize(int size) { BaseSize = size; Apply(font, scale); }
        public void Apply(TMP_FontAsset value, float textScale)
        {
            font = value; scale = textScale;
            var text = GetComponent<TextMeshProUGUI>();
            text.font = font != null ? font : TMP_Settings.defaultFontAsset;
            text.fontSize = Mathf.RoundToInt(BaseSize * scale);
        }
    }
}
