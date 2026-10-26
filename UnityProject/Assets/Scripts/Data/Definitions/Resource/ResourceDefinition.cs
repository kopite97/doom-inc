using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "ResourceDefinition", menuName = "Mobile Game/Definitions/Resource")]
    public sealed class ResourceDefinition : ThingDefinition
    {
        public override ThingKind Kind => ThingKind.Resource;
    }
}
