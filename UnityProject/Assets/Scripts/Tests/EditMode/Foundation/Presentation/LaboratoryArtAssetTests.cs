using System.IO;
using System.Linq;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryArtAssetTests
    {
        [TestCase("Assets/Sprites/World/Laboratory/LaboratoryFloorSet.png", 4, 64, 4, 1)]
        [TestCase("Assets/Sprites/Facility/Gathering/Gathering.png", 36, 128, 6, 6)]
        [TestCase("Assets/Sprites/Creature/PrototypeSupport/PrototypeSupport.png", 64, 64, 8, 8)]
        [TestCase("Assets/Sprites/Resource/OrganicMatter/OrganicMatter.png", 1, 32, 1, 1)]
        [TestCase("Assets/Sprites/UI/Laboratory/Build.png", 1, 64, 1, 1)]
        public void DeliveredSheetsRetainEveryFullCellAndPixelImportContract(string path, int count, int cell, int columns, int rows)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.mipmapEnabled || importer.isReadable, Is.False);
            Assert.That(importer.sRGBTexture && importer.alphaIsTransparency, Is.True);
            Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput));
            Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None));
            foreach (var platform in new[] { "Android", "Standalone", "iPhone", "WebGL" })
                Assert.That(importer.GetPlatformTextureSettings(platform).overridden, Is.False);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(texture.width, Is.EqualTo(columns * cell)); Assert.That(texture.height, Is.EqualTo(rows * cell));
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();
            Assert.That(sprites.Length, Is.EqualTo(count));
            for (var i = 0; i < count; i++)
            {
                Assert.That(sprites[i].rect, Is.EqualTo(new Rect(i % columns * cell, (rows - 1 - i / columns) * cell, cell, cell)));
                Assert.That(sprites[i].pivot, Is.EqualTo(Vector2.one * (cell * .5f)));
                Assert.That(sprites[i].vertices.Length, Is.EqualTo(4), "Transparent margins must retain a full rectangular mesh.");
            }
        }

        [Test]
        public void RegenerationKeepsApprovedReferencesAndAllDirectionFrames()
        {
            var path = LaboratoryUiAssetBuilder.GetIconPath(LaboratoryIcon.Build);
            Assert.That(path, Is.EqualTo("Assets/Sprites/UI/Laboratory/Build.png"));
            var png = File.ReadAllBytes(path);
            var floorMeta = File.ReadAllText(LaboratoryUiAssetBuilder.FloorPath + ".meta");
            var configured = AssetDatabase.LoadAssetAtPath<AppConfiguration>("Assets/SO/Application/AppConfiguration.asset").uiAssets;
            var rebuilt = LaboratoryUiAssetBuilder.CreateMissing();
            Assert.That(rebuilt.Art, Is.SameAs(configured.Art));
            Assert.That(rebuilt.WorldMaterial, Is.Not.Null);
            Assert.That(rebuilt.WorldMaterial, Is.SameAs(configured.WorldMaterial));
            Assert.That(rebuilt.WorldMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            Assert.That(LayerMask.NameToLayer(LaboratoryWorldMap.LayerName), Is.GreaterThanOrEqualTo(8));
            var organic = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Resource/OrganicMatter/OrganicMatter.png");
            Assert.That(rebuilt.Art.CargoSupport(organic), Is.EqualTo(new Vector2(18, 28)));
            Assert.That(rebuilt.GetIcon(LaboratoryIcon.Build), Is.SameAs(configured.GetIcon(LaboratoryIcon.Build)));
            CollectionAssert.AreEqual(png, File.ReadAllBytes(path));
            Assert.That(File.ReadAllText(LaboratoryUiAssetBuilder.FloorPath + ".meta"), Is.EqualTo(floorMeta));
            for (var i = 0; i < 36; i++) Assert.That(rebuilt.Art.GatheringFrame(i), Is.Not.Null);
            var directions = new[] { "Down", "Left", "Right", "Up" };
            for (var d = 0; d < 4; d++)
            {
                var index = d * 16;
                foreach (var state in new[] { "Idle", "Move", "Work" })
                    for (var frame = 1; frame <= (state == "Idle" ? 4 : 6); frame++, index++)
                    {
                        Assert.That(rebuilt.Art.PrototypeFrame(index).name, Is.EqualTo("PrototypeSupport_" + state + "_" + directions[d] + "_" + frame.ToString("00")));
                        Assert.That(rebuilt.Art.PrototypeHead(index).x, Is.InRange(31, 33));
                        Assert.That(rebuilt.Art.PrototypeHead(index).y, Is.InRange(21, 27));
                    }
                Assert.That(rebuilt.Art.PrototypeHead(d * 16), Is.EqualTo(new Vector2(32, 24)), "The r001 head anchor must not survive.");
            }
        }
    }
}
