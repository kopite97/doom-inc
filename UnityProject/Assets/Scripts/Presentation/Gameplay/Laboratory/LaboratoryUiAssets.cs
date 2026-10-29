using System;
using UnityEngine;

namespace MobileGame.Presentation
{
    [Serializable]
    public sealed class LaboratoryUiAssets
    {
        private static readonly LaboratoryIcon[] KnownIcons = (LaboratoryIcon[])Enum.GetValues(typeof(LaboratoryIcon));
        [SerializeField] private Font font;
        [SerializeField] private TextAsset fontLicense;
        [SerializeField] private Sprite[] icons;

        public Font Font => font;
        public TextAsset FontLicense => fontLicense;
        public bool IsComplete
        {
            get
            {
                if (font == null || fontLicense == null) return false;
                foreach (var icon in KnownIcons)
                    if (icon != LaboratoryIcon.None && GetIcon(icon) == null) return false;
                return true;
            }
        }

        public LaboratoryUiAssets(Font font, Sprite[] icons, TextAsset fontLicense = null)
        {
            this.font = font;
            this.fontLicense = fontLicense;
            this.icons = icons == null ? Array.Empty<Sprite>() : (Sprite[])icons.Clone();
        }

        public Sprite GetIcon(LaboratoryIcon icon)
        {
            var index = (int)icon;
            return icon == LaboratoryIcon.None || icons == null || index < 0 || index >= icons.Length
                ? null : icons[index];
        }
    }
}
