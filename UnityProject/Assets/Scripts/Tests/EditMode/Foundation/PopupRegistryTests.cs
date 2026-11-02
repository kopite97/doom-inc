using System;
using System.Linq;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MobileGame.Foundation.Tests
{
    public sealed class PopupRegistryTests
    {
        [Test] public void CombinedRegistryRejectsDuplicatesNullMissingAndWrongTypes()
        {
            var common = AssetDatabase.LoadAssetAtPath<PopupRegistry>(PopupAssets.RegistryPath);
            var scene = ScriptableObject.CreateInstance<PopupRegistry>();
            var resource = ScriptableObject.CreateInstance<PopupResource>();
            var replacement = ScriptableObject.CreateInstance<PopupRegistry>();
            try
            {
                Assert.That(PopupHost.Validate(common, null).Count, Is.EqualTo(3));
                scene.Configure(new[] { common.Entries[0] });
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(common, scene));
                scene.Configure(new PopupResource[] { null });
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(common, scene));
                resource.Configure("Missing", null); scene.Configure(new[] { resource });
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(common, scene));
                resource.Configure("Confirm", common.Entries.Single(e => e.Id == "Notice").Prefab);
                replacement.Configure(common.Entries.Where(e => e.Id != "Confirm").Concat(new[] { resource }).ToArray());
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(replacement, null));
                replacement.Configure(common.Entries.Where(e => e.Id != "Confirm").ToArray());
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(replacement, null));
            }
            finally { Object.DestroyImmediate(scene); Object.DestroyImmediate(resource); Object.DestroyImmediate(replacement); }
        }
        [Test] public void MissingPrefabBindingsAreRejectedBeforePresentation()
        {
            var common = AssetDatabase.LoadAssetAtPath<PopupRegistry>(PopupAssets.RegistryPath);
            var scene = ScriptableObject.CreateInstance<PopupRegistry>();
            var resource = ScriptableObject.CreateInstance<PopupResource>();
            var root = new GameObject("Missing bindings", typeof(RectTransform), typeof(CanvasGroup));
            root.SetActive(false); root.AddComponent<NoticePopup>();
            try
            {
                resource.Configure("Broken", root); scene.Configure(new[] { resource });
                Assert.Throws<InvalidOperationException>(() => PopupHost.Validate(common, scene));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(scene); Object.DestroyImmediate(resource); }
        }
    }
}
