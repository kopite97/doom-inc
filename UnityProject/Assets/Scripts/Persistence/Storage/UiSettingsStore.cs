using System;
using System.IO;
using System.Security;
using System.Text;
using UnityEngine;

namespace MobileGame.Persistence
{
    // Kept outside the game snapshot: changing or restarting a run cannot reset preferences.
    public sealed class UiSettingsStore
    {
        private const int CurrentVersion = 1;
        private readonly Func<string> read;
        private readonly Action<string> write;

        public UiSettings Current { get; private set; } = UiSettings.Default;
        public string LastError { get; private set; }
        public bool HasUnsavedChanges { get; private set; }

        public UiSettingsStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A UI settings path is required.", nameof(path));
            var fullPath = Path.GetFullPath(path);
            read = () => ReadFile(fullPath);
            write = json => WriteFile(fullPath, json);
        }

        // A missing file is represented by null. This seam also permits deterministic I/O failures.
        public UiSettingsStore(Func<string> read, Action<string> write)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
        }

        public static string PathForGameSave(string gameSavePath)
        {
            if (string.IsNullOrWhiteSpace(gameSavePath))
                throw new ArgumentException("A game save path is required.", nameof(gameSavePath));
            return Path.GetFullPath(gameSavePath) + ".ui-settings.json";
        }

        // Call once when the view is initialized, not on session replacement or orientation changes.
        // Loading never repairs a file by writing over it; only an explicit preference change writes.
        public bool Load()
        {
            Current = UiSettings.Default;
            LastError = null;
            HasUnsavedChanges = false;
            try
            {
                var json = read();
                if (json == null) return true;
                var trimmed = json.Trim();
                if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
                    throw new InvalidDataException("The UI settings file is not a JSON object.");
                var document = JsonUtility.FromJson<SettingsDocument>(json);
                if (document == null || document.version != CurrentVersion)
                    throw new InvalidDataException("The UI settings version is unsupported.");
                if (!ValidChoice(document.facilityStatus, "on", "off") || !ValidChoice(document.textSize, "normal", "large"))
                    throw new InvalidDataException("The UI settings contain an invalid preference.");
                // Missing/null fields retain defaults rather than becoming false through deserialization.
                Current = new UiSettings(document.facilityStatus != "off", document.textSize == "large");
                return true;
            }
            catch (Exception error) when (IsExpectedError(error))
            {
                LastError = error.Message;
                return false;
            }
        }

        // Apply the visible choice immediately. A failed persistence attempt must not undo the UI choice.
        public bool TrySet(bool showFacilityStatus, bool largeText)
        {
            Current = new UiSettings(showFacilityStatus, largeText);
            HasUnsavedChanges = true;
            return TrySave();
        }

        public bool TrySave()
        {
            try
            {
                write(JsonUtility.ToJson(new SettingsDocument
                {
                    version = CurrentVersion,
                    facilityStatus = Current.ShowFacilityStatus ? "on" : "off",
                    textSize = Current.LargeText ? "large" : "normal"
                }, true));
                LastError = null;
                HasUnsavedChanges = false;
                return true;
            }
            catch (Exception error) when (IsExpectedError(error))
            {
                LastError = error.Message;
                HasUnsavedChanges = true;
                return false;
            }
        }

        // JsonUtility can materialize a JSON null string field as empty/null text rather than a C# null.
        // All unset representations retain the same defaults; only explicit "off"/"large" override them.
        private static bool ValidChoice(string value, string first, string second) => string.IsNullOrEmpty(value)
            || value == "null" || value == first || value == second;
        private static bool IsExpectedError(Exception error) => error is IOException || error is UnauthorizedAccessException
            || error is InvalidDataException || error is SecurityException || error is ArgumentException || error is NotSupportedException;

        private static string ReadFile(string path)
        {
            try { return File.ReadAllText(path, Encoding.UTF8); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        private static void WriteFile(string path, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }
            }
        }

        [Serializable]
        private sealed class SettingsDocument
        {
            public int version;
            // Strings deliberately distinguish absent/null fields from an explicit OFF preference.
            public string facilityStatus;
            public string textSize;
        }
    }
}
