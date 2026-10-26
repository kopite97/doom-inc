using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class FoundationDefinitionAssets
    {
        private const string DefinitionFolder = "Assets/SO";

        [MenuItem("Tools/Mobile Game/Create Foundation Definitions")]
        public static void CreateMissing()
        {
            // Check every existing asset before creating any of the missing definitions.
            var organic = CheckExisting<ResourceDefinition>("OrganicMatter", "resource.organic", "유기물",
                ThingComponentKind.ResourceStack);
            var storage = CheckExisting<FacilityDefinition>("Storage", "facility.storage", "범용 저장소",
                ThingComponentKind.ResourceContainer);
            var prototype = CheckExisting<CreatureDefinition>("PrototypeSupport", "creature.support.prototype",
                "보조형 프로토타입", ThingComponentKind.ResourceContainer);

            EnsureFolder(DefinitionFolder);
            if (organic == null)
                Create<ResourceDefinition>("OrganicMatter", "resource.organic", "유기물", ThingComponentKind.ResourceStack);
            if (storage == null)
                Create<FacilityDefinition>("Storage", "facility.storage", "범용 저장소", ThingComponentKind.ResourceContainer);
            if (prototype == null)
                Create<CreatureDefinition>("PrototypeSupport", "creature.support.prototype", "보조형 프로토타입",
                    ThingComponentKind.ResourceContainer);

            AssetDatabase.SaveAssets();
        }

        private static T CheckExisting<T>(string fileName, string id, string displayName,
            ThingComponentKind component) where T : ThingDefinition
        {
            var path = AssetPath<T>(fileName);
            var existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing == null)
            {
                if (PathExists(path))
                    throw new InvalidOperationException($"Cannot create '{path}': an existing file or folder could not be loaded.");
                return null;
            }

            if (!(existing is T definition))
                throw new InvalidOperationException($"Asset '{path}' must be a {typeof(T).Name}; it was not overwritten.");

            definition.Validate();
            if (definition.DefinitionId != id || definition.DisplayName != displayName
                || definition.ComponentKinds.Count != 1 || definition.ComponentKinds[0] != component)
                throw new InvalidOperationException($"Asset '{path}' differs from the expected foundation definition; it was not overwritten.");
            return definition;
        }

        private static void Create<T>(string fileName, string id, string displayName,
            ThingComponentKind component) where T : ThingDefinition
        {
            var path = AssetPath<T>(fileName);
            if (PathExists(path))
                throw new InvalidOperationException($"Cannot create '{path}': the path already exists.");
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));

            var definition = ScriptableObject.CreateInstance<T>();
            try
            {
                using (var serialized = new SerializedObject(definition))
                {
                    serialized.FindProperty("definitionId").stringValue = id;
                    serialized.FindProperty("displayName").stringValue = displayName;
                    var components = serialized.FindProperty("components");
                    components.arraySize = 1;
                    components.GetArrayElementAtIndex(0).intValue = (int)component;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                definition.Validate();
                AssetDatabase.CreateAsset(definition, path);
                if (!EditorUtility.IsPersistent(definition))
                    throw new InvalidOperationException($"Unity did not save the new definition at '{path}'.");
            }
            finally
            {
                if (!EditorUtility.IsPersistent(definition))
                    UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        private static string AssetPath<T>(string fileName) where T : ThingDefinition
        {
            var topic = typeof(T) == typeof(ResourceDefinition) ? "Resource"
                : typeof(T) == typeof(FacilityDefinition) ? "Facility"
                : typeof(T) == typeof(CreatureDefinition) ? "Creature"
                : throw new InvalidOperationException($"Unsupported definition type: {typeof(T).Name}");
            return $"{DefinitionFolder}/{topic}/{fileName}.asset";
        }

        private static bool PathExists(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var fullPath = Path.Combine(projectRoot, assetPath);
            return File.Exists(fullPath) || Directory.Exists(fullPath)
                || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath, AssetPathToGUIDOptions.OnlyExistingAssets));
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            if (PathExists(path))
                throw new InvalidOperationException($"Cannot create asset folder '{path}': the path already exists but is not an imported folder.");

            var separator = path.LastIndexOf('/');
            var parent = path.Substring(0, separator);
            EnsureFolder(parent);
            var guid = AssetDatabase.CreateFolder(parent, path.Substring(separator + 1));
            if (string.IsNullOrEmpty(guid) || AssetDatabase.GUIDToAssetPath(guid) != path)
                throw new InvalidOperationException($"Unity did not create the asset folder '{path}'.");
        }
    }
}
