using MobileGame.TestSupport;
using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace MobileGame.Foundation.Tests
{
    public class ThingFoundationTests : FoundationTestFixture
    {
        [Test]
        public void FacilitiesShareDefinitionButKeepIndependentComponentsAndInventory()
        {
            var registry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var resourceDefinition = CreateResourceDefinition();
            var definitionBefore = DataFixtureSnapshot.ToJson(storageDefinition);
            var first = CreateHolder(registry, storageDefinition);
            var second = CreateHolder(registry, storageDefinition);
            var resource = registry.CreateResource(resourceDefinition, 5, first);

            Assert.That(first.Owner.Definition, Is.SameAs(second.Owner.Definition));
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.Contents, Is.EquivalentTo(new[] { resource }));
            Assert.That(second.Contents, Is.Empty);

            registry.ConsumeResource(resource, 2);

            Assert.That(first.Contents.Single().GetComponent<ResourceStackComponent>().Quantity,
                Is.EqualTo(3));
            Assert.That(second.Contents, Is.Empty);
            Assert.That(DataFixtureSnapshot.ToJson(storageDefinition), Is.EqualTo(definitionBefore),
                "Changing an individual facility's inventory must not modify its shared SO.");
        }

        [Test]
        public void ComponentsBelongToExactlyTheirThingAndMissingLookupDoesNotAddAFeature()
        {
            var registry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var first = registry.Create(storageDefinition);
            var second = registry.Create(storageDefinition);
            var creature = registry.Create(CreateDefinition<CreatureData>("fixture.no-features"));

            Assert.That(first.TryGetComponent<ResourceContainerComponent>(out var firstContainer), Is.True);
            Assert.That(second.TryGetComponent<ResourceContainerComponent>(out var secondContainer), Is.True);
            Assert.That(firstContainer.Owner, Is.SameAs(first));
            Assert.That(secondContainer.Owner, Is.SameAs(second));
            Assert.That(firstContainer, Is.Not.SameAs(secondContainer));
            Assert.That(first.Components, Is.EquivalentTo(new[] { firstContainer }));
            Assert.That(first.TryGetComponent<ResourceStackComponent>(out _), Is.False);
            Assert.That(creature.TryGetComponent<ResourceContainerComponent>(out _), Is.False);
            Assert.That(creature.Components, Is.Empty);
            Assert.That(creature.Exists, Is.True,
                "A logical Thing can exist without a scene object, Map position or resource holder.");
            Assert.That(creature.HolderId, Is.Null);
        }

        [Test]
        public void UniqueIdsAndCreationOrderStayStableAcrossResourceChanges()
        {
            var registry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var first = CreateHolder(registry, storageDefinition);
            var second = CreateHolder(registry, storageDefinition);
            var resourceDefinition = CreateResourceDefinition();
            var resource = registry.CreateResource(resourceDefinition, 5, first);
            var created = new[] { first.Owner, second.Owner, resource };
            var resourceId = resource.Id;
            var creationOrder = resource.CreationOrder;

            Assert.That(created.Select(thing => thing.Id), Is.All.Not.EqualTo(Guid.Empty));
            Assert.That(created.Select(thing => thing.Id).Distinct().Count(), Is.EqualTo(created.Length));
            Assert.That(first.Owner.CreationOrder, Is.GreaterThan(0));
            Assert.That(second.Owner.CreationOrder, Is.GreaterThan(first.Owner.CreationOrder));
            Assert.That(resource.CreationOrder, Is.GreaterThan(second.Owner.CreationOrder));

            registry.TransferResource(resource, second);
            registry.ConsumeResource(resource, 1);

            Assert.That(resource.Id, Is.EqualTo(resourceId));
            Assert.That(resource.CreationOrder, Is.EqualTo(creationOrder));
            Assert.That(resource.Definition, Is.SameAs(resourceDefinition));
            Assert.That(registry.TryGet(resourceId, out var found), Is.True);
            Assert.That(found, Is.SameAs(resource));
            Assert.That(registry.TryGet(Guid.NewGuid(), out _), Is.False);
        }
    }
}
