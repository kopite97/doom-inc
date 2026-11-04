using System;
using MobileGame.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public enum LoadingScreenKind { Loading, Confirmation, Error }

    public sealed class LoadingScreen : MonoBehaviour
    {
        private UiCanvas screen;
        private TextMeshProUGUI title;
        private TextMeshProUGUI percent;
        private LoadingTextWave wave;
        private Image track;
        private RectTransform fill;
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
        private TMP_FontAsset font;
        private Func<bool> largeText;
        public bool Visible { get; private set; }

        public void Configure(TMP_FontAsset value, Func<bool> large, AppLocalization texts = null)
        { font = value; largeText = large; localization = texts; }

        public void Show(LoadingScreenKind kind, LocalizedMessage status, Action retryAction = null, Action recoveryAction = null,
            LocalizedMessage retryText = null, LocalizedMessage recoverText = null)
        {
            ValidateKind(kind);
            localizedMessage = status;
            localizedRetry = retryText ?? new LocalizedMessage("Bootstrap", "retry");
            localizedRecover = recoverText ?? new LocalizedMessage("Bootstrap", "return_title");
            message = status.ToString();
            Present(kind, retryAction, recoveryAction);
        }
        public void Show(LoadingScreenKind kind, string status, Action retryAction = null, Action recoveryAction = null,
            string retryText = null, string recoverText = null)
        {
            ValidateKind(kind);
            localizedMessage = localizedRetry = localizedRecover = null;
            message = status; retryLabel = retryText; recoverLabel = recoverText;
            Present(kind, retryAction, recoveryAction);
        }
        private static void ValidateKind(LoadingScreenKind kind)
        {
            if (!Enum.IsDefined(typeof(LoadingScreenKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        }
        private void Present(LoadingScreenKind kind, Action retryAction, Action recoveryAction)
        {
            Ensure(); Kind = kind; retry = retryAction; recover = recoveryAction;
            track.gameObject.SetActive(false);
            percent.gameObject.SetActive(false);
            Visible = true;
            wave.enabled = kind == LoadingScreenKind.Loading;
            screen.SetVisible(true);
            AppUiLifetime.Invalidate();
            Render();
        }
        internal void SetStatus(LocalizedMessage status)
        {
            localizedMessage = status;
            message = status.ToString();
        }
        internal void SetSceneProgress(float observed, bool complete)
        {
            var fraction = Mathf.Clamp01(observed);
            var percentage = complete ? 100 : Mathf.Min(99, Mathf.FloorToInt(fraction * 100f));
            fill.anchorMax = new Vector2(complete ? 1f : fraction, 1);
            percent.SetText("{0:0}%", percentage);
            track.gameObject.SetActive(true);
            percent.gameObject.SetActive(true);
        }
        public void Hide()
        {
            Visible = false;
            if (wave != null) wave.enabled = false;
            screen?.SetVisible(false);
            AppUiLifetime.Invalidate();
        }
        private void Ensure()
        {
            if (screen != null) return;
            screen = new UiCanvas(transform, "Temp_LoadingCanvas", 100);
            screen.SetVisible(false);
            // The shield also covers areas outside the safe area.
            var shield = UiWidgets.Panel("InputShield", screen.Canvas.transform, new Color32(13, 24, 40, 255)).rectTransform;
            shield.anchorMin = Vector2.zero; shield.anchorMax = Vector2.one; shield.offsetMin = shield.offsetMax = Vector2.zero;
            shield.SetAsFirstSibling();
            title = UiWidgets.Label("LoadingTitle", screen.SafeRoot, 24);
            title.color = new Color32(255, 244, 218, 255);
            wave = title.gameObject.AddComponent<LoadingTextWave>();
            wave.enabled = false;
            percent = UiWidgets.Label("ScenePercent", screen.SafeRoot, 20);
            percent.alignment = TextAlignmentOptions.MidlineRight;
            var accent = new Color32(242, 185, 100, 255);
            percent.color = accent;
            track = UiWidgets.Panel("SceneProgress", screen.SafeRoot, new Color32(45, 65, 80, 255));
            track.raycastTarget = false;
            var bar = UiWidgets.Panel("Fill", track.transform, accent);
            bar.raycastTarget = false;
            fill = bar.rectTransform;
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            scroll = UiWidgets.Scroll("LoadingContent", screen.SafeRoot, out var content);
            rows = new UiList(content, () => screen.LayoutEpoch);
        }
        private void LateUpdate() { if (Visible) Render(); }
        private void Render()
        {
            screen.RefreshLayout();
            var size = screen.Size;
            var loading = Kind == LoadingScreenKind.Loading;
            var scale = largeText?.Invoke() == true ? 1.25f : 1;
            title.text = localization == null ? Kind.ToString() : Kind == LoadingScreenKind.Confirmation
                ? localization.Format("Loading", "confirmation") : localization.Format("Bootstrap", loading ? "loading_wave" : "error_title");

            var width = loading ? Mathf.Min(520, size.x - 48) : size.x - 40;
            var left = (size.x - width) * 0.5f;
            var top = loading ? Mathf.Max(20, size.y * 0.45f - 48) : 20;
            title.alignment = loading ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.TopLeft;
            UiWidgets.Place(title.rectTransform, left, top, loading ? width - 88 : width, loading ? 64 : 90);
            UiWidgets.Place(percent.rectTransform, left + width - 84, top, 84, 64);
            UiWidgets.Place(track.rectTransform, left, top + 71, width, 6);
            var contentTop = top + (loading ? 91 : 100);
            UiWidgets.Place((RectTransform)scroll.transform, left, contentTop, width, Mathf.Max(70, size.y - contentTop - 20));
            scroll.GetComponent<Image>().color = loading ? Color.clear : UiWidgets.PanelColor;
            var titleStyle = title.GetComponent<UiText>();
            titleStyle.Initialize(loading ? 36 : 24);
            titleStyle.Apply(font, scale);
            percent.GetComponent<UiText>().Apply(font, scale);
            rows.Style(font, scale, screen.MinimumTouchSize);
            rows.Begin(width - 20);
            rows.Text("Status", localizedMessage == null ? message : localization.Format(localizedMessage));
            if (retry != null) rows.Action("Retry", localizedRetry == null ? retryLabel : localization.Format(localizedRetry), retry, AppUiLifetime.AcceptInput, message);
            if (recover != null) rows.Action("Recover", localizedRecover == null ? recoverLabel : localization.Format(localizedRecover), recover, AppUiLifetime.AcceptInput, message);
            rows.End();
        }
        private void OnDestroy() { screen?.Dispose(); screen = null; }
    }
}
