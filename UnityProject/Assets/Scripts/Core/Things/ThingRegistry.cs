using System;
using System.Collections.Generic;

namespace MobileGame.Foundation
{
    // One instance per running world/session; no static identity or mutable gameplay state.
    // Call on the simulation thread. Jobs, capacity and reservations are later layers.
    public sealed class ThingRegistry
    {
        private readonly Dictionary<Guid, Thing> things = new Dictionary<Guid, Thing>();
        private readonly Dictionary<string, ThingDefinition> definitions =
            new Dictionary<string, ThingDefinition>(StringComparer.Ordinal);
        private long lastCreationOrder;

        public long LastCreationOrder => lastCreationOrder;

        public IEnumerable<Thing> Things
        {
            get
            {
                foreach (var thing in things.Values)
                    yield return thing;
            }
        }

        public bool TryGet(Guid id, out Thing thing) => things.TryGetValue(id, out thing);

        public Thing Create(ThingDefinition definition)
        {
            ValidateDefinition(definition);
            if (definition.Kind == ThingKind.Resource)
                throw new ArgumentException("Resources require a quantity and a holder. Use CreateResource.", nameof(definition));
            return Register(definition, null, 0);
        }

        public Thing CreateResource(ResourceDefinition definition, int quantity,
            ResourceContainerComponent holder)
        {
            ValidateDefinition(definition);
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "A live resource stack must be positive.");
            RequireHolder(holder);
            return Register(definition, holder.Owner.Id, quantity);
        }

        public void TransferResource(Thing resource, ResourceContainerComponent destination)
        {
            RequireResource(resource);
            RequireHolder(destination);
            // All validation precedes this single source-of-truth change. Same-holder transfer is harmless.
            resource.SetHolder(destination.Owner.Id);
        }

        public void ConsumeResource(Thing resource, int amount)
        {
            var stack = RequireResource(resource);
            if (amount <= 0 || amount > stack.Quantity)
                throw new ArgumentOutOfRangeException(nameof(amount));

            stack.SetQuantity(stack.Quantity - amount);
            if (stack.Quantity == 0)
                RemoveEntry(resource);
        }

        // Logical lifecycle only: this is not a player demolition or combat command.
        public void Remove(Thing thing)
        {
            RequireRegistered(thing);
            if (thing.TryGetComponent<ResourceContainerComponent>(out var container))
            {
                foreach (var held in container.Contents)
                    throw new InvalidOperationException("Move or consume held resources before removing their holder.");
            }
            if (thing.TryGetComponent<ResourceStackComponent>(out var stack))
                stack.SetQuantity(0);
            RemoveEntry(thing);
        }

        internal IEnumerable<Thing> GetHeldResources(Thing holder)
        {
            if (!holder.Exists)
                yield break;
            foreach (var thing in things.Values)
                if (thing.HolderId == holder.Id)
                    yield return thing;
        }

        // Restore into a new, unpublished registry. Persistence owns snapshot validation,
        // while this boundary still enforces the normal identity and ownership invariants.
        internal Thing RestoreThing(Guid id, long creationOrder, ThingDefinition definition,
            Guid? holderId, int resourceQuantity)
        {
            ValidateDefinition(definition);
            if (id == Guid.Empty || things.ContainsKey(id))
                throw new ArgumentException("A restored Thing needs a unique, nonempty ID.", nameof(id));
            if (creationOrder <= 0)
                throw new ArgumentOutOfRangeException(nameof(creationOrder));
            foreach (var existing in things.Values)
                if (existing.CreationOrder == creationOrder)
                    throw new ArgumentException("Creation orders must be unique.", nameof(creationOrder));

            if (definition.Kind == ThingKind.Resource)
            {
                if (resourceQuantity <= 0 || !holderId.HasValue ||
                    !things.TryGetValue(holderId.Value, out var holder) ||
                    !holder.TryGetComponent<ResourceContainerComponent>(out _))
                    throw new ArgumentException("A restored resource needs positive quantity and a live container.");
            }
            else if (holderId.HasValue || resourceQuantity != 0)
                throw new ArgumentException("Only a resource may have a holder or quantity.");

            var thing = new Thing(this, id, creationOrder, definition, holderId, resourceQuantity);
            things.Add(id, thing);
            definitions[definition.DefinitionId] = definition;
            lastCreationOrder = Math.Max(lastCreationOrder, creationOrder);
            return thing;
        }

        internal void RestoreHighWater(long value)
        {
            if (value < lastCreationOrder || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "The creation high-water mark cannot move backwards.");
            lastCreationOrder = value;
        }

        private void ValidateDefinition(ThingDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            definition.Validate();
            if (definitions.TryGetValue(definition.DefinitionId, out var known) &&
                !ReferenceEquals(known, definition))
                throw new ArgumentException($"Definition ID already refers to another asset: {definition.DefinitionId}", nameof(definition));
        }

        private Thing Register(ThingDefinition definition, Guid? holderId, int resourceQuantity)
        {
            var order = checked(lastCreationOrder + 1);
            Guid id;
            do { id = Guid.NewGuid(); } while (things.ContainsKey(id));
            var thing = new Thing(this, id, order, definition, holderId, resourceQuantity);
            things.Add(id, thing);
            definitions[definition.DefinitionId] = definition;
            lastCreationOrder = order;
            return thing;
        }

        private void RequireRegistered(Thing thing)
        {
            if (thing == null)
                throw new ArgumentNullException(nameof(thing));
            if (!ReferenceEquals(thing.Registry, this) || !thing.Exists ||
                !things.TryGetValue(thing.Id, out var known) || !ReferenceEquals(known, thing))
                throw new InvalidOperationException("Thing must exist in this registry.");
        }

        private void RequireHolder(ResourceContainerComponent holder)
        {
            if (holder == null)
                throw new ArgumentNullException(nameof(holder));
            RequireRegistered(holder.Owner);
            if (!ReferenceEquals(holder.Owner.GetComponent<ResourceContainerComponent>(), holder))
                throw new InvalidOperationException("Container must belong to its registered Thing.");
        }

        private ResourceStackComponent RequireResource(Thing resource)
        {
            RequireRegistered(resource);
            if (!resource.TryGetComponent<ResourceStackComponent>(out var stack))
                throw new ArgumentException("Thing is not a resource stack.", nameof(resource));
            return stack;
        }

        private void RemoveEntry(Thing thing)
        {
            thing.Map?.Detach(thing);
            things.Remove(thing.Id);
            thing.MarkRemoved();
        }
    }
}
