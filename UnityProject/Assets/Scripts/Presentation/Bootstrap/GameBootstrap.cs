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
        private void OnGUI()
        {
            if (Ready) return;
            GUI.Box(new Rect(20, 20, Mathf.Max(300, Screen.width - 40), 140), "Game data");
            GUI.Label(new Rect(40, 50, Screen.width - 80, 60), Error ?? "Loading definitions...");
            if (!starting && Error != null && GUI.Button(new Rect(40, 115, 140, 30), "Retry loading")) _ = StartSessionAsync();
        }
    }
}
