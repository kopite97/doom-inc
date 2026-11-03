using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public static class CultivationTarget
    {
        public const string None = "";
        public const string Support = "creature.support";
        public const string Invader = "creature.invader";
    }

    // Selected target belongs to the next cycle; ActiveTarget belongs to consumed inputs.
    public sealed class ProductionComponent : ThingComponent
    {
        internal ProductionComponent(Thing owner) : base(owner) { }
        public string SelectedTarget { get; internal set; } = CultivationTarget.None;
        public string ActiveTarget { get; internal set; } = CultivationTarget.None;
        public bool InProgress { get; internal set; }
        public double Progress { get; internal set; }
        public int Outstanding { get; internal set; }
    }
}
