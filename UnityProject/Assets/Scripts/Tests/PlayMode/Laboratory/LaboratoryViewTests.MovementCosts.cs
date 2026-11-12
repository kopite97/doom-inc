#if UNITY_EDITOR
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator MotionEvaluationHasNoSteadyAllocationsOrExtraQueriesAtTwelveTwentyFourAndFortyEightWorkers()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var catalog = GameDataStore.Instance.Catalog;
                var codec = new SessionSnapshotCodec(catalog.Definitions.Values, true);
                var results = new object[3];
                var sizes = new[] { 12, 24, 48 };
                for (var sample = 0; sample < sizes.Length; sample++)
                {
                    var registry = new ThingRegistry(GameplayComponentFactory.Attach);
                    var map = new GridMap(registry, 128, 8);
                    var simulation = new LaboratoryManager(registry, map, catalog.Selected<ResourceData>("organicId"), catalog.Selected<ResourceData>("cultureId"));
                    var state = new SessionState(registry, map, simulation);
                    var workers = new List<Thing>(sizes[sample]);
                    var targets = new Thing[sizes[sample]];
                    var gathering = catalog.Selected<FacilityData>("gatheringId");
                    for (var i = 0; i < sizes[sample]; i++)
                    {
                        var target = registry.Create(gathering);
                        Assert.That(map.Commands.Place(target.Id, new Vector2Int(i * 2 + 1, 3)).Succeeded, Is.True);
                        var construction = target.GetComponent<ConstructionComponent>();
                        SetMovementProperty(construction, nameof(ConstructionComponent.Delivered), gathering.BuildCost);
                        SetMovementProperty(construction, nameof(ConstructionComponent.Progress), (double)gathering.BuildSeconds);
                        SetMovementProperty(construction, nameof(ConstructionComponent.IsComplete), true);
                        targets[i] = target;
                        var worker = registry.Create(catalog.Selected<CreatureData>("supportId"));
                        Assert.That(map.Commands.Place(worker.Id, new Vector2Int(i * 2, 1)).Succeeded, Is.True);
                        workers.Add(worker);
                    }
                    var motions = new CreatureMotion[workers.Count];
                    for (var i = 0; i < workers.Count; i++)
                    {
                        var thing = workers[i];
                        var worker = thing.GetComponent<WorkerComponent>();
                        var job = new WorkerJob();
                        SetMovementProperty(job, nameof(WorkerJob.Kind), WorkKind.Gathering);
                        SetMovementProperty(job, nameof(WorkerJob.Stage), JobStage.ToDestination);
                        SetMovementProperty(job, nameof(WorkerJob.TargetId), targets[i].Id);
                        SetMovementProperty(worker, nameof(WorkerComponent.Job), job);
                        WorkerManager.Move(simulation, thing, targets[i], ((CreatureData)thing.Definition).MoveSecondsPerCell * .5);
                        motions[i] = new CreatureMotion();
                        motions[i].Evaluate(simulation, thing);
                        Assert.That(motions[i].IsMoving, Is.True, "Each cost sample must include actual valid in-flight interpolation.");
                        Assert.That(motions[i].Position, Is.Not.EqualTo((Vector2)thing.Placement.Origin));
                    }
                    var things = workers.ToArray();
                    const int warmup = 1000;
                    const int iterations = 4096;
                    for (var repeat = 0; repeat < warmup; repeat++)
                        for (var i = 0; i < things.Length; i++) motions[i].Evaluate(simulation, things[i]);
                    var semanticBefore = codec.Serialize(codec.Capture(state, SessionPhase.Playing));
                    var pathsBefore = simulation.Map.PathQueries;
                    var visitedBefore = simulation.Map.VisitedCells;
                    var durations = new long[iterations];
                    // Warm the measurement APIs as well as the evaluator before the allocation interval.
                    Stopwatch.GetTimestamp();
                    GC.GetAllocatedBytesForCurrentThread();
                    var bytesBefore = GC.GetAllocatedBytesForCurrentThread();
                    var firstTimestamp = Stopwatch.GetTimestamp();
                    for (var repeat = 0; repeat < iterations; repeat++)
                    {
                        var begin = Stopwatch.GetTimestamp();
                        for (var i = 0; i < things.Length; i++) motions[i].Evaluate(simulation, things[i]);
                        durations[repeat] = Stopwatch.GetTimestamp() - begin;
                    }
                    var totalTicks = Stopwatch.GetTimestamp() - firstTimestamp;
                    var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - bytesBefore;
                    var pathsAdded = simulation.Map.PathQueries - pathsBefore;
                    var visitedAdded = simulation.Map.VisitedCells - visitedBefore;
                    Assert.That(allocatedBytes, Is.Zero, "Only steady CreatureMotion evaluation is inside the allocation interval.");
                    Assert.That(pathsAdded, Is.Zero);
                    Assert.That(visitedAdded, Is.Zero);
                    Assert.That(codec.Serialize(codec.Capture(state, SessionPhase.Playing)), Is.EqualTo(semanticBefore),
                        "Repeated presentation evaluation cannot change logical state or its save.");
                    Array.Sort(durations);
                    var ticksToMs = 1000d / Stopwatch.Frequency;
                    results[sample] = new { workers = things.Length, warmupIterations = warmup, iterations,
                        allocatedBytes, pathsAdded, visitedAdded, stopwatchFrequency = Stopwatch.Frequency,
                        medianMs = (durations[iterations / 2 - 1] + durations[iterations / 2]) * .5 * ticksToMs,
                        p95Ms = durations[(int)Math.Ceiling(iterations * .95) - 1] * ticksToMs,
                        maxMs = durations[iterations - 1] * ticksToMs, totalMs = totalTicks * ticksToMs };
                }
                Directory.CreateDirectory(MovementEvidenceRoot);
                var data = new { results, note = "Supplementary bounded Evaluate-only comparison. Each Thing has its own completed facility work claim and actual cached in-flight route, fixed simulation time and a separately owned CreatureMotion. The supplementary fixed 128x8 map isolates target-count scaling; it is not a new product map or a full-scene benchmark. Buffers, routes, jobs, JIT warmup, fixtures, serialization and assertions are outside the measured allocation interval. Stopwatch samples include the loop and clock overhead; these are not full-frame CPU or device timings." };
                File.WriteAllText(Path.Combine(MovementEvidenceRoot, "movement-evaluation-costs.json"),
                    (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                        .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { data }));
            }
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator TenSessionReplacementsDiscardOldCreatureDisplaysAndWorldRoots()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var ids = new Guid[10];
                var rootCounts = new int[10];
                var memory = new long[10];
                for (var cycle = 0; cycle < ids.Length; cycle++)
                {
                    var worker = input.View.Simulation.Registry.Things.First(thing => thing.TryGetComponent<WorkerComponent>(out _));
                    var oldId = worker.Id;
                    var oldArt = input.Map.World.Root.Find("ThingArt_" + oldId.ToString("N"));
                    var oldSimulation = input.View.Simulation;
                    Assert.That(oldArt, Is.Not.Null);
                    Assert.That(input.Map.TryDisplayedPosition(oldId, out _), Is.True);
                    Assert.That(input.View.Restart(), Is.True);
                    yield return null; yield return null;
                    Assert.That(input.View.Simulation.Registry.TryGet(oldId, out _), Is.False);
                    Assert.That(input.Map.TryDisplayedPosition(oldId, out _), Is.False);
                    Assert.That(input.Map.TryThingScreenPoint(oldId, out _), Is.False);
                    Assert.That(oldArt == null, Is.True, "The previous session's body/cargo/selection root must be destroyed.");
                    var oldEvent = (Delegate)typeof(LaboratoryManager).GetField(nameof(LaboratoryManager.GatheringCompleted),
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(oldSimulation);
                    Assert.That(oldEvent?.GetInvocationList().Any(handler => ReferenceEquals(handler.Target, input.Map)) ?? false, Is.False);
                    Assert.That(input.Map.World.Root.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>()
                        .Count(group => group.name.StartsWith("ThingArt_", StringComparison.Ordinal)), Is.EqualTo(1));
                    ids[cycle] = input.View.Simulation.Registry.Things.Single(thing => thing.TryGetComponent<WorkerComponent>(out _)).Id;
                    rootCounts[cycle] = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                        .Count(transform => transform.name == "LaboratoryWorld");
                    Assert.That(rootCounts[cycle], Is.EqualTo(1));
                    GC.Collect();
                    memory[cycle] = GC.GetTotalMemory(false);
                }
                Assert.That(ids.Distinct().Count(), Is.EqualTo(10));
                var releasedId = ids[ids.Length - 1];
                var releasedSimulation = input.View.Simulation;
                var releasedMap = input.Map;
                var path = input.View.SavePath;
                input.View.ReleaseSession();
                Assert.That(input.View.Controller, Is.Null);
                Assert.That(releasedMap.TryDisplayedPosition(releasedId, out _), Is.False);
                Assert.That(releasedMap.TryThingScreenPoint(releasedId, out _), Is.False);
                var fields = typeof(LaboratoryMapView);
                const System.Reflection.BindingFlags instancePrivate = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                Assert.That(((System.Collections.IDictionary)fields.GetField("thingArt", instancePrivate).GetValue(releasedMap)).Count, Is.Zero,
                    "Explicit session release must immediately release Thing-owned display and interpolation caches.");
                Assert.That(fields.GetField("artSimulation", instancePrivate).GetValue(releasedMap), Is.Null);
                var oldGatheringEvent = (Delegate)typeof(LaboratoryManager).GetField(nameof(LaboratoryManager.GatheringCompleted), instancePrivate)
                    .GetValue(releasedSimulation);
                Assert.That(oldGatheringEvent?.GetInvocationList().Any(handler => ReferenceEquals(handler.Target, releasedMap)) ?? false,
                    Is.False, "The released simulation must not retain this map's gathering-event subscription.");
                Assert.That(releasedMap.World.Root.gameObject.activeSelf, Is.False);
                yield return null; yield return null;
                Assert.That(releasedMap.World.Root.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true)
                    .Count(group => group.name.StartsWith("ThingArt_", StringComparison.Ordinal)), Is.Zero);
                Assert.That(input.View.InitializeSession(GameDataStore.Instance.Catalog, path, 30, SessionStart.Continue), Is.True);
                input.View.SetTransitionPaused(false);
                input.View.SimulationRate = 0;
                yield return null; yield return null;
                Assert.That(input.View.Simulation, Is.Not.SameAs(releasedSimulation));
                Assert.That(input.Map.World.Root.gameObject.activeSelf, Is.True);
                Assert.That(input.Map.TryDisplayedPosition(releasedId, out _), Is.True,
                    "A continued session must rebuild fresh display state, without keeping its previous simulation reference.");
                var finalMap = input.Map;
                UnityEngine.Object.DestroyImmediate(input.View.gameObject);
                yield return null; yield return null;
                Assert.That(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                    .Count(transform => transform.name == "LaboratoryWorld"), Is.Zero);
                Assert.That(finalMap == null, Is.True);
                Directory.CreateDirectory(MovementEvidenceRoot);
                File.WriteAllText(Path.Combine(MovementEvidenceRoot, "movement-session-lifetime.json"),
                    (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                        .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null,
                            new object[] { new { cycles = 10, rootCounts, explicitReleaseAndContinue = true, collectedManagedBytes = memory,
                                note = "After each isolated restart the old identity is absent from displayed-position and input publication, its body/cargo/selection root is destroyed, and exactly one map remains. Managed values are post-explicit-GC observations, not proof that all Editor caches are constant." } }));
            }
            LogAssert.NoUnexpectedReceived();
        }

        private static void SetMovementProperty(object target, string name, object value)
            => target.GetType().GetProperty(name).GetSetMethod(true).Invoke(target, new[] { value });
    }
}
#endif
