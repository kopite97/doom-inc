using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Four direction rows with definition-specific frame counts and timing.
    [Serializable]
    public sealed class CreatureArtSet
    {
        [SerializeField] private CreatureDefinition definition;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Vector2[] heads;
        [SerializeField] private int framesPerDirection = 16;
        [SerializeField] private float idleSeconds = .25f, moveSeconds = .1f;
        public int PlaybackFrame(int direction, int motion, double time)
        {
            var stride = framesPerDirection > 0 ? framesPerDirection : 16;
            if (motion == 2 && stride < 16) motion = 0;
            var seconds = motion == 0 ? idleSeconds : motion == 1 ? moveSeconds : .12f;
            if (seconds <= 0) seconds = motion == 0 ? .25f : .1f;
            return Mathf.Clamp(direction, 0, 3) * stride + (motion == 0 ? 0 : motion == 1 ? 4 : 10)
                + (int)(time / seconds) % (motion == 0 ? 4 : 6);
        }
        public bool Matches(ThingData value) => definition != null && value.DefinitionId == definition.DefinitionId && Frame(0) != null;
        public Sprite Frame(int index) => frames != null && index >= 0 && index < frames.Length ? frames[index] : null;
        public Vector2 Head(int index) => heads != null && index >= 0 && index < heads.Length ? heads[index] : new Vector2(32, 22);
    }
}
