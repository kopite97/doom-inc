using System;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class SceneFlowManager
    {
        public PopupHost PopupHost { get; private set; }
        public PopupManager Popups => PopupHost != null ? PopupHost.Manager : null;
        public PopupScope ScenePopupScope { get; private set; } = new PopupScope();
        private PopupHandle prompt;
        public bool BlocksScreenInput => IsTransitioning || Loading?.Visible == true || Popups?.BlocksInput == true;
        private void EnsurePopups()
        {
            if (PopupHost != null) return;
            if (Configuration.popupHost == null) throw new InvalidOperationException("Missing popup host prefab.");
            var host = Instantiate(Configuration.popupHost, transform);
            try
            {
                host.Configure(Configuration.popups, Localization, Configuration.uiAssets?.Font, () => Settings.Current.LargeText);
                host.Manager.Ready = true; host.Manager.SetSuspended(IsTransitioning || Loading.Visible);
                host.Manager.SetForeground(!paused && focused); PopupHost = host;
            }
            catch { Destroy(host.gameObject); throw; }
        }
        private void PromptFinished(PopupResult result)
        { if (result != PopupResult.Confirmed) CancelPrompt(); }
        private void ReleasePopups()
        {
            ScenePopupScope?.Dispose(); ScenePopupScope = new PopupScope();
            if (PopupHost != null) { PopupHost.Manager?.Dispose(); Destroy(PopupHost.gameObject); }
            PopupHost = null; prompt = null;
        }
    }
}
