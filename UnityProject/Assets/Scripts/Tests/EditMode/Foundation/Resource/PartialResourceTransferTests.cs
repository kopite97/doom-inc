using MobileGame.TestSupport;
using System;
using System.Linq;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class PartialResourceTransferTests : FoundationTestFixture
    {
        [Test]
        public void PartialTransferConservesQuantityAndWholeTransferKeepsIdentity()
        {
            var registry = new ThingRegistry();
            var holder = CreateHolder(registry, CreateStorageDefinition("fixture.source"));
            var target = CreateHolder(registry, CreateStorageDefinition("fixture.target"));
            var resource = registry.CreateResource(CreateResourceDefinition(), 5, holder);
            var originalOrder = resource.CreationOrder;
            var orderBefore = registry.LastCreationOrder;

            var carried = registry.TransferResource(resource, target, 2);

            Assert.That(carried.Id, Is.Not.EqualTo(resource.Id));
            Assert.That(carried.HolderId, Is.EqualTo(target.Owner.Id));
            Assert.That(carried.Definition, Is.SameAs(resource.Definition));
            Assert.That(carried.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(2));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
            Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
            Assert.That(resource.CreationOrder, Is.EqualTo(originalOrder));
            Assert.That(registry.LastCreationOrder, Is.EqualTo(orderBefore + 1));
            Assert.That(registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(5));

            Assert.That(registry.TransferResource(resource, target, 3), Is.SameAs(resource));
            Assert.That(resource.CreationOrder, Is.EqualTo(originalOrder));
            Assert.That(holder.Contents, Is.Empty);
            Assert.That(target.Contents, Is.EquivalentTo(new[] { carried, resource }));
            Assert.That(registry.LastCreationOrder, Is.EqualTo(orderBefore + 1));
        }

        [Test]
        public void InvalidOrFailedSplitLeavesOriginalAndCreationOrderUntouched()
        {
            var rejectNewStack = false;
            var registry = new ThingRegistry(thing =>
            {
                if (rejectNewStack && thing.Definition.Kind == ThingKind.Resource)
                    throw new InvalidOperationException("Fixture component initialization failed.");
            });
            var holder = CreateHolder(registry, CreateStorageDefinition("fixture.source"));
            var target = CreateHolder(registry, CreateStorageDefinition("fixture.target"));
            var resource = registry.CreateResource(CreateResourceDefinition(), 5, holder);
            var before = registry.Things.ToArray();
            var orderBefore = registry.LastCreationOrder;
            foreach (var amount in new[] { -1, 0, 6 })
                Assert.Throws<ArgumentOutOfRangeException>(() => registry.TransferResource(resource, target, amount));
            var foreign = CreateHolder(new ThingRegistry(), CreateStorageDefinition("fixture.foreign"));
            Assert.Throws<InvalidOperationException>(() => registry.TransferResource(resource, foreign, 2));
            rejectNewStack = true;
            Assert.Throws<InvalidOperationException>(() => registry.TransferResource(resource, target, 2));

            Assert.That(registry.Things, Is.EquivalentTo(before));
            Assert.That(registry.LastCreationOrder, Is.EqualTo(orderBefore));
            Assert.That(resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(resource.HolderId, Is.EqualTo(holder.Owner.Id));
            Assert.That(target.Contents, Is.Empty);
        }

        [Test]
        public void MergePreservesDestinationIdentityAndRejectsInvalidInputsAtomically()
        {
            var registry = new ThingRegistry();
            var holderDefinition = CreateStorageDefinition();
            var holder = CreateHolder(registry, holderDefinition);
            var otherHolder = CreateHolder(registry, holderDefinition);
            var definition = CreateResourceDefinition();
            var destination = registry.CreateResource(definition, 3, holder);
            var source = registry.CreateResource(definition, 2, holder);
            var otherDefinition = registry.CreateResource(CreateResourceDefinition("fixture.other"), 1, holder);
            var otherLocation = registry.CreateResource(definition, 1, otherHolder);
            var otherRegistry = new ThingRegistry();
            var foreignHolder = CreateHolder(otherRegistry, holderDefinition);
            var foreign = otherRegistry.CreateResource(definition, 1, foreignHolder);
            var before = registry.Things.ToArray();
            var orderBefore = registry.LastCreationOrder;
            var id = destination.Id;
            var order = destination.CreationOrder;

            Assert.Throws<ArgumentException>(() => registry.MergeResources(destination, destination));
            Assert.Throws<ArgumentException>(() => registry.MergeResources(destination, otherDefinition));
            Assert.Throws<ArgumentException>(() => registry.MergeResources(destination, otherLocation));
            Assert.Throws<InvalidOperationException>(() => registry.MergeResources(destination, foreign));
            Assert.That(registry.Things, Is.EquivalentTo(before));
            Assert.That(destination.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
            Assert.That(source.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(2));

            registry.MergeResources(destination, source);

            Assert.That(destination.Id, Is.EqualTo(id));
            Assert.That(destination.CreationOrder, Is.EqualTo(order));
            Assert.That(destination.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(source.Exists, Is.False);
            Assert.That(source.HolderId, Is.Null);
            Assert.That(source.GetComponent<ResourceStackComponent>().Quantity, Is.Zero);
            Assert.That(registry.TryGet(source.Id, out _), Is.False);
            Assert.That(registry.LastCreationOrder, Is.EqualTo(orderBefore));

            var excessive = registry.CreateResource(definition, int.MaxValue, holder);
            var overflowBefore = registry.Things.ToArray();
            Assert.Throws<OverflowException>(() => registry.MergeResources(destination, excessive));
            Assert.That(registry.Things, Is.EquivalentTo(overflowBefore));
            Assert.That(destination.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
            Assert.That(excessive.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(int.MaxValue));
        }
    }
}
