using System;
using System.IO;
using MobileGame.Persistence;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Single active slot; changing scenes never discards dirty preferences.
    public static class AppUiSettings
    {
        private static string slot;
        private static UiSettingsStore current;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() { slot = null; current = null; }
        public static UiSettingsStore ForSlot(string path)
        {
            var next = Path.GetFullPath(path);
            if (current != null && string.Equals(slot, next, StringComparison.OrdinalIgnoreCase)) return current;
            slot = next; current = new UiSettingsStore(UiSettingsStore.PathForGameSave(next)); current.Load();
            return current;
        }
    }
}
