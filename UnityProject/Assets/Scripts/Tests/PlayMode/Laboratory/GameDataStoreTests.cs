#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed class GameDataStoreTests
    {
        private GameObject owner;
        private string directory;
        [UnitySetUp] public IEnumerator Prepare()
        {
            if (GameDataStore.Instance != null) UnityEngine.Object.Destroy(GameDataStore.Instance.gameObject);
            yield return null;
            directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN012-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (owner != null) UnityEngine.Object.Destroy(owner);
            if (GameDataStore.Instance != null) UnityEngine.Object.Destroy(GameDataStore.Instance.gameObject);
            yield return null;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        private static IEnumerator Wait(Task task)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 10;
            while (!task.IsCompleted && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(task.IsCompleted, Is.True, "Async loading timed out.");
        }
        [UnityTest] public IEnumerator ConcurrentRequestsShareOneLoadAndDuplicatesCannotDestroyTheCatalog()
        {
            var catalog = GameDataAssets.LoadCatalog(); var gate = new TaskCompletionSource<GameDataCatalog>();
            var store = GameDataStore.GetOrCreate(); var calls = 0;
            var first = store.InitializeAsync(token => { calls++; return gate.Task; });
            var second = store.InitializeAsync(token => throw new Exception("Duplicate load"));
            Assert.That(second, Is.SameAs(first)); Assert.That(store.State, Is.EqualTo(GameDataLoadState.Loading));
            Assert.That(store.Catalog, Is.Null); yield return null; Assert.That(calls, Is.EqualTo(1));
            gate.SetResult(catalog); yield return Wait(first); Assert.That(first.IsFaulted, Is.False);
            var duplicate = new GameObject("DuplicateStore").AddComponent<GameDataStore>(); yield return null;
            Assert.That(duplicate == null, Is.True); Assert.That(GameDataStore.Instance, Is.SameAs(store));
            Assert.That(store.Catalog, Is.SameAs(catalog));
            var reused = store.InitializeAsync(token => throw new Exception("Ready store must not reload"));
            Assert.That(reused.Result, Is.SameAs(catalog));
        }
        [UnityTest] public IEnumerator FailureCanRetryAndDestroyedLoaderCannotPublishLateResults()
        {
            var catalog = GameDataAssets.LoadCatalog(); var store = GameDataStore.GetOrCreate();
            var failed = store.InitializeAsync(token => throw new FormatException("bad.csv: line 2")); yield return Wait(failed);
            Assert.That(failed.IsFaulted, Is.True); Assert.That(store.Catalog, Is.Null); Assert.That(store.Error, Does.Contain("bad.csv"));
            var retry = store.InitializeAsync(token => Task.FromResult(catalog)); yield return Wait(retry);
            Assert.That(store.State, Is.EqualTo(GameDataLoadState.Ready));
            UnityEngine.Object.Destroy(store.gameObject); yield return null;
            store = GameDataStore.GetOrCreate(); var gate = new TaskCompletionSource<GameDataCatalog>(); CancellationToken captured = default;
            var old = store.InitializeAsync(token => { captured = token; return gate.Task; }); yield return null;
            UnityEngine.Object.Destroy(store.gameObject); yield return null;
            Assert.That(captured.IsCancellationRequested, Is.True);
            var replacement = GameDataStore.GetOrCreate(); var fresh = replacement.InitializeAsync(token => Task.FromResult(catalog));
            gate.SetResult(catalog); yield return Wait(old); yield return Wait(fresh);
            Assert.That(old.IsCanceled, Is.True); Assert.That(GameDataStore.Instance, Is.SameAs(replacement));
            Assert.That(replacement.Catalog, Is.SameAs(catalog));
        }
        [UnityTest] public IEnumerator BootstrapWaitsForWholeBundlePreservesSaveOnErrorAndRetries()
        {
            owner = new GameObject("IsolatedBootstrap"); var view = owner.AddComponent<LaboratoryView>();
            GameDataAssets.Configure(owner); var bootstrap = owner.GetComponent<GameBootstrap>();
            var path = Path.Combine(directory, "session.json"); File.WriteAllText(path, "original save bytes");
            bootstrap.SetSaveSlotBeforeStart(path, 0.01);
            var source = new DelayedSource(); var loading = bootstrap.PrepareAsync(source); var start = bootstrap.StartSessionAsync();
            yield return null; yield return null;
            Assert.That(view.Controller, Is.Null); Assert.That(File.ReadAllText(path), Is.EqualTo("original save bytes"));
            source.Gate.SetException(new FormatException("remote.csv: line 2")); yield return Wait(loading); yield return Wait(start);
            Assert.That(bootstrap.Error, Does.Contain("remote.csv")); Assert.That(view.Controller, Is.Null);
            Assert.That(File.ReadAllText(path), Is.EqualTo("original save bytes"));
            var retry = bootstrap.StartSessionAsync(); yield return Wait(retry);
            Assert.That(bootstrap.Ready, Is.True); Assert.That(view.Controller.Current, Is.Null, "Invalid save must not auto-start a run.");
            Assert.That(File.ReadAllText(path), Is.EqualTo("original save bytes"));
            Assert.That(view.Controller.Restart(), Is.True, "Explicit restart may replace invalid save after a successful write.");
            var data = GameDataStore.Instance.Catalog; var first = view.Controller.Current;
            Assert.That(view.Controller.Restart(), Is.True);
            Assert.That(view.Controller.Current, Is.Not.SameAs(first)); Assert.That(GameDataStore.Instance.Catalog, Is.SameAs(data));
            LogAssert.NoUnexpectedReceived();
        }
        private sealed class DelayedSource : ICsvDataSource
        {
            public readonly TaskCompletionSource<CsvBundle> Gate = new TaskCompletionSource<CsvBundle>();
            public Task<CsvBundle> LoadAsync(CancellationToken token) => Gate.Task;
        }
    }
}
#endif
