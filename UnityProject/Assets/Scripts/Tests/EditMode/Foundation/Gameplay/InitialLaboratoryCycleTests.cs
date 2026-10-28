using MobileGame.TestSupport;
using System;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class InitialLaboratoryCycleTests : FoundationTestFixture
    {
        [Test]
        public void FullStorageResumesThroughPaidConstructionAndActualPartialAdmissions()
        {
            var organic = CreateResourceDefinition("cycle.organic");
            var prototype = CreateDefinition<CreatureData>("cycle.prototype", ThingComponentKind.ResourceContainer);
            var gatheringDefinition = Facility("cycle.gathering", FacilityRole.Gathering, organic,
                new Vector2Int(2, 2), new Vector2Int(0, -1));
            var storageDefinition = Facility("cycle.storage", FacilityRole.Storage, organic,
                new Vector2Int(3, 2), new Vector2Int(1, -1));
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN008-Cycle-" + Guid.NewGuid().ToString("N"));
            try
            {
                var codec = new SessionSnapshotCodec(new ThingData[]
                    { organic, prototype, gatheringDefinition, storageDefinition }, true);
                var controller = new GameFlowManager(codec,
                    new JsonFileSessionStore(Path.Combine(directory, "session.json")),
                    () => LaboratorySessionFactory.Create(organic, gatheringDefinition, storageDefinition, prototype));
                Assert.That(controller.StartOrContinue(), Is.True);
                var commands = new LaboratoryCommands(controller);
                var simulation = (LaboratoryManager)controller.Current.Runtime;
                Assert.That(simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                Assert.That(simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(TotalOrganic(simulation), Is.EqualTo(4));
                var worker = simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(simulation.TotalAt(worker.Id), Is.EqualTo(4), "Initial materials have a real holder before storage exists.");

                Build(commands, gatheringDefinition, new Vector2Int(4, 4));
                Build(commands, storageDefinition, new Vector2Int(8, 4));
                var gathering = simulation.Registry.Things.Single(t => t.Definition == gatheringDefinition);
                var firstStorage = simulation.Registry.Things.Single(t => t.Definition == storageDefinition);
                var batch = gathering.GetComponent<GatheringComponent>();
                AdvanceUntil(controller, () => simulation.TotalAt(firstStorage.Id) == 20 && batch.Outstanding == 5,
                    "The actual initial build and five collection cycles did not reach full storage.");
                Assert.That(simulation.IsComplete(gathering) && simulation.IsComplete(firstStorage), Is.True);
                Assert.That(TotalOrganic(simulation), Is.EqualTo(25));
                Assert.That(simulation.TotalAt(gathering.Id), Is.EqualTo(5));

                controller.Advance(20);
                Assert.That(simulation.TotalAt(firstStorage.Id), Is.EqualTo(20));
                Assert.That(TotalOrganic(simulation), Is.EqualTo(25), "Full storage must stop additional production after the pending batch.");
                Assert.That(batch.Outstanding, Is.EqualTo(5));
                Assert.That(batch.Progress, Is.EqualTo(5));

                Build(commands, storageDefinition, new Vector2Int(8, 7));
                var secondStorage = simulation.Registry.Things.Single(t => t.Definition == storageDefinition && t.Id != firstStorage.Id);
                var construction = secondStorage.GetComponent<ConstructionComponent>();
                Assert.That(construction.IsComplete, Is.False);
                Assert.That(construction.Delivered, Is.Zero);
                AdvanceUntil(controller, () => simulation.TotalAt(worker.Id) == 2 && simulation.TotalAt(firstStorage.Id) == 18,
                    "Construction did not withdraw its cost from the full storage.");
                Assert.That(TotalOrganic(simulation), Is.EqualTo(25), "Pickup changes ownership but does not consume construction cost.");
                Assert.That(construction.Delivered, Is.Zero);
                AdvanceUntil(controller, () => construction.Delivered == 2,
                    "The worker did not deliver and consume the second storage's construction materials.");
                Assert.That(TotalOrganic(simulation), Is.EqualTo(23));
                Assert.That(construction.Progress, Is.Zero, "The delivery delta must not also count as construction work.");
                controller.Advance(2.9);
                Assert.That(construction.IsComplete, Is.False);
                Assert.That(construction.Progress, Is.EqualTo(2.9).Within(1e-9));
                controller.Advance(0.1);
                Assert.That(construction.IsComplete, Is.True);
                Assert.That(construction.Progress, Is.EqualTo(3));
                Assert.That(batch.Outstanding, Is.EqualTo(5), "Construction has freed capacity but no output has been admitted yet.");

                AdvanceUntil(controller, () => batch.Outstanding == 3,
                    "The two newly free units in the nearer original storage were not used for partial admission.");
                Assert.That(simulation.TotalAt(firstStorage.Id), Is.EqualTo(20));
                Assert.That(simulation.TotalAt(secondStorage.Id), Is.Zero);
                Assert.That(batch.Progress, Is.EqualTo(5), "Partial admission must not reset the gathering cycle.");
                Assert.That(TotalOrganic(simulation), Is.EqualTo(23));
                AdvanceUntil(controller, () => batch.Outstanding == 0,
                    "The remainder of the pending batch did not reach the new storage.");
                Assert.That(simulation.TotalAt(firstStorage.Id), Is.EqualTo(20));
                Assert.That(simulation.TotalAt(secondStorage.Id), Is.EqualTo(3));
                Assert.That(simulation.TotalAt(gathering.Id), Is.Zero);
                Assert.That(simulation.TotalAt(worker.Id), Is.Zero);
                Assert.That(TotalOrganic(simulation), Is.EqualTo(23), "Exactly two units were spent, with no duplicated output or delivery.");
                Assert.That(simulation.Reservations, Is.Empty);
                Assert.That(batch.Progress, Is.Zero);

                AdvanceUntil(controller, () => batch.Progress > 0, "Actual gathering did not resume after the full pending batch was stored.");
                Assert.That(batch.Outstanding, Is.Zero);
                Assert.That(TotalOrganic(simulation), Is.EqualTo(23));
                Assert.That(controller.LastSaveError, Is.Null);
                Assert.That(controller.SuccessfulSaves, Is.GreaterThan(1), "This cycle also passed real automatic snapshot/file boundaries.");
                simulation.ValidateState();
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static void Build(LaboratoryCommands commands, FacilityData definition, Vector2Int position)
        {
            Assert.That(commands.BeginConstruction(definition), Is.True);
            Assert.That(commands.PreviewConstruction(position, QuarterTurn.None), Is.True);
            Assert.That(commands.ConfirmConstruction(), Is.True);
        }

        private static void AdvanceUntil(GameFlowManager controller, Func<bool> condition, string failure)
        {
            for (var step = 0; step < 2400 && !condition(); step++) controller.Advance(0.05);
            Assert.That(condition(), Is.True, failure + " (120 simulated seconds maximum)");
        }

        private static int TotalOrganic(LaboratoryManager simulation) => simulation.Registry.Things
            .Where(t => t.Definition.Kind == ThingKind.Resource).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);

        private FacilityData Facility(string id, FacilityRole role, ResourceData organic,
            Vector2Int footprint, Vector2Int interaction)
        {
            var definition = CreateStorageDefinition(id);
            using (var serialized = new DataFixtureEditor(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.FindProperty("footprint").vector2IntValue = footprint;
                serialized.FindProperty("interactionOffset").vector2IntValue = interaction;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return definition;
        }
    }
}
