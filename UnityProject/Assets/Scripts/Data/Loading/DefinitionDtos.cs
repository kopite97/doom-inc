using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MobileGame.Foundation
{
    // Input-only DTOs; the catalog copies values and never publishes these mutable inputs.
    public abstract class DefinitionDto
    {
        public string definitionId;
        internal string Location;
        internal readonly Dictionary<string, string> NumericText = new Dictionary<string, string>(StringComparer.Ordinal);
        public string Id() => definitionId;
        public FormatException Error(string field, string reason) => new FormatException(Location + "." + field + ", ID '" + definitionId + "': " + reason);
        internal IEnumerable<KeyValuePair<string, string>> CompatibilityFields()
        {
            foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                if (field.Name == "displayName") continue;
                var value = field.GetValue(this);
                yield return new KeyValuePair<string,string>(field.Name, NumericText.TryGetValue(field.Name, out var number) ? number
                    : value is ThingComponentKind[] components ? string.Join("|", components.Select(c => c.ToString()))
                    : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
            }
        }
    }
    public abstract class ThingDefinitionDto : DefinitionDto
    {
        public string displayName;
        public ThingComponentKind[] components;
    }
    public sealed class ResourceDto : ThingDefinitionDto
    {
    }
    public sealed class CreatureDto : ThingDefinitionDto
    {
        public CreatureRole role;
        public int carryCapacity;
        public double moveSecondsPerCell;
    }
    public sealed class FacilityDto : ThingDefinitionDto
    {
        public FacilityRole role;
        public int width;
        public int height;
        public int interactionX;
        public int interactionY;
        public string resource = "";
        public int buildCost;
        public double buildSeconds;
        public int storageCapacity;
        public int gatheringAmount;
        public double gatheringSeconds;
        public string inputResource = "";
        public string outputResource = "";
        public string supportOutput = "";
        public string invaderOutput = "";
        public int inputAmount;
        public int outputAmount;
        public double productionSeconds;
    }
    public sealed class InvasionDto : DefinitionDto
    {
        public double activitySeconds;
        public int smallScore;
        public int majorScore;
        public int majorEvery;
        public long targetScore;
        public long detailedUntilScore;
        public int reportLimit;
    }
    public sealed class SessionDto : DefinitionDto
    {
        public int mapWidth;
        public int mapHeight;
        public int startX;
        public int startY;
        public string organicId = "";
        public string cultureId = "";
        public string gatheringId = "";
        public string storageId = "";
        public string prototypeId = "";
        public string processingId = "";
        public string cultivationId = "";
        public string supportId = "";
        public string invaderId = "";
        public double staffedRate;
        public double unattendedRate;
        public double endingSeconds;
    }
}
