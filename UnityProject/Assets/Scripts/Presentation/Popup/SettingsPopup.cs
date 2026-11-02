using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    public sealed class SettingsPopup : PopupBase
    {
        protected override bool Accepts(PopupRequest request) => request is SettingsPopupRequest;
        protected override void Populate(float width)
        {
            var data = (SettingsPopupRequest)Request;
            var settings = data.Settings.Current;
            var enabled = Interactive && AppUiLifetime.AcceptInput;
            Rows.Action("Status", Host.Texts.Format(new LocalizedMessage("Common", "status_setting", new TextArgument("value",
                new LocalizedMessage("Common", settings.ShowFacilityStatus ? "on" : "off")))), () =>
                Change(!data.Settings.Current.ShowFacilityStatus, data.Settings.Current.LargeText), enabled, Request.Key);
            Rows.Action("Text", Host.Texts.Format(new LocalizedMessage("Common", "text_setting", new TextArgument("value",
                new LocalizedMessage("Common", settings.LargeText ? "large" : "normal")))), () =>
                Change(data.Settings.Current.ShowFacilityStatus, !data.Settings.Current.LargeText), enabled, Request.Key);
            LocalizationSettingsRows.Add(Rows, Host.Texts, enabled, Request.Key);
            if (data.Settings.LastError != null)
            {
                Rows.Text("SettingsError", Host.Texts.Format("Common", "settings_save_error"));
                Rows.Action("RetrySettings", Host.Texts.Format("Common", "retry_settings"), () => data.Settings.TrySave(), enabled, Request.Key);
            }
        }
        private void Change(bool status, bool large)
        {
            var data = (SettingsPopupRequest)Request;
            data.Settings.TrySet(status, large); data.Changed?.Invoke(); Host.Invalidate();
        }
    }
}
