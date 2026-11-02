using System;
using System.IO;

namespace MobileGame.Persistence
{
    public enum SaveFileState { Missing, Readable, Unreadable }

    // Readable says nothing about snapshot validity. Only the codec can validate a saved session.
    public readonly struct SaveFileProbe
    {
        public SaveFileState State { get; }
        public string Error { get; }
        private SaveFileProbe(SaveFileState state, string error = null) { State = state; Error = error; }

        public static SaveFileProbe Read(string path) => Read(() => new JsonFileSessionStore(path).Read());

        public static SaveFileProbe Read(Func<string> read)
        {
            if (read == null) throw new ArgumentNullException(nameof(read));
            try { read(); return new SaveFileProbe(SaveFileState.Readable); }
            catch (FileNotFoundException) { return new SaveFileProbe(SaveFileState.Missing); }
            catch (DirectoryNotFoundException) { return new SaveFileProbe(SaveFileState.Missing); }
            catch (Exception error) { return new SaveFileProbe(SaveFileState.Unreadable, error.Message); }
        }
    }
}
