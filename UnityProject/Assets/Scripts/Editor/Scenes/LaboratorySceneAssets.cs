using System;
using System.IO;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace MobileGame.Foundation.Editor
{
    public static class LaboratorySceneAssets
    {
        public const string ScenePath = "Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity";

        [MenuItem("Tools/Mobile Game/Upgrade Production Laboratory")]
        public static void UpgradeProduction()
        {
            Open(); // Checks Play Mode and unsaved scenes before touching assets.
            ProductionDefinitionAssets.CreateMissing();
            var view = UnityEngine.Object.FindFirstObjectByType<LaboratoryView>();
            if (view == null) throw new InvalidOperationException("Laboratory view is missing.");
            GameDataAssets.Configure(view.gameObject);
            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            if (!EditorSceneManager.SaveScene(view.gameObject.scene)) throw new InvalidOperationException("Could not save laboratory.");
        }

        [MenuItem("Tools/Mobile Game/Create Laboratory Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create the laboratory scene outside Play Mode.");
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("Temp_Laboratory already exists; it was not overwritten.");
            LaboratoryDefinitionAssets.CreateOrUpgrade();
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(ScenePath).Replace('\\', '/'));
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Temp_LaboratoryCamera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.055f, 0.075f);
                cameraObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
                var viewObject = new GameObject("Temp_LaboratoryView");
                SceneManager.MoveGameObjectToScene(viewObject, scene);
                var view = viewObject.AddComponent<LaboratoryView>();
                GameDataAssets.Configure(view.gameObject);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Could not save Temp_Laboratory.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        [MenuItem("Tools/Mobile Game/Open Laboratory Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open the laboratory scene outside Play Mode.");
            for (var index = 0; index < SceneManager.sceneCount; index++)
                if (SceneManager.GetSceneAt(index).isDirty)
                    throw new InvalidOperationException("Save the current scene before opening the laboratory.");
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}
