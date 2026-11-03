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

        // A source tree belongs to its map and expires when a facility changes the walkable grid.
        internal sealed class DistanceTree
        {
            private readonly Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
            internal DistanceTree(GridMap map, Vector2Int start)
            {
                var frontier = new Queue<Vector2Int>();
                distances.Add(start, 0);
                frontier.Enqueue(start);
#if UNITY_EDITOR
                map.PathQueries++;
#endif
                while (frontier.Count > 0)
                {
                    var cell = frontier.Dequeue();
#if UNITY_EDITOR
                    map.VisitedCells++;
#endif
                    int distance = distances[cell] + 1;
                    foreach (var direction in Directions)
                    {
                        var next = cell + direction;
                        if (!map.IsWalkable(next) || distances.ContainsKey(next)) continue;
                        distances.Add(next, distance);
                        frontier.Enqueue(next);
                    }
                }
            }
            internal int? To(Vector2Int goal) => distances.TryGetValue(goal, out var distance) ? distance : (int?)null;
        }

        public static int? Distance(GridMap map, Vector2Int start, Vector2Int goal)
        {
            if (!map.Contains(start) || !map.IsWalkable(goal)) return null;
            if (start == goal) return 0;
            if (!map.IndexCells) return FindPath(map, start, goal)?.Count;
            if (!map.Routes.TryGetValue(start, out var tree))
            {
                if (map.Routes.Count >= 32) map.Routes.Clear();
                map.Routes.Add(start, tree = new DistanceTree(map, start));
            }
            return tree.To(goal);
        }

        // Returned steps exclude start and include goal. Null means no walkable route.
        public static IReadOnlyList<Vector2Int> FindPath(GridMap map, Vector2Int start, Vector2Int goal)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
#if UNITY_EDITOR
            map.PathQueries++;
#endif
            if (!map.Contains(start) || !map.IsWalkable(goal)) return null;
            if (start == goal) return Array.Empty<Vector2Int>();

            var parents = new Dictionary<Vector2Int, Vector2Int>();
            var frontier = new Queue<Vector2Int>();
            parents.Add(start, start);
            frontier.Enqueue(start);
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
#if UNITY_EDITOR
                map.VisitedCells++;
#endif
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
