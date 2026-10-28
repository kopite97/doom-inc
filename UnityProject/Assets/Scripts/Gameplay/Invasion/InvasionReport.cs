using System;

namespace MobileGame.Gameplay
{
    public sealed class InvasionReport
    {
        public long Sequence { get; internal set; }
        public Guid InvaderId { get; internal set; }
        public long Activity { get; internal set; }
        public int Score { get; internal set; }
        public bool IsMajor { get; internal set; }
        public double Time { get; internal set; }
        public long TotalScore { get; internal set; }
        public string Text => IsMajor ? "A major stronghold has fallen." : "Local defenses disrupted.";
    }
}
