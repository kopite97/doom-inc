namespace MobileGame.Persistence
{
    // User preferences have no session, definition, or GameDataStore ownership.
    public sealed class UiSettings
    {
        public static UiSettings Default { get; } = new UiSettings(true, false);
        public bool ShowFacilityStatus { get; }
        public bool LargeText { get; }

        public UiSettings(bool showFacilityStatus, bool largeText)
        {
            ShowFacilityStatus = showFacilityStatus;
            LargeText = largeText;
        }
    }
}
