using System;
using MobileGame.Foundation;

namespace MobileGame.Persistence
{
    [Serializable]
    public sealed class InvasionSnapshot
    {
        public int schemaVersion;
        public InvasionSettings settings;
        public long score;
        public long activitySequence;
        public string elapsed;
        public bool goalReached;
        public int legacyEndVersion;
        public InvaderSnapshot[] invaders;
        public InvasionReportSnapshot[] reports;
    }
    [Serializable]
    public sealed class InvaderSnapshot
    {
        public string thingId;
        public bool isExternal;
        public string progress;
        public long completedActivities;
    }
    [Serializable]
    public sealed class InvasionReportSnapshot
    {
        public long sequence;
        public string invaderId;
        public long activity;
        public int score;
        public bool isMajor;
        public string time;
        public long totalScore;
    }
}
