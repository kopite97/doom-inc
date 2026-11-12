using System.Linq;
using System.Reflection;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class FacilityAnimationTests
    {
        private static FacilityArtSet Set(int index) => ((FacilityArtSet[])typeof(LaboratoryArtAssets)
            .GetField("facilities", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath)))[index];

        [TestCase(0, "Processing", 128, .125, .5)]
        [TestCase(1, "Cultivation", 128, .15, .4)]
        [TestCase(2, "Storage", 192, .1, .6)]
        public void ApprovedCellsTimingAndRegenerationPreserveTheirContract(int index, string name, int width, double step, double duration)
        {
            var set = Set(index);
            var motion = index == 2 ? FacilityArtMotion.Receive : FacilityArtMotion.Working;
            var once = index == 2 ? FacilityArtMotion.Send : FacilityArtMotion.Complete;
            Assert.That(set.FrameIndex(motion, 0), Is.EqualTo(4));
            Assert.That(set.FrameIndex(motion, step), Is.EqualTo(5));
            Assert.That(set.Duration(once), Is.EqualTo(duration).Within(1e-6));
            Assert.That(set.FrameIndex(once, duration * 2), Is.EqualTo(15));
            if (index != 2) Assert.That(set.FrameIndex(motion, set.Duration(motion)), Is.EqualTo(4));
            var path = "Assets/Sprites/Facility/" + name + "/" + name + ".png";
            var all = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            Assert.That(all.Length, Is.EqualTo(16));
            for (var i = 0; i < 16; i++)
            {
                var sprite = set.Frame(i);
                Assert.That(sprite.rect, Is.EqualTo(new Rect(i % 4 * width, (3 - i / 4) * 128, width, 128)));
                Assert.That(sprite.pivot, Is.EqualTo(new Vector2(width / 2f, 64)));
                Assert.That(sprite.vertices.Length, Is.EqualTo(4));
            }
            var meta = System.IO.File.ReadAllBytes(path + ".meta");
            LaboratoryFacilityArtBuilder.EnsureReferences();
            CollectionAssert.AreEqual(meta, System.IO.File.ReadAllBytes(path + ".meta"));
        }

        [Test]
        public void SimulationTimePausesAndLatestReactionCannotBuildAnUnboundedHistory()
        {
            var state = new FacilityAnimation(); var set = Set(2);
            state.Observe(FacilityArtMotion.Receive, 0, .6);
            Assert.That(state.Evaluate(.1, set, false, 0, true), Is.EqualTo(5));
            Assert.That(state.Evaluate(.1, set, false, 0, true), Is.EqualTo(5));
            state.Observe(FacilityArtMotion.Send, .15, .6);
            state.Observe(FacilityArtMotion.Receive, .2, .6);
            state.Evaluate(.6, set, false, 0, true);
            Assert.That(state.Motion, Is.EqualTo(FacilityArtMotion.Receive));
            state.Evaluate(5, set, false, 0, true);
            Assert.That(state.Motion, Is.EqualTo(FacilityArtMotion.Idle));
            var fresh = new FacilityAnimation();
            fresh.Evaluate(5, set, false, 0, true);
            Assert.That(fresh.Motion, Is.EqualTo(FacilityArtMotion.Idle));
        }

        [Test]
        public void WorkingUsesActualProgressAndCompletionDoesNotReplayWhileOutputWaits()
        {
            var state = new FacilityAnimation(); var set = Set(0);
            Assert.That(state.Evaluate(1, set, true, .125, true), Is.EqualTo(5));
            Assert.That(state.Evaluate(2, set, false, .125, true), Is.LessThan(4));
            Assert.That(state.Evaluate(3, set, true, .125, true), Is.EqualTo(5));
            state.Observe(FacilityArtMotion.Complete, 3, .5);
            Assert.That(state.Evaluate(3.125, set, true, .25, true), Is.EqualTo(13));
            Assert.That(state.Evaluate(3.6, set, false, .25, true), Is.LessThan(4));
            Assert.That(state.Evaluate(9, set, false, .25, true), Is.LessThan(4));
        }
    }
}
