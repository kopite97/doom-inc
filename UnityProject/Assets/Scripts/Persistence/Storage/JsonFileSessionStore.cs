using System;
using System.IO;
using System.Text;

namespace MobileGame.Persistence
{
    // One logical slot. A failed write never falls back to deleting the previous save.
    public sealed class JsonFileSessionStore : ISessionStore
    {
        private readonly string path;

        public JsonFileSessionStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A save path is required.", nameof(path));
            this.path = Path.GetFullPath(path);
        }

        public bool Exists => File.Exists(path);
        public string Read() => File.ReadAllText(path, Encoding.UTF8);

        public void Write(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("A serialized snapshot is required.", nameof(json));
            AtomicTextFile.Write(path, json);
        }
    }
}
