using System;

namespace MobileGame.Foundation
{
    // Runtime components are C# objects. Their lifetime and owner belong to a Thing.
    public abstract class ThingComponent
    {
        protected ThingComponent(Thing owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public Thing Owner { get; }
    }
}
