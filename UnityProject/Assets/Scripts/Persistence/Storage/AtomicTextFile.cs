using System;
using System.IO;
using System.Security;
using System.Text;

namespace MobileGame.Persistence
{
    // Replace only after the complete file has been flushed. Never delete the old destination first.
    public static class AtomicTextFile
    {
        public static void Write(string path, string text)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(text);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally
            {
                // Best-effort cleanup cannot turn a successful commit into a reported failure,
                // or hide the original exception when the commit failed.
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }
            }
        }
    }
}
