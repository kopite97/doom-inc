using System;
using System.IO;
using System.Linq;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class ProductionTests
    {
        [TestCase("partial-input")]
        [TestCase("input-reserved")]
        [TestCase("twenty-percent")]
        [TestCase("hundred-percent")]
        [TestCase("next-target")]
        [TestCase("output-carried")]
        [TestCase("assignment-in-transit")]
        [TestCase("spawned")]
        public void FileCheckpointsRestoreWholeGraphAndContinueWithoutDuplicatingWork(string checkpoint)
        {
            var worker = Worker(support, 4, 3, new[] { 0, 0, 1, 2 });
            var store = Facility(storage, 10, 3);
            var isCultivation = checkpoint == "next-target" || checkpoint == "spawned";
            var facility = Facility(isCultivation ? cultivation : processing, 4, 4);
            if (isCultivation) lab.SelectCultivation(facility.Id, CultivationTarget.Support);
            if (checkpoint == "partial-input") Stock(facility, organic, 2);
            else if (checkpoint == "input-reserved" || checkpoint == "assignment-in-transit")
            { Stock(store, organic, 10); lab.Advance(0.05); }
            else
            {
                Stock(facility, isCultivation ? culture : organic, isCultivation ? 2 : 5);
                if (checkpoint == "hundred-percent") lab.AssignWorker(worker.Id, facility.Id);
                lab.Advance(1);
                if (checkpoint == "next-target") lab.SelectCultivation(facility.Id, CultivationTarget.Invader);
                if (checkpoint == "output-carried") Until(lab.Advance, () => lab.Reservations.Any(r => r.PickedUp));
                if (checkpoint == "spawned") Until(lab.Advance, () => registry.Things.Count(t => t.Definition == support) == 2);
            }
            if (checkpoint == "assignment-in-transit") lab.AssignWorker(worker.Id, facility.Id);
            var flow = Flow(() => new SessionState(registry, map, lab));
            var codec = new SessionSnapshotCodec(catalog, true);
            Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            var before = codec.Serialize(codec.Capture(flow.Current, SessionPhase.Playing));
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            Assert.That(codec.Serialize(codec.Capture(flow.Current, SessionPhase.Playing)), Is.EqualTo(before));
            var restored = (LaboratoryManager)flow.Current.Runtime;
            Assert.That(restored, Is.Not.SameAs(lab));
            // Continue the original and fresh restored graphs through the same elapsed time.
            lab.Advance(110); flow.Advance(110);
            lab.ValidateState(); restored.ValidateState();
            var left = codec.Capture(new SessionState(registry, map, lab), SessionPhase.Playing);
            var right = codec.Capture(flow.Current, SessionPhase.Playing);
            Assert.That(right.things.Select(t => t.definitionId).OrderBy(id => id), Is.EqualTo(left.things.Select(t => t.definitionId).OrderBy(id => id)));
            Assert.That(right.things.Sum(t => t.quantity), Is.EqualTo(left.things.Sum(t => t.quantity)));
            var actual = restored.Registry.Things.Single(t => t.Id == facility.Id).GetComponent<ProductionComponent>();
            var expected = facility.GetComponent<ProductionComponent>();
            Assert.That(actual.Progress, Is.EqualTo(expected.Progress).Within(1e-8));
            Assert.That(actual.Outstanding, Is.EqualTo(expected.Outstanding));
            Assert.That(actual.ActiveTarget, Is.EqualTo(expected.ActiveTarget));
            Assert.That(flow.LastSaveError, Is.Null.Or.Empty);
        }
    }
}
