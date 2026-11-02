using System;
using UnityEngine;
using UnityEngine.UI;
using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    public enum LoadingScreenKind { Loading, Confirmation, Error }

    public sealed class LoadingScreen : MonoBehaviour
    {
        private UiCanvas screen;
        private Text title;
        private ScrollRect scroll;
        private UiList rows;
        private Action retry;
        private Action recover;
        private string message;
        private AppLocalization localization;
        private LocalizedMessage localizedMessage, localizedRetry, localizedRecover;
        public LoadingScreenKind Kind { get; private set; }
        private string retryLabel;
        private string recoverLabel;
        private Font font;
        private Func<bool> largeText;
        public bool Visible { get; private set; }
        public void Configure(Font value, Func<bool> large, AppLocalization texts = null) { font = value; largeText = large; localization = texts; }
        public void Show(LoadingScreenKind kind, LocalizedMessage status, Action retryAction = null, Action recoveryAction = null,
            LocalizedMessage retryText = null, LocalizedMessage recoverText = null)
        {
            Show(kind, status.ToString(), retryAction, recoveryAction, "", "");
            localizedMessage = status;
            localizedRetry = retryText ?? new LocalizedMessage("Bootstrap", "retry");
            localizedRecover = recoverText ?? new LocalizedMessage("Bootstrap", "return_title");
            Render();
        }
        public void Show(LoadingScreenKind kind, string status, Action retryAction = null, Action recoveryAction = null,
            string retryText = null, string recoverText = null)
        {
            localizedMessage = localizedRetry = localizedRecover = null;
            if (!Enum.IsDefined(typeof(LoadingScreenKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            Ensure(); Kind = kind; message = status; retry = retryAction; recover = recoveryAction;
            retryLabel = retryText; recoverLabel = recoverText;
            Visible = true; screen.SetVisible(true); AppUiLifetime.Invalidate(); Render();
        }
        public void Hide() { Visible = false; screen?.SetVisible(false); AppUiLifetime.Invalidate(); }
        private void Ensure()
        {
            if (screen != null) return;
            screen = new UiCanvas(transform, "Temp_LoadingCanvas", 100);
            // Full canvas shield covers areas outside SafeRoot as well.
            var shield = UiWidgets.Panel("InputShield", screen.Canvas.transform, UiWidgets.Background).rectTransform;
            shield.anchorMin = Vector2.zero; shield.anchorMax = Vector2.one; shield.offsetMin = shield.offsetMax = Vector2.zero;
            shield.SetAsFirstSibling();
            title = UiWidgets.Label("LoadingTitle", screen.SafeRoot, 28);
            scroll = UiWidgets.Scroll("LoadingContent", screen.SafeRoot, out var content);
            rows = new UiList(content, () => screen.LayoutEpoch);
        }
        private void LateUpdate() { if (Visible) Render(); }
        private void Render()
        {
            screen.RefreshLayout(); var size = screen.Size;
            title.text = localization == null ? Kind.ToString() : Kind == LoadingScreenKind.Confirmation
                ? localization.Format("Loading", "confirmation") : localization.Format("Bootstrap", Kind == LoadingScreenKind.Loading ? "loading" : "error_title");
            UiWidgets.Place(title.rectTransform, 20, 20, size.x - 40, 90);
            UiWidgets.Place((RectTransform)scroll.transform, 20, 120, size.x - 40, Mathf.Max(70, size.y - 140));
            rows.Style(font, largeText?.Invoke() == true ? 1.25f : 1, screen.MinimumTouchSize);
            rows.Begin(size.x - 60); rows.Text("Status", localizedMessage == null ? message : localization.Format(localizedMessage));
            if (retry != null) rows.Action("Retry", localizedRetry == null ? retryLabel : localization.Format(localizedRetry), retry, AppUiLifetime.AcceptInput, message);
            if (recover != null) rows.Action("Recover", localizedRecover == null ? recoverLabel : localization.Format(localizedRecover), recover, AppUiLifetime.AcceptInput, message);
            rows.End();
            title.GetComponent<UiText>().Initialize(24);
            title.GetComponent<UiText>().Apply(font, largeText?.Invoke() == true ? 1.25f : 1);
        }
        private void OnDestroy() { screen?.Dispose(); screen = null; }
    }
}
