namespace MobileGame.Presentation
{
    public static class LocalizationSettingsRows
    {
        public static void Add(UiList rows, AppLocalization localization, bool enabled, string context)
        {
            if (localization == null || !localization.Texts.IsReady) return;
            rows.Text("LanguageHeading", localization.Format("Common", "language"));
            foreach (var language in localization.Service.Languages)
            {
                var code = language.Code;
                var label = (localization.Service.CurrentLanguage == code ? "● " : "") + language.NativeName;
                rows.Action("Language_" + code, label, () => { _ = localization.SelectLanguageAsync(code); }, enabled, context);
            }
            if (localization.ChangeError != null) rows.Text("LanguageChangeError", localization.Format("Common", "language_error"));
            if (localization.Preferences.LastError != null)
            {
                rows.Text("LanguageSaveError", localization.Format("Common", "language_save_error"));
                rows.Action("LanguageSaveRetry", localization.Format("Common", "retry_settings"),
                    () => localization.Preferences.TrySet(localization.Service.CurrentLanguage), enabled, context);
            }
        }
    }
}
