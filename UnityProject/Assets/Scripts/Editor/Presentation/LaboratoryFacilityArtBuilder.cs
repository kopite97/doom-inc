using System;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class LaboratoryFacilityArtBuilder
    {
        private static readonly string[] Names = { "Processing", "Cultivation", "Storage" };
        public static void InstallReceived()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import outside Play Mode.");
            for (var k = 0; k < Names.Length; k++)
            {
                var name = Names[k];
                var target = PathFor(name);
                FoundationDefinitionAssets.EnsureFolder("Assets/Sprites/Facility/" + name);
                if (!File.Exists(target))
                {
                    var source = "Assets/Sprites/Require/PLAN-029/PLAN-029-SPR-00" + (k + 1) + "/r001/" + name + ".png";
                    if (!File.Exists(source)) throw new FileNotFoundException(source);
                    var error = AssetDatabase.MoveAsset(source, target);
                    if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                }
                Import(target, name, k == 2 ? 192 : 128);
            }
            EnsureReferences();
        }
        private static string PathFor(string name) => "Assets/Sprites/Facility/" + name + "/" + name + ".png";
        private static string FrameName(string name, int i)
        {
            var storage = name == "Storage";
            var split = storage ? 10 : 12;
            var state = i < 4 ? "Idle" : i < split ? storage ? "Receive" : "Working" : storage ? "Send" : "Complete";
            return name + "_" + state + "_" + (i - (i < 4 ? 0 : i < split ? 4 : split) + 1).ToString("00");
        }
        private static void Import(string path, string name, int width)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            var change = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Multiple
                || importer.spritePixelsPerUnit != 32 || importer.filterMode != FilterMode.Point || importer.wrapMode != TextureWrapMode.Clamp
                || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.mipmapEnabled || importer.isReadable
                || !importer.sRGBTexture || !importer.alphaIsTransparency || importer.alphaSource != TextureImporterAlphaSource.FromInput
                || importer.npotScale != TextureImporterNPOTScale.None || importer.maxTextureSize != (width == 192 ? 1024 : 512)
                || settings.spriteMeshType != SpriteMeshType.FullRect;
            foreach (var platform in new[] { "Android", "Standalone", "iPhone", "WebGL" })
            { change |= importer.GetPlatformTextureSettings(platform).overridden; importer.ClearPlatformTextureSettings(platform); }
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false; importer.isReadable = false;
            importer.sRGBTexture = true; importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = width == 192 ? 1024 : 512;
            importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            if (change) importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects();
            var rectangles = new SpriteRect[16];
            var same = old.Length == 16;
            for (var i = 0; i < 16; i++)
            {
                var spriteName = FrameName(name, i);
                var previous = old.FirstOrDefault(item => item.name == spriteName);
                var rect = new Rect(i % 4 * width, (3 - i / 4) * 128, width, 128);
                rectangles[i] = new SpriteRect { name = spriteName, rect = rect, alignment = SpriteAlignment.Center,
                    pivot = Vector2.one * .5f, spriteID = previous == null ? GUID.Generate() : previous.spriteID };
                same &= previous != null && previous.rect == rect && previous.pivot == Vector2.one * .5f && previous.alignment == SpriteAlignment.Center;
            }
            if (same) return;
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rectangles.Select(item => new SpriteNameFileIdPair(item.name, item.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
        public static void EnsureReferences()
        {
            if (!Names.All(name => File.Exists(PathFor(name)))) return;
            var art = AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath);
            if (art == null) throw new InvalidOperationException("Existing laboratory artwork binding is required.");
            var data = new SerializedObject(art); var entries = data.FindProperty("facilities"); entries.arraySize = 3;
            var definitions = AssetDatabase.FindAssets("t:FacilityDefinition", new[] { "Assets/SO" })
                .Select(id => AssetDatabase.LoadAssetAtPath<FacilityDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
            for (var k = 0; k < Names.Length; k++)
            {
                var name = Names[k]; var entry = entries.GetArrayElementAtIndex(k);
                var definition = definitions.Single(item => item.DefinitionId == "facility." + name.ToLowerInvariant());
                entry.FindPropertyRelative("definition").objectReferenceValue = definition;
                var sprites = AssetDatabase.LoadAllAssetsAtPath(PathFor(name)).OfType<Sprite>().ToArray();
                var frames = entry.FindPropertyRelative("frames"); frames.arraySize = 16;
                var seconds = entry.FindPropertyRelative("seconds"); seconds.arraySize = 16;
                for (var i = 0; i < 16; i++)
                {
                    frames.GetArrayElementAtIndex(i).objectReferenceValue = sprites.Single(sprite => sprite.name == FrameName(name, i));
                    seconds.GetArrayElementAtIndex(i).floatValue = i < 4 ? name == "Storage" ? .3f : .25f
                        : name == "Storage" ? .1f : name == "Processing" ? .125f : i < 12 ? .15f : .1f;
                }
                var definitionData = new SerializedObject(definition);
                definitionData.FindProperty("icon").objectReferenceValue = frames.GetArrayElementAtIndex(0).objectReferenceValue;
                if (definitionData.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(definition);
            }
            if (data.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssetIfDirty(art);
        }
    }
}
