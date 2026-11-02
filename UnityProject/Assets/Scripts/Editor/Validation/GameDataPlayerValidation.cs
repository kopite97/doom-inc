using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class GameDataPlayerValidation
    {
        [Serializable] private sealed class Evidence
        {
            public string target;
            public string dataHash;
            public string[] assemblies;
            public string[] jsonDependencies;
            public bool succeeded;
        }
        [MenuItem("Tools/Mobile Game/Verification/Enable Domain Reload")]
        public static void EnableDomainReload()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Change settings outside Play Mode.");
            if (!UnityEditor.SessionState.GetBool("PLAN012.HasOriginalOptions", false))
            {
                UnityEditor.SessionState.SetBool("PLAN012.HasOriginalOptions", true);
                UnityEditor.SessionState.SetBool("PLAN012.OriginalOptionsEnabled", EditorSettings.enterPlayModeOptionsEnabled);
                UnityEditor.SessionState.SetInt("PLAN012.OriginalOptions", (int)EditorSettings.enterPlayModeOptions);
            }
            EditorSettings.enterPlayModeOptionsEnabled = false;
            Debug.Log("PLAN012 verification: domain and scene reload enabled.");
        }
        [MenuItem("Tools/Mobile Game/Verification/Read Play Options")]
        public static void ReadPlayOptions()
        {
            Debug.Log("PLAN012 play options: enabled=" + EditorSettings.enterPlayModeOptionsEnabled
                + "; flags=" + EditorSettings.enterPlayModeOptions
                + "; savedOriginalEnabled=" + UnityEditor.SessionState.GetBool("PLAN012.OriginalOptionsEnabled", false));
        }
        [MenuItem("Tools/Mobile Game/Verification/Disable Domain Reload")]
        public static void DisableDomainReload()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Change settings outside Play Mode.");
            EnableDomainReload();
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            ReadPlayOptions();
        }
        [MenuItem("Tools/Mobile Game/Verification/Restore Play Options")]
        public static void RestorePlayOptions()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Restore settings outside Play Mode.");
            if (!UnityEditor.SessionState.GetBool("PLAN012.HasOriginalOptions", false)) return;
            EditorSettings.enterPlayModeOptionsEnabled = UnityEditor.SessionState.GetBool("PLAN012.OriginalOptionsEnabled", false);
            EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)UnityEditor.SessionState.GetInt("PLAN012.OriginalOptions", 0);
            UnityEditor.SessionState.EraseBool("PLAN012.HasOriginalOptions");
            Debug.Log("PLAN012 verification: original Play options restored.");
        }
        [MenuItem("Tools/Mobile Game/Validate JSON Scene References")]
        public static void ValidateSceneReferences()
        {
            GameDataAssets.LoadCatalog();
            var files = GameDataAssets.Tables().Select(AssetDatabase.GetAssetPath).ToArray();
            var scenes = AppSceneAssets.ProductScenes.Concat(new[] { MapFoundationSceneAssets.ScenePath, SessionFoundationSceneAssets.ScenePath });
            foreach (var scene in scenes)
            {
                var dependencies = AssetDatabase.GetDependencies(scene, true);
                if (files.Any(p => !dependencies.Contains(p))) throw new InvalidOperationException("Missing JSON scene dependency: " + scene);
                if (dependencies.Any(p => p.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Stale CSV scene dependency: " + scene);
            }
            Debug.Log("JSON dependencies verified for five saved scenes; no Player compilation/build invoked.");
        }
        [MenuItem("Tools/Mobile Game/Validate Player Data Compilation")]
        public static void Compile()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Compile outside Play Mode.");
            ValidateSceneReferences();
            var catalog = GameDataAssets.LoadCatalog();
            var json = GameDataAssets.Tables().Select(AssetDatabase.GetAssetPath).ToArray();
            var target = EditorUserBuildSettings.activeBuildTarget;
            var attempt = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            var result = PlayerBuildInterface.CompilePlayerScripts(new ScriptCompilationSettings
            { target = target, group = BuildPipeline.GetBuildTargetGroup(target), options = ScriptCompilationOptions.None }, "Library/PlayerDataValidation/" + attempt);
            var evidence = new Evidence { target = target.ToString(), dataHash = catalog.Settings.CompatibilityHash,
                assemblies = result.assemblies?.ToArray(), jsonDependencies = json, succeeded = result.assemblies != null && result.assemblies.Count > 0 };
            var directory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "DataValidation");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "player-compilation-" + attempt + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(evidence, true));
            if (!evidence.succeeded) throw new InvalidOperationException("Player scripts did not compile.");
            Debug.Log("Player script compilation and JSON scene dependencies verified: " + target);
        }
    }
}
