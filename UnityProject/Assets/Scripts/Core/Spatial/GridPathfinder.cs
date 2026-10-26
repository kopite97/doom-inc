using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobileGame.Foundation
{
    public static class GridPathfinder
    {
        // Stable four-neighbour order only breaks ties between equally short routes.
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down
        };

        // Returned steps exclude start and include goal. Null means no walkable route.
        public static IReadOnlyList<Vector2Int> FindPath(GridMap map, Vector2Int start, Vector2Int goal)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (!map.Contains(start) || !map.IsWalkable(goal)) return null;
            if (start == goal) return Array.Empty<Vector2Int>();

            var parents = new Dictionary<Vector2Int, Vector2Int>();
            var frontier = new Queue<Vector2Int>();
            parents.Add(start, start);
            frontier.Enqueue(start);
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                foreach (var offset in Directions)
                {
                    var next = current + offset;
                    if (!map.IsWalkable(next) || parents.ContainsKey(next)) continue;
                    parents.Add(next, current);
                    if (next == goal) return Reconstruct(parents, start, goal);
                    frontier.Enqueue(next);
                }
            }
            return null;
        }

        private static IReadOnlyList<Vector2Int> Reconstruct(
            Dictionary<Vector2Int, Vector2Int> parents, Vector2Int start, Vector2Int goal)
        {
            var result = new List<Vector2Int>();
            for (var cell = goal; cell != start; cell = parents[cell]) result.Add(cell);
            result.Reverse();
            return result.AsReadOnly();
        }
    }
}
