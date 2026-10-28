using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private ResourceDefinitionRegistry resources;
        [SerializeField] private CreatureDefinitionRegistry creatures;
        [SerializeField] private FacilityDefinitionRegistry facilities;
        [SerializeField] private TextAsset[] csvTables;
        private string savePath;
        private double autoSaveSeconds = 5;
        private bool starting;
        private bool applicationPaused;
        private bool applicationFocused = true;
        private BootstrapLoadingView loadingView;
        public string Error { get; private set; }
        public bool Ready { get; private set; }
        public void SetSaveSlotBeforeStart(string path, double interval = 5)
        {
            if (starting || Ready) throw new InvalidOperationException("Configure save slot before initialization.");
            savePath = path; autoSaveSeconds = interval;
        }
        public Task<GameDataCatalog> PrepareAsync(ICsvDataSource source = null)
        {
            var provider = source ?? new LocalCsvDataSource(csvTables);
            return GameDataStore.GetOrCreate().InitializeAsync(async token =>
            {
                var bundle = await provider.LoadAsync(token);
                token.ThrowIfCancellationRequested();
                return GameDataComposer.Compose(bundle, resources, creatures, facilities);
            });
        }
        private async void Start() => await StartSessionAsync();
        public async Task StartSessionAsync()
        {
            if (starting || Ready) return;
            starting = true; Error = null;
            try
            {
                var data = await PrepareAsync();
                if (this == null) return;
                var laboratory = GetComponent<LaboratoryView>();
                var diagnostic = GetComponent<SessionFoundationView>();
                var map = GetComponent<MapFoundationView>();
                if (laboratory != null && laboratory.Controller == null)
                    laboratory.Initialize(data, savePath ?? Path.Combine(Application.persistentDataPath, "Gameplay", "session.json"), autoSaveSeconds);
                else if (diagnostic != null && diagnostic.Controller == null)
                    diagnostic.Initialize(data.Selected<FacilityData>("storageId"), data.Selected<CreatureData>("prototypeId"),
                        data.Selected<ResourceData>("organicId"), savePath ?? Path.Combine(Application.persistentDataPath, "Diagnostics", "PLAN-007", "session.json"), autoSaveSeconds);
                else if (map != null && map.Session == null)
                    map.Initialize(data.Selected<FacilityData>("storageId"), data.Selected<CreatureData>("prototypeId"), data.Selected<ResourceData>("organicId"));
                Ready = true;
            }
            catch (Exception e) { if (this != null) Error = e.Message; }
            finally { if (this != null) starting = false; }
        }
        private void LateUpdate()
        {
            if (Ready) { loadingView?.Dispose(); loadingView = null; return; }
            if (loadingView == null) loadingView = new BootstrapLoadingView(transform);
            loadingView.Update(Error, starting, () =>
            {
                if (!starting && !applicationPaused && applicationFocused) _ = StartSessionAsync();
            });
        }
        private void OnApplicationPause(bool paused) { applicationPaused = paused; loadingView?.InvalidateInput(); }
        private void OnApplicationFocus(bool focused) { applicationFocused = focused; loadingView?.InvalidateInput(); }
        private void OnDisable() => loadingView?.SetVisible(false);
        private void OnDestroy() { loadingView?.Dispose(); loadingView = null; }
    }
}
