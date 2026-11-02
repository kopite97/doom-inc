using UnityEngine;

namespace MobileGame.Presentation
{
    // Remembers the intended text size, so preference changes never multiply an already scaled size.
    public sealed class UiText : MonoBehaviour
    {
        public int BaseSize { get; private set; }
        private Font font;
        private float scale = 1;
        public void Initialize(int size) { BaseSize = size; Apply(font, scale); }
        public void Apply(Font value, float textScale)
        {
            font = value; scale = textScale;
            var text = GetComponent<UnityEngine.UI.Text>();
            text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(BaseSize * scale);
        }
    }
}
