using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public enum FacilityArtMotion { Idle, Working, Complete, Receive, Send }

    [Serializable]
    public sealed class FacilityArtSet
    {
        [SerializeField] private FacilityDefinition definition;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float[] seconds;
        public bool Matches(ThingData data) => definition != null && data != null
            && definition.DefinitionId == data.DefinitionId && frames != null && frames.Length == 16 && Frame(0) != null;
        public Sprite Frame(int index) => frames != null && index >= 0 && index < frames.Length ? frames[index] : null;
        private static void Range(FacilityArtMotion motion, out int start, out int count)
        {
            start = motion == FacilityArtMotion.Idle ? 0 : motion == FacilityArtMotion.Complete ? 12
                : motion == FacilityArtMotion.Send ? 10 : 4;
            count = motion == FacilityArtMotion.Working ? 8
                : motion == FacilityArtMotion.Receive || motion == FacilityArtMotion.Send ? 6 : 4;
        }
        public double Duration(FacilityArtMotion motion)
        {
            Range(motion, out var start, out var count);
            var total = 0d;
            for (var i = start; i < start + count; i++) total += Seconds(i);
            return total;
        }
        public int FrameIndex(FacilityArtMotion motion, double elapsed)
        {
            Range(motion, out var start, out var count);
            var time = Math.Max(0, elapsed);
            if (motion == FacilityArtMotion.Idle || motion == FacilityArtMotion.Working) time %= Duration(motion);
            for (var i = start; i < start + count - 1; i++)
            {
                var duration = Seconds(i);
                if (time + 1e-8 < duration) return i;
                time -= duration;
            }
            return start + count - 1;
        }
        private double Seconds(int index) => seconds != null && index < seconds.Length && seconds[index] > 0
            ? Math.Round(seconds[index], 6) : .1;
    }
}
