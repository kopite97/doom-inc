using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    public interface IPopupView
    {
        void Open(Action ready);
        void Close(Action finished);
        void SetInteractable(bool enabled);
    }
    public interface IPopupHost
    {
        IPopupView Create(PopupRequest request, Action<PopupResult> choose);
        void Remove(IPopupView view);
        void SetBlocked(bool blocked);
    }

    // No Unity lifecycle, global lookup, simulation state or business operation lives here.
    public sealed class PopupManager : IDisposable
    {
        private sealed class Entry
        {
            public PopupRequest Request;
            public PopupHandle Handle;
            public IPopupView View;
            public Action Confirm, Cancel;
            public Action<PopupResult> Finished;
            public Action Ended;
            public PopupResult Choice;
        }
        private readonly IPopupHost host;
        private readonly Action<Exception> report;
        private readonly List<Entry> stack = new List<Entry>();
        private bool disposed, dispatching;
        public bool Ready { get; set; }
        public bool Foreground { get; private set; } = true;
        public bool Suspended { get; private set; }
        public bool BlocksInput => stack.Count != 0 || dispatching;
        public PopupHandle Top => stack.Count == 0 ? null : stack[stack.Count - 1].Handle;
        public int Count => stack.Count;
        public PopupManager(IPopupHost host, Action<Exception> report = null)
        { this.host = host ?? throw new ArgumentNullException(nameof(host)); this.report = report; }

        public PopupHandle ShowConfirm(PopupScope owner, string key, LocalizedMessage title, LocalizedMessage message,
            Action onConfirm, Action onCancel = null, LocalizedMessage confirmText = null, LocalizedMessage cancelText = null,
            Action<PopupResult> finished = null, PopupHandle parent = null)
            => Show(new DialogPopupRequest(owner, key, title, message, confirmText ?? new LocalizedMessage("Common", "confirm"),
                cancelText ?? new LocalizedMessage("Common", "cancel"), parent), onConfirm, onCancel, finished);

        public PopupHandle Show(PopupRequest request, Action onConfirm = null, Action onCancel = null, Action<PopupResult> finished = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var existing = stack.FirstOrDefault(e => e.Request.Owner == request.Owner && e.Request.Key == request.Key);
            if (existing != null) return existing.Handle;
            if (disposed || !Ready) return Reject(request, PopupResult.NotReady);
            if (!request.Owner.IsAlive || request.Parent?.IsCompleted == true) return Reject(request, PopupResult.OwnerEnded);
            if (Suspended || !Foreground || (stack.Count != 0 && (request.Parent != Top || Top.State != PopupState.Open))
                || (request.Parent != null && (request.Parent != Top || request.Owner != request.Parent.Children)))
                return Reject(request, PopupResult.Busy);
            var entry = new Entry { Request = request, Handle = new PopupHandle(request.Owner), Confirm = onConfirm, Cancel = onCancel, Finished = finished };
            entry.Ended = () => Finish(entry, PopupResult.OwnerEnded);
            entry.Handle.CancelAction = () => Finish(entry, PopupResult.CallerCanceled);
            request.Owner.Ended += entry.Ended;
            stack.Add(entry);
            try
            {
                Refresh();
                if (entry.Handle.IsCompleted) return entry.Handle;
                entry.View = host.Create(request, result => Choose(entry.Handle, result));
                entry.View.SetInteractable(false);
                entry.View.Open(() =>
                {
                    if (entry.Handle.IsCompleted || entry.Handle.State != PopupState.Opening) return;
                    entry.Handle.State = PopupState.Open; Refresh();
                });
            }
            catch (Exception e) { entry.Handle.Error = e.Message; Finish(entry, PopupResult.Failed); Report(e); }
            return entry.Handle;
        }
        private static PopupHandle Reject(PopupRequest request, PopupResult result)
        {
            var handle = new PopupHandle(request.Owner) { State = PopupState.Closed, Result = result };
            handle.Children.Dispose(); return handle;
        }
        public bool Choose(PopupHandle handle, PopupResult result)
        {
            if (disposed || !Foreground || Suspended || handle == null || Top != handle || handle.State != PopupState.Open) return false;
            var entry = stack[stack.Count - 1];
            if (result != PopupResult.Confirmed && result != PopupResult.Canceled) return false;
            if (result == PopupResult.Canceled && !entry.Request.CanDismiss) return false;
            handle.State = PopupState.Closing; entry.Choice = result;
            try
            {
                Refresh();
                if (handle.IsCompleted) return true;
                entry.View.Close(() =>
                {
                    if (handle.IsCompleted) return;
                    handle.State = PopupState.WaitingForForeground;
                    Deliver(entry);
                });
            }
            catch (Exception e) { handle.Error = e.Message; Finish(entry, PopupResult.Failed); Report(e); }
            return true;
        }
        public bool Back()
        {
            if (!BlocksInput) return false;
            if (Top != null) Choose(Top, PopupResult.Canceled);
            return true;
        }
        public void SetForeground(bool value)
        {
            Foreground = value;
            foreach (var entry in stack.ToArray()) Deliver(entry);
            Refresh();
        }
        public void SetSuspended(bool value)
        { Suspended = value; foreach (var entry in stack.ToArray()) Deliver(entry); Refresh(); }
        private void Deliver(Entry entry)
        {
            if (entry.Handle.IsCompleted || entry.Handle.State != PopupState.WaitingForForeground || !Foreground || Suspended) return;
            Finish(entry, entry.Request.Owner.IsAlive ? entry.Choice : PopupResult.OwnerEnded);
        }
        private void Finish(Entry entry, PopupResult result)
        {
            if (entry.Handle.IsCompleted) return;
            entry.Handle.Result = result; entry.Handle.State = PopupState.Closed; entry.Handle.CancelAction = null;
            entry.Request.Owner.Ended -= entry.Ended;
            var previousDispatch = dispatching; dispatching = true;
            var action = result == PopupResult.Confirmed ? entry.Confirm : result == PopupResult.Canceled ? entry.Cancel : null;
            var finished = entry.Finished;
            entry.Confirm = entry.Cancel = null; entry.Finished = null;
            try
            {
                // Cancel children before removing their parent. Their outcomes never invoke user cancel actions.
                Safe(entry.Handle.Children.Dispose);
                stack.Remove(entry);
                Safe(() => host.Remove(entry.View)); entry.View = null;
                if (entry.Request.Owner.IsAlive && action != null) Safe(action);
                if (finished != null) Safe(() => finished(result));
            }
            finally { dispatching = previousDispatch; Refresh(); }
        }
        private void Safe(Action action) { try { action(); } catch (Exception e) { Report(e); } }
        private void Report(Exception e) { try { report?.Invoke(e); } catch { /* cleanup must complete */ } }
        private void Refresh()
        {
            foreach (var entry in stack.ToArray())
            {
                if (entry.Handle.IsCompleted) continue;
                try { entry.View?.SetInteractable(entry.Handle == Top && entry.Handle.State == PopupState.Open && Foreground && !Suspended && !dispatching); }
                catch (Exception e) { entry.Handle.Error = e.Message; Finish(entry, PopupResult.Failed); Report(e); }
            }
            Safe(() => host.SetBlocked(BlocksInput));
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; Ready = false;
            foreach (var entry in stack.AsEnumerable().Reverse().ToArray()) Finish(entry, PopupResult.OwnerEnded);
        }
    }
}
