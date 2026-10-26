using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class ConstructionComponent : ThingComponent
    {
        internal ConstructionComponent(Thing owner) : base(owner) { }

        public int Delivered { get; internal set; }
        public double Progress { get; internal set; }
        public bool IsComplete { get; internal set; }
    }
}
