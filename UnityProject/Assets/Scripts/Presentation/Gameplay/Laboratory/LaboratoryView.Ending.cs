using System;
using MobileGame.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private Vector2 endingScroll;
        private RectTransform endingRoot;
        private Text endingTitle;
        private Text endingCaption;
        private LaboratoryUiWorld endingWorld;
        private ScrollRect resultScroll;
        private LaboratoryUiList resultRows;
        private LaboratoryUiButton newRunButton;
        private LaboratoryUiButton retrySaveButton;
        private Text restartCaption;

        private void CreateEndingScreen()
        {
            endingRoot = LaboratoryUiWidgets.Panel("EndingScreen", screen.SafeRoot, LaboratoryUiWidgets.Background).rectTransform;
            endingTitle = LaboratoryUiWidgets.Label("EndingTitle", endingRoot, 30); endingTitle.alignment = TextAnchor.MiddleCenter;
            endingTitle.color = new Color(1f, 0.65f, 0.4f);
            var worldRect = LaboratoryUiWidgets.Rect("Temp_EndingWorld", endingRoot);
            endingWorld = worldRect.gameObject.AddComponent<LaboratoryUiWorld>(); endingWorld.raycastTarget = false;
            endingCaption = LaboratoryUiWidgets.Label("EndingCaption", endingRoot, 18); endingCaption.alignment = TextAnchor.MiddleCenter;
            resultScroll = LaboratoryUiWidgets.Scroll("ResultDetails", endingRoot, out var content);
            resultRows = new LaboratoryUiList(content, () => screen.LayoutEpoch);
            newRunButton = LaboratoryUiWidgets.Button("RestartFromResults", endingRoot, "새 판 시작", () => screen.LayoutEpoch);
            retrySaveButton = LaboratoryUiWidgets.Button("RetrySave", endingRoot, "저장 재시도", () => screen.LayoutEpoch);
            restartCaption = LaboratoryUiWidgets.Label("RestartCaption", endingRoot, 16);
            restartCaption.text = "프로토타입 1개체와 초기 자원으로 시작합니다. 저장에 성공하면 이전 판을 초기 상태로 교체합니다.";
            endingRoot.gameObject.SetActive(false);
        }

        private void UpdateEndingScreen()
        {
            var size = screen.Size; var w = size.x; var h = size.y;
            LaboratoryUiWidgets.Place(endingRoot, 0, 0, w, h);
            var results = Controller.Phase == SessionPhase.Results;
            var error = !string.IsNullOrEmpty(Controller.LastSaveError) || !string.IsNullOrEmpty(Controller.LastError);
            endingTitle.text = results ? "세계 멸망 · 한 판 종료" : "마지막 신호";
            LaboratoryUiWidgets.Place(endingTitle.rectTransform, 16, 14, w - 32, 74);
            var progress = results ? 1f : (float)Ending.Progress;
            endingWorld.Progress = progress;
            endingCaption.text = results ? "모든 신호가 사라졌습니다." : progress < 0.33f ? "마지막 방어선이 무너집니다."
                : progress < 0.72f ? "세계가 붕괴하고 있습니다." : "신호가 점차 사라집니다.";
            var detailsX = 16f; var detailsY = 90f; var detailsWidth = w - 32;
            if (screen.Portrait)
            {
                var worldSize = Mathf.Min(220, w - 64, h * 0.25f);
                LaboratoryUiWidgets.Place(endingWorld.rectTransform, (w - worldSize) / 2, 82, worldSize, worldSize);
                LaboratoryUiWidgets.Place(endingCaption.rectTransform, 16, 86 + worldSize, w - 32, 40);
                detailsY = 132 + worldSize;
            }
            else
            {
                var worldSize = Mathf.Min(w * 0.43f - 32, h - 180);
                LaboratoryUiWidgets.Place(endingWorld.rectTransform, 16 + (w * 0.43f - worldSize) / 2, 90, worldSize, worldSize);
                LaboratoryUiWidgets.Place(endingCaption.rectTransform, 16, 106 + worldSize, w * 0.43f, 48);
                detailsX = w * 0.47f; detailsWidth = w - detailsX - 16;
            }
            var touch = MinimumTouchSize;
            var footerHeight = (results ? touch + 74 : 0) + (error ? touch + 10 : 0);
            var scrollHeight = Mathf.Max(80, h - detailsY - footerHeight - 22);
            LaboratoryUiWidgets.Place((RectTransform)resultScroll.transform, detailsX, detailsY, detailsWidth, scrollHeight);
            var footerY = detailsY + scrollHeight + 8;
            newRunButton.gameObject.SetActive(results); restartCaption.gameObject.SetActive(results);
            retrySaveButton.gameObject.SetActive(error);
            if (results)
            {
                LaboratoryUiWidgets.Place((RectTransform)newRunButton.transform, detailsX, footerY, detailsWidth, touch);
                LaboratoryUiWidgets.Place(restartCaption.rectTransform, detailsX, footerY + touch + 6, detailsWidth, 64);
                footerY += touch + 74;
            }
            if (error) LaboratoryUiWidgets.Place((RectTransform)retrySaveButton.transform, detailsX, footerY, detailsWidth, touch);
            var request = Ending.Request; var generation = Controller.Generation;
            newRunButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) RestartFromResults(request); }, UiContext + ":" + request?.GetHashCode());
            newRunButton.interactable = CanClickScreen() && request != null;
            retrySaveButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) Save(); }, UiContext);
            retrySaveButton.interactable = CanClickScreen();
            resultRows.Begin(detailsWidth - 16);
            resultRows.Text("Stopped", "연구소와 외부 활동이 모두 정지했습니다.");
            if (Invasion.LegacyEndVersion > 0) resultRows.Text("LegacyResult", "이전 버전에서 종료된 판입니다. 활동 기록은 남아 있지 않습니다.");
            else
            {
                resultRows.Text("WorldScore", "멸망 점수\n" + Invasion.Score + " / " + Invasion.Settings.TargetScore, 26);
                resultRows.Text("ActivityCount", "완료한 외부 활동: " + Invasion.ActivitySequence);
                resultRows.Text("OutsideCount", "외부 침공형: " + Simulation.Invasion.ExternalCount);
                resultRows.Text("ActiveTime", "진행 시간: " + Invasion.Elapsed.ToString("0.0") + " s");
            }
            if (!results) resultRows.Text("InProgress", "멸망 연출 진행 중…");
            if (error)
            {
                resultRows.Text("EndingError", !string.IsNullOrEmpty(Controller.LastSaveError) ? "진행을 저장하지 못했습니다.\n" + Controller.LastSaveError : "저장을 불러오지 못했습니다.\n" + Controller.LastError);
                resultRows.Text("SavePreserved", "현재 판은 정지 상태를 유지합니다. 마지막으로 성공한 저장은 보존됩니다.");
            }
            if (LastCommand != null && LastCommand.StartsWith("Start new run", StringComparison.Ordinal) && LastCommand.EndsWith(": rejected / failed", StringComparison.Ordinal))
                resultRows.Text("RestartError", LastCommand);
            resultRows.End(); endingScroll = resultScroll.content.anchoredPosition;
        }
    }
}
