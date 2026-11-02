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
        private UiList resultRows;
        private UiButton newRunButton;
        private UiButton retrySaveButton;
        private Text restartCaption;

        private void CreateEndingScreen()
        {
            endingRoot = UiWidgets.Panel("EndingScreen", screen.SafeRoot, UiWidgets.Background).rectTransform;
            endingTitle = UiWidgets.Label("EndingTitle", endingRoot, 30); endingTitle.alignment = TextAnchor.MiddleCenter;
            endingTitle.color = new Color(1f, 0.65f, 0.4f);
            var worldRect = UiWidgets.Rect("Temp_EndingWorld", endingRoot);
            endingWorld = worldRect.gameObject.AddComponent<LaboratoryUiWorld>(); endingWorld.raycastTarget = false;
            endingCaption = UiWidgets.Label("EndingCaption", endingRoot, 18); endingCaption.alignment = TextAnchor.MiddleCenter;
            resultScroll = UiWidgets.Scroll("ResultDetails", endingRoot, out var content);
            resultRows = new UiList(content, () => screen.LayoutEpoch);
            newRunButton = UiWidgets.Button("RestartFromResults", endingRoot, R("new_run"), () => screen.LayoutEpoch);
            retrySaveButton = UiWidgets.Button("RetrySave", endingRoot, R("retry_save"), () => screen.LayoutEpoch);
            restartCaption = UiWidgets.Label("RestartCaption", endingRoot, 16);
            restartCaption.text = R("restart_hint");
            endingRoot.gameObject.SetActive(false);
        }

        private void UpdateEndingScreen()
        {
            newRunButton.GetComponentInChildren<Text>().text = R("new_run");
            retrySaveButton.GetComponentInChildren<Text>().text = R("retry_save");
            restartCaption.text = R("restart_hint");
            var size = screen.Size; var w = size.x; var h = size.y;
            UiWidgets.Place(endingRoot, 0, 0, w, h);
            var results = Controller.Phase == SessionPhase.Results;
            var error = !string.IsNullOrEmpty(Controller.LastSaveError) || !string.IsNullOrEmpty(Controller.LastError);
            endingTitle.text = results ? R("ended") : R("last_signal");
            UiWidgets.Place(endingTitle.rectTransform, 16, 14, w - 32, 74);
            var progress = results ? 1f : (float)Ending.Progress;
            endingWorld.Progress = progress;
            endingCaption.text = results ? R("signals_lost") : progress < 0.33f ? R("defenses_fall")
                : progress < 0.72f ? R("world_falls") : R("signals_fade");
            var detailsX = 16f; var detailsY = 90f; var detailsWidth = w - 32;
            if (screen.Portrait)
            {
                var worldSize = Mathf.Min(220, w - 64, h * 0.25f);
                UiWidgets.Place(endingWorld.rectTransform, (w - worldSize) / 2, 82, worldSize, worldSize);
                UiWidgets.Place(endingCaption.rectTransform, 16, 86 + worldSize, w - 32, 40);
                detailsY = 132 + worldSize;
            }
            else
            {
                var worldSize = Mathf.Min(w * 0.43f - 32, h - 180);
                UiWidgets.Place(endingWorld.rectTransform, 16 + (w * 0.43f - worldSize) / 2, 90, worldSize, worldSize);
                UiWidgets.Place(endingCaption.rectTransform, 16, 106 + worldSize, w * 0.43f, 48);
                detailsX = w * 0.47f; detailsWidth = w - detailsX - 16;
            }
            var touch = MinimumTouchSize;
            var footerHeight = (results ? touch + 74 : 0) + (error ? touch + 10 : 0);
            var scrollHeight = Mathf.Max(80, h - detailsY - footerHeight - 22);
            UiWidgets.Place((RectTransform)resultScroll.transform, detailsX, detailsY, detailsWidth, scrollHeight);
            var footerY = detailsY + scrollHeight + 8;
            newRunButton.gameObject.SetActive(results); restartCaption.gameObject.SetActive(results);
            retrySaveButton.gameObject.SetActive(error);
            if (results)
            {
                UiWidgets.Place((RectTransform)newRunButton.transform, detailsX, footerY, detailsWidth, touch);
                UiWidgets.Place(restartCaption.rectTransform, detailsX, footerY + touch + 6, detailsWidth, 64);
                footerY += touch + 74;
            }
            if (error) UiWidgets.Place((RectTransform)retrySaveButton.transform, detailsX, footerY, detailsWidth, touch);
            var request = Ending.Request; var generation = Controller.Generation;
            newRunButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) RestartFromResults(request); }, UiContext + ":" + request?.GetHashCode());
            newRunButton.interactable = CanClickScreen() && request != null;
            retrySaveButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) Save(); }, UiContext);
            retrySaveButton.interactable = CanClickScreen();
            resultRows.Style(UiFont, UiTextScale, MinimumTouchSize);
            resultRows.Begin(detailsWidth - 16);
            if (results && SceneFlowManager.Instance != null)
                resultRows.Action("ReturnToTitle", R("return_title"), () => SceneFlowManager.Instance.RequestReturnToTitle(), CanClickScreen(), UiContext);
            resultRows.Text("Stopped", R("stopped"));
            if (Invasion.LegacyEndVersion > 0) resultRows.Text("LegacyResult", R("legacy"));
            else
            {
                resultRows.Text("WorldScore", R("score", A("score", Invasion.Score), A("target", Invasion.Settings.TargetScore)), 26);
                resultRows.Text("ActivityCount", R("activities", A("count", Invasion.ActivitySequence)));
                resultRows.Text("OutsideCount", R("outside", A("count", Simulation.Invasion.ExternalCount)));
                resultRows.Text("ActiveTime", R("elapsed", A("seconds", Invasion.Elapsed)));
            }
            if (!results) resultRows.Text("InProgress", R("in_progress"));
            if (error)
            {
                resultRows.Text("EndingError", R(!string.IsNullOrEmpty(Controller.LastSaveError) ? "save_error" : "load_error"));
                resultRows.Text("SavePreserved", R("save_preserved"));
            }
            if (LastCommand != null && LastCommand.StartsWith("Start new run", StringComparison.Ordinal) && LastCommand.EndsWith(": rejected / failed", StringComparison.Ordinal))
                resultRows.Text("RestartError", R("restart_error"));
            resultRows.End(); endingScroll = resultScroll.content.anchoredPosition;
        }
    }
}
