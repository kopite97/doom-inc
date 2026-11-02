using System;
using UnityEngine;
using UnityEngine.UI;

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
        public LoadingScreenKind Kind { get; private set; }
        private string retryLabel;
        private string recoverLabel;
        private Font font;
        private Func<bool> largeText;
        public bool Visible { get; private set; }
        public void Configure(Font value, Func<bool> large) { font = value; largeText = large; }
        public void Show(LoadingScreenKind kind, string status, Action retryAction = null, Action recoveryAction = null,
            string retryText = "다시 시도", string recoverText = "메인 메뉴로")
        {
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
            title.text = Kind == LoadingScreenKind.Loading ? "게임 준비 중"
                : Kind == LoadingScreenKind.Confirmation ? "선택 확인" : "준비 중 문제가 발생했습니다";
            UiWidgets.Place(title.rectTransform, 20, 20, size.x - 40, 90);
            UiWidgets.Place((RectTransform)scroll.transform, 20, 120, size.x - 40, Mathf.Max(70, size.y - 140));
            rows.Style(font, largeText?.Invoke() == true ? 1.25f : 1, screen.MinimumTouchSize);
            rows.Begin(size.x - 60); rows.Text("Status", message);
            if (retry != null) rows.Action("Retry", retryLabel, retry, AppUiLifetime.AcceptInput, message);
            if (recover != null) rows.Action("Recover", recoverLabel, recover, AppUiLifetime.AcceptInput, message);
            rows.End();
            title.GetComponent<UiText>().Initialize(24);
            title.GetComponent<UiText>().Apply(font, largeText?.Invoke() == true ? 1.25f : 1);
        }
        private void OnDestroy() { screen?.Dispose(); screen = null; }
    }
}
