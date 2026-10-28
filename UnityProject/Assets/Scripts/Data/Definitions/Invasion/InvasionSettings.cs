using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    // Shared initial values live here; a run keeps its own immutable copy across saves.
    [Serializable]
    public sealed class InvasionSettings
    {
        [SerializeField] private double activitySeconds;
        [SerializeField] private int smallScore;
        [SerializeField] private int majorScore;
        [SerializeField] private int majorEvery;
        [SerializeField] private long targetScore;
        [SerializeField] private long detailedUntilScore;
        [SerializeField] private int reportLimit;

        public InvasionSettings(double activitySeconds, int smallScore, int majorScore, int majorEvery, long targetScore, long detailedUntilScore, int reportLimit)
        {
            this.activitySeconds = activitySeconds; this.smallScore = smallScore; this.majorScore = majorScore;
            this.majorEvery = majorEvery; this.targetScore = targetScore; this.detailedUntilScore = detailedUntilScore; this.reportLimit = reportLimit;
            Validate();
        }
        public double ActivitySeconds => activitySeconds;
        public int SmallScore => smallScore;
        public int MajorScore => majorScore;
        public int MajorEvery => majorEvery;
        public long TargetScore => targetScore;
        public long DetailedUntilScore => detailedUntilScore;
        public int ReportLimit => reportLimit;
        public InvasionSettings Copy() => (InvasionSettings)MemberwiseClone();
        public bool IsMajor(long round) => round > 0 && round % majorEvery == 0;
        public int ScoreFor(long round) => IsMajor(round) ? majorScore : smallScore;
        public long ScoreThrough(long rounds) => checked(rounds * smallScore + (rounds / majorEvery) * (majorScore - smallScore));
        public void Validate()
        {
            if (double.IsNaN(activitySeconds) || double.IsInfinity(activitySeconds) || activitySeconds < 0.01 ||
                smallScore <= 0 || majorScore < smallScore || majorEvery < 2 || targetScore <= 0 ||
                detailedUntilScore < 0 || detailedUntilScore > targetScore || reportLimit < 1 || reportLimit > 1000)
                throw new ArgumentException("Invalid invasion settings.");
        }
    }
}
