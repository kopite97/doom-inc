using UnityEngine;

namespace MobileGame.Gameplay
{
    // Only the latest successful logical step is retained. It is neither a queue nor saved state.
    public readonly struct WorkerMoveObservation
    {
        public WorkerMoveObservation(long sequence, Vector2Int from, Vector2Int to,
            double atSeconds, bool isWander, long layoutVersion)
        {
            Sequence = sequence;
            From = from;
            To = to;
            AtSeconds = atSeconds;
            IsWander = isWander;
            LayoutVersion = layoutVersion;
        }

        // Zero denotes a worker which has not committed an observed step in this session.
        public long Sequence { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public double AtSeconds { get; }
        public bool IsWander { get; }
        public long LayoutVersion { get; }
    }
}
