using UnityEngine;

namespace MobileGame.Presentation
{
    // Remembers the intended text size, so preference changes never multiply an already scaled size.
    public sealed class LaboratoryUiText : MonoBehaviour
    {
        public int BaseSize { get; private set; }
        public void Initialize(int size) { BaseSize = size; Apply(GetComponentInParent<LaboratoryView>()); }
        public void Apply(LaboratoryView view)
        {
            var text = GetComponent<UnityEngine.UI.Text>();
            text.font = view != null ? view.UiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(BaseSize * (view != null ? view.UiTextScale : 1));
        }
    }
}
