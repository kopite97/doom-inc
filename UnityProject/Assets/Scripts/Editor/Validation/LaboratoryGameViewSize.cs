using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    // Editor verification only. Restore the user's Game View selection and remove the temporary size.
    public sealed class LaboratoryGameViewSize : IDisposable
    {
        private readonly EditorWindow window;
        private readonly object group;
        private readonly PropertyInfo selected;
        private readonly int previous;
        private readonly int customIndex;
        public LaboratoryGameViewSize(int width, int height)
        {
            var assembly = typeof(EditorWindow).Assembly;
            var viewType = assembly.GetType("UnityEditor.GameView", true);
            window = EditorWindow.GetWindow(viewType); window.Show();
            selected = viewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            previous = (int)selected.GetValue(window);
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes", true);
            var sizes = sizesType.BaseType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType", true);
            group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType,
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Standalone") });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize", true);
            var kind = assembly.GetType("UnityEditor.GameViewSizeType", true);
            var size = Activator.CreateInstance(sizeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { Enum.Parse(kind, "FixedResolution"), (object)width, height, "PLAN013 Verification" }, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            var index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
            customIndex = index;
            selected.SetValue(window, index); window.Repaint();
        }
        public void Dispose()
        {
            if (window == null) return;
            selected.SetValue(window, previous);
            group.GetType().GetMethod("RemoveCustomSize").Invoke(group, new object[] { customIndex });
            window.Repaint();
        }
    }
}
