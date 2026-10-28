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

        public bool Select(Guid facilityId, CultivationTarget target)
        {
            if ((target != CultivationTarget.Support && target != CultivationTarget.Invader) ||
                !laboratory.Registry.TryGet(facilityId, out var facility) || !laboratory.IsComplete(facility) ||
                !(facility.Definition is FacilityData definition) || definition.Role != FacilityRole.Cultivation)
            { LastError = "Select a completed cultivator and a support or invader target."; return false; }
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

        public bool CanOperate(Thing facility)
        {
            if (!laboratory.IsComplete(facility) || !facility.TryGetComponent<ProductionComponent>(out var state)
                || state.Outstanding > 0 || !Accessible(facility)) return false;
            var definition = (FacilityData)facility.Definition;
            return state.InProgress || ((definition.Role == FacilityRole.Processing || state.SelectedTarget != CultivationTarget.None)
                && InputCount(facility) == definition.InputAmount);
        }

        public string WaitReason(Thing facility)
        {
            var state = facility.GetComponent<ProductionComponent>();
            var definition = (FacilityData)facility.Definition;
            if (!laboratory.IsComplete(facility)) return "Construction incomplete";
            if (state.Outstanding > 0) return "Completed output awaits actual storage admission";
            if (!state.InProgress && definition.Role == FacilityRole.Cultivation && state.SelectedTarget == CultivationTarget.None)
                return "Choose a cultivation target";
            if (!Accessible(facility)) return "Interaction cell is unreachable";
            if (!state.InProgress && InputCount(facility) < definition.InputAmount) return "Waiting for current-cycle inputs";
            return string.Empty;
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
                if (definition.Role == FacilityRole.Processing)
                {
                    laboratory.Registry.CreateResource(definition.OutputResource, definition.OutputAmount,
                        facility.GetComponent<ResourceContainerComponent>());
                    state.Progress = definition.ProductionSeconds;
                    state.Outstanding = definition.OutputAmount;
                }
                else
                {
                    var creature = laboratory.Registry.Create(state.ActiveTarget == CultivationTarget.Support
                        ? definition.SupportOutput : definition.InvaderOutput);
                    var placed = laboratory.Map.Commands.Place(creature.Id, facility.Placement.InteractionCell.Value);
                    if (!placed.Succeeded) throw new InvalidOperationException(placed.ToString());
                    if (creature.TryGetComponent<WorkerComponent>(out var worker))
                        worker.Priorities = laboratory.Preset.ToArray();
                    state.Progress = 0;
                    state.ActiveTarget = CultivationTarget.None;
                }
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
