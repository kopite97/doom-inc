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
using Newtonsoft.Json.Linq;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.Foundation.Tests
{
    public sealed class JsonGameDataTests
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
        private Dictionary<string, string> Texts() => GameDataAssets.Files().ToDictionary(f => f.topic, f => f.asset.text, StringComparer.Ordinal);
        private GameDataCatalog Compose(Dictionary<string, string> tables = null) => GameDataComposer.Compose(new GameDataBundle(tables ?? Texts(), "test"), resources, creatures, facilities);
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

        private Dictionary<string,string> Changed(string topic, Action<JObject> mutation)
        { var text = Texts(); var obj = JObject.Parse(text[topic]); mutation(obj); text[topic] = obj.ToString(); return text; }
        [TestCase("{'schemaVersion':1,'entries':[]}")]
        [TestCase("{schemaVersion:1,\"entries\":[]}")]
        [TestCase("{\"schemaVersion\":1,\"schemaVersion\":1,\"entries\":[]}")]
        [TestCase("{\"schemaVersion\":1,\"entries\":[],}")]
        [TestCase("{\"schemaVersion\":01,\"entries\":[]}")]
        [TestCase("{\"schemaVersion\":1,/*comment*/\"entries\":[]}")]
        [TestCase("{\"schemaVersion\":NaN,\"entries\":[]}")]
        [TestCase("{\"schemaVersion\":1,\"entries\":[]} {}")]
        [TestCase("{\"schemaVersion\":1,\"entries\":null}")]
        [TestCase("[]")]
        public void MalformedJsonFailsWithLocation(string json)
        {
            var text = Texts(); text["resources"] = json;
            Assert.That(Assert.Throws<FormatException>(() => Compose(text)).Message, Does.Contain("resources.json"));
        }
        [TestCase("creatures", "moveSecondsPerCell", "0")]
        [TestCase("creatures", "moveSecondsPerCell", "1e100")]
        [TestCase("creatures", "carryCapacity", "1.5")]
        [TestCase("creatures", "carryCapacity", "2147483648")]
        [TestCase("invasion", "targetScore", "9223372036854775808")]
        [TestCase("session", "mapWidth", "0")]
        [TestCase("resources", "unknown", "1")]
        [TestCase("resources", "definitionId", "null")]
        [TestCase("creatures", "role", "\"Unknown\"")]
        [TestCase("facilities", "resource", "\"resource.missing\"")]
        [TestCase("facilities", "gatheringSeconds", "1e100")]
        [TestCase("facilities", "gatheringSeconds", "1e-100")]
        public void InvalidTypesNumbersRolesAndReferencesReject(string topic, string field, string literal)
        { Assert.That(() => Compose(Changed(topic, j => j["entries"][0][field] = JToken.Parse(literal))), Throws.Exception); }
        [Test] public void MissingFieldsDuplicateIdsAndComponentsReject()
        {
            Assert.Throws<FormatException>(() => Compose(Changed("resources", j => ((JObject)j["entries"][0]).Remove("definitionId"))));
            Assert.Throws<FormatException>(() => Compose(Changed("resources", j => j["entries"][1]["definitionId"] = j["entries"][0]["definitionId"].DeepClone())));
            Assert.Throws<FormatException>(() => Compose(Changed("resources", j => j["entries"][0]["components"] = new JArray("ResourceStack", "ResourceStack"))));
        }
        [Test] public void MissingOrUnknownTopicAndSchemaReject()
        {
            var text = Texts(); text.Remove("invasion"); Assert.Throws<FormatException>(() => Compose(text));
            text = Texts(); text.Add("unused", "{}"); Assert.Throws<FormatException>(() => Compose(text));
            Assert.Throws<FormatException>(() => Compose(Changed("session", j => j["schemaVersion"] = 2)));
        }
        [Test] public void EveryFieldMatchesFrozenCsvBeforeRuntimeConversion()
        {
            var parsed = JsonDefinitionParser.Parse(new GameDataBundle(Texts(), "test"));
            var names = LocalizationCsvTools.ReadAll().Single(d => d.Topic == "Definitions");
            foreach (var topic in JsonDefinitionParser.Topics)
            {
                var lines = File.ReadAllLines(Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "docs/plans/evidence/PLAN-015/baseline-csv", topic + ".csv"));
                var columns = lines[0].Split(','); Assert.That(parsed[topic].Count, Is.EqualTo(lines.Length - 1));
                foreach (var line in lines.Skip(1))
                {
                    var cells = line.Split(','); var dto = parsed[topic].Single(r => r.definitionId == cells[0]);
                    foreach (var field in dto.GetType().GetFields())
                    {
                        if (field.Name == "displayNameKey")
                        {
                            var key = (string)field.GetValue(dto);
                            Assert.That(names.Rows.Single(r => r.Key == key).Values["ko"], Is.EqualTo(cells[Array.IndexOf(columns, "displayName")]));
                            continue;
                        }
                        var original = cells[Array.IndexOf(columns, field.Name)]; var actual = field.GetValue(dto);
                        if (actual is ThingComponentKind[] parts) Assert.That(string.Join("|", parts.Select(c => c.ToString())), Is.EqualTo(original));
                        else if (field.FieldType == typeof(double)) Assert.That((double)actual, Is.EqualTo(double.Parse(original, CultureInfo.InvariantCulture)));
                        else Assert.That(Convert.ToString(actual, CultureInfo.InvariantCulture), Is.EqualTo(original), topic + ":" + field.Name);
                    }
                }
            }
        }
        [Test] public void OptionalDefaultsAndNumericSpellingsPreserveHashAndLongsRemainExact()
        {
            var expected = Compose().Settings.CompatibilityHash;
            var text = Changed("facilities", j => {
                foreach (JObject row in j["entries"])
                    foreach (var prop in row.Properties().Where(p =>
                        (new[] { "storageCapacity", "gatheringAmount", "gatheringSeconds", "inputAmount", "outputAmount", "productionSeconds" }.Contains(p.Name)
                            && p.Value.Type == JTokenType.Integer && (int)p.Value == 0)
                        || (p.Value.Type == JTokenType.String && (string)p.Value == "")).ToArray()) prop.Remove();
            });
            Assert.That(Compose(text).Settings.CompatibilityHash, Is.EqualTo(expected));
            text = Texts(); text["session"] = text["session"].Replace("\"staffedRate\": 1,", "\"staffedRate\": 1.0,");
            text["creatures"] = text["creatures"].Replace("0.25", "25e-2"); Assert.That(Compose(text).Settings.CompatibilityHash, Is.EqualTo(expected));
            text = Texts(); text["invasion"] = text["invasion"].Replace("\"targetScore\": 120", "\"targetScore\": 9007199254740993.0");
            Assert.That(Compose(text).Settings.Invasion.TargetScore, Is.EqualTo(9007199254740993L));
            text = Texts(); text["creatures"] = text["creatures"].Replace("0.25", "0.25000000000000001");
            Assert.That(Compose(text).Settings.CompatibilityHash, Is.Not.EqualTo(expected), "Hash must be computed before runtime float rounding.");
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
            Assert.Throws<ArgumentException>(() => GameDataComposer.Compose(new GameDataBundle(Texts(), "test"), null, creatures, facilities));
        }
        [Test] public void RowRegistryOrderAndDisplayEditsPreserveCompatibilityButValuesDoNot()
        {
            var baseline = Compose(); Entries(creatures, creatures.Entries.Reverse().Cast<UnityEngine.Object>().ToArray());
            var text = Changed("creatures", j => { var rows = (JArray)j["entries"]; j["entries"] = new JArray(rows.Reverse().Select(r => r.DeepClone())); });
            var resourcesJson = JObject.Parse(text["resources"]);
            resourcesJson["entries"][0]["displayNameKey"] = "resource.renamed-translation-key";
            text["resources"] = resourcesJson.ToString();
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
        [Test] public void EditingLocalJsonChangesActualConstructionAfterReloadAndRestoresBaseline()
        {
            const string path = "Assets/JSON/Balance/Facilities.json";
            var original = File.ReadAllText(path); var baseline = GameDataAssets.LoadCatalog();
            try
            {
                Assert.That(original, Does.Contain("\"buildSeconds\": 3"));
                File.WriteAllText(path, original.Replace("\"buildSeconds\": 3", "\"buildSeconds\": 6"));
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
            IGameDataSource local = new LocalJsonDataSource(GameDataAssets.Files());
            IGameDataSource memory = new MemorySource(Texts());
            var localTask = local.LoadAsync(default); var memoryTask = memory.LoadAsync(default);
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while ((!localTask.IsCompleted || !memoryTask.IsCompleted) && DateTime.UtcNow < deadline) yield return null;
            Assert.That(localTask.IsCompleted && memoryTask.IsCompleted, Is.True, "JSON provider timed out.");
            var first = GameDataComposer.Compose(localTask.GetAwaiter().GetResult(), resources, creatures, facilities);
            var second = GameDataComposer.Compose(memoryTask.GetAwaiter().GetResult(), resources, creatures, facilities);
            Assert.That(first.Settings.CompatibilityHash, Is.EqualTo(second.Settings.CompatibilityHash));
            Assert.That(first.SourceHash, Is.EqualTo(second.SourceHash));
            var canceled = memory.LoadAsync(new CancellationToken(true)); deadline = DateTime.UtcNow.AddSeconds(10);
            while (!canceled.IsCompleted && DateTime.UtcNow < deadline) yield return null;
            Assert.That(canceled.IsCanceled, Is.True);
        }
        private sealed class MemorySource : IGameDataSource
        {
            private readonly IDictionary<string, string> tables;
            public MemorySource(IDictionary<string, string> tables) => this.tables = tables;
            public async Task<GameDataBundle> LoadAsync(CancellationToken token) { await Task.Yield(); token.ThrowIfCancellationRequested(); return new GameDataBundle(tables, "memory"); }
        }
    }
}
