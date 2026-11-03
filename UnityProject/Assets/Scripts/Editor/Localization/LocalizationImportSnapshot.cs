using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Localization;

namespace MobileGame.Foundation.Editor
{
    /// <summary>Editor API rollback, including Addressables registrations and generated recovery text.</summary>
    internal sealed class LocalizationImportSnapshot : IDisposable
    {
        private static readonly string[] Roots = { LocalizationCsvTools.TableRoot, LocalizationCsvTools.SettingsRoot,
            "Assets/AddressableAssetsData", "Assets/SO/Application" };
        private readonly Dictionary<UnityEngine.Object, string> objects;
        private readonly HashSet<string> paths;
        private readonly string bootstrapPath = LocalizationCsvTools.SettingsRoot + "/BootstrapText.json";
        private readonly string bootstrap;
        private readonly UnityEngine.Localization.Settings.LocalizationSettings settings;
        private readonly UnityEditor.AddressableAssets.Settings.AddressableAssetSettings addressables;
        private readonly int previousPlayModeBuilder;
        private bool committed;
        public LocalizationImportSnapshot()
        {
            paths = new HashSet<string>(Paths(), StringComparer.Ordinal);
            objects = paths.Where(p => p.EndsWith(".asset", StringComparison.Ordinal)).Select(AssetDatabase.LoadMainAssetAtPath)
                .Where(o => o != null).ToDictionary(o => o, EditorJsonUtility.ToJson);
            bootstrap = File.Exists(bootstrapPath) ? File.ReadAllText(bootstrapPath) : null;
            settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            addressables = AddressableAssetSettingsDefaultObject.Settings;
            if (addressables != null) previousPlayModeBuilder = addressables.ActivePlayModeDataBuilderIndex;
        }
        private static IEnumerable<string> Paths()
        {
            var roots = Roots.Where(AssetDatabase.IsValidFolder).ToArray();
            return roots.Length == 0 ? Array.Empty<string>() : AssetDatabase.FindAssets("", roots)
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => !AssetDatabase.IsValidFolder(p)).ToArray();
        }
        public void Commit() => committed = true;
        public void Dispose()
        {
            if (committed) return;
            foreach (var path in Paths().Where(p => !paths.Contains(p)).ToArray()) AssetDatabase.DeleteAsset(path);
            // The package keeps this selector outside the serialized settings asset.
            if (addressables != null && addressables.ActivePlayModeDataBuilderIndex != previousPlayModeBuilder)
                addressables.ActivePlayModeDataBuilderIndex = previousPlayModeBuilder;
            foreach (var snapshot in objects)
                if (snapshot.Key != null && EditorJsonUtility.ToJson(snapshot.Key) != snapshot.Value)
                { EditorJsonUtility.FromJsonOverwrite(snapshot.Value, snapshot.Key); EditorUtility.SetDirty(snapshot.Key); }
            if (bootstrap != null) { File.WriteAllText(bootstrapPath, bootstrap); AssetDatabase.ImportAsset(bootstrapPath); }
            else if (File.Exists(bootstrapPath)) AssetDatabase.DeleteAsset(bootstrapPath);
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            AddressableAssetSettingsDefaultObject.Settings = addressables;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
