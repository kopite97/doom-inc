using System;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public enum GameDataLoadState { Uninitialized, Loading, Ready, Failed }

    public sealed class GameDataStore : MonoBehaviour
    {
        public static GameDataStore Instance { get; private set; }
        public GameDataLoadState State { get; private set; }
        public GameDataCatalog Catalog { get; private set; }
        public string Error { get; private set; }
        private CancellationTokenSource lifetime;
        private Task<GameDataCatalog> pending;
        private int generation;
        private int ownerThread;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Instance = null; }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; ownerThread = Thread.CurrentThread.ManagedThreadId;
            lifetime = new CancellationTokenSource(); DontDestroyOnLoad(gameObject);
        }
        public static GameDataStore GetOrCreate()
        {
            if (Instance == null)
            {
                // Also supports entering Play without scene reload.
                var existing = FindFirstObjectByType<GameDataStore>();
                if (existing != null) existing.ReinitializeLifetime();
                else new GameObject("GameDataStore").AddComponent<GameDataStore>();
            }
            return Instance;
        }
        private void ReinitializeLifetime()
        {
            generation++; lifetime?.Cancel(); lifetime?.Dispose(); lifetime = new CancellationTokenSource();
            pending = null; Catalog = null; Error = null; State = GameDataLoadState.Uninitialized;
            Instance = this; ownerThread = Thread.CurrentThread.ManagedThreadId; DontDestroyOnLoad(gameObject);
        }
        public Task<GameDataCatalog> InitializeAsync(Func<CancellationToken, Task<GameDataCatalog>> load)
        {
            if (Instance != this || Thread.CurrentThread.ManagedThreadId != ownerThread) throw new InvalidOperationException("Initialize the active Store on its Unity thread.");
            if (State == GameDataLoadState.Ready) return Task.FromResult(Catalog);
            if (State == GameDataLoadState.Loading) return pending;
            if (load == null) throw new ArgumentNullException(nameof(load));
            State = GameDataLoadState.Loading; Error = null;
            pending = Load(load, ++generation, lifetime.Token);
            return pending;
        }
        private async Task<GameDataCatalog> Load(Func<CancellationToken, Task<GameDataCatalog>> load, int request, CancellationToken token)
        {
            // Publish pending before even a synchronous local provider completes.
            await Task.Yield();
            try
            {
                token.ThrowIfCancellationRequested();
                var candidate = await load(token);
                token.ThrowIfCancellationRequested();
                if (request != generation || Instance != this) throw new OperationCanceledException();
                if (candidate == null) throw new InvalidOperationException("Loader returned no catalog.");
                Catalog = candidate; State = GameDataLoadState.Ready; return candidate;
            }
            catch (Exception e)
            {
                if (request == generation && Instance == this) { Error = e.Message; State = GameDataLoadState.Failed; }
                throw;
            }
        }
        private void OnDestroy()
        {
            if (Instance != this) return;
            generation++; lifetime?.Cancel(); lifetime?.Dispose(); lifetime = null;
            Instance = null; Catalog = null; pending = null;
        }
    }
}
