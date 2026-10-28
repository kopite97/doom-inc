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
        internal GameSettings(CsvRow row, CsvRow invasion, string hash)
        {
            CompatibilityHash = hash; MapWidth = row.Int("mapWidth"); MapHeight = row.Int("mapHeight");
            StartX = row.Int("startX"); StartY = row.Int("startY");
            StaffedRate = row.Number("staffedRate"); UnattendedRate = row.Number("unattendedRate"); EndingSeconds = row.Number("endingSeconds");
            if (MapWidth <= 0 || MapHeight <= 0 || StartX < 0 || StartY < 0 || StartX >= MapWidth || StartY >= MapHeight)
                throw row.Error("mapWidth/mapHeight/startX/startY", "Invalid starting map or position.");
            if (StaffedRate <= 0 || UnattendedRate <= 0 || UnattendedRate > StaffedRate || EndingSeconds <= 0)
                throw row.Error("staffedRate/unattendedRate/endingSeconds", "Expected positive rates/duration; unattended cannot exceed staffed.");
            selections = new ReadOnlyDictionary<string, string>(row.Cells.Where(p => p.Key.EndsWith("Id", StringComparison.Ordinal)
                && p.Key != "definitionId").ToDictionary(p => p.Key, p => row.Id(p.Key), StringComparer.Ordinal));
            try
            {
                Invasion = new InvasionSettings(invasion.Number("activitySeconds"), invasion.Int("smallScore"), invasion.Int("majorScore"),
                    invasion.Int("majorEvery"), invasion.Long("targetScore"), invasion.Long("detailedUntilScore"), invasion.Int("reportLimit"));
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
        private const string Common = "definitionId,displayName,components";
        public static readonly IReadOnlyDictionary<string, string> Schemas = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "resources", Common },
            { "creatures", Common + ",role,carryCapacity,moveSecondsPerCell" },
            { "facilities", Common + ",role,width,height,interactionX,interactionY,resource,buildCost,buildSeconds,storageCapacity,gatheringAmount,gatheringSeconds,inputResource,outputResource,supportOutput,invaderOutput,inputAmount,outputAmount,productionSeconds" },
            { "invasion", "definitionId,activitySeconds,smallScore,majorScore,majorEvery,targetScore,detailedUntilScore,reportLimit" },
            { "session", "definitionId,mapWidth,mapHeight,startX,startY,organicId,cultureId,gatheringId,storageId,prototypeId,processingId,cultivationId,supportId,invaderId,staffedRate,unattendedRate,endingSeconds" }
        });

        public static GameDataCatalog Compose(CsvBundle bundle, ResourceDefinitionRegistry resources,
            CreatureDefinitionRegistry creatures, FacilityDefinitionRegistry facilities)
        {
            if (resources == null || creatures == null || facilities == null) throw new ArgumentException("All three SO registries are required.");
            var assets = new Dictionary<string, ThingDefinition>(StringComparer.Ordinal);
            DefinitionRegistryValidation.Add(resources.Entries, assets);
            DefinitionRegistryValidation.Add(creatures.Entries, assets);
            DefinitionRegistryValidation.Add(facilities.Entries, assets);
            if (bundle == null || bundle.SchemaVersion != 1 || !new HashSet<string>(bundle.Tables.Keys).SetEquals(Schemas.Keys))
                throw new FormatException("Unsupported CSV schema or missing/unknown tables.");
            var tables = Schemas.ToDictionary(p => p.Key, p => CsvParser.Parse(p.Key, bundle.Tables[p.Key], p.Value));
            if (tables["invasion"].Count != 1 || tables["session"].Count != 1) throw new FormatException("Exactly one invasion and session row required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in tables.Values.SelectMany(rows => rows))
                if (!ids.Add(row.Id())) throw row.Error("definitionId", "ID repeated across tables.");
            var definitionIds = tables.Where(p => p.Key != "invasion" && p.Key != "session").SelectMany(p => p.Value).Select(r => r.Id());
            if (!new HashSet<string>(definitionIds, StringComparer.Ordinal).SetEquals(assets.Keys))
                throw new FormatException("CSV/Registry IDs differ. CSV only: " + string.Join(", ", definitionIds.Except(assets.Keys))
                    + "; Registry only: " + string.Join(", ", assets.Keys.Except(definitionIds)));
            var canonical = new StringBuilder();
            foreach (var table in tables.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                canonical.Append(CsvBundle.Token(table.Key));
                foreach (var row in table.Value.OrderBy(r => r.Id(), StringComparer.Ordinal))
                foreach (var cell in row.Cells.Where(p => p.Key != "displayName").OrderBy(p => p.Key, StringComparer.Ordinal))
                    canonical.Append(CsvBundle.Token(cell.Key)).Append(CsvBundle.Token(cell.Value.Trim()));
            }
            var settings = new GameSettings(tables["session"][0], tables["invasion"][0], CsvBundle.Hash(canonical.ToString()));
            var data = new Dictionary<string, ThingData>(StringComparer.Ordinal);
            foreach (var table in new[] { "resources", "creatures", "facilities" })
            foreach (var row in tables[table])
            {
                try
                {
                    var asset = assets[row.Id()];
                    ThingData value = table == "resources" && asset is ResourceDefinition resource ? new ResourceData(row, resource, settings)
                        : table == "creatures" && asset is CreatureDefinition creature ? new CreatureData(row, creature, settings)
                        : table == "facilities" && asset is FacilityDefinition facility ? new FacilityData(row, facility, settings, data)
                        : throw row.Error("definitionId", "Registry entry kind differs from CSV table.");
                    value.Validate(); data.Add(row.Id(), value);
                }
                catch (ArgumentException e) { throw row.Error("definition", e.Message); }
            }
            var catalog = new GameDataCatalog(data, settings, bundle.ContentHash);
            var organic = catalog.Selected<ResourceData>("organicId");
            var culture = catalog.Selected<ResourceData>("cultureId");
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
