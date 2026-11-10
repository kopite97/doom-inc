#if UNITY_EDITOR || (UNITY_ANDROID && DEVELOPMENT_BUILD)
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MobileGame.Foundation;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace MobileGame.Presentation
{
    // PLAN-025 diagnostics only. No game-state injection; excluded from the final Player.
    public sealed partial class Plan025PlayerProbe : MonoBehaviour
    {
        [Serializable] private sealed class Request
        {
            public string id;
            public double warmupSeconds;
            public double sampleSeconds;
        }
        [Serializable] private sealed class State
        {
            public string utc, scene, phase, dataHash, saveSha256, error;
            public double realtime, gameTime, simulationRate;
            public int frame, width, height, facilities, creatures, external;
            public long managedBytes, unityAllocatedBytes, unityReservedBytes;
            public bool paused;
        }
        [Serializable] private sealed class Result
        {
            public string unityVersion, status, gcCounter = "Memory/GC Allocated In Frame (bytes)";
            public bool development, gcCounterValid, interrupted;
            public int sampleCount;
            public Request request;
            public State beforeWarmup, afterSampling;
        }
        private struct Sample
        {
            public int frame;
            public double realtime, gameTime, updateIntervalMs;
            public long gcBytes;
            public bool gcValid, paused;
        }
        private string root, runDirectory;
        private double nextPoll, sampleStart, sampleEnd;
        private Sample[] samples;
        private int count;
        private Result result;
        private ProfilerRecorder gc;

#if UNITY_ANDROID && DEVELOPMENT_BUILD && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var owner = new GameObject("PLAN025 Player Probe");
            DontDestroyOnLoad(owner);
            owner.AddComponent<Plan025PlayerProbe>();
        }
#endif
        private void Awake()
        {
            root = Path.Combine(Application.persistentDataPath, "PLAN-025-Probe");
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "ready.json"), JsonUtility.ToJson(Capture(), true));
        }
        private void LateUpdate()
        {
            if (result == null && PollUiScenario()) return;
            var now = Time.realtimeSinceStartupAsDouble;
            if (result != null)
            {
                if (now >= sampleEnd) { Finish("completed"); return; }
                if (now < sampleStart) return;
                if (count == samples.Length) { Finish("sample capacity exceeded"); return; }
                var view = SceneFlowManager.Instance?.Laboratory;
                samples[count++] = new Sample {
                    frame = Time.frameCount, realtime = now, gameTime = view?.Invasion?.Elapsed ?? -1,
                    updateIntervalMs = Time.unscaledDeltaTime * 1000.0,
                    gcValid = gc.Valid, gcBytes = gc.Valid ? gc.LastValue : -1,
                    paused = view?.Controller?.IsApplicationPaused ?? true
                };
                return;
            }
            if (now < nextPoll) return;
            nextPoll = now + 1;
            var input = Path.Combine(root, "request.json");
            if (!File.Exists(input)) return;
            try
            {
                var request = JsonUtility.FromJson<Request>(File.ReadAllText(input));
                if (request == null || string.IsNullOrEmpty(request.id) || request.id.Length > 80
                    || request.id.Any(c => !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '-' && c != '_')
                    || !double.IsFinite(request.warmupSeconds) || request.warmupSeconds < 0 || request.warmupSeconds > 60
                    || !double.IsFinite(request.sampleSeconds) || request.sampleSeconds < 1 || request.sampleSeconds > 120)
                    throw new InvalidOperationException("Invalid PLAN-025 sample request.");
                runDirectory = Path.Combine(root, request.id);
                if (Directory.Exists(runDirectory)) throw new InvalidOperationException("Sample ID already exists.");
                Directory.CreateDirectory(runDirectory);
                File.Move(input, Path.Combine(runDirectory, "request.json"));
                // Allocate before warmup. Sampling stores structs, with no per-frame file writes or formatting.
                samples = new Sample[36000]; count = 0;
                gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
                result = new Result { unityVersion = Application.unityVersion, development = Debug.isDebugBuild,
                    status = "running", request = request, beforeWarmup = Capture(), gcCounterValid = gc.Valid };
                sampleStart = Time.realtimeSinceStartupAsDouble + request.warmupSeconds;
                sampleEnd = sampleStart + request.sampleSeconds;
                File.WriteAllText(Path.Combine(runDirectory, "started.json"), JsonUtility.ToJson(result, true));
            }
            catch (Exception error)
            {
                if (result != null) Finish("error: " + error.Message);
                else { gc.Dispose(); samples = null; }
                File.WriteAllText(Path.Combine(root, "last-error.txt"), error.ToString());
                if (File.Exists(input)) File.Move(input, Path.Combine(root, "rejected-" + DateTime.UtcNow.Ticks + ".json"));
            }
        }
        private void OnApplicationPause(bool paused)
        {
            RecordUiPause(paused);
            if (paused && result != null) { result.interrupted = true; Finish("interrupted by application pause"); }
        }
        private void OnDestroy()
        {
            uiOwner?.Dispose();
            if (result != null) Finish("interrupted by probe destruction");
            gc.Dispose();
        }
        private void Finish(string status)
        {
            var completed = result;
            result = null;
            completed.status = status; completed.sampleCount = count; completed.afterSampling = Capture();
            gc.Dispose();
            using (var writer = new StreamWriter(Path.Combine(runDirectory, "samples.csv")))
            {
                writer.WriteLine("frame,realtime_seconds,game_seconds,unity_update_interval_ms,gc_allocated_bytes,gc_valid,application_paused");
                for (var i = 0; i < count; i++)
                {
                    var s = samples[i];
                    writer.WriteLine(string.Join(",", s.frame.ToString(CultureInfo.InvariantCulture),
                        s.realtime.ToString("R", CultureInfo.InvariantCulture), s.gameTime.ToString("R", CultureInfo.InvariantCulture),
                        s.updateIntervalMs.ToString("R", CultureInfo.InvariantCulture), s.gcBytes.ToString(CultureInfo.InvariantCulture),
                        s.gcValid ? "true" : "false", s.paused ? "true" : "false"));
                }
            }
            File.WriteAllText(Path.Combine(runDirectory, "result.json"), JsonUtility.ToJson(completed, true));
            samples = null;
        }
        private static State Capture()
        {
            var flow = SceneFlowManager.Instance; var view = flow?.Laboratory;
            var state = new State { utc = DateTime.UtcNow.ToString("O"), realtime = Time.realtimeSinceStartupAsDouble,
                frame = Time.frameCount, scene = SceneManager.GetActiveScene().path,
                phase = view?.Controller?.Phase.ToString(), gameTime = view?.Invasion?.Elapsed ?? -1,
                simulationRate = view?.SimulationRate ?? -1, paused = view?.Controller?.IsApplicationPaused ?? true,
                width = Screen.width, height = Screen.height,
                facilities = view?.Simulation?.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility) ?? 0,
                creatures = view?.Simulation?.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature) ?? 0,
                external = view?.Simulation?.Invasion.ExternalCount ?? 0,
                dataHash = GameDataStore.Instance?.Catalog?.Settings.CompatibilityHash,
                managedBytes = GC.GetTotalMemory(false), unityAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(),
                unityReservedBytes = Profiler.GetTotalReservedMemoryLong(), error = flow?.Error };
            if (flow != null && File.Exists(flow.SavePath))
            {
                try
                {
                    using (var file = File.OpenRead(flow.SavePath))
                    using (var sha = SHA256.Create()) state.saveSha256 = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
                }
                catch (IOException error) { state.error = "Save hash read: " + error.Message; }
            }
            return state;
        }
    }
}
#endif
