using UnityEngine;
using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    // Immutable supplied artwork. Playback and per-Thing direction belong to the map view.
    public sealed class LaboratoryArtAssets : ScriptableObject
    {
        [SerializeField] private FacilityDefinition gatheringDefinition;
        [SerializeField] private CreatureDefinition prototypeDefinition;
        [SerializeField] private Sprite[] gatheringFrames;
        [SerializeField] private Sprite[] prototypeFrames;
        [SerializeField] private Vector2[] prototypeHeads;
        [SerializeField] private Sprite[] cargoSprites;
        [SerializeField] private Vector2[] cargoSupports;
        [SerializeField] private FacilityArtSet[] facilities;
        [SerializeField] private CreatureArtSet[] creatures;
        [SerializeField] private Sprite[] boundary;
        [SerializeField] private Sprite birthPulse, releaseTrail;
        [SerializeField] private Sprite[] colonization;
        public Sprite[] Boundary => boundary;
        public Sprite BirthPulse => birthPulse;
        public Sprite ReleaseTrail => releaseTrail;
        public Sprite ColonizationFrame(float progress) => At(colonization, Mathf.Min(31, Mathf.FloorToInt(Mathf.Clamp01(progress) * 32)));
        public int CreaturePlaybackFrame(ThingData definition, int direction, int motion, double time)
        {
            var set = Creature(definition);
            if (set != null) return set.PlaybackFrame(direction, motion, time);
            return direction * 16 + (motion == 0 ? 0 : motion == 1 ? 4 : 10)
                + (int)(time / (motion == 0 ? .25 : motion == 1 ? .1 : .12)) % (motion == 0 ? 4 : 6);
        }
        private CreatureArtSet Creature(ThingData definition)
        {
            if (creatures != null)
                foreach (var entry in creatures)
                    if (entry != null && entry.Matches(definition)) return entry;
            return null;
        }
        // Preserve the serialized prototype binding and its existing sub-sprite identities.
        public bool HasCreature(ThingData definition) => HasPrototype(definition) || Creature(definition) != null;
        public Sprite CreatureFrame(ThingData definition, int index) => HasPrototype(definition)
            ? PrototypeFrame(index) : Creature(definition)?.Frame(index);
        public Vector2 CreatureHead(ThingData definition, int index) => HasPrototype(definition)
            ? PrototypeHead(index) : Creature(definition)?.Head(index) ?? new Vector2(32, 22);
        public bool TryFacility(ThingData definition, out FacilityArtSet result)
        {
            if (facilities != null)
                foreach (var entry in facilities)
                    if (entry != null && entry.Matches(definition)) { result = entry; return true; }
            result = null; return false;
        }
        public Vector2 CargoSupport(Sprite sprite)
        {
            if (cargoSprites != null && cargoSupports != null)
                for (var i = 0; i < cargoSprites.Length && i < cargoSupports.Length; i++)
                    if (cargoSprites[i] == sprite) return cargoSupports[i];
            return new Vector2(sprite.rect.width * .5f, sprite.rect.height);
        }

        public bool HasGathering(ThingData definition) => gatheringDefinition != null
            && definition.DefinitionId == gatheringDefinition.DefinitionId && GatheringFrame(0) != null;
        public bool HasPrototype(ThingData definition) => prototypeDefinition != null
            && definition.DefinitionId == prototypeDefinition.DefinitionId && PrototypeFrame(0) != null;
        public Sprite GatheringFrame(int index) => At(gatheringFrames, index);
        public Sprite PrototypeFrame(int index) => At(prototypeFrames, index);
        // Source coordinates have their origin at the top left, including transparent margins.
        public Vector2 PrototypeHead(int index) => prototypeHeads != null && index >= 0 && index < prototypeHeads.Length
            ? prototypeHeads[index] : new Vector2(32, 24);
        private static Sprite At(Sprite[] frames, int index) => frames != null && index >= 0 && index < frames.Length ? frames[index] : null;
    }
}
