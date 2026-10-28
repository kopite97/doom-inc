using System;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobileGame.Foundation.Editor
{
    public static class GameDataAssets
    {
        public const string RegistryFolder = "Assets/SO/Registry";
        public static readonly string[] DefinitionPaths = {
            "Resource/OrganicMatter", "Resource/CultureMedium", "Creature/PrototypeSupport", "Creature/Support", "Creature/Invader",
            "Facility/Gathering", "Facility/Storage", "Facility/Processing", "Facility/Cultivation" };
        public static TextAsset[] Tables() => GameDataComposer.Schemas.Keys.Select(n =>
            AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/CSV/Balance/" + n + ".csv")
                ?? throw new InvalidOperationException("Missing CSV: " + n)).ToArray();
        public static T Registry<T>(string topic) where T : ScriptableObject => AssetDatabase.LoadAssetAtPath<T>(RegistryFolder + "/" + topic + ".asset")
            ?? throw new InvalidOperationException("Missing registry: " + topic);
        public static GameDataCatalog LoadCatalog() => GameDataComposer.Compose(new LocalCsvDataSource(Tables()).LoadAsync(default).GetAwaiter().GetResult(),
            Registry<ResourceDefinitionRegistry>("Resources"), Registry<CreatureDefinitionRegistry>("Creatures"), Registry<FacilityDefinitionRegistry>("Facilities"));

        [MenuItem("Tools/Mobile Game/Validate CSV and Registries")]
        public static void Validate() { var data = LoadCatalog(); Debug.Log("Game data ready: " + data.Definitions.Count + "; hash=" + data.Settings.CompatibilityHash); }

        [MenuItem("Tools/Mobile Game/Migrate CSV Data and Registries")]
        public static void Migrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Migrate outside Play Mode.");
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save the current scene first.");
            EnsureAssets();
            var paths = new[] { LaboratorySceneAssets.ScenePath, MapFoundationSceneAssets.ScenePath, SessionFoundationSceneAssets.ScenePath };
            foreach (var path in paths)
            {
                if (!File.Exists(path)) throw new InvalidOperationException("Missing scene: " + path);
                var scene = EditorSceneManager.OpenScene(path);
                foreach (var root in scene.GetRootGameObjects())
                    if (root.GetComponent<LaboratoryView>() != null || root.GetComponent<SessionFoundationView>() != null || root.GetComponent<MapFoundationView>() != null)
                        Configure(root);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + path);
            }
            AssetDatabase.ForceReserializeAssets(DefinitionPaths.Select(p => "Assets/SO/" + p + ".asset").ToList());
            // The former numeric-only invasion SO has no consumers after all three scenes are migrated.
            if (AssetDatabase.LoadMainAssetAtPath("Assets/SO/Session/Invasion.asset") != null)
                AssetDatabase.DeleteAsset("Assets/SO/Session/Invasion.asset");
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(LaboratorySceneAssets.ScenePath);
            Validate();
        }

        public static void EnsureAssets()
        {
            FoundationDefinitionAssets.EnsureFolder(RegistryFolder);
            var all = DefinitionPaths.Select(p => AssetDatabase.LoadAssetAtPath<ThingDefinition>("Assets/SO/" + p + ".asset")
                ?? throw new InvalidOperationException("Missing definition asset: " + p)).ToArray();
            EnsureRegistry<ResourceDefinitionRegistry>("Resources", all.OfType<ResourceDefinition>().Cast<UnityEngine.Object>().ToArray());
            EnsureRegistry<CreatureDefinitionRegistry>("Creatures", all.OfType<CreatureDefinition>().Cast<UnityEngine.Object>().ToArray());
            EnsureRegistry<FacilityDefinitionRegistry>("Facilities", all.OfType<FacilityDefinition>().Cast<UnityEngine.Object>().ToArray());
            AssetDatabase.SaveAssets(); LoadCatalog();
        }
        private static void EnsureRegistry<T>(string name, UnityEngine.Object[] entries) where T : ScriptableObject
        {
            var path = RegistryFolder + "/" + name + ".asset";
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return;
            if (File.Exists(path)) throw new InvalidOperationException("Incompatible existing registry: " + path);
            var registry = ScriptableObject.CreateInstance<T>();
            using (var serialized = new SerializedObject(registry))
            {
                var list = serialized.FindProperty("entries"); list.arraySize = entries.Length;
                for (var i = 0; i < entries.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = entries[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.CreateAsset(registry, path);
        }
        public static void Configure(GameObject target)
        {
            var bootstrap = target.GetComponent<GameBootstrap>() ?? target.AddComponent<GameBootstrap>();
            using (var serialized = new SerializedObject(bootstrap))
            {
                serialized.FindProperty("resources").objectReferenceValue = Registry<ResourceDefinitionRegistry>("Resources");
                serialized.FindProperty("creatures").objectReferenceValue = Registry<CreatureDefinitionRegistry>("Creatures");
                serialized.FindProperty("facilities").objectReferenceValue = Registry<FacilityDefinitionRegistry>("Facilities");
                var tables = Tables(); var list = serialized.FindProperty("csvTables"); list.arraySize = tables.Length;
                for (var i = 0; i < tables.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = tables[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(bootstrap);
        }
    }
}
