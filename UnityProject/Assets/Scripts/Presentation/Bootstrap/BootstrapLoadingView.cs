using System;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed class BootstrapLoadingView : IDisposable
    {
        private readonly UiCanvas screen;
        private readonly RectTransform background;
        private readonly Text title;
        private readonly ScrollRect scroll;
        private readonly UiList content;
        private readonly UiButton retry;

        public BootstrapLoadingView(Transform parent)
        {
            screen = new UiCanvas(parent, "Temp_LoadingCanvas", 50);
            background = UiWidgets.Panel("Loading", screen.SafeRoot, UiWidgets.Background).rectTransform;
            title = UiWidgets.Label("Title", background, 28); title.text = "게임 준비";
            scroll = UiWidgets.Scroll("LoadingDetails", background, out var contentRect);
            content = new UiList(contentRect, () => screen.LayoutEpoch);
            retry = UiWidgets.Button("RetryLoading", background, "불러오기 재시도", () => screen.LayoutEpoch);
        }
        public void Update(string error, bool loading, Action retryAction)
        {
            screen.RefreshLayout(); screen.SetVisible(true);
            var size = screen.Size; var margin = 20f;
            UiWidgets.Place(background, 0, 0, size.x, size.y);
            UiWidgets.Place(title.rectTransform, margin, margin, size.x - margin * 2, 52);
            UiWidgets.Place((RectTransform)scroll.transform, margin, 84, size.x - margin * 2, size.y - 180);
            UiWidgets.Place((RectTransform)retry.transform, margin, size.y - screen.MinimumTouchSize - 24, size.x - margin * 2, screen.MinimumTouchSize);
            content.Begin(size.x - margin * 2 - 16);
            content.Text("Status", error ?? "게임 데이터를 준비하고 있습니다…");
            if (error != null) content.Text("Hint", "게임 데이터를 준비하지 못했습니다. 기존 저장은 보존됩니다. 데이터 원인을 확인한 후 다시 시도하세요.");
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
