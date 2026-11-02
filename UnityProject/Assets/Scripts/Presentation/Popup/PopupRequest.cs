using System;
using MobileGame.Foundation;
using MobileGame.Persistence;

namespace MobileGame.Presentation
{
    public enum PopupResult { Confirmed, Canceled, CallerCanceled, OwnerEnded, Failed, Busy, NotReady }
    public enum PopupState { Opening, Open, Closing, WaitingForForeground, Closed }

    public abstract class PopupRequest
    {
        public PopupScope Owner { get; }
        public string Key { get; }
        public string Id { get; }
        public PopupHandle Parent { get; }
        public LocalizedMessage Title { get; }
        public bool CanDismiss { get; }
        protected PopupRequest(string id, PopupScope owner, string key, LocalizedMessage title, PopupHandle parent, bool dismiss)
        {
            Id = id; Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Key = string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("A request key is required.") : key;
            Title = title ?? throw new ArgumentNullException(nameof(title)); Parent = parent; CanDismiss = dismiss;
        }
    }
    public sealed class DialogPopupRequest : PopupRequest
    {
        public LocalizedMessage Message { get; }
        public LocalizedMessage ConfirmText { get; }
        public LocalizedMessage CancelText { get; }
        public DialogPopupRequest(PopupScope owner, string key, LocalizedMessage title, LocalizedMessage message,
            LocalizedMessage confirmText, LocalizedMessage cancelText = null, PopupHandle parent = null, bool canDismiss = true)
            : base(cancelText == null ? "Notice" : "Confirm", owner, key, title, parent, canDismiss)
        { Message = message ?? throw new ArgumentNullException(nameof(message)); ConfirmText = confirmText ?? throw new ArgumentNullException(nameof(confirmText)); CancelText = cancelText; }
    }
    public sealed class SettingsPopupRequest : PopupRequest
    {
        public UiSettingsStore Settings { get; }
        public Action Changed { get; }
        public SettingsPopupRequest(PopupScope owner, UiSettingsStore settings, Action changed = null)
            : base("Settings", owner, "settings", new LocalizedMessage("Common", "settings"), null, true)
        { Settings = settings ?? throw new ArgumentNullException(nameof(settings)); Changed = changed; }
    }
    public sealed class PopupHandle
    {
        internal Action CancelAction;
        public PopupState State { get; internal set; } = PopupState.Opening;
        public PopupResult? Result { get; internal set; }
        public string Error { get; internal set; }
        public bool IsCompleted => Result.HasValue;
        public PopupScope Children { get; }
        internal PopupHandle(PopupScope owner) { Children = new PopupScope(owner); }
        public void Cancel() => CancelAction?.Invoke();
    }
}
