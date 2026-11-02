namespace MobileGame.Gameplay
{
    public enum ProductionBlockReason
    {
        None, Unsupported, Construction, OutputAdmission, TargetRequired, Unreachable, Inputs, ExcessInputs
    }

    // An observation, never a second owner of the production cycle.
    public readonly struct ProductionReadiness
    {
        public ProductionBlockReason Reason { get; }
        public bool CanOperate => Reason == ProductionBlockReason.None;
        public int Inputs { get; }
        public bool NeedsTarget { get; }
        internal ProductionReadiness(ProductionBlockReason reason, int inputs = 0, bool needsTarget = false)
        { Reason = reason; Inputs = inputs; NeedsTarget = needsTarget; }
    }
}
