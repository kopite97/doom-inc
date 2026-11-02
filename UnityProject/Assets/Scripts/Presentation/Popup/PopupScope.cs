using System;

namespace MobileGame.Presentation
{
    // Ownership is independent of the prefab registry. Child requests end with their parent.
    public sealed class PopupScope : IDisposable
    {
        private readonly PopupScope parent;
        public bool IsAlive { get; private set; } = true;
        public event Action Ended;
        public PopupScope(PopupScope parent = null)
        {
            this.parent = parent;
            if (parent == null) return;
            if (!parent.IsAlive) IsAlive = false;
            else parent.Ended += Dispose;
        }
        public void Dispose()
        {
            if (!IsAlive) return;
            IsAlive = false;
            if (parent != null) parent.Ended -= Dispose;
            var listeners = Ended; Ended = null;
            // One faulty observer must not prevent the remaining owners from cleaning up.
            Exception first = null;
            if (listeners != null) foreach (Action listener in listeners.GetInvocationList())
                try { listener(); } catch (Exception e) { first = first ?? e; }
            if (first != null) throw first;
        }
    }
}
