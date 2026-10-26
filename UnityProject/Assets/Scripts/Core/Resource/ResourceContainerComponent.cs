using System.Collections.Generic;

namespace MobileGame.Foundation
{
    public sealed class ResourceContainerComponent : ThingComponent
    {
        internal ResourceContainerComponent(Thing owner) : base(owner) { }

        // This is a derived view, not a second mutable inventory or a reservation ledger.
        public IEnumerable<Thing> Contents => Owner.Registry.GetHeldResources(Owner);
    }
}
