using System;
using System.IO;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class LaboratoryResourceArtBuilder
    {
        public const string CulturePath = "Assets/Sprites/Resource/CultureMedium/CultureMedium.png";
        public static void InstallReceived()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import outside Play Mode.");
            FoundationDefinitionAssets.EnsureFolder("Assets/Sprites/Resource/CultureMedium");
            if (!File.Exists(CulturePath))
            {
                const string received = "Assets/Sprites/Require/PLAN-035/PLAN-035-SPR-001/r001/CultureMedium.png";
                AssetDatabase.ImportAsset(received, ImportAssetOptions.ForceSynchronousImport);
                var error = AssetDatabase.MoveAsset(received, CulturePath);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(CulturePath);
            importer.GetSourceTextureWidthAndHeight(out var w, out var h);
            if (w != 32 || h != 32) throw new InvalidOperationException("CultureMedium requires a 32px canvas.");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false; importer.isReadable = false;
            importer.sRGBTexture = true; importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 32;
            foreach (var platform in new[] { "Android", "Standalone", "iPhone", "WebGL" }) importer.ClearPlatformTextureSettings(platform);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = Vector2.one * .5f; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            EnsureReferences();
        }
        public static void EnsureReferences()
        {
            if (!File.Exists(CulturePath)) return;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CulturePath);
            var definition = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/SO/Resource/CultureMedium.asset");
            var art = AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath);
            if (sprite == null || definition == null || art == null) throw new InvalidOperationException("CultureMedium dependencies missing.");
            using var binding = new SerializedObject(definition);
            binding.FindProperty("icon").objectReferenceValue = sprite;
            if (binding.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(definition);
            using var data = new SerializedObject(art);
            var sprites = data.FindProperty("cargoSprites"); var supports = data.FindProperty("cargoSupports");
            var index = 0;
            while (index < sprites.arraySize && sprites.GetArrayElementAtIndex(index).objectReferenceValue != sprite) index++;
            if (index == sprites.arraySize) sprites.arraySize++;
            if (supports.arraySize < sprites.arraySize) supports.arraySize = sprites.arraySize;
            sprites.GetArrayElementAtIndex(index).objectReferenceValue = sprite;
            supports.GetArrayElementAtIndex(index).vector2Value = new Vector2(16, 28);
            if (data.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(art);
        }
    }
}
