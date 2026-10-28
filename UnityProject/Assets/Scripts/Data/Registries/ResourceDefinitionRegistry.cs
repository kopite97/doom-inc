using System;
using System.Collections.Generic;
using UnityEngine;
namespace MobileGame.Foundation
{
    [CreateAssetMenu(menuName = "Mobile Game/Registries/Resource")]
    public sealed class ResourceDefinitionRegistry : ScriptableObject
    {
        [SerializeField] private ResourceDefinition[] entries = Array.Empty<ResourceDefinition>();
        public IReadOnlyList<ResourceDefinition> Entries => Array.AsReadOnly(entries ?? throw new InvalidOperationException("Missing Resource entries."));
    }
}
