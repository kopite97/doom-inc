using System;

namespace MobileGame.Gameplay
{
    public sealed class TransportReservation
    {
        public Guid WorkerId { get; internal set; }
        public Guid SourceId { get; internal set; }
        public Guid DestinationId { get; internal set; }
        public Guid ResourceId { get; internal set; }
        public int Amount { get; internal set; }
        public bool PickedUp { get; internal set; }
        public Guid? BatchOriginId { get; internal set; }
        public WorkKind Purpose { get; internal set; }
    }
}
