using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "Invasion", menuName = "Mobile Game/Definitions/Invasion")]
    public sealed class InvasionDefinition : ScriptableObject
    {
        [SerializeField] private InvasionSettings settings = new InvasionSettings();
        public InvasionSettings CreateSettings() { settings.Validate(); return settings.Copy(); }
    }
}
