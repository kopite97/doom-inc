using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    public sealed class FacilityData : ThingData
    {
        private readonly Vector2Int footprint;
        private readonly Vector2Int interactionOffset;
        private readonly FacilityRole role;
        private readonly ResourceData resource;
        private readonly int buildCost;
        private readonly float buildSeconds;
        private readonly int storageCapacity;
        private readonly int gatheringAmount;
        private readonly float gatheringSeconds;

        private readonly ResourceData inputResource;
        private readonly ResourceData outputResource;
        private readonly CreatureData supportOutput;
        private readonly CreatureData invaderOutput;
        private readonly int inputAmount;
        private readonly int outputAmount;
        private readonly float productionSeconds;
        public ResourceData InputResource => inputResource;
        public ResourceData OutputResource => outputResource;
        public CreatureData SupportOutput => supportOutput;
        public CreatureData InvaderOutput => invaderOutput;
        public int InputAmount => inputAmount;
        public int OutputAmount => outputAmount;
        public double ProductionSeconds => productionSeconds;
        public bool HasProduction => role == FacilityRole.Processing || role == FacilityRole.Cultivation;

        private FacilityData() { }
        public FacilityData(CsvRow row, FacilityDefinition asset, GameSettings settings, System.Collections.Generic.IReadOnlyDictionary<string, ThingData> definitions) : base(row, asset, settings)
        {
            footprint = new Vector2Int(row.Int("width"), row.Int("height"));
            interactionOffset = new Vector2Int(row.Int("interactionX"), row.Int("interactionY"));
            role = row.EnumValue<FacilityRole>("role");
            resource = Resolve<ResourceData>(row, "resource", definitions);
            buildCost = row.Int("buildCost"); buildSeconds = row.Float("buildSeconds");
            storageCapacity = row.Int("storageCapacity"); gatheringAmount = row.Int("gatheringAmount"); gatheringSeconds = row.Float("gatheringSeconds");
            inputResource = Resolve<ResourceData>(row, "inputResource", definitions);
            outputResource = Resolve<ResourceData>(row, "outputResource", definitions);
            supportOutput = Resolve<CreatureData>(row, "supportOutput", definitions);
            invaderOutput = Resolve<CreatureData>(row, "invaderOutput", definitions);
            inputAmount = row.Int("inputAmount"); outputAmount = row.Int("outputAmount"); productionSeconds = row.Float("productionSeconds");
        }
        private static T Resolve<T>(CsvRow row, string column, System.Collections.Generic.IReadOnlyDictionary<string, ThingData> definitions) where T : ThingData
        {
            var id = row.Text(column); if (id.Length == 0) return null;
            if (!definitions.TryGetValue(id, out var value) || !(value is T result)) throw row.Error(column, "Missing or wrong-kind reference: " + id);
            return result;
        }

        public override ThingKind Kind => ThingKind.Facility;
        public Vector2Int Footprint => footprint;
        public Vector2Int InteractionOffset => interactionOffset;
        public FacilityRole Role => role;
        public ResourceData Resource => resource;
        public int BuildCost => buildCost;
        public double BuildSeconds => buildSeconds;
        public int StorageCapacity => storageCapacity;
        public int GatheringAmount => gatheringAmount;
        public double GatheringSeconds => gatheringSeconds;

        public override void Validate()
        {
            base.Validate();
            if (footprint.x <= 0 || footprint.y <= 0)
                throw new ArgumentException("Facility footprint dimensions must be positive.");
            // A fixed cell on one exterior edge, expressed in the unrotated local grid.
            var onVerticalEdge = (interactionOffset.x == -1 || interactionOffset.x == footprint.x)
                && interactionOffset.y >= 0 && interactionOffset.y < footprint.y;
            var onHorizontalEdge = (interactionOffset.y == -1 || interactionOffset.y == footprint.y)
                && interactionOffset.x >= 0 && interactionOffset.x < footprint.x;
            if (!onVerticalEdge && !onHorizontalEdge)
                throw new ArgumentException("The interaction cell must adjoin one footprint edge.");

            if (!Enum.IsDefined(typeof(FacilityRole), role))
                throw new ArgumentException("Facility role is unknown.");
            if (role == FacilityRole.None) return;
            if (resource == null)
                throw new ArgumentException("A gameplay facility needs its construction/output resource definition.");
            resource.Validate();
            if (buildCost <= 0 || buildSeconds <= 0 || float.IsNaN(buildSeconds) || float.IsInfinity(buildSeconds))
                throw new ArgumentException("Construction cost and finite work duration must be positive.");
            if (role == FacilityRole.Storage && storageCapacity <= 0)
                throw new ArgumentException("Storage capacity must be positive.");
            if (role == FacilityRole.Gathering && (gatheringAmount <= 0 || gatheringSeconds <= 0
                || float.IsNaN(gatheringSeconds) || float.IsInfinity(gatheringSeconds)))
                throw new ArgumentException("Gathering output and finite work duration must be positive.");
            if (HasProduction)
            {
                if (inputResource == null || inputAmount <= 0 || outputAmount != 1 || productionSeconds <= 0
                    || float.IsNaN(productionSeconds) || float.IsInfinity(productionSeconds))
                    throw new ArgumentException("Production requires input, a single output and finite positive work.");
                inputResource.Validate();
                if (role == FacilityRole.Processing && (outputResource == null || outputResource == inputResource))
                    throw new ArgumentException("Processing needs a distinct output resource.");
                if (role == FacilityRole.Cultivation && (supportOutput == null || invaderOutput == null ||
                    supportOutput.Role != CreatureRole.Support || invaderOutput.Role != CreatureRole.Invader))
                    throw new ArgumentException("Cultivation needs support and invader definitions.");
                if (role == FacilityRole.Processing) outputResource.Validate();
                else { supportOutput.Validate(); invaderOutput.Validate(); }
            }
            var hasContainer = false;
            foreach (var component in ComponentKinds)
                if (component == ThingComponentKind.ResourceContainer) hasContainer = true;
            if (!hasContainer)
                throw new ArgumentException("A gameplay facility needs a resource container.");
        }
    }
}
