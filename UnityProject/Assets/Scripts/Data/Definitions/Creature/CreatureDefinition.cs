using UnityEngine;
namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "CreatureDefinition", menuName = "Mobile Game/Definitions/Creature")]
    public sealed class CreatureDefinition : ThingDefinition
    { public override ThingKind Kind => ThingKind.Creature; }
}
