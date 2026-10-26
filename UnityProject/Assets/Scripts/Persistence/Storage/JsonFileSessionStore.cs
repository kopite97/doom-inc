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
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                    File.Replace(temporary, path, null);
                else
                    File.Move(temporary, path);
            }
            finally
            {
                // Cleanup must not hide the original read/write error.
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
