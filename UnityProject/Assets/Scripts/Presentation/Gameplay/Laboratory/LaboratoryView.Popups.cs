using MobileGame.Foundation;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        public PopupManager Popups { get; private set; }
        public PopupScope PopupScope { get; private set; }
        private PopupScope scenePopupScope;
        private PopupHandle sessionPrompt;
        private SessionPhase popupPhase;
        private bool externalPopupLifetime;
        public void ConfigurePopups(PopupManager manager, PopupScope sceneScope, bool externalLifetime = false)
        {
            externalPopupLifetime = externalLifetime;
            PopupScope?.Dispose(); Popups = manager; scenePopupScope = sceneScope;
            RenewPopupScope();
        }
        private void RenewPopupScope()
        {
            PopupScope?.Dispose();
            PopupScope = scenePopupScope == null ? null : new PopupScope(scenePopupScope);
            popupPhase = Controller?.Phase ?? SessionPhase.Playing;
            sessionPrompt = null;
        }
        private void SynchronizePopupPhase()
        {
            if (Controller == null || popupPhase == Controller.Phase) return;
            popupPhase = Controller.Phase;
            if (popupPhase == SessionPhase.Destruction || popupPhase == SessionPhase.Results) RenewPopupScope();
        }
        public PopupHandle ShowSettings()
        {
            if (Popups == null || PopupScope?.IsAlive != true || Controller?.Phase == SessionPhase.Destruction) return null;
            return Popups.Show(new SettingsPopupRequest(PopupScope, uiSettings, () => { ApplyTextStyle(); InvalidateInput(); }));
        }
        private void ShowSessionFailure()
        {
            Popups?.Show(new DialogPopupRequest(PopupScope, "session-failure", new LocalizedMessage("Bootstrap", "error_title"),
                M("session_failed"), C("back")));
        }
        private void CompleteSessionAction(LaboratorySessionAction action)
        {
            pendingSessionAction = LaboratorySessionAction.None;
            var success = action == LaboratorySessionAction.Reload ? Reload() : Restart();
            uiNotice = M(success ? "session_changed" : "session_failed");
            if (!success) ShowSessionFailure();
        }
    }
}
