namespace MobileGame.Foundation
{
    // GameFlow consumes boundaries, without owning external activity or score rules.
    public interface IExternalSessionRuntime
    {
        double SecondsUntilExternalEvent { get; }
        bool HasReachedGoal { get; }
        void AdvanceExternal(double seconds);
    }
}
