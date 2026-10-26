using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    // Shared geometry projection for placement validation and its visual preview.
    // Geometry does not imply that the footprint is inside the map or unobstructed.
    public static class FacilityGeometry
    {
        public static Vector2Int GetSize(FacilityDefinition definition, QuarterTurn rotation)
        {
            Require(definition, rotation);
            var original = definition.Footprint;
            return (int)rotation % 2 == 0 ? original : new Vector2Int(original.y, original.x);
        }

        public static Vector2Int GetInteractionCell(FacilityDefinition definition, Vector2Int origin,
            QuarterTurn rotation)
        {
            Require(definition, rotation);
            var original = definition.Footprint;
            var offset = definition.InteractionOffset;
            switch (rotation)
            {
                case QuarterTurn.Clockwise90:
                    offset = new Vector2Int(offset.y, original.x - 1 - offset.x);
                    break;
                case QuarterTurn.Clockwise180:
                    offset = new Vector2Int(original.x - 1 - offset.x, original.y - 1 - offset.y);
                    break;
                case QuarterTurn.Clockwise270:
                    offset = new Vector2Int(original.y - 1 - offset.y, offset.x);
                    break;
            }
            return origin + offset;
        }

        private static void Require(FacilityDefinition definition, QuarterTurn rotation)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if ((int)rotation < 0 || (int)rotation > 3) throw new ArgumentOutOfRangeException(nameof(rotation));
        }
    }
}
