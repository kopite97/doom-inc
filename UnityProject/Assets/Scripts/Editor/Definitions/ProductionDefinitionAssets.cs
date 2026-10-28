using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class ProductionDefinitionAssets
    {
        public const string CulturePath = "Assets/SO/Resource/CultureMedium.asset";
        public const string ProcessingPath = "Assets/SO/Facility/Processing.asset";
        public const string CultivationPath = "Assets/SO/Facility/Cultivation.asset";
        public const string SupportPath = "Assets/SO/Creature/Support.asset";
        public const string InvaderPath = "Assets/SO/Creature/Invader.asset";

        [MenuItem("Tools/Mobile Game/Create Production Definitions")]
        public static void CreateMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create definitions outside Play Mode.");
            var organic = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(LaboratoryDefinitionAssets.ResourcePath);
            if (organic == null) throw new InvalidOperationException("Create the initial laboratory definitions first.");
            var culture = Create<ResourceDefinition>(CulturePath, "resource.culture-medium", "배양액", s =>
            {
                var components = s.FindProperty("components");
                components.arraySize = 1;
                components.GetArrayElementAtIndex(0).intValue = (int)ThingComponentKind.ResourceStack;
            });
            var support = Creature(SupportPath, "creature.support", "보급형", CreatureRole.Support);
            var invader = Creature(InvaderPath, "creature.invader", "침공형", CreatureRole.Invader);
            Facility(ProcessingPath, "facility.processing", "재료 가공시설", FacilityRole.Processing,
                organic, organic, culture, support, invader, 5, 10);
            Facility(CultivationPath, "facility.cultivation", "공용 배양시설", FacilityRole.Cultivation,
                organic, culture, null, support, invader, 2, 20);
            AssetDatabase.SaveAssets();
        }

        public static ThingDefinition[] LoadDefinitions() => new ThingDefinition[]
        {
            AssetDatabase.LoadAssetAtPath<ResourceDefinition>(CulturePath),
            AssetDatabase.LoadAssetAtPath<FacilityDefinition>(ProcessingPath),
            AssetDatabase.LoadAssetAtPath<FacilityDefinition>(CultivationPath),
            AssetDatabase.LoadAssetAtPath<CreatureDefinition>(SupportPath),
            AssetDatabase.LoadAssetAtPath<CreatureDefinition>(InvaderPath)
        };

        private static CreatureDefinition Creature(string path, string id, string name, CreatureRole role) =>
            Create<CreatureDefinition>(path, id, name, s =>
            {
                s.FindProperty("role").intValue = (int)role;
                s.FindProperty("carryCapacity").intValue = 5;
                s.FindProperty("moveSecondsPerCell").floatValue = 0.25f;
                var components = s.FindProperty("components");
                components.arraySize = 1;
                components.GetArrayElementAtIndex(0).intValue = (int)ThingComponentKind.ResourceContainer;
            });

        private static void Facility(string path, string id, string name, FacilityRole role, ResourceDefinition organic,
            ResourceDefinition input, ResourceDefinition output, CreatureDefinition support, CreatureDefinition invader,
            int inputAmount, float seconds) => Create<FacilityDefinition>(path, id, name, s =>
            {
                s.FindProperty("role").intValue = (int)role;
                s.FindProperty("resource").objectReferenceValue = organic;
                s.FindProperty("inputResource").objectReferenceValue = input;
                s.FindProperty("outputResource").objectReferenceValue = output;
                s.FindProperty("supportOutput").objectReferenceValue = support;
                s.FindProperty("invaderOutput").objectReferenceValue = invader;
                s.FindProperty("inputAmount").intValue = inputAmount;
                s.FindProperty("outputAmount").intValue = 1;
                s.FindProperty("productionSeconds").floatValue = seconds;
                s.FindProperty("buildCost").intValue = 2;
                s.FindProperty("buildSeconds").floatValue = 3;
                s.FindProperty("footprint").vector2IntValue = new Vector2Int(2, 2);
                s.FindProperty("interactionOffset").vector2IntValue = new Vector2Int(0, -1);
                var components = s.FindProperty("components");
                components.arraySize = 1;
                components.GetArrayElementAtIndex(0).intValue = (int)ThingComponentKind.ResourceContainer;
            });

        private static T Create<T>(string path, string id, string name, Action<SerializedObject> configure) where T : ThingDefinition
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                if (existing.DefinitionId != id) throw new InvalidOperationException("Existing definition identity differs: " + path);
                existing.Validate();
                return existing;
            }
            if (File.Exists(path) || Directory.Exists(path) || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                throw new InvalidOperationException("Asset path is already occupied: " + path);
            FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var definition = ScriptableObject.CreateInstance<T>();
            try
            {
                using (var serialized = new SerializedObject(definition))
                {
                    serialized.FindProperty("definitionId").stringValue = id;
                    serialized.FindProperty("displayName").stringValue = name;
                    configure(serialized);
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                definition.Validate();
                AssetDatabase.CreateAsset(definition, path);
                if (!EditorUtility.IsPersistent(definition)) throw new InvalidOperationException("Could not create " + path);
                return definition;
            }
            finally { if (!EditorUtility.IsPersistent(definition)) UnityEngine.Object.DestroyImmediate(definition); }
        }
    }
}
