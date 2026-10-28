using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Gameplay
{
    // One session's orchestration and commands. Components own durable state;
    // paths and the unconfirmed construction preview are disposable projections.
    public sealed class LaboratoryManager : ISessionRuntime, IExternalSessionRuntime
    {
        private readonly WorkerManager workers;
        public ConstructionManager Construction { get; }

        public LaboratoryManager(ThingRegistry registry, GridMap map, ResourceData organic, ResourceData culture = null,
            InvasionSettings invasionSettings = null)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Organic = organic ?? throw new ArgumentNullException(nameof(organic));
            if (!ReferenceEquals(map.Registry, registry))
                throw new ArgumentException("Map and simulation must share a registry.");
            organic.Validate();
            if (culture != null)
            {
                culture.Validate();
                if (culture == organic) throw new ArgumentException("Laboratory resource definitions must be distinct.");
            }
            Culture = culture;
            Production = new ProductionManager(this);
            Logistics = new LogisticsManager(this);
            workers = new WorkerManager(this);
            Construction = new ConstructionManager(this);
            Invasion = new InvasionManager(this, invasionSettings ?? organic.Settings.Invasion);
        }

        public GameSettings Settings => Organic.Settings;
        public ThingRegistry Registry { get; }
        public GridMap Map { get; }
        public ResourceData Organic { get; }
        public ResourceData Culture { get; }
        public ProductionManager Production { get; }
        public InvasionManager Invasion { get; }
        public double SecondsUntilExternalEvent => Invasion.SecondsUntilEvent;
        public bool HasReachedGoal => Invasion.State.GoalReached;
        public void AdvanceExternal(double seconds) => Invasion.Advance(seconds);
        public bool Supports(ResourceData resource) => resource != null && (resource == Organic || resource == Culture);
        public IReadOnlyList<int> Preset => workers.Preset;
        public LogisticsManager Logistics { get; }
        public IReadOnlyList<TransportReservation> Reservations => Logistics.Reservations;
        public bool IsEditing => Construction.IsEditing;
        public FacilityData ConstructionDefinition => Construction.ConstructionDefinition;
        public Vector2Int ConstructionOrigin => Construction.ConstructionOrigin;
        public QuarterTurn ConstructionRotation => Construction.ConstructionRotation;
        public PlacementResult ConstructionValidation => Construction.ConstructionValidation;
        public string LastError { get; private set; }

        public bool BeginConstruction(FacilityData definition) => Result(Construction.BeginConstruction(definition), Construction.LastError);
        public bool PreviewConstruction(Vector2Int origin, QuarterTurn rotation) => Result(Construction.PreviewConstruction(origin, rotation), Construction.LastError);
        public bool ConfirmConstruction() => Result(Construction.ConfirmConstruction(), Construction.LastError);
        public bool CancelConstruction() => Result(Construction.CancelConstruction(), Construction.LastError);
        public bool SetAllowed(Guid id, bool allowed) => Result(Logistics.SetAllowed(id, allowed), Logistics.LastError);
        public bool SetAllowed(Guid id, ResourceData resource, bool allowed) => Result(Logistics.SetAllowed(id, resource, allowed), Logistics.LastError);
        public bool SelectCultivation(Guid id, CultivationTarget target) => Result(Production.Select(id, target), Production.LastError);
        public bool AssignWorker(Guid id, Guid? facility) => Result(workers.Assign(id, facility), workers.LastError);
        public bool SetPreset(int[] priorities) => Result(workers.SetPreset(priorities), workers.LastError);
        public bool SetWorkerPriorities(Guid id, int[] priorities, bool usePreset = false) => Result(workers.SetWorkerPriorities(id, priorities, usePreset), workers.LastError);
        private bool Result(bool success, string error) { LastError = error; return success; }

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
                Production.Tick(step, workers.ProducingFacilities);
                workers.Reconcile();
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
            workers.SetPreset(priorities);
            Logistics.Restore(reservations);
        }

        public void ValidateState() => LaboratoryStateValidator.Validate(this);
        internal static bool ValidPriorities(int[] values) => values != null && (values.Length == 3 || values.Length == 4) && values.All(value => value >= 0);
    }
}
