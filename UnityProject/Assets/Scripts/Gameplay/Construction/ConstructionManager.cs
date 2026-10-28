using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    public sealed class ConstructionManager
    {
        private readonly LaboratoryManager laboratory;
        public ConstructionManager(LaboratoryManager laboratory) { this.laboratory = laboratory; }
        private ThingRegistry Registry => laboratory.Registry;
        private GridMap Map => laboratory.Map;
        private ResourceDefinition Organic => laboratory.Organic;
        public bool IsEditing => ConstructionDefinition != null;
        public FacilityDefinition ConstructionDefinition { get; private set; }
        public Vector2Int ConstructionOrigin { get; private set; }
        public QuarterTurn ConstructionRotation { get; private set; }
        public PlacementResult ConstructionValidation { get; private set; }
        public string LastError { get; private set; }
        public bool BeginConstruction(FacilityDefinition definition)
        {
            if (IsEditing || Map.Commands.IsEditing) return Reject("Finish or cancel the current edit first.");
            if (definition == null || definition.Role == FacilityRole.None ||
                !ReferenceEquals(definition.Resource, Organic))
                return Reject("Choose a laboratory facility definition.");
            try { definition.Validate(); }
            catch (ArgumentException error) { return Reject(error.Message); }
            catch (InvalidOperationException error) { return Reject(error.Message); }
            if (definition.HasProduction && (!laboratory.Supports(definition.InputResource) ||
                (definition.Role == FacilityRole.Processing && !laboratory.Supports(definition.OutputResource))))
                return Reject("The laboratory does not support this recipe's resources.");
            ConstructionDefinition = definition;
            ConstructionOrigin = Vector2Int.zero;
            ConstructionRotation = QuarterTurn.None;
            ConstructionValidation = Map.CheckPlacement(definition, ConstructionOrigin, ConstructionRotation);
            LastError = null;
            return true;
        }

        public bool PreviewConstruction(Vector2Int origin, QuarterTurn rotation)
        {
            if (!IsEditing) return Reject("No construction preview is active.");
            ConstructionOrigin = origin;
            ConstructionRotation = rotation;
            ConstructionValidation = Map.CheckPlacement(ConstructionDefinition, origin, rotation);
            LastError = ConstructionValidation.Succeeded ? null : ConstructionValidation.ToString();
            return ConstructionValidation.Succeeded;
        }

        public bool ConfirmConstruction()
        {
            if (!IsEditing || Map.Commands.IsEditing) return Reject("No exclusive construction preview is active.");
            ConstructionValidation = Map.CheckPlacement(ConstructionDefinition, ConstructionOrigin, ConstructionRotation);
            if (!ConstructionValidation.Succeeded) return Reject(ConstructionValidation.ToString());
            // No Thing, creation order, reservation or cost exists before this commit.
            var site = Registry.Create(ConstructionDefinition);
            var result = Map.Commands.Place(site.Id, ConstructionOrigin, ConstructionRotation);
            if (!result.Succeeded)
            {
                Registry.Remove(site);
                return Reject(result.ToString());
            }
            ConstructionDefinition = null;
            LastError = null;
            laboratory.OnMapChanged();
            return true;
        }

        public bool CancelConstruction()
        {
            if (!IsEditing) return Reject("No construction preview is active.");
            ConstructionDefinition = null;
            LastError = null;
            return true;
        }

        private bool Reject(string message) { LastError = message; return false; }
    }
}
