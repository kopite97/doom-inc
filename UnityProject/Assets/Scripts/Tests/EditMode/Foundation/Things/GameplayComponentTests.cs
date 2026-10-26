using System;
using System.Linq;
using MobileGame.Gameplay;
using NUnit.Framework;
using UnityEditor;

namespace MobileGame.Foundation.Tests
{
    public sealed class GameplayComponentTests : FoundationTestFixture
    {
        [Test]
        public void InjectedFactoryAddsOnlyRoleFeaturesWithIndependentOwnersAndState()
        {
            var resource = CreateResourceDefinition();
            var storage = Facility("fixture.storage", FacilityRole.Storage, resource);
            var gathering = Facility("fixture.gathering", FacilityRole.Gathering, resource);
            var creature = CreateDefinition<CreatureDefinition>("fixture.worker", ThingComponentKind.ResourceContainer);
            var registry = new ThingRegistry(GameplayComponentFactory.Attach);
            var first = registry.Create(storage);
            var second = registry.Create(storage);
            var production = registry.Create(gathering);
            var worker = registry.Create(creature);
            var another = registry.Create(creature);
            var before = EditorJsonUtility.ToJson(storage);

            Assert.That(first.GetComponent<ConstructionComponent>().IsComplete, Is.False);
            Assert.That(first.GetComponent<StorageComponent>().AllowsOrganic, Is.True);
            Assert.That(first.Components.All(c => ReferenceEquals(c.Owner, first)), Is.True);
            Assert.That(first.GetComponent<StorageComponent>(), Is.Not.SameAs(second.GetComponent<StorageComponent>()));
            Assert.That(first.TryGetComponent<GatheringComponent>(out _), Is.False);
            Assert.That(production.GetComponent<GatheringComponent>().Outstanding, Is.Zero);
            Assert.That(production.TryGetComponent<StorageComponent>(out _), Is.False);
            Assert.That(worker.GetComponent<WorkerComponent>().Job, Is.Null);
            worker.GetComponent<WorkerComponent>().Priorities[0] = 0;
            Assert.That(another.GetComponent<WorkerComponent>().Priorities[0], Is.EqualTo(1));
            Assert.That(EditorJsonUtility.ToJson(storage), Is.EqualTo(before));

            var baseline = new ThingRegistry().Create(storage);
            Assert.That(baseline.TryGetComponent<ConstructionComponent>(out _), Is.False,
                "The common registry does not attach gameplay features unless requested by its session.");
        }

        [Test]
        public void InvalidFactoryAndDuplicateComponentsCannotPartiallyRegister()
        {
            var invalid = CreateDefinition<CreatureDefinition>("fixture.missing-container");
            var registry = new ThingRegistry(GameplayComponentFactory.Attach);
            Assert.Throws<InvalidOperationException>(() => registry.Create(invalid));
            Assert.That(registry.Things, Is.Empty);
            Assert.That(registry.LastCreationOrder, Is.Zero);
            var replacement = CreateDefinition<CreatureDefinition>(invalid.DefinitionId, ThingComponentKind.ResourceContainer);
            var valid = registry.Create(replacement);
            var before = valid.Components.ToArray();
            Assert.Throws<InvalidOperationException>(() => GameplayComponentFactory.Attach(valid));
            Assert.That(valid.Components, Is.EqualTo(before));
            Assert.That(valid.CreationOrder, Is.EqualTo(1));
        }

        [Test]
        public void GameplayDefinitionsRejectMissingResourcesAndInvalidSettings()
        {
            var resource = CreateResourceDefinition();
            var storage = Facility("fixture.storage", FacilityRole.Storage, resource);
            storage.Validate();
            using (var serialized = new SerializedObject(storage))
            {
                serialized.FindProperty("storageCapacity").intValue = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Assert.Throws<ArgumentException>(() => storage.Validate());
            using (var serialized = new SerializedObject(storage))
            {
                serialized.FindProperty("storageCapacity").intValue = 20;
                serialized.FindProperty("resource").objectReferenceValue = null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Assert.Throws<ArgumentException>(() => storage.Validate());
            var creature = CreateDefinition<CreatureDefinition>("fixture.creature", ThingComponentKind.ResourceContainer);
            using (var serialized = new SerializedObject(creature))
            {
                serialized.FindProperty("moveSecondsPerCell").floatValue = float.NaN;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Assert.Throws<ArgumentException>(() => creature.Validate());
        }

        private FacilityDefinition Facility(string id, FacilityRole role, ResourceDefinition resource)
        {
            var result = CreateStorageDefinition(id);
            using (var serialized = new SerializedObject(result))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = resource;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return result;
        }
    }
}
