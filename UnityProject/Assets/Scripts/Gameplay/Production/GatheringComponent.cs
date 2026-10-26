using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class GatheringComponent : ThingComponent
    {
        internal GatheringComponent(Thing owner) : base(owner) { }

        public double Progress { get; internal set; }
        // Includes completed resources carried by workers until actual storage admission.
        public int Outstanding { get; internal set; }
    }
}
