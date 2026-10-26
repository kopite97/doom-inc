using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobileGame.Foundation
{
    public abstract class ThingDefinition : ScriptableObject
    {
        [SerializeField] private string definitionId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private ThingComponentKind[] components = Array.Empty<ThingComponentKind>();

        public string DefinitionId => definitionId;
        public string DisplayName => displayName;
        public abstract ThingKind Kind { get; }
        public IReadOnlyList<ThingComponentKind> ComponentKinds =>
            Array.AsReadOnly(components ?? Array.Empty<ThingComponentKind>());

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(definitionId))
                throw new ArgumentException("A definition must have a nonblank ID.", nameof(definitionId));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A definition must have a nonblank display name.", nameof(displayName));
            if (!Enum.IsDefined(typeof(ThingKind), Kind))
                throw new ArgumentException($"Unknown Thing kind: {Kind}.", nameof(Kind));
            if (components == null)
                throw new InvalidOperationException($"Definition '{definitionId}' has a null component list.");

            var uniqueComponents = new HashSet<ThingComponentKind>();
            foreach (var component in components)
            {
                if (!Enum.IsDefined(typeof(ThingComponentKind), component))
                    throw new ArgumentException($"Unknown component kind: {component}.", nameof(components));
                if (!uniqueComponents.Add(component))
                    throw new InvalidOperationException($"Definition '{definitionId}' repeats component '{component}'.");
            }

            if (Kind == ThingKind.Resource)
            {
                if (components.Length != 1 || components[0] != ThingComponentKind.ResourceStack)
                    throw new InvalidOperationException($"Resource '{definitionId}' must have only ResourceStack.");
            }
            else if (uniqueComponents.Contains(ThingComponentKind.ResourceStack))
            {
                throw new InvalidOperationException($"Definition '{definitionId}' cannot have ResourceStack unless it is a resource.");
            }
        }
    }
}
