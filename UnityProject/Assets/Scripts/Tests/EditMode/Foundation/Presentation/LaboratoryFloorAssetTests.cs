using System.Linq;
using MobileGame.Foundation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryFloorAssetTests
    {
        [Test]
        public void FloorSheetPreservesRequestedCellsQualityAndSharedReferences()
        {
            var tiles = LaboratoryUiAssetBuilder.LoadFloorTiles();
            Assert.That(tiles.Length, Is.EqualTo(4));
            Assert.That(tiles.All(t => t != null), Is.True);
            var importer = (TextureImporter)AssetImporter.GetAtPath(LaboratoryUiAssetBuilder.FloorPath);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.mipmapEnabled || importer.isReadable, Is.False);
            Assert.That(importer.sRGBTexture && importer.alphaIsTransparency, Is.True);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None));
            foreach (var platform in new[] { "Android", "Standalone", "iPhone", "WebGL" })
                Assert.That(importer.GetPlatformTextureSettings(platform).overridden, Is.False, platform);
            var config = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("Assets/SO/Application/AppConfiguration.asset"));
            var refs = config.FindProperty("uiAssets.floorTiles");
            Assert.That(refs.arraySize, Is.EqualTo(4));
            for (var i = 0; i < 4; i++)
            {
                Assert.That(tiles[i].rect, Is.EqualTo(new Rect(i * 64, 0, 64, 64)));
                Assert.That(tiles[i].pivot, Is.EqualTo(new Vector2(32, 32)));
                Assert.That(tiles[i].pixelsPerUnit, Is.EqualTo(32));
                Assert.That(refs.GetArrayElementAtIndex(i).objectReferenceValue, Is.EqualTo(tiles[i]));
            }
        }
    }
}
