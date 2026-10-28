using System;
using System.Collections.Generic;
using UnityEngine;
namespace MobileGame.Foundation
{
    [CreateAssetMenu(menuName = "Mobile Game/Registries/Creature")]
    public sealed class CreatureDefinitionRegistry : ScriptableObject
    {
        [SerializeField] private CreatureDefinition[] entries = Array.Empty<CreatureDefinition>();
        public IReadOnlyList<CreatureDefinition> Entries => Array.AsReadOnly(entries ?? throw new InvalidOperationException("Missing Creature entries."));
    }
}
