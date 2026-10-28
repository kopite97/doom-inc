using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Persistence
{
    public sealed class SessionSnapshotCodec
    {
        public const int LegacyVersion = 1;
        public const int CurrentVersion = 5;
        private readonly bool requireLaboratory;
        private string DataHash => definitions.Values.FirstOrDefault()?.Settings?.CompatibilityHash;
        private readonly Dictionary<string, ThingData> definitions =
            new Dictionary<string, ThingData>(StringComparer.Ordinal);

        public SessionSnapshotCodec(IEnumerable<ThingData> definitions, bool requireLaboratory = false)
        {
            this.requireLaboratory = requireLaboratory;
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));
            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("The definition catalog cannot contain null.", nameof(definitions));
                definition.Validate();
                if (this.definitions.TryGetValue(definition.DefinitionId, out var existing) &&
                    !ReferenceEquals(existing, definition))
                    throw new ArgumentException("Duplicate definition ID: " + definition.DefinitionId, nameof(definitions));
                this.definitions[definition.DefinitionId] = definition;
            }
        }

        public SessionSnapshot Capture(SessionState state, SessionPhase phase)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (!ReferenceEquals(state.Registry, state.Map.Registry))
                throw Invalid("The session Map belongs to another registry.");
            if (state.Runtime != null && !(state.Runtime is LaboratoryManager))
                throw Invalid("This session runtime has no supported save contract.");
            var laboratory = state.Runtime as LaboratoryManager;
            if (laboratory != null && (!ReferenceEquals(laboratory.Registry, state.Registry) ||
                !ReferenceEquals(laboratory.Map, state.Map)))
                throw Invalid("The laboratory runtime belongs to another session.");
            var things = new List<ThingSnapshot>();
            foreach (var thing in state.Registry.Things)
            {
                if (!ReferenceEquals(GetDefinition(thing.Definition.DefinitionId), thing.Definition))
                    throw Invalid("The active definition differs from the catalog: " + thing.Definition.DefinitionId);
                if (thing.Map != null && !ReferenceEquals(thing.Map, state.Map))
                    throw Invalid("A Thing is placed on a Map outside this session: " + thing.Id);
                things.Add(new ThingSnapshot
                {
                    id = thing.Id.ToString("N"), definitionId = thing.Definition.DefinitionId,
                    creationOrder = thing.CreationOrder,
                    quantity = thing.TryGetComponent<ResourceStackComponent>(out var stack) ? stack.Quantity : 0,
                    holderId = thing.HolderId?.ToString("N")
                });
            }
            things.Sort((left, right) => left.creationOrder.CompareTo(right.creationOrder));
            var placements = new List<PlacementSnapshot>();
            foreach (var placement in state.Map.GetPlacements())
                placements.Add(new PlacementSnapshot
                {
                    thingId = placement.ThingId.ToString("N"), x = placement.Origin.x,
                    y = placement.Origin.y, rotation = (int)placement.Rotation
                });
            var snapshot = new SessionSnapshot
            {
                dataHash = DataHash,
                version = laboratory == null ? LegacyVersion : CurrentVersion, phase = phase,
                runtimeKind = laboratory == null ? null : "laboratory",
                lastCreationOrder = state.Registry.LastCreationOrder,
                mapWidth = state.Map.Width, mapHeight = state.Map.Height,
                things = things.ToArray(), placements = placements.ToArray(),
                laboratory = laboratory == null ? null : LaboratorySnapshotCodec.Capture(laboratory),
                invasion = laboratory == null ? null : InvasionSnapshotCodec.Capture(laboratory, phase)
            };
            // Validate that the captured graph can actually be reconstructed before it
            // replaces a good save. No temporary edit/selection state is persisted.
            Restore(snapshot);
            return snapshot;
        }

        public SessionState Restore(SessionSnapshot snapshot)
        {
            ValidateStructure(snapshot);
            if (string.IsNullOrEmpty(snapshot.dataHash))
            {
                if (snapshot.version >= CurrentVersion || DataHash != LegacyDataBaseline.Hash)
                    throw Invalid("Legacy save requires the baseline data. Preserve the save and restart with matching data, or explicitly start a new run.");
            }
            else if (snapshot.dataHash != DataHash)
                throw Invalid("Saved balance differs from loaded data. Restart the app with matching data, or explicitly start a new run. The save is preserved.");
            if (requireLaboratory && snapshot.version < 2)
                throw Invalid("This play slot requires a laboratory session, not a diagnostic save.");
            try
            {
                var registry = new ThingRegistry(snapshot.version >= 2
                    ? GameplayComponentFactory.Attach : (Action<Thing>)null);
                // Holder creation order may be later than a transferred resource's order.
                foreach (var saved in snapshot.things)
                {
                    var definition = GetDefinition(saved.definitionId);
                    if (definition.Kind != ThingKind.Resource)
                        registry.RestoreThing(ParseId(saved.id), saved.creationOrder, definition,
                            ParseOptionalId(saved.holderId), saved.quantity);
                }
                foreach (var saved in snapshot.things)
                {
                    var definition = GetDefinition(saved.definitionId);
                    if (definition.Kind == ThingKind.Resource)
                        registry.RestoreThing(ParseId(saved.id), saved.creationOrder, definition,
                            ParseOptionalId(saved.holderId), saved.quantity);
                }
                registry.RestoreHighWater(snapshot.lastCreationOrder);
                var map = new GridMap(registry, snapshot.mapWidth, snapshot.mapHeight);
                // Existing rules permit a facility to be placed over a creature. Restore
                // creatures first so replay order does not reject that legal saved state.
                RestorePlacements(snapshot, registry, map, ThingKind.Creature);
                RestorePlacements(snapshot, registry, map, ThingKind.Facility);
                var runtime = snapshot.version >= 2
                    ? LaboratorySnapshotCodec.Restore(snapshot.laboratory, registry, map, GetDefinition, snapshot.version,
                        definitions.Values.OfType<FacilityData>().FirstOrDefault(d => d.Role == FacilityRole.Processing)?.OutputResource,
                        snapshot.version >= 4 ? snapshot.invasion.settings : null) : null;
                if (runtime != null)
                {
                    InvasionSnapshotCodec.Restore(snapshot.invasion, runtime, snapshot.version, snapshot.phase);
                    runtime.ValidateState();
                }
                return new SessionState(registry, map, runtime);
            }
            catch (Exception exception) when (exception is ArgumentException ||
                exception is InvalidOperationException || exception is OverflowException)
            {
                throw new InvalidDataException("The saved session graph is invalid: " + exception.Message, exception);
            }
        }

        public string Serialize(SessionSnapshot snapshot)
        {
            ValidateStructure(snapshot);
            return JsonUtility.ToJson(snapshot, true);
        }

        public SessionSnapshot Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw Invalid("The save is empty.");
            SessionSnapshot snapshot;
            try { snapshot = JsonUtility.FromJson<SessionSnapshot>(json); }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("The save is not valid JSON.", exception);
            }
            ValidateStructure(snapshot);
            return snapshot;
        }

        private ThingData GetDefinition(string id)
        {
            if (!definitions.TryGetValue(id, out var definition) || definition == null)
                throw Invalid("Missing definition: " + id);
            definition.Validate();
            return definition;
        }

        private static void RestorePlacements(SessionSnapshot snapshot, ThingRegistry registry,
            GridMap map, ThingKind kind)
        {
            foreach (var saved in snapshot.placements)
            {
                var id = ParseId(saved.thingId);
                if (!registry.TryGet(id, out var thing))
                    throw Invalid("Placement references a missing Thing: " + saved.thingId);
                if (thing.Definition.Kind == ThingKind.Resource)
                    throw Invalid("Resources cannot have Map placements: " + saved.thingId);
                if (thing.Definition.Kind != kind)
                    continue;
                var result = map.Commands.Place(id, new Vector2Int(saved.x, saved.y), (QuarterTurn)saved.rotation);
                if (!result.Succeeded)
                    throw Invalid("Invalid placement for " + saved.thingId + ": " + result);
            }
        }

        private static void ValidateStructure(SessionSnapshot snapshot)
        {
            if (snapshot == null)
                throw Invalid("The save has no session object.");
            if (snapshot.version != LegacyVersion && snapshot.version != 2 && snapshot.version != 3 && snapshot.version != 4 && snapshot.version != CurrentVersion)
                throw Invalid("Unsupported save version: " + snapshot.version);
            if (snapshot.version >= 4) InvasionSnapshotCodec.ValidateStructure(snapshot.invasion);
            // JsonUtility materializes null nested classes as empty objects. The
            // discriminator distinguishes an absent legacy extension from real data.
            else if (snapshot.invasion != null && (snapshot.invasion.schemaVersion != 0 ||
                snapshot.invasion.score != 0 || snapshot.invasion.activitySequence != 0 || !string.IsNullOrEmpty(snapshot.invasion.elapsed) ||
                snapshot.invasion.goalReached || snapshot.invasion.legacyEndVersion != 0 ||
                (snapshot.invasion.invaders?.Length ?? 0) != 0 || (snapshot.invasion.reports?.Length ?? 0) != 0))
                throw Invalid("External activity requires save version 4.");
            if (snapshot.version >= 2)
            {
                if (snapshot.runtimeKind != "laboratory")
                    throw Invalid("Version 2 requires the laboratory runtime contract.");
                LaboratorySnapshotCodec.ValidateStructure(snapshot.laboratory);
                if (snapshot.version == 2 && (!string.IsNullOrEmpty(snapshot.laboratory.cultureDefinitionId) ||
                    snapshot.laboratory.preset.Length != 3 || snapshot.laboratory.things.Any(t => t.hasProduction ||
                        (t.hasWorker && (!string.IsNullOrEmpty(t.worker.assignedFacilityId) || t.worker.priorities.Length != 3 ||
                            (t.worker.hasJob && t.worker.job.kind == WorkKind.Production))))))
                    throw Invalid("Production state requires save version 3.");
            }
            else if (!string.IsNullOrEmpty(snapshot.runtimeKind) ||
                (snapshot.laboratory != null && !string.IsNullOrEmpty(snapshot.laboratory.organicDefinitionId)))
                throw Invalid("A legacy save cannot contain laboratory state.");
            if (snapshot.phase != SessionPhase.Playing && snapshot.phase != SessionPhase.Destruction &&
                snapshot.phase != SessionPhase.Results)
                throw Invalid("Only a prepared session phase can be saved.");
            if (snapshot.lastCreationOrder < 0 || snapshot.mapWidth <= 0 || snapshot.mapHeight <= 0)
                throw Invalid("Invalid creation high-water mark or Map dimensions.");
            if (snapshot.things == null || snapshot.placements == null)
                throw Invalid("The save must contain Thing and placement arrays.");
            var ids = new HashSet<Guid>();
            var orders = new HashSet<long>();
            foreach (var thing in snapshot.things)
            {
                if (thing == null || string.IsNullOrWhiteSpace(thing.definitionId))
                    throw Invalid("A Thing or its definition ID is missing.");
                if (!ids.Add(ParseId(thing.id)))
                    throw Invalid("Duplicate Thing ID: " + thing.id);
                if (thing.creationOrder <= 0 || thing.creationOrder > snapshot.lastCreationOrder ||
                    !orders.Add(thing.creationOrder))
                    throw Invalid("Invalid or duplicate creation order: " + thing.creationOrder);
                if (thing.quantity < 0)
                    throw Invalid("A resource quantity cannot be negative.");
                ParseOptionalId(thing.holderId);
            }
            var placedIds = new HashSet<Guid>();
            foreach (var placement in snapshot.placements)
            {
                if (placement == null)
                    throw Invalid("A placement is missing.");
                if (!placedIds.Add(ParseId(placement.thingId)))
                    throw Invalid("Duplicate placement: " + placement.thingId);
                if (placement.rotation < 0 || placement.rotation > 3)
                    throw Invalid("Invalid quarter-turn rotation.");
            }
        }

        private static Guid ParseId(string value)
        {
            if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
                throw Invalid("Invalid Thing ID: " + value);
            return id;
        }

        private static Guid? ParseOptionalId(string value)
            => string.IsNullOrEmpty(value) ? (Guid?)null : ParseId(value);

        private static InvalidDataException Invalid(string message) => new InvalidDataException(message);
    }
}
