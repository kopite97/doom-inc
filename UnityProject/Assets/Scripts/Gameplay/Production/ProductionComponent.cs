using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public enum CultivationTarget { None = 0, Support = 1, Invader = 2 }

    // Selected target belongs to the next cycle; ActiveTarget belongs to consumed inputs.
    public sealed class ProductionComponent : ThingComponent
    {
        internal ProductionComponent(Thing owner) : base(owner) { }
        public CultivationTarget SelectedTarget { get; internal set; }
        public CultivationTarget ActiveTarget { get; internal set; }
        public bool InProgress { get; internal set; }
        public double Progress { get; internal set; }
        public int Outstanding { get; internal set; }
    }
}
