using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour, IPopupView
    {
        [SerializeField] private UnityEngine.UI.Text heading;
        [SerializeField] private UnityEngine.UI.ScrollRect scroll;
        [SerializeField] private UiButton confirm;
        [SerializeField] private UiButton cancel;
        protected PopupHost Host { get; private set; }
        protected PopupRequest Request { get; private set; }
        protected UiList Rows { get; private set; }
        protected bool Interactive { get; private set; }
        private Action<PopupResult> choose;
        private CanvasGroup group;
        private Coroutine animationRoutine;
        private bool initialized, released;
        public UnityEngine.UI.ScrollRect Scroll => scroll;
        public bool HasBindings => heading != null && scroll != null && scroll.content != null && scroll.viewport != null && confirm != null && cancel != null;

        // Called by the asset authoring tool. References remain editable in the prefab.
        public void CreateTemplate()
        {
            if (heading != null) return;
            var background = gameObject.AddComponent<UnityEngine.UI.Image>(); background.color = UiWidgets.PanelColor;
            heading = UiWidgets.Label("PopupTitle", transform, 26);
            scroll = UiWidgets.Scroll("PopupContent", transform, out var content);
            confirm = UiWidgets.Button("Confirm", transform, "", null);
            cancel = UiWidgets.Button("Cancel", transform, "", null);
        }
        internal void Initialize(PopupHost host, PopupRequest request, Action<PopupResult> choice)
        {
            if (initialized) throw new InvalidOperationException("Popup already initialized.");
            if (heading == null || scroll == null || confirm == null || cancel == null)
                throw new InvalidOperationException("Popup prefab bindings are missing.");
            if (!Accepts(request)) throw new InvalidOperationException("Popup request type does not match its prefab.");
            Host = host; Request = request; choose = choice; group = GetComponent<CanvasGroup>();
            heading.GetComponent<UiText>().Initialize(26);
            confirm.GetComponentInChildren<UiText>().Initialize(18);
            cancel.GetComponentInChildren<UiText>().Initialize(18);
            Rows = new UiList(scroll.content, () => Host.LayoutEpoch);
            confirm.Epoch = cancel.Epoch = () => Host.LayoutEpoch;
            confirm.Bind(() => Select(PopupResult.Confirmed), request.Key);
            cancel.Bind(() => Select(PopupResult.Canceled), request.Key);
            initialized = true; SetInteractable(false); Render();
        }
        protected abstract bool Accepts(PopupRequest request);
        protected abstract void Populate(float width);
        protected virtual LocalizedMessage ConfirmLabel => (Request as DialogPopupRequest)?.ConfirmText;
        protected virtual LocalizedMessage CancelLabel => (Request as DialogPopupRequest)?.CancelText ?? new LocalizedMessage("Common", "back");
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected void Select(PopupResult result) { if (Interactive && AppUiLifetime.AcceptInput) choose?.Invoke(result); }
        public void Open(Action ready)
        {
            if (!initialized || released) throw new InvalidOperationException("Popup is not prepared.");
            gameObject.SetActive(true); OnOpen();
            animationRoutine = Host.Animate(group, 0, 1, ready);
        }
        public void Close(Action finished)
        {
            SetInteractable(false);
            if (animationRoutine != null) Host.StopCoroutine(animationRoutine);
            animationRoutine = Host.Animate(group, group.alpha, 0, finished);
        }
        public void SetInteractable(bool enabled)
        {
            Interactive = enabled;
            if (group == null) group = GetComponent<CanvasGroup>();
            group.interactable = group.blocksRaycasts = enabled;
            if (initialized && !released) Render();
        }
        internal void Release()
        {
            if (released) return;
            released = true; choose = null;
            try { if (initialized) OnClose(); }
            finally
            {
                if (animationRoutine != null && Host != null) Host.StopCoroutine(animationRoutine);
                animationRoutine = null; Request = null; Rows = null;
                gameObject.SetActive(false);
            }
        }
        private void LateUpdate() { if (initialized && !released) Render(); }
        private void OnDestroy() { Release(); }
        private void Render()
        {
            var available = Host.Size;
            var width = Mathf.Min(600, available.x - 24);
            var height = Mathf.Min(680, available.y - 24);
            UiWidgets.Place((RectTransform)transform, (available.x - width) / 2, (available.y - height) / 2, width, height);
            var scale = Host.TextScale;
            heading.GetComponent<UiText>().Apply(Host.Font, scale);
            heading.text = Host.Texts.Format(Request.Title);
            UiWidgets.Place(heading.rectTransform, 16, 12, width - 32, 52);
            var headerHeight = Mathf.Max(52, heading.preferredHeight + 16);
            headerHeight = Mathf.Min(headerHeight, height * .25f);
            UiWidgets.Place(heading.rectTransform, 16, 12, width - 32, headerHeight);
            var touch = Host.MinimumTouch;
            var hasConfirm = ConfirmLabel != null;
            var hasCancel = Request is SettingsPopupRequest || (Request is DialogPopupRequest dialog && dialog.CancelText != null);
            var y = height - 16;
            if (hasCancel) y = PlaceButton(cancel, CancelLabel, width, y, touch);
            cancel.gameObject.SetActive(hasCancel);
            if (hasConfirm) y = PlaceButton(confirm, ConfirmLabel, width, y, touch);
            confirm.gameObject.SetActive(hasConfirm);
            UiWidgets.Place((RectTransform)scroll.transform, 12, headerHeight + 28, width - 24, Mathf.Max(40, y - headerHeight - 36));
            Rows.Style(Host.Font, scale, touch); Rows.Begin(width - 48); Populate(width - 48); Rows.End();
        }
        private float PlaceButton(UiButton button, LocalizedMessage message, float width, float bottom, float touch)
        {
            var label = button.GetComponentInChildren<UnityEngine.UI.Text>();
            label.GetComponent<UiText>().Apply(Host.Font, Host.TextScale); label.text = Host.Texts.Format(message);
            UiWidgets.Place((RectTransform)button.transform, 16, 0, width - 32, touch);
            var height = Mathf.Max(touch, label.preferredHeight + 16);
            UiWidgets.Place((RectTransform)button.transform, 16, bottom - height, width - 32, height);
            button.interactable = Interactive && AppUiLifetime.AcceptInput;
            return bottom - height - 8;
        }
    }
}
