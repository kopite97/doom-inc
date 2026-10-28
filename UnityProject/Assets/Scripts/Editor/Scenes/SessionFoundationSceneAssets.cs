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
    public static class SessionFoundationSceneAssets
    {
        public const string ScenePath = "Assets/Scenes/Diagnostics/Session/Temp_SessionFoundation.unity";

        [MenuItem("Tools/Mobile Game/Create Session Foundation Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create the diagnostic scene outside Play Mode.");
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("Temp_SessionFoundation already exists; it was not overwritten.");
            FoundationDefinitionAssets.CreateMissing();
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(ScenePath).Replace('\\', '/'));
            var storage = AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Storage.asset");
            var creature = AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/PrototypeSupport.asset");
            var resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/SO/Resource/OrganicMatter.asset");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Temp_SessionCamera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.055f, 0.075f);
                cameraObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);

                var viewObject = new GameObject("Temp_SessionFoundationView");
                SceneManager.MoveGameObjectToScene(viewObject, scene);
                var view = viewObject.AddComponent<SessionFoundationView>();
                GameDataAssets.Configure(view.gameObject);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Could not save Temp_SessionFoundation.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
            }
        }

        [MenuItem("Tools/Mobile Game/Open Session Foundation Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open the diagnostic scene outside Play Mode.");
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save the current scene before opening the fixture.");
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}
