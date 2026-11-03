using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MobileGame.Foundation
{
    public sealed class GameSettings
    {
        public string CompatibilityHash { get; }
        public int MapWidth { get; }
        public int MapHeight { get; }
        public int StartX { get; }
        public int StartY { get; }
        public double StaffedRate { get; }
        public double UnattendedRate { get; }
        public double EndingSeconds { get; }
        public InvasionSettings Invasion { get; }
        private readonly IReadOnlyDictionary<string, string> selections;
        public string Selection(string key) => selections[key];
        internal GameSettings(SessionDto row, InvasionDto invasion, string hash)
        {
            CompatibilityHash = hash; MapWidth = row.mapWidth; MapHeight = row.mapHeight;
            StartX = row.startX; StartY = row.startY;
            StaffedRate = row.staffedRate; UnattendedRate = row.unattendedRate; EndingSeconds = row.endingSeconds;
            if (MapWidth <= 0 || MapHeight <= 0 || StartX < 0 || StartY < 0 || StartX >= MapWidth || StartY >= MapHeight)
                throw row.Error("mapWidth/mapHeight/startX/startY", "Invalid starting map or position.");
            if (StaffedRate <= 0 || UnattendedRate <= 0 || UnattendedRate > StaffedRate || EndingSeconds <= 0)
                throw row.Error("staffedRate/unattendedRate/endingSeconds", "Expected positive rates/duration; unattended cannot exceed staffed.");
            selections = new ReadOnlyDictionary<string, string>(typeof(SessionDto).GetFields().Where(f => f.Name.EndsWith("Id", StringComparison.Ordinal) && f.Name != "definitionId")
                .ToDictionary(f => f.Name, f => (string)f.GetValue(row), StringComparer.Ordinal));
            try
            {
                Invasion = new InvasionSettings(invasion.activitySeconds, invasion.smallScore, invasion.majorScore,
                    invasion.majorEvery, invasion.targetScore, invasion.detailedUntilScore, invasion.reportLimit);
            }
            catch (ArgumentException e) { throw invasion.Error("settings", e.Message); }
        }
    }

    public sealed class GameDataCatalog
    {
        public IReadOnlyDictionary<string, ThingData> Definitions { get; }
        public GameSettings Settings { get; }
        public string SourceHash { get; }
        internal GameDataCatalog(IDictionary<string, ThingData> definitions, GameSettings settings, string sourceHash)
        { Definitions = new ReadOnlyDictionary<string, ThingData>(new Dictionary<string, ThingData>(definitions, StringComparer.Ordinal)); Settings = settings; SourceHash = sourceHash; }
        public T Get<T>(string id) where T : ThingData => Definitions.TryGetValue(id, out var value) && value is T typed
            ? typed : throw new ArgumentException("Missing or wrong-kind Data: " + id);
        public T Selected<T>(string key) where T : ThingData => Get<T>(Settings.Selection(key));
    }

    public static class GameDataComposer
    {
        public static GameDataCatalog Compose(GameDataBundle bundle, ResourceDefinitionRegistry resources,
            CreatureDefinitionRegistry creatures, FacilityDefinitionRegistry facilities)
        {
            if (resources == null || creatures == null || facilities == null) throw new ArgumentException("All three SO registries are required.");
            var assets = new Dictionary<string, ThingDefinition>(StringComparer.Ordinal);
            DefinitionRegistryValidation.Add(resources.Entries, assets);
            DefinitionRegistryValidation.Add(creatures.Entries, assets);
            DefinitionRegistryValidation.Add(facilities.Entries, assets);
            if (bundle == null || bundle.SchemaVersion != 1 || !new HashSet<string>(bundle.Tables.Keys).SetEquals(JsonDefinitionParser.Topics))
                throw new FormatException("Unsupported JSON schema or missing/unknown tables.");
            var tables = JsonDefinitionParser.Parse(bundle);
            if (tables["invasion"].Count != 1 || tables["session"].Count != 1) throw new FormatException("Exactly one invasion and session row required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in tables.Values.SelectMany(rows => rows))
                if (!ids.Add(row.Id())) throw row.Error("definitionId", "ID repeated across tables.");
            var definitionIds = tables.Where(p => p.Key != "invasion" && p.Key != "session").SelectMany(p => p.Value).Select(r => r.Id());
            if (!new HashSet<string>(definitionIds, StringComparer.Ordinal).SetEquals(assets.Keys))
                throw new FormatException("JSON/Registry IDs differ. JSON only: " + string.Join(", ", definitionIds.Except(assets.Keys))
                    + "; Registry only: " + string.Join(", ", assets.Keys.Except(definitionIds)));
            var canonical = new StringBuilder();
            foreach (var table in tables.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                canonical.Append(GameDataBundle.Token(table.Key));
                foreach (var row in table.Value.OrderBy(r => r.Id(), StringComparer.Ordinal))
                foreach (var cell in row.CompatibilityFields())
                    canonical.Append(GameDataBundle.Token(cell.Key)).Append(GameDataBundle.Token(cell.Value.Trim()));
            }
            var settings = new GameSettings((SessionDto)tables["session"][0], (InvasionDto)tables["invasion"][0], GameDataBundle.Hash(canonical.ToString()));
            var data = new Dictionary<string, ThingData>(StringComparer.Ordinal);
            foreach (var table in new[] { "resources", "creatures", "facilities" })
            foreach (var row in tables[table])
            {
                try
                {
                    var asset = assets[row.Id()];
                    ThingData value = table == "resources" && asset is ResourceDefinition resource ? new ResourceData((ResourceDto)row, resource, settings)
                        : table == "creatures" && asset is CreatureDefinition creature ? new CreatureData((CreatureDto)row, creature, settings)
                        : table == "facilities" && asset is FacilityDefinition facility ? new FacilityData((FacilityDto)row, facility, settings, data)
                        : throw row.Error("definitionId", "Registry entry kind differs from JSON table.");
                    value.Validate(); data.Add(row.Id(), value);
                }
                catch (Exception e) when (e is ArgumentException || e is InvalidOperationException || e is OverflowException)
                { throw row.Error("definition", e.Message); }
            }
            var catalog = new GameDataCatalog(data, settings, bundle.ContentHash);
            var organic = catalog.Selected<ResourceData>("organicId");
            var culture = catalog.Selected<ResourceData>("cultureId");
            foreach (var model in data.Values.OfType<CreatureData>().Where(model => model.ActivityBehavior == "Raider"))
                if (model.SupplyResource != organic.DefinitionId && model.SupplyResource != culture.DefinitionId)
                    throw new FormatException("Unsupported supply resource: " + model.SupplyResource);
            var gather = catalog.Selected<FacilityData>("gatheringId"); var storage = catalog.Selected<FacilityData>("storageId");
            var prototype = catalog.Selected<CreatureData>("prototypeId");
            if (organic == culture || gather.Role != FacilityRole.Gathering || storage.Role != FacilityRole.Storage
                || gather.Resource != organic || storage.Resource != organic || prototype.Role != CreatureRole.Prototype
                || checked(gather.BuildCost + storage.BuildCost) > prototype.CarryCapacity)
                throw new FormatException("session: incompatible starting definitions or starting carry capacity.");
            if (catalog.Selected<FacilityData>("processingId").Role != FacilityRole.Processing
                || catalog.Selected<FacilityData>("cultivationId").Role != FacilityRole.Cultivation
                || catalog.Selected<CreatureData>("supportId").Role != CreatureRole.Support
                || catalog.Selected<CreatureData>("invaderId").Role != CreatureRole.Invader)
                throw new FormatException("session: selected definition roles do not match.");
            foreach (var f in data.Values.OfType<FacilityData>())
                if (f.Footprint.x > settings.MapWidth || f.Footprint.y > settings.MapHeight)
                    throw new FormatException("session: facility exceeds starting map: " + f.DefinitionId);
            return catalog;
        }
    }
}
