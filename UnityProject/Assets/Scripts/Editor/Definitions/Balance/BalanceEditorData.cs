using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;

namespace MobileGame.Foundation.Editor
{
    public static class BalanceEditorData
    {
        public const string ConfigurationPath = "Assets/SO/Application/AppConfiguration.asset";
        public static AppConfiguration Configuration => AssetDatabase.LoadAssetAtPath<AppConfiguration>(ConfigurationPath)
            ?? throw new InvalidOperationException("실행 AppConfiguration이 없습니다.");
        public static void CheckBindings(AppConfiguration config, IReadOnlyList<JsonDefinitionTopic> topics, IReadOnlyDictionary<string, string> expectedPaths)
        {
            if (config == null || config.jsonFiles == null || config.jsonFiles.Length != topics.Count) throw new InvalidOperationException("공통 등록과 실행 jsonFiles 개수가 다릅니다.");
            foreach (var topic in topics)
            {
                var bindings = config.jsonFiles.Where(f => f != null && f.topic == topic.Key).ToArray();
                if (bindings.Length != 1 || bindings[0].asset == null) throw new InvalidOperationException(topic.Key + ": 실행 파일 누락/중복");
                if (!expectedPaths.TryGetValue(topic.Key, out var expected) || Path.GetFullPath(AssetDatabase.GetAssetPath(bindings[0].asset)) != Path.GetFullPath(expected))
                    throw new InvalidOperationException(topic.Key + ": 편집 파일과 실행 파일 연결이 다릅니다.");
            }
        }
        public static BalanceDocumentStore Load()
        {
            var documents = JsonDefinitionTopics.All.Select(t => new BalanceDocument(t, "Assets/JSON/Balance/" + t.FileName)).ToArray();
            var paths = documents.ToDictionary(d => d.Topic.Key, d => d.Path);
            return new BalanceDocumentStore(documents, "Library/BalanceEditor/Recovery", texts =>
            {
                var config = Configuration; CheckBindings(config, JsonDefinitionTopics.All, paths);
                GameDataComposer.Compose(new GameDataBundle(new Dictionary<string, string>(texts), "balance-editor"), config.resources, config.creatures, config.facilities);
            }, () =>
            {
                foreach (var topic in JsonDefinitionTopics.All) AssetDatabase.ImportAsset("Assets/JSON/Balance/" + topic.FileName, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            });
        }
        public static void CheckCurrentBindings()
        { CheckBindings(Configuration, JsonDefinitionTopics.All, JsonDefinitionTopics.All.ToDictionary(t => t.Key, t => "Assets/JSON/Balance/" + t.FileName)); }
    }
}
