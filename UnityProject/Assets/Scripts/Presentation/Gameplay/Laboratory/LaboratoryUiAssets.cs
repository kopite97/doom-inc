using System;
using TMPro;
using UnityEngine;

namespace MobileGame.Presentation
{
    [Serializable]
    public sealed class LaboratoryUiAssets
    {
        private static readonly LaboratoryIcon[] KnownIcons = (LaboratoryIcon[])Enum.GetValues(typeof(LaboratoryIcon));
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private TextAsset fontLicense;
        [SerializeField] private AudioClip releaseSound;
        [SerializeField] private Sprite[] icons;
        [SerializeField] private Sprite[] floorTiles;
        [SerializeField] private LaboratoryArtAssets art;

        public LaboratoryArtAssets Art => art;

        public AudioClip ReleaseSound => releaseSound;
        public TMP_FontAsset Font => font;
        public TextAsset FontLicense => fontLicense;
        public bool IsComplete
        {
            get
            {
                if (font == null || fontLicense == null || releaseSound == null) return false;
                foreach (var icon in KnownIcons)
                    if (icon != LaboratoryIcon.None && GetIcon(icon) == null) return false;
                return true;
            }
        }

        public LaboratoryUiAssets(TMP_FontAsset font, Sprite[] icons, TextAsset fontLicense, AudioClip releaseSound,
            Sprite[] floorTiles = null, LaboratoryArtAssets art = null)
        {
            this.font = font;
            this.fontLicense = fontLicense;
            this.releaseSound = releaseSound;
            this.icons = icons == null ? Array.Empty<Sprite>() : (Sprite[])icons.Clone();
            this.floorTiles = floorTiles == null ? Array.Empty<Sprite>() : (Sprite[])floorTiles.Clone();
            this.art = art;
        }

        public Sprite GetFloorTile(int variant)
        {
            if (floorTiles == null || floorTiles.Length == 0) return null;
            return variant >= 0 && variant < floorTiles.Length && floorTiles[variant] != null
                ? floorTiles[variant] : floorTiles[0];
        }

        public Sprite GetIcon(LaboratoryIcon icon)
        {
            var index = (int)icon;
            return icon == LaboratoryIcon.None || icons == null || index < 0 || index >= icons.Length
                ? null : icons[index];
        }
    }
}
