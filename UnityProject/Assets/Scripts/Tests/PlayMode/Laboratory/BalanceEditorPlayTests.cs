#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace MobileGame.SpatialTests
{
    public sealed class BalanceEditorPlayTests
    {
        [UnityTest] public IEnumerator SavedBalanceLoadsThroughRuntimeAndControlsConstruction()
        {
            if (GameDataStore.Instance != null) UnityEngine.Object.Destroy(GameDataStore.Instance.gameObject);
            yield return null;
            var config = BalanceEditorData.Configuration;
            var loading = config.PrepareAsync();
            for (var i = 0; i < 180 && !loading.IsCompleted; i++) yield return null;
            Assert.That(loading.IsCompletedSuccessfully, Is.True, loading.Exception?.ToString());
            var data = loading.Result;
            var source = GameDataAssets.LoadCatalog();
            Assert.That(data.Settings.CompatibilityHash, Is.EqualTo(source.Settings.CompatibilityHash));
            var gathering = data.Selected<FacilityData>("gatheringId");
            Assert.That(gathering.BuildSeconds, Is.EqualTo(source.Selected<FacilityData>("gatheringId").BuildSeconds));
            var folder = Path.Combine(Path.GetTempPath(), "BalancePlay-" + Guid.NewGuid().ToString("N"));
            try
            {
                var flow = new GameFlowManager(new SessionSnapshotCodec(data.Definitions.Values, true), new JsonFileSessionStore(Path.Combine(folder, "session.json")),
                    () => LaboratorySessionFactory.Create(data.Selected<ResourceData>("organicId"), gathering, data.Selected<FacilityData>("storageId"),
                        data.Selected<CreatureData>("prototypeId"), data.Selected<ResourceData>("cultureId"), data.Settings.Invasion));
                Assert.That(flow.Restart(), Is.True); var commands = new LaboratoryCommands(flow);
                Assert.That(commands.BeginConstruction(gathering), Is.True);
                Assert.That(commands.PreviewConstruction(new Vector2Int(3, 3), QuarterTurn.None), Is.True);
                Assert.That(commands.ConfirmConstruction(), Is.True);
                var thing = flow.Current.Registry.Things.Single(t => t.Definition == gathering);
                var construction = thing.GetComponent<ConstructionComponent>();
                for (var i = 0; i < 1000 && construction.Progress == 0; i++) flow.Advance(.01);
                Assert.That(construction.Progress, Is.GreaterThan(0));
                var remaining = gathering.BuildSeconds - construction.Progress;
                Assert.That(remaining, Is.GreaterThan(.01));
                flow.Advance(remaining / 2); Assert.That(construction.IsComplete, Is.False);
                for (var i = 0; i < 1000 && !construction.IsComplete; i++) flow.Advance(.05);
                Assert.That(construction.IsComplete, Is.True);
                Assert.That(construction.Progress, Is.EqualTo(gathering.BuildSeconds).Within(.00001));
                Debug.Log("PLAN043 runtime construction seconds=" + gathering.BuildSeconds + ", hash=" + data.Settings.CompatibilityHash);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<FacilityDefinition>("Assets/SO/Facility/Gathering.asset");
                var window = BalanceEditorWindow.OpenDefinition(asset);
                var fields = window.rootVisualElement.Query<TextField>().ToList().Where(f => f.label != "이름 / ID 검색").ToArray();
                Assert.That(fields.Length, Is.GreaterThan(0)); Assert.That(fields.All(f => !f.enabledInHierarchy), Is.True);
                var before = File.ReadAllBytes("Assets/JSON/Balance/Facilities.json"); window.SaveDraft();
                Assert.That(File.ReadAllBytes("Assets/JSON/Balance/Facilities.json"), Is.EqualTo(before));
                window.Close();
            }
            finally
            {
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
                if (GameDataStore.Instance != null) UnityEngine.Object.Destroy(GameDataStore.Instance.gameObject);
            }
            yield return null;
        }
    }
}
#endif
