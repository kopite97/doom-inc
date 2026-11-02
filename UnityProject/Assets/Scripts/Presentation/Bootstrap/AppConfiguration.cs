using System;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Unity asset bindings only. Numeric game definitions remain in JSON.
    public sealed class AppConfiguration : ScriptableObject
    {
        public ResourceDefinitionRegistry resources;
        public CreatureDefinitionRegistry creatures;
        public FacilityDefinitionRegistry facilities;
        public JsonDataAsset[] jsonFiles;
        public LaboratoryUiAssets uiAssets;
        public LoadingScreen loadingScreen;
        public LocalizationAssets localization;
        public string bootstrapScene;
        public string titleScene;
        public string laboratoryScene;

        public Task<GameDataCatalog> PrepareAsync(IGameDataSource source = null)
        {
            var provider = source ?? new LocalJsonDataSource(jsonFiles);
            return GameDataStore.GetOrCreate().InitializeAsync(async token =>
            {
                var bundle = await provider.LoadAsync(token);
                token.ThrowIfCancellationRequested();
                return GameDataComposer.Compose(bundle, resources, creatures, facilities);
            });
        }
    }
    public enum AppScene { Bootstrap, Title, Laboratory, Diagnostic }
    public enum SessionStart { NewGame, Continue, StartOrContinue }
    public interface IGameSceneEntry
    {
        bool InitializeSession(GameDataCatalog data, string path, double interval, SessionStart start);
        string InitializationError { get; }
    }
}
