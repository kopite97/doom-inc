using System;
using UnityEngine;
namespace MobileGame.Foundation
{
    public abstract class ThingDefinition : ScriptableObject
    {
        [SerializeField] private string definitionId = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;
        public string DefinitionId => definitionId;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public abstract ThingKind Kind { get; }
        public void Validate()
        { if (string.IsNullOrWhiteSpace(definitionId) || definitionId != definitionId.Trim()) throw new ArgumentException("Invalid SO definition ID: " + name); }
    }
    public sealed class DefinitionAssets
    {
        public Sprite Icon { get; }
        public GameObject Prefab { get; }
        public DefinitionAssets(ThingDefinition source) { Icon = source.Icon; Prefab = source.Prefab; }
    }
}
