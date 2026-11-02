using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private AppConfiguration configuration;
        [SerializeField] private AppScene sceneRole = AppScene.Laboratory;
        private string savePath;
        private double autoSaveSeconds = 5;
        private bool starting;
        private BootstrapLoadingView loadingView;
        private AppLocalization isolatedLocalization;
        public string Error { get; private set; }
        public bool Ready { get; private set; }
        public AppConfiguration Configuration => configuration;
        public AppScene SceneRole => sceneRole;
        public void Configure(AppConfiguration value, AppScene role) { configuration = value; sceneRole = role; }
        public void SetSaveSlotBeforeStart(string path, double interval = 5)
        {
            if (starting || Ready) throw new InvalidOperationException("Configure save slot before initialization.");
            savePath = path; autoSaveSeconds = interval;
        }
        public Task<GameDataCatalog> PrepareAsync(IGameDataSource source = null)
        {
            if (configuration == null) throw new InvalidOperationException("Missing application configuration.");
            return configuration.PrepareAsync(source);
        }
        private async void Start()
        {
            if (Ready || starting) return;
            // Explicit isolated test slots and diagnostics use the same data/session contracts without menu navigation.
            if (savePath != null || sceneRole == AppScene.Diagnostic) { await StartSessionAsync(); return; }
            try
            {
                if (SceneFlowManager.ConsumeAbandonedEntry(gameObject.scene.path))
                { Error = "Scene transition was canceled. Retry to prepare this screen."; return; }
                if (SceneFlowManager.Instance != null && SceneFlowManager.Instance.IsTransitioning) return;
                var flow = SceneFlowManager.GetOrCreate(configuration);
                if (!flow.IsTransitioning) flow.EnterCurrent(sceneRole, EditorStartChoice);
            }
            catch (Exception e) { Error = e.Message; }
        }
        public static SessionStart EditorStartChoice { get; set; } = SessionStart.NewGame;
        public static string EditorSlotName { get; set; } = "Default";
        public void RestartEditorEntry()
        {
            if (SceneFlowManager.Instance != null && SceneFlowManager.Instance.IsTransitioning) return;
            Ready = starting = false; Error = null; savePath = null;
            GetComponent<LaboratoryView>()?.ReleaseSession(); Start();
        }
        public async Task StartSessionAsync()
        {
            if (starting || Ready) return;
            starting = true; Error = null;
            try
            {
                var data = await PrepareAsync();
                if (this == null) return;
                var entry = GetComponents<MonoBehaviour>().OfType<IGameSceneEntry>().SingleOrDefault();
                if (entry == null) throw new InvalidOperationException("Missing scene session entry.");
                var path = savePath ?? Path.Combine(Application.temporaryCachePath, "Diagnostics", sceneRole.ToString(), "session.json");
                if (isolatedLocalization == null)
                    isolatedLocalization = new AppLocalization(configuration.localization, Path.Combine(Path.GetDirectoryName(path), "language.json"));
                await isolatedLocalization.InitializeAsync();
                if (this == null) return;
                if (entry is LaboratoryView laboratory) laboratory.ConfigureLocalization(isolatedLocalization);
                var succeeded = entry.InitializeSession(data, path, autoSaveSeconds, SessionStart.StartOrContinue);
                // Isolated/diagnostic entries expose their own restore-error UI after data preparation.
                // Product transitions instead require InitializeSession == true before hiding LoadingScreen.
                Ready = true;
                if (!succeeded) Error = entry.InitializationError;
            }
            catch (Exception e) { if (this != null) Error = e.Message; }
            finally { if (this != null) starting = false; }
        }
        public void MarkReady() { Ready = true; Error = null; }
        private void LateUpdate()
        {
            if (Ready || (savePath == null && sceneRole != AppScene.Diagnostic && SceneFlowManager.Instance?.Loading != null))
            { loadingView?.Dispose(); loadingView = null; return; }
            if (loadingView == null && configuration?.localization != null) loadingView = new BootstrapLoadingView(transform, configuration.localization);
            if (loadingView == null) return;
            loadingView.Update(Error, starting, () =>
            {
                if (starting) return;
                if (savePath != null || sceneRole == AppScene.Diagnostic) _ = StartSessionAsync();
                else Start();
            });
        }
        private void OnApplicationPause(bool paused) => loadingView?.InvalidateInput();
        private void OnApplicationFocus(bool focused) => loadingView?.InvalidateInput();
        private void OnDisable() => loadingView?.SetVisible(false);
        private void OnDestroy() { loadingView?.Dispose(); loadingView = null; isolatedLocalization?.Dispose(); isolatedLocalization = null; }
    }
}
