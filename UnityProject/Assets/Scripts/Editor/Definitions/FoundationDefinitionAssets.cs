using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace MobileGame.Foundation.Editor
{
    public static class FoundationDefinitionAssets
    {

        [MenuItem("Tools/Mobile Game/Create Foundation Definitions")]
        public static void CreateMissing() => GameDataAssets.EnsureAssets();
        internal static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path)) return;
            if(File.Exists(path) || Directory.Exists(path)) throw new InvalidOperationException("Existing unimported path: " + path);
            var parent = Path.GetDirectoryName(path).Replace('\\','/'); EnsureFolder(parent);
            if(string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, Path.GetFileName(path)))) throw new InvalidOperationException("Cannot create " + path);
        }
    }
}
