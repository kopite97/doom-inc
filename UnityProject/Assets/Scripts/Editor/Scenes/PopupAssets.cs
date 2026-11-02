using System;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static class PopupAssets
    {
        public const string RegistryPath = "Assets/SO/UI/Popup/CommonPopupRegistry.asset";
        public const string HostPath = "Assets/Prefabs/UI/Popup/Temp_PopupHost.prefab";
        [MenuItem("Tools/Mobile Game/Popup/Create Missing Assets")]
        public static void CreateMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Create popup assets outside Play.");
            FoundationDefinitionAssets.EnsureFolder("Assets/Prefabs/UI/Popup");
            FoundationDefinitionAssets.EnsureFolder("Assets/SO/UI/Popup");
            var resources = new[] { Resource<ConfirmPopup>("Confirm"), Resource<NoticePopup>("Notice"), Resource<SettingsPopup>("Settings") };
            var registry = AssetDatabase.LoadAssetAtPath<PopupRegistry>(RegistryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<PopupRegistry>(); registry.Configure(resources);
                AssetDatabase.CreateAsset(registry, RegistryPath);
            }
            PopupHost.Validate(registry, null);
            var host = AssetDatabase.LoadAssetAtPath<PopupHost>(HostPath);
            if (host == null)
            {
                var root = new GameObject("Temp_PopupHost");
                try { root.AddComponent<PopupHost>(); host = PrefabUtility.SaveAsPrefabAsset(root, HostPath).GetComponent<PopupHost>(); }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            var config = AppSceneAssets.EnsureConfiguration();
            if (config.popupHost != null && config.popupHost != host || config.popups != null && config.popups != registry)
                throw new InvalidOperationException("Application already uses another popup configuration.");
            config.popupHost = host; config.popups = registry;
            EditorUtility.SetDirty(config); AssetDatabase.SaveAssets();
            Debug.Log("Popup assets validated and connected. No build invoked.");
        }
        private static PopupResource Resource<T>(string id) where T : PopupBase
        {
            var path = "Assets/Prefabs/UI/Popup/Temp_" + id + "Popup.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                var root = new GameObject("Temp_" + id + "Popup", typeof(RectTransform), typeof(CanvasGroup));
                root.SetActive(false);
                try { root.AddComponent<T>().CreateTemplate(); prefab = PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            var resourcePath = "Assets/SO/UI/Popup/" + id + "PopupResource.asset";
            var resource = AssetDatabase.LoadAssetAtPath<PopupResource>(resourcePath);
            if (resource == null)
            {
                resource = ScriptableObject.CreateInstance<PopupResource>(); resource.Configure(id, prefab);
                AssetDatabase.CreateAsset(resource, resourcePath);
            }
            return resource;
        }
        [MenuItem("Tools/Mobile Game/Popup/Validate Assets")]
        public static void Validate()
        {
            var config = AppSceneAssets.EnsureConfiguration();
            if (config.popupHost == null) throw new InvalidOperationException("Missing popup host.");
            PopupHost.Validate(config.popups, null);
            foreach (var item in config.popups.Entries)
            {
                var serialized = new SerializedObject(item.Prefab.GetComponent<PopupBase>());
                foreach (var field in new[] { "heading", "scroll", "confirm", "cancel" })
                    if (serialized.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException(item.Id + ": missing " + field);
            }
            Debug.Log("Popup registry and prefab bindings valid.");
        }
        [MenuItem("Tools/Mobile Game/Popup/Review Confirm Translation")]
        public static void ReviewConfirm() => LocalizationCsvTools.ConfirmReview("Common", "confirm", "en");
    }
}
