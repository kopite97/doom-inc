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
            heading.text = flow.Localization.Format("Title", "heading");
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
                rows.Text("Settings", flow.Localization.Format("Title", "settings_heading"), 24);
                rows.Action("Status", flow.Localization.Format("Common", "status_setting", new MobileGame.Foundation.TextArgument("value", new MobileGame.Foundation.LocalizedMessage("Common", flow.Settings.Current.ShowFacilityStatus ? "on" : "off"))),
                    () => ChangeSettings(true), enabled, "settings");
                rows.Action("Text", flow.Localization.Format("Common", "text_setting", new MobileGame.Foundation.TextArgument("value", new MobileGame.Foundation.LocalizedMessage("Common", flow.Settings.Current.LargeText ? "large" : "normal"))),
                    () => ChangeSettings(false), enabled, "settings");
                LocalizationSettingsRows.Add(rows, flow.Localization, enabled, "title-settings");
                rows.Action("Close", flow.Localization.Format("Common", "back"), () => { settingsOpen = false; screen.InvalidatePointers(); }, enabled, "settings");
            }
            else
            {
                rows.Action("Continue", flow.Localization.Format("Title", "continue"), () => flow.Continue(), enabled && HasSave && SaveError == null, "title");
                rows.Action("New", flow.Localization.Format("Title", "new_game"), () => flow.StartNewGame(), enabled, "title");
                rows.Action("Settings", flow.Localization.Format("Common", "settings"), () => { settingsOpen = true; screen.InvalidatePointers(); }, enabled, "title");
                if (!HasSave) rows.Text("NoSave", flow.Localization.Format("Title", "no_save"));
                if (SaveError != null)
                {
                    rows.Text("SaveError", flow.Localization.Format("Title", "save_error"));
                    rows.Action("RefreshSave", flow.Localization.Format("Title", "retry_save"), RefreshSave, enabled, "title");
                }
            }
            if (flow.Settings.LastError != null)
            {
                rows.Text("SettingsError", flow.Localization.Format("Common", "settings_save_error"));
                rows.Action("RetrySettings", flow.Localization.Format("Common", "retry_settings"), () => flow.Settings.TrySave(), enabled, "settings-error");
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
