using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    public static class LaboratorySessionFactory
    {
        public static SessionState Create(ResourceData organic, FacilityData gathering,
            FacilityData storage, CreatureData prototype, ResourceData culture = null, InvasionSettings invasionSettings = null)
        {
            if (organic == null || gathering == null || storage == null || prototype == null)
                throw new ArgumentNullException("Laboratory definitions are required.");
            organic.Validate(); gathering.Validate(); storage.Validate(); prototype.Validate();
            if (gathering.Role != FacilityRole.Gathering || storage.Role != FacilityRole.Storage ||
                !ReferenceEquals(gathering.Resource, organic) || !ReferenceEquals(storage.Resource, organic))
                throw new ArgumentException("Laboratory facilities must use the supplied organic resource.");
            var startingAmount = checked(gathering.BuildCost + storage.BuildCost);
            if (startingAmount > prototype.CarryCapacity)
                throw new ArgumentException("The initial worker must be able to hold both initial construction costs.");
            var registry = new ThingRegistry(GameplayComponentFactory.Attach);
            var map = new GridMap(registry, organic.Settings.MapWidth, organic.Settings.MapHeight);
            var worker = registry.Create(prototype);
            var placement = map.Commands.Place(worker.Id, new Vector2Int(organic.Settings.StartX, organic.Settings.StartY));
            if (!placement.Succeeded) throw new InvalidOperationException(placement.ToString());
            registry.CreateResource(organic, startingAmount, worker.GetComponent<ResourceContainerComponent>());
            return new SessionState(registry, map, new LaboratoryManager(registry, map, organic, culture, invasionSettings));
        }
    }
}
