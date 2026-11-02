namespace MobileGame.Gameplay
{
    // Derived runtime observation; reset on restore and recomputed by work/path selection.
    public enum WorkerWaitReason
    {
        None, TargetUnavailable, TargetUnreachable, PathBlocked, PrioritiesDisabled,
        NoWork, FacilityClaimed, OutputAdmission, NoStorage, TransportChanged
    }
}
