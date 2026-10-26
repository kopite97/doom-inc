using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class LaboratoryDefinitionAssets
    {
        public const string ResourcePath = "Assets/SO/Resource/OrganicMatter.asset";
        public const string StoragePath = "Assets/SO/Facility/Storage.asset";
        public const string GatheringPath = "Assets/SO/Facility/Gathering.asset";
        public const string PrototypePath = "Assets/SO/Creature/PrototypeSupport.asset";

        [MenuItem("Tools/Mobile Game/Create Laboratory Definitions")]
        public static void CreateOrUpgrade()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create definitions outside Play Mode.");
            FoundationDefinitionAssets.CreateMissing();
            var resource = Load<ResourceDefinition>(ResourcePath);
            var storage = Load<FacilityDefinition>(StoragePath);
            var prototype = Load<CreatureDefinition>(PrototypePath);
            var gathering = AssetDatabase.LoadAssetAtPath<FacilityDefinition>(GatheringPath);
            if (gathering == null && (File.Exists(GatheringPath) || Directory.Exists(GatheringPath)
                || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(GatheringPath))))
                throw new InvalidOperationException("Gathering definition path already exists but is not a facility definition.");
            if (gathering != null && gathering.DefinitionId != "facility.gathering")
                throw new InvalidOperationException("Existing gathering definition has a different identity; it was not overwritten.");

            ConfigureFacility(storage, FacilityRole.Storage, resource);
            using (var serialized = new SerializedObject(prototype))
            {
                serialized.FindProperty("carryCapacity").intValue = 5;
                serialized.FindProperty("moveSecondsPerCell").floatValue = 0.25f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            prototype.Validate();
            EditorUtility.SetDirty(prototype);

            if (gathering == null)
            {
                gathering = ScriptableObject.CreateInstance<FacilityDefinition>();
                try
                {
                    using (var serialized = new SerializedObject(gathering))
                    {
                        serialized.FindProperty("definitionId").stringValue = "facility.gathering";
                        serialized.FindProperty("displayName").stringValue = "기초 자원 획득 시설";
                        var components = serialized.FindProperty("components");
                        components.arraySize = 1;
                        components.GetArrayElementAtIndex(0).intValue = (int)ThingComponentKind.ResourceContainer;
                        serialized.FindProperty("footprint").vector2IntValue = new Vector2Int(2, 2);
                        serialized.FindProperty("interactionOffset").vector2IntValue = new Vector2Int(0, -1);
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    ConfigureFacility(gathering, FacilityRole.Gathering, resource);
                    AssetDatabase.CreateAsset(gathering, GatheringPath);
                    if (!EditorUtility.IsPersistent(gathering))
                        throw new InvalidOperationException("Unity did not create the gathering definition.");
                }
                finally
                {
                    if (!EditorUtility.IsPersistent(gathering)) UnityEngine.Object.DestroyImmediate(gathering);
                }
            }
            else ConfigureFacility(gathering, FacilityRole.Gathering, resource);
            AssetDatabase.SaveAssets();
        }

        // Same assets are assigned to the runtime scene and to the snapshot definition catalog.
        public static ThingDefinition[] LoadDefinitions()
        {
            return new ThingDefinition[]
            {
                Load<ResourceDefinition>(ResourcePath), Load<CreatureDefinition>(PrototypePath),
                Load<FacilityDefinition>(GatheringPath), Load<FacilityDefinition>(StoragePath)
            };
        }

        private static T Load<T>(string path) where T : ThingDefinition
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value == null) throw new InvalidOperationException($"Missing definition: {path}");
            value.Validate();
            return value;
        }

        private static void ConfigureFacility(FacilityDefinition definition, FacilityRole role,
            ResourceDefinition resource)
        {
            using (var serialized = new SerializedObject(definition))
            {
                serialized.FindProperty("role").intValue = (int)role;
                serialized.FindProperty("resource").objectReferenceValue = resource;
                serialized.FindProperty("buildCost").intValue = 2;
                serialized.FindProperty("buildSeconds").floatValue = 3f;
                serialized.FindProperty("storageCapacity").intValue = 20;
                serialized.FindProperty("gatheringAmount").intValue = 5;
                serialized.FindProperty("gatheringSeconds").floatValue = 5f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            definition.Validate();
            EditorUtility.SetDirty(definition);
        }
    }
}
