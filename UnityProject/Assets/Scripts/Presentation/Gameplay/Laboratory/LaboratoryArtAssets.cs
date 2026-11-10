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
