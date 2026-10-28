using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class InvaderComponent : ThingComponent
    {
        internal InvaderComponent(Thing owner) : base(owner) { }
        public bool IsExternal { get; internal set; }
        public double Progress { get; internal set; }
        public long CompletedActivities { get; internal set; }
    }
}
