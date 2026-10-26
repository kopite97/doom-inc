using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "FacilityDefinition", menuName = "Mobile Game/Definitions/Facility")]
    public sealed class FacilityDefinition : ThingDefinition
    {
        [SerializeField] private Vector2Int footprint = Vector2Int.one;
        [SerializeField] private Vector2Int interactionOffset = new Vector2Int(0, -1);

        public override ThingKind Kind => ThingKind.Facility;
        public Vector2Int Footprint => footprint;
        public Vector2Int InteractionOffset => interactionOffset;

        public override void Validate()
        {
            base.Validate();
            if (footprint.x <= 0 || footprint.y <= 0)
                throw new ArgumentException("Facility footprint dimensions must be positive.");
            // A fixed cell on one exterior edge, expressed in the unrotated local grid.
            var onVerticalEdge = (interactionOffset.x == -1 || interactionOffset.x == footprint.x)
                && interactionOffset.y >= 0 && interactionOffset.y < footprint.y;
            var onHorizontalEdge = (interactionOffset.y == -1 || interactionOffset.y == footprint.y)
                && interactionOffset.x >= 0 && interactionOffset.x < footprint.x;
            if (!onVerticalEdge && !onHorizontalEdge)
                throw new ArgumentException("The interaction cell must adjoin one footprint edge.");
        }
    }
}
