namespace MobileGame.Foundation
{
    // Stable values shared by progression and the versioned save contract.
    public enum SessionPhase
    {
        Preparing = 0,
        Playing = 1,
        Destruction = 2,
        Results = 3
    }
}
