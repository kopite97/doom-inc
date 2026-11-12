#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation.Editor;
using MobileGame.TestSupport;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        // Explicit comparison scenario; same frozen fixture and deterministic simulation deltas on both renderers.
        [UnityTest, Category("RenderingPerformance"), Explicit("Historical PLAN-027 renderer comparison writes its evidence; run it only when explicitly reproducing that comparison.")]
        public IEnumerator RenderingComparisonRecordsBasicAndHeavyFixtures()
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/plans/evidence/PLAN-027"));
            Directory.CreateDirectory(root);
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var mode = input.Map.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>() != null
                    || GameObject.Find("LaboratoryWorld") != null ? "world" : "ugui";
                foreach (var level in new[] { 0, 2 })
                {
                    var fixturePath = Path.Combine(root, "fixture-" + level + ".json");
                    if (!File.Exists(fixturePath)) File.WriteAllText(fixturePath, Plan020Probe.Prepare(level));
                    for (var run = 1; run <= 3; run++)
                    {
                        File.WriteAllText(input.View.SavePath, File.ReadAllText(fixturePath));
                        Assert.That(input.View.Reload(), Is.True);
                        input.View.SimulationRate = 0;
                        input.View.SetUiPreferences(true, false);
                        input.Map.Fit();
                        // Pause model at a repeatable operational boundary, independent of host frame rate.
                        for (var i = 0; i < 100; i++) input.View.AdvanceFrame(.05, 0);
                        yield return new WaitForSecondsRealtime(5);
                        using (var main = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1))
                        using (var render = ProfilerRecorder.StartNew(ProfilerCategory.Render, "RenderLoop", 1))
                        using (var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1))
                        using (var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1))
                        {
                            var rows = new List<double[]>(10000);
                            var start = Time.realtimeSinceStartupAsDouble;
                            while (Time.realtimeSinceStartupAsDouble - start < 10)
                            {
                                yield return null;
                                rows.Add(new[] { (double)Time.frameCount, main.Valid ? main.LastValue / 1e6 : -1,
                                    render.Valid ? render.LastValue / 1e6 : -1, gc.Valid ? (double)gc.LastValue : -1,
                                    batches.Valid ? (double)batches.LastValue : -1 });
                            }
                            var data = new { mode, level, run, warmupSeconds = 5, sampleSeconds = 10,
                                resolution = new[] { Screen.width, Screen.height }, zoom = input.Map.Zoom,
                                mainValid = main.Valid, renderValid = render.Valid, gcValid = gc.Valid, batchesValid = batches.Valid,
                                objects = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length,
                                columns = new[] { "frame", "mainMs", "renderLoopMs", "gcBytes", "batches" }, rows,
                                note = "Editor Play Mode, frozen simulation at 5 s; Main includes waits and Editor overhead; RenderLoop measures the available rendering marker across cameras. Test sampling itself allocates a fixed double[5] per frame. No device claim." };
                            var json = (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                                .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { data });
                            File.WriteAllText(Path.Combine(root, mode + "-level" + level + "-run" + run + ".json"), json);
                        }
                        if (run == 1)
                            yield return AppSceneFlowTests.Capture("PLAN027-" + mode,
                                Path.Combine(root, mode + "-level" + level + ".png"));
                    }
                }
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
