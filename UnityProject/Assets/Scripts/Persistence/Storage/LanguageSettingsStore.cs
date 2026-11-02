using System;
using System.IO;
using System.Security;
using UnityEngine;

namespace MobileGame.Persistence
{
    public sealed class LanguageSettingsStore
    {
        private readonly Func<string> read;
        private readonly Action<string> write;
        public string Language { get; private set; }
        public string LastError { get; private set; }
        public bool HasUnsavedChanges { get; private set; }
        public LanguageSettingsStore(string path) : this(() => ReadOptional(path),
            text => AtomicTextFile.Write(path, text)) { }
        private static string ReadOptional(string path)
        {
            try { return File.ReadAllText(path); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }
        public LanguageSettingsStore(Func<string> read, Action<string> write)
        { this.read = read ?? throw new ArgumentNullException(nameof(read)); this.write = write ?? throw new ArgumentNullException(nameof(write)); }
        public void Load()
        {
            Language = null; LastError = null; HasUnsavedChanges = false;
            try
            {
                var json = read(); if (json == null) return;
                var doc = JsonUtility.FromJson<Document>(json);
                if (doc == null || doc.version != 1 || string.IsNullOrWhiteSpace(doc.language)) throw new InvalidDataException("Invalid language preference.");
                Language = doc.language;
            }
            catch (Exception e) when (Expected(e)) { LastError = e.Message; }
        }
        public bool TrySet(string language)
        { Language = language; HasUnsavedChanges = true; return TrySave(); }
        public bool TrySave()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Language)) throw new InvalidDataException("No language selected.");
                write(JsonUtility.ToJson(new Document { version = 1, language = Language }, true));
                HasUnsavedChanges = false; LastError = null; return true;
            }
            catch (Exception e) when (Expected(e)) { LastError = e.Message; return false; }
        }
        private static bool Expected(Exception e) => e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is SecurityException
            || e is ArgumentException || e is NotSupportedException;
        [Serializable] private sealed class Document { public int version; public string language; }
    }
}
