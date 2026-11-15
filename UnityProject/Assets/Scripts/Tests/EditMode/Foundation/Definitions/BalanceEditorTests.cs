using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class BalanceEditorTests
    {
        private string folder;
        private BalanceDocument[] documents;
        private BalanceEditSession session;
        private int imports;
        [SetUp] public void Setup()
        {
            folder = Path.Combine(Path.GetTempPath(), "DoomBalanceTests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
            foreach (var topic in JsonDefinitionTopics.All) File.Copy("Assets/JSON/Balance/" + topic.FileName, Path.Combine(folder, topic.FileName));
            Reload(); session = new BalanceEditSession(); session.Reset(documents); imports = 0;
        }
        [TearDown] public void Cleanup() { if (folder != null && Path.GetFileName(folder).StartsWith("DoomBalanceTests-", StringComparison.Ordinal)) Directory.Delete(folder, true); }
        private void Reload() => documents = JsonDefinitionTopics.All.Select(t => new BalanceDocument(t, Path.Combine(folder, t.FileName))).ToArray();
        private BalanceDocument Doc(string topic) => documents.Single(d => d.Topic.Key == topic);
        private void Change(string topic, string id, string field, string value) => session.Set(Doc(topic), id,
            BalanceMetadataReader.Read(Doc(topic).Topic.DtoType).Single(f => f.Field.Name == field), value);
        private void Validate(IReadOnlyDictionary<string, string> texts)
        {
            var config = BalanceEditorData.Configuration;
            GameDataComposer.Compose(new GameDataBundle(new Dictionary<string, string>(texts), "balance-test"), config.resources, config.creatures, config.facilities);
        }
        private BalanceDocumentStore Store(Action<string> checkpoint = null, Action refresh = null, Action<IReadOnlyDictionary<string, string>> validation = null)
            => new BalanceDocumentStore(documents, Path.Combine(folder, "recovery"), validation ?? Validate, refresh ?? (() => imports++), checkpoint);

        [Test] public void MetadataLeavesRuntimeFingerprintAndExcludedFieldsUntouched()
        {
            Assert.That(GameDataAssets.LoadCatalog().Settings.CompatibilityHash, Is.EqualTo(LegacyDataBaseline.RaiderBaselineHash));
            Assert.That(BalanceMetadataReader.Read(typeof(SessionDto)).Single(f => f.Field.Name == "endingSeconds").Attribute, Is.Null);
            Assert.That(BalanceMetadataReader.Read(typeof(InvasionDto)).Single(f => f.Field.Name == "targetScore").Attribute.Unit, Is.EqualTo("점"));
            Assert.That(BalanceMetadataReader.Read(typeof(ResourceDto)).All(f => f.Attribute == null), Is.True);
            BalanceEditorData.CheckCurrentBindings();
        }
        [Test] public void SaveOnlyChangesSelectedNumberAndPreservesOtherBytes()
        {
            var before = Doc("facilities").Bytes;
            Change("facilities", "facility.processing", "productionSeconds", "11.25");
            var doc = Doc("facilities"); var token = doc.Entry("facility.processing").Fields["productionSeconds"];
            var expected = doc.Encode(doc.Text.Remove(token.Start, token.Length).Insert(token.Start, "11.25"));
            Store().Save(session);
            Assert.That(File.ReadAllBytes(doc.Path), Is.EqualTo(expected)); Assert.That(imports, Is.EqualTo(1));
            foreach (var other in documents.Where(d => d.Topic.Key != "facilities")) Assert.That(File.ReadAllBytes(other.Path), Is.EqualTo(other.Bytes));
            Assert.That(Store().Pending(), Is.Empty); Assert.That(before, Is.Not.EqualTo(expected));
        }
        [Test] public void NoChangesDoesNotWriteOrImport()
        {
            var stamps = documents.Select(d => File.GetLastWriteTimeUtc(d.Path)).ToArray(); Store().Save(session);
            Assert.That(documents.Select(d => File.GetLastWriteTimeUtc(d.Path)).ToArray(), Is.EqualTo(stamps)); Assert.That(imports, Is.Zero);
        }
        [TestCase("0")][TestCase("NaN")][TestCase("1e1000")][TestCase("\"2\"")]
        public void InvalidValueCannotWrite(string value)
        {
            Change("facilities", "facility.processing", "productionSeconds", value);
            Assert.Catch(() => Store().Save(session)); Assert.That(File.ReadAllBytes(Doc("facilities").Path), Is.EqualTo(Doc("facilities").Bytes));
        }
        [Test] public void CrossFieldErrorIdentifiesReadOnlyReportThreshold()
        {
            Change("invasion", "settings.invasion", "targetScore", "10");
            var error = Assert.Catch(() => Store().Save(session));
            Assert.That(error.Message, Does.Contain("targetScore").And.Contain("detailedUntilScore"));
            Assert.That(File.ReadAllBytes(Doc("invasion").Path), Is.EqualTo(Doc("invasion").Bytes));
        }
        [Test] public void StartingCarryConstraintUsesRealComposer()
        {
            Change("facilities", "facility.gathering", "buildCost", "9");
            Assert.That(Assert.Catch(() => Store().Validate(session)).Message, Does.Contain("carry capacity"));
        }
        [Test] public void ExternalModificationBlocksSaveAndPreservesDraft()
        {
            Change("session", "settings.session", "staffedRate", "2"); var path = Doc("session").Path;
            File.AppendAllText(path, "\n"); var bytes = File.ReadAllBytes(path);
            Assert.Throws<IOException>(() => Store().Save(session)); Assert.That(File.ReadAllBytes(path), Is.EqualTo(bytes)); Assert.That(session.Dirty, Is.True);
        }
        [TestCase("before-write:invasion")][TestCase("before-import")][TestCase("before-final-validation")]
        public void FailuresRestoreEveryOriginalAndKeepDraft(string phase)
        {
            Change("facilities", "facility.processing", "productionSeconds", "12"); Change("invasion", "settings.invasion", "targetScore", "200");
            var store = Store(point => { if (point == phase) throw new IOException("injected failure"); });
            Assert.Throws<IOException>(() => store.Save(session));
            foreach (var doc in documents) Assert.That(File.ReadAllBytes(doc.Path), Is.EqualTo(doc.Bytes));
            Assert.That(session.Dirty, Is.True); Assert.That(store.Pending(), Is.Empty);
        }
        [Test] public void FinalValidationFailureAlsoRollsBack()
        {
            Change("session", "settings.session", "staffedRate", "2"); var calls = 0;
            var store = Store(validation: texts => { Validate(texts); if (++calls == 2) throw new IOException("final validation failed"); });
            Assert.Throws<IOException>(() => store.Save(session)); Assert.That(calls, Is.EqualTo(3));
            Assert.That(File.ReadAllBytes(Doc("session").Path), Is.EqualTo(Doc("session").Bytes));
        }
        [Test] public void FailedRecoveryPersistsJournalAndCanRecoverOnNextSession()
        {
            Change("session", "settings.session", "staffedRate", "2");
            var store = Store(refresh: () => throw new IOException("import failure"));
            Assert.That(Assert.Throws<IOException>(() => store.Save(session)).Message, Does.Contain("복구 미완료"));
            var journal = store.Pending().Single(); Assert.That(File.Exists(journal), Is.True);
            Assert.Throws<IOException>(() => store.Save(session));
            Reload(); var restarted = Store(); restarted.Recover(journal); Assert.That(restarted.Pending(), Is.Empty);
        }
        [Test] public void RecoveryNeverOverwritesOutsideChanges()
        {
            Change("session", "settings.session", "staffedRate", "2");
            var path = Doc("session").Path; var store = Store(point => { if (point == "before-final-validation") { File.AppendAllText(path, "\n "); throw new IOException("external change"); } });
            Assert.Throws<IOException>(() => store.Save(session)); var outside = File.ReadAllBytes(path);
            Assert.That(store.Pending().Count(), Is.EqualTo(1)); Assert.Throws<IOException>(() => store.Recover(store.Pending().Single()));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(outside));
        }
        [Test] public void ChangedDtoSignatureOrFieldRejectsRecoveredDraft()
        {
            Change("session", "settings.session", "staffedRate", "2"); session.changes[0].signature = "old-type";
            Assert.That(session.Conflicts(documents), Is.Not.Empty); Assert.Throws<InvalidOperationException>(() => Store().Preview(session));
            session.changes[0].field = "renamed"; Assert.That(session.Conflicts(documents), Is.Not.Empty);
        }
        [Test] public void PresentationMetadataDoesNotEnterDraftSignature()
        {
            var first = new BalanceFieldInfo(typeof(MetadataExample).GetField("amount")); var before = first.Signature;
            first.Attribute.Unit = "새 단위 라벨"; first.Attribute.Group = "새 그룹";
            Assert.That(first.Signature, Is.EqualTo(before));
        }
        [Test] public void RemovedAttributeOrChangedConditionInvalidatesDraftSignature()
        {
            var field = new BalanceFieldInfo(typeof(MetadataExample).GetField("conditional"));
            var signature = field.Signature;
            field.Attribute.ConditionValues = new[] { "off" };
            Assert.That(field.Signature, Is.Not.EqualTo(signature));
            Change("session", "settings.session", "staffedRate", "2");
            session.changes[0].field = "endingSeconds";
            Assert.That(session.Conflicts(documents), Is.Not.Empty);
            Assert.Throws<InvalidOperationException>(() => Store().Preview(session));
        }
        [Test] public void ExternalDeletionBlocksSaveAndRecoveryWithoutRecreatingFile()
        {
            Change("session", "settings.session", "staffedRate", "2");
            var path = Doc("session").Path;
            var store = Store(point => { if (point == "before-import") { File.Delete(path); throw new IOException("external deletion"); } });
            Assert.Throws<IOException>(() => store.Save(session));
            Assert.That(File.Exists(path), Is.False);
            Assert.That(store.Pending().Count(), Is.EqualTo(1));
            Assert.Throws<IOException>(() => store.Recover(store.Pending().Single()));
            Assert.Throws<IOException>(() => store.Save(session));
            Assert.That(File.Exists(path), Is.False);
        }
        public sealed class MetadataExample : DefinitionDto
        {
            [BalanceField("수량", "테스트 수량")] public long amount = 1;
            public string mode = "off";
            [BalanceField("조건값", "조건 테스트", ConditionField = nameof(mode), ConditionValues = new[] { "on" })] public int conditional = 1;
            [BalanceField("배열", "지원하지 않는 자료형")] public int[] unsupported = Array.Empty<int>();
            [BalanceField("잘못된 조건", "잘못된 조건 테스트", ConditionField = "missing", ConditionValues = new[] { "on" })] public int invalid = 1;
            [BalanceField("누락", "JSON에는 없는 필드")] public double absent = 1;
        }
        [Test] public void GenericNewTopicProducesDiagnosticsWithoutAnEditorRegistry()
        {
            var topic = new JsonDefinitionTopic("test-new", "Test.json", typeof(MetadataExample), "테스트");
            var path = Path.Combine(folder, "Test.json"); File.WriteAllText(path, "{\"schemaVersion\":1,\"entries\":[{\"definitionId\":\"new.id\",\"amount\":9007199254740993,\"mode\":\"off\",\"conditional\":1}]}");
            var doc = new BalanceDocument(topic, path); var entry = doc.Entry("new.id"); var fields = BalanceMetadataReader.Read(topic.DtoType).ToDictionary(f => f.Field.Name);
            Assert.That(fields["amount"].Diagnostic(entry), Is.EqualTo("편집 가능"));
            Assert.That(fields["mode"].Diagnostic(entry), Is.EqualTo("속성 없음"));
            Assert.That(fields["conditional"].Diagnostic(entry), Does.Contain("조건 불일치"));
            Assert.That(fields["unsupported"].Diagnostic(entry), Is.EqualTo("미지원 자료형"));
            Assert.That(fields["invalid"].Diagnostic(entry), Does.Contain("조건 참조 오류"));
            Assert.That(fields["absent"].Diagnostic(entry), Is.EqualTo("JSON 값 없음"));
            var draft = new BalanceEditSession(); draft.Reset(new[] { doc }); draft.Set(doc, "new.id", fields["amount"], "9007199254740995");
            Assert.That(doc.Candidate(draft.changes), Does.Contain("9007199254740995"));
        }
        [Test] public void ExactSpansPreserveBomCrLfEscapesAndUneditedDecimal()
        {
            var topic = new JsonDefinitionTopic("exact", "Exact.json", typeof(MetadataExample), "정밀도"); var path = Path.Combine(folder, "Exact.json");
            var text = "{\r\n \"schemaVersion\":1,\"entries\":[{\"definitionId\":\"a\\u0062\",\"amount\":1,\"other\":1.234567890123456789123,\"note\":\"amount:1\"},{\"definitionId\":\"c\",\"amount\":1}]}\r\n";
            File.WriteAllText(path, text, new UTF8Encoding(true)); var doc = new BalanceDocument(topic, path);
            var draft = new BalanceEditSession(); draft.Reset(new[] { doc }); draft.Set(doc, "ab", BalanceMetadataReader.Read(topic.DtoType).Single(f => f.Field.Name == "amount"), "123");
            var candidate = doc.Candidate(draft.changes); var token = doc.Entry("ab").Fields["amount"];
            Assert.That(candidate, Is.EqualTo(text.Remove(token.Start, token.Length).Insert(token.Start, "123")));
            Assert.That(doc.Encode(candidate).Take(3), Is.EqualTo(new byte[] { 239, 187, 191 }));
        }
        [TestCase("file")][TestCase("missing")][TestCase("duplicate")]
        public void RuntimeBindingMismatchIsDetectedWithoutChangingConfiguration(string mismatch)
        {
            var config = UnityEngine.Object.Instantiate(BalanceEditorData.Configuration);
            try
            {
                config.jsonFiles = Array.ConvertAll(config.jsonFiles, f => new JsonDataAsset(f.topic, f.asset));
                var paths = JsonDefinitionTopics.All.ToDictionary(t => t.Key, t => "Assets/JSON/Balance/" + t.FileName);
                if (mismatch == "file") config.jsonFiles[0].asset = config.jsonFiles[1].asset;
                else if (mismatch == "missing") config.jsonFiles = config.jsonFiles.Skip(1).ToArray();
                else config.jsonFiles[0].topic = config.jsonFiles[1].topic;
                Assert.Throws<InvalidOperationException>(() => BalanceEditorData.CheckBindings(config, JsonDefinitionTopics.All, paths));
            }
            finally { UnityEngine.Object.DestroyImmediate(config); }
        }
        [TestCase("{\"a\":1,\"a\":2}")][TestCase("{\"a\":1,}")][TestCase("[1,]")][TestCase("{\"a\":01}")][TestCase("\"\\q\"")]
        public void TokenLocatorRejectsMalformedJson(string text) => Assert.Throws<FormatException>(() => BalanceJsonToken.Parse(text));
    }
}
