using MobileGame.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed class TitleView : MonoBehaviour
    {
        [SerializeField] private Sprite landscapeRoom;
        [SerializeField] private Sprite portraitRoom;
        [SerializeField] private Sprite wordmark;
        [Tooltip("The lettering in the wordmark sprite. Other translations use the localized title text.")]
        [SerializeField] private string wordmarkLine;
        [SerializeField] private Sprite buttonFrame;

        private static readonly Color Ink = new Color32(13, 24, 40, 255);
        private static readonly Color Lettering = new Color32(255, 244, 218, 255);
        private SceneFlowManager flow;
        private UiCanvas screen;
        private Image room;
        private Image emblem;
        private TextMeshProUGUI heading;
        private string lastHeading;
        private ScrollRect scroll;
        private UiList rows;
        private CanvasGroup continueAppearance;

        public bool HasSave { get; private set; }
        public string SaveError { get; private set; }

        public void Initialize(SceneFlowManager owner)
        {
            flow = owner;
            if (screen == null)
            {
                screen = new UiCanvas(transform, "Temp_TitleCanvas", 10);
                var background = UiWidgets.Panel("Background", screen.Canvas.transform, Ink);
                background.raycastTarget = false;
                background.rectTransform.anchorMin = Vector2.zero;
                background.rectTransform.anchorMax = Vector2.one;
                background.rectTransform.offsetMin = background.rectTransform.offsetMax = Vector2.zero;
                background.transform.SetAsFirstSibling();

                room = Illustration("Temp_NightShiftRoom");
                emblem = Illustration("Temp_NightShiftWordmark");
                emblem.sprite = wordmark;
                heading = UiWidgets.Label("Title", screen.SafeRoot);
                heading.color = Lettering;
                heading.alignment = TextAlignmentOptions.Center;
                heading.fontStyle = FontStyles.Bold;
                heading.enableAutoSizing = true;
                heading.fontSizeMin = 10;
                scroll = UiWidgets.Scroll("Menu", screen.SafeRoot, out var content);
                scroll.GetComponent<Image>().color = Color.clear;
                rows = new UiList(content, () => screen.LayoutEpoch, StyleButton);
            }
            RefreshSave();
            Render();
        }

        private Image Illustration(string name)
        {
            var image = UiWidgets.Panel(name, screen.SafeRoot, Color.white);
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private void StyleButton(UiButton button)
        {
            if (button.name == "Continue") continueAppearance = button.gameObject.AddComponent<CanvasGroup>();
            button.image.color = new Color32(40, 66, 86, 255);
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            label.color = Lettering;
            label.fontStyle = FontStyles.Bold;
            if (buttonFrame == null) return;

            // Only draw the sliced edges. The center belongs to the live, localized button.
            var frame = UiWidgets.Panel("Temp_ButtonFrame", button.transform, Color.white);
            frame.raycastTarget = false;
            frame.sprite = buttonFrame;
            frame.type = Image.Type.Sliced;
            frame.fillCenter = false;
            frame.pixelsPerUnitMultiplier = 5;
            frame.rectTransform.anchorMin = Vector2.zero;
            frame.rectTransform.anchorMax = Vector2.one;
            frame.rectTransform.offsetMin = frame.rectTransform.offsetMax = Vector2.zero;
            frame.transform.SetAsFirstSibling();
        }

        public void RefreshSave()
        {
            var probe = SaveFileProbe.Read(flow.SavePath);
            HasSave = probe.State != SaveFileState.Missing;
            SaveError = probe.Error;
        }

        private void LateUpdate() { if (flow != null && screen != null) Render(); }

        private void Render()
        {
            screen.RefreshLayout();
            var size = screen.Size;
            var portrait = screen.Portrait;
            var textScale = flow.Settings.Current.LargeText ? 1.25f : 1f;
            var column = portrait ? size.x : Mathf.Clamp(size.x * 0.34f, Mathf.Min(220, size.x * 0.46f), size.y * 0.73f);
            var menuWidth = column * (portrait ? 0.84f : 0.80f) + 24;
            var buttonHeight = Mathf.Max(screen.MinimumTouchSize, Mathf.Min(size.y * (portrait ? 0.075f : 0.124f), menuWidth * 0.27f));
            rows.Style(flow.Configuration.uiAssets?.Font, textScale, buttonHeight);
            rows.Begin(menuWidth - 16);
            RenderMenu(Mathf.Clamp(Mathf.RoundToInt(buttonHeight * 0.48f), 20, 36));
            rows.End();

            // Additional save notices and retries grow into the illustration's space, then scroll.
            var menuHeight = Mathf.Min(scroll.content.sizeDelta.y + 8, size.y * (portrait ? 0.49f : 0.64f));
            var menuTop = size.y - size.y * 0.025f - menuHeight;
            if (!portrait) menuTop = Mathf.Min(menuTop, size.y * 0.45f);
            UiWidgets.Place((RectTransform)scroll.transform, (column - menuWidth) / 2, menuTop, menuWidth, menuHeight);

            var headerHeight = Mathf.Min(portrait ? size.y * 0.225f : size.y * 0.42f, menuTop * (portrait ? 0.45f : 0.97f));
            UpdateHeading();
            var markHeight = emblem.enabled ? headerHeight * 0.76f : 0;
            if (emblem.enabled) FitIllustration(emblem, column * 0.025f, 0, column * 0.95f, markHeight);
            PlaceHeading(column, markHeight, headerHeight - markHeight, textScale);

            room.sprite = portrait ? portraitRoom : landscapeRoom;
            room.enabled = room.sprite != null;
            if (room.enabled)
            {
                if (portrait) FitIllustration(room, 0, headerHeight, size.x, menuTop - headerHeight);
                else FitIllustration(room, column, 0, size.x - column, size.y);
            }
        }

        private static void FitIllustration(Image image, float left, float top, float width, float height)
        {
            var natural = image.sprite.rect.size;
            var fitted = natural * Mathf.Min(width / natural.x, height / natural.y);
            UiWidgets.Place(image.rectTransform, left + (width - fitted.x) / 2, top + (height - fitted.y) / 2, fitted.x, fitted.y);
        }

        private void UpdateHeading()
        {
            var value = flow.Localization.Format("Title", "heading");
            if (value == lastHeading) return;
            lastHeading = value;
            var split = value.IndexOf('\n');
            emblem.enabled = wordmark != null && split >= 0 && value.Substring(0, split).TrimEnd() == wordmarkLine;
            heading.text = emblem.enabled ? value.Substring(split + 1) : value;
        }

        private void PlaceHeading(float column, float top, float height, float textScale)
        {
            UiWidgets.Place(heading.rectTransform, column * 0.035f, top, column * 0.93f, height);
            var style = heading.GetComponent<UiText>();
            style.Initialize(Mathf.Max(14, Mathf.RoundToInt(height * 0.84f)));
            style.Apply(flow.Configuration.uiAssets?.Font, textScale);
            heading.fontSizeMax = heading.fontSize;
        }

        private void RenderMenu(int fontSize)
        {
            var enabled = !flow.BlocksScreenInput && !flow.IsPrompting && AppUiLifetime.AcceptInput;
            var canContinue = HasSave && SaveError == null;
            rows.Action("Continue", flow.Localization.Format("Title", "continue"), () => flow.Continue(), enabled && canContinue, "title", fontSize);
            continueAppearance.alpha = canContinue ? 1f : 0.55f;
            rows.Action("New", flow.Localization.Format("Title", "new_game"), () => flow.StartNewGame(), enabled, "title", fontSize);
            rows.Action("Settings", flow.Localization.Format("Common", "settings"),
                () => flow.Popups.Show(new SettingsPopupRequest(flow.ScenePopupScope, flow.Settings)), enabled, "title", fontSize);
            if (!HasSave) rows.Text("NoSave", flow.Localization.Format("Title", "no_save"));
            if (SaveError != null)
            {
                rows.Text("SaveError", flow.Localization.Format("Title", "save_error"));
                rows.Action("RefreshSave", flow.Localization.Format("Title", "retry_save"), RefreshSave, enabled, "title");
            }
            if (flow.Settings.LastError != null)
            {
                rows.Text("SettingsError", flow.Localization.Format("Common", "settings_save_error"));
                rows.Action("RetrySettings", flow.Localization.Format("Common", "retry_settings"), () => flow.Settings.TrySave(), enabled, "settings-error");
            }
        }

        private void OnApplicationFocus(bool focused) { screen?.InvalidatePointers(); }
        private void OnApplicationPause(bool paused) { screen?.InvalidatePointers(); }
        private void OnDestroy() { screen?.Dispose(); screen = null; }
    }
}
