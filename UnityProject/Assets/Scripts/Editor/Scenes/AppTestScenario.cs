using System;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    // Preparation only: runs the normal command/time path into an isolated Editor slot.
    public static class AppTestScenario
    {
        [MenuItem("Tools/Mobile Game/Play/Prepare Operating Test Save")]
        public static void Operating() => PrepareSelected(false);
        [MenuItem("Tools/Mobile Game/Play/Prepare Near Ending Test Save")]
        public static void NearEnding() => PrepareSelected(true);
        private static void PrepareSelected(bool nearEnding)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Prepare outside Play Mode.");
            Prepare(GameDataAssets.LoadCatalog(), SceneFlowManager.DefaultSavePath, nearEnding);
            Debug.Log("Prepared " + (nearEnding ? "near ending" : "operating") + " scenario via commands/time in "
                + SceneFlowManager.DefaultSavePath + ". Choose Current Scene - Continue Test Save. This is preparation, not device input verification.");
        }
        public static void Prepare(GameDataCatalog data, string path, bool nearEnding)
        {
            // A memory store prevents a partially prepared scenario from replacing the selected slot.
            var memory = new PreparationStore();
            var organic = data.Selected<ResourceData>("organicId"); var culture = data.Selected<ResourceData>("cultureId");
            var gathering = data.Selected<FacilityData>("gatheringId"); var storage = data.Selected<FacilityData>("storageId");
            var processing = data.Selected<FacilityData>("processingId"); var cultivation = data.Selected<FacilityData>("cultivationId");
            var codec = new SessionSnapshotCodec(data.Definitions.Values, true);
            var flow = new GameFlowManager(codec, memory, () => LaboratorySessionFactory.Create(organic, gathering, storage,
                data.Selected<CreatureData>("prototypeId"), culture, data.Settings.Invasion));
            Require(flow.Restart()); var commands = new LaboratoryCommands(flow);
            var lab = (LaboratoryManager)flow.Current.Runtime;
            void Place(FacilityData definition, int x, int y)
            {
                Require(commands.BeginConstruction(definition));
                Require(commands.PreviewConstruction(new Vector2Int(x, y), QuarterTurn.None));
                Require(commands.ConfirmConstruction());
            }
            void Until(Func<bool> done, int seconds)
            {
                for (var i = 0; i < seconds * 2 && !done(); i++) flow.Advance(.5);
                if (!done()) throw new InvalidOperationException("Scenario preparation timed out; selected save unchanged.");
            }
            Place(gathering, 3, 3); Place(storage, 8, 3);
            Until(() => lab.Registry.Things.Any(t => t.Definition == storage && lab.TotalAt(t.Id) >= 5), 120);
            Place(processing, 3, 7); Place(cultivation, 8, 7); Place(storage, 12, 3);
            var stores = lab.Registry.Things.Where(t => t.Definition == storage).OrderBy(t => t.CreationOrder).ToArray();
            Require(commands.SetAllowed(stores[0].Id, culture, false)); Require(commands.SetAllowed(stores[1].Id, organic, false));
            Until(() => lab.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                .All(t => t.GetComponent<ConstructionComponent>().IsComplete), 200);
            var cultivator = lab.Registry.Things.Single(t => t.Definition == cultivation);
            Require(commands.SelectCultivation(cultivator.Id, CultivationTarget.Invader));
            Until(() => lab.Invasion.WaitingCount >= 2, 2400);
            if (nearEnding)
            {
                Require(commands.ReleaseInvaders());
                Until(() => lab.Invasion.State.Score >= data.Settings.Invasion.TargetScore * .9, 2400);
                if (flow.Phase != SessionPhase.Playing) throw new InvalidOperationException("Preparation crossed the goal; selected save unchanged.");
            }
            Require(flow.Save());
            new JsonFileSessionStore(Path.GetFullPath(path)).Write(memory.Read());
        }
        private static void Require(bool result) { if (!result) throw new InvalidOperationException("Scenario command rejected; selected save unchanged."); }
        private sealed class PreparationStore : ISessionStore
        {
            private string saved;
            public bool Exists => saved != null;
            public string Read() => saved;
            public void Write(string value) => saved = value;
        }
    }
}
