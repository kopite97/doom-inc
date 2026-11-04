using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed class BootstrapLoadingView : IDisposable
    {
        private readonly UiCanvas screen;
        private readonly RectTransform background;
        private readonly TextMeshProUGUI title;
        private readonly ScrollRect scroll;
        private readonly UiList content;
        private readonly UiButton retry;
        private readonly BootstrapText text;

        public BootstrapLoadingView(Transform parent, LocalizationAssets assets, TMP_FontAsset font)
        {
            text = new BootstrapText(assets);
            screen = new UiCanvas(parent, "Temp_LoadingCanvas", 50);
            background = UiWidgets.Panel("Loading", screen.SafeRoot, UiWidgets.Background).rectTransform;
            title = UiWidgets.Label("Title", background, 28);
            title.GetComponent<UiText>().Apply(font, 1);
            scroll = UiWidgets.Scroll("LoadingDetails", background, out var contentRect);
            content = new UiList(contentRect, () => screen.LayoutEpoch);
            retry = UiWidgets.Button("RetryLoading", background, text.Get("retry"), () => screen.LayoutEpoch);
            retry.GetComponentInChildren<UiText>(true).Apply(font, 1);
            content.Style(font, 1, screen.MinimumTouchSize);
        }
        public void Update(string error, bool loading, Action retryAction)
        {
            screen.RefreshLayout(); screen.SetVisible(true);
            var size = screen.Size; var margin = 20f;
            UiWidgets.Place(background, 0, 0, size.x, size.y);
            UiWidgets.Place(title.rectTransform, margin, margin, size.x - margin * 2, 52);
            UiWidgets.Place((RectTransform)scroll.transform, margin, 84, size.x - margin * 2, size.y - 180);
            UiWidgets.Place((RectTransform)retry.transform, margin, size.y - screen.MinimumTouchSize - 24, size.x - margin * 2, screen.MinimumTouchSize);
            title.text = text.Get(error == null ? "loading_wave" : "error_title");
            content.Begin(size.x - margin * 2 - 16);
            content.Text("Status", text.Get(error == null ? "preparing_data" : "error_body"));
            content.End();
            retry.gameObject.SetActive(error != null);
            retry.interactable = !loading;
            retry.Bind(retryAction, error ?? "loading");
        }
        public void SetVisible(bool visible) => screen.SetVisible(visible);
        public void InvalidateInput() => screen.InvalidatePointers();
        public void Dispose() => screen.Dispose();
    }
}
