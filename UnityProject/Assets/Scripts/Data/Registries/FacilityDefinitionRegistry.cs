using System;
using System.Collections.Generic;
using UnityEngine;
namespace MobileGame.Foundation
{
    [CreateAssetMenu(menuName = "Mobile Game/Registries/Facility")]
    public sealed class FacilityDefinitionRegistry : ScriptableObject
    {
        [SerializeField] private FacilityDefinition[] entries = Array.Empty<FacilityDefinition>();
        public IReadOnlyList<FacilityDefinition> Entries => Array.AsReadOnly(entries ?? throw new InvalidOperationException("Missing Facility entries."));
    }
}
