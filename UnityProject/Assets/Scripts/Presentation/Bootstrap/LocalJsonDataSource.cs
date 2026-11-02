using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    [Serializable] public sealed class JsonDataAsset
    {
        public string topic;
        public TextAsset asset;
        public JsonDataAsset(string topic, TextAsset asset) { this.topic = topic; this.asset = asset; }
    }
    public sealed class LocalJsonDataSource : IGameDataSource
    {
        private readonly JsonDataAsset[] files;
        public LocalJsonDataSource(JsonDataAsset[] files) => this.files = files == null ? throw new ArgumentNullException(nameof(files))
            : Array.ConvertAll(files, f => f == null ? null : new JsonDataAsset(f.topic, f.asset));
        public Task<GameDataBundle> LoadAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var contents = new Dictionary<string,string>(StringComparer.Ordinal);
            foreach (var file in files)
            {
                token.ThrowIfCancellationRequested();
                if (file == null || string.IsNullOrWhiteSpace(file.topic) || file.asset == null) throw new ArgumentException("Missing local JSON topic/TextAsset.");
                if (contents.ContainsKey(file.topic)) throw new ArgumentException("Duplicate local JSON topic: " + file.topic);
                contents.Add(file.topic, file.asset.text);
            }
            return Task.FromResult(new GameDataBundle(contents, "packaged-local"));
        }
    }
}
