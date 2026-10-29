using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MobileGame.Presentation
{
    public enum LaboratoryPanel { None, Selection, Build, Creatures, Invasion, Game, Candidates, License }
    public enum LaboratorySessionAction { None, Reload, Restart }

    public sealed partial class LaboratoryView
    {
        [SerializeField] private LaboratoryUiAssets uiAssets;
        private UiSettingsStore uiSettings;
        private FacilityStatusQuery facilityStatus;
        private LaboratoryManager statusSession;
        private readonly Dictionary<string, Vector2> panelPositions = new Dictionary<string, Vector2>();
        private readonly List<Guid> candidates = new List<Guid>();
        private LaboratoryPanel panel;
        private bool detailsExpanded;
        private LaboratorySessionAction pendingSessionAction;
        private string activePanelKey;
        private string uiNotice;
        private LaboratoryPanel editReturnPanel;
        private bool editReturnExpanded;

        private void EnterEditingPresentation()
        {
            SavePanelPosition(); editReturnPanel = panel; editReturnExpanded = detailsExpanded;
            panel = LaboratoryPanel.None; InvalidateInput(); RestorePanelPosition();
        }
        private void LeaveEditingPresentation()
        {
            panel = editReturnPanel; detailsExpanded = editReturnExpanded;
            InvalidateInput(); RestorePanelPosition();
        }

        public LaboratoryUiAssets UiAssets => uiAssets;
        public Font UiFont => uiAssets?.Font != null ? uiAssets.Font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public bool ShowFacilityStatus => uiSettings?.Current.ShowFacilityStatus ?? true;
        public bool LargeText => uiSettings?.Current.LargeText ?? false;
        public float UiTextScale => LargeText ? 1.25f : 1;
        public float MinimumTouchSize => screen?.MinimumTouchSize ?? 48;
        public LaboratoryPanel OpenedPanel => panel;
        public bool DetailsExpanded => detailsExpanded;
        public LaboratorySessionAction PendingSessionAction => pendingSessionAction;
        public bool CanUseMap => CanInteract && pendingSessionAction == LaboratorySessionAction.None;

        public void ConfigureUiAssets(LaboratoryUiAssets assets) { uiAssets = assets; ApplyTextStyle(); }

        private void InitializeUiSettings()
        {
            uiSettings = new UiSettingsStore(UiSettingsStore.PathForGameSave(SavePath));
            uiSettings.Load();
        }
        public bool SetUiPreferences(bool showStatus, bool largeText)
        {
            if (uiSettings == null) return false;
            var statusChanged = ShowFacilityStatus != showStatus;
            var saved = uiSettings.TrySet(showStatus, largeText);
            uiNotice = statusChanged ? "시설 상태 표시: " + (showStatus ? "ON" : "OFF")
                : "글자 크기: " + (largeText ? "크게" : "보통");
            if (!saved) uiNotice += " · 설정 저장 실패";
            ApplyTextStyle(); InvalidateInput(); return saved;
        }
        private void ApplyTextStyle()
        {
            foreach (var label in GetComponentsInChildren<LaboratoryUiText>(true)) label.Apply(this);
        }
        public FacilityStatus StatusFor(Thing thing)
        {
            if (statusSession != Simulation) { statusSession = Simulation; facilityStatus = new FacilityStatusQuery(Simulation); }
            return facilityStatus.Read(thing);
        }
        private string PanelKey => panel + (panel == LaboratoryPanel.Selection ? ":" + SelectedId : "");
        private void SavePanelPosition()
        {
            if (controlsScroll != null && activePanelKey != null) panelPositions[activePanelKey] = controlsScroll.content.anchoredPosition;
        }
        private void RestorePanelPosition()
        {
            if (controlsScroll == null) return;
            var key = PanelKey;
            if (activePanelKey == key) return;
            SavePanelPosition(); activePanelKey = key;
            controlsScroll.StopMovement();
            controlsScroll.content.anchoredPosition = panelPositions.TryGetValue(key, out var position) ? position : Vector2.zero;
        }
        public void OpenPanel(LaboratoryPanel target)
        {
            if (!CanClickScreen() || pendingSessionAction != LaboratorySessionAction.None) return;
            if (IsEditing && target != LaboratoryPanel.Game && target != LaboratoryPanel.License) return;
            SavePanelPosition(); panel = target; detailsExpanded = target != LaboratoryPanel.Selection;
            controlsCollapsed = false; InvalidateInput(); RestorePanelPosition();
        }
        public void ExpandSelection()
        {
            if (SelectedId == Guid.Empty) return;
            panel = LaboratoryPanel.Selection; detailsExpanded = true; controlsCollapsed = false;
            mapScreen?.RequestSelectionVisibility();
            InvalidateInput(); RestorePanelPosition();
        }
        private void SelectionChanged()
        {
            SavePanelPosition(); panel = LaboratoryPanel.Selection; detailsExpanded = false; controlsCollapsed = false;
            candidates.Clear(); InvalidateInput(); RestorePanelPosition();
            mapScreen?.RequestSelectionVisibility();
        }
        public void ShowSelectionCandidates(IEnumerable<Guid> ids)
        {
            if (!CanUseMap || IsEditing) return;
            candidates.Clear();
            foreach (var id in ids)
                if (Simulation.Registry.TryGet(id, out var thing) && thing.IsPlaced && !candidates.Contains(id)) candidates.Add(id);
            if (candidates.Count == 1) { SelectThing(candidates[0]); return; }
            if (candidates.Count > 1) OpenPanel(LaboratoryPanel.Candidates);
        }
        public void Back()
        {
            if (!CanClickScreen() || Ending?.IsEnding == true) return;
            if (pendingSessionAction != LaboratorySessionAction.None) { CancelSessionAction(); return; }
            if (panel == LaboratoryPanel.License) { OpenPanel(LaboratoryPanel.Game); return; }
            if (panel == LaboratoryPanel.Game) { ClosePanel(); return; }
            if (IsEditing) { CancelPlacement(); return; }
            if (panel == LaboratoryPanel.Selection && detailsExpanded) { detailsExpanded = false; InvalidateInput(); return; }
            if (panel != LaboratoryPanel.None && panel != LaboratoryPanel.Selection) { ClosePanel(); return; }
            if (SelectedId != Guid.Empty) { SelectedId = Guid.Empty; panel = LaboratoryPanel.None; InvalidateInput(); return; }
            OpenPanel(LaboratoryPanel.Game);
        }
        private void ClosePanel()
        {
            SavePanelPosition(); panel = IsEditing || SelectedId == Guid.Empty ? LaboratoryPanel.None : LaboratoryPanel.Selection;
            detailsExpanded = false; controlsCollapsed = false; InvalidateInput(); RestorePanelPosition();
        }
        public void RequestSessionAction(LaboratorySessionAction action)
        {
            if (!CanClickScreen() || action == LaboratorySessionAction.None) return;
            OpenPanel(LaboratoryPanel.Game); pendingSessionAction = action; InvalidateInput();
        }
        public void CancelSessionAction() { pendingSessionAction = LaboratorySessionAction.None; InvalidateInput(); }
        public bool ConfirmSessionAction()
        {
            if (!CanClickScreen() || pendingSessionAction == LaboratorySessionAction.None) return false;
            var action = pendingSessionAction; pendingSessionAction = LaboratorySessionAction.None; InvalidateInput();
            var success = action == LaboratorySessionAction.Reload ? Reload() : Restart();
            uiNotice = success ? "진행을 변경했습니다." : "진행을 변경하지 못했습니다. 현재 상태는 유지됩니다.";
            if (!success) OpenPanel(LaboratoryPanel.Game);
            return success;
        }
        private void ResetNavigation()
        {
            pendingSessionAction = LaboratorySessionAction.None; panelPositions.Clear(); candidates.Clear();
            panel = LaboratoryPanel.None; activePanelKey = null; detailsExpanded = false; controlsCollapsed = false;
            editReturnPanel = LaboratoryPanel.None; editReturnExpanded = false;
            facilityStatus = null; statusSession = null; uiNotice = null;
        }
        private void PollBack()
        { if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) Back(); }
        private void SelectionUnavailable()
        {
            if (SelectedId != Guid.Empty && Simulation != null && !Simulation.Registry.TryGet(SelectedId, out _))
            { SelectedId = Guid.Empty; ClosePanel(); }
        }
    }
}
