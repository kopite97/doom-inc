using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.Foundation.Tests
{
    public sealed class CsvGameDataTests
    {
        private readonly List<UnityEngine.Object> temporary = new List<UnityEngine.Object>();
        private ResourceDefinitionRegistry resources;
        private CreatureDefinitionRegistry creatures;
        private FacilityDefinitionRegistry facilities;
        [SetUp] public void Prepare()
        {
            resources = Clone(GameDataAssets.Registry<ResourceDefinitionRegistry>("Resources"));
            creatures = Clone(GameDataAssets.Registry<CreatureDefinitionRegistry>("Creatures"));
            facilities = Clone(GameDataAssets.Registry<FacilityDefinitionRegistry>("Facilities"));
        }
        private T Clone<T>(T value) where T : UnityEngine.Object { var copy = UnityEngine.Object.Instantiate(value); temporary.Add(copy); return copy; }
        [TearDown] public void Clean() { foreach (var value in temporary) if (value != null) UnityEngine.Object.DestroyImmediate(value); temporary.Clear(); }
        private Dictionary<string, string> Texts() => GameDataAssets.Tables().ToDictionary(t => t.name, t => t.text, StringComparer.Ordinal);
        private GameDataCatalog Compose(Dictionary<string, string> tables = null) => GameDataComposer.Compose(new CsvBundle(tables ?? Texts(), "test"), resources, creatures, facilities);
        private static void Entries(ScriptableObject registry, UnityEngine.Object[] values)
        {
            using (var serialized = new SerializedObject(registry))
            {
                var list = serialized.FindProperty("entries"); list.arraySize = values.Length;
                for (var i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        [Test] public void BaselineHasFrozenFingerprintAndIndependentDataWithCanonicalLinks()
        {
            var before = facilities.Entries.Select(EditorJsonUtility.ToJson).ToArray();
            var data = Compose(); Assert.That(data.Settings.CompatibilityHash, Is.EqualTo(LegacyDataBaseline.Hash));
            Assert.That(data.Definitions.Count, Is.EqualTo(9));
            var processing = data.Selected<FacilityData>("processingId"); var cultivation = data.Selected<FacilityData>("cultivationId");
            Assert.That(processing.InputResource, Is.SameAs(data.Selected<ResourceData>("organicId")));
            Assert.That(processing.OutputResource, Is.SameAs(data.Selected<ResourceData>("cultureId")));
            Assert.That(processing.InputAmount, Is.EqualTo(5)); Assert.That(processing.ProductionSeconds, Is.EqualTo(10));
            Assert.That(cultivation.InputAmount, Is.EqualTo(2)); Assert.That(cultivation.ProductionSeconds, Is.EqualTo(20));
            Assert.That(cultivation.SupportOutput, Is.SameAs(data.Selected<CreatureData>("supportId")));
            var first = LaboratorySessionFactory.Create(data.Selected<ResourceData>("organicId"), data.Selected<FacilityData>("gatheringId"),
                data.Selected<FacilityData>("storageId"), data.Selected<CreatureData>("prototypeId"));
            Assert.That(first.Map.Width, Is.EqualTo(16)); Assert.That(first.Map.Height, Is.EqualTo(10));
            Assert.That(facilities.Entries.Select(EditorJsonUtility.ToJson), Is.EqualTo(before));
            Assert.Throws<NotSupportedException>(() => ((IDictionary<string, ThingData>)data.Definitions).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<ThingComponentKind>)processing.ComponentKinds)[0] = ThingComponentKind.ResourceStack);
        }
        [Test] public void QuotedUnicodeCommaNewlineBomAndEscapedQuoteParseWithoutLosingText()
        {
            var rows = CsvParser.Parse("test", "\uFEFFdefinitionId,displayName\r\na,\"한글,\"\"이름\"\"\n둘째 줄\"\r\n", "definitionId,displayName");
            Assert.That(rows[0].Text("displayName"), Is.EqualTo("한글,\"이름\"\n둘째 줄"));
        }
        [TestCase("definitionId,definitionId\na,a\n")]
        [TestCase("definitionId,unknown\na,a\n")]
        [TestCase("definitionId,displayName\na,\"unclosed")]
        [TestCase("definitionId,displayName\na,\"closed\"extra\n")]
        [TestCase("definitionId,displayName\na,A\na,B\n")]
        [TestCase("definitionId,displayName\na,A,extra\n")]
        public void MalformedCsvFailsWithLocation(string text) => Assert.Throws<FormatException>(() => CsvParser.Parse("bad.csv", text, "definitionId,displayName"));

        [TestCase("creatures", "0.25", "NaN")]
        [TestCase("creatures", "0.25", "Infinity")]
        [TestCase("creatures", "0.25", "0")]
        [TestCase("creatures", "0.25", "1e100")]
        [TestCase("creatures", "Prototype", "Unknown")]
        [TestCase("facilities", "facility.storage", "facility.missing")]
        [TestCase("facilities", "resource.organic", "resource.missing")]
        [TestCase("invasion", ",120,", ",9223372036854775808,")]
        [TestCase("session", ",16,10,", ",0,10,")]
        public void InvalidNumbersRolesAndReferencesRejectTheWholeCatalog(string table, string from, string to)
        {
            var text = Texts(); Assert.That(text[table], Does.Contain(from)); text[table] = text[table].Replace(from, to);
            Assert.That(() => Compose(text), Throws.Exception);
        }
        [Test] public void MissingOrUnknownTableAndSchemaAreRejected()
        {
            var text = Texts(); text.Remove("invasion"); Assert.Throws<FormatException>(() => Compose(text));
            text = Texts(); text.Add("unused", "definitionId\na\n"); Assert.Throws<FormatException>(() => Compose(text));
            Assert.Throws<FormatException>(() => GameDataComposer.Compose(new CsvBundle(Texts(), "test", 2), resources, creatures, facilities));
        }
        [Test] public void InvalidInvasionRuleReportsTheTableAndRow()
        {
            var text = Texts(); text["invasion"] = text["invasion"].Replace(",120,", ",-1,");
            var error = Assert.Throws<FormatException>(() => Compose(text));
            Assert.That(error.Message, Does.Contain("invasion: line 2"));
            Assert.That(error.Message, Does.Contain("settings.invasion"));
        }
        [Test] public void RegistryNullDuplicateMissingAndWrongKindAreRejected()
        {
            var original = creatures.Entries.Cast<UnityEngine.Object>().ToArray();
            Entries(creatures, new UnityEngine.Object[] { null }); Assert.Throws<ArgumentException>(() => Compose());
            Entries(creatures, new[] { original[0], original[0] }); Assert.Throws<ArgumentException>(() => Compose());
            Entries(creatures, original.Take(1).ToArray()); Assert.Throws<FormatException>(() => Compose());
            Entries(creatures, original);
            var text = Texts(); text["creatures"] = text["creatures"].Replace("creature.invader", "resource.organic");
            text["resources"] = text["resources"].Replace("resource.organic", "creature.invader");
            Assert.Throws<FormatException>(() => Compose(text));
            Assert.Throws<ArgumentException>(() => GameDataComposer.Compose(new CsvBundle(Texts(), "test"), null, creatures, facilities));
        }
        [Test] public void RowRegistryOrderAndDisplayEditsPreserveCompatibilityButValuesDoNot()
        {
            var baseline = Compose(); Entries(creatures, creatures.Entries.Reverse().Cast<UnityEngine.Object>().ToArray());
            var text = Texts(); var rows = text["creatures"].TrimEnd().Split('\n');
            text["creatures"] = rows[0] + "\n" + string.Join("\n", rows.Skip(1).Reverse()) + "\n";
            text["resources"] = text["resources"].Replace("유기물", "새 표시 이름");
            Assert.That(Compose(text).Settings.CompatibilityHash, Is.EqualTo(baseline.Settings.CompatibilityHash));
            text["creatures"] = text["creatures"].Replace("0.25", "0.5"); var changed = Compose(text);
            Assert.That(changed.Settings.CompatibilityHash, Is.Not.EqualTo(baseline.Settings.CompatibilityHash));
            Assert.That(changed.Selected<CreatureData>("prototypeId").MoveSecondsPerCell, Is.EqualTo(0.5));
            Assert.That(baseline.Selected<CreatureData>("prototypeId").MoveSecondsPerCell, Is.EqualTo(0.25));
        }
        [Test] public void NewSaveFingerprintRejectsChangedDataAndLegacyRequiresBaseline()
        {
            var baseline = Compose(); var codec = new SessionSnapshotCodec(baseline.Definitions.Values, true);
            var session = LaboratorySessionFactory.Create(baseline.Selected<ResourceData>("organicId"), baseline.Selected<FacilityData>("gatheringId"),
                baseline.Selected<FacilityData>("storageId"), baseline.Selected<CreatureData>("prototypeId"), baseline.Selected<ResourceData>("cultureId"));
            var save = codec.Capture(session, SessionPhase.Playing); Assert.That(save.version, Is.EqualTo(5));
            Assert.That(save.dataHash, Is.EqualTo(LegacyDataBaseline.Hash)); Assert.DoesNotThrow(() => codec.Restore(save));
            var text = Texts(); text["creatures"] = text["creatures"].Replace("0.25", "0.5");
            var other = new SessionSnapshotCodec(Compose(text).Definitions.Values, true);
            Assert.That(() => other.Restore(save), Throws.Exception);
            save.version = 4; save.dataHash = null; Assert.DoesNotThrow(() => codec.Restore(save));
            Assert.That(() => other.Restore(save), Throws.Exception);
            save.version = 5; Assert.That(() => codec.Restore(save), Throws.Exception);
        }
        [Test] public void EditingLocalCsvChangesActualConstructionAfterReloadAndRestoresBaseline()
        {
            const string path = "Assets/CSV/Balance/facilities.csv";
            var original = File.ReadAllText(path); var baseline = GameDataAssets.LoadCatalog();
            try
            {
                Assert.That(original, Does.Contain("resource.organic,2,3,"));
                File.WriteAllText(path, original.Replace("resource.organic,2,3,", "resource.organic,2,6,"));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var changed = GameDataAssets.LoadCatalog();
                var first = Building(baseline); var second = Building(changed);
                first.Advance(4); second.Advance(4);
                Assert.That(first.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility).GetComponent<ConstructionComponent>().IsComplete, Is.True);
                Assert.That(second.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility).GetComponent<ConstructionComponent>().IsComplete, Is.False);
                second.Advance(4);
                Assert.That(second.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility).GetComponent<ConstructionComponent>().IsComplete, Is.True);
                Assert.That(baseline.Selected<FacilityData>("gatheringId").BuildSeconds, Is.EqualTo(3));
            }
            finally
            {
                File.WriteAllText(path, original);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
            Assert.That(GameDataAssets.LoadCatalog().Settings.CompatibilityHash, Is.EqualTo(LegacyDataBaseline.Hash));
        }
        private static LaboratoryManager Building(GameDataCatalog data)
        {
            var session = LaboratorySessionFactory.Create(data.Selected<ResourceData>("organicId"), data.Selected<FacilityData>("gatheringId"),
                data.Selected<FacilityData>("storageId"), data.Selected<CreatureData>("prototypeId"));
            var lab = (LaboratoryManager)session.Runtime;
            Assert.That(lab.BeginConstruction(data.Selected<FacilityData>("gatheringId")), Is.True);
            Assert.That(lab.PreviewConstruction(new Vector2Int(2, 2), QuarterTurn.None), Is.True);
            Assert.That(lab.ConfirmConstruction(), Is.True);
            return lab;
        }
        [UnityTest] public IEnumerator LocalAndMemoryProvidersUseTheSameComposerContract()
        {
            ICsvDataSource local = new LocalCsvDataSource(GameDataAssets.Tables());
            ICsvDataSource memory = new MemorySource(Texts());
            var localTask = local.LoadAsync(default); var memoryTask = memory.LoadAsync(default);
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while ((!localTask.IsCompleted || !memoryTask.IsCompleted) && DateTime.UtcNow < deadline) yield return null;
            Assert.That(localTask.IsCompleted && memoryTask.IsCompleted, Is.True, "CSV provider timed out.");
            var first = GameDataComposer.Compose(localTask.GetAwaiter().GetResult(), resources, creatures, facilities);
            var second = GameDataComposer.Compose(memoryTask.GetAwaiter().GetResult(), resources, creatures, facilities);
            Assert.That(first.Settings.CompatibilityHash, Is.EqualTo(second.Settings.CompatibilityHash));
            Assert.That(first.SourceHash, Is.EqualTo(second.SourceHash));
            var canceled = memory.LoadAsync(new CancellationToken(true)); deadline = DateTime.UtcNow.AddSeconds(10);
            while (!canceled.IsCompleted && DateTime.UtcNow < deadline) yield return null;
            Assert.That(canceled.IsCanceled, Is.True);
        }
        private sealed class MemorySource : ICsvDataSource
        {
            private readonly IDictionary<string, string> tables;
            public MemorySource(IDictionary<string, string> tables) => this.tables = tables;
            public async Task<CsvBundle> LoadAsync(CancellationToken token) { await Task.Yield(); token.ThrowIfCancellationRequested(); return new CsvBundle(tables, "memory"); }
        }
    }
}
