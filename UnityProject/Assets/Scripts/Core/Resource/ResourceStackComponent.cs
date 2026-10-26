using System;

namespace MobileGame.Foundation
{
    public sealed class ResourceStackComponent : ThingComponent
    {
        internal ResourceStackComponent(Thing owner, int quantity) : base(owner)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
        }

        public int Quantity { get; private set; }

        internal void SetQuantity(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
        }
    }
}
