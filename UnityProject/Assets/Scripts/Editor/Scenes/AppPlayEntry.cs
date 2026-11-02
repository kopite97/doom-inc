using System;
using System.IO;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    [InitializeOnLoad]
    public static class AppPlayEntry
    {
        private const string Key = "MobileGame.AppPlayEntry.";
        static AppPlayEntry()
        {
            Configure();
            EditorApplication.playModeStateChanged += StateChanged;
            // Restore a temporary start scene after a domain reload or editor restart.
            if (!EditorApplication.isPlayingOrWillChangePlaymode) RestoreStartScene();
        }
        private static void Configure()
        {
            GameBootstrap.EditorStartChoice = EditorPrefs.GetBool(Key + "Continue", false) ? SessionStart.Continue : SessionStart.NewGame;
            GameBootstrap.EditorSlotName = EditorPrefs.GetBool(Key + "SlotB", false) ? "SlotB" : "Default";
        }
        [InitializeOnEnterPlayMode] private static void Enter(EnterPlayModeOptions options) => Configure();
        private static void StateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Configure();
            if (state == PlayModeStateChange.EnteredEditMode) RestoreStartScene();
            if (state == PlayModeStateChange.EnteredPlayMode && EditorSettings.enterPlayModeOptionsEnabled
                && (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) != 0)
            {
                foreach (var entry in UnityEngine.Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None))
                    entry.RestartEditorEntry();
            }
        }
        [MenuItem("Tools/Mobile Game/Play/Current Scene - New Test Run")]
        public static void CurrentNew() { SetMode(false); SetTemporaryStartScene(null); EditorApplication.isPlaying = true; }
        [MenuItem("Tools/Mobile Game/Play/Current Scene - Continue Test Save")]
        public static void CurrentContinue() { SetMode(true); SetTemporaryStartScene(null); EditorApplication.isPlaying = true; }
        private static void SetMode(bool continuation)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Choose a start mode outside Play Mode.");
            RestoreStartScene(); EditorPrefs.SetBool(Key + "Continue", continuation); Configure();
        }
        [MenuItem("Tools/Mobile Game/Play/Full Bootstrap Flow")]
        public static void FullFlow()
        {
            SetMode(false);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AppSceneAssets.BootstrapPath);
            if (scene == null) throw new InvalidOperationException("Create app scenes first.");
            SetTemporaryStartScene(scene); EditorApplication.isPlaying = true;
        }
        private static void SetTemporaryStartScene(SceneAsset scene)
        {
            EditorPrefs.SetString(Key + "OriginalStart", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorPrefs.SetBool(Key + "RestoreStart", true);
            EditorSceneManager.playModeStartScene = scene;
        }
        private static void RestoreStartScene()
        {
            if (!EditorPrefs.GetBool(Key + "RestoreStart", false)) return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorPrefs.GetString(Key + "OriginalStart", ""));
            EditorPrefs.DeleteKey(Key + "RestoreStart"); EditorPrefs.DeleteKey(Key + "OriginalStart");
        }
        [MenuItem("Tools/Mobile Game/Play/Use Default Test Slot")]
        public static void SlotA() => SelectSlot(false);
        [MenuItem("Tools/Mobile Game/Play/Use Alternate Test Slot")]
        public static void SlotB() => SelectSlot(true);
        private static void SelectSlot(bool alternate)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Change slots outside Play Mode.");
            EditorPrefs.SetBool(Key + "SlotB", alternate); Configure();
            Debug.Log("Editor test slot: " + SceneFlowManager.DefaultSavePath);
        }
    }
}
