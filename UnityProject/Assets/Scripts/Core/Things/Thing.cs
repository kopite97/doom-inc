using System;
using System.Collections.Generic;

namespace MobileGame.Foundation
{
    public sealed class Thing
    {
        private readonly List<ThingComponent> configuredComponents = new List<ThingComponent>();
        private readonly IReadOnlyList<ThingComponent> components;

        internal Thing(ThingRegistry registry, Guid id, long creationOrder,
            ThingData definition, Guid? holderId, int resourceQuantity)
        {
            Registry = registry;
            Id = id;
            CreationOrder = creationOrder;
            Definition = definition;
            HolderId = holderId;
            Exists = true;

            components = configuredComponents.AsReadOnly();
            foreach (var kind in definition.ComponentKinds)
            {
                switch (kind)
                {
                    case ThingComponentKind.ResourceStack:
                        AddComponent(new ResourceStackComponent(this, resourceQuantity));
                        break;
                    case ThingComponentKind.ResourceContainer:
                        AddComponent(new ResourceContainerComponent(this));
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported component: {kind}");
                }
            }
        }

        internal ThingRegistry Registry { get; }
        public Guid Id { get; }
        public long CreationOrder { get; }
        public ThingData Definition { get; }
        public bool Exists { get; private set; }
        public GridMap Map { get; internal set; }
        public MapPlacement Placement => Map != null && Map.TryGetPlacement(Id, out var placement)
            ? placement : null;
        public bool IsPlaced => Placement != null;
        public Guid? HolderId { get; private set; }
        public IReadOnlyList<ThingComponent> Components => components;

        public bool TryGetComponent<T>(out T component) where T : ThingComponent
        {
            foreach (var candidate in components)
            {
                if (candidate is T match)
                {
                    component = match;
                    return true;
                }
            }
            component = null;
            return false;
        }

        public T GetComponent<T>() where T : ThingComponent
        {
            if (TryGetComponent<T>(out var component))
                return component;
            throw new InvalidOperationException($"Thing {Id} has no {typeof(T).Name}.");
        }

        internal void AddComponent(ThingComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (!ReferenceEquals(component.Owner, this))
                throw new ArgumentException("A component must belong to this Thing.", nameof(component));
            foreach (var existing in configuredComponents)
                if (existing.GetType() == component.GetType())
                    throw new InvalidOperationException($"Thing {Id} already has {component.GetType().Name}.");
            configuredComponents.Add(component);
        }

        internal void SetHolder(Guid holderId) => HolderId = holderId;

        internal void MarkRemoved()
        {
            HolderId = null;
            Exists = false;
        }
    }
}
