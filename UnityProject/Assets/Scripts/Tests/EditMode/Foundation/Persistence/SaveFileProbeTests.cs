using System;
using System.IO;
using MobileGame.Persistence;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class SaveFileProbeTests
    {
        private string directory;
        private string path;
        [SetUp] public void Prepare()
        {
            directory = Path.Combine(Path.GetTempPath(), "PLAN017-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); path = Path.Combine(directory, "session.json");
        }
        [TearDown] public void Cleanup() { Directory.Delete(directory, true); }

        [TestCase("")]
        [TestCase("broken saved session")]
        [TestCase("null")]
        public void ReadableDoesNotPromiseValidSnapshotAndNeverChangesBytes(string contents)
        {
            Assert.That(SaveFileProbe.Read(path).State, Is.EqualTo(SaveFileState.Missing));
            File.WriteAllText(path, contents);
            Assert.That(SaveFileProbe.Read(path).State, Is.EqualTo(SaveFileState.Readable));
            Assert.That(File.ReadAllText(path), Is.EqualTo(contents));
            Assert.That(Directory.GetFiles(directory), Has.Length.EqualTo(1));
        }
        [Test] public void ProbeDistinguishesMissingFromDeniedAndDoesNotCacheEarlierResult()
        {
            Assert.That(SaveFileProbe.Read(() => throw new FileNotFoundException()).State, Is.EqualTo(SaveFileState.Missing));
            Assert.That(SaveFileProbe.Read(() => throw new DirectoryNotFoundException()).State, Is.EqualTo(SaveFileState.Missing));
            var denied = SaveFileProbe.Read(() => throw new UnauthorizedAccessException("denied"));
            Assert.That(denied.State, Is.EqualTo(SaveFileState.Unreadable));
            Assert.That(denied.Error, Is.EqualTo("denied"));
            File.WriteAllText(path, "first");
            Assert.That(SaveFileProbe.Read(path).State, Is.EqualTo(SaveFileState.Readable));
            File.Delete(path); Directory.CreateDirectory(path);
            Assert.That(SaveFileProbe.Read(path).State, Is.EqualTo(SaveFileState.Unreadable));
        }
        [Test] public void FailedReplacementPreservesExistingBytesAndCleansTemporaryFile()
        {
            var store = new JsonFileSessionStore(path);
            store.Write("previous 저장");
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                Assert.That(() => store.Write("replacement"), Throws.Exception);
            Assert.That(store.Read(), Is.EqualTo("previous 저장"));
            Assert.That(Directory.GetFiles(directory, "*.tmp-*"), Is.Empty);
            store.Write("retry 저장");
            Assert.That(store.Read(), Is.EqualTo("retry 저장"));
        }
    }
}
