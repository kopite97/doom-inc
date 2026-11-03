using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Metadata;
using MobileGame.Persistence;

namespace MobileGame.Foundation.Editor
{
    public static class LocalizationCsvTools
    {
        public const string CsvRoot = "Assets/CSV/Localization";
        public const string TableRoot = "Assets/SO/Localization";
        public const string SettingsRoot = "Assets/Settings/Localization";
        public const string AssetsPath = TableRoot + "/LocalizationAssets.asset";

        [MenuItem("Tools/Mobile Game/Localization/Validate CSV")]
        public static void ValidateMenu() { var docs = ReadAll(); Debug.Log($"Localization CSV valid: {docs.Sum(d => d.Rows.Count)} entries."); }

        [MenuItem("Tools/Mobile Game/Localization/Normalize CSV Source")]
        public static void NormalizeMenu()
        {
            foreach (var doc in ReadAll(false)) AtomicTextFile.Write(CsvRoot + "/" + doc.Topic + ".csv", doc.Write());
            AssetDatabase.Refresh(); Debug.Log("CSV ordering/quoting normalized; review stamps unchanged.");
        }

        [MenuItem("Tools/Mobile Game/Localization/Inspect Key References")]
        public static void ReferencesMenu()
        {
            var docs = ReadAll(); var used = LocalizationReferences.Validate(docs);
            var unused = docs.SelectMany(d => d.Rows.Select(r => d.Topic + "/" + r.Key)).Where(k => !used.Contains(k)).ToArray();
            Debug.Log("Localization references valid. Unreferenced entries: " + unused.Length + "\n" + string.Join("\n", unused));
        }

        public static List<LocalizationCsvDocument> ReadAll(bool requireReviewed = true)
        {
            if (!Directory.Exists(CsvRoot)) throw new InvalidOperationException("No localization CSV directory.");
            var locales = LocalizationEditorSettings.GetLocales().Select(l => l.Identifier.Code).OrderBy(c => c).ToArray();
            if (locales.Length == 0) locales = new[] { "ko", "en" };
            var docs = Directory.GetFiles(CsvRoot, "*.csv").OrderBy(p => p, StringComparer.Ordinal)
                .Select(p => LocalizationCsvDocument.Read(Path.GetFileNameWithoutExtension(p), File.ReadAllText(p, Encoding.UTF8), locales, requireReviewed)).ToList();
            if (docs.Count == 0) throw new InvalidOperationException("No localization CSV files.");
            return docs;
        }

        // A specific key/locale must be named; import never calls this method.
        public static void ConfirmReview(string topic, string key, string language)
        {
            var doc = ReadAll(false).Single(d => d.Topic == topic);
            doc.ConfirmReview(key, language);
            AtomicTextFile.Write(CsvRoot + "/" + topic + ".csv", doc.Write());
            AssetDatabase.Refresh();
            Debug.Log("Confirmed translation review: " + topic + "/" + key + " (" + language + ")");
        }

        [MenuItem("Tools/Mobile Game/Localization/Preview CSV Changes")]
        public static void PreviewMenu() => Debug.Log(Preview(ReadAll(false)));

        [MenuItem("Tools/Mobile Game/Localization/Validate Imported Tables")]
        public static void ValidateImportedMenu() { ValidateImported(); Debug.Log("Localization tables match reviewed CSV."); }

        public static void ValidateImported()
        {
            var docs = ReadAll();
            LocalizationReferences.Validate(docs);
            ValidateTopics(docs);
            foreach (var doc in docs)
            {
                var collection = LocalizationEditorSettings.GetStringTableCollection(doc.Topic);
                if (collection == null) throw new InvalidDataException("Missing collection: " + doc.Topic);
                if (collection.SharedData.Entries.Count != doc.Rows.Count) throw new InvalidDataException(doc.Topic + ": stale/missing keys.");
                foreach (var row in doc.Rows)
                {
                    if (collection.SharedData.GetEntry(row.Key)?.Id != row.Id) throw new InvalidDataException(doc.Topic + "/" + row.Key + ": ID mismatch.");
                    if (collection.SharedData.GetEntry(row.Key).Metadata.GetMetadata<Comment>()?.CommentText != row.Context)
                        throw new InvalidDataException(doc.Topic + "/" + row.Key + ": context mismatch.");
                    foreach (var code in doc.Languages)
                    {
                        var entry = (collection.GetTable(code) as StringTable)?.GetEntry(row.Id);
                        if (entry == null || entry.LocalizedValue != row.Values[code] || entry.IsSmart != row.Smart)
                            throw new InvalidDataException(doc.Topic + "/" + row.Key + " " + code + ": CSV/table mismatch.");
                    }
                }
            }
            var assets = AssetDatabase.LoadAssetAtPath<LocalizationAssets>(AssetsPath);
            if (assets == null || assets.tables == null || !new HashSet<string>(assets.tables.Select(t => t.TableCollectionName)).SetEquals(docs.Select(d => d.Topic)))
                throw new InvalidDataException("Localization registry differs from CSV topics.");
            if (assets.bootstrapText == null || assets.bootstrapText.text != BootstrapJson(docs))
                throw new InvalidDataException("Generated BootstrapText differs from CSV.");
        }

        public static string Preview(IEnumerable<LocalizationCsvDocument> docs)
        {
            var changes = new List<string>();
            foreach (var doc in docs)
            {
                var collection = LocalizationEditorSettings.GetStringTableCollection(doc.Topic);
                foreach (var row in doc.Rows)
                {
                    var old = collection?.SharedData.GetEntry(row.Id);
                    if (old == null) changes.Add("ADD " + doc.Topic + "/" + row.Key);
                    else if (old.Key != row.Key) changes.Add("RENAME " + doc.Topic + "/" + old.Key + " -> " + row.Key);
                    foreach (var code in doc.Languages)
                    {
                        var table = collection?.GetTable(code) as StringTable;
                        if (table?.GetEntry(row.Id)?.LocalizedValue != row.Values[code]) changes.Add("VALUE " + doc.Topic + "/" + row.Key + " " + code);
                        if (code != "ko" && !TranslationReview.IsReviewed(row.Revision, code, row.Values[code], row.Reviews[code]))
                            changes.Add("REVIEW " + doc.Topic + "/" + row.Key + " " + code);
                    }
                }
                if (collection != null)
                    foreach (var old in collection.SharedData.Entries.Where(e => !doc.Rows.Any(r => r.Id == e.Id)))
                        changes.Add("REMOVE " + doc.Topic + "/" + old.Key);
            }
            return changes.Count == 0 ? "Localization: no changes." : string.Join("\n", changes);
        }

        [MenuItem("Tools/Mobile Game/Localization/Import Reviewed CSV")]
        public static void ImportMenu() => ImportAll();

        [MenuItem("Tools/Mobile Game/Localization/Apply Reviewed Key Changes")]
        public static void ImportKeyChangesMenu() => ImportAll(true);

        public static void ImportAll(bool allowKeyChanges = false, Action<int> afterTable = null)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import translations outside Play mode.");
            var docs = ReadAll(); // Validate every document before changing any asset.
            LocalizationReferences.Validate(docs);
            ValidateTopics(docs);
            var preview = Preview(docs);
            if (!allowKeyChanges && (preview.Contains("RENAME ") || preview.Contains("REMOVE ")))
                throw new InvalidOperationException("Review key references and Preview CSV Changes before explicitly allowing key changes.\n" + preview);
            var created = new List<string>();
            using (var snapshot = new LocalizationImportSnapshot())
            {
                EnsureSetup(created);
                var count = 0;
                foreach (var doc in docs)
                {
                    var collection = LocalizationEditorSettings.GetStringTableCollection(doc.Topic);
                    if (collection == null)
                    {
                        var directory = TableRoot + "/Tables/" + doc.Topic; EnsureFolder(directory);
                        collection = LocalizationEditorSettings.CreateStringTableCollection(doc.Topic, directory);
                        created.Add(AssetDatabase.GetAssetPath(collection.SharedData));
                        created.Add(AssetDatabase.GetAssetPath(collection));
                        created.AddRange(collection.StringTables.Select(AssetDatabase.GetAssetPath));
                    }
                    // Prevent silently changing an existing key's ID (including spreadsheet number corruption).
                    foreach (var row in doc.Rows)
                    {
                        var existing = collection.SharedData.GetEntry(row.Key);
                        if (existing != null && existing.Id != row.Id) throw new InvalidOperationException(doc.Topic + "/" + row.Key + ": existing ID changed.");
                    }
                    var mappings = new List<CsvColumns> { new KeyIdColumns { IncludeId = true, IncludeSharedComments = true } };
                    foreach (var code in doc.Languages)
                    {
                        if (collection.GetTable(code) == null) throw new InvalidOperationException("Create the locale table before importing: " + doc.Topic + "/" + code);
                        mappings.Add(new LocaleColumns { LocaleIdentifier = code, FieldName = code, IncludeComments = false });
                    }
                    using (var reader = new StringReader(doc.Write())) Csv.ImportInto(reader, collection, mappings, removeMissingEntries: true);
                    foreach (var table in collection.StringTables)
                    {
                        foreach (var row in doc.Rows) table.GetEntry(row.Id).IsSmart = row.Smart;
                        EditorUtility.SetDirty(table);
                    }
                    EditorUtility.SetDirty(collection.SharedData); EditorUtility.SetDirty(collection);
                    LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
                    afterTable?.Invoke(++count);
                }
                var assets = AssetDatabase.LoadAssetAtPath<LocalizationAssets>(AssetsPath);
                if (assets == null) { assets = ScriptableObject.CreateInstance<LocalizationAssets>(); AssetDatabase.CreateAsset(assets, AssetsPath); created.Add(AssetsPath); }
                assets.tables = docs.Select(d => LocalizationEditorSettings.GetStringTableCollection(d.Topic).SharedData).ToArray();
                WriteBootstrapText(docs, assets);
                EditorUtility.SetDirty(assets);
                foreach (var config in AssetDatabase.FindAssets("t:AppConfiguration", new[] { "Assets/SO/Application" })
                    .Select(g => AssetDatabase.LoadAssetAtPath<AppConfiguration>(AssetDatabase.GUIDToAssetPath(g))))
                {
                    if (config.localization == assets) continue;
                    config.localization = assets; EditorUtility.SetDirty(config);
                }
                AssetDatabase.SaveAssets();
                snapshot.Commit();
                Debug.Log("Localization import complete. " + docs.Sum(d => d.Rows.Count) + " entries; no content build executed.");
            }
        }

        private static void ValidateTopics(IEnumerable<LocalizationCsvDocument> docs)
        {
            var topics = new HashSet<string>(docs.Select(d => d.Topic));
            foreach (var collection in LocalizationEditorSettings.GetStringTableCollections())
                if (AssetDatabase.GetAssetPath(collection).StartsWith(TableRoot + "/", StringComparison.Ordinal) && !topics.Contains(collection.TableCollectionName))
                    throw new InvalidDataException("Orphan table collection: " + collection.TableCollectionName + ". Remove its references and collection explicitly before removing its CSV.");
        }

        private static void EnsureSetup(List<string> created)
        {
            EnsureFolder(SettingsRoot); EnsureFolder(TableRoot + "/Locales");
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                var path = SettingsRoot + "/LocalizationSettings.asset";
                AssetDatabase.CreateAsset(settings, path); created.Add(path);
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }
            foreach (var code in new[] { "ko", "en" })
                if (!LocalizationEditorSettings.GetLocales().Any(l => l.Identifier.Code == code))
                {
                    var locale = Locale.CreateLocale(code); var path = TableRoot + "/Locales/" + code + ".asset";
                    AssetDatabase.CreateAsset(locale, path); created.Add(path); LocalizationEditorSettings.AddLocale(locale);
                }
            var aa = AddressableAssetSettingsDefaultObject.Settings;
            if (aa != null)
            {
                var fast = aa.DataBuilders.FindIndex(b => b != null && b.GetType().Name == "BuildScriptFastMode");
                if (fast >= 0 && aa.ActivePlayModeDataBuilderIndex != fast)
                    aa.ActivePlayModeDataBuilderIndex = fast;
            }
        }

        private static void WriteBootstrapText(IEnumerable<LocalizationCsvDocument> docs, LocalizationAssets assets)
        {
            var path = SettingsRoot + "/BootstrapText.json";
            var json = BootstrapJson(docs);
            if (!File.Exists(path) || File.ReadAllText(path) != json) AtomicTextFile.Write(path, json);
            AssetDatabase.ImportAsset(path);
            assets.bootstrapText = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        private static string BootstrapJson(IEnumerable<LocalizationCsvDocument> docs)
        {
            var emergency = docs.SingleOrDefault(d => d.Topic == "Bootstrap");
            if (emergency == null) throw new InvalidOperationException("Bootstrap CSV is required for independent recovery text.");
            var content = new BootstrapTextDocument { entries = emergency.Rows.Select(r => new BootstrapTextEntry
                { key = r.Key, translations = r.Values.Select(v => new BootstrapTranslation { language = v.Key, text = v.Value }).ToArray() }).ToArray() };
            return JsonUtility.ToJson(content, true) + "\n";
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
