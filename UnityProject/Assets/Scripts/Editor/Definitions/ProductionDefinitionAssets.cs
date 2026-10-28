using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
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
        public static void CreateMissing() => GameDataAssets.EnsureAssets();
        public static ThingDefinition[] LoadDefinitions() => new[] { CulturePath, ProcessingPath, CultivationPath, SupportPath, InvaderPath }.Select(AssetDatabase.LoadAssetAtPath<ThingDefinition>).ToArray();
    }
}
