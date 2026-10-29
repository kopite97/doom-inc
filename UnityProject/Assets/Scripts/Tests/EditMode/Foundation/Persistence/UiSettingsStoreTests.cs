using System;
using System.IO;
using MobileGame.Persistence;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class UiSettingsStoreTests : FoundationTestFixture
    {
        private string directory;
        private string path;

        [SetUp]
        public void PrepareSettingsDirectory()
        {
            directory = Path.Combine(Path.GetTempPath(), "DoomInc-UiSettingsTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            path = UiSettingsStore.PathForGameSave(Path.Combine(directory, "session.json"));
        }

        [TearDown]
        public void RemoveSettingsDirectory()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void MissingSettingsUseVisibleStatusAndNormalTextWithoutCreatingAFile()
        {
            var store = new UiSettingsStore(path);
            Assert.That(store.Load(), Is.True);
            AssertDefault(store);
            Assert.That(store.LastError, Is.Null);
            Assert.That(store.HasUnsavedChanges, Is.False);
            Assert.That(File.Exists(path), Is.False);
        }

        [TestCase("{\"version\":1}")]
        [TestCase("{\"version\":1,\"facilityStatus\":null,\"textSize\":null}")]
        public void MissingOrNullPreferencesFallBackToDefaults(string json)
        {
            File.WriteAllText(path, json);
            var store = new UiSettingsStore(path);
            Assert.That(store.Load(), Is.True);
            AssertDefault(store);
            Assert.That(File.ReadAllText(path), Is.EqualTo(json), "Reading defaults must not rewrite the source.");
        }

        [TestCase("")]
        [TestCase("null")]
        [TestCase("{broken}")]
        [TestCase("{\"version\":2,\"facilityStatus\":\"off\",\"textSize\":\"large\"}")]
        [TestCase("{\"version\":1,\"facilityStatus\":false,\"textSize\":\"large\"}")]
        [TestCase("{\"version\":1,\"facilityStatus\":\"invalid\",\"textSize\":\"large\"}")]
        [TestCase("{\"version\":1,\"facilityStatus\":\"off\",\"textSize\":\"invalid\"}")]
        public void InvalidSettingsUseDefaultsAndReportAnIndependentError(string json)
        {
            File.WriteAllText(path, json);
            var store = new UiSettingsStore(path);
            Assert.That(store.Load(), Is.False);
            AssertDefault(store);
            Assert.That(store.LastError, Is.Not.Null.And.Not.Empty);
            Assert.That(store.HasUnsavedChanges, Is.False);
            Assert.That(File.ReadAllText(path), Is.EqualTo(json));
        }

        [Test]
        public void EachMissingPreferenceRetainsItsOwnDefaultWithoutLosingTheOtherChoice()
        {
            File.WriteAllText(path, "{\"version\":1,\"textSize\":\"large\"}");
            var store = new UiSettingsStore(path);
            Assert.That(store.Load(), Is.True);
            Assert.That(store.Current.ShowFacilityStatus, Is.True);
            Assert.That(store.Current.LargeText, Is.True);
            File.WriteAllText(path, "{\"version\":1,\"facilityStatus\":\"off\"}");
            Assert.That(store.Load(), Is.True);
            Assert.That(store.Current.ShowFacilityStatus, Is.False);
            Assert.That(store.Current.LargeText, Is.False);
        }

        [Test]
        public void PreferencesSurviveNewStoreAndReplacePreviousFileWithoutLeavingTemporaryFiles()
        {
            var first = new UiSettingsStore(path);
            Assert.That(first.TrySet(false, true), Is.True);
            var second = new UiSettingsStore(path);
            Assert.That(second.Load(), Is.True);
            Assert.That(second.Current.ShowFacilityStatus, Is.False);
            Assert.That(second.Current.LargeText, Is.True);
            Assert.That(second.TrySet(true, false), Is.True);
            var third = new UiSettingsStore(path);
            Assert.That(third.Load(), Is.True);
            AssertDefault(third);
            Assert.That(Directory.GetFiles(directory, "*.tmp-*"), Is.Empty);
        }

        [Test]
        public void FailedWriteKeepsVisibleChoiceAndPreviousPersistedValueUntilExplicitRetrySucceeds()
        {
            string persisted = null;
            var fail = false;
            var store = new UiSettingsStore(() => persisted, json =>
            {
                if (fail) throw new IOException("Injected settings write failure.");
                persisted = json;
            });
            Assert.That(store.TrySet(true, false), Is.True);
            var previous = persisted;
            fail = true;
            Assert.That(store.TrySet(false, true), Is.False);
            Assert.That(store.Current.ShowFacilityStatus, Is.False);
            Assert.That(store.Current.LargeText, Is.True);
            Assert.That(store.HasUnsavedChanges, Is.True);
            Assert.That(store.LastError, Does.Contain("Injected settings write failure"));
            Assert.That(persisted, Is.EqualTo(previous));
            fail = false;
            Assert.That(store.TrySave(), Is.True);
            Assert.That(store.HasUnsavedChanges, Is.False);
            Assert.That(store.LastError, Is.Null);
            var restarted = new UiSettingsStore(() => persisted, _ => Assert.Fail("Loading must not write."));
            Assert.That(restarted.Load(), Is.True);
            Assert.That(restarted.Current.ShowFacilityStatus, Is.False);
            Assert.That(restarted.Current.LargeText, Is.True);
        }

        [Test]
        public void FailedFileCommitRemovesTemporaryFileAndReturnsError()
        {
            Directory.CreateDirectory(path);
            var store = new UiSettingsStore(path);
            Assert.That(store.TrySet(false, true), Is.False, "A directory cannot become a settings file.");
            Assert.That(store.Current.ShowFacilityStatus, Is.False);
            Assert.That(store.Current.LargeText, Is.True);
            Assert.That(store.HasUnsavedChanges, Is.True);
            Assert.That(store.LastError, Is.Not.Null.And.Not.Empty);
            Assert.That(Directory.Exists(path), Is.True);
            Assert.That(Directory.GetFiles(directory, "*.tmp-*"), Is.Empty);
        }

        [Test]
        public void UnreadableSettingsUseDefaultsAndDoNotTryToOverwriteTheFile()
        {
            var store = new UiSettingsStore(() => throw new UnauthorizedAccessException("Injected read denial."),
                _ => Assert.Fail("Reading must never repair or overwrite a file."));
            Assert.That(store.Load(), Is.False);
            AssertDefault(store);
            Assert.That(store.LastError, Does.Contain("Injected read denial"));
        }

        [Test]
        public void SettingsChangesDoNotTouchGameSaveSnapshotOrDefinitionHash()
        {
            var definition = CreateResourceDefinition();
            var hash = definition.Settings.CompatibilityHash;
            var codec = new SessionSnapshotCodec(new[] { definition });
            var registry = new ThingRegistry();
            var session = new SessionState(registry, new GridMap(registry, 8, 8));
            var snapshot = codec.Serialize(codec.Capture(session, SessionPhase.Playing));
            var gamePath = Path.Combine(directory, "session.json");
            File.WriteAllText(gamePath, snapshot);
            var originalBytes = File.ReadAllBytes(gamePath);
            var store = new UiSettingsStore(UiSettingsStore.PathForGameSave(gamePath));
            Assert.That(store.Load(), Is.True);
            Assert.That(store.TrySet(false, true), Is.True);
            Assert.That(File.ReadAllBytes(gamePath), Is.EqualTo(originalBytes));
            Assert.That(codec.Serialize(codec.Capture(session, SessionPhase.Playing)), Is.EqualTo(snapshot));
            Assert.That(definition.Settings.CompatibilityHash, Is.EqualTo(hash));
            Assert.That(codec.Deserialize(snapshot).dataHash, Is.EqualTo(hash));
        }

        private static void AssertDefault(UiSettingsStore store)
        {
            Assert.That(store.Current.ShowFacilityStatus, Is.True);
            Assert.That(store.Current.LargeText, Is.False);
        }
    }
}
