using System;

namespace MobileGame.Foundation
{
    // Groups the canonical owners; does not copy inventory or placement state.
    public sealed class SessionState
    {
        public SessionState(ThingRegistry registry, GridMap map)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            Map = map ?? throw new ArgumentNullException(nameof(map));
            if (!ReferenceEquals(map.Registry, registry))
                throw new ArgumentException("The session Map must use its own registry.", nameof(map));
        }

        public ThingRegistry Registry { get; }
        public GridMap Map { get; }
    }
}
