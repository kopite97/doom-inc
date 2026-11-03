using System;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private Vector2 panelScroll;
        private UiCanvas screen;
        private RectTransform playRoot, toolRoot, cameraTools, navigationRoot, summaryRoot;
        private Text titleText, hudText, editText, summaryText, feedbackText;
        private RectTransform worldGauge, worldGaugeFill;
        private ScrollRect controlsScroll;
        private UiList controls;
        private UiButton collapseButton, rotateButton, confirmButton, cancelButton;
        private UiButton zoomInButton, zoomOutButton, fitButton, statusButton, gameMenuButton;
        private UiButton buildMenuButton, creatureMenuButton, invasionMenuButton, detailsButton, closeButton;
        private LaboratoryMapView mapScreen;
        private bool controlsCollapsed;
        private int screenGeneration = -1;
        private SessionPhase screenPhase;

        public RectTransform MapViewport { get; private set; }
        public Canvas UiCanvas => screen?.Canvas;
        public RectTransform UiSafeRoot => screen?.SafeRoot;
        public bool ControlsCollapsed => controlsCollapsed;
        public UiButton FindUiButton(string key) => UiCanvas == null ? null
            : UiCanvas.GetComponentsInChildren<UiButton>(true).FirstOrDefault(b => b.name == key);

        private void EnsureScreen()
        {
            if (screen != null) return;
            screen = new UiCanvas(transform, "Temp_LaboratoryCanvas", 10);
            playRoot = UiWidgets.Panel("PlayScreen", screen.SafeRoot, UiWidgets.Background).rectTransform;
            titleText = UiWidgets.Label("Title", playRoot, 24); titleText.text = L("heading");
            hudText = UiWidgets.Label("Resources", playRoot, 17);
            worldGauge = UiWidgets.Panel("WorldProgress", playRoot, new Color(.18f, .2f, .25f)).rectTransform;
            worldGaugeFill = UiWidgets.Panel("Progress", worldGauge, new Color(.85f, .3f, .2f)).rectTransform;
            gameMenuButton = IconButton("GameMenu", playRoot, LaboratoryIcon.More, L("menu"), () => OpenPanel(LaboratoryPanel.Game));
            collapseButton = UiWidgets.Button("ToggleControls", playRoot, L("collapse"), () => screen.LayoutEpoch);
            collapseButton.GetComponentInChildren<UiText>().Initialize(14);
            SetCompactButtonLabel(collapseButton);
            collapseButton.Bind(() => { if (!CanClickScreen()) return; controlsCollapsed = !controlsCollapsed; InvalidateInput(); }, "screen");
            var mapBackground = UiWidgets.Panel("MapViewport", playRoot, new Color(.025f, .04f, .065f));
            MapViewport = mapBackground.rectTransform; MapViewport.gameObject.AddComponent<RectMask2D>();
            mapScreen = MapViewport.gameObject.AddComponent<LaboratoryMapView>(); mapScreen.Initialize(this);
            cameraTools = UiWidgets.Rect("CameraTools", playRoot);
            zoomOutButton = IconButton("ZoomOut", cameraTools, LaboratoryIcon.ZoomOut, L("zoom_out"), () => mapScreen.ZoomBy(1 / 1.3f), true);
            zoomInButton = IconButton("ZoomIn", cameraTools, LaboratoryIcon.ZoomIn, L("zoom_in"), () => mapScreen.ZoomBy(1.3f), true);
            fitButton = IconButton("FitMap", cameraTools, LaboratoryIcon.FitMap, L("fit"), () => mapScreen.Fit(), true);
            statusButton = IconButton("ToggleStatus", cameraTools, LaboratoryIcon.ShowStatus, L("status_on"), () => SetUiPreferences(!ShowFacilityStatus, LargeText), true);
            toolRoot = UiWidgets.Rect("PlacementTools", playRoot);
            editText = UiWidgets.Label("PlacementStatus", playRoot, 16);
            rotateButton = UiWidgets.Button("Rotate", toolRoot, L("rotate"), () => screen.LayoutEpoch);
            confirmButton = UiWidgets.Button("Confirm", toolRoot, L("confirm_placement"), () => screen.LayoutEpoch);
            cancelButton = UiWidgets.Button("Cancel", toolRoot, L("cancel"), () => screen.LayoutEpoch);
            navigationRoot = UiWidgets.Rect("Navigation", playRoot);
            buildMenuButton = IconButton("NavBuild", navigationRoot, LaboratoryIcon.Build, L("build"), () => OpenPanel(LaboratoryPanel.Build));
            creatureMenuButton = IconButton("NavCreatures", navigationRoot, LaboratoryIcon.Creatures, L("creatures"), () => OpenPanel(LaboratoryPanel.Creatures));
            invasionMenuButton = IconButton("NavInvasion", navigationRoot, LaboratoryIcon.Invasion, L("invasion"), () => OpenPanel(LaboratoryPanel.Invasion));
            controlsScroll = UiWidgets.Scroll("Controls", playRoot, out var content);
            controls = new UiList(content, () => screen.LayoutEpoch);
            summaryRoot = UiWidgets.Panel("SelectionSummary", playRoot, UiWidgets.PanelColor).rectTransform;
            summaryText = UiWidgets.Label("Summary", summaryRoot, 18);
            detailsButton = UiWidgets.Button("Details", summaryRoot, L("details"), () => screen.LayoutEpoch);
            detailsButton.Bind(ExpandSelection, "details");
            closeButton = UiWidgets.Button("ClosePanel", playRoot, L("close"), () => screen.LayoutEpoch);
            closeButton.GetComponentInChildren<UiText>().Initialize(14);
            SetCompactButtonLabel(closeButton);
            closeButton.Bind(Back, "close");
            feedbackText = UiWidgets.Label("Feedback", playRoot, 15);
            CreateEndingScreen();
            ApplyTextStyle();
        }
        private UiButton IconButton(string key, Transform parent, LaboratoryIcon icon, string label, Action action, bool compact = false)
        {
            var button = UiWidgets.Button(key, parent, label, () => screen.LayoutEpoch);
            button.Bind(() => { if (CanClickScreen() && pendingSessionAction == LaboratorySessionAction.None) action(); }, key);
            SetIcon(button, icon, label, compact); return button;
        }
        private static void SetCompactButtonLabel(UiButton button)
        {
            var rect = button.GetComponentInChildren<Text>().rectTransform;
            rect.offsetMin = new Vector2(4, 4); rect.offsetMax = new Vector2(-4, -4);
        }
        private void SetIcon(UiButton button, LaboratoryIcon icon, string label, bool compact = false)
        {
            var graphic = button.transform.Find("Icon")?.GetComponent<Image>();
            if (graphic == null)
            {
                graphic = UiWidgets.Panel("Icon", button.transform, Color.white);
                graphic.raycastTarget = false; graphic.preserveAspect = true;
            }
            graphic.rectTransform.anchorMin = graphic.rectTransform.anchorMax = new Vector2(.5f, compact ? .5f : .76f);
            graphic.rectTransform.pivot = new Vector2(.5f, .5f);
            graphic.rectTransform.sizeDelta = new Vector2(22, 22);
            graphic.rectTransform.anchoredPosition = Vector2.zero;
            if (compact)
            {
                // Keep the full density-aware button as the hit target; only the visible plate is small.
                var hitArea = button.GetComponent<Image>(); hitArea.color = Color.clear; hitArea.raycastTarget = true;
                var plate = button.transform.Find("IconPlate")?.GetComponent<Image>();
                if (plate == null)
                {
                    plate = UiWidgets.Panel("IconPlate", button.transform, new Color(.16f, .23f, .31f));
                    plate.raycastTarget = false; plate.transform.SetAsFirstSibling();
                    plate.rectTransform.anchorMin = plate.rectTransform.anchorMax = plate.rectTransform.pivot = new Vector2(.5f, .5f);
                    plate.rectTransform.anchoredPosition = Vector2.zero;
                    plate.rectTransform.sizeDelta = new Vector2(36, 36);
                }
                if (button.targetGraphic != plate) button.targetGraphic = plate;
            }
            graphic.sprite = uiAssets?.GetIcon(icon); graphic.gameObject.SetActive(graphic.sprite != null);
            var text = button.GetComponentInChildren<Text>(); text.text = label;
            // Preserve the functional name and use it as the visible fallback if an icon is unavailable.
            text.enabled = !compact || graphic.sprite == null;
            text.alignment = compact || graphic.sprite == null ? TextAnchor.MiddleCenter : TextAnchor.LowerCenter;
            var style = text.GetComponent<UiText>();
            if (style.BaseSize != 13) style.Initialize(13);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = compact || graphic.sprite == null ? Vector2.one : new Vector2(1, 0);
            text.rectTransform.pivot = new Vector2(.5f, 0);
            text.rectTransform.offsetMin = new Vector2(2, 2);
            text.rectTransform.offsetMax = compact || graphic.sprite == null ? new Vector2(-2, -2) : new Vector2(-2, 22);
        }
        private bool CanClickScreen() => isActiveAndEnabled && Controller != null && !Controller.IsApplicationPaused && !transitionPaused && Popups?.BlocksInput != true;
        private string UiContext => Controller == null ? "none" : Controller.Generation + ":" + Controller.Phase;
        private void BindTool(UiButton button, Action action, bool enabled)
        {
            var generation = Controller.Generation; var selection = SelectedId; var editing = IsEditing;
            button.Bind(() => { if (CanUseMap && generation == Controller.Generation && selection == SelectedId && editing == IsEditing) action(); }, UiContext + ":" + SelectedId + ":" + IsEditing);
            button.interactable = enabled && CanUseMap;
        }
        private void UpdateScreen()
        {
            if (Controller == null) return;
            EnsureScreen(); RefreshFixedText(); screen.RefreshLayout(); screen.SetVisible(isActiveAndEnabled);
            if (screenGeneration != Controller.Generation || screenPhase != Controller.Phase)
            {
                InvalidateInput();
                if (screenGeneration != Controller.Generation)
                {
                    controlsScroll.StopMovement(); resultScroll.StopMovement();
                    controlsScroll.content.anchoredPosition = panelScroll; resultScroll.content.anchoredPosition = endingScroll;
                }
                screenGeneration = Controller.Generation; screenPhase = Controller.Phase;
            }
            var ending = Ending?.IsEnding == true;
            playRoot.gameObject.SetActive(!ending); endingRoot.gameObject.SetActive(ending);
            if (ending) UpdateEndingScreen();
            else { SelectionUnavailable(); RestorePanelPosition(); LayoutPlayScreen(); UpdateHud(); UpdateTools(); UpdateControls(); }
            RepaintCount++;
        }
        private bool HasSelection => Simulation != null && SelectedId != Guid.Empty && Simulation.Registry.TryGet(SelectedId, out _);
        private bool HasPanel => !controlsCollapsed && (panel != LaboratoryPanel.None && panel != LaboratoryPanel.Selection
            || panel == LaboratoryPanel.Selection && HasSelection && (!screen.Portrait || detailsExpanded));
        private bool HasSummary => !controlsCollapsed && screen.Portrait && panel == LaboratoryPanel.Selection && HasSelection && !detailsExpanded;
        private void LayoutPlayScreen()
        {
            var w = screen.Size.x; var h = screen.Size.y; var margin = 12f; var touch = MinimumTouchSize;
            var collapseWidth = Mathf.Max(touch, 64);
            var closeWidth = Mathf.Max(touch, 64);
            feedbackText.text = uiNotice == null ? "" : FormatMessage(uiNotice);
            UiWidgets.Place(feedbackText.rectTransform, margin, 0, w - 24, 26);
            var feedbackHeight = Mathf.Max(26, Mathf.Ceil(feedbackText.preferredHeight) + 4);
            var top = screen.Portrait ? Mathf.Max(116, touch + 66) : Mathf.Max(touch + 16, 100 * UiTextScale);
            var footer = touch + feedbackHeight + 6; var cameraHeight = screen.Portrait ? touch + 6 : 0;
            var collapseX = w - touch - collapseWidth - margin - 8;
            var cameraWidth = 4 * touch + 18;
            var cameraX = collapseX - cameraWidth - 8;
            var headerWidth = screen.Portrait ? w - touch - collapseWidth - 42 : cameraX - margin - 12;
            // A smaller landscape title leaves two full HUD lines above the gauge, including large text.
            var titleSize = screen.Portrait ? 24 : 18;
            var titleStyle = titleText.GetComponent<UiText>();
            if (titleStyle.BaseSize != titleSize) titleStyle.Initialize(titleSize);
            var titleHeight = screen.Portrait ? 36 : 28 * UiTextScale;
            var hudY = 8 + titleHeight + 2;
            UiWidgets.Place(playRoot, 0, 0, w, h);
            UiWidgets.Place(titleText.rectTransform, margin, 8, headerWidth, titleHeight);
            UiWidgets.Place(gameMenuButton.GetComponent<RectTransform>(), w - touch - margin, 8, touch, touch);
            UiWidgets.Place(collapseButton.GetComponent<RectTransform>(), collapseX, 8, collapseWidth, touch);
            collapseButton.GetComponentInChildren<Text>().text = controlsCollapsed ? L("expand") : L("collapse");
            collapseButton.interactable = CanClickScreen() && pendingSessionAction == LaboratorySessionAction.None;
            UiWidgets.Place(hudText.rectTransform, margin, hudY, headerWidth, top - hudY - 12);
            UiWidgets.Place(worldGauge, margin, top - 10, w - margin * 2, 6);
            var panelWidth = !screen.Portrait && HasPanel ? Mathf.Clamp(w * .34f, 310, 430) : 0;
            var panelHeight = screen.Portrait && HasPanel ? Mathf.Clamp(h * .37f, 230, 470) : HasSummary ? Mathf.Max(92, touch + 24) : 0;
            var panelGap = panelWidth > 0 ? 12 : 0;
            var mapWidth = w - 24 - panelWidth - panelGap;
            var mapHeight = Mathf.Max(80, h - top - footer - cameraHeight - panelHeight - 30 - (IsEditing ? 48 : 0));
            UiWidgets.Place(MapViewport, margin, top, mapWidth, mapHeight);
            // This toolbar remains a playRoot sibling of the clipped map, never a map overlay.
            UiWidgets.Place(cameraTools, screen.Portrait ? margin + mapWidth - cameraWidth : cameraX,
                screen.Portrait ? top + mapHeight + 4 : 8, cameraWidth, touch);
            var cameraButtons = new[] { zoomOutButton, zoomInButton, fitButton, statusButton };
            for (var i = 0; i < cameraButtons.Length; i++) UiWidgets.Place((RectTransform)cameraButtons[i].transform,
                i * (touch + 6), 0, touch, touch);
            var panelY = top + mapHeight + cameraHeight + 10;
            controlsScroll.gameObject.SetActive(HasPanel); summaryRoot.gameObject.SetActive(HasSummary);
            closeButton.gameObject.SetActive(HasPanel || HasSummary);
            closeButton.interactable = pendingSessionAction == LaboratorySessionAction.None;
            if (HasPanel)
            {
                var x = screen.Portrait ? margin : w - panelWidth - margin;
                var y = screen.Portrait ? panelY : top;
                var width = screen.Portrait ? w - 24 : panelWidth;
                var height = screen.Portrait ? panelHeight : h - top - footer - 8;
                UiWidgets.Place((RectTransform)closeButton.transform, x + width - closeWidth, y, closeWidth, touch);
                UiWidgets.Place((RectTransform)controlsScroll.transform, x, y + touch + 4, width, Mathf.Max(60, height - touch - 4));
            }
            if (HasSummary)
            {
                var detailsWidth = Mathf.Max(touch + 12, 120);
                UiWidgets.Place(summaryRoot, margin, panelY, w - 24, panelHeight);
                UiWidgets.Place(summaryText.rectTransform, 10, 8, w - closeWidth - detailsWidth - 60, panelHeight - 12);
                UiWidgets.Place((RectTransform)detailsButton.transform, w - closeWidth - detailsWidth - 42, 8, detailsWidth, touch);
                UiWidgets.Place((RectTransform)closeButton.transform, w - closeWidth - margin, panelY + 8, closeWidth, touch);
                summaryText.text = SelectionSummary();
            }
            navigationRoot.gameObject.SetActive(!IsEditing); toolRoot.gameObject.SetActive(IsEditing);
            UiWidgets.Place(navigationRoot, margin, h - footer, w - 24, touch);
            UiWidgets.Place(toolRoot, margin, h - footer, w - 24, touch);
            var nav = new[] { buildMenuButton, creatureMenuButton, invasionMenuButton };
            var tools = new[] { cancelButton, rotateButton, confirmButton };
            var itemWidth = (w - 40) / 3;
            for (var i = 0; i < 3; i++)
            {
                UiWidgets.Place((RectTransform)nav[i].transform, i * (itemWidth + 8), 0, itemWidth, touch);
                UiWidgets.Place((RectTransform)tools[i].transform, i * (itemWidth + 8), 0, itemWidth, touch);
                nav[i].interactable = CanUseMap && !IsEditing;
            }
            editText.gameObject.SetActive(IsEditing);
            UiWidgets.Place(editText.rectTransform, margin, h - footer - 44, w - 24, 42);
            UiWidgets.Place(feedbackText.rectTransform, margin, h - feedbackHeight - 2, w - 24, feedbackHeight);
        }
        private string SelectionSummary()
        {
            if (!HasSelection) return "";
            Simulation.Registry.TryGet(SelectedId, out var selected);
            if (selected.Definition.Kind == ThingKind.Facility)
            {
                var status = StatusFor(selected);
                return L("selection_summary", A("thing", ThingMessage(selected.Id)), A("state", StatusMessage(status)));
            }
            return L("selection_summary", A("thing", ThingMessage(selected.Id)), A("state", CreatureActivity(selected)));
        }
        private void UpdateHud()
        {
            UiWidgets.Place(worldGaugeFill, 0, 0, worldGauge.rect.width * (Invasion == null ? 0 : (float)Math.Min(1, (double)Invasion.Score / Invasion.Settings.TargetScore)), 6);
            if (Simulation == null) { hudText.text = L("load_error"); panel = LaboratoryPanel.Game; return; }
            var things = Simulation.Registry.Things;
            var organicCount = things.Where(t => t.Definition == organic).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            var cultureCount = things.Where(t => t.Definition == culture).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            hudText.text = L(IsEditing ? "hud_paused" : "hud", A("organic", organicCount), A("culture", cultureCount),
                A("creatures", things.Count(t => t.Definition.Kind == ThingKind.Creature)), A("score", Invasion.Score), A("target", Invasion.Settings.TargetScore));
        }
        private void UpdateTools()
        {
            var editing = IsEditing;
            if (editing)
            {
                var constructing = Simulation.ConstructionDefinition != null;
                var edit = Simulation.Map.Commands.Edit;
                var origin = constructing ? Simulation.ConstructionOrigin : edit.Origin;
                var valid = constructing ? Simulation.ConstructionValidation.Succeeded : edit.Validation.Succeeded;
                editText.text = L(valid ? "placement_valid" : "placement_invalid", A("action", M(constructing ? "placing_new" : "moving")), A("position", origin.ToString()));
            }
            BindTool(rotateButton, () => RotatePreview(), editing);
            BindTool(confirmButton, () => ConfirmPlacement(), editing && (Simulation.ConstructionDefinition != null ? Simulation.ConstructionValidation.Succeeded : Simulation.Map.Commands.Edit.Validation.Succeeded));
            BindTool(cancelButton, () => CancelPlacement(), editing);
            BindTool(zoomInButton, () => mapScreen.ZoomBy(1.3f), Simulation != null);
            BindTool(zoomOutButton, () => mapScreen.ZoomBy(1 / 1.3f), Simulation != null);
            BindTool(fitButton, () => mapScreen.Fit(), Simulation != null);
            BindTool(statusButton, () => SetUiPreferences(!ShowFacilityStatus, LargeText), Simulation != null);
            SetIcon(statusButton, ShowFacilityStatus ? LaboratoryIcon.ShowStatus : LaboratoryIcon.HideStatus, ShowFacilityStatus ? L("status_on") : L("status_off"), true);
        }
        private void UiAction(string key, string label, Action action, bool enabled = true, Guid? selected = null)
        {
            var generation = Controller.Generation; var phase = Controller.Phase; var currentPanel = panel; var confirmation = pendingSessionAction;
            controls.Action(key, label, () =>
            {
                if (!CanClickScreen() || Controller.Generation != generation || Controller.Phase != phase || panel != currentPanel
                    || confirmation != pendingSessionAction || (selected.HasValue && SelectedId != selected.Value)) return;
                action();
            }, enabled && CanClickScreen(), UiContext + ":" + panel + ":" + pendingSessionAction + (selected.HasValue ? ":" + selected : ""));
        }
        private void UpdateControls()
        {
            if (!HasPanel) return;
            float anchorOffset = 0;
            var reportAnchor = panel == LaboratoryPanel.Invasion && ReportsExpanded
                ? controls.CaptureAnchor("Report_", controlsScroll.content.anchoredPosition.y, out anchorOffset) : null;
            controls.Style(UiFont, UiTextScale, MinimumTouchSize);
            controls.Begin(Mathf.Max(100, ((RectTransform)controlsScroll.transform).rect.width - 16));
            var canOperate = CanUseMap && !IsEditing;
            if (panel == LaboratoryPanel.Game) AddGameControls();
            else if (panel == LaboratoryPanel.License)
                controls.Text("FontLicense", uiAssets?.FontLicense != null ? uiAssets.FontLicense.text : L("license_missing"), 16);
            else if (Simulation != null)
            {
                switch (panel)
                {
                    case LaboratoryPanel.Selection:
                        if (Simulation.Registry.TryGet(SelectedId, out var selected)) AddSelectedControls(selected);
                        break;
                    case LaboratoryPanel.Build:
                        controls.Text("ConstructionTitle", L("build"), 24);
                        controls.Text("BuildHint", L("build_hint"), 16);
                        UiAction("BuildGathering", L("build_cost", A("facility", DefinitionName(gathering)), A("resource", DefinitionName(organic)), A("count", gathering.BuildCost)), () => BeginGatheringConstruction(), canOperate);
                        UiAction("BuildStorage", L("build_cost", A("facility", DefinitionName(storage)), A("resource", DefinitionName(organic)), A("count", storage.BuildCost)), () => BeginStorageConstruction(), canOperate);
                        UiAction("BuildProcessing", L("build_cost", A("facility", DefinitionName(processing)), A("resource", DefinitionName(organic)), A("count", processing.BuildCost)), () => BeginProcessingConstruction(), canOperate);
                        UiAction("BuildCultivation", L("build_cost", A("facility", DefinitionName(cultivation)), A("resource", DefinitionName(organic)), A("count", cultivation.BuildCost)), () => BeginCultivationConstruction(), canOperate);
                        controls.Text("Facilities", L("facilities"), 22); AddRoster(ThingKind.Facility, canOperate); break;
                    case LaboratoryPanel.Creatures:
                        controls.Text("Creatures", L("creatures"), 24); AddRoster(ThingKind.Creature, canOperate);
                        controls.Text("PresetTitle", L("preset_title"), 22);
                        controls.Text("PresetHint", L("preset_hint"), 16);
                        AddPriorities("Preset", Simulation.Preset.ToArray(), values => SetPreset(values), canOperate); break;
                    case LaboratoryPanel.Candidates:
                        controls.Text("Candidates", L("candidates"), 22);
                        foreach (var id in candidates.ToArray()) if (Simulation.Registry.TryGet(id, out var candidate) && candidate.IsPlaced)
                            UiAction("Select_" + id.ToString("N"), L("roster_entry", A("thing", ThingMessage(id)), A("state", CreatureActivity(candidate))), () => SelectThing(id), canOperate);
                        break;
                    case LaboratoryPanel.Invasion: AddInvasionControls(canOperate); break;
                }
            }
            if (!string.IsNullOrEmpty(Controller.LastError)) controls.Text("LoadError", L("load_error"));
            if (!string.IsNullOrEmpty(Controller.LastSaveError)) controls.Text("SaveError", L("save_failed"));
            controls.End();
            if (reportAnchor != null)
            {
                if (!controls.TryRestoreAnchor(reportAnchor, anchorOffset, out var y) && Invasion.Reports.Count > 0)
                {
                    controls.TryRestoreAnchor("Report_" + Invasion.Reports.First().Sequence, 0, out y);
                    uiNotice = M("reports_expired");
                }
                if (Mathf.Abs(controlsScroll.content.anchoredPosition.y - y) > .1f)
                { controlsScroll.StopMovement(); controlsScroll.content.anchoredPosition = new Vector2(0, y); }
            }
            panelScroll = controlsScroll.content.anchoredPosition;
        }
        private void AddRoster(ThingKind kind, bool canOperate)
        {
            var entries = Simulation.Registry.Things.Where(t => t.Definition.Kind == kind).OrderBy(t => t.CreationOrder).ToArray();
            if (entries.Length == 0) controls.Text("EmptyRoster", kind == ThingKind.Facility ? L("empty_facilities") : L("empty_creatures"));
            foreach (var thing in entries)
            {
                var id = thing.Id;
                UiAction("Select_" + id.ToString("N"), L("roster_entry", A("thing", ThingMessage(id)), A("state", CreatureActivity(thing))), () =>
                {
                    if (thing.IsPlaced) SelectThing(id);
                    else { SelectedId = id; SelectionChanged(); }
                }, canOperate);
            }
        }
        private LocalizedMessage CreatureActivity(Thing thing) => !thing.IsPlaced ? M("outside")
            : thing.Definition.Kind == ThingKind.Facility ? LaboratoryStatusText.Short(StatusFor(thing))
            : M(thing.TryGetComponent<WorkerComponent>(out var worker) ? worker.Job == null ? "idle" : "working" : "waiting_release");
        private void AddInvasionControls(bool canOperate)
        {
            controls.Text("WorldTitle", L("invasion"), 24);
            controls.Text("InvasionCounts", L("invasion_counts", A("waiting", Simulation.Invasion.WaitingCount), A("outside", Simulation.Invasion.ExternalCount)));
            controls.Text("SupplyReceipts", L("supply_receipts", A("total", Simulation.Receiving.Sources.Sum(s => s.Delivered)), A("pending", Simulation.Receiving.PendingCount)));
            UiAction("Release", L("release", A("count", Simulation.Invasion.WaitingCount)), () => ReleaseInvaders(), canOperate && Simulation.Invasion.WaitingCount > 0);
            UiAction("Reports", ReportsExpanded ? L("fold_reports") : L("show_reports", A("count", Invasion.Reports.Count)), ToggleReports);
            if (!ReportsExpanded) return;
            UiAction("LatestReports", L("latest_reports"), () => { controlsScroll.StopMovement(); controlsScroll.content.anchoredPosition = Vector2.zero; });
            if (Invasion.Reports.Count == 0) controls.Text("NoReports", L("no_reports"));
            foreach (var report in Invasion.Reports.Reverse()) controls.Text("Report_" + report.Sequence,
                FormatMessage(ReportMessage(report)));
        }
        private void AddGameControls()
        {
            controls.Text("GameTitle", L("game_menu"), 24);
            if (SceneFlowManager.Instance != null)
                UiAction("ReturnToTitle", L("return_title"), () => SceneFlowManager.Instance.RequestReturnToTitle());
            UiAction("Save", L("save"), () => { uiNotice = M(Save() ? "saved" : "save_failed"); }, Controller.Current != null);
            UiAction("Reload", L("reload"), () => RequestSessionAction(LaboratorySessionAction.Reload));
            UiAction("Restart", L("restart"), () => RequestSessionAction(LaboratorySessionAction.Restart));
            UiAction("Settings", FormatMessage(C("settings")), () => ShowSettings());
            controls.Text("MapControlsHelp", L("map_help"), 16);
            UiAction("FontLicense", L("font_license"), () => OpenPanel(LaboratoryPanel.License));
        }
    }
}
