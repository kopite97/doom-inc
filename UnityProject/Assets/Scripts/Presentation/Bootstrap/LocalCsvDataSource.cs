using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Unity access is confined to this adapter. The transport contract returns plain text.
    public sealed class LocalCsvDataSource : ICsvDataSource
    {
        private readonly TextAsset[] tables;
        public LocalCsvDataSource(TextAsset[] tables) => this.tables = tables == null ? throw new ArgumentNullException(nameof(tables)) : (TextAsset[])tables.Clone();
        public Task<CsvBundle> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var contents = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var asset in tables)
            {
                if (asset == null) throw new ArgumentException("Missing local CSV TextAsset.");
                if (contents.ContainsKey(asset.name)) throw new ArgumentException("Duplicate local CSV table: " + asset.name);
                contents.Add(asset.name, asset.text);
            }
            return Task.FromResult(new CsvBundle(contents, "packaged-local"));
        }
    }
}
