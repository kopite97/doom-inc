using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace MobileGame.Foundation.Tests
{
    public class ResourceOwnershipTests : FoundationTestFixture
    {
        [Test]
        public void ResourceHasOneHolderAtCreationAndAfterRepeatedTransfers()
        {
            var registry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var first = CreateHolder(registry, storageDefinition);
            var second = CreateHolder(registry, storageDefinition);
            var resource = registry.CreateResource(CreateResourceDefinition(), 5, first);

            Assert.That(resource.HolderId, Is.EqualTo(first.Owner.Id));
            Assert.That(first.Contents, Is.EquivalentTo(new[] { resource }));
            Assert.That(second.Contents, Is.Empty);

            registry.TransferResource(resource, second);
            registry.TransferResource(resource, second);

            Assert.That(resource.HolderId, Is.EqualTo(second.Owner.Id));
            Assert.That(first.Contents, Is.Empty);
            Assert.That(second.Contents, Is.EquivalentTo(new[] { resource }));
            Assert.That(registry.Things.Count(thing => thing.Id == resource.Id), Is.EqualTo(1));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
        }

        [Test]
        public void ConsumingOneStackDoesNotChangeAnotherStackOrSharedDefinition()
        {
            var registry = new ThingRegistry();
            var holder = CreateHolder(registry, CreateStorageDefinition());
            var definition = CreateResourceDefinition();
            var definitionBefore = EditorJsonUtility.ToJson(definition);
            var first = registry.CreateResource(definition, 5, holder);
            var second = registry.CreateResource(definition, 5, holder);
            var firstStack = first.GetComponent<ResourceStackComponent>();
            var secondStack = second.GetComponent<ResourceStackComponent>();

            registry.ConsumeResource(first, 2);

            Assert.That(firstStack, Is.Not.SameAs(secondStack));
            Assert.That(firstStack.Owner, Is.SameAs(first));
            Assert.That(secondStack.Owner, Is.SameAs(second));
            Assert.That(firstStack.Quantity, Is.EqualTo(3));
            Assert.That(secondStack.Quantity, Is.EqualTo(5));
            Assert.That(holder.Contents, Is.EquivalentTo(new[] { first, second }));
            Assert.That(EditorJsonUtility.ToJson(definition), Is.EqualTo(definitionBefore));
        }

        [Test]
        public void InvalidQuantitiesLeaveRegistryDefinitionsOwnershipAndQuantityUnchanged()
        {
            foreach (var invalidQuantity in new[] { 0, -1 })
            {
                var registry = new ThingRegistry();
                var holder = CreateHolder(registry, CreateStorageDefinition("fixture.holder." + invalidQuantity));
                var rejectedDefinition = CreateResourceDefinition("fixture.resource." + invalidQuantity);
                AssertRejectedWithoutRegistryChange(registry,
                    () => registry.CreateResource(rejectedDefinition, invalidQuantity, holder));

                var validDefinition = CreateResourceDefinition(rejectedDefinition.DefinitionId);
                var resource = registry.CreateResource(validDefinition, 5, holder);
                Assert.That(resource.Definition, Is.SameAs(validDefinition),
                    "A rejected creation must not reserve the failed definition ID.");
                foreach (var invalidAmount in new[] { 0, -1, 6 })
                {
                    AssertRejectedWithoutRegistryChange(registry,
                        () => registry.ConsumeResource(resource, invalidAmount));
                    Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
                    Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
                    Assert.That(holder.Contents, Is.EquivalentTo(new[] { resource }));
                }
            }
        }

        [Test]
        public void NullInputsAreRejectedWithoutLosingExistingResources()
        {
            var registry = new ThingRegistry();
            var holder = CreateHolder(registry, CreateStorageDefinition());
            var definition = CreateResourceDefinition();
            var resource = registry.CreateResource(definition, 5, holder);

            AssertRejectedWithoutRegistryChange(registry, () => registry.Create(null));
            AssertRejectedWithoutRegistryChange(registry, () => registry.CreateResource(null, 1, holder));
            AssertRejectedWithoutRegistryChange(registry, () => registry.CreateResource(definition, 1, null));
            AssertRejectedWithoutRegistryChange(registry, () => registry.TransferResource(null, holder));
            AssertRejectedWithoutRegistryChange(registry, () => registry.TransferResource(resource, null));
            AssertRejectedWithoutRegistryChange(registry, () => registry.ConsumeResource(null, 1));
            AssertRejectedWithoutRegistryChange(registry, () => registry.Remove(null));
            Assert.That(resource.Exists, Is.True);
            Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(holder.Contents, Is.EquivalentTo(new[] { resource }));
        }

        [Test]
        public void ForeignRegistryHoldersAndResourcesCannotCrossOwnershipBoundaries()
        {
            var registry = new ThingRegistry();
            var foreignRegistry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var definition = CreateResourceDefinition();
            var holder = CreateHolder(registry, storageDefinition);
            var foreignHolder = CreateHolder(foreignRegistry, storageDefinition);
            var resource = registry.CreateResource(definition, 5, holder);
            var foreignResource = foreignRegistry.CreateResource(definition, 3, foreignHolder);

            AssertRejectedWithoutRegistryChange(registry,
                () => registry.CreateResource(definition, 1, foreignHolder));
            AssertRejectedWithoutRegistryChange(registry,
                () => registry.TransferResource(resource, foreignHolder));
            AssertRejectedWithoutRegistryChange(registry,
                () => registry.TransferResource(foreignResource, holder));
            AssertRejectedWithoutRegistryChange(registry,
                () => registry.ConsumeResource(foreignResource, 1));
            AssertRejectedWithoutRegistryChange(registry, () => registry.Remove(foreignResource));

            Assert.That(holder.Contents, Is.EquivalentTo(new[] { resource }));
            Assert.That(foreignHolder.Contents, Is.EquivalentTo(new[] { foreignResource }));
            Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
            Assert.That(foreignResource.HolderId, Is.EqualTo(foreignHolder.Owner.Id));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(foreignResource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
        }

        [Test]
        public void RemovedHolderCannotReceiveNewOrExistingResources()
        {
            var registry = new ThingRegistry();
            var storageDefinition = CreateStorageDefinition();
            var holder = CreateHolder(registry, storageDefinition);
            var removedHolder = CreateHolder(registry, storageDefinition);
            var definition = CreateResourceDefinition();
            var resource = registry.CreateResource(definition, 5, holder);
            registry.Remove(removedHolder.Owner);

            Assert.That(removedHolder.Owner.Exists, Is.False);
            Assert.That(registry.TryGet(removedHolder.Owner.Id, out _), Is.False);
            AssertRejectedWithoutRegistryChange(registry,
                () => registry.CreateResource(definition, 1, removedHolder));
            AssertRejectedWithoutRegistryChange(registry,
                () => registry.TransferResource(resource, removedHolder));
            Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
            Assert.That(holder.Contents, Is.EquivalentTo(new[] { resource }));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
        }

        [Test]
        public void NonemptyHolderCannotBeRemovedAndFullConsumptionEndsResourceExistence()
        {
            var registry = new ThingRegistry();
            var holder = CreateHolder(registry, CreateStorageDefinition());
            var resource = registry.CreateResource(CreateResourceDefinition(), 5, holder);
            var stack = resource.GetComponent<ResourceStackComponent>();

            AssertRejectedWithoutRegistryChange(registry, () => registry.Remove(holder.Owner));
            Assert.That(holder.Owner.Exists, Is.True);
            Assert.That(holder.Contents, Is.EquivalentTo(new[] { resource }));
            registry.ConsumeResource(resource, 5);

            Assert.That(resource.Exists, Is.False);
            Assert.That(resource.HolderId, Is.Null);
            Assert.That(stack.Quantity, Is.Zero);
            Assert.That(registry.TryGet(resource.Id, out _), Is.False);
            Assert.That(holder.Contents, Is.Empty);
            AssertRejectedWithoutRegistryChange(registry, () => registry.ConsumeResource(resource, 1));
            AssertRejectedWithoutRegistryChange(registry, () => registry.TransferResource(resource, holder));

            registry.Remove(holder.Owner);

            Assert.That(holder.Owner.Exists, Is.False);
            Assert.That(registry.Things, Is.Empty);
        }
    }
}
