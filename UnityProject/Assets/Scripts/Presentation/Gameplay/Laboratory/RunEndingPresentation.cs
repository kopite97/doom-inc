using System;
using MobileGame.Foundation;
using MobileGame.GameFlow;

namespace MobileGame.Presentation
{
    // Transient presentation clock. Both the real view and harness use this implementation.
    public sealed class RunEndingPresentation : IDisposable
    {
        public double Duration { get; }
        public const double MaximumFrameSeconds = 0.1;
        private readonly GameFlowManager flow;
        private readonly GameFlowManager.EndingBinding binding;
        private int generation = -1;
        private SessionPhase phase = SessionPhase.Preparing;
        private bool visible = true;
        private bool skipDelta = true;
        private bool disposed;
        public RunEndingPresentation(GameFlowManager flow, double duration)
        {
            this.flow = flow ?? throw new ArgumentNullException(nameof(flow));
            if (double.IsNaN(duration) || double.IsInfinity(duration) || duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
            Duration = duration;
            binding = flow.BindEndingPresentation();
            Synchronize();
        }
        public GameFlowManager.EndingRequest Request { get; private set; }
        public double Elapsed { get; private set; }
        public double Progress => Math.Min(1, Elapsed / Duration);
        public bool IsEnding => flow.Current != null && (flow.Phase == SessionPhase.Destruction || flow.Phase == SessionPhase.Results);

        public void Synchronize()
        {
            if (disposed || (generation == flow.Generation && phase == flow.Phase)) return;
            flow.ReleaseEndingRequest(Request);
            generation = flow.Generation;
            phase = flow.Phase;
            Elapsed = 0;
            skipDelta = true;
            Request = visible ? flow.AcquireEndingRequest(binding) : null;
        }

        public void SetVisible(bool value)
        {
            if (disposed || visible == value) return;
            visible = value;
            skipDelta = true;
            flow.ReleaseEndingRequest(Request);
            Request = null;
            Synchronize();
            if (visible) Request = flow.AcquireEndingRequest(binding);
        }

        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (disposed) return;
            Synchronize();
            if (!visible || flow.IsApplicationPaused) { skipDelta = true; return; }
            if (skipDelta) { skipDelta = false; return; }
            if (phase != SessionPhase.Destruction || !flow.IsCurrentEndingRequest(Request)) return;
            // A stalled/resumed frame cannot skip the visual sequence. Never use simulation speed here.
            Elapsed = Math.Min(Duration, Elapsed + Math.Min(seconds, MaximumFrameSeconds));
            if (Elapsed >= Duration) { flow.FinishDestruction(Request); Synchronize(); }
        }

        public bool Restart(GameFlowManager.EndingRequest request)
        {
            if (disposed || !visible || !ReferenceEquals(request, Request)) return false;
            var succeeded = flow.RestartFromResults(request);
            Synchronize();
            return succeeded;
        }

        public void Dispose()
        {
            flow.UnbindEndingPresentation(binding);
            flow.ReleaseEndingRequest(Request);
            Request = null;
            disposed = true;
        }
    }
}
