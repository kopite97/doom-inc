using UnityEngine;

namespace MobileGame.Foundation
{
    public sealed class ResourceData : ThingData
    {
        private ResourceData() { }
        public ResourceData(ResourceDto row, ResourceDefinition asset, GameSettings settings) : base(row, asset, settings) { }

        public override ThingKind Kind => ThingKind.Resource;
    }
}
