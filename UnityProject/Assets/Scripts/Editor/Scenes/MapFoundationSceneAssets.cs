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
    public static class MapFoundationSceneAssets
    {
        public const string ScenePath = "Assets/Scenes/Diagnostics/Map/MapFoundation.unity";

        [MenuItem("Tools/Mobile Game/Create Map Foundation Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create the scene outside Play Mode.");
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("MapFoundation already exists; it was not overwritten.");

            FoundationDefinitionAssets.CreateMissing();
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(ScenePath).Replace('\\', '/'));
            var storage = AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Storage.asset");
            var creature = AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/PrototypeSupport.asset");
            var resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/SO/Resource/OrganicMatter.asset");
            // Development tuning for the representative store, not a finalized balance value.
            using (var serialized = new SerializedObject(storage))
            {
                serialized.FindProperty("footprint").vector2IntValue = new Vector2Int(3, 2);
                serialized.FindProperty("interactionOffset").vector2IntValue = new Vector2Int(1, -1);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            storage.Validate();
            AssetDatabase.SaveAssetIfDirty(storage);

            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Main Camera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.055f, 0.075f);
                cameraObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);

                var viewObject = new GameObject("Map Foundation View");
                SceneManager.MoveGameObjectToScene(viewObject, scene);
                var view = viewObject.AddComponent<MapFoundationView>();
                using (var serialized = new SerializedObject(view))
                {
                    serialized.FindProperty("storage").objectReferenceValue = storage;
                    serialized.FindProperty("creature").objectReferenceValue = creature;
                    serialized.FindProperty("resource").objectReferenceValue = resource;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Could not save MapFoundation.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
            }
        }

        [MenuItem("Tools/Mobile Game/Open Map Foundation Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open the fixture outside Play Mode.");
            // Do not discard an unsaved scene as a side effect of an automation command.
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save the current scene before opening the fixture.");
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}
