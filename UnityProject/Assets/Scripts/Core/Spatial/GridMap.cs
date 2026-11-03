using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobileGame.Foundation
{
    // Canonical placement state; occupancy is derived from these same immutable records.
    // No pathfinder, construction costs, production or global simulation clock here.
    public sealed class GridMap
    {
        private readonly Dictionary<Guid, MapPlacement> placements = new Dictionary<Guid, MapPlacement>();

        // Large custom maps keep the sparse placement representation.
        internal bool IndexCells => (long)Width * Height <= 4096;
        private readonly Dictionary<Vector2Int, Guid> blockingFacilities = new Dictionary<Vector2Int, Guid>();
        internal readonly Dictionary<Vector2Int, GridPathfinder.DistanceTree> Routes = new Dictionary<Vector2Int, GridPathfinder.DistanceTree>();
        public long LayoutVersion { get; private set; }

        public GridMap(ThingRegistry registry, int width, int height)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Map dimensions must be positive.");
            Width = width;
            Height = height;
            Commands = new PlacementCommands(this);
        }

#if UNITY_EDITOR
        public long PathQueries { get; internal set; }
        public long VisitedCells { get; internal set; }
        public long WalkabilityChecks { get; private set; }
#endif
        public ThingRegistry Registry { get; }
        public int Width { get; }
        public int Height { get; }
        public PlacementCommands Commands { get; }
        public bool Contains(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        public bool TryGetPlacement(Guid id, out MapPlacement placement) => placements.TryGetValue(id, out placement);

        public IReadOnlyList<MapPlacement> GetPlacements()
        {
            var snapshot = new List<MapPlacement>(placements.Values);
            snapshot.Sort((left, right) => left.CreationOrder.CompareTo(right.CreationOrder));
            return snapshot.AsReadOnly();
        }

        public Guid? GetBlockingFacility(Vector2Int cell)
        {
            if (IndexCells) return blockingFacilities.TryGetValue(cell, out var owner) ? owner : (Guid?)null;
            foreach (var placement in placements.Values)
                if (placement.Kind == ThingKind.Facility && placement.Contains(cell))
                    return placement.ThingId;
            return null;
        }

        public bool IsWalkable(Vector2Int cell)
        {
#if UNITY_EDITOR
            WalkabilityChecks++;
#endif
            return Contains(cell) && !GetBlockingFacility(cell).HasValue;
        }

        public PlacementResult CheckPlacement(Guid id, Vector2Int origin, QuarterTurn rotation)
            => BuildPlacement(id, origin, rotation, out _);

        // A construction candidate is only definition + geometry until confirmed.
        public PlacementResult CheckPlacement(FacilityData definition, Vector2Int origin,
            QuarterTurn rotation = QuarterTurn.None)
            => CheckGeometry(definition, null, origin, rotation, out _, out _);

        internal PlacementResult ApplyPlacement(Guid id, Vector2Int origin, QuarterTurn rotation)
        {
            var result = BuildPlacement(id, origin, rotation, out var placement);
            if (!result.Succeeded)
                return result;
            Registry.TryGet(id, out var thing);
            if (placement.Kind == ThingKind.Facility)
            {
                if (placements.TryGetValue(id, out var old)) IndexFacility(old, false);
                IndexFacility(placement, true);
            }
            placements[id] = placement;
            thing.Map = this;
            return result;
        }

        internal void Detach(Thing thing)
        {
            if (placements.TryGetValue(thing.Id, out var old) && old.Kind == ThingKind.Facility) IndexFacility(old, false);
            placements.Remove(thing.Id);
            if (ReferenceEquals(thing.Map, this))
                thing.Map = null;
        }

        private void IndexFacility(MapPlacement placement, bool occupied)
        {
            LayoutVersion++;
            Routes.Clear();
            if (!IndexCells) return;
            for (int y = 0; y < placement.Footprint.y; y++)
                for (int x = 0; x < placement.Footprint.x; x++)
                {
                    var cell = placement.Origin + new Vector2Int(x, y);
                    if (occupied) blockingFacilities[cell] = placement.ThingId;
                    else blockingFacilities.Remove(cell);
                }
        }

        private PlacementResult BuildPlacement(Guid id, Vector2Int origin, QuarterTurn rotation,
            out MapPlacement candidate)
        {
            candidate = null;
            if (!Registry.TryGet(id, out var thing) || !thing.Exists)
                return PlacementResult.Reject(PlacementError.UnknownThing);
            if (thing.Map != null && !ReferenceEquals(thing.Map, this))
                return PlacementResult.Reject(PlacementError.OtherMap);
            var result = CheckGeometry(thing.Definition, id, origin, rotation, out var size, out var interaction);
            if (!result.Succeeded) return result;
            candidate = new MapPlacement(thing, origin, rotation, size, interaction);
            return PlacementResult.Success;
        }

        private PlacementResult CheckGeometry(ThingData definition, Guid? excludedId,
            Vector2Int origin, QuarterTurn rotation, out Vector2Int size, out Vector2Int? interaction)
        {
            size = default;
            interaction = null;
            if (definition == null)
                return PlacementResult.Reject(PlacementError.InvalidDefinition);
            if (definition.Kind == ThingKind.Resource)
                return PlacementResult.Reject(PlacementError.ResourceNotSpatial);
            if ((int)rotation < 0 || (int)rotation > 3)
                return PlacementResult.Reject(PlacementError.InvalidRotation);

            var facility = definition as FacilityData;
            try { definition.Validate(); }
            catch (ArgumentException) { return PlacementResult.Reject(PlacementError.InvalidDefinition); }
            catch (InvalidOperationException) { return PlacementResult.Reject(PlacementError.InvalidDefinition); }

            size = facility != null ? FacilityGeometry.GetSize(facility, rotation) : Vector2Int.one;
            if (origin.x < 0 || origin.y < 0
                || (long)origin.x + size.x > Width || (long)origin.y + size.y > Height)
                return PlacementResult.Reject(PlacementError.OutsideBounds);

            foreach (var other in placements.Values)
            {
                if (other.ThingId == excludedId || other.Kind != ThingKind.Facility)
                    continue;
                var overlaps = (long)origin.x < (long)other.Origin.x + other.Footprint.x
                    && (long)other.Origin.x < (long)origin.x + size.x
                    && (long)origin.y < (long)other.Origin.y + other.Footprint.y
                    && (long)other.Origin.y < (long)origin.y + size.y;
                if (overlaps)
                    return PlacementResult.Reject(facility != null
                        ? PlacementError.FacilityOverlap : PlacementError.CellBlocked);
            }

            if (facility != null) interaction = FacilityGeometry.GetInteractionCell(facility, origin, rotation);
            return PlacementResult.Success;
        }
    }
}
