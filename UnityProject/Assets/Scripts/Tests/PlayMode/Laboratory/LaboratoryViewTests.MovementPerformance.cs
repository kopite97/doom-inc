#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        // Explicitly selected long measurements. Every output states its workload and actual wall duration.
        [UnityTest, Category("MovementPerformance"), Explicit("Select this controlled performance scenario with include_explicit=true."), Timeout(600000)]
        public IEnumerator MovementComparisonRecordsBasicFixture() => MeasureMovementFixture(0, false);

        [UnityTest, Category("MovementPerformance"), Explicit("Select this controlled performance scenario with include_explicit=true."), Timeout(600000)]
        public IEnumerator MovementComparisonRecordsMediumFixture() => MeasureMovementFixture(1, false);

        [UnityTest, Category("MovementPerformance"), Explicit("Select this controlled performance scenario with include_explicit=true."), Timeout(600000)]
        public IEnumerator MovementComparisonRecordsHeavyFixture() => MeasureMovementFixture(2, false);

        [UnityTest, Category("MovementPerformance"), Explicit("Select this controlled performance scenario with include_explicit=true."), Timeout(300000)]
        public IEnumerator MovementComparisonRecordsMostlyOffscreenFixture() => MeasureMovementFixture(2, true);

        private struct MovementPerformanceRow
        {
            public int frame, step, workers, moving, waiting, carrying, visible;
            public double wallSeconds, simulationSeconds;
            public long main, playerLoop, render, canvasBuild, canvasSend, gc, batches;
            public long evaluate, draw, sort, status, waitForTargetFps, waitForPresent, pathQueries, visitedCells;
        }

        private sealed class MovementRecorders : IDisposable
        {
            private readonly ProfilerRecorder[] recorders;
            public readonly string[] Requested = { "Main Thread", "PlayerLoop", "RenderLoop", "Canvas.BuildBatch",
                "Canvas.SendWillRenderCanvases", "GC Allocated In Frame", "Batches Count",
                "Laboratory.Movement.Evaluate", "Laboratory.Map.DrawArt", "Laboratory.Map.Sort", "Laboratory.Status.Layout",
                "WaitForTargetFPS", "Gfx.WaitForPresentOnGfxThread" };
            public readonly string[] Matched;
            public readonly string[] Units;

            public MovementRecorders()
            {
                var handles = new List<ProfilerRecorderHandle>();
                ProfilerRecorderHandle.GetAvailable(handles);
                recorders = new ProfilerRecorder[Requested.Length];
                Matched = new string[Requested.Length];
                Units = new string[Requested.Length];
                for (var i = 0; i < Requested.Length; i++)
                {
                    foreach (var handle in handles)
                    {
                        var description = ProfilerRecorderHandle.GetDescription(handle);
                        if (!string.Equals(description.Name, Requested[i], StringComparison.Ordinal)) continue;
                        recorders[i] = ProfilerRecorder.StartNew(description.Category, description.Name, 1);
                        Matched[i] = description.Name;
                        Units[i] = description.UnitType.ToString();
                        break;
                    }
                }
            }

            public long Read(int index) => recorders[index].Valid ? recorders[index].LastValue : -1;
            public bool[] Validity() => recorders.Select(recorder => recorder.Valid).ToArray();
            public void Dispose() { foreach (var recorder in recorders) recorder.Dispose(); }
        }

        private static string MovementEvidenceRoot => Path.GetFullPath(Path.Combine(Application.dataPath,
            "../../docs/plans/evidence/PLAN-028"));

        private static string PrepareMovementFixture(int level)
        {
            var catalog = GameDataStore.Instance.Catalog;
            var codec = new SessionSnapshotCodec(catalog.Definitions.Values, true);
            var state = codec.Restore(codec.Deserialize(Plan020Probe.Prepare(level)));
            var laboratory = (LaboratoryManager)state.Runtime;
            if (level == 0)
            {
                // One operational worker with capacity for the full measured interval, using unchanged product definitions.
                var definitions = new[] { catalog.Selected<FacilityData>("gatheringId"), catalog.Selected<FacilityData>("storageId"), catalog.Selected<FacilityData>("storageId") };
                var positions = new[] { new Vector2Int(3, 3), new Vector2Int(8, 3), new Vector2Int(12, 6) };
                for (var i = 0; i < definitions.Length; i++)
                {
                    var thing = laboratory.Registry.Create(definitions[i]);
                    Assert.That(laboratory.Map.Commands.Place(thing.Id, positions[i]).Succeeded, Is.True);
                    var construction = thing.GetComponent<ConstructionComponent>();
                    SetMovementFixtureProperty(construction, nameof(ConstructionComponent.Delivered), definitions[i].BuildCost);
                    SetMovementFixtureProperty(construction, nameof(ConstructionComponent.Progress), (double)definitions[i].BuildSeconds);
                    SetMovementFixtureProperty(construction, nameof(ConstructionComponent.IsComplete), true);
                }
            }
            // Real production assignments sustain hauling. Invader cultivation keeps worker counts at 1/4/24.
            foreach (var thing in state.Registry.Things)
                if (thing.Definition is FacilityData definition && definition.Role == FacilityRole.Cultivation)
                    Assert.That(laboratory.SelectCultivation(thing.Id, CultivationTarget.Invader), Is.True);
            var operators = state.Registry.Things.Where(thing => thing.Definition is CreatureData creature && creature.CanOperate)
                .OrderBy(thing => thing.CreationOrder).ToArray();
            var production = state.Registry.Things.Where(thing => thing.Definition is FacilityData facility && facility.HasProduction)
                .OrderBy(thing => thing.CreationOrder).ToArray();
            for (var i = 0; i < production.Length; i++) Assert.That(laboratory.AssignWorker(operators[i].Id, production[i].Id), Is.True);
            laboratory.ValidateState();
            return codec.Serialize(codec.Capture(state, SessionPhase.Playing));
        }

        private static void SetMovementFixtureProperty(object target, string name, object value)
            => target.GetType().GetProperty(name).GetSetMethod(true).Invoke(target, new[] { value });

        private static void AdvanceMovementCommand(LaboratoryView view, int commandIndex)
        {
            // Use the same existing game command at the same indices in every pre/post run.
            if ((commandIndex + 1) % 600 == 0) view.Simulation.Invasion.ReleaseAll();
            view.AdvanceFrame(1d / 60, 0);
        }

        private IEnumerator MeasureMovementFixture(int level, bool mostlyOffscreen)
        {
            const double delta = 1d / 60;
            const double warmupSeconds = 30;
            const double sampleSeconds = 60;
            const int warmupSteps = 1800;
            const int sampleSteps = 3600;
            var root = MovementEvidenceRoot;
            Directory.CreateDirectory(root);
            var stagePath = Path.Combine(root, "measurement-stage.txt");
            Assert.That(File.Exists(stagePath), Is.True, "Set measurement-stage.txt to baseline or after before selecting this measurement.");
            var stage = File.ReadAllText(stagePath).Trim();
            Assert.That(stage, Is.EqualTo("baseline").Or.EqualTo("after"));
            var fixturePath = Path.Combine(root, "movement-fixture-" + level + ".json");
            var previousVSync = QualitySettings.vSyncCount;
            var previousFrameRate = Application.targetFrameRate;
            var previousCaptureRate = Time.captureFramerate;
            try
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                Time.captureFramerate = 0;
                using (var input = new PointerScene())
                {
                    yield return input.Load();
                    if (!File.Exists(fixturePath))
                    {
                        Assert.That(stage, Is.EqualTo("baseline"), "An after measurement must reuse the actual baseline fixture.");
                        File.WriteAllText(fixturePath, PrepareMovementFixture(level));
                    }
                    var fixture = File.ReadAllText(fixturePath);
                    for (var run = 1; run <= (mostlyOffscreen ? 1 : 3); run++)
                    {
                        File.WriteAllText(input.View.SavePath, fixture);
                        Assert.That(input.View.Reload(), Is.True);
                        input.View.SimulationRate = 0;
                        input.View.SetUiPreferences(true, false);
                        // Reload increments the generation. Let CheckLayout reset its camera before applying this run's zoom.
                        yield return null;
                        input.Map.Fit();
                        if (mostlyOffscreen) input.Map.ZoomBy(8);
                        yield return null;
                        Assert.That(input.Map.Zoom, Is.EqualTo(mostlyOffscreen ? 8 : 1));
                        var simulation = input.View.Simulation;
                        var workers = simulation.Registry.Things.Where(thing => thing.IsPlaced
                            && thing.TryGetComponent<WorkerComponent>(out _)).OrderBy(thing => thing.CreationOrder).ToArray();
                        Assert.That(workers.Length, Is.EqualTo(level == 0 ? 1 : level == 1 ? 4 : 24));
                        var previousCells = new Vector2Int[workers.Length];
                        var workerStates = workers.Select(thing => thing.GetComponent<WorkerComponent>()).ToArray();
                        var bodies = new SpriteRenderer[workers.Length];
                        for (var i = 0; i < workers.Length; i++) previousCells[i] = workers[i].Placement.Origin;
                        // Fixed 1/60 delta, with the existing release command at indices 599,1199,...,5399.
                        var warmStart = Time.realtimeSinceStartupAsDouble;
                        var step = 0;
                        while (step < warmupSteps)
                        {
                            AdvanceMovementCommand(input.View, step);
                            step++;
                            yield return null;
                        }
                        var actualWarmupSeconds = Time.realtimeSinceStartupAsDouble - warmStart;
                        for (var i = 0; i < workers.Length; i++) bodies[i] = input.Map.World.Root
                            .Find("ThingArt_" + workers[i].Id.ToString("N") + "/Body").GetComponent<SpriteRenderer>();
                        // Capacity is fixed before collection; no row objects, lists, strings or JSON are allocated per frame.
                        var rows = new MovementPerformanceRow[sampleSteps];
                        var count = 0;
                        var memoryBefore = GC.GetTotalMemory(false);
                        var unityMemoryBefore = Profiler.GetTotalAllocatedMemoryLong();
                        var collectionsBefore = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
                        var modelStart = simulation.ElapsedSeconds;
                        var pathsBefore = simulation.Map.PathQueries;
                        var visitedBefore = simulation.Map.VisitedCells;
                        using (var recorders = new MovementRecorders())
                        {
                            var start = Time.realtimeSinceStartupAsDouble;
                            while (count < sampleSteps)
                            {
                                AdvanceMovementCommand(input.View, step);
                                step++;
                                yield return null;
                                if (count >= rows.Length) throw new InvalidOperationException("Increase preallocated sample capacity before a new complete comparison.");
                                var moving = 0; var waiting = 0; var carrying = 0; var visible = 0;
                                for (var i = 0; i < workers.Length; i++)
                                {
                                    var thing = workers[i];
                                    var worker = workerStates[i];
                                    if (thing.Placement.Origin != previousCells[i] || worker.MoveProgress > 0) moving++;
                                    previousCells[i] = thing.Placement.Origin;
                                    if (worker.WaitReason != WorkerWaitReason.None || worker.Job?.Stage == JobStage.Waiting) waiting++;
                                    if (worker.Job?.ResourceId != null && simulation.Registry.TryGet(worker.Job.ResourceId.Value, out var cargo)
                                        && cargo.Exists && cargo.HolderId == thing.Id && cargo.Definition.Kind == ThingKind.Resource) carrying++;
                                    var camera = input.Map.World.Camera;
                                    var bounds = bodies[i].bounds;
                                    var screenMin = camera.WorldToScreenPoint(bounds.min);
                                    var screenMax = camera.WorldToScreenPoint(bounds.max);
                                    if (bodies[i].enabled && bodies[i].gameObject.activeInHierarchy
                                        && Rect.MinMaxRect(screenMin.x, screenMin.y, screenMax.x, screenMax.y).Overlaps(camera.pixelRect)) visible++;
                                }
                                rows[count++] = new MovementPerformanceRow { frame = Time.frameCount, step = step,
                                    wallSeconds = Time.realtimeSinceStartupAsDouble - start, simulationSeconds = simulation.ElapsedSeconds,
                                    workers = workers.Length, moving = moving, waiting = waiting, carrying = carrying, visible = visible,
                                    main = recorders.Read(0), playerLoop = recorders.Read(1), render = recorders.Read(2),
                                    canvasBuild = recorders.Read(3), canvasSend = recorders.Read(4), gc = recorders.Read(5),
                                    batches = recorders.Read(6), evaluate = recorders.Read(7), draw = recorders.Read(8),
                                    sort = recorders.Read(9), status = recorders.Read(10), waitForTargetFps = recorders.Read(11),
                                    waitForPresent = recorders.Read(12), pathQueries = simulation.Map.PathQueries,
                                    visitedCells = simulation.Map.VisitedCells };
                            }
                            var actualSampleSeconds = Time.realtimeSinceStartupAsDouble - start;
                            var meanVisible = 0d;
                            for (var i = 0; i < count; i++) meanVisible += rows[i].visible;
                            meanVisible /= count;
                            var memoryAfter = GC.GetTotalMemory(false);
                            var unityMemoryAfter = Profiler.GetTotalAllocatedMemoryLong();
                            var collectionsAfter = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
                            Assert.That(count, Is.GreaterThan(0));
                            simulation.ValidateState();
                            var label = stage + "-level" + level + (mostlyOffscreen ? "-offscreen" : "") + "-run" + run;
                            var data = new { stage, level, run, mostlyOffscreen, meanVisible, warmupSeconds, actualWarmupSeconds, releaseEverySteps = 600,
                                sampleSeconds, actualSampleSeconds, warmupSteps, sampleSteps, fixedSimulationDelta = delta, simulationSteps = step,
                                sampleCount = count, modelStart, modelEnd = simulation.ElapsedSeconds,
                                resolution = new[] { Screen.width, Screen.height }, zoom = input.Map.Zoom,
                                vSync = QualitySettings.vSyncCount, targetFrameRate = Application.targetFrameRate,
                                recorderNames = recorders.Requested, matchedRecorders = recorders.Matched,
                                recorderUnits = recorders.Units, recorderValid = recorders.Validity(),
                                memoryBefore, memoryAfter, unityMemoryBefore, unityMemoryAfter,
                                collectionsBefore, collectionsAfter, pathsBefore, visitedBefore,
                                pathsAfter = simulation.Map.PathQueries, visitedAfter = simulation.Map.VisitedCells,
                                activeTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length,
                                rows = rows.Take(count).ToArray(),
                                note = "Editor Play Mode. Profiler durations are in recorded units (usually nanoseconds). Main Thread includes frame waits and Editor overhead; PlayerLoop and available wait markers are separate and must not be summed as exclusive samples. Every repeat advances exactly 1800 warmup and 3600 collection commands of 1/60 simulation seconds, one per rendered frame; 60fps cap targets 30/60 wall seconds, whose actual durations are reported separately. Sampling row storage and renderer references are preallocated; TestRunner coroutine and existing product allocations remain in full-frame GC. Unknown recorders are -1 and invalid, never zero-cost. Visible counts use current Body SpriteRenderer screen bounds intersected with the active map camera pixelRect. No device claim." };
                            var json = (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                                .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { data });
                            File.WriteAllText(Path.Combine(root, label + ".json"), json);
                            if (mostlyOffscreen) Assert.That(meanVisible, Is.LessThan(workers.Length * .5), "The offscreen condition must actually hide most worker bodies.");
                            if (run == 1) yield return AppSceneFlowTests.Capture("PLAN028-" + label, Path.Combine(root, label + ".png"));
                        }
                    }
                }
            }
            finally
            {
                QualitySettings.vSyncCount = previousVSync;
                Application.targetFrameRate = previousFrameRate;
                Time.captureFramerate = previousCaptureRate;
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
