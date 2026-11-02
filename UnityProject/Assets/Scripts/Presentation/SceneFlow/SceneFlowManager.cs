using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MobileGame.Presentation
{
    public sealed class SceneFlowManager : MonoBehaviour
    {
        public static SceneFlowManager Instance { get; private set; }
        public AppConfiguration Configuration { get; private set; }
        public string SavePath { get; private set; }
        public bool IsTransitioning { get; private set; }
        public bool IsPrompting { get; private set; }
        public string Error { get; private set; }
        public LoadingScreen Loading { get; private set; }
        public LaboratoryView Laboratory { get; private set; }
        public AppLocalization Localization { get; private set; }
        public UiSettingsStore Settings => AppUiSettings.ForSlot(SavePath);
        private int generation;
        private Action cancelPrompt;
        private bool paused;
        private bool focused = true;
        private static AsyncOperation pendingSceneLoad;
        private static string pendingScenePath;
        private static bool abandonedSceneEntry;
        private sealed class Request
        {
            public AppScene Target;
            public SessionStart Start;
            public bool UseCurrent;
            public bool Committed;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        { Instance = null; pendingSceneLoad = null; pendingScenePath = null; abandonedSceneEntry = false; }
        public static bool ConsumeAbandonedEntry(string scenePath)
        {
            if (!abandonedSceneEntry || pendingScenePath != scenePath) return false;
            abandonedSceneEntry = false;
            return true;
        }
        public static string DefaultSavePath
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.temporaryCachePath, "EditorSessions", GameBootstrap.EditorSlotName, "session.json");
#else
                return Path.Combine(Application.persistentDataPath, "Gameplay", "session.json");
#endif
            }
        }
        public static SceneFlowManager GetOrCreate(AppConfiguration config, string slot = null)
        {
            if (config == null) throw new InvalidOperationException("Missing application configuration.");
            if (config.loadingScreen == null) throw new InvalidOperationException("Missing shared LoadingScreen prefab.");
            var resolvedSlot = Path.GetFullPath(slot ?? DefaultSavePath);
            if (Instance == null)
            {
                var existing = FindFirstObjectByType<SceneFlowManager>();
                if (existing != null) { existing.ResetLifetime(); Instance = existing; }
                else new GameObject("SceneFlowManager").AddComponent<SceneFlowManager>();
            }
            if (Instance.Configuration == null)
            {
                var preferences = Path.Combine(Application.isEditor ? Application.temporaryCachePath : Application.persistentDataPath,
                    "Preferences", "language.json");
                Instance.Localization = new AppLocalization(config.localization, preferences);
                Instance.Configuration = config; Instance.SavePath = resolvedSlot;
                Instance.Loading = Instantiate(config.loadingScreen, Instance.transform);
                Instance.Loading.Configure(config.uiAssets?.Font, () => Instance != null && Instance.Settings.Current.LargeText, Instance.Localization);
                Instance.Loading.Hide();
            }
            if (Instance.Configuration != config) throw new InvalidOperationException("Application configuration cannot change during a run.");
            return Instance;
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject); AppUiLifetime.Ensure();
        }
        private void ResetLifetime()
        {
            generation++; StopAllCoroutines(); IsTransitioning = IsPrompting = false;
            Localization?.Dispose(); Localization = null;
            if (Loading != null) Destroy(Loading.gameObject);
            Loading = null; Configuration = null; Laboratory = null; cancelPrompt = null; Error = null;
        }
        public bool EnterCurrent(AppScene scene, SessionStart start = SessionStart.NewGame)
            => Begin(new Request { Target = scene == AppScene.Bootstrap ? AppScene.Title : scene,
                Start = start, UseCurrent = scene != AppScene.Bootstrap });
        public bool Continue() => Begin(new Request { Target = AppScene.Laboratory, Start = SessionStart.Continue });
        public bool StartNewGame(bool confirmed = false)
        {
            if (IsTransitioning || IsPrompting || paused || !focused) return false;
            if (!confirmed)
            {
                var probe = SaveFileProbe.Read(SavePath);
                if (probe.State == SaveFileState.Readable) { ShowConfirmation(); return true; }
                if (probe.State == SaveFileState.Unreadable)
                { Fail(probe.Error, () => StartNewGame(), null); return false; }
            }
            return Begin(new Request { Target = AppScene.Laboratory, Start = SessionStart.NewGame });
        }
        private void ShowConfirmation()
        {
            IsPrompting = true;
            cancelPrompt = CancelPrompt;
            Loading.Show(LoadingScreenKind.Confirmation, new LocalizedMessage("Loading", "new_game_prompt"),
                () => { CancelPrompt(); StartNewGame(true); }, CancelPrompt, new LocalizedMessage("Loading", "start_new"), new LocalizedMessage("Common", "cancel"));
        }
        public bool RequestReturnToTitle()
        {
            if (IsTransitioning || IsPrompting || Laboratory?.Controller?.Current == null || paused || !focused
                || Laboratory.Controller.Phase == SessionPhase.Destruction) return false;
            IsPrompting = true; Laboratory.SetTransitionPaused(true); cancelPrompt = CancelPrompt;
            Loading.Show(LoadingScreenKind.Confirmation, new LocalizedMessage("Loading", "return_prompt"), ConfirmReturn,
                CancelPrompt, new LocalizedMessage("Loading", "save_return"), new LocalizedMessage("Common", "cancel"));
            return true;
        }
        public void CancelPrompt()
        {
            if (IsTransitioning) return;
            IsPrompting = false; cancelPrompt = null; Error = null; Loading.Hide();
            if (Laboratory != null) Laboratory.SetTransitionPaused(false);
        }
        private void ConfirmReturn()
        {
            if (!IsPrompting || Laboratory == null || paused || !focused) return;
            if (Laboratory.IsEditing) Laboratory.CancelPlacement();
            if (!Laboratory.Save())
            {
                Error = Laboratory.Controller.LastSaveError ?? Laboratory.Controller.LastError;
                Debug.LogWarning("Save before title failed: " + Error);
                Loading.Show(LoadingScreenKind.Error, new LocalizedMessage("Loading", "save_failed"), ConfirmReturn, CancelPrompt,
                    new LocalizedMessage("Loading", "retry_save"), new LocalizedMessage("Loading", "cancel_return"));
                return;
            }
            IsPrompting = false; cancelPrompt = null;
            Begin(new Request { Target = AppScene.Title });
        }
        private bool Begin(Request request)
        {
            if (IsTransitioning || IsPrompting || paused || !focused) return false;
            IsTransitioning = true; Error = null; generation++;
            Laboratory?.SetTransitionPaused(true); AppUiLifetime.Invalidate();
            StartCoroutine(Guard(Transition(request, generation), request, generation));
            return true;
        }
        private IEnumerator Guard(IEnumerator work, Request request, int token)
        {
            while (token == generation)
            {
                bool next; object value = null; Exception failure = null;
                try { next = work.MoveNext(); if (next) value = work.Current; }
                catch (Exception e) { next = false; failure = e; }
                if (failure != null)
                {
                    (work as IDisposable)?.Dispose();
                    IsTransitioning = false;
                    Fail(failure.Message, () => Begin(request), () => Begin(new Request { Target = AppScene.Title }));
                    yield break;
                }
                if (!next) { (work as IDisposable)?.Dispose(); yield break; }
                yield return value;
            }
            (work as IDisposable)?.Dispose();
        }
        private IEnumerator Transition(Request request, int token)
        {
            Loading.Show(LoadingScreenKind.Loading, new LocalizedMessage("Bootstrap", "preparing_data")); yield return null;
            // Unity cannot cancel an already scheduled scene load. A replacement owner waits
            // for it instead of allowing the older load to overwrite a newer destination.
            while (pendingSceneLoad != null && !pendingSceneLoad.isDone) yield return null;
            pendingSceneLoad = null;
            var preparation = Configuration.PrepareAsync();
            while (!preparation.IsCompleted) yield return null;
            var data = preparation.GetAwaiter().GetResult();
            var localization = Localization.InitializeAsync();
            while (!localization.IsCompleted) yield return null;
            localization.GetAwaiter().GetResult();
            if (token != generation) yield break;
            Loading.Show(LoadingScreenKind.Loading, new LocalizedMessage("Bootstrap", "preparing_screen")); yield return null;
            if (!request.UseCurrent)
            {
                var path = request.Target == AppScene.Title ? Configuration.titleScene : Configuration.laboratoryScene;
                if (string.IsNullOrWhiteSpace(path) || !Application.CanStreamedLevelBeLoaded(path))
                    throw new InvalidOperationException("Scene is unavailable: " + path);
                Laboratory?.ReleaseSession(); Laboratory = null;
                pendingScenePath = path;
                var operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
                pendingSceneLoad = operation;
                if (operation == null) throw new InvalidOperationException("Scene loading did not start.");
                while (!operation.isDone) yield return null;
                if (pendingSceneLoad == operation) pendingSceneLoad = null;
            }
            request.UseCurrent = false;
            if (token != generation) yield break;
            while (paused || !focused) yield return null;
            if (request.Target == AppScene.Laboratory)
            {
                Laboratory = FindFirstObjectByType<LaboratoryView>();
                if (Laboratory == null) throw new InvalidOperationException("Laboratory view is missing.");
                Laboratory.ConfigureUiAssets(Configuration.uiAssets);
                Laboratory.ConfigureLocalization(Localization);
                var start = request.Committed ? SessionStart.Continue : request.Start;
                if (!Laboratory.InitializeSession(data, SavePath, 5, start))
                    throw new InvalidOperationException(Laboratory.InitializationError);
                request.Committed = true;
                Laboratory.SetTransitionPaused(true);
            }
            else
            {
                var title = FindFirstObjectByType<TitleView>();
                if (title == null) throw new InvalidOperationException("Title view is missing.");
                title.Initialize(this);
            }
            yield return null; // render the prepared destination under the shield
            while (paused || !focused) yield return null;
            if (token != generation) yield break;
            IsTransitioning = false; Loading.Hide(); Laboratory?.SetTransitionPaused(false);
            foreach (var entry in FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) entry.MarkReady();
        }
        private void Fail(string error, Action retry, Action recover)
        {
            Error = error; AppUiLifetime.Invalidate();
            Debug.LogWarning("Scene preparation failed: " + error);
            Loading.Show(LoadingScreenKind.Error, new LocalizedMessage("Bootstrap", "error_body"), retry, recover);
        }
        private void Update()
        {
            if (!paused && focused && AppUiLifetime.AcceptInput && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                cancelPrompt?.Invoke();
        }
        private void OnApplicationPause(bool value) { if (paused == value) return; paused = value; AppUiLifetime.Invalidate(); }
        private void OnApplicationFocus(bool value) { if (focused == value) return; focused = value; AppUiLifetime.Invalidate(); }
        private void OnDestroy()
        {
            if (Instance != this) return;
            if (pendingSceneLoad != null) abandonedSceneEntry = true;
            generation++; StopAllCoroutines();
            // Scene objects can be destroyed before their persistent owner during Play exit/application shutdown.
            if (Laboratory != null) Laboratory.ReleaseSession();
            Localization?.Dispose(); Localization = null;
            Instance = null; AppUiLifetime.Invalidate();
        }
    }
}
