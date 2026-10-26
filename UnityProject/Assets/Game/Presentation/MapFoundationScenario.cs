using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Explicit foundation fixture, NOT the game's initial inventory/construction flow.
    public sealed class MapFoundationScenario
    {
        public MapFoundationScenario(FacilityDefinition storage, CreatureDefinition creature, ResourceDefinition resource)
        {
            Registry = new ThingRegistry();
            Map = new GridMap(Registry, 12, 8);
            var first = Registry.Create(storage);
            var second = Registry.Create(storage);
            var workerA = Registry.Create(creature);
            var workerB = Registry.Create(creature);
            Facilities = Array.AsReadOnly(new[] { first, second });
            Creatures = Array.AsReadOnly(new[] { workerA, workerB });
            Resource = Registry.CreateResource(resource, 5, first.GetComponent<ResourceContainerComponent>());
            Require(Map.Commands.Place(first.Id, new Vector2Int(1, 2)));
            Require(Map.Commands.Place(second.Id, new Vector2Int(7, 3)));
            Require(Map.Commands.Place(workerA.Id, new Vector2Int(4, 1)));
            Require(Map.Commands.Place(workerB.Id, new Vector2Int(4, 1)));
        }

        public ThingRegistry Registry { get; }
        public GridMap Map { get; }
        public IReadOnlyList<Thing> Facilities { get; }
        public IReadOnlyList<Thing> Creatures { get; }
        public Thing Resource { get; }

        private static void Require(PlacementResult result)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException("Invalid foundation fixture: " + result);
        }
    }
}
