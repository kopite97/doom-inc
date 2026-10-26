using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    public enum QuarterTurn { None, Clockwise90, Clockwise180, Clockwise270 }

    // Immutable query value. Origin is the bottom-left of the rotated footprint.
    public sealed class MapPlacement
    {
        internal MapPlacement(Thing thing, Vector2Int origin, QuarterTurn rotation,
            Vector2Int footprint, Vector2Int? interactionCell)
        {
            ThingId = thing.Id;
            CreationOrder = thing.CreationOrder;
            DefinitionId = thing.Definition.DefinitionId;
            DisplayName = thing.Definition.DisplayName;
            Kind = thing.Definition.Kind;
            Origin = origin;
            Rotation = rotation;
            Footprint = footprint;
            InteractionCell = interactionCell;
        }

        public Guid ThingId { get; }
        public long CreationOrder { get; }
        public string DefinitionId { get; }
        public string DisplayName { get; }
        public ThingKind Kind { get; }
        public Vector2Int Origin { get; }
        public QuarterTurn Rotation { get; }
        public Vector2Int Footprint { get; }
        public Vector2Int? InteractionCell { get; }

        public bool Contains(Vector2Int cell) => cell.x >= Origin.x && cell.y >= Origin.y
            && (long)cell.x < (long)Origin.x + Footprint.x
            && (long)cell.y < (long)Origin.y + Footprint.y;
    }
}
