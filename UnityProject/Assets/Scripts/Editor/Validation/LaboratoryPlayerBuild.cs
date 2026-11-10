using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobileGame.Foundation.Editor
{
    // The verification APK always starts in the actual game, independently of an open diagnostic scene.
    public static class LaboratoryPlayerBuild
    {
        [Serializable] private sealed class SceneEntry
        {
            public string path;
            public bool enabled;
        }

        [Serializable] private sealed class BuildEvidence
        {
            public string plan;
            public string buildKind;
            public string attempt;
            public string startedUtc;
            public string finishedUtc;
            public string unityVersion;
            public string host;
            public string target;
            public string applicationId;
            public string backend;
            public string architectures;
            public string minimumSdk;
            public string targetSdk;
            public string orientation;
            public bool portrait;
            public bool portraitUpsideDown;
            public bool landscapeLeft;
            public bool landscapeRight;
            public int activeInputHandler;
            public string dataHash;
            public string[] scenes;
            public SceneEntry[] editorScenes;
            public string buildOptions;
            public string managedStrippingLevel;
            public bool development;
            public bool scriptDebugging;
            public bool connectProfiler;
            public bool deepProfiling;
            public string addressablesProfile;
            public string addressablesOutput;
            public string addressablesError;
            public double addressablesSeconds;
            public string apk;
            public string sha256;
            public string buildGuid;
            public string result;
            public int errors;
            public int warnings;
            public double seconds;
            public long bytes;
            public string[] messages;
            public string failure;
        }

        [MenuItem("Tools/Mobile Game/Configure Laboratory Player Entry")]
        public static void ConfigurePlayerEntry()
        {
            RequireIdleEditor();
            ValidateEntryScene();
            EditorBuildSettings.scenes = AppSceneAssets.ProductScenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Player entry configured: Bootstrap, Title, Laboratory; diagnostics excluded.");
        }

        [MenuItem("Tools/Mobile Game/Build PLAN-013 Android APK")]
        public static void BuildAndroidApk() => BuildAndroidForPlan("PLAN-013");

        [MenuItem("Tools/Mobile Game/Build PLAN-014 Android APK")]
        public static void BuildAndroidPlan014() => BuildAndroidForPlan("PLAN-014");

        [MenuItem("Tools/Mobile Game/Build PLAN-025 Final Android APK")]
        public static void BuildAndroidPlan025Final() => BuildAndroidForPlan("PLAN-025", false);

        [MenuItem("Tools/Mobile Game/Build PLAN-025 Diagnostic Android APK")]
        public static void BuildAndroidPlan025Diagnostic() => BuildAndroidForPlan("PLAN-025", true);

        private static void BuildAndroidForPlan(string planId, bool development = true)
        {
            RequireIdleEditor();
            var attempt = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            var buildKind = development ? "diagnostic" : "final";
            var project = Directory.GetParent(Application.dataPath).FullName;
            var evidenceDirectory = Path.Combine(Directory.GetParent(project).FullName, "docs/plans/evidence", planId);
            var outputDirectory = Path.Combine(project, "Builds", planId);
            if (planId == "PLAN-025")
            {
                evidenceDirectory = Path.Combine(evidenceDirectory, buildKind, attempt);
                outputDirectory = Path.Combine(outputDirectory, buildKind, attempt);
            }
            Directory.CreateDirectory(evidenceDirectory);
            Directory.CreateDirectory(outputDirectory);
            var output = Path.Combine(outputDirectory, "Temp_Laboratory-" + attempt + ".apk");
            var options = BuildOptions.DetailedBuildReport;
            if (development) options |= BuildOptions.Development;
            var evidence = new BuildEvidence
            {
                plan = planId, buildKind = buildKind, attempt = attempt, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion,
                host = SystemInfo.operatingSystem, target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                applicationId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),
                backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android).ToString(),
                architectures = PlayerSettings.Android.targetArchitectures.ToString(),
                minimumSdk = PlayerSettings.Android.minSdkVersion.ToString(), targetSdk = PlayerSettings.Android.targetSdkVersion.ToString(),
                orientation = PlayerSettings.defaultInterfaceOrientation.ToString(),
                portrait = PlayerSettings.allowedAutorotateToPortrait, portraitUpsideDown = PlayerSettings.allowedAutorotateToPortraitUpsideDown,
                landscapeLeft = PlayerSettings.allowedAutorotateToLandscapeLeft, landscapeRight = PlayerSettings.allowedAutorotateToLandscapeRight,
                scenes = AppSceneAssets.ProductScenes,
                editorScenes = EditorBuildSettings.scenes.Select(s => new SceneEntry { path = s.path, enabled = s.enabled }).ToArray(),
                buildOptions = options.ToString(),
                managedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.Android).ToString(),
                development = development, scriptDebugging = false, connectProfiler = false, deepProfiling = false,
                apk = "UnityProject/" + output.Substring(project.Length + 1).Replace('\\', '/'), result = "NotStarted"
            };
            var originalBundle = EditorUserBuildSettings.buildAppBundle;
            var originalExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            var originalDevelopment = EditorUserBuildSettings.development;
            var originalDebugging = EditorUserBuildSettings.allowDebugging;
            var originalConnectProfiler = EditorUserBuildSettings.connectProfiler;
            var originalDeepProfiling = EditorUserBuildSettings.buildWithDeepProfilingSupport;
            var addressables = planId == "PLAN-025" ? AddressableAssetSettingsDefaultObject.Settings : null;
            var originalAddressablesBuild = addressables != null ? addressables.BuildAddressablesWithPlayerBuild : default;
            try
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new InvalidOperationException("Switch to the approved Android target before building.");
                if (planId == "PLAN-025" && (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP
                    || PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64))
                    throw new InvalidOperationException("PLAN-025 requires IL2CPP and ARM64 only.");
                if (PlayerSettings.Android.useCustomKeystore)
                    throw new InvalidOperationException("Verification builds use the default debug signing configuration; custom signing is not changed by this tool.");
                if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation
                    || !evidence.portrait || !(evidence.landscapeLeft || evidence.landscapeRight))
                    throw new InvalidOperationException("Enable portrait and landscape autorotation before mobile verification.");
                var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
                if (playerSettings == null) throw new InvalidOperationException("Cannot load Player Settings to inspect the input backend.");
                using (var settings = new SerializedObject(playerSettings))
                {
                    var input = settings.FindProperty("activeInputHandler");
                    if (input == null) throw new InvalidOperationException("Cannot inspect the configured player input backend.");
                    evidence.activeInputHandler = input.intValue;
                    if (input.intValue != 1) throw new InvalidOperationException("Use the project's Input System-only backend for Android verification.");
                }
                ValidateEntryScene();
                evidence.dataHash = GameDataAssets.LoadCatalog().Settings.CompatibilityHash;
                var dependencies = AssetDatabase.GetDependencies(LaboratorySceneAssets.ScenePath, true);
                if (GameDataAssets.Tables().Select(AssetDatabase.GetAssetPath).Any(path => !dependencies.Contains(path)))
                    throw new InvalidOperationException("The player entry scene must include every local JSON table.");
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                EditorUserBuildSettings.development = development;
                EditorUserBuildSettings.allowDebugging = false;
                EditorUserBuildSettings.connectProfiler = false;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
                if (planId == "PLAN-025")
                {
                    if (addressables == null) throw new InvalidOperationException("Missing local Addressables settings.");
                    evidence.addressablesProfile = addressables.profileSettings.GetProfileName(addressables.activeProfileId);
                    // Explicitly build approved content once, independent of this machine's Editor preference.
                    AddressableAssetSettings.BuildPlayerContent(out var content);
                    evidence.addressablesOutput = content?.OutputPath;
                    evidence.addressablesError = content?.Error;
                    evidence.addressablesSeconds = content?.Duration ?? 0;
                    if (content == null || !string.IsNullOrEmpty(content.Error))
                        throw new InvalidOperationException("Local Addressables build failed: " + (content?.Error ?? "no result"));
                    addressables.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
                }
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = evidence.scenes, locationPathName = output, target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android, options = options
                });
                if (report == null) throw new InvalidOperationException("BuildPipeline returned no report.");
                evidence.result = report.summary.result.ToString();
                evidence.buildGuid = report.summary.guid.ToString();
                evidence.errors = (int)report.summary.totalErrors; evidence.warnings = (int)report.summary.totalWarnings;
                evidence.seconds = report.summary.totalTime.TotalSeconds; evidence.bytes = (long)report.summary.totalSize;
                evidence.messages = report.steps.SelectMany(step => step.messages)
                    .Where(m => m.type == LogType.Error || m.type == LogType.Exception || m.type == LogType.Warning)
                    .Select(m => m.type + ": " + m.content).ToArray();
                if (report.summary.result != BuildResult.Succeeded || !File.Exists(output))
                    throw new InvalidOperationException("Android verification build did not produce a successful APK: " + evidence.result);
                using (var stream = File.OpenRead(output))
                using (var hash = SHA256.Create()) evidence.sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                Debug.Log(planId + " APK ready: " + output + "; sha256=" + evidence.sha256);
            }
            catch (Exception error)
            {
                evidence.failure = error.Message;
                if (evidence.result == "NotStarted") evidence.result = "PreflightFailed";
                throw;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = originalBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = originalExport;
                EditorUserBuildSettings.development = originalDevelopment;
                EditorUserBuildSettings.allowDebugging = originalDebugging;
                EditorUserBuildSettings.connectProfiler = originalConnectProfiler;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = originalDeepProfiling;
                if (addressables != null) addressables.BuildAddressablesWithPlayerBuild = originalAddressablesBuild;
                evidence.finishedUtc = DateTime.UtcNow.ToString("O");
                File.WriteAllText(Path.Combine(evidenceDirectory, "android-build-" + attempt + ".json"), JsonUtility.ToJson(evidence, true));
            }
        }

        private static void RequireIdleEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Run player configuration/build outside Play Mode, compilation and other builds.");
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save modified scenes before validating the player entry.");
        }

        private static void ValidateEntryScene()
        {
            var configuration = AssetDatabase.LoadAssetAtPath<AppConfiguration>(AppSceneAssets.ConfigurationPath);
            if (configuration == null || configuration.loadingScreen == null || configuration.uiAssets == null
                || configuration.bootstrapScene != AppSceneAssets.BootstrapPath || configuration.titleScene != AppSceneAssets.TitlePath
                || configuration.laboratoryScene != LaboratorySceneAssets.ScenePath)
                throw new InvalidOperationException("The shared application configuration is incomplete.");
            foreach (var path in AppSceneAssets.ProductScenes)
            {
                var candidate = SceneManager.GetSceneByPath(path);
                var added = !candidate.IsValid() || !candidate.isLoaded;
                if (added) candidate = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    var objects = candidate.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
                    if (objects.Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0))
                        throw new InvalidOperationException("Missing script in " + path);
                    var entries = objects.Select(t => t.GetComponent<GameBootstrap>()).Where(b => b != null).ToArray();
                    var role = path == AppSceneAssets.BootstrapPath ? AppScene.Bootstrap
                        : path == AppSceneAssets.TitlePath ? AppScene.Title : AppScene.Laboratory;
                    if (entries.Length != 1 || entries[0].Configuration != configuration || entries[0].SceneRole != role
                        || !entries[0].isActiveAndEnabled)
                        throw new InvalidOperationException("Invalid shared entry in " + path);
                    if (role == AppScene.Title && objects.Count(t => t.GetComponent<TitleView>() != null) != 1)
                        throw new InvalidOperationException("Title needs one menu view.");
                }
                finally { if (added) EditorSceneManager.CloseScene(candidate, true); }
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LaboratorySceneAssets.ScenePath) == null)
                throw new InvalidOperationException("The Laboratory entry scene is missing.");
            var scene = SceneManager.GetSceneByPath(LaboratorySceneAssets.ScenePath);
            var opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(LaboratorySceneAssets.ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var bootstraps = roots.SelectMany(root => root.GetComponentsInChildren<GameBootstrap>(true)).ToArray();
                var views = roots.SelectMany(root => root.GetComponentsInChildren<LaboratoryView>(true)).ToArray();
                if (bootstraps.Length != 1 || views.Length != 1 || bootstraps[0].gameObject != views[0].gameObject)
                    throw new InvalidOperationException("Player entry needs exactly one LaboratoryView and its GameBootstrap on the same object.");
                if (!bootstraps[0].enabled || !views[0].enabled || !views[0].gameObject.activeInHierarchy)
                    throw new InvalidOperationException("The Laboratory entry must be active and enabled.");
                if (roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0))
                    throw new InvalidOperationException("The Laboratory entry contains a missing script.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
