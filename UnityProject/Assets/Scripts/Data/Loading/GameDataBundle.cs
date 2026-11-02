using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileGame.Foundation
{
    public interface IGameDataSource
    {
        Task<GameDataBundle> LoadAsync(CancellationToken cancellationToken);
    }

    public sealed class GameDataBundle
    {
        public int SchemaVersion { get; }
        public string Source { get; }
        public IReadOnlyDictionary<string, string> Tables { get; }
        public string ContentHash { get; }
        public GameDataBundle(IDictionary<string, string> tables, string source, int schemaVersion = 1)
        {
            if (tables == null || tables.Any(p => string.IsNullOrWhiteSpace(p.Key) || p.Value == null))
                throw new ArgumentException("JSON bundle contains missing names or contents.");
            Tables = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(tables, StringComparer.Ordinal));
            Source = source; SchemaVersion = schemaVersion;
            ContentHash = Hash(string.Concat(Tables.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => Token(p.Key) + Token(p.Value))));
        }
        internal static string Token(string value) => value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
        internal static string Hash(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
    }

}
