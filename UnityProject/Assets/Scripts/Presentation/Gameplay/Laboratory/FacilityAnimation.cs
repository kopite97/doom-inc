using System;

namespace MobileGame.Presentation
{
    // Screen-owned state. Observes committed events and simulation time; never changes production or stock.
    public sealed class FacilityAnimation
    {
        private bool reacting, pending;
        private FacilityArtMotion reaction, next;
        private double began, length, nextAt, nextLength;
        public FacilityArtMotion Motion { get; private set; }
        public void Observe(FacilityArtMotion motion, double now, double duration)
        {
            if (!reacting || now >= began + length)
            { reaction = motion; began = now; length = duration; reacting = true; pending = false; }
            else { next = motion; nextAt = now; nextLength = duration; pending = true; }
        }
        public int Evaluate(double now, FacilityArtSet set, bool working, double progress, bool complete)
        {
            if (!complete) { reacting = pending = false; Motion = FacilityArtMotion.Idle; return set.FrameIndex(Motion, now); }
            if (reacting && now + 1e-8 >= began + length)
            {
                var end = began + length;
                reacting = false;
                if (pending && now - nextAt < nextLength && now < end + nextLength)
                { reaction = next; began = end; length = nextLength; reacting = true; }
                pending = false;
            }
            Motion = reacting ? reaction : working ? FacilityArtMotion.Working : FacilityArtMotion.Idle;
            return set.FrameIndex(Motion, reacting ? now - began : working ? progress : now);
        }
    }
}
