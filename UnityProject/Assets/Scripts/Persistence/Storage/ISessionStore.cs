namespace MobileGame.Persistence
{
    public interface ISessionStore
    {
        bool Exists { get; }
        string Read();
        void Write(string json);
    }
}
