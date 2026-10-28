using UnityEngine;
namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "FacilityDefinition", menuName = "Mobile Game/Definitions/Facility")]
    public sealed class FacilityDefinition : ThingDefinition
    { public override ThingKind Kind => ThingKind.Facility; }
}
