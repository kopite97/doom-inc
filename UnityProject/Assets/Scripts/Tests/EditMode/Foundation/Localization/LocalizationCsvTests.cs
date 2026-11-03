using System;
using System.IO;
using System.Linq;
using MobileGame.Foundation.Editor;
using NUnit.Framework;
using UnityEditor;

namespace MobileGame.Foundation.Tests
{
    public sealed class LocalizationCsvTests
    {
        private static LocalizationCsvDocument Fixture()
        {
            var doc = new LocalizationCsvDocument("Fixture", new[] { "ko", "en" });
            var row = new LocalizationCsvRow { Key = "stock", Id = 7, Context = "Inventory, remaining stock", Smart = true, Contract = "count:number" };
            row.Values.Add("ko", "보유 \"자원\",\n{count:N0}개"); row.Values.Add("en", "Stock \"resource\",\n{count:N0} {count:plural:item|items}");
            row.Reviews.Add("en", ""); doc.Rows.Add(row); doc.ConfirmReview("stock", "en"); return doc;
        }
        [Test]
        public void CsvRoundTripPreservesBomQuotesNewlinesContextIdsAndSmartContract()
        {
            var original = Fixture();
            var parsed = LocalizationCsvDocument.Read("Fixture", "\uFEFF" + original.Write(), original.Languages);
            Assert.That(parsed.Write(), Is.EqualTo(original.Write()));
            Assert.That(parsed.Rows.Single().Id, Is.EqualTo(7));
        }
        [TestCase("key")]
        [TestCase("id")]
        [TestCase("empty")]
        [TestCase("unused")]
        [TestCase("unknown")]
        [TestCase("format")]
        [TestCase("review")]
        public void InvalidInputIsRejectedWithTopicAndKey(string failure)
        {
            var doc = Fixture(); var row = doc.Rows[0];
            if (failure == "key") doc.Rows.Add(row);
            if (failure == "id") row.Id = 0;
            if (failure == "empty") row.Values["en"] = " ";
            if (failure == "unused") row.Values["en"] = "No count";
            if (failure == "unknown") row.Values["en"] = "{wrong}";
            if (failure == "format") row.Values["en"] = "{count";
            if (failure == "review") row.Context += " Changed meaning";
            var error = Assert.Throws<FormatException>(() => LocalizationCsvDocument.Read("Fixture", doc.Write(), doc.Languages));
            Assert.That(error.Message, Does.Contain("Fixture").And.Contain("stock"));
        }
        [Test]
        public void ThirdLocaleAndExplicitReviewNeedNoColumnCountChange()
        {
            var doc = new LocalizationCsvDocument("Fixture", new[] { "ko", "en", "fr" });
            var row = Fixture().Rows[0]; row.Values.Add("fr", "{count:N0} {count:plural:objet|objets}"); row.Reviews.Add("fr", ""); doc.Rows.Add(row);
            Assert.Throws<FormatException>(() => LocalizationCsvDocument.Read(doc.Topic, doc.Write(), doc.Languages));
            doc.ConfirmReview("stock", "fr");
            Assert.That(LocalizationCsvDocument.Read(doc.Topic, doc.Write(), doc.Languages).Languages.Count, Is.EqualTo(3));
            row.Values["ko"] = "수량 {count}";
            Assert.Throws<FormatException>(() => LocalizationCsvDocument.Read(doc.Topic, doc.Write(), doc.Languages));
            Assert.That(row.Reviews["en"], Is.Not.EqualTo(TranslationReview.ReviewStamp(row.Revision, "en", row.Values["en"])));
            doc.ConfirmReview("stock", "en"); doc.ConfirmReview("stock", "fr");
            Assert.DoesNotThrow(() => LocalizationCsvDocument.Read(doc.Topic, doc.Write(), doc.Languages));
        }
        [Test]
        public void ReferencedKeyDeletionIsBlockedBeforeImport()
        {
            var docs = LocalizationCsvTools.ReadAll();
            docs.Single(d => d.Topic == "Title").Rows.RemoveAll(r => r.Key == "new_game");
            Assert.That(Assert.Throws<InvalidDataException>(() => LocalizationReferences.Validate(docs)).Message, Does.Contain("Title/new_game"));
        }
        [Test]
        public void KeyMaintenancePreservesIdAndDetectsManualTableDrift()
        {
            var path = LocalizationCsvTools.CsvRoot + "/Common.csv";
            var original = File.ReadAllText(path);
            var doc = LocalizationCsvTools.ReadAll().Single(d => d.Topic == "Common");
            var row = new LocalizationCsvRow { Key = "maintenance_fixture", Id = 900000000000001, Context = "Temporary maintenance test", Contract = "" };
            row.Values.Add("ko", "검증 문구"); row.Values.Add("en", "Verification text"); row.Reviews.Add("en", "");
            doc.Rows.Add(row); doc.ConfirmReview(row.Key, "en");
            try
            {
                File.WriteAllText(path, doc.Write()); LocalizationCsvTools.ImportAll(); LocalizationCsvTools.ValidateImported();
                var collection = UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection("Common");
                Assert.That(collection.SharedData.GetEntry(row.Key).Id, Is.EqualTo(row.Id));
                row.Key = "maintenance_renamed";
                File.WriteAllText(path, doc.Write());
                Assert.Throws<InvalidOperationException>(() => LocalizationCsvTools.ImportAll());
                LocalizationCsvTools.ImportAll(allowKeyChanges: true); LocalizationCsvTools.ValidateImported();
                Assert.That(collection.SharedData.GetEntry(row.Id).Key, Is.EqualTo(row.Key));
                var table = (UnityEngine.Localization.Tables.StringTable)collection.GetTable("en");
                table.GetEntry(row.Id).Value = "Manual drift";
                Assert.Throws<InvalidDataException>(() => LocalizationCsvTools.ValidateImported());
                LocalizationCsvTools.ImportAll(); LocalizationCsvTools.ValidateImported();
                Assert.That(table.GetEntry(row.Id).LocalizedValue, Is.EqualTo("Verification text"));
            }
            finally
            {
                File.WriteAllText(path, original); LocalizationCsvTools.ImportAll(allowKeyChanges: true); AssetDatabase.Refresh();
            }
            LocalizationCsvTools.ValidateImported();
            Assert.That(UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection("Common").SharedData.GetEntry(row.Id), Is.Null);
        }
        [Test]
        public void FailedImportRestoresPreviousEditorPlayModeSelection()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            var original = settings.ActivePlayModeDataBuilderIndex;
            var alternate = settings.DataBuilders.FindIndex(builder =>
                builder != null && builder.GetType().Name == "BuildScriptPackedPlayMode");
            Assert.That(alternate, Is.GreaterThanOrEqualTo(0));
            try
            {
                settings.ActivePlayModeDataBuilderIndex = alternate;
                Assert.Throws<IOException>(() => LocalizationCsvTools.ImportAll(afterTable: _ => throw new IOException("Injected failure")));
                Assert.That(settings.ActivePlayModeDataBuilderIndex, Is.EqualTo(alternate));
            }
            finally
            {
                if (settings.ActivePlayModeDataBuilderIndex != original)
                    settings.ActivePlayModeDataBuilderIndex = original;
            }
        }



        [Test]
        public void ImportFailureRestoresExistingAssetsAndRepeatImportHasStableContent()
        {
            LocalizationCsvTools.ValidateImported();
            var paths = AssetDatabase.FindAssets("", new[] { LocalizationCsvTools.TableRoot, LocalizationCsvTools.SettingsRoot, "Assets/AddressableAssetsData" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(".asset")).OrderBy(p => p).ToArray();
            // Canonicalize package-generated templates before byte comparisons; Unity fills provider type metadata on first serialization.
            AssetDatabase.ForceReserializeAssets(paths);
            var before = paths.ToDictionary(p => p, File.ReadAllText);
            var bootstrap = File.ReadAllText(LocalizationCsvTools.SettingsRoot + "/BootstrapText.json");
            var csvPath = LocalizationCsvTools.CsvRoot + "/Bootstrap.csv"; var source = File.ReadAllText(csvPath);
            try
            {
                var doc = LocalizationCsvTools.ReadAll().Single(d => d.Topic == "Bootstrap");
                var row = doc.Rows.Single(r => r.Key == "loading"); row.Values["ko"] += " (검증)"; doc.ConfirmReview(row.Key, "en");
                File.WriteAllText(csvPath, doc.Write());
                Assert.Throws<IOException>(() => LocalizationCsvTools.ImportAll(afterTable: count => { if (count == 1) throw new IOException("Injected apply failure"); }));
            }
            finally { File.WriteAllText(csvPath, source); AssetDatabase.Refresh(); }
            foreach (var path in paths) Assert.That(File.ReadAllText(path), Is.EqualTo(before[path]), path);
            Assert.That(File.ReadAllText(LocalizationCsvTools.SettingsRoot + "/BootstrapText.json"), Is.EqualTo(bootstrap));
            LocalizationCsvTools.ValidateImported(); LocalizationCsvTools.ImportAll(); LocalizationCsvTools.ValidateImported();
            foreach (var path in paths) Assert.That(File.ReadAllText(path), Is.EqualTo(before[path]), "Idempotent import: " + path);
        }
    }
}
