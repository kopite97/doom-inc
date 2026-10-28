using System;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Temporary presentation only: all commands, production and time gating belong to the shared runtime.
    public sealed partial class LaboratoryView : MonoBehaviour
    {
        [SerializeField] private ResourceDefinition organic;
        [SerializeField] private FacilityDefinition gathering;
        [SerializeField] private FacilityDefinition storage;
        [SerializeField] private CreatureDefinition prototype;
        [SerializeField] private ResourceDefinition culture;
        [SerializeField] private FacilityDefinition processing;
        [SerializeField] private FacilityDefinition cultivation;
        [SerializeField] private CreatureDefinition support;
        [SerializeField] private CreatureDefinition invader;
        [SerializeField] private InvasionDefinition invasion;
        private bool skipNextDelta = true;
        private bool applicationPaused;
        private bool applicationFocused = true;
        private double simulationRate = 1;

        public GameFlowManager Controller { get; private set; }
        public LaboratoryCommands Commands { get; private set; }
        public RunEndingPresentation Ending { get; private set; }
        public LaboratoryManager Simulation => Controller?.Current?.Runtime as LaboratoryManager;
        public InvasionState Invasion => Simulation?.Invasion.State;
        public bool ReportsExpanded { get; private set; }
        public Guid SelectedId { get; private set; }
        public string SavePath { get; private set; }
        public string LastCommand { get; private set; }
        public int RepaintCount { get; private set; }
        public bool IsEditing => Simulation != null && (Simulation.ConstructionDefinition != null
            || Controller.Current.Map.Commands.IsEditing);

        // Harness time control uses the same Advance path as normal Update; default gameplay speed is 1.
        public double SimulationRate
        {
            get => simulationRate;
            set
            {
                if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                simulationRate = value;
            }
        }

        private void Start()
        {
            if (Controller == null)
                Initialize(organic, gathering, storage, prototype,
                    Path.Combine(Application.persistentDataPath, "Gameplay", "session.json"));
        }

        public void Initialize(ResourceDefinition organicDefinition, FacilityDefinition gatheringDefinition,
            FacilityDefinition storageDefinition, CreatureDefinition prototypeDefinition,
            string savePath, double autoSaveSeconds = 5)
        {
            if (Controller != null) throw new InvalidOperationException("This view already has a session.");
            if (string.IsNullOrWhiteSpace(savePath)) throw new ArgumentException("Provide a save slot.", nameof(savePath));
            organic = organicDefinition;
            gathering = gatheringDefinition;
            storage = storageDefinition;
            prototype = prototypeDefinition;
            SavePath = Path.GetFullPath(savePath);
            var codec = new SessionSnapshotCodec(new ThingDefinition[]
                { organic, gathering, storage, prototype, culture, processing, cultivation, support, invader }.Where(d => d != null),
                requireLaboratory: true);
            Controller = new GameFlowManager(codec, new JsonFileSessionStore(SavePath),
                () => LaboratorySessionFactory.Create(organic, gathering, storage, prototype, culture, invasion?.CreateSettings()), autoSaveSeconds);
            Commands = new LaboratoryCommands(Controller);
            Ending = new RunEndingPresentation(Controller);
            Controller.SessionChanged += OnSessionChanged;
            Record("Start / Continue", Controller.StartOrContinue());
            Controller.NotifyApplicationPause(applicationPaused || !applicationFocused);
            UpdateEndingVisibility();
        }

        private void OnEnable() { skipNextDelta = true; UpdateEndingVisibility(); }
        private void OnDisable() { skipNextDelta = true; Ending?.SetVisible(false); }

        private void Update()
        {
            if (Controller == null || applicationPaused || !applicationFocused) return;
            if (skipNextDelta) { skipNextDelta = false; return; }
            AdvanceFrame(Time.unscaledDeltaTime * simulationRate, Time.unscaledDeltaTime);
        }

        public void AdvanceFrame(double gameSeconds, double presentationSeconds)
        {
            if (Controller == null || !isActiveAndEnabled || applicationPaused || !applicationFocused) return;
            Controller.Advance(gameSeconds);
            SynchronizeEnding();
            Ending.Advance(presentationSeconds);
        }

        private void SynchronizeEnding()
        {
            Ending?.Synchronize();
            if (Ending?.IsEnding != true) return;
            if (Simulation?.ConstructionDefinition != null) Commands.CancelConstruction();
            if (Controller.Current.Map.Commands.IsEditing) Controller.CancelPlacement();
            SelectedId = Guid.Empty;
            ReportsExpanded = false;
        }

        private void UpdateEndingVisibility() => Ending?.SetVisible(isActiveAndEnabled && !applicationPaused && applicationFocused);

        private void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            skipNextDelta = true;
            Controller?.NotifyApplicationPause(paused || !applicationFocused);
            UpdateEndingVisibility();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (applicationFocused == focused) return;
            applicationFocused = focused;
            skipNextDelta = true;
            Controller?.NotifyApplicationPause(applicationPaused || !focused);
            UpdateEndingVisibility();
        }

        private void OnApplicationQuit() => Controller?.NotifyApplicationQuit();
        private void OnDestroy()
        {
            Ending?.Dispose();
            if (Controller != null) Controller.SessionChanged -= OnSessionChanged;
        }

        private void OnSessionChanged()
        {
            SelectedId = Guid.Empty;
            ReportsExpanded = false;
            skipNextDelta = true;
            panelScroll = Vector2.zero;
            endingScroll = Vector2.zero;
            SynchronizeEnding();
        }

        public bool SelectThing(Guid id)
        {
            if (Simulation == null || IsEditing || !Simulation.Registry.TryGet(id, out var thing)
                || thing.Definition.Kind == ThingKind.Resource || !thing.IsPlaced) return false;
            SelectedId = id;
            return true;
        }

        public bool BeginGatheringConstruction() => BeginConstruction(gathering);
        public bool BeginStorageConstruction() => BeginConstruction(storage);
        public bool BeginProcessingConstruction() => BeginConstruction(processing);
        public bool BeginCultivationConstruction() => BeginConstruction(cultivation);
        public bool BeginConstruction(FacilityDefinition definition) => Record("Designate construction", Commands.BeginConstruction(definition));
        public bool BeginPlacement() => Record("Move facility", Controller.BeginPlacement(SelectedId));
        public bool PreviewAt(Vector2Int origin)
        {
            if (Simulation.ConstructionDefinition != null)
                return Record("Preview construction", Commands.PreviewConstruction(origin, Simulation.ConstructionRotation));
            return Record("Preview move", Controller.PreviewPlacement(origin,
                Controller.Current.Map.Commands.Edit?.Rotation ?? QuarterTurn.None));
        }

        public bool RotatePreview()
        {
            if (Simulation.ConstructionDefinition != null)
                return Record("Rotate construction", Commands.PreviewConstruction(Simulation.ConstructionOrigin,
                    NextRotation(Simulation.ConstructionRotation)));
            var edit = Controller.Current.Map.Commands.Edit;
            return Record("Rotate move", edit != null && Controller.PreviewPlacement(edit.Origin, NextRotation(edit.Rotation)));
        }

        public bool ConfirmPlacement() => Record("Confirm", Simulation.ConstructionDefinition != null
            ? Commands.ConfirmConstruction() : Controller.ConfirmPlacement());
        public bool CancelPlacement() => Record("Cancel", Simulation.ConstructionDefinition != null
            ? Commands.CancelConstruction() : Controller.CancelPlacement());
        public bool SetAllowed(Guid storageId, bool allowed) => Record("Storage allowance", Commands.SetAllowed(storageId, allowed));
        public bool SetAllowed(Guid storageId, ResourceDefinition resource, bool allowed) =>
            Record("Storage allowance", Commands.SetAllowed(storageId, resource, allowed));
        public bool SelectCultivation(Guid id, CultivationTarget target) => Record("Cultivation target", Commands.SelectCultivation(id, target));
        public bool AssignWorker(Guid id, Guid? facilityId) => Record("Worker assignment", Commands.AssignWorker(id, facilityId));
        public bool SetPreset(int[] priorities) => Record("Default priorities", Commands.SetPreset(priorities));
        public bool SetWorkerPriorities(Guid id, int[] priorities, bool usePreset = false) =>
            Record("Worker priorities", Commands.SetWorkerPriorities(id, priorities, usePreset));
        public bool Save() => Record("Save", Controller.Save());
        public bool Reload() => Record("Continue last save", Controller.Reload());
        public bool Restart() => Record("Restart", Controller.Restart());
        public bool RestartFromResults(GameFlowManager.EndingRequest request) => Record("Start new run", Ending.Restart(request));
        public void ToggleReports() => ReportsExpanded = !ReportsExpanded;
        public bool ReleaseInvaders()
        {
            var succeeded = Commands.ReleaseInvaders();
            if (succeeded && Simulation.Registry.TryGet(SelectedId, out var selected) && !selected.IsPlaced)
                SelectedId = Guid.Empty;
            return Record("Release all waiting invaders", succeeded);
        }

        private static QuarterTurn NextRotation(QuarterTurn rotation) => (QuarterTurn)(((int)rotation + 1) % 4);
        private bool Record(string command, bool succeeded)
        {
            LastCommand = command + (succeeded ? ": OK" : ": rejected / failed");
            return succeeded;
        }
    }
}
