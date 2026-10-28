using System;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class InvasionDefinitionAssets
    {
        public const string Path = "Assets/SO/Session/Invasion.asset";
        [MenuItem("Tools/Mobile Game/Upgrade Invasion Laboratory")]
        public static void Upgrade()
        {
            LaboratorySceneAssets.Open();
            var definition = AssetDatabase.LoadAssetAtPath<InvasionDefinition>(Path);
            if (definition == null)
            {
                if (System.IO.File.Exists(Path)) throw new InvalidOperationException("Incompatible invasion asset already exists.");
                FoundationDefinitionAssets.EnsureFolder("Assets/SO/Session");
                definition = ScriptableObject.CreateInstance<InvasionDefinition>();
                definition.name = "Invasion";
                AssetDatabase.CreateAsset(definition, Path);
            }
            definition.CreateSettings();
            var invader = AssetDatabase.LoadAssetAtPath<CreatureDefinition>(ProductionDefinitionAssets.InvaderPath);
            // Remove the old presentation-only waiting suffix. The shared model name
            // must also describe released creatures; never overwrite a custom label.
            if (invader != null && invader.DisplayName == "침공형 (방출 대기)")
            {
                using (var serialized = new SerializedObject(invader))
                {
                    serialized.FindProperty("displayName").stringValue = "침공형";
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorUtility.SetDirty(invader);
            }
            var view = UnityEngine.Object.FindFirstObjectByType<LaboratoryView>();
            if (view == null) throw new InvalidOperationException("Missing laboratory view.");
            using (var serialized = new SerializedObject(view))
            {
                serialized.FindProperty("invasion").objectReferenceValue = definition;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            if (!EditorSceneManager.SaveScene(view.gameObject.scene)) throw new InvalidOperationException("Could not save laboratory.");
            AssetDatabase.SaveAssets();
        }
    }
}
