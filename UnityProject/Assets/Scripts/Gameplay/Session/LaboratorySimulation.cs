using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    // One session's orchestration and commands. Components own durable state;
    // paths and the unconfirmed construction preview are disposable projections.
    public sealed class LaboratorySimulation : ISessionRuntime
    {
        private int[] preset = { 1, 2, 3 };
        private readonly WorkerSystem workers;

        public LaboratorySimulation(ThingRegistry registry, GridMap map, ResourceDefinition organic)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Organic = organic ?? throw new ArgumentNullException(nameof(organic));
            if (!ReferenceEquals(map.Registry, registry))
                throw new ArgumentException("Map and simulation must share a registry.");
            organic.Validate();
            Logistics = new LogisticsSystem(this);
            workers = new WorkerSystem(this);
        }

        public ThingRegistry Registry { get; }
        public GridMap Map { get; }
        public ResourceDefinition Organic { get; }
        public IReadOnlyList<int> Preset => Array.AsReadOnly(preset);
        public LogisticsSystem Logistics { get; }
        public IReadOnlyList<TransportReservation> Reservations => Logistics.Reservations;
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
            OnMapChanged();
            return true;
        }

        public bool CancelConstruction()
        {
            if (!IsEditing) return Reject("No construction preview is active.");
            ConstructionDefinition = null;
            LastError = null;
            return true;
        }

        public bool SetAllowed(Guid storageId, bool allowed)
        {
            if (!Registry.TryGet(storageId, out var thing) ||
                !thing.TryGetComponent<StorageComponent>(out var storage))
                return Reject("Select a storage facility.");
            storage.AllowsOrganic = allowed;
            Logistics.Reconcile();
            LastError = null;
            return true;
        }

        public bool SetPreset(int[] priorities)
        {
            if (!ValidPriorities(priorities)) return Reject("Provide three nonnegative priorities.");
            preset = priorities.ToArray();
            foreach (var thing in Registry.Things)
                if (thing.TryGetComponent<WorkerComponent>(out var worker) && worker.UsesPreset)
                    worker.Priorities = preset.ToArray();
            LastError = null;
            return true;
        }

        public bool SetWorkerPriorities(Guid workerId, int[] priorities, bool usePreset = false)
        {
            if (!ValidPriorities(priorities) || !Registry.TryGet(workerId, out var thing) ||
                !thing.TryGetComponent<WorkerComponent>(out var worker))
                return Reject("Select a worker and provide three nonnegative priorities.");
            worker.UsesPreset = usePreset;
            worker.Priorities = (usePreset ? preset : priorities).ToArray();
            LastError = null;
            return true;
        }

        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (IsEditing || Map.Commands.IsEditing) return;
            // The same bounded steps are used by Update and explicit harness time.
            // This is not a separate test simulation or an offline catch-up clock.
            while (seconds > 1e-10)
            {
                var step = Math.Min(0.05, seconds);
                Logistics.Reconcile();
                workers.Tick(step);
                seconds -= step;
            }
        }

        public void OnMapChanged()
        {
            Logistics.Reconcile();
            workers.Reconcile();
        }

        public int TotalAt(Guid holderId) => Registry.TryGet(holderId, out var holder) &&
            holder.TryGetComponent<ResourceContainerComponent>(out var container)
            ? container.Contents.Sum(resource => resource.GetComponent<ResourceStackComponent>().Quantity) : 0;

        public bool IsComplete(Thing thing) => thing != null && thing.Exists &&
            thing.TryGetComponent<ConstructionComponent>(out var construction) && construction.IsComplete;

        public Vector2Int? TargetCell(Thing thing) => thing?.Placement == null ? (Vector2Int?)null :
            thing.Placement.InteractionCell ?? thing.Placement.Origin;

        public int? Distance(Thing from, Thing to)
        {
            var start = TargetCell(from);
            var goal = TargetCell(to);
            if (!start.HasValue || !goal.HasValue) return null;
            return GridPathfinder.FindPath(Map, start.Value, goal.Value)?.Count;
        }

        public Thing Nearest(IEnumerable<Thing> candidates, Vector2Int from)
        {
            return candidates.Select(thing => new { Thing = thing, Cell = TargetCell(thing) })
                .Where(item => item.Cell.HasValue)
                .Select(item => new { item.Thing, Path = GridPathfinder.FindPath(Map, from, item.Cell.Value) })
                .Where(item => item.Path != null).OrderBy(item => item.Path.Count)
                .ThenBy(item => item.Thing.CreationOrder).Select(item => item.Thing).FirstOrDefault();
        }

        public void RestoreState(int[] priorities, IEnumerable<TransportReservation> reservations)
        {
            if (!ValidPriorities(priorities)) throw new System.IO.InvalidDataException("Invalid saved preset.");
            preset = priorities.ToArray();
            Logistics.Restore(reservations);
        }

        public void ValidateState() => LaboratoryStateValidator.Validate(this);
        internal static bool ValidPriorities(int[] values) => values != null && values.Length == 3 && values.All(value => value >= 0);
        private bool Reject(string message) { LastError = message; return false; }
    }
}
