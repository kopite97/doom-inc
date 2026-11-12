using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public sealed class ProductionManager
    {
        private readonly LaboratoryManager laboratory;
        public ProductionManager(LaboratoryManager laboratory) { this.laboratory = laboratory; }
        public string LastError { get; private set; }

        public bool Select(Guid facilityId, string target)
        {
            if (!laboratory.Registry.TryGet(facilityId, out var facility) || !laboratory.IsComplete(facility) ||
                !(facility.Definition is FacilityData definition) || definition.FindOutput(target) == null)
            { LastError = "Select a completed cultivator and a support or invader target."; return false; }
            if (facility.GetComponent<ProductionComponent>().SelectedTarget != target) laboratory.WorkChanged();
            facility.GetComponent<ProductionComponent>().SelectedTarget = target;
            LastError = null;
            return true;
        }

        public int InputCount(Thing facility) => facility.GetComponent<ResourceContainerComponent>().Contents
            .Where(t => t.Definition == ((FacilityData)facility.Definition).InputResource)
            .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);

        public int InputNeeded(Thing facility)
        {
            if (!laboratory.IsComplete(facility) || !facility.TryGetComponent<ProductionComponent>(out var state)) return 0;
            var definition = (FacilityData)facility.Definition;
            if (state.InProgress || state.Outstanding > 0 ||
                (definition.Role == FacilityRole.Cultivation && state.SelectedTarget == CultivationTarget.None)) return 0;
            return Math.Max(0, definition.InputAmount - InputCount(facility));
        }

        // Reachability uses real support workers and the same Map/pathfinder. A blocked
        // interaction cell cannot produce, even with nobody assigned to the facility.
        public bool Accessible(Thing facility) => facility.IsPlaced && laboratory.Registry.Things.Any(worker =>
            worker.TryGetComponent<WorkerComponent>(out _) && laboratory.Distance(worker, facility).HasValue);

        public bool CanOperate(Thing facility) => Evaluate(facility).CanOperate;

        public ProductionReadiness Evaluate(Thing facility)
        {
            if (facility == null || !facility.TryGetComponent<ProductionComponent>(out var state))
                return new ProductionReadiness(ProductionBlockReason.Unsupported);
            var definition = (FacilityData)facility.Definition;
            var inputs = InputCount(facility);
            var needsTarget = !state.InProgress && definition.Role == FacilityRole.Cultivation
                && state.SelectedTarget == CultivationTarget.None;
            var reason = !laboratory.IsComplete(facility) ? ProductionBlockReason.Construction
                : state.Outstanding > 0 ? ProductionBlockReason.OutputAdmission
                : needsTarget ? ProductionBlockReason.TargetRequired
                : !Accessible(facility) ? ProductionBlockReason.Unreachable
                : !state.InProgress && inputs < definition.InputAmount ? ProductionBlockReason.Inputs
                : !state.InProgress && inputs > definition.InputAmount ? ProductionBlockReason.ExcessInputs
                : ProductionBlockReason.None;
            return new ProductionReadiness(reason, inputs, needsTarget);
        }

        public string WaitReason(Thing facility)
        {
            switch (Evaluate(facility).Reason)
            {
                case ProductionBlockReason.Construction: return "Construction incomplete";
                case ProductionBlockReason.OutputAdmission: return "Completed output awaits actual storage admission";
                case ProductionBlockReason.TargetRequired: return "Choose a cultivation target";
                case ProductionBlockReason.Unreachable: return "Interaction cell is unreachable";
                case ProductionBlockReason.Inputs: return "Waiting for current-cycle inputs";
                case ProductionBlockReason.ExcessInputs: return "Invalid current-cycle input quantity";
                case ProductionBlockReason.Unsupported: return "Not a production facility";
                default: return string.Empty;
            }
        }

        public void Tick(double seconds, ISet<Guid> staffed)
        {
            foreach (var facility in laboratory.Registry.Things.Where(t => t.TryGetComponent<ProductionComponent>(out _))
                .OrderBy(t => t.CreationOrder).ToArray())
            {
                if (!CanOperate(facility)) continue;
                var definition = (FacilityData)facility.Definition;
                var state = facility.GetComponent<ProductionComponent>();
                if (!state.InProgress)
                {
                    foreach (var stock in facility.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                        laboratory.Registry.ConsumeResource(stock, stock.GetComponent<ResourceStackComponent>().Quantity);
                    state.InProgress = true;
                    state.ActiveTarget = state.SelectedTarget;
                    state.Progress = 0;
                }
                state.Progress = Math.Min(definition.ProductionSeconds,
                    state.Progress + seconds * (staffed.Contains(facility.Id) ? laboratory.Settings.StaffedRate : laboratory.Settings.UnattendedRate));
                if (state.Progress + 1e-9 < definition.ProductionSeconds) continue;
                state.InProgress = false;
                laboratory.WorkChanged();
                if (definition.Role == FacilityRole.Processing)
                {
                    laboratory.Registry.CreateResource(definition.OutputResource, definition.OutputAmount,
                        facility.GetComponent<ResourceContainerComponent>());
                    state.Progress = definition.ProductionSeconds;
                    state.Outstanding = definition.OutputAmount;
                }
                else
                {
                    var creature = laboratory.Registry.Create(definition.FindOutput(state.ActiveTarget));
                    var placed = laboratory.Map.Commands.Place(creature.Id, facility.Placement.InteractionCell.Value);
                    if (!placed.Succeeded) throw new InvalidOperationException(placed.ToString());
                    if (creature.TryGetComponent<WorkerComponent>(out var worker))
                        worker.Priorities = laboratory.Preset.ToArray();
                    state.Progress = 0;
                    state.ActiveTarget = CultivationTarget.None;
                }
                laboratory.NotifyProductionCompleted(facility.Id);
            }
        }

        public void AdmitOutput(Thing origin, int amount)
        {
            if (origin.TryGetComponent<GatheringComponent>(out var gathering))
            {
                gathering.Outstanding -= amount;
                if (gathering.Outstanding == 0) gathering.Progress = 0;
            }
            else
            {
                var production = origin.GetComponent<ProductionComponent>();
                production.Outstanding -= amount;
                if (production.Outstanding == 0) production.Progress = 0;
            }
        }
    }
}
