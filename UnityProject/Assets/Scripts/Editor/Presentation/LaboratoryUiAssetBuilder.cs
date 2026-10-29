using System;
using System.IO;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    // Generates only missing temporary presentation assets; never replaces supplied artwork.
    public static class LaboratoryUiAssetBuilder
    {
        public const string IconDirectory = "Assets/Sprites/UI/Laboratory";
        public const string FontPath = "Assets/Fonts/NanumGothic/Temp_NanumGothic-Regular.ttf";
        public const string FontLicensePath = "Assets/Fonts/NanumGothic/OFL.txt";
        private const int IconSize = 64;

        public static LaboratoryUiAssets CreateMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Prepare UI assets outside Play Mode.");
            if (!File.Exists(FontPath) || !File.Exists(FontLicensePath))
                throw new FileNotFoundException("The bundled Korean font and OFL notice are required.");

            FoundationDefinitionAssets.EnsureFolder(IconDirectory);
            var values = (LaboratoryIcon[])Enum.GetValues(typeof(LaboratoryIcon));
            var icons = new Sprite[values.Length];
            foreach (var icon in values)
            {
                if (icon == LaboratoryIcon.None) continue;
                var path = GetIconPath(icon);
                if (!File.Exists(path)) WriteIcon(path, icon);
                ImportIcon(path);
                icons[(int)icon] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (icons[(int)icon] == null) throw new InvalidOperationException("Could not load UI icon: " + path);
            }

            AssetDatabase.ImportAsset(FontPath, ImportAssetOptions.ForceSynchronousImport);
            var fontImporter = AssetImporter.GetAtPath(FontPath) as TrueTypeFontImporter;
            if (fontImporter == null) throw new InvalidOperationException("Korean font importer is missing.");
            if (fontImporter.fontTextureCase != FontTextureCase.Dynamic || !fontImporter.includeFontData)
            {
                fontImporter.fontTextureCase = FontTextureCase.Dynamic;
                fontImporter.includeFontData = true;
                fontImporter.SaveAndReimport();
            }
            AssetDatabase.ImportAsset(FontLicensePath, ImportAssetOptions.ForceSynchronousImport);
            var binding = new LaboratoryUiAssets(AssetDatabase.LoadAssetAtPath<Font>(FontPath), icons,
                AssetDatabase.LoadAssetAtPath<TextAsset>(FontLicensePath));
            if (!binding.IsComplete) throw new InvalidOperationException("UI asset binding is incomplete.");
            return binding;
        }

        public static string GetIconPath(LaboratoryIcon icon) => IconDirectory + "/Temp_" + icon + ".png";

        private static void ImportIcon(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Icon texture importer is missing: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = IconSize;
            importer.ClearPlatformTextureSettings("Android");
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void WriteIcon(string path, LaboratoryIcon icon)
        {
            var pixels = new IconPixels(IconSize);
            switch (icon)
            {
                case LaboratoryIcon.ZoomOut:
                case LaboratoryIcon.ZoomIn:
                    pixels.Ring(26, 38, 16); pixels.Line(38, 26, 54, 10, 6);
                    pixels.Line(17, 38, 35, 38);
                    if (icon == LaboratoryIcon.ZoomIn) pixels.Line(26, 29, 26, 47);
                    break;
                case LaboratoryIcon.FitMap:
                    pixels.Path(10, 25, 10, 10, 25, 10); pixels.Path(39, 10, 54, 10, 54, 25);
                    pixels.Path(10, 39, 10, 54, 25, 54); pixels.Path(39, 54, 54, 54, 54, 39);
                    pixels.Box(23, 23, 18, 18); pixels.Line(32, 23, 32, 41, 3); pixels.Line(23, 32, 41, 32, 3);
                    break;
                case LaboratoryIcon.ShowStatus:
                case LaboratoryIcon.HideStatus:
                    pixels.Eye(); pixels.Ring(32, 32, 7);
                    if (icon == LaboratoryIcon.HideStatus) pixels.Line(12, 12, 52, 52, 6);
                    break;
                case LaboratoryIcon.Build:
                    pixels.Path(8, 34, 32, 54, 56, 34); pixels.Path(15, 34, 15, 12, 49, 12, 49, 34);
                    pixels.Path(27, 12, 27, 27, 37, 27, 37, 12);
                    break;
                case LaboratoryIcon.Creatures:
                    pixels.Ring(24, 43, 8); pixels.Ring(45, 38, 6);
                    pixels.Path(9, 13, 13, 28, 35, 28, 39, 13); pixels.Path(41, 26, 51, 26, 56, 13);
                    break;
                case LaboratoryIcon.Invasion:
                    pixels.Line(14, 13, 50, 49, 7); pixels.Path(31, 49, 50, 49, 50, 30);
                    pixels.Path(12, 28, 12, 10, 30, 10);
                    break;
                case LaboratoryIcon.More:
                    pixels.Disc(14, 32, 5); pixels.Disc(32, 32, 5); pixels.Disc(50, 32, 5);
                    break;
                case LaboratoryIcon.Worker:
                    pixels.Ring(32, 44, 9); pixels.Path(15, 12, 20, 28, 44, 28, 49, 12);
                    pixels.Line(23, 12, 41, 12);
                    break;
                case LaboratoryIcon.Automatic:
                    pixels.Arc(32, 32, 21, 55, 340); pixels.Path(43, 54, 44, 42, 55, 43);
                    pixels.Line(32, 32, 32, 43, 3); pixels.Line(32, 32, 23, 28, 3);
                    break;
                case LaboratoryIcon.Material:
                    pixels.Box(14, 14, 36, 36); pixels.Line(14, 38, 50, 38); pixels.Line(32, 38, 32, 50);
                    pixels.Line(22, 24, 30, 24, 3);
                    break;
                case LaboratoryIcon.Delivery:
                    pixels.Path(11, 25, 11, 12, 53, 12, 53, 25);
                    pixels.Line(32, 52, 32, 27); pixels.Path(20, 38, 32, 26, 44, 38);
                    break;
                case LaboratoryIcon.Blocked:
                    pixels.Ring(32, 32, 22); pixels.Line(17, 17, 47, 47, 6);
                    break;
                case LaboratoryIcon.SelectTarget:
                    pixels.Ring(32, 32, 18); pixels.Ring(32, 32, 6);
                    pixels.Line(5, 32, 18, 32); pixels.Line(46, 32, 59, 32);
                    pixels.Line(32, 5, 32, 18); pixels.Line(32, 46, 32, 59);
                    break;
                case LaboratoryIcon.Construction:
                    pixels.Line(16, 11, 40, 35, 8); pixels.Path(25, 43, 39, 55, 55, 38, 42, 26, 25, 43);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(icon));
            }
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            try
            {
                texture.name = "Temp_" + icon;
                texture.SetPixels32(pixels.Values); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private sealed class IconPixels
        {
            private readonly int size;
            public Color32[] Values { get; }
            public IconPixels(int size) { this.size = size; Values = new Color32[size * size]; }
            public void Line(float x1, float y1, float x2, float y2, float thickness = 4)
            {
                var start = new Vector2(x1, y1); var delta = new Vector2(x2 - x1, y2 - y1);
                var squaredLength = delta.sqrMagnitude;
                for (var y = 0; y < size; y++)
                    for (var x = 0; x < size; x++)
                    {
                        var point = new Vector2(x + 0.5f, y + 0.5f);
                        var t = squaredLength == 0 ? 0 : Mathf.Clamp01(Vector2.Dot(point - start, delta) / squaredLength);
                        if ((point - start - delta * t).sqrMagnitude <= thickness * thickness * 0.25f) Set(x, y);
                    }
            }
            public void Path(params float[] points)
            { for (var i = 2; i < points.Length; i += 2) Line(points[i - 2], points[i - 1], points[i], points[i + 1]); }
            public void Box(float x, float y, float width, float height)
                => Path(x, y, x + width, y, x + width, y + height, x, y + height, x, y);
            public void Ring(float x, float y, float radius) => Arc(x, y, radius, 0, 360);
            public void Arc(float x, float y, float radius, float fromDegrees, float toDegrees)
            {
                var previous = new Vector2(x + radius * Mathf.Cos(fromDegrees * Mathf.Deg2Rad), y + radius * Mathf.Sin(fromDegrees * Mathf.Deg2Rad));
                for (var angle = fromDegrees + 4; angle < toDegrees + 4; angle += 4)
                {
                    var radians = Mathf.Min(angle, toDegrees) * Mathf.Deg2Rad;
                    var next = new Vector2(x + radius * Mathf.Cos(radians), y + radius * Mathf.Sin(radians));
                    Line(previous.x, previous.y, next.x, next.y); previous = next;
                }
            }
            public void Disc(float x, float y, float radius) => Line(x, y, x, y, radius * 2);
            public void Eye()
            {
                for (var side = -1; side <= 1; side += 2)
                {
                    var previous = new Vector2(7, 32);
                    for (var step = 1; step <= 24; step++)
                    {
                        var next = new Vector2(7 + 50f * step / 24, 32 + side * 15 * Mathf.Sin(Mathf.PI * step / 24));
                        Line(previous.x, previous.y, next.x, next.y); previous = next;
                    }
                }
            }
            private void Set(int x, int y) => Values[y * size + x] = new Color32(255, 255, 255, 255);
        }
    }
}
