using System;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class LaboratoryCreatureArtBuilder
    {
        public const string SupportPath = "Assets/Sprites/Creature/Support/Support.png";
        private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };
        // Art PLAN-009 r002, top-left source pixels; identical ground point (32,54).
        private static readonly int[] HeadY = { 22,23,22,21,22,24,20,21,23,22,22,24,25,23,19,21 };
        public static string FrameName(int i)
        {
            var local = i % 16;
            return "Support_" + (local < 4 ? "Idle" : local < 10 ? "Move" : "Work") + "_" + Directions[i / 16]
                + "_" + (local - (local < 4 ? 0 : local < 10 ? 4 : 10) + 1).ToString("00");
        }
        public static void InstallReceived()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import outside Play Mode.");
            FoundationDefinitionAssets.EnsureFolder("Assets/Sprites/Creature/Support");
            if (!File.Exists(SupportPath))
            {
                const string received = "Assets/Sprites/Require/PLAN-033/PLAN-033-SPR-001/r002/Support.png";
                if (!File.Exists(received)) throw new FileNotFoundException(received);
                AssetDatabase.ImportAsset(received, ImportAssetOptions.ForceSynchronousImport);
                var error = AssetDatabase.MoveAsset(received, SupportPath);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            }
            ImportSupport();
            EnsureReferences();
        }
        private static void ImportSupport()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SupportPath);
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            if (width != 512 || height != 512) throw new InvalidOperationException("Support requires an 8x8 sheet of 64px cells.");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false; importer.isReadable = false;
            importer.sRGBTexture = true; importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 512;
            foreach (var platform in new[] { "Android", "Standalone", "iPhone", "WebGL" }) importer.ClearPlatformTextureSettings(platform);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite data provider unavailable.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite frame edit capability unavailable.");
            foreach (var capability in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteName, EEditCapability.EditSpriteRect, EEditCapability.EditPivot })
                if (!edit.GetEditCapability().HasCapability(capability)) throw new InvalidOperationException("Sprite edit unsupported: " + capability);
            var old = provider.GetSpriteRects();
            var rectangles = new SpriteRect[64];
            for (var i = 0; i < rectangles.Length; i++)
            {
                var name = FrameName(i); var previous = old.FirstOrDefault(item => item.name == name);
                rectangles[i] = new SpriteRect { name = name, rect = new Rect(i % 8 * 64, (7 - i / 8) * 64, 64, 64),
                    alignment = SpriteAlignment.Center, pivot = Vector2.one * .5f, spriteID = previous == null ? GUID.Generate() : previous.spriteID };
            }
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(item => new SpriteNameFileIdPair(item.name, item.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
        public static void EnsureReferences()
        {
            if (!File.Exists(SupportPath)) return;
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SupportPath).OfType<Sprite>().ToArray();
            if (sprites.Length != 64) throw new InvalidOperationException("Slice all 64 Support frames before binding.");
            var definition = AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/Support.asset");
            var art = AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath);
            if (definition == null || art == null) throw new InvalidOperationException("Existing Support definition and laboratory art are required.");
            using var data = new SerializedObject(art);
            var entries = data.FindProperty("creatures");
            var index = 0;
            while (index < entries.arraySize && entries.GetArrayElementAtIndex(index).FindPropertyRelative("definition").objectReferenceValue != definition) index++;
            if (index == entries.arraySize) entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("definition").objectReferenceValue = definition;
            var frames = entry.FindPropertyRelative("frames"); frames.arraySize = 64;
            var heads = entry.FindPropertyRelative("heads"); heads.arraySize = 64;
            for (var i = 0; i < 64; i++)
            {
                frames.GetArrayElementAtIndex(i).objectReferenceValue = sprites.Single(sprite => sprite.name == FrameName(i));
                var local = i % 16; var direction = i / 16;
                var lean = local == 6 || local == 7 || local == 12 || local == 13;
                heads.GetArrayElementAtIndex(i).vector2Value = new Vector2(32 + (lean ? direction == 1 ? -1 : direction == 2 ? 1 : 0 : 0), HeadY[local]);
            }
            if (data.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(art);
            using var binding = new SerializedObject(definition);
            binding.FindProperty("icon").objectReferenceValue = sprites.Single(sprite => sprite.name == FrameName(0));
            if (binding.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(definition);
        }
    }
}
