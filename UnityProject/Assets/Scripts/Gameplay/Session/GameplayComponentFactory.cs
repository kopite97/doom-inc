using System;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    // Injected by a gameplay session; the shared Thing/Core layer has no gameplay dependency.
    public static class GameplayComponentFactory
    {
        public static void Attach(Thing thing)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));
            if (thing.Definition is FacilityDefinition facility && facility.Role != FacilityRole.None)
            {
                thing.GetComponent<ResourceContainerComponent>();
                thing.AddComponent(new ConstructionComponent(thing));
                if (facility.Role == FacilityRole.Gathering)
                    thing.AddComponent(new GatheringComponent(thing));
                else if (facility.Role == FacilityRole.Storage)
                    thing.AddComponent(new StorageComponent(thing));
                else if (facility.HasProduction)
                    thing.AddComponent(new ProductionComponent(thing));
            }
            else if (thing.Definition is CreatureDefinition creature && creature.CanWork)
            {
                thing.GetComponent<ResourceContainerComponent>();
                thing.AddComponent(new WorkerComponent(thing));
            }
            else if (thing.Definition is CreatureDefinition invader && invader.Role == CreatureRole.Invader)
                thing.AddComponent(new InvaderComponent(thing));
        }
    }
}
