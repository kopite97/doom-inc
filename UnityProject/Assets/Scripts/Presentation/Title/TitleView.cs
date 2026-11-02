using System;
using MobileGame.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed class TitleView : MonoBehaviour
    {
        private SceneFlowManager flow;
        private UiCanvas screen;
        private Text heading;
        private ScrollRect scroll;
        private UiList rows;
        private bool settingsOpen;
        public bool HasSave { get; private set; }
        public string SaveError { get; private set; }
        public void Initialize(SceneFlowManager owner)
        {
            flow = owner;
            if (screen == null)
            {
                screen = new UiCanvas(transform, "Temp_TitleCanvas", 10);
                var bg = UiWidgets.Panel("Background", screen.Canvas.transform, UiWidgets.Background).rectTransform;
                bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one; bg.offsetMin = bg.offsetMax = Vector2.zero; bg.SetAsFirstSibling();
                heading = UiWidgets.Label("Title", screen.SafeRoot, 36);
                scroll = UiWidgets.Scroll("Menu", screen.SafeRoot, out var content);
                rows = new UiList(content, () => screen.LayoutEpoch);
            }
            RefreshSave(); Render();
        }
        public void RefreshSave()
        {
            var probe = SaveFileProbe.Read(flow.SavePath);
            HasSave = probe.State != SaveFileState.Missing;
            SaveError = probe.Error;
        }
        private void Update()
        {
            if (flow == null || flow.IsTransitioning || flow.IsPrompting || flow.Loading.Visible || !AppUiLifetime.AcceptInput) return;
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true && settingsOpen)
            { settingsOpen = false; screen.InvalidatePointers(); }
        }
        private void LateUpdate() { if (flow != null && screen != null) Render(); }
        private void Render()
        {
            screen.RefreshLayout(); var size = screen.Size;
            var scale = flow.Settings.Current.LargeText ? 1.25f : 1f;
            heading.text = "DOOM INC.\n연구소";
            heading.GetComponent<UiText>().Initialize(32);
            heading.GetComponent<UiText>().Apply(flow.Configuration.uiAssets?.Font, scale);
            var width = Mathf.Min(640, size.x - 32); var x = (size.x - width) / 2;
            UiWidgets.Place(heading.rectTransform, x, 20, width, 100);
            UiWidgets.Place((RectTransform)scroll.transform, x, 132, width, Mathf.Max(80, size.y - 150));
            rows.Style(flow.Configuration.uiAssets?.Font, scale, screen.MinimumTouchSize);
            rows.Begin(width - 24);
            var enabled = !flow.IsTransitioning && !flow.IsPrompting && !flow.Loading.Visible && AppUiLifetime.AcceptInput;
            if (settingsOpen)
            {
                rows.Text("Settings", "화면 표시 설정", 24);
                rows.Action("Status", "시설 상태 표시: " + (flow.Settings.Current.ShowFacilityStatus ? "ON" : "OFF"),
                    () => ChangeSettings(true), enabled, "settings");
                rows.Action("Text", "글자 크기: " + (flow.Settings.Current.LargeText ? "크게" : "보통"),
                    () => ChangeSettings(false), enabled, "settings");
                rows.Action("Close", "돌아가기", () => { settingsOpen = false; screen.InvalidatePointers(); }, enabled, "settings");
            }
            else
            {
                rows.Action("Continue", "이어하기", () => flow.Continue(), enabled && HasSave && SaveError == null, "title");
                rows.Action("New", "새 게임", () => flow.StartNewGame(), enabled, "title");
                rows.Action("Settings", "설정", () => { settingsOpen = true; screen.InvalidatePointers(); }, enabled, "title");
                if (!HasSave) rows.Text("NoSave", "저장된 진행이 없습니다. 새 게임으로 시작하세요.");
                if (SaveError != null)
                {
                    rows.Text("SaveError", "저장을 확인하지 못했습니다.\n" + SaveError);
                    rows.Action("RefreshSave", "저장 확인 재시도", RefreshSave, enabled, "title");
                }
            }
            if (flow.Settings.LastError != null)
            {
                rows.Text("SettingsError", "표시 설정을 저장하지 못했습니다. 현재 선택은 유지됩니다.\n" + flow.Settings.LastError);
                rows.Action("RetrySettings", "설정 저장 재시도", () => flow.Settings.TrySave(), enabled, "settings-error");
            }
            rows.End();
        }
        private void ChangeSettings(bool status)
        {
            var current = flow.Settings.Current;
            flow.Settings.TrySet(status ? !current.ShowFacilityStatus : current.ShowFacilityStatus,
                status ? current.LargeText : !current.LargeText);
            screen.InvalidatePointers();
        }
        private void OnApplicationFocus(bool focused) { screen?.InvalidatePointers(); }
        private void OnApplicationPause(bool paused) { screen?.InvalidatePointers(); }
        private void OnDestroy() { screen?.Dispose(); screen = null; }
    }
}
