namespace MobileGame.Foundation
{
    public enum PlacementError
    {
        None, UnknownThing, OtherMap, ResourceNotSpatial, InvalidDefinition,
        InvalidRotation, OutsideBounds, FacilityOverlap, CellBlocked,
        EditInProgress, NoEdit, FacilityRequired, TargetChanged, NotPlaced
    }

    public readonly struct PlacementResult
    {
        private PlacementResult(PlacementError error) { Error = error; }
        public PlacementError Error { get; }
        public bool Succeeded => Error == PlacementError.None;
        public static PlacementResult Success => new PlacementResult(PlacementError.None);
        public static PlacementResult Reject(PlacementError error) => new PlacementResult(error);
        public override string ToString() => Succeeded ? "OK" : Error.ToString();
    }
}
