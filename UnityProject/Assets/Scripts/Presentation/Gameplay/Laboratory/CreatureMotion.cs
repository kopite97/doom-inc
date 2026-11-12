using System;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Presentation
{
    // One screen owns one of these per Thing. Coordinates are cell origins, before renderer offsets.
    // This samples simulation state; it never advances time, places Things, or owns a second job/path.
    public sealed class CreatureMotion
    {
        private struct Sample
        {
            public Vector2 Position;
            public Vector2Int From, To, Logical;
            public Guid Target;
            public WorkKind Kind;
            public JobStage Stage;
            public bool Segment, Wander;
            public long Sequence;
            public bool Moving => Segment || Wander;
        }

        public Vector2 Position { get; private set; }
        public Vector2Int Direction { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsCorrecting { get; private set; }
        public WorkerComponent Worker { get; private set; }
        private LaboratoryManager owner;
        private Thing ownerThing;
        private CreatureData creature;
        private Guid thingId;
        private bool initialized, restoreAttempted, restoreFound;
        private WorkerMoveSegment restored;
        private Vector2Int restoredGoal, supportFrom, supportTo;
        private Sample previous;
        private double time, correctionEnd;

        public void Evaluate(LaboratoryManager simulation, Thing thing)
        {
            if (simulation == null || thing == null || !thing.IsPlaced || !ReferenceEquals(thing.Map, simulation.Map)
                || !simulation.Registry.TryGet(thing.Id, out var registered) || !ReferenceEquals(registered, thing))
            {
                Reset();
                return;
            }
            var now = simulation.ElapsedSeconds;
            if (!Finite(now) || now < 0) { Reset(); Position = thing.Placement.Origin; return; }
            if (!ReferenceEquals(owner, simulation) || !ReferenceEquals(ownerThing, thing) || thingId != thing.Id || now < time)
            {
                Reset(); owner = simulation; ownerThing = thing; thingId = thing.Id;
                creature = thing.Definition as CreatureData;
                thing.TryGetComponent<WorkerComponent>(out var cachedWorker); Worker = cachedWorker;
            }
            var logical = thing.Placement.Origin;
            if (creature == null || Worker == null || !Finite(creature.MoveSecondsPerCell) || creature.MoveSecondsPerCell <= 0)
            {
                Snap(new Sample { Position = logical, From = logical, To = logical, Logical = logical }, now);
                return;
            }
            var duration = creature.MoveSecondsPerCell;
            var worker = Worker;
            var sample = ReadSample(simulation, thing, worker, duration);
            var move = worker.LastMove;
            if (!initialized) { Snap(sample, now); return; }

            // A command/restore may relocate even an adjacent creature. Only committed movement
            // observations authorize continuity. Long Advances publish the newest segment directly.
            var relocated = logical != previous.Logical;
            var observed = move.Sequence > previous.Sequence && move.To == logical && move.AtSeconds >= time - 1e-9;
            var skipped = move.Sequence - previous.Sequence > 1;
            if ((relocated && !observed) || skipped)
            {
                Snap(sample, now);
                return;
            }

            if (!ValidSupport(simulation.Map))
            {
                // The old edge itself is now blocked. Re-alignment is an explicit topology exception.
                var safe = sample; safe.Position = logical; safe.From = safe.To = logical;
                safe.Segment = safe.Wander = false;
                Snap(safe, now);
                return;
            }

            var sameTrack = SameTrack(previous, sample) && (!sample.Segment
                || Vector2.Dot(sample.Position - previous.Position, (Vector2)(sample.To - sample.From)) >= -1e-6f);
            var continues = previous.Segment && sample.Segment && previous.To == sample.From;
            var starts = !previous.Moving && sample.Segment && Near(Position, sample.From);
            var beginsWander = sample.Wander && observed && previous.Logical == sample.From;
            var arrives = previous.Moving && !sample.Moving && logical == previous.To;
            if (!IsCorrecting && (sameTrack || continues || starts || beginsWander || arrives || Near(Position, sample.Position)))
            {
                Snap(sample, now);
                return;
            }

            if (!TryConnection(simulation.Map, sample.Logical, move, out var anchor))
            {
                // We do not invent a route between unobserved historical corners.
                Snap(sample, now);
                return;
            }
            if (!IsCorrecting)
            {
                IsCorrecting = true;
                correctionEnd = now + duration;
                // Preserve the last published point on the frame that changes the route/job.
            }
            else if (now > time)
            {
                var remaining = correctionEnd - time;
                var fraction = remaining <= 0 ? 1 : Math.Min(1, (now - time) / remaining);
                var length = Distance(Position, anchor) + Distance(anchor, sample.Logical)
                    + Distance(sample.Logical, sample.Position);
                var travel = (float)(length * fraction);
                WalkLeg(anchor, supportFrom, supportTo, ref travel);
                WalkLeg(sample.Logical, anchor, sample.Logical, ref travel);
                WalkLeg(sample.Position, sample.From, sample.To, ref travel);
                if (now + 1e-9 >= correctionEnd || Near(Position, sample.Position))
                {
                    Position = sample.Position; supportFrom = sample.From; supportTo = sample.To;
                    IsCorrecting = false;
                }
            }
            IsMoving = IsCorrecting || sample.Moving;
            previous = sample; time = now;
        }

        private Sample ReadSample(LaboratoryManager simulation, Thing thing, WorkerComponent worker, double duration)
        {
            var logical = thing.Placement.Origin;
            var move = worker.LastMove;
            var result = new Sample { Position = logical, From = logical, To = logical,
                Logical = logical, Sequence = move.Sequence };
            var found = WorkerManager.TryGetMoveSegment(simulation, thing, worker, out var segment);
            if (found) restoreFound = false;
            else found = RestoreSegment(simulation, thing, worker, duration, out segment);
            if (found)
            {
                result.From = segment.From; result.To = segment.To;
                result.Position = (Vector2)segment.From + (Vector2)(segment.To - segment.From)
                    * (float)(segment.ProgressSeconds / segment.DurationSeconds);
                result.Target = segment.TargetId; result.Kind = segment.Kind; result.Stage = segment.Stage;
                result.Segment = true;
            }
            else if (move.Sequence > 0 && move.IsWander && move.To == logical
                && move.LayoutVersion == simulation.Map.LayoutVersion && Finite(move.AtSeconds)
                && simulation.ElapsedSeconds >= move.AtSeconds && simulation.ElapsedSeconds < move.AtSeconds + duration
                && Edge(simulation.Map, move.From, move.To))
            {
                result.From = move.From; result.To = move.To;
                result.Position = (Vector2)move.From + (Vector2)(move.To - move.From)
                    * (float)((simulation.ElapsedSeconds - move.AtSeconds) / duration);
                result.Wander = true;
            }
            return result;
        }

        private bool RestoreSegment(LaboratoryManager simulation, Thing thing, WorkerComponent worker,
            double duration, out WorkerMoveSegment segment)
        {
            segment = default;
            var job = worker.Job;
            if (worker.HasCachedMoveRoute || worker.LastMove.Sequence != 0 || worker.WaitReason != WorkerWaitReason.None
                || !Finite(worker.MoveProgress) || worker.MoveProgress <= 0 || worker.MoveProgress >= duration
                || job == null || (job.Stage != JobStage.ToSupply && job.Stage != JobStage.ToDestination)) return false;
            var targetId = job.Stage == JobStage.ToSupply ? job.SourceId : job.TargetId;
            if (!targetId.HasValue || !simulation.Registry.TryGet(targetId.Value, out var target) || !target.IsPlaced
                || !ReferenceEquals(target.Map, simulation.Map)) return false;
            var goal = simulation.TargetCell(target);
            if (!goal.HasValue || goal.Value == thing.Placement.Origin) return false;
            if (!restoreAttempted)
            {
                // Only the initial display of a restored, already-progressing worker can query.
                // One attempt total per screen/Thing lifetime also caches failure without growing a key set.
                if (initialized) return false;
                restoreAttempted = true;
                var path = GridPathfinder.FindPath(simulation.Map, thing.Placement.Origin, goal.Value);
                restoreFound = path != null && path.Count > 0;
                if (restoreFound)
                {
                    restored = new WorkerMoveSegment(thing.Placement.Origin, path[0], worker.MoveProgress, duration,
                        targetId.Value, job.Kind, job.Stage, simulation.Map.LayoutVersion);
                    restoredGoal = goal.Value;
                }
            }
            if (!restoreFound || restored.From != thing.Placement.Origin || restored.TargetId != targetId.Value
                || restored.Kind != job.Kind || restored.Stage != job.Stage || restoredGoal != goal.Value
                || restored.LayoutVersion != simulation.Map.LayoutVersion || !Edge(simulation.Map, restored.From, restored.To)) return false;
            segment = new WorkerMoveSegment(restored.From, restored.To, worker.MoveProgress, duration,
                restored.TargetId, restored.Kind, restored.Stage, restored.LayoutVersion);
            return true;
        }

        private bool ValidSupport(GridMap map) => Near(Position, supportFrom)
            ? map.IsWalkable(supportFrom) : Near(Position, supportTo) ? map.IsWalkable(supportTo)
            : Edge(map, supportFrom, supportTo);

        private bool TryConnection(GridMap map, Vector2Int logical, WorkerMoveObservation move, out Vector2Int anchor)
        {
            anchor = logical;
            if (supportFrom == logical || supportTo == logical) return true;
            if (move.Sequence > 0 && move.To == logical && move.LayoutVersion == map.LayoutVersion
                && (supportFrom == move.From || supportTo == move.From) && Edge(map, move.From, move.To))
            { anchor = move.From; return true; }
            return false;
        }

        private void WalkLeg(Vector2 end, Vector2Int edgeFrom, Vector2Int edgeTo, ref float remaining)
        {
            var offset = end - Position;
            var length = offset.magnitude;
            if (length <= 1e-7f) return;
            if (remaining <= 0) return;
            Direction = Cardinal(offset);
            if (remaining >= length)
            { Position = end; remaining -= length; }
            else
            { Position += offset * (remaining / length); remaining = 0; }
            supportFrom = edgeFrom; supportTo = edgeTo;
        }

        private void Snap(Sample sample, double now)
        {
            Position = sample.Position; IsMoving = sample.Moving; IsCorrecting = false;
            supportFrom = sample.From; supportTo = sample.To;
            if (sample.Moving && sample.To != sample.From) Direction = sample.To - sample.From;
            previous = sample; time = now; initialized = true;
        }
        private void Reset()
        {
            owner = null; ownerThing = null; creature = null; Worker = null; thingId = Guid.Empty;
            initialized = restoreAttempted = restoreFound = false;
            Position = Vector2.zero; Direction = Vector2Int.zero; IsMoving = IsCorrecting = false;
            previous = default; supportFrom = supportTo = default; time = correctionEnd = 0;
        }
        private static bool SameTrack(Sample a, Sample b) => a.Segment == b.Segment && a.Wander == b.Wander
            && a.From == b.From && a.To == b.To && (!a.Wander || a.Sequence == b.Sequence)
            && (!a.Segment || SameDestination(a, b));
        private static bool SameDestination(Sample a, Sample b) => a.Target == b.Target && a.Kind == b.Kind && a.Stage == b.Stage;
        private static bool Edge(GridMap map, Vector2Int from, Vector2Int to)
            => Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y) == 1 && map.IsWalkable(from) && map.IsWalkable(to);
        private static bool Near(Vector2 a, Vector2 b) => (a - b).sqrMagnitude < 1e-10f;
        private static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static Vector2Int Cardinal(Vector2 delta) => Math.Abs(delta.x) > Math.Abs(delta.y)
            ? new Vector2Int(delta.x < 0 ? -1 : 1, 0) : new Vector2Int(0, delta.y < 0 ? -1 : 1);
    }
}
