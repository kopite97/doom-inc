using MobileGame.TestSupport;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class GridPathfinderTests : FoundationTestFixture
    {
        [Test]
        public void ShortestPathUsesFacilityOccupancyAndIgnoresCreatures()
        {
            var registry = new ThingRegistry();
            var wall = registry.Create(Facility("fixture.wall", 1, 4));
            var creature = registry.Create(CreateDefinition<CreatureData>("fixture.creature"));
            var map = new GridMap(registry, 7, 5);
            map.Commands.Place(wall.Id, new Vector2Int(3, 0));
            map.Commands.Place(creature.Id, new Vector2Int(3, 4));
            var start = new Vector2Int(0, 2);
            var goal = new Vector2Int(6, 2);

            var path = GridPathfinder.FindPath(map, start, goal);

            Assert.That(path.Count, Is.EqualTo(10), "The only route crosses above the four-cell wall.");
            Assert.That(path.Last(), Is.EqualTo(goal));
            Assert.That(path.Contains(start), Is.False);
            Assert.That(path.Contains(creature.Placement.Origin), Is.True);
            Assert.That(path.All(map.IsWalkable), Is.True);
            var previous = start;
            foreach (var cell in path)
            {
                Assert.That(Mathf.Abs(cell.x - previous.x) + Mathf.Abs(cell.y - previous.y), Is.EqualTo(1));
                previous = cell;
            }
            Assert.That(GridPathfinder.FindPath(map, start, goal), Is.EqualTo(path), "Equal routes have stable tie order.");
        }

        [Test]
        public void BlockedStartCanLeaveOnlyThroughAdjacentWalkableCell()
        {
            var registry = new ThingRegistry();
            var facility = registry.Create(Facility("fixture.square", 2, 2));
            var map = new GridMap(registry, 6, 6);
            map.Commands.Place(facility.Id, new Vector2Int(1, 1));
            var coveredStart = new Vector2Int(1, 1);
            var path = GridPathfinder.FindPath(map, coveredStart, new Vector2Int(3, 1));
            Assert.That(path.Count, Is.EqualTo(4), "Covered cells cannot be used to walk through the facility.");
            Assert.That(path.All(map.IsWalkable), Is.True);
            Assert.That(GridPathfinder.FindPath(map, coveredStart, coveredStart), Is.Null);
            Assert.That(GridPathfinder.FindPath(map, Vector2Int.zero, new Vector2Int(2, 2)), Is.Null);

            var enclosing = registry.Create(Facility("fixture.enclosing", 3, 3));
            map.Commands.Unplace(facility.Id);
            map.Commands.Place(enclosing.Id, new Vector2Int(1, 1));
            Assert.That(GridPathfinder.FindPath(map, new Vector2Int(2, 2), Vector2Int.zero), Is.Null,
                "A fully enclosed worker waits; it does not teleport across the footprint.");
        }

        [Test]
        public void LatestMapStateControlsBlockedRecoveredAndOutOfBoundsRoutes()
        {
            var registry = new ThingRegistry();
            var wall = registry.Create(Facility("fixture.wall", 3, 1));
            var map = new GridMap(registry, 3, 5);
            map.Commands.Place(wall.Id, new Vector2Int(0, 2));
            var start = new Vector2Int(1, 1);
            var goal = new Vector2Int(1, 3);
            Assert.That(GridPathfinder.FindPath(map, start, goal), Is.Null);
            map.Commands.Place(wall.Id, new Vector2Int(0, 0));
            Assert.That(GridPathfinder.FindPath(map, start, goal).Count, Is.EqualTo(2));
            Assert.That(GridPathfinder.FindPath(map, start, start), Is.Empty);
            Assert.That(GridPathfinder.FindPath(map, new Vector2Int(-1, 1), goal), Is.Null);
            Assert.That(GridPathfinder.FindPath(map, start, new Vector2Int(3, 3)), Is.Null);
        }

        [Test]
        public void ConstructionCandidateUsesSameChecksWithoutCreatingThingsOrChangingMap()
        {
            var registry = new ThingRegistry();
            var definition = Facility("fixture.facility", 2, 3);
            var existing = registry.Create(definition);
            var unplaced = registry.Create(definition);
            var map = new GridMap(registry, 8, 8);
            map.Commands.Place(existing.Id, Vector2Int.zero);
            var before = registry.Things.ToArray();
            var orderBefore = registry.LastCreationOrder;
            var placements = map.GetPlacements().ToArray();
            foreach (var position in new[] { Vector2Int.zero, new Vector2Int(6, 5), new Vector2Int(7, 7), new Vector2Int(-1, 0) })
            foreach (var rotation in new[] { QuarterTurn.None, QuarterTurn.Clockwise90, (QuarterTurn)4 })
                Assert.That(map.CheckPlacement(definition, position, rotation).Error,
                    Is.EqualTo(map.CheckPlacement(unplaced.Id, position, rotation).Error));

            Assert.That(map.CheckPlacement((FacilityData)null, Vector2Int.zero).Error,
                Is.EqualTo(PlacementError.InvalidDefinition));
            Assert.That(registry.Things, Is.EquivalentTo(before));
            Assert.That(registry.LastCreationOrder, Is.EqualTo(orderBefore));
            Assert.That(map.GetPlacements(), Is.EqualTo(placements));
            Assert.That(unplaced.IsPlaced, Is.False);
        }

        private FacilityData Facility(string id, int width, int height)
        {
            var result = CreateStorageDefinition(id);
            using (var serialized = new DataFixtureEditor(result))
            {
                serialized.FindProperty("footprint").vector2IntValue = new Vector2Int(width, height);
                serialized.FindProperty("interactionOffset").vector2IntValue = new Vector2Int(0, -1);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return result;
        }
    }
}
