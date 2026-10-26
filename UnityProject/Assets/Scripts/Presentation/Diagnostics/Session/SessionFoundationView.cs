using System;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.GameFlow;
using MobileGame.Persistence;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Temporary diagnostic surface. SessionController owns commands, time gating and saves.
    public sealed class SessionFoundationView : MonoBehaviour
    {
        [SerializeField] private FacilityDefinition storage;
        [SerializeField] private CreatureDefinition creature;
        [SerializeField] private ResourceDefinition resource;
        private GUIStyle cellStyle;
        private GUIStyle wrappedLabelStyle;
        private Vector2 panelScroll;
        private bool skipNextDelta = true;
        private bool applicationPaused;

        public SessionController Controller { get; private set; }
        public Guid SelectedId { get; private set; }
        public string SavePath { get; private set; }
        public string LastCommand { get; private set; }
        public int RepaintCount { get; private set; }
        public double LaboratorySecondsObserved { get; private set; }
        public double ExternalSecondsObserved { get; private set; }

        private void Start()
        {
            if (Controller == null)
                Initialize(storage, creature, resource,
                    Path.Combine(Application.persistentDataPath, "Diagnostics", "PLAN-007", "session.json"));
        }

        // A caller may initialize before Start to isolate its diagnostic/test save slot.
        public void Initialize(FacilityDefinition storageDefinition, CreatureDefinition creatureDefinition,
            ResourceDefinition resourceDefinition, string savePath, double autoSaveSeconds = 5)
        {
            if (Controller != null)
                throw new InvalidOperationException("This view already has a session controller.");
            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentException("Provide a diagnostic save path.", nameof(savePath));
            var codec = new SessionSnapshotCodec(new ThingDefinition[]
                { storageDefinition, creatureDefinition, resourceDefinition });
            SavePath = Path.GetFullPath(savePath);
            Controller = new SessionController(codec, new JsonFileSessionStore(SavePath), () =>
            {
                // Reuse an explicit fixture; this is not the approved game's start configuration.
                var fixture = new MapFoundationScenario(storageDefinition, creatureDefinition, resourceDefinition);
                return new SessionState(fixture.Registry, fixture.Map);
            }, autoSaveSeconds);
            Controller.SessionChanged += OnSessionChanged;
            Controller.LaboratoryAdvanced += ObserveLaboratory;
            Controller.ExternalAdvanced += ObserveExternal;
            Record("Start / Continue", Controller.StartOrContinue());
        }

        private void OnEnable() => skipNextDelta = true;

        private void Update()
        {
            if (Controller == null || applicationPaused)
                return;
            if (skipNextDelta)
            {
                skipNextDelta = false;
                return;
            }
            Controller.Advance(Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            skipNextDelta = true;
            Controller?.NotifyApplicationPause(paused);
        }

        private void OnApplicationQuit() => Controller?.NotifyApplicationQuit();

        private void OnDestroy()
        {
            if (Controller == null)
                return;
            Controller.SessionChanged -= OnSessionChanged;
            Controller.LaboratoryAdvanced -= ObserveLaboratory;
            Controller.ExternalAdvanced -= ObserveExternal;
        }

        private void OnSessionChanged()
        {
            SelectedId = Guid.Empty;
            LaboratorySecondsObserved = 0;
            ExternalSecondsObserved = 0;
            skipNextDelta = true;
        }

        private void ObserveLaboratory(double seconds) => LaboratorySecondsObserved += seconds;
        private void ObserveExternal(double seconds) => ExternalSecondsObserved += seconds;

        public bool SelectThing(Guid id)
        {
            var session = Controller?.Current;
            if (session == null || session.Map.Commands.IsEditing || !session.Registry.TryGet(id, out var thing)
                || thing.Definition.Kind == ThingKind.Resource)
                return false;
            SelectedId = id;
            return true;
        }

        public bool BeginPlacement() => Record("Begin placement", Controller.BeginPlacement(SelectedId));
        public bool PreviewAt(Vector2Int origin) => Record("Preview placement", Controller.PreviewPlacement(origin,
            Controller.Current.Map.Commands.Edit?.Rotation ?? QuarterTurn.None));
        public bool RotatePreview()
        {
            var edit = Controller.Current.Map.Commands.Edit;
            return Record("Rotate preview", edit != null && Controller.PreviewPlacement(edit.Origin,
                (QuarterTurn)(((int)edit.Rotation + 1) % 4)));
        }
        public bool ConfirmPlacement() => Record("Confirm placement", Controller.ConfirmPlacement());
        public bool CancelPlacement() => Record("Cancel placement", Controller.CancelPlacement());
        public bool Save() => Record("Save", Controller.Save());
        public bool Reload() => Record("Reload", Controller.Reload());
        public bool Restart() => Record("Restart", Controller.Restart());
        public bool ReachGoal() => Record("Goal signal", Controller.ReachGoal());
        public bool FinishDestruction() => Record("Finish presentation", Controller.FinishDestruction());

        private bool Record(string command, bool succeeded)
        {
            LastCommand = command + (succeeded ? ": OK" : ": rejected / failed");
            return succeeded;
        }

        private void OnGUI()
        {
            if (Controller == null)
                return;
            if (Event.current.type == EventType.Repaint)
                RepaintCount++;
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 12, Screen.width - 40, 25), "TEMP SESSION FOUNDATION / PC VERIFICATION");
            GUI.Label(new Rect(20, 38, Screen.width - 40, 25),
                "Diagnostic fixture only. Goal / presentation buttons are test signals; production and invasion are not implemented.");
            var session = Controller.Current;
            var panelWidth = Mathf.Min(330, Screen.width * 0.4f);
            var gridWidth = Mathf.Max(120, Screen.width - panelWidth - 70);
            if (session != null)
            {
                var size = Mathf.Max(8, Mathf.Min(48, gridWidth / session.Map.Width,
                    (Screen.height - 180f) / session.Map.Height));
                var grid = new Rect(20, 90, size * session.Map.Width, size * session.Map.Height);
                DrawGrid(grid, size);
                GUI.Label(new Rect(20, grid.yMax + 12, gridWidth, 24),
                    "Blue: facility   Orange: creature   I: interaction   P: preview");
                GUI.Label(new Rect(20, grid.yMax + 38, gridWidth, 44),
                    "Actual update signals observed (diagnostic counters only)\nLaboratory: "
                    + LaboratorySecondsObserved.ToString("F2") + "s / External: " + ExternalSecondsObserved.ToString("F2") + "s");
            }
            DrawPanel(new Rect(Screen.width - panelWidth - 20, 76, panelWidth, Mathf.Max(120, Screen.height - 90)));
        }

        private void DrawGrid(Rect grid, float size)
        {
            if (cellStyle == null)
            {
                cellStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                cellStyle.normal.textColor = Color.white;
                cellStyle.hover.textColor = Color.white;
                cellStyle.active.textColor = Color.white;
            }
            var map = Controller.Current.Map;
            var placements = map.GetPlacements();
            var edit = map.Commands.Edit;
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                var rect = new Rect(grid.x + x * size, grid.y + (map.Height - 1 - y) * size, size - 2, size - 2);
                var facility = placements.FirstOrDefault(p => p.Kind == ThingKind.Facility && p.Contains(cell));
                var creatures = placements.Count(p => p.Kind == ThingKind.Creature && p.Contains(cell));
                var interaction = placements.FirstOrDefault(p => p.InteractionCell == cell);
                var label = facility != null ? "F" + facility.CreationOrder : creatures > 0 ? "C x" + creatures
                    : interaction != null ? "I" + interaction.CreationOrder : "";
                var tint = facility != null ? new Color(0.18f, 0.42f, 0.62f)
                    : creatures > 0 ? new Color(0.72f, 0.4f, 0.16f) : new Color(0.13f, 0.17f, 0.23f);
                if (edit != null && edit.Origin == cell)
                {
                    tint = edit.Validation.Succeeded ? new Color(0.12f, 0.55f, 0.32f) : new Color(0.65f, 0.15f, 0.18f);
                    label = "P";
                }
                GUI.color = tint;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                if (GUI.Button(rect, label, cellStyle))
                {
                    if (edit != null) PreviewAt(cell);
                    else if (facility != null) SelectThing(facility.ThingId);
                    else if (creatures > 0) SelectThing(placements.First(p => p.Kind == ThingKind.Creature && p.Contains(cell)).ThingId);
                }
            }
        }

        private void DrawPanel(Rect rect)
        {
            if (wrappedLabelStyle == null)
                wrappedLabelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.BeginArea(rect, GUI.skin.box);
            panelScroll = GUILayout.BeginScrollView(panelScroll);
            GUILayout.Label("Phase: " + Controller.Phase + " / generation " + Controller.Generation);
            GUILayout.Label("Laboratory: " + (Controller.CanAdvanceLaboratory ? "running" : "stopped")
                + " / External: " + (Controller.CanAdvanceExternal ? "running" : "stopped"));
            GUILayout.Label("Successful saves: " + Controller.SuccessfulSaves);
            GUILayout.Label("One diagnostic slot: " + SavePath, wrappedLabelStyle);
            if (GUILayout.Button("Save committed state")) Save();
            if (GUILayout.Button("Reload last save")) Reload();
            if (GUILayout.Button("Restart fixture + replace save")) Restart();
            GUI.enabled = Controller.Phase == SessionPhase.Playing;
            if (GUILayout.Button("TEST SIGNAL: goal reached")) ReachGoal();
            GUI.enabled = Controller.Phase == SessionPhase.Destruction;
            if (GUILayout.Button("TEST SIGNAL: presentation finished")) FinishDestruction();
            GUI.enabled = true;

            var session = Controller.Current;
            if (session != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("FACILITIES");
                GUI.enabled = !session.Map.Commands.IsEditing;
                foreach (var thing in session.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Facility)
                             .OrderBy(t => t.CreationOrder))
                    if (GUILayout.Button("Store " + thing.CreationOrder + (thing.Id == SelectedId ? " [selected]" : "")))
                        SelectThing(thing.Id);
                GUI.enabled = true;
                if (session.Registry.TryGet(SelectedId, out var selected))
                {
                    GUILayout.Label("Selected: " + selected.Definition.Kind + " #" + selected.CreationOrder);
                    GUILayout.Label(selected.Placement == null ? "Outside Map" : "Origin: " + selected.Placement.Origin
                        + " / " + ((int)selected.Placement.Rotation * 90) + " deg");
                    if (selected.TryGetComponent<ResourceContainerComponent>(out var holder))
                        GUILayout.Label("Stored quantity: " + holder.Contents.Sum(t => t.GetComponent<ResourceStackComponent>().Quantity));
                    GUI.enabled = Controller.Phase == SessionPhase.Playing && !session.Map.Commands.IsEditing;
                    if (GUILayout.Button("Move / Rotate selected facility")) BeginPlacement();
                    GUI.enabled = true;
                }
                var edit = session.Map.Commands.Edit;
                if (edit != null)
                {
                    GUILayout.Label("Preview: " + edit.Origin + " / " + edit.Validation);
                    GUI.enabled = Controller.Phase == SessionPhase.Playing;
                    if (GUILayout.Button("Rotate 90 degrees")) RotatePreview();
                    if (GUILayout.Button("Confirm placement")) ConfirmPlacement();
                    GUI.enabled = true;
                    if (GUILayout.Button("Cancel placement")) CancelPlacement();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(LastCommand ?? "No command");
            if (!string.IsNullOrEmpty(Controller.LastError)) GUILayout.Label("Command error: " + Controller.LastError, wrappedLabelStyle);
            if (!string.IsNullOrEmpty(Controller.LastSaveError)) GUILayout.Label("Save error: " + Controller.LastSaveError, wrappedLabelStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
