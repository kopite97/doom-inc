using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "FacilityDefinition", menuName = "Mobile Game/Definitions/Facility")]
    public sealed class FacilityDefinition : ThingDefinition
    {
        [SerializeField] private Vector2Int footprint = Vector2Int.one;
        [SerializeField] private Vector2Int interactionOffset = new Vector2Int(0, -1);
        [SerializeField] private FacilityRole role;
        [SerializeField] private ResourceDefinition resource;
        [SerializeField] private int buildCost = 2;
        [SerializeField] private float buildSeconds = 3f;
        [SerializeField] private int storageCapacity = 20;
        [SerializeField] private int gatheringAmount = 5;
        [SerializeField] private float gatheringSeconds = 5f;

        public override ThingKind Kind => ThingKind.Facility;
        public Vector2Int Footprint => footprint;
        public Vector2Int InteractionOffset => interactionOffset;
        public FacilityRole Role => role;
        public ResourceDefinition Resource => resource;
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
            var hasContainer = false;
            foreach (var component in ComponentKinds)
                if (component == ThingComponentKind.ResourceContainer) hasContainer = true;
            if (!hasContainer)
                throw new ArgumentException("A gameplay facility needs a resource container.");
        }
    }
}
