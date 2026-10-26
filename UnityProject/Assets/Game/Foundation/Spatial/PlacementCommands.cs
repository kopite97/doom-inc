using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    public sealed class PlacementEdit
    {
        internal PlacementEdit(Guid id, MapPlacement original, Vector2Int origin,
            QuarterTurn rotation, PlacementResult validation)
        {
            ThingId = id; Original = original; Origin = origin; Rotation = rotation; Validation = validation;
        }
        public Guid ThingId { get; }
        public Vector2Int Origin { get; }
        public QuarterTurn Rotation { get; }
        public PlacementResult Validation { get; }
        internal MapPlacement Original { get; }
    }

    // The map has one command service: UI and harness use the same instance and rules.
    public sealed class PlacementCommands
    {
        private readonly GridMap map;
        internal PlacementCommands(GridMap map) { this.map = map; }
        public PlacementEdit Edit { get; private set; }
        public bool IsEditing => Edit != null;

        public PlacementResult Place(Guid id, Vector2Int origin, QuarterTurn rotation = QuarterTurn.None)
        {
            if (IsEditing)
                return PlacementResult.Reject(PlacementError.EditInProgress);
            return map.ApplyPlacement(id, origin, rotation);
        }

        public PlacementResult Unplace(Guid id)
        {
            if (IsEditing)
                return PlacementResult.Reject(PlacementError.EditInProgress);
            if (!map.Registry.TryGet(id, out var thing))
                return PlacementResult.Reject(PlacementError.UnknownThing);
            if (thing.Map != null && !ReferenceEquals(thing.Map, map))
                return PlacementResult.Reject(PlacementError.OtherMap);
            if (!thing.IsPlaced)
                return PlacementResult.Reject(PlacementError.NotPlaced);
            map.Detach(thing);
            return PlacementResult.Success;
        }

        public PlacementResult BeginEdit(Guid id)
        {
            if (IsEditing)
                return PlacementResult.Reject(PlacementError.EditInProgress);
            if (!map.Registry.TryGet(id, out var thing))
                return PlacementResult.Reject(PlacementError.UnknownThing);
            if (thing.Definition.Kind != ThingKind.Facility)
                return PlacementResult.Reject(PlacementError.FacilityRequired);
            if (thing.Map != null && !ReferenceEquals(thing.Map, map))
                return PlacementResult.Reject(PlacementError.OtherMap);
            var original = thing.Placement;
            var origin = original?.Origin ?? Vector2Int.zero;
            var rotation = original?.Rotation ?? QuarterTurn.None;
            Edit = new PlacementEdit(id, original, origin, rotation, map.CheckPlacement(id, origin, rotation));
            return PlacementResult.Success;
        }

        public PlacementResult Preview(Vector2Int origin, QuarterTurn rotation)
        {
            if (!IsEditing)
                return PlacementResult.Reject(PlacementError.NoEdit);
            var result = map.CheckPlacement(Edit.ThingId, origin, rotation);
            Edit = new PlacementEdit(Edit.ThingId, Edit.Original, origin, rotation, result);
            return result;
        }

        public PlacementResult Confirm()
        {
            if (!IsEditing)
                return PlacementResult.Reject(PlacementError.NoEdit);
            map.TryGetPlacement(Edit.ThingId, out var current);
            if (!ReferenceEquals(Edit.Original, current))
                return PlacementResult.Reject(PlacementError.TargetChanged);
            // Revalidate at commit time; previews do not reserve space or mutate placement.
            var result = map.ApplyPlacement(Edit.ThingId, Edit.Origin, Edit.Rotation);
            if (result.Succeeded)
                Edit = null;
            return result;
        }

        public PlacementResult Cancel()
        {
            if (!IsEditing)
                return PlacementResult.Reject(PlacementError.NoEdit);
            Edit = null;
            return PlacementResult.Success;
        }
    }
}
