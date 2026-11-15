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
                if (field.Name == "displayNameKey") continue;
                var value = field.GetValue(this);
                yield return new KeyValuePair<string,string>(field.Name, NumericText.TryGetValue(field.Name, out var number) ? number
                    : value is ThingComponentKind[] components ? string.Join("|", components.Select(c => c.ToString()))
                    : value is string[] ids ? string.Concat(ids.Select(GameDataBundle.Token))
                    : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
            }
        }
    }
    public abstract class ThingDefinitionDto : DefinitionDto
    {
        public string displayNameKey;
        public ThingComponentKind[] components;
    }
    public sealed class ResourceDto : ThingDefinitionDto
    {
    }
    public sealed class CreatureDto : ThingDefinitionDto
    {
        public CreatureRole role;
        public string activityBehavior;
        [BalanceField("일반 성과", "약탈형의 일반 활동 성과 점수", Unit = "점", Order = 2, ConditionField = nameof(activityBehavior), ConditionValues = new[] { "Raider" })]
        public int smallScore;
        [BalanceField("주요 성과", "약탈형의 주요 활동 성과 점수", Unit = "점", Order = 3, ConditionField = nameof(activityBehavior), ConditionValues = new[] { "Raider" })]
        public int majorScore;
        [BalanceField("보급 수량", "약탈형의 회당 보급 자원 수량", Unit = "개", Order = 4, ConditionField = nameof(activityBehavior), ConditionValues = new[] { "Raider" })]
        public int supplyAmount;
        [BalanceField("보급 주기", "약탈형이 자원을 보급하는 활동 횟수 간격", Unit = "회", Order = 5, ConditionField = nameof(activityBehavior), ConditionValues = new[] { "Raider" })]
        public int supplyEvery;
        public string supplyResource;
        [BalanceField("운반 용량", "한 번에 운반할 수 있는 자원 수량", Unit = "개", Order = 0)]
        public int carryCapacity;
        [BalanceField("이동 시간", "논리적인 한 칸 이동 시간. 애니메이션 재생 속도가 아닙니다.", Unit = "초/칸", Order = 1)]
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
        [BalanceField("건설 비용", "시설 건설에 소비하는 자원 수량", Unit = "개", Order = 0)]
        public int buildCost;
        [BalanceField("건설 시간", "시설을 완공하는 데 필요한 작업 시간", Unit = "초", Order = 1)]
        public double buildSeconds;
        [BalanceField("저장 용량", "저장소가 보관할 수 있는 자원 수량", Unit = "개", Order = 2, ConditionField = nameof(role), ConditionValues = new[] { "Storage" })]
        public int storageCapacity;
        [BalanceField("획득량", "획득 작업 한 회차의 산출량", Unit = "개", Order = 3, ConditionField = nameof(role), ConditionValues = new[] { "Gathering" })]
        public int gatheringAmount;
        [BalanceField("획득 시간", "획득 작업 한 회차의 기준 시간", Unit = "초", Order = 4, ConditionField = nameof(role), ConditionValues = new[] { "Gathering" })]
        public double gatheringSeconds;
        public string inputResource = "";
        public string outputResource = "";
        public string[] cultivationOutputs;
        public string supportOutput = "";
        public string invaderOutput = "";
        [BalanceField("투입량", "생산 한 회차에 소비하는 재료 수량", Unit = "개", Order = 5, ConditionField = nameof(role), ConditionValues = new[] { "Processing", "Cultivation" })]
        public int inputAmount;
        [BalanceField("산출량", "생산 한 회차의 산출 수량", Unit = "개", Order = 6, ConditionField = nameof(role), ConditionValues = new[] { "Processing", "Cultivation" })]
        public int outputAmount;
        [BalanceField("생산 시간", "생산 한 회차에 필요한 기준 작업 시간", Unit = "초", Order = 7, ConditionField = nameof(role), ConditionValues = new[] { "Processing", "Cultivation" })]
        public double productionSeconds;
    }
    public sealed class InvasionDto : DefinitionDto
    {
        [BalanceField("활동 주기", "외부 활동 한 회차의 시간", Unit = "초", Order = 0)]
        public double activitySeconds;
        [BalanceField("일반 성과", "일반 침공형의 일반 활동 성과", Unit = "점", Order = 1)]
        public int smallScore;
        [BalanceField("주요 성과", "일반 침공형의 주요 활동 성과", Unit = "점", Order = 2)]
        public int majorScore;
        [BalanceField("주요 성과 주기", "주요 성과가 발생하는 활동 횟수 간격", Unit = "회", Order = 3)]
        public int majorEvery;
        [BalanceField("목표 점수", "한 판의 목표 누적 성과 점수", Unit = "점", Order = 4)]
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
        [BalanceField("작업자 생산 배율", "작업자가 시설에서 작업할 때의 생산 속도", Unit = "배", Order = 0)]
        public double staffedRate;
        [BalanceField("무인 생산 배율", "가공·배양시설에 작업자가 없을 때의 생산 속도", Unit = "배", Order = 1)]
        public double unattendedRate;
        public double endingSeconds;
    }
}
