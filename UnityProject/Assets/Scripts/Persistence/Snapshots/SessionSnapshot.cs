using System;
using MobileGame.Foundation;

namespace MobileGame.Persistence
{
    // Serialization data only. Runtime ownership remains in ThingRegistry and GridMap.
    [Serializable]
    public sealed class SessionSnapshot
    {
        public int version;
        public string dataHash;
        public string runtimeKind;
        public SessionPhase phase;
        public long lastCreationOrder;
        public int mapWidth;
        public int mapHeight;
        public ThingSnapshot[] things;
        public PlacementSnapshot[] placements;
        public LaboratorySnapshot laboratory;
        public InvasionSnapshot invasion;
    }

    [Serializable]
    public sealed class ThingSnapshot
    {
        public string id;
        public string definitionId;
        public long creationOrder;
        public int quantity;
        public string holderId;
    }

    [Serializable]
    public sealed class PlacementSnapshot
    {
        public string thingId;
        public int x;
        public int y;
        public int rotation;
    }
}
