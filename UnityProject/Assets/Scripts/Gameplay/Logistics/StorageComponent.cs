using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class StorageComponent : ThingComponent
    {
        internal StorageComponent(Thing owner) : base(owner) { }

        public bool AllowsCulture { get; internal set; } = true;
        public bool AllowsOrganic { get; internal set; } = true;
    }
}
