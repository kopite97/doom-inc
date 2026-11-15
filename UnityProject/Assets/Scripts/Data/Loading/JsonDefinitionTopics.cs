using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileGame.Foundation
{
    public sealed class JsonDefinitionTopic
    {
        public string Key { get; }
        public string FileName { get; }
        public Type DtoType { get; }
        public string Name { get; }
        public JsonDefinitionTopic(string key, string fileName, Type dtoType, string name)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(fileName) || dtoType == null
                || dtoType.IsAbstract || !typeof(DefinitionDto).IsAssignableFrom(dtoType))
                throw new ArgumentException("Invalid JSON topic registration.");
            Key = key; FileName = fileName; DtoType = dtoType; Name = name;
        }
    }

    public static class JsonDefinitionTopics
    {
        public static readonly IReadOnlyList<JsonDefinitionTopic> All = Array.AsReadOnly(new[] {
            new JsonDefinitionTopic("resources", "Resources.json", typeof(ResourceDto), "자원"),
            new JsonDefinitionTopic("creatures", "Creatures.json", typeof(CreatureDto), "생명체"),
            new JsonDefinitionTopic("facilities", "Facilities.json", typeof(FacilityDto), "시설"),
            new JsonDefinitionTopic("invasion", "Invasion.json", typeof(InvasionDto), "침공"),
            new JsonDefinitionTopic("session", "Session.json", typeof(SessionDto), "공통 설정") });
        public static JsonDefinitionTopic Get(string key) => All.Single(t => t.Key == key);
    }
}
