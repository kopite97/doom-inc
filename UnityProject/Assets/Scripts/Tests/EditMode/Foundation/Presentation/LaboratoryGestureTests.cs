using System.Collections.Generic;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryGestureTests
    {
        private LaboratoryGesture gesture;
        private List<Vector2> taps;
        private List<Vector2> pans;
        private List<float> scales;
        private List<Vector2> centers;

        [SetUp]
        public void Prepare()
        {
            gesture = new LaboratoryGesture();
            taps = new List<Vector2>(); pans = new List<Vector2>();
            scales = new List<float>(); centers = new List<Vector2>();
            gesture.Tapped += taps.Add;
            gesture.Panned += pans.Add;
            gesture.Zoomed += (scale, center) => { scales.Add(scale); centers.Add(center); };
        }

        [Test]
        public void TapWaitsForReleaseAndSmallMovementRemainsATap()
        {
            gesture.Begin(1, new Vector2(20, 30), true);
            gesture.Move(1, new Vector2(23, 34));
            Assert.That(taps, Is.Empty); Assert.That(pans, Is.Empty);
            gesture.End(1, new Vector2(23, 34));
            Assert.That(taps, Is.EqualTo(new[] { new Vector2(23, 34) }));
            Assert.That(gesture.HasPointers, Is.False);
        }

        [Test]
        public void DragIncludesThresholdDistanceAndNeverTapsOnRelease()
        {
            gesture.Begin(1, Vector2.zero, true);
            gesture.Move(1, new Vector2(5, 0));
            gesture.Move(1, new Vector2(15, 0));
            gesture.Move(1, new Vector2(20, 4));
            gesture.End(1, new Vector2(22, 4));
            Assert.That(pans, Is.EqualTo(new[] { new Vector2(15, 0), new Vector2(5, 4), new Vector2(2, 0) }));
            Assert.That(taps, Is.Empty);
        }

        [Test]
        public void DistantReleaseWithoutAnIntermediateMoveCannotTap()
        {
            gesture.Begin(-1, Vector2.zero, true);
            gesture.End(-1, new Vector2(100, 100));
            Assert.That(taps, Is.Empty);
            Assert.That(pans.Count, Is.EqualTo(1));
        }

        [Test]
        public void UiOwnedPointerCannotBecomeAMapGestureAfterCrossingTheBoundary()
        {
            gesture.Begin(1, new Vector2(100, 0), false);
            gesture.Move(1, Vector2.zero);
            gesture.End(1, Vector2.zero);
            Assert.That(taps, Is.Empty); Assert.That(pans, Is.Empty); Assert.That(scales, Is.Empty);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MixedUiAndMapContactsBlockUntilEveryFingerIsReleased(bool mapFirst)
        {
            gesture.Begin(1, Vector2.zero, mapFirst);
            gesture.Begin(2, new Vector2(100, 0), !mapFirst);
            gesture.Move(1, new Vector2(20, 0));
            gesture.End(2, new Vector2(100, 0));
            gesture.Begin(3, new Vector2(50, 50), true);
            gesture.End(3, new Vector2(50, 50));
            gesture.End(1, new Vector2(20, 0));
            Assert.That(taps, Is.Empty); Assert.That(pans, Is.Empty); Assert.That(scales, Is.Empty);
            gesture.Begin(4, Vector2.zero, true); gesture.End(4, Vector2.zero);
            Assert.That(taps.Count, Is.EqualTo(1));
        }

        [Test]
        public void PinchPublishesCenterMotionAndRatioAndSuppressesTheRemainingFinger()
        {
            gesture.Begin(1, Vector2.zero, true);
            gesture.Begin(2, new Vector2(100, 0), true);
            gesture.Move(2, new Vector2(200, 0));
            Assert.That(scales, Is.EqualTo(new[] { 2f }));
            Assert.That(centers, Is.EqualTo(new[] { new Vector2(100, 0) }));
            Assert.That(pans, Is.EqualTo(new[] { new Vector2(50, 0) }));
            gesture.End(2, new Vector2(200, 0));
            gesture.Move(1, new Vector2(40, 0));
            gesture.End(1, new Vector2(40, 0));
            Assert.That(taps, Is.Empty); Assert.That(pans.Count, Is.EqualTo(1));
            gesture.Begin(7, Vector2.zero, true); gesture.End(7, Vector2.zero);
            Assert.That(taps.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThirdFingerCancelsTheWholeSequence()
        {
            gesture.Begin(1, Vector2.zero, true);
            gesture.Begin(2, new Vector2(100, 0), true);
            gesture.Begin(3, new Vector2(200, 0), true);
            gesture.End(3, new Vector2(200, 0));
            gesture.Move(2, new Vector2(250, 0));
            gesture.End(2, new Vector2(250, 0));
            gesture.End(1, Vector2.zero);
            Assert.That(taps, Is.Empty); Assert.That(pans, Is.Empty); Assert.That(scales, Is.Empty);
        }

        [Test]
        public void LayoutCancellationRetainsContactsButRejectsStaleRelease()
        {
            gesture.Begin(1, Vector2.zero, true);
            gesture.Cancel();
            Assert.That(gesture.HasPointers, Is.True);
            gesture.Move(1, new Vector2(100, 0));
            gesture.End(1, new Vector2(100, 0));
            Assert.That(taps, Is.Empty); Assert.That(pans, Is.Empty);
            Assert.That(gesture.HasPointers, Is.False);
            gesture.Begin(2, Vector2.one, true); gesture.End(2, Vector2.one);
            Assert.That(taps, Is.EqualTo(new[] { Vector2.one }));
        }

        [Test]
        public void DeviceCancellationSuppressesTapAndUnknownReleaseIsIgnored()
        {
            gesture.Begin(1, Vector2.zero, true);
            gesture.End(1, Vector2.zero, canceled: true);
            gesture.End(1, Vector2.zero);
            gesture.End(999, Vector2.zero);
            Assert.That(taps, Is.Empty);
            Assert.That(gesture.HasPointers, Is.False);
        }

        [Test]
        public void DuplicateBeginCannotResetDragOrChangeUiOwnership()
        {
            gesture.Begin(1, Vector2.zero, false);
            gesture.Begin(1, Vector2.zero, true);
            gesture.End(1, Vector2.zero);
            gesture.Begin(2, Vector2.zero, true);
            gesture.Move(2, new Vector2(50, 0));
            gesture.Begin(2, new Vector2(50, 0), true);
            gesture.End(2, new Vector2(50, 0));
            Assert.That(taps, Is.Empty); Assert.That(pans.Count, Is.EqualTo(1));
        }

        [Test]
        public void CoincidentPinchContactsDoNotPublishInvalidScales()
        {
            gesture.Begin(1, Vector2.zero, true); gesture.Begin(2, Vector2.zero, true);
            gesture.Move(2, new Vector2(100, 0));
            Assert.That(scales, Is.Empty);
            gesture.Move(2, new Vector2(50, 0));
            Assert.That(scales, Is.EqualTo(new[] { 0.5f }));
        }

        [Test]
        public void CancelDuringPanCannotPublishAScaleForTheOldScreen()
        {
            gesture.Panned += delta => gesture.Cancel();
            gesture.Begin(1, Vector2.zero, true); gesture.Begin(2, new Vector2(100, 0), true);
            gesture.Move(2, new Vector2(200, 0));
            Assert.That(pans.Count, Is.EqualTo(1)); Assert.That(scales, Is.Empty);
            gesture.End(1, Vector2.zero); gesture.End(2, new Vector2(200, 0));
            Assert.That(taps, Is.Empty);
        }
    }
}
