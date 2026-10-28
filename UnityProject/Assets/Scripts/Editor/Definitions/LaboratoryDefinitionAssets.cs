using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace MobileGame.Foundation.Editor
{
    public static class LaboratoryDefinitionAssets
    {
        public const string ResourcePath = "Assets/SO/Resource/OrganicMatter.asset";
        public const string StoragePath = "Assets/SO/Facility/Storage.asset";
        public const string GatheringPath = "Assets/SO/Facility/Gathering.asset";
        public const string PrototypePath = "Assets/SO/Creature/PrototypeSupport.asset";
        [MenuItem("Tools/Mobile Game/Create Laboratory Definitions")]
        public static void CreateOrUpgrade() => GameDataAssets.EnsureAssets();

    }
}
