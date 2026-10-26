namespace MobileGame.Foundation
{
    // The flow coordinator owns when to advance; each gameplay runtime owns its rules.
    public interface ISessionRuntime
    {
        bool IsEditing { get; }
        void Advance(double seconds);
        void OnMapChanged();
    }
}
