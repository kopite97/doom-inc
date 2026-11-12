#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        private struct PresentDiagnosticRow
        {
            public int frame, command;
            public double wallSeconds, simulationSeconds, frameDeltaMs;
            public long renderLoop, presentFrame, renderCpuCounter, mainCpuCounter, playerLoop, main, waitTarget, waitPresent, deviceWait;
            public uint timingCount;
            public ulong timingTimestamp;
            public bool freshTiming;
            public double cpuFrameMs, cpuMainMs, cpuPresentWaitMs, cpuRenderMs, gpuMs;
        }

        private sealed class PresentDiagnosticRecorders : IDisposable
        {
            public readonly string[] Names = { "RenderLoop", "Gfx.PresentFrame", "CPU Render Thread Frame Time",
                "CPU Main Thread Frame Time", "PlayerLoop", "Main Thread", "WaitForTargetFPS",
                "Gfx.WaitForPresentOnGfxThread", "GfxDeviceD3D12.WaitForLastPresent" };
            public readonly string[] Units;
            public readonly string[] Categories;
            public readonly string[] AvailableRelated;
            private readonly ProfilerRecorder[] recorders;
            public PresentDiagnosticRecorders()
            {
                var handles = new List<ProfilerRecorderHandle>();
                ProfilerRecorderHandle.GetAvailable(handles);
                recorders = new ProfilerRecorder[Names.Length];
                Units = new string[Names.Length]; Categories = new string[Names.Length];
                var related = new List<string>();
                foreach (var handle in handles)
                {
                    var description = ProfilerRecorderHandle.GetDescription(handle);
                    if (description.Name.Contains("Present") || description.Name.Contains("Render Thread Frame") || description.Name.Contains("WaitFor"))
                        related.Add(description.Name + " [" + description.Category + "] " + description.UnitType);
                    for (var i = 0; i < Names.Length; i++)
                    {
                        if (recorders[i].Valid || description.Name != Names[i]) continue;
                        recorders[i] = ProfilerRecorder.StartNew(description.Category, description.Name, 1);
                        Units[i] = description.UnitType.ToString(); Categories[i] = description.Category.ToString();
                    }
                }
                AvailableRelated = related.ToArray();
            }
            public long Read(int index) => recorders[index].Valid ? recorders[index].LastValue : -1;
            public bool[] Validity() => recorders.Select(item => item.Valid).ToArray();
            public void Dispose() { foreach (var recorder in recorders) recorder.Dispose(); }
        }

        [UnityTest, Category("PresentDiagnostic"), Timeout(300000), Explicit("Supplementary CPU diagnosis; does not replace the formal before/after benchmark.")]
        public IEnumerator CurrentRendererPresentAndActiveCpuDiagnostic()
        {
            const int warmupFrames = 1800;
            const int sampleFrames = 3600;
            var previousVSync = QualitySettings.vSyncCount;
            var previousFrameRate = Application.targetFrameRate;
            var previousCaptureRate = Time.captureFramerate;
            var runFile = Path.Combine(MovementEvidenceRoot, "native-comparison", "current-run.txt");
            var runName = File.Exists(runFile) ? File.ReadAllText(runFile).Trim() : null;
            if (runName != null && !System.Text.RegularExpressions.Regex.IsMatch(runName, @"^(baseline|after)-run[123]$"))
                throw new InvalidOperationException("Unexpected native comparison run name.");
            var evidenceRoot = runName == null ? MovementEvidenceRoot : Path.Combine(MovementEvidenceRoot, "native-comparison", runName);
            Directory.CreateDirectory(evidenceRoot);
            try
            {
                QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60; Time.captureFramerate = 0;
                using (var input = new PointerScene())
                {
                    yield return input.Load();
                    foreach (var level in new[] { 0, 2 })
                    {
                        var fixture = Path.Combine(MovementEvidenceRoot, "movement-fixture-" + level + ".json");
                        Assert.That(File.Exists(fixture), Is.True);
                        File.WriteAllText(input.View.SavePath, File.ReadAllText(fixture));
                        Assert.That(input.View.Reload(), Is.True);
                        input.View.SimulationRate = 0; input.View.SetUiPreferences(true, false);
                        yield return null; input.Map.Fit(); yield return null;
                        var timingsEnabled = FrameTimingManager.IsFeatureEnabled();
                        var timings = new FrameTiming[1];
                        var command = 0;
                        for (var i = 0; i < warmupFrames; i++)
                        {
                            AdvanceMovementCommand(input.View, command++);
                            if (timingsEnabled) FrameTimingManager.CaptureFrameTimings();
                            yield return null;
                        }
                        var rows = new PresentDiagnosticRow[sampleFrames];
                        ulong previousTiming = 0;
                        using (var recorders = new PresentDiagnosticRecorders())
                        {
                            var start = Time.realtimeSinceStartupAsDouble;
                            for (var i = 0; i < sampleFrames; i++)
                            {
                                AdvanceMovementCommand(input.View, command++);
                                if (timingsEnabled) FrameTimingManager.CaptureFrameTimings();
                                yield return null;
                                var count = timingsEnabled ? FrameTimingManager.GetLatestTimings(1, timings) : 0u;
                                var timing = count > 0 ? timings[0] : default;
                                rows[i] = new PresentDiagnosticRow { frame = Time.frameCount, command = command,
                                    wallSeconds = Time.realtimeSinceStartupAsDouble - start, simulationSeconds = input.View.Simulation.ElapsedSeconds,
                                    frameDeltaMs = Time.unscaledDeltaTime * 1000d, renderLoop = recorders.Read(0), presentFrame = recorders.Read(1),
                                    renderCpuCounter = recorders.Read(2), mainCpuCounter = recorders.Read(3), playerLoop = recorders.Read(4),
                                    main = recorders.Read(5), waitTarget = recorders.Read(6), waitPresent = recorders.Read(7), deviceWait = recorders.Read(8),
                                    timingCount = count, timingTimestamp = timing.frameStartTimestamp,
                                    freshTiming = count > 0 && timing.frameStartTimestamp != 0 && timing.frameStartTimestamp != previousTiming,
                                    cpuFrameMs = count > 0 ? timing.cpuFrameTime : -1,
                                    cpuMainMs = count > 0 ? timing.cpuMainThreadFrameTime : -1,
                                    cpuPresentWaitMs = count > 0 ? timing.cpuMainThreadPresentWaitTime : -1,
                                    cpuRenderMs = count > 0 ? timing.cpuRenderThreadFrameTime : -1,
                                    gpuMs = count > 0 ? timing.gpuFrameTime : -1 };
                                if (count > 0) previousTiming = timing.frameStartTimestamp;
                            }
                            var data = new { implementation = runName == null ? "after-only" : runName, formalComparison = false, level, warmupFrames, sampleFrames,
                                fixedDelta = 1d / 60, releaseEveryCommands = 600, elapsedWallSeconds = Time.realtimeSinceStartupAsDouble - start,
                                resolution = new[] { Screen.width, Screen.height }, zoom = input.Map.Zoom,
                                graphics = SystemInfo.graphicsDeviceType.ToString(), threadedGraphics = SystemInfo.graphicsMultiThreaded,
                                vSync = QualitySettings.vSyncCount, targetFrameRate = Application.targetFrameRate,
                                frameTimingFeatureEnabled = timingsEnabled, frameTimingCpuFrequency = FrameTimingManager.GetCpuTimerFrequency(),
                                names = recorders.Names, categories = recorders.Categories, units = recorders.Units, valid = recorders.Validity(),
                                availableRelated = recorders.AvailableRelated, rows,
                                note = "Supplementary renderer CPU comparison: same fixtures, 1800 warmup plus 3600 sample commands covering the same model-time 30 to 90 seconds as the formal full-scene benchmark. Native comparison run directories identify the independently verified product source hashes. ProfilerRecorder fields retain raw values and reported units. FrameTiming API durations are recorded in milliseconds per Scripting API; timingCount/freshTiming identify usable observations. No PlayerSettings or rendering configuration is changed. Missing counters are -1/invalid; zero or unavailable render-thread values, especially with threadedGraphics=false, do not prove no rendering work. No ad-hoc subtraction is performed. This test makes no allocation-zero claim." };
                            File.WriteAllText(Path.Combine(evidenceRoot, "present-diagnostic-level" + level + ".json"),
                                (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                                    .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { data }));
                        }
                    }
                }
            }
            finally
            {
                QualitySettings.vSyncCount = previousVSync; Application.targetFrameRate = previousFrameRate; Time.captureFramerate = previousCaptureRate;
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
