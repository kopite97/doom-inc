using System;
using UnityEngine;

namespace MobileGame.Gameplay
{
    // A read-only projection of the currently valid logical movement interval.
    public readonly struct WorkerMoveSegment
    {
        public WorkerMoveSegment(Vector2Int from, Vector2Int to, double progressSeconds,
            double durationSeconds, Guid targetId, WorkKind kind, JobStage stage, long layoutVersion)
        {
            From = from;
            To = to;
            ProgressSeconds = progressSeconds;
            DurationSeconds = durationSeconds;
            TargetId = targetId;
            Kind = kind;
            Stage = stage;
            LayoutVersion = layoutVersion;
        }

        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public double ProgressSeconds { get; }
        public double DurationSeconds { get; }
        public Guid TargetId { get; }
        public WorkKind Kind { get; }
        public JobStage Stage { get; }
        public long LayoutVersion { get; }
    }
}
