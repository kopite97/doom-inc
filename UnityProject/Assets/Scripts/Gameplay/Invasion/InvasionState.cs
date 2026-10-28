using System.Collections.Generic;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class InvasionState
    {
        private readonly List<InvasionReport> reports = new List<InvasionReport>();
        internal InvasionState(InvasionSettings settings) { settings.Validate(); Settings = settings.Copy(); }
        public InvasionSettings Settings { get; }
        public long Score { get; internal set; }
        public long ActivitySequence { get; internal set; }
        public double Elapsed { get; internal set; }
        public int LegacyEndVersion { get; internal set; }
        public bool GoalReached => Score >= Settings.TargetScore;
        public bool DetailedReports => Score < Settings.DetailedUntilScore;
        public IReadOnlyList<InvasionReport> Reports => reports.AsReadOnly();
        internal void AddReport(InvasionReport report)
        {
            reports.Add(report);
            if (reports.Count > Settings.ReportLimit) reports.RemoveAt(0);
        }
    }
}
