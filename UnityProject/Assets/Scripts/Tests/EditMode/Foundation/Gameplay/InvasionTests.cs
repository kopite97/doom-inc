using MobileGame.TestSupport;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed partial class InvasionTests
    {
        private ThingData[] catalog;
        private ResourceData organic, culture;
        private FacilityData gathering, storage, processing, cultivation;
        private CreatureData prototype, support, invader;
        private SessionSnapshotCodec codec;
        private string directory;

        [SetUp]
        public void Prepare()
        {
            catalog = new[] { "Resource/OrganicMatter", "Resource/CultureMedium", "Facility/Gathering", "Facility/Storage",
                "Facility/Processing", "Facility/Cultivation", "Creature/PrototypeSupport", "Creature/Support", "Creature/Invader" }
                .Select(path => DataTestAssets.LoadAssetAtPath<ThingData>("Assets/SO/" + path + ".asset")).ToArray();
            Assert.That(catalog.All(d => d != null), Is.True);
            organic = (ResourceData)catalog[0]; culture = (ResourceData)catalog[1];
            gathering = (FacilityData)catalog[2]; storage = (FacilityData)catalog[3];
            processing = (FacilityData)catalog[4]; cultivation = (FacilityData)catalog[5];
            prototype = (CreatureData)catalog[6]; support = (CreatureData)catalog[7]; invader = (CreatureData)catalog[8];
            codec = new SessionSnapshotCodec(catalog, true);
            directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN010-" + Guid.NewGuid().ToString("N"));
        }
        [TearDown]
        public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

        [Test]
        public void ReleaseAllRetainsIdentityAndExcludesOtherModelsAndFutureBirths()
        {
            var session = Fresh(); var lab = Lab(session);
            var first = AddInvader(session); var second = AddInvader(session);
            var helper = session.Registry.Create(support);
            Assert.That(session.Map.Commands.Place(helper.Id, Vector2Int.one).Succeeded, Is.True);
            var flow = Flow(session); var commands = new LaboratoryCommands(flow);
            var ids = session.Registry.Things.Select(t => t.Id).ToArray(); var order = first.CreationOrder;
            var labBefore = JsonUtility.ToJson(codec.Capture(session, flow.Phase).laboratory);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            Assert.That(first.IsPlaced || second.IsPlaced, Is.False);
            Assert.That(session.Registry.Things.Select(t => t.Id), Is.EquivalentTo(ids));
            Assert.That(first.CreationOrder, Is.EqualTo(order));
            Assert.That(helper.IsPlaced, Is.True);
            Assert.That(first.GetComponent<InvaderComponent>().IsExternal, Is.True);
            Assert.That(first.TryGetComponent<WorkerComponent>(out _), Is.False);
            Assert.That(lab.Invasion.State.Score, Is.Zero);
            Assert.That(JsonUtility.ToJson(codec.Capture(session, flow.Phase).laboratory), Is.EqualTo(labBefore));
            var snapshot = Capture(flow);
            Assert.That(commands.ReleaseInvaders(), Is.False);
            Assert.That(Capture(flow), Is.EqualTo(snapshot));
            var later = AddInvader(session);
            flow.Advance(5);
            Assert.That(lab.Invasion.State.Score, Is.EqualTo(4));
            Assert.That(later.IsPlaced, Is.True);
            Assert.That(later.GetComponent<InvaderComponent>().CompletedActivities, Is.Zero);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            Assert.That(lab.Invasion.ExternalCount, Is.EqualTo(3));
            lab.ValidateState();
        }

        [Test]
        public void InvalidTargetAndEditingRejectWithoutPartialRelease()
        {
            var session = Fresh(); var first = AddInvader(session); var second = AddInvader(session);
            var flow = Flow(session); var commands = new LaboratoryCommands(flow);
            Assert.That(commands.BeginConstruction(gathering), Is.True);
            var before = Capture(flow);
            Assert.That(commands.ReleaseInvaders(), Is.False);
            Assert.That(Capture(flow), Is.EqualTo(before));
            Assert.That(commands.CancelConstruction(), Is.True);
            Assert.That(session.Map.Commands.Unplace(second.Id).Succeeded, Is.True); // Deliberately corrupt fixture.
            Assert.That(commands.ReleaseInvaders(), Is.False);
            Assert.That(first.IsPlaced, Is.True);
            Assert.That(first.GetComponent<InvaderComponent>().IsExternal, Is.False);
            Assert.That(Lab(session).Invasion.State.Score, Is.Zero);
        }

        [Test]
        public void GoalCommitsActivityBeforeAutomaticFileAndConsumesOnlyTimeToBoundary()
        {
            var session = Fresh(Settings(2)); var lab = Lab(session); var outside = AddInvader(session);
            var processor = CompleteFacility(session, processing, new Vector2Int(4, 4));
            session.Registry.CreateResource(organic, 5, processor.GetComponent<ResourceContainerComponent>());
            var store = new FaultStore(Slot()); var flow = Flow(session, store); var commands = new LaboratoryCommands(flow);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            var saves = flow.SuccessfulSaves;
            flow.Advance(1000);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(flow.SuccessfulSaves, Is.EqualTo(saves + 1), "Use only ReachGoal's synchronous save.");
            Assert.That(lab.Invasion.State.Elapsed, Is.EqualTo(5));
            Assert.That(processor.GetComponent<ProductionComponent>().Progress, Is.EqualTo(1).Within(1e-8));
            Assert.That(outside.GetComponent<InvaderComponent>().CompletedActivities, Is.EqualTo(1));
            var restored = codec.Restore(codec.Deserialize(store.Read())); // No extra Save or Advance before reading.
            Assert.That(Lab(restored).Invasion.State.Score, Is.EqualTo(2));
            Assert.That(Lab(restored).Invasion.State.Reports.Single().Activity, Is.EqualTo(1));
            Assert.That(Lab(restored).Invasion.State.ActivitySequence, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(codec.Capture(restored, flow.Phase).invasion),
                Is.EqualTo(JsonUtility.ToJson(codec.Deserialize(store.Read()).invasion)));
            Assert.That(restored.Registry.TryGet(processor.Id, out var restoredProcessor), Is.True);
            Assert.That(restoredProcessor.GetComponent<ProductionComponent>().Progress, Is.EqualTo(1).Within(1e-8));
            var before = Capture(flow);
            Assert.That(commands.ReleaseInvaders(), Is.False);
            Assert.That(flow.ReachGoal(), Is.False);
            flow.Advance(1000);
            Assert.That(Capture(flow), Is.EqualTo(before));
        }

        [Test]
        public void JustBeforeActivityBoundaryDoesNotAwardOrStopEarly()
        {
            var session = Fresh(Settings(2)); AddInvader(session); var flow = Flow(session);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True);
            flow.Advance(5 - 1e-8);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Playing));
            Assert.That(Lab(session).Invasion.State.Score, Is.Zero);
            flow.Advance(2e-8);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(Lab(session).Invasion.State.Elapsed, Is.EqualTo(5).Within(1e-12));
        }

        [Test]
        public void LargeAndSplitDeltaReachSameBoundaryAndSimultaneousBatchIsComplete()
        {
            var session = Fresh(Settings(6)); AddInvader(session); AddInvader(session);
            var initial = codec.Capture(session, SessionPhase.Playing);
            var big = Flow(session); var small = Flow(codec.Restore(initial));
            Assert.That(new LaboratoryCommands(big).ReleaseInvaders(), Is.True);
            Assert.That(new LaboratoryCommands(small).ReleaseInvaders(), Is.True);
            big.Advance(100);
            for (var index = 0; index < 400 && small.Phase == SessionPhase.Playing; index++) small.Advance(0.25);
            Assert.That(Capture(small), Is.EqualTo(Capture(big)));
            Assert.That(Lab(big.Current).Invasion.State.Score, Is.EqualTo(8), "Both completions at the same instant commit together.");
            Assert.That(Lab(big.Current).Invasion.State.ActivitySequence, Is.EqualTo(4));
            Assert.That(Lab(big.Current).Invasion.State.Elapsed, Is.EqualTo(10));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EditingPausesOnlyLabAndGoalCannotResumeAfterCancel(bool moving)
        {
            var session = Fresh(Settings(2)); var lab = Lab(session); AddInvader(session);
            var processor = CompleteFacility(session, processing, new Vector2Int(4, 4));
            session.Registry.CreateResource(organic, 5, processor.GetComponent<ResourceContainerComponent>());
            var flow = Flow(session); var commands = new LaboratoryCommands(flow);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            if (moving)
            {
                Assert.That(flow.BeginPlacement(processor.Id), Is.True);
                Assert.That(flow.PreviewPlacement(new Vector2Int(4, 4), QuarterTurn.Clockwise90), Is.True);
            }
            else Assert.That(commands.BeginConstruction(storage), Is.True);
            var laboratoryBefore = JsonUtility.ToJson(codec.Capture(session, flow.Phase).laboratory);
            flow.Advance(50);
            Assert.That(flow.Phase, Is.EqualTo(SessionPhase.Destruction));
            Assert.That(JsonUtility.ToJson(codec.Capture(session, flow.Phase).laboratory), Is.EqualTo(laboratoryBefore));
            Assert.That(moving ? flow.CancelPlacement() : commands.CancelConstruction(), Is.True);
            var stopped = Capture(flow); flow.Advance(10);
            Assert.That(Capture(flow), Is.EqualTo(stopped));
            Assert.That(flow.CanAdvanceExternal || flow.CanAdvanceLaboratory, Is.False);
        }

        [Test]
        public void ApplicationPauseQuitAndContinueDoNotAdvanceOfflineOrPermitRelease()
        {
            var session = Fresh(); AddInvader(session); var flow = Flow(session); var commands = new LaboratoryCommands(flow);
            flow.NotifyApplicationPause(true);
            Assert.That(commands.ReleaseInvaders(), Is.False);
            flow.NotifyApplicationPause(false);
            Assert.That(commands.ReleaseInvaders(), Is.True);
            flow.Advance(2);
            flow.NotifyApplicationPause(true); var before = Capture(flow); flow.Advance(10000);
            Assert.That(Capture(flow), Is.EqualTo(before));
            flow.NotifyApplicationPause(false); flow.Advance(3);
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.EqualTo(2));
            flow.NotifyApplicationQuit(); before = Capture(flow); flow.Advance(10000);
            Assert.That(flow.Reload(), Is.True, flow.LastError);
            Assert.That(Capture(flow), Is.EqualTo(before));
        }

        [Test]
        public void ReportsFilterAndRetentionDoNotAlterScoreAndReloadDoesNotReplay()
        {
            var settings = Settings(1000); Field(settings, "detailedUntilScore", 8L); Field(settings, "reportLimit", 3);
            var session = Fresh(settings); AddInvader(session); var flow = Flow(session);
            Assert.That(new LaboratoryCommands(flow).ReleaseInvaders(), Is.True);
            flow.Advance(20);
            var state = Lab(flow.Current).Invasion.State;
            Assert.That(state.Reports.Select(r => r.Sequence), Is.EqualTo(new long[] { 2, 3, 4 }));
            var score = state.Score; var reports = state.Reports.Count;
            flow.Advance(5);
            Assert.That(state.Score, Is.EqualTo(score + 2));
            Assert.That(state.Reports.Count, Is.EqualTo(reports));
            flow.Advance(25);
            Assert.That(state.Score, Is.EqualTo(36));
            Assert.That(state.Reports.Select(r => r.Sequence), Is.EqualTo(new long[] { 3, 4, 8 }));
            Assert.That(state.Reports.Sum(r => r.Score), Is.LessThan(state.Score));
            var snapshot = Capture(flow); Assert.That(flow.Save(), Is.True, flow.LastSaveError);
            Assert.That(flow.Reload(), Is.True, flow.LastError); Assert.That(Capture(flow), Is.EqualTo(snapshot));
            flow.Advance(5);
            Assert.That(Lab(flow.Current).Invasion.State.ActivitySequence, Is.EqualTo(11));
            Assert.That(Lab(flow.Current).Invasion.State.Score, Is.EqualTo(38));
        }

        private SessionState Fresh(InvasionSettings settings = null) => LaboratorySessionFactory.Create(organic, gathering, storage, prototype, culture, settings);
        private static LaboratoryManager Lab(SessionState session) => (LaboratoryManager)session.Runtime;
        private Thing AddInvader(SessionState session)
        {
            var thing = session.Registry.Create(invader);
            Assert.That(session.Map.Commands.Place(thing.Id, new Vector2Int(1, 1)).Succeeded, Is.True);
            return thing;
        }
        private Thing CompleteFacility(SessionState session, FacilityData definition, Vector2Int cell)
        {
            var thing = session.Registry.Create(definition);
            Assert.That(session.Map.Commands.Place(thing.Id, cell).Succeeded, Is.True);
            var component = thing.GetComponent<ConstructionComponent>();
            Property(component, "Delivered", definition.BuildCost); Property(component, "Progress", (double)definition.BuildSeconds);
            Property(component, "IsComplete", true); return thing;
        }
        private string Slot() => Path.Combine(directory, Guid.NewGuid().ToString("N") + ".json");
        private GameFlowManager Flow(SessionState session, ISessionStore store = null)
        {
            var flow = new GameFlowManager(codec, store ?? new JsonFileSessionStore(Slot()), () => session, 10000);
            Assert.That(flow.StartOrContinue(), Is.True, flow.LastError); return flow;
        }
        private string Capture(GameFlowManager flow) => codec.Serialize(codec.Capture(flow.Current, flow.Phase));
        private static InvasionSettings Settings(long target)
        {
            var settings = DataTestAssets.Catalog.Settings.Invasion.Copy(); Field(settings, "targetScore", target);
            Field(settings, "detailedUntilScore", Math.Min(60, target)); return settings;
        }
        private static void Field(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static void Property(object target, string property, object value) => target.GetType().GetProperty(property).GetSetMethod(true).Invoke(target, new[] { value });
        private sealed class FaultStore : ISessionStore
        {
            private readonly JsonFileSessionStore file;
            public FaultStore(string path) { file = new JsonFileSessionStore(path); }
            public bool FailWrite;
            public string ReadOverride;
            public bool Exists => file.Exists;
            public string Read() => ReadOverride ?? file.Read();
            public string DurableRead() => file.Read();
            public void Write(string json) { if (FailWrite) throw new IOException("Injected invasion write failure."); file.Write(json); }
        }
    }
}
