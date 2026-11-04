using System;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace MobileGame.Foundation.Editor
{
    public static class AppSceneAssets
    {
        public const string ConfigurationPath = "Assets/SO/Application/AppConfiguration.asset";
        public const string BootstrapPath = "Assets/Scenes/Gameplay/Bootstrap/Temp_Bootstrap.unity";
        public const string TitlePath = "Assets/Scenes/Gameplay/Title/Temp_Title.unity";
        public const string LoadingPath = "Assets/Prefabs/UI/Loading/Temp_LoadingScreen.prefab";
        public static string[] ProductScenes => new[] { BootstrapPath, TitlePath, LaboratorySceneAssets.ScenePath };
        public static AppConfiguration EnsureConfiguration()
        {
            var config = AssetDatabase.LoadAssetAtPath<AppConfiguration>(ConfigurationPath);
            if (config != null) return config;
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(ConfigurationPath).Replace('\\', '/'));
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(LoadingPath).Replace('\\', '/'));
            var prefab = AssetDatabase.LoadAssetAtPath<LoadingScreen>(LoadingPath);
            if (prefab == null)
            {
                var owner = new GameObject("Temp_LoadingScreen");
                try { owner.AddComponent<LoadingScreen>(); prefab = PrefabUtility.SaveAsPrefabAsset(owner, LoadingPath).GetComponent<LoadingScreen>(); }
                finally { UnityEngine.Object.DestroyImmediate(owner); }
            }
            config = ScriptableObject.CreateInstance<AppConfiguration>();
            config.resources = GameDataAssets.Registry<ResourceDefinitionRegistry>("Resources");
            config.creatures = GameDataAssets.Registry<CreatureDefinitionRegistry>("Creatures");
            config.facilities = GameDataAssets.Registry<FacilityDefinitionRegistry>("Facilities");
            config.jsonFiles = GameDataAssets.Files();
            config.uiAssets = LaboratoryUiAssetBuilder.CreateMissing(); config.loadingScreen = prefab;
            config.bootstrapScene = BootstrapPath; config.titleScene = TitlePath; config.laboratoryScene = LaboratorySceneAssets.ScenePath;
            AssetDatabase.CreateAsset(config, ConfigurationPath); AssetDatabase.SaveAssets(); return config;
        }
        [MenuItem("Tools/Mobile Game/Create App Scenes")]
        public static void CreateAndMigrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Create scenes outside Play Mode.");
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before migrating.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var config = EnsureConfiguration();
                Create(BootstrapPath, AppScene.Bootstrap, config);
                Create(TitlePath, AppScene.Title, config);
                foreach (var path in new[] { LaboratorySceneAssets.ScenePath, MapFoundationSceneAssets.ScenePath, SessionFoundationSceneAssets.ScenePath })
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (var root in scene.GetRootGameObjects())
                        if (root.GetComponent<GameBootstrap>() != null) GameDataAssets.Configure(root);
                    if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed: " + path);
                }
                EditorBuildSettings.scenes = ProductScenes.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("PLAN016 application scenes and shared configuration saved. No build invoked.");
        }
        private static void Create(string path, AppScene role, AppConfiguration config)
        {
            if (File.Exists(path)) return;
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Temp_" + role + "Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = UiWidgets.Background;
            camera.orthographic = true; camera.tag = "MainCamera";
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
            var root = new GameObject("Temp_" + role);
            root.AddComponent<GameBootstrap>().Configure(config, role);
            if (role == AppScene.Title) BindTitleArtwork(root.AddComponent<TitleView>());
            if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Scene save failed: " + path);
        }
        internal static void BindTitleArtwork(TitleView view)
        {
            const string sheets = "Assets/Sprites/UI/Title/Temp_NightShift";
            var sprites = new[] { "Landscape", "Portrait" }
                .SelectMany(sheet => AssetDatabase.LoadAllAssetsAtPath(sheets + sheet + ".png"))
                .OfType<Sprite>().ToDictionary(sprite => sprite.name);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("landscapeRoom").objectReferenceValue = sprites["Temp_NightShiftRoomWide"];
            serialized.FindProperty("portraitRoom").objectReferenceValue = sprites["Temp_NightShiftRoomTall"];
            serialized.FindProperty("buttonFrame").objectReferenceValue = sprites["Temp_NightShiftButtonFrame"];
            serialized.FindProperty("wordmark").objectReferenceValue = sprites["Temp_NightShiftWordmark"];
            // Artwork metadata. A translated first line with different lettering is drawn as text.
            serialized.FindProperty("wordmarkLine").stringValue = "DOOM INC.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        [MenuItem("Tools/Mobile Game/Open Bootstrap Scene")]
        public static void OpenBootstrap() => Open(BootstrapPath);
        [MenuItem("Tools/Mobile Game/Open Title Scene")]
        public static void OpenTitle() => Open(TitlePath);
        private static void Open(string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Open outside Play Mode.");
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
        }
    }
}
