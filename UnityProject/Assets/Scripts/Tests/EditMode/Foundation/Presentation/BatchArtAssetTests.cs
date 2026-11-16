using System.Linq;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace MobileGame.Foundation.Tests
{
    public sealed class BatchArtAssetTests
    {
        [TestCase("Invader",40,64,64,10,4)]
        [TestCase("Raider",40,64,64,10,4)]
        [TestCase("LaboratoryBoundary",8,64,64,4,2)]
        [TestCase("TitleLandscape",16,640,360,4,4)]
        [TestCase("TitlePortrait",16,360,640,4,4)]
        [TestCase("LoadingDecoration",16,64,64,4,4)]
        [TestCase("WorldDestruction",32,128,128,8,4)]
        public void EveryReceivedFrameRetainsPixelsGeometryAndIdentity(string id,int count,int width,int height,int columns,int rows)
        {
            var frames=BatchArtAssets.Frames(id);Assert.That(frames.Length,Is.EqualTo(count));Assert.That(frames.All(s=>s!=null),Is.True);
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(frames[0]));
            Assert.That(importer.spritePixelsPerUnit,Is.EqualTo(32));Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(importer.mipmapEnabled||importer.isReadable,Is.False);Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(frames[0].texture.width,Is.EqualTo(columns*width));Assert.That(frames[0].texture.height,Is.EqualTo(rows*height));
            for(var i=0;i<count;i++)
            {
                Assert.That(frames[i].rect,Is.EqualTo(new Rect(i%columns*width,(rows-1-i/columns)*height,width,height)));
                Assert.That(frames[i].vertices.Length,Is.EqualTo(4));
                Assert.That(frames[i].pivot,Is.EqualTo(id=="Invader"||id=="Raider"?new Vector2(32,10):new Vector2(width*.5f,height*.5f)));
            }
        }
        [Test]
        public void FinalFrameAndRegeneratedCreatureBindingsRemainStable()
        {
            var art=AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath);
            var old=BatchArtAssets.Frames("WorldDestruction");
            Assert.That(art.ColonizationFrame(0),Is.SameAs(old[0]));Assert.That(art.ColonizationFrame(.5f),Is.SameAs(old[16]));Assert.That(art.ColonizationFrame(1),Is.SameAs(old[31]));
            var catalog=GameDataAssets.LoadCatalog();
            foreach(var id in new[]{"Invader","Raider"})
            {
                var definition=catalog.Get<CreatureData>("creature."+id.ToLowerInvariant());
                var before=Enumerable.Range(0,40).Select(i=>art.CreatureFrame(definition,i)).ToArray();
                LaboratoryUiAssetBuilder.CreateMissing();
                Assert.That(definition.Assets.Icon,Is.SameAs(before[0]));
                for(var i=0;i<40;i++) Assert.That(art.CreatureFrame(definition,i),Is.SameAs(before[i]));
                for(var direction=0;direction<4;direction++)
                {
                    Assert.That(art.CreaturePlaybackFrame(definition,direction,0,0),Is.EqualTo(direction*10));
                    Assert.That(art.CreaturePlaybackFrame(definition,direction,1,0),Is.EqualTo(direction*10+4));
                    Assert.That(art.CreaturePlaybackFrame(definition,direction,1,id=="Invader"?.13:.11),Is.EqualTo(direction*10+5));
                }
            }
        }
        [Test]
        public void NineSliceBordersAndExistingFillContractRemainIntact()
        {
            var skin=AssetDatabase.LoadAssetAtPath<UiSkinAssets>(BatchArtAssets.SkinPath);
            Assert.That(skin.panel.border,Is.EqualTo(Vector4.one*4));Assert.That(skin.hud.border,Is.EqualTo(Vector4.one*4));
            Assert.That(skin.button.border,Is.EqualTo(Vector4.one*3));Assert.That(skin.selection.border,Is.EqualTo(Vector4.one*3));
            Assert.That(skin.scrollThumb.border,Is.EqualTo(Vector4.one*3));Assert.That(skin.destructionTrack.border,Is.EqualTo(Vector4.one));
        }
    }
}
