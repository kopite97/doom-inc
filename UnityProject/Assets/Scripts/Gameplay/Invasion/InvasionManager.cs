using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    // Session-scoped external rules. No event subscription, Unity clock, or duplicate Thing list.
    public sealed class InvasionManager
    {
        private readonly LaboratoryManager laboratory;
        public InvasionManager(LaboratoryManager laboratory, InvasionSettings settings)
        {
            this.laboratory = laboratory ?? throw new ArgumentNullException(nameof(laboratory));
            State = new InvasionState(settings ?? throw new ArgumentNullException(nameof(settings)));
        }
        public InvasionState State { get; }
        public string LastError { get; private set; }
        public int WaitingCount => Invaders().Count(t => !t.GetComponent<InvaderComponent>().IsExternal);
        public int ExternalCount => Invaders().Count(t => t.GetComponent<InvaderComponent>().IsExternal);
        private Thing[] Invaders() => laboratory.Registry.Things.Where(t => t.TryGetComponent<InvaderComponent>(out _))
            .OrderBy(t => t.CreationOrder).ToArray();
        public double SecondsUntilEvent => Invaders().Where(t => t.GetComponent<InvaderComponent>().IsExternal)
            .Select(t => State.Settings.ActivitySeconds - t.GetComponent<InvaderComponent>().Progress)
            .DefaultIfEmpty(double.PositiveInfinity).Min();

        public bool ReleaseAll()
        {
            if (State.GoalReached || State.LegacyEndVersion != 0 || laboratory.IsEditing || laboratory.Map.Commands.IsEditing)
                return Reject("Release is unavailable while editing or after the goal.");
            // Validate the entire target graph before the first Map mutation. Unplace has
            // no callbacks and can only reject absent placements or an active edit.
            try { laboratory.ValidateState(); }
            catch (InvalidDataException error) { return Reject(error.Message); }
            var waiting = Invaders().Where(t => !t.GetComponent<InvaderComponent>().IsExternal).ToArray();
            if (waiting.Length == 0) return Reject("No invaders are waiting.");
            foreach (var thing in waiting)
            {
                var result = laboratory.Map.Commands.Unplace(thing.Id);
                if (!result.Succeeded) throw new InvalidOperationException("Validated release failed: " + result);
                thing.GetComponent<InvaderComponent>().IsExternal = true;
            }
            LastError = null;
            return true;
        }

        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            while (seconds > 0 && !State.GoalReached && State.LegacyEndVersion == 0)
            {
                var step = Math.Min(seconds, SecondsUntilEvent);
                State.Elapsed += step;
                // All completions at the same instant form one committed batch. Goal
                // notification happens after every counter and report is finalized.
                foreach (var thing in Invaders())
                {
                    var invader = thing.GetComponent<InvaderComponent>();
                    if (!invader.IsExternal) continue;
                    var completes = State.Settings.ActivitySeconds - invader.Progress <= step;
                    invader.Progress += step;
                    if (!completes) continue;
                    invader.Progress = 0;
                    invader.CompletedActivities++;
                    var score = State.Settings.ScoreFor(invader.CompletedActivities);
                    var major = State.Settings.IsMajor(invader.CompletedActivities);
                    var detailed = State.DetailedReports;
                    State.Score = checked(State.Score + score);
                    State.ActivitySequence++;
                    if (detailed || major)
                        State.AddReport(new InvasionReport { Sequence = State.ActivitySequence, InvaderId = thing.Id,
                            Activity = invader.CompletedActivities, Score = score, IsMajor = major,
                            Time = State.Elapsed, TotalScore = State.Score });
                }
                seconds -= step;
            }
        }

        public void ValidatePhase(SessionPhase phase)
        {
            ValidateState();
            if (phase == SessionPhase.Playing ? State.GoalReached || State.LegacyEndVersion != 0
                : (phase != SessionPhase.Destruction && phase != SessionPhase.Results) ||
                    (!State.GoalReached && State.LegacyEndVersion == 0))
                throw new InvalidDataException("Session phase and invasion goal disagree.");
        }

        public void ValidateState()
        {
            State.Settings.Validate();
            if (State.Elapsed < 0 || double.IsNaN(State.Elapsed) || double.IsInfinity(State.Elapsed) ||
                State.Score < 0 || State.ActivitySequence < 0)
                throw new InvalidDataException("Invalid external time or score.");
            long score = 0, activities = 0;
            foreach (var thing in laboratory.Registry.Things)
            {
                var isInvader = thing.Definition is CreatureDefinition definition && definition.Role == CreatureRole.Invader;
                var hasState = thing.TryGetComponent<InvaderComponent>(out var invader);
                if (isInvader != hasState) throw new InvalidDataException("Wrong model for invasion state.");
                if (!hasState) continue;
                if (thing.TryGetComponent<WorkerComponent>(out _) || laboratory.TotalAt(thing.Id) != 0 ||
                    (invader.IsExternal ? thing.IsPlaced : !ReferenceEquals(thing.Map, laboratory.Map)) ||
                    invader.CompletedActivities < 0 || double.IsNaN(invader.Progress) || invader.Progress < 0 ||
                    invader.Progress >= State.Settings.ActivitySeconds ||
                    (!invader.IsExternal && (invader.Progress != 0 || invader.CompletedActivities != 0)) ||
                    invader.CompletedActivities * State.Settings.ActivitySeconds + invader.Progress > State.Elapsed + 1e-7)
                    throw new InvalidDataException("Invalid invader placement or activity progress.");
                score = checked(score + State.Settings.ScoreThrough(invader.CompletedActivities));
                activities = checked(activities + invader.CompletedActivities);
            }
            if (score != State.Score || activities != State.ActivitySequence)
                throw new InvalidDataException("Activity counters and aggregate score disagree.");
            if (State.LegacyEndVersion != 0 && ((State.LegacyEndVersion != 2 && State.LegacyEndVersion != 3) ||
                score != 0 || activities != 0 || State.Elapsed != 0 || ExternalCount != 0 || State.Reports.Count != 0))
                throw new InvalidDataException("Legacy ended sessions cannot invent activity history.");
            long previous = 0, previousScore = 0;
            double previousTime = 0;
            var lastReportedActivity = new Dictionary<Guid, long>();
            foreach (var report in State.Reports)
            {
                if (report == null || report.Sequence <= previous || report.Sequence > activities ||
                    !laboratory.Registry.TryGet(report.InvaderId, out var thing) ||
                    !thing.TryGetComponent<InvaderComponent>(out var invader) || !invader.IsExternal ||
                    report.Activity <= 0 || report.Activity > invader.CompletedActivities ||
                    (lastReportedActivity.TryGetValue(report.InvaderId, out var lastActivity) && report.Activity <= lastActivity) ||
                    report.Score != State.Settings.ScoreFor(report.Activity) || report.IsMajor != State.Settings.IsMajor(report.Activity) ||
                    double.IsNaN(report.Time) || report.Time < previousTime || report.Time > State.Elapsed ||
                    report.TotalScore <= previousScore || report.TotalScore > score || report.TotalScore < report.Score ||
                    (!report.IsMajor && report.TotalScore - report.Score >= State.Settings.DetailedUntilScore))
                    throw new InvalidDataException("Invalid report history.");
                previous = report.Sequence; previousTime = report.Time; previousScore = report.TotalScore;
                lastReportedActivity[report.InvaderId] = report.Activity;
            }
        }
        private bool Reject(string error) { LastError = error; return false; }
    }
}
