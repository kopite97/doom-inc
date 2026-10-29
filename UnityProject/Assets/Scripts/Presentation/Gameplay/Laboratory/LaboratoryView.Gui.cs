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
        private LaboratoryUiCanvas screen;
        private RectTransform playRoot, toolRoot, cameraTools, navigationRoot, summaryRoot;
        private Text titleText, hudText, editText, summaryText, feedbackText;
        private RectTransform worldGauge, worldGaugeFill;
        private ScrollRect controlsScroll;
        private LaboratoryUiList controls;
        private LaboratoryUiButton collapseButton, rotateButton, confirmButton, cancelButton;
        private LaboratoryUiButton zoomInButton, zoomOutButton, fitButton, statusButton, gameMenuButton;
        private LaboratoryUiButton buildMenuButton, creatureMenuButton, invasionMenuButton, detailsButton, closeButton;
        private LaboratoryMapView mapScreen;
        private bool controlsCollapsed;
        private int screenGeneration = -1;
        private SessionPhase screenPhase;

        public RectTransform MapViewport { get; private set; }
        public Canvas UiCanvas => screen?.Canvas;
        public RectTransform UiSafeRoot => screen?.SafeRoot;
        public bool ControlsCollapsed => controlsCollapsed;
        public LaboratoryUiButton FindUiButton(string key) => UiCanvas == null ? null
            : UiCanvas.GetComponentsInChildren<LaboratoryUiButton>(true).FirstOrDefault(b => b.name == key);

        private void EnsureScreen()
        {
            if (screen != null) return;
            screen = new LaboratoryUiCanvas(transform, "Temp_LaboratoryCanvas", 10);
            playRoot = LaboratoryUiWidgets.Panel("PlayScreen", screen.SafeRoot, LaboratoryUiWidgets.Background).rectTransform;
            titleText = LaboratoryUiWidgets.Label("Title", playRoot, 24); titleText.text = "DOOM INC · 연구소";
            hudText = LaboratoryUiWidgets.Label("Resources", playRoot, 17);
            worldGauge = LaboratoryUiWidgets.Panel("WorldProgress", playRoot, new Color(.18f, .2f, .25f)).rectTransform;
            worldGaugeFill = LaboratoryUiWidgets.Panel("Progress", worldGauge, new Color(.85f, .3f, .2f)).rectTransform;
            gameMenuButton = IconButton("GameMenu", playRoot, LaboratoryIcon.More, "메뉴", () => OpenPanel(LaboratoryPanel.Game));
            collapseButton = LaboratoryUiWidgets.Button("ToggleControls", playRoot, "접기", () => screen.LayoutEpoch);
            collapseButton.GetComponentInChildren<LaboratoryUiText>().Initialize(14);
            SetCompactButtonLabel(collapseButton);
            collapseButton.Bind(() => { if (!CanClickScreen()) return; controlsCollapsed = !controlsCollapsed; InvalidateInput(); }, "screen");
            var mapBackground = LaboratoryUiWidgets.Panel("MapViewport", playRoot, new Color(.025f, .04f, .065f));
            MapViewport = mapBackground.rectTransform; MapViewport.gameObject.AddComponent<RectMask2D>();
            mapScreen = MapViewport.gameObject.AddComponent<LaboratoryMapView>(); mapScreen.Initialize(this);
            cameraTools = LaboratoryUiWidgets.Rect("CameraTools", playRoot);
            zoomOutButton = IconButton("ZoomOut", cameraTools, LaboratoryIcon.ZoomOut, "축소", () => mapScreen.ZoomBy(1 / 1.3f), true);
            zoomInButton = IconButton("ZoomIn", cameraTools, LaboratoryIcon.ZoomIn, "확대", () => mapScreen.ZoomBy(1.3f), true);
            fitButton = IconButton("FitMap", cameraTools, LaboratoryIcon.FitMap, "전체 보기", () => mapScreen.Fit(), true);
            statusButton = IconButton("ToggleStatus", cameraTools, LaboratoryIcon.ShowStatus, "상태 ON", () => SetUiPreferences(!ShowFacilityStatus, LargeText), true);
            toolRoot = LaboratoryUiWidgets.Rect("PlacementTools", playRoot);
            editText = LaboratoryUiWidgets.Label("PlacementStatus", playRoot, 16);
            rotateButton = LaboratoryUiWidgets.Button("Rotate", toolRoot, "90° 회전", () => screen.LayoutEpoch);
            confirmButton = LaboratoryUiWidgets.Button("Confirm", toolRoot, "배치 확정", () => screen.LayoutEpoch);
            cancelButton = LaboratoryUiWidgets.Button("Cancel", toolRoot, "취소", () => screen.LayoutEpoch);
            navigationRoot = LaboratoryUiWidgets.Rect("Navigation", playRoot);
            buildMenuButton = IconButton("NavBuild", navigationRoot, LaboratoryIcon.Build, "건설", () => OpenPanel(LaboratoryPanel.Build));
            creatureMenuButton = IconButton("NavCreatures", navigationRoot, LaboratoryIcon.Creatures, "생명체", () => OpenPanel(LaboratoryPanel.Creatures));
            invasionMenuButton = IconButton("NavInvasion", navigationRoot, LaboratoryIcon.Invasion, "침공·보고", () => OpenPanel(LaboratoryPanel.Invasion));
            controlsScroll = LaboratoryUiWidgets.Scroll("Controls", playRoot, out var content);
            controls = new LaboratoryUiList(content, () => screen.LayoutEpoch);
            summaryRoot = LaboratoryUiWidgets.Panel("SelectionSummary", playRoot, LaboratoryUiWidgets.PanelColor).rectTransform;
            summaryText = LaboratoryUiWidgets.Label("Summary", summaryRoot, 18);
            detailsButton = LaboratoryUiWidgets.Button("Details", summaryRoot, "상세 보기", () => screen.LayoutEpoch);
            detailsButton.Bind(ExpandSelection, "details");
            closeButton = LaboratoryUiWidgets.Button("ClosePanel", playRoot, "닫기", () => screen.LayoutEpoch);
            closeButton.GetComponentInChildren<LaboratoryUiText>().Initialize(14);
            SetCompactButtonLabel(closeButton);
            closeButton.Bind(Back, "close");
            feedbackText = LaboratoryUiWidgets.Label("Feedback", playRoot, 15);
            CreateEndingScreen();
        }
        private LaboratoryUiButton IconButton(string key, Transform parent, LaboratoryIcon icon, string label, Action action, bool compact = false)
        {
            var button = LaboratoryUiWidgets.Button(key, parent, label, () => screen.LayoutEpoch);
            button.Bind(() => { if (CanClickScreen() && pendingSessionAction == LaboratorySessionAction.None) action(); }, key);
            SetIcon(button, icon, label, compact); return button;
        }
        private static void SetCompactButtonLabel(LaboratoryUiButton button)
        {
            var rect = button.GetComponentInChildren<Text>().rectTransform;
            rect.offsetMin = new Vector2(4, 4); rect.offsetMax = new Vector2(-4, -4);
        }
        private void SetIcon(LaboratoryUiButton button, LaboratoryIcon icon, string label, bool compact = false)
        {
            var graphic = button.transform.Find("Icon")?.GetComponent<Image>();
            if (graphic == null)
            {
                graphic = LaboratoryUiWidgets.Panel("Icon", button.transform, Color.white);
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
                    plate = LaboratoryUiWidgets.Panel("IconPlate", button.transform, new Color(.16f, .23f, .31f));
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
            var style = text.GetComponent<LaboratoryUiText>();
            if (style.BaseSize != 13) style.Initialize(13);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = compact || graphic.sprite == null ? Vector2.one : new Vector2(1, 0);
            text.rectTransform.pivot = new Vector2(.5f, 0);
            text.rectTransform.offsetMin = new Vector2(2, 2);
            text.rectTransform.offsetMax = compact || graphic.sprite == null ? new Vector2(-2, -2) : new Vector2(-2, 22);
        }
        private bool CanClickScreen() => isActiveAndEnabled && Controller != null && !Controller.IsApplicationPaused;
        private string UiContext => Controller == null ? "none" : Controller.Generation + ":" + Controller.Phase;
        private void BindTool(LaboratoryUiButton button, Action action, bool enabled)
        {
            var generation = Controller.Generation; var selection = SelectedId; var editing = IsEditing;
            button.Bind(() => { if (CanUseMap && generation == Controller.Generation && selection == SelectedId && editing == IsEditing) action(); }, UiContext + ":" + SelectedId + ":" + IsEditing);
            button.interactable = enabled && CanUseMap;
        }
        private void UpdateScreen()
        {
            if (Controller == null) return;
            EnsureScreen(); screen.RefreshLayout(); screen.SetVisible(isActiveAndEnabled);
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
            var top = screen.Portrait ? Mathf.Max(116, touch + 66) : Mathf.Max(touch + 16, 100 * UiTextScale);
            var footer = touch + 32; var cameraHeight = screen.Portrait ? touch + 6 : 0;
            var collapseX = w - touch - collapseWidth - margin - 8;
            var cameraWidth = 4 * touch + 18;
            var cameraX = collapseX - cameraWidth - 8;
            var headerWidth = screen.Portrait ? w - touch - collapseWidth - 42 : cameraX - margin - 12;
            // A smaller landscape title leaves two full HUD lines above the gauge, including large text.
            var titleSize = screen.Portrait ? 24 : 18;
            var titleStyle = titleText.GetComponent<LaboratoryUiText>();
            if (titleStyle.BaseSize != titleSize) titleStyle.Initialize(titleSize);
            var titleHeight = screen.Portrait ? 36 : 28 * UiTextScale;
            var hudY = 8 + titleHeight + 2;
            LaboratoryUiWidgets.Place(playRoot, 0, 0, w, h);
            LaboratoryUiWidgets.Place(titleText.rectTransform, margin, 8, headerWidth, titleHeight);
            LaboratoryUiWidgets.Place(gameMenuButton.GetComponent<RectTransform>(), w - touch - margin, 8, touch, touch);
            LaboratoryUiWidgets.Place(collapseButton.GetComponent<RectTransform>(), collapseX, 8, collapseWidth, touch);
            collapseButton.GetComponentInChildren<Text>().text = controlsCollapsed ? "펼치기" : "접기";
            collapseButton.interactable = CanClickScreen() && pendingSessionAction == LaboratorySessionAction.None;
            LaboratoryUiWidgets.Place(hudText.rectTransform, margin, hudY, headerWidth, top - hudY - 12);
            LaboratoryUiWidgets.Place(worldGauge, margin, top - 10, w - margin * 2, 6);
            var panelWidth = !screen.Portrait && HasPanel ? Mathf.Clamp(w * .34f, 310, 430) : 0;
            var panelHeight = screen.Portrait && HasPanel ? Mathf.Clamp(h * .37f, 230, 470) : HasSummary ? Mathf.Max(92, touch + 24) : 0;
            var panelGap = panelWidth > 0 ? 12 : 0;
            var mapWidth = w - 24 - panelWidth - panelGap;
            var mapHeight = Mathf.Max(80, h - top - footer - cameraHeight - panelHeight - 30 - (IsEditing ? 48 : 0));
            LaboratoryUiWidgets.Place(MapViewport, margin, top, mapWidth, mapHeight);
            // This toolbar remains a playRoot sibling of the clipped map, never a map overlay.
            LaboratoryUiWidgets.Place(cameraTools, screen.Portrait ? margin + mapWidth - cameraWidth : cameraX,
                screen.Portrait ? top + mapHeight + 4 : 8, cameraWidth, touch);
            var cameraButtons = new[] { zoomOutButton, zoomInButton, fitButton, statusButton };
            for (var i = 0; i < cameraButtons.Length; i++) LaboratoryUiWidgets.Place((RectTransform)cameraButtons[i].transform,
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
                LaboratoryUiWidgets.Place((RectTransform)closeButton.transform, x + width - touch, y, touch, touch);
                LaboratoryUiWidgets.Place((RectTransform)controlsScroll.transform, x, y + touch + 4, width, Mathf.Max(60, height - touch - 4));
            }
            if (HasSummary)
            {
                var detailsWidth = Mathf.Max(touch + 12, 120);
                LaboratoryUiWidgets.Place(summaryRoot, margin, panelY, w - 24, panelHeight);
                LaboratoryUiWidgets.Place(summaryText.rectTransform, 10, 8, w - touch - detailsWidth - 60, panelHeight - 12);
                LaboratoryUiWidgets.Place((RectTransform)detailsButton.transform, w - touch - detailsWidth - 42, 8, detailsWidth, touch);
                LaboratoryUiWidgets.Place((RectTransform)closeButton.transform, w - touch - margin, panelY + 8, touch, touch);
                summaryText.text = SelectionSummary();
            }
            navigationRoot.gameObject.SetActive(!IsEditing); toolRoot.gameObject.SetActive(IsEditing);
            LaboratoryUiWidgets.Place(navigationRoot, margin, h - footer, w - 24, touch);
            LaboratoryUiWidgets.Place(toolRoot, margin, h - footer, w - 24, touch);
            var nav = new[] { buildMenuButton, creatureMenuButton, invasionMenuButton };
            var tools = new[] { cancelButton, rotateButton, confirmButton };
            var itemWidth = (w - 40) / 3;
            for (var i = 0; i < 3; i++)
            {
                LaboratoryUiWidgets.Place((RectTransform)nav[i].transform, i * (itemWidth + 8), 0, itemWidth, touch);
                LaboratoryUiWidgets.Place((RectTransform)tools[i].transform, i * (itemWidth + 8), 0, itemWidth, touch);
                nav[i].interactable = CanUseMap && !IsEditing;
            }
            editText.gameObject.SetActive(IsEditing);
            LaboratoryUiWidgets.Place(editText.rectTransform, margin, h - footer - 44, w - 24, 42);
            LaboratoryUiWidgets.Place(feedbackText.rectTransform, margin, h - 28, w - 24, 26);
            feedbackText.text = uiNotice ?? "";
        }
        private string SelectionSummary()
        {
            if (!HasSelection) return "";
            Simulation.Registry.TryGet(SelectedId, out var selected);
            if (selected.Definition.Kind == ThingKind.Facility)
            {
                var status = StatusFor(selected);
                return ThingLabel(selected.Id) + "\n" + LaboratoryStatusText.Short(status)
                    + (status.HasProgress ? " · 진행 " + (status.ProgressFraction * 100).ToString("0") + "%" : "");
            }
            return ThingLabel(selected.Id) + "\n" + (!selected.IsPlaced ? "외부 활동 중" : selected.TryGetComponent<WorkerComponent>(out var worker)
                ? (worker.Job == null ? "작업 대기" : "작업 중 · 상세에서 확인") : "방출 대기");
        }
        private void UpdateHud()
        {
            LaboratoryUiWidgets.Place(worldGaugeFill, 0, 0, worldGauge.rect.width * (Invasion == null ? 0 : (float)Math.Min(1, (double)Invasion.Score / Invasion.Settings.TargetScore)), 6);
            if (Simulation == null) { hudText.text = "저장을 불러오지 못했습니다. 게임 메뉴에서 재시도하세요."; panel = LaboratoryPanel.Game; return; }
            var things = Simulation.Registry.Things;
            var organicCount = things.Where(t => t.Definition == organic).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            var cultureCount = things.Where(t => t.Definition == culture).Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
            hudText.text = "유기물 " + organicCount + " · 배양액 " + cultureCount + " · 생명체 " + things.Count(t => t.Definition.Kind == ThingKind.Creature)
                + "\n멸망 " + Invasion.Score + " / " + Invasion.Settings.TargetScore + (IsEditing ? " · 연구소 일시정지" : "");
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
                editText.text = (constructing ? "새 건설" : "시설 이동") + " · 위치 " + origin + (valid ? " · 확정 가능 (자재는 이후 운반)" : " · 겹침/영역 경계를 확인하세요.");
            }
            BindTool(rotateButton, () => RotatePreview(), editing);
            BindTool(confirmButton, () => ConfirmPlacement(), editing && (Simulation.ConstructionDefinition != null ? Simulation.ConstructionValidation.Succeeded : Simulation.Map.Commands.Edit.Validation.Succeeded));
            BindTool(cancelButton, () => CancelPlacement(), editing);
            BindTool(zoomInButton, () => mapScreen.ZoomBy(1.3f), Simulation != null);
            BindTool(zoomOutButton, () => mapScreen.ZoomBy(1 / 1.3f), Simulation != null);
            BindTool(fitButton, () => mapScreen.Fit(), Simulation != null);
            BindTool(statusButton, () => SetUiPreferences(!ShowFacilityStatus, LargeText), Simulation != null);
            SetIcon(statusButton, ShowFacilityStatus ? LaboratoryIcon.ShowStatus : LaboratoryIcon.HideStatus, ShowFacilityStatus ? "상태 ON" : "상태 OFF", true);
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
            controls.Begin(Mathf.Max(100, ((RectTransform)controlsScroll.transform).rect.width - 16));
            var canOperate = CanUseMap && !IsEditing;
            if (panel == LaboratoryPanel.Game) AddGameControls();
            else if (panel == LaboratoryPanel.License)
                controls.Text("FontLicense", uiAssets?.FontLicense != null ? uiAssets.FontLicense.text : "글꼴 라이선스를 찾을 수 없습니다.", 16);
            else if (Simulation != null)
            {
                switch (panel)
                {
                    case LaboratoryPanel.Selection:
                        if (Simulation.Registry.TryGet(SelectedId, out var selected)) AddSelectedControls(selected);
                        break;
                    case LaboratoryPanel.Build:
                        controls.Text("ConstructionTitle", "건설", 24);
                        controls.Text("BuildHint", "부지를 확정하면 자재 운반 후 시공합니다. 자재가 부족해도 부지를 지정할 수 있습니다.", 16);
                        UiAction("BuildGathering", "채집 시설 · 유기물 " + gathering.BuildCost, () => BeginGatheringConstruction(), canOperate);
                        UiAction("BuildStorage", "저장소 · 유기물 " + storage.BuildCost, () => BeginStorageConstruction(), canOperate);
                        UiAction("BuildProcessing", "가공 시설 · 유기물 " + processing.BuildCost, () => BeginProcessingConstruction(), canOperate);
                        UiAction("BuildCultivation", "배양 시설 · 유기물 " + cultivation.BuildCost, () => BeginCultivationConstruction(), canOperate);
                        controls.Text("Facilities", "설치한 시설", 22); AddRoster(ThingKind.Facility, canOperate); break;
                    case LaboratoryPanel.Creatures:
                        controls.Text("Creatures", "생명체", 24); AddRoster(ThingKind.Creature, canOperate);
                        controls.Text("PresetTitle", "기본 작업 우선순위", 22);
                        controls.Text("PresetHint", "0: 사용 안 함 · 낮은 숫자 우선\n동률: 건설 → 채집 → 운반 → 생산", 16);
                        AddPriorities("Preset", Simulation.Preset.ToArray(), values => SetPreset(values), canOperate); break;
                    case LaboratoryPanel.Candidates:
                        controls.Text("Candidates", "겹친 대상 선택", 22);
                        foreach (var id in candidates.ToArray()) if (Simulation.Registry.TryGet(id, out var candidate) && candidate.IsPlaced)
                            UiAction("Select_" + id.ToString("N"), ThingLabel(id) + " · " + CreatureActivity(candidate), () => SelectThing(id), canOperate);
                        break;
                    case LaboratoryPanel.Invasion: AddInvasionControls(canOperate); break;
                }
            }
            if (!string.IsNullOrEmpty(Controller.LastError)) controls.Text("LoadError", "진행을 불러오지 못했습니다.\n" + Controller.LastError);
            if (!string.IsNullOrEmpty(Controller.LastSaveError)) controls.Text("SaveError", "저장 실패 · 이전 저장은 유지됩니다.\n" + Controller.LastSaveError);
            controls.End();
            if (reportAnchor != null)
            {
                if (!controls.TryRestoreAnchor(reportAnchor, anchorOffset, out var y) && Invasion.Reports.Count > 0)
                {
                    controls.TryRestoreAnchor("Report_" + Invasion.Reports.First().Sequence, 0, out y);
                    uiNotice = "보존 기간이 지난 보고가 정리되어 남은 기록으로 이동했습니다.";
                }
                if (Mathf.Abs(controlsScroll.content.anchoredPosition.y - y) > .1f)
                { controlsScroll.StopMovement(); controlsScroll.content.anchoredPosition = new Vector2(0, y); }
            }
            panelScroll = controlsScroll.content.anchoredPosition;
        }
        private void AddRoster(ThingKind kind, bool canOperate)
        {
            var entries = Simulation.Registry.Things.Where(t => t.Definition.Kind == kind).OrderBy(t => t.CreationOrder).ToArray();
            if (entries.Length == 0) controls.Text("EmptyRoster", kind == ThingKind.Facility ? "아직 시설이 없습니다." : "아직 생명체가 없습니다.");
            foreach (var thing in entries)
            {
                var id = thing.Id;
                UiAction("Select_" + id.ToString("N"), ThingLabel(id) + " · " + CreatureActivity(thing), () =>
                {
                    if (thing.IsPlaced) SelectThing(id);
                    else { SelectedId = id; SelectionChanged(); }
                }, canOperate);
            }
        }
        private string CreatureActivity(Thing thing) => !thing.IsPlaced ? "외부 활동"
            : thing.Definition.Kind == ThingKind.Facility ? LaboratoryStatusText.Short(StatusFor(thing))
            : thing.TryGetComponent<WorkerComponent>(out var worker) ? worker.Job == null ? "작업 대기" : "작업 중" : "방출 대기";
        private void AddInvasionControls(bool canOperate)
        {
            controls.Text("WorldTitle", "침공·보고", 24);
            controls.Text("InvasionCounts", "방출 대기 " + Simulation.Invasion.WaitingCount + " · 외부 활동 " + Simulation.Invasion.ExternalCount);
            UiAction("Release", "대기 중인 침공형 전원 방출 (" + Simulation.Invasion.WaitingCount + ")", () => ReleaseInvaders(), canOperate && Simulation.Invasion.WaitingCount > 0);
            UiAction("Reports", ReportsExpanded ? "보고 접기" : "전과 보고 보기 (" + Invasion.Reports.Count + ")", ToggleReports);
            if (!ReportsExpanded) return;
            UiAction("LatestReports", "최신 보고로", () => { controlsScroll.StopMovement(); controlsScroll.content.anchoredPosition = Vector2.zero; });
            if (Invasion.Reports.Count == 0) controls.Text("NoReports", "아직 전과 보고가 없습니다.");
            foreach (var report in Invasion.Reports.Reverse()) controls.Text("Report_" + report.Sequence,
                "#" + report.Sequence + " · " + ThingLabel(report.InvaderId) + "\n" + report.Text + " +" + report.Score + " (" + report.Time.ToString("F1") + "초)");
        }
        private void AddGameControls()
        {
            controls.Text("GameTitle", "게임 메뉴", 24);
            if (pendingSessionAction != LaboratorySessionAction.None)
            {
                controls.Text("SessionWarning", pendingSessionAction == LaboratorySessionAction.Reload
                    ? "최근 저장을 불러올까요?\n현재 진행과 미확정 배치를 저장된 상태로 교체합니다."
                    : "새 판을 시작할까요?\n현재 진행과 저장 슬롯을 초기 상태로 바꿉니다. 표시 설정은 유지됩니다.");
                UiAction("ConfirmSessionAction", "확인 · 진행 변경", () => ConfirmSessionAction());
                UiAction("CancelSessionAction", "취소 · 현재 판 유지", CancelSessionAction); return;
            }
            UiAction("Save", "지금 저장", () => { uiNotice = Save() ? "진행을 저장했습니다." : "저장하지 못했습니다. 이전 저장을 유지합니다."; }, Controller.Current != null);
            UiAction("Reload", "최근 저장 불러오기", () => RequestSessionAction(LaboratorySessionAction.Reload));
            UiAction("Restart", "새 판 시작", () => RequestSessionAction(LaboratorySessionAction.Restart));
            controls.Text("UiPreferences", "화면 표시", 22);
            controls.Text("MapControlsHelp", "지도 조작: 돋보기 −/＋는 축소/확대, 네 모서리 표시는 전체 보기, 눈/가린 눈은 시설 상태 표시입니다.", 16);
            UiAction("StatusPreference", "시설 상태 표시: " + (ShowFacilityStatus ? "ON" : "OFF"), () => SetUiPreferences(!ShowFacilityStatus, LargeText));
            UiAction("TextPreference", "글자 크기: " + (LargeText ? "크게" : "보통"), () => SetUiPreferences(ShowFacilityStatus, !LargeText));
            if (!string.IsNullOrEmpty(uiSettings?.LastError))
            {
                controls.Text("UiSettingsError", "화면 설정 저장 오류 · 게임 진행 저장과 별개입니다.\n" + uiSettings.LastError);
                UiAction("RetryUiSettings", "화면 설정 저장 재시도", () => { uiSettings.TrySave(); });
            }
            UiAction("FontLicense", "글꼴 라이선스", () => OpenPanel(LaboratoryPanel.License));
        }
    }
}
