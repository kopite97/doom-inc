using System;
using System.IO;
using MobileGame.Persistence;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class LanguageSettingsTests
    {
        [TestCase("broken")]
        [TestCase("{}")]
        [TestCase("{\"version\":2,\"language\":\"en\"}")]
        public void CorruptPreferencesDoNotOverwriteTheSource(string source)
        {
            var writes = 0; var settings = new LanguageSettingsStore(() => source, _ => writes++);
            settings.Load(); Assert.That(settings.Language, Is.Null); Assert.That(settings.LastError, Is.Not.Empty); Assert.That(writes, Is.Zero);
        }
        [Test]
        public void FailedWriteRetainsSelectionAndRetryPersistsIt()
        {
            string file = null; var fail = true;
            var settings = new LanguageSettingsStore(() => file, text => { if (fail) throw new IOException("read-only fixture"); file = text; });
            Assert.That(settings.TrySet("en"), Is.False); Assert.That(settings.Language, Is.EqualTo("en")); Assert.That(settings.HasUnsavedChanges, Is.True);
            fail = false; Assert.That(settings.TrySave(), Is.True); Assert.That(settings.LastError, Is.Null);
            var restarted = new LanguageSettingsStore(() => file, _ => Assert.Fail("Load must be read-only.")); restarted.Load();
            Assert.That(restarted.Language, Is.EqualTo("en")); Assert.That(restarted.HasUnsavedChanges, Is.False);
        }
        [Test]
        public void ReadAccessFailureIsDifferentFromMissingPreferences()
        {
            var denied = new LanguageSettingsStore(() => throw new UnauthorizedAccessException("fixture"), _ => { }); denied.Load();
            Assert.That(denied.LastError, Is.Not.Empty);
            var absent = new LanguageSettingsStore(() => null, _ => { }); absent.Load(); Assert.That(absent.LastError, Is.Null);
        }
    }
}
