using System;
using System.IO;
using System.Linq;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class InvasionTests
    {
        private Thing Raider(SessionState session)
        {
            var thing = session.Registry.Create(catalog.Single(d => d.DefinitionId == "creature.raider"));
            Assert.That(session.Map.Commands.Place(thing.Id, Vector2Int.one).Succeeded, Is.True);
            return thing;
        }

        [Test]
        public void PendingAndSkippedRoundsSurviveAdmissionAndReload()
        {
            var session = Fresh(); var lab = Lab(session);
            var raider = Raider(session); var normal = AddInvader(session);
            var flow = Flow(session);
            Assert.That(lab.Invasion.ReleaseAll(), Is.True);
            flow.Advance(30);
            var row = lab.Receiving.Sources.Single();
            Assert.That((row.PendingRound, row.LastRound), Is.EqualTo((3L, 6L)));
            var saved = codec.Capture(session, flow.Phase);
            foreach (var corruption in new Action<ReceiptSnapshot>[] {
                r => r.source = normal.Id.ToString("N"), r => r.pendingRound = 9,
                r => r.pendingRound = 2, r => r.resource = culture.DefinitionId, r => r.amount++ })
            {
                var copy = codec.Deserialize(codec.Serialize(saved));
                corruption(copy.invasion.receipts.Single());
                Assert.Throws<InvalidDataException>(() => codec.Restore(copy));
            }
            var store = CompleteFacility(session, storage, new Vector2Int(7, 7));
            flow.Advance(0.05);
            Assert.That(flow.Save() && flow.Reload(), Is.True, flow.LastError);
            lab = Lab(flow.Current);
            Assert.That(lab.Receiving.Receive(raider.Id, 6, organic.DefinitionId, 1), Is.EqualTo(ReceiptResult.AlreadyProcessed));
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(1));
            Assert.That(lab.Receiving.PendingCount, Is.Zero);
            var outcome = ((CreatureData)raider.Definition).Behavior.ResolveActivity(
                new InvaderActivityContext((CreatureData)raider.Definition, lab.Invasion.State.Settings, 12));
            Assert.That((outcome.Score, outcome.Amount), Is.EqualTo((5, 1)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FinalReceiptCommitsBeforeDestructionSave(bool editing)
        {
            var session = Fresh(Settings(9)); var lab = Lab(session);
            AddInvader(session); Raider(session);
            var store = CompleteFacility(session, storage, new Vector2Int(7, 7));
            var flow = Flow(session); Assert.That(lab.Invasion.ReleaseAll(), Is.True);
            if (editing) Assert.That(lab.BeginConstruction(gathering), Is.True);
            flow.Advance(60);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            lab = Lab(flow.Current);
            Assert.That(lab.Invasion.State.Score, Is.EqualTo(9));
            Assert.That(lab.Receiving.PendingCount, Is.EqualTo(editing ? 1 : 0));
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(editing ? 0 : 1));
            flow.Advance(60);
            Assert.That(lab.TotalAt(store.Id), Is.EqualTo(editing ? 0 : 1));
        }

        [Test]
        public void UnmodifiedVersionFiveFixtureLoadsWithNewCatalog()
        {
            var path = Path.Combine(Application.dataPath, "../../docs/plans/evidence/PLAN-022/legacy-v5.json");
            var saved = codec.Deserialize(File.ReadAllText(path));
            Assert.That(saved.version, Is.EqualTo(5));
            Assert.That(saved.dataHash, Is.EqualTo(LegacyDataBaseline.Hash));
            var migrated = codec.Capture(codec.Restore(saved), saved.phase);
            Assert.That(migrated.version, Is.EqualTo(6));
            Assert.That(migrated.invasion.receipts, Is.Empty);

            var busy = codec.Deserialize(File.ReadAllText(Path.Combine(Application.dataPath,
                "../../docs/plans/evidence/PLAN-020/fixture-2.json")));
            var resumed = codec.Restore(busy);
            Assert.That(Lab(resumed).Invasion.ExternalCount, Is.EqualTo(16));
            var vat = resumed.Registry.Things.Single(t => t.Definition == cultivation);
            Assert.That(vat.GetComponent<ProductionComponent>().SelectedTarget, Is.EqualTo(CultivationTarget.Support));
            Assert.That(codec.Capture(resumed, busy.phase).invasion.score, Is.EqualTo(busy.invasion.score));
        }
    }
}
