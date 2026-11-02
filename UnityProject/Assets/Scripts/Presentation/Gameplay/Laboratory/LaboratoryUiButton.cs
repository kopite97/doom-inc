using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    // Capture an action on press; a layout or binding change invalidates its later release.
    public sealed class LaboratoryUiButton : Button
    {
        public Func<int> Epoch { get; set; }
        private Action action;
        private Action pressedAction;
        private string context;
        private string pressedContext;
        private int pressedEpoch;
        private int pressedPointer;
        private int appEpoch;

        public void Bind(Action value, string bindingContext)
        { action = value; context = bindingContext; }
        public override void OnPointerDown(PointerEventData data)
        {
            base.OnPointerDown(data);
            pressedAction = null;
            if (data.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()
                || !AppUiLifetime.AcceptInput) return;
            pressedAction = action; pressedContext = context; pressedEpoch = Epoch?.Invoke() ?? 0; pressedPointer = data.pointerId;
            appEpoch = AppUiLifetime.InputEpoch;
        }
        public override void OnPointerClick(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()
                || !AppUiLifetime.AcceptInput || appEpoch != AppUiLifetime.InputEpoch
                || data.pointerId != pressedPointer || pressedEpoch != (Epoch?.Invoke() ?? 0) || pressedContext != context)
            { pressedAction = null; return; }
            var callback = pressedAction; pressedAction = null;
            callback?.Invoke();
        }
        public override void OnSubmit(BaseEventData data)
        {
            if (IsActive() && IsInteractable() && AppUiLifetime.AcceptInput) action?.Invoke();
        }
        protected override void OnDisable() { pressedAction = null; base.OnDisable(); }
    }
}
