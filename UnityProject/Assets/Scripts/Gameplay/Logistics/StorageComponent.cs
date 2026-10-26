using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class StorageComponent : ThingComponent
    {
        internal StorageComponent(Thing owner) : base(owner) { }

        public bool AllowsOrganic { get; internal set; } = true;
    }
}
