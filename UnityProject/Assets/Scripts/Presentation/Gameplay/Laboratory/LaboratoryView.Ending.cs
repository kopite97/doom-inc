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
            newRunButton = LaboratoryUiWidgets.Button("RestartFromResults", endingRoot, "START A NEW RUN", () => screen.LayoutEpoch);
            retrySaveButton = LaboratoryUiWidgets.Button("RetrySave", endingRoot, "Retry save", () => screen.LayoutEpoch);
            restartCaption = LaboratoryUiWidgets.Label("RestartCaption", endingRoot, 16);
            restartCaption.text = "Start with one prototype and initial resources. The saved run is replaced only after saving succeeds.";
            endingRoot.gameObject.SetActive(false);
        }

        private void UpdateEndingScreen()
        {
            var size = screen.Size; var w = size.x; var h = size.y;
            LaboratoryUiWidgets.Place(endingRoot, 0, 0, w, h);
            var results = Controller.Phase == SessionPhase.Results;
            var error = !string.IsNullOrEmpty(Controller.LastSaveError) || !string.IsNullOrEmpty(Controller.LastError);
            endingTitle.text = results ? "WORLD ENDED / RUN COMPLETE" : "THE FINAL SIGNAL";
            LaboratoryUiWidgets.Place(endingTitle.rectTransform, 16, 14, w - 32, 74);
            var progress = results ? 1f : (float)Ending.Progress;
            endingWorld.Progress = progress;
            endingCaption.text = results ? "No signals remain." : progress < 0.33f ? "The last defenses fall."
                : progress < 0.72f ? "The world fractures." : "The signals fade into silence.";
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
            var footerHeight = (results ? 112 : 0) + (error ? 58 : 0);
            var scrollHeight = Mathf.Max(80, h - detailsY - footerHeight - 22);
            LaboratoryUiWidgets.Place((RectTransform)resultScroll.transform, detailsX, detailsY, detailsWidth, scrollHeight);
            var footerY = detailsY + scrollHeight + 8;
            newRunButton.gameObject.SetActive(results); restartCaption.gameObject.SetActive(results);
            retrySaveButton.gameObject.SetActive(error);
            if (results)
            {
                LaboratoryUiWidgets.Place((RectTransform)newRunButton.transform, detailsX, footerY, detailsWidth, 48);
                LaboratoryUiWidgets.Place(restartCaption.rectTransform, detailsX, footerY + 54, detailsWidth, 52);
                footerY += 112;
            }
            if (error) LaboratoryUiWidgets.Place((RectTransform)retrySaveButton.transform, detailsX, footerY, detailsWidth, 48);
            var request = Ending.Request; var generation = Controller.Generation;
            newRunButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) RestartFromResults(request); }, UiContext + ":" + request?.GetHashCode());
            newRunButton.interactable = CanClickScreen() && request != null;
            retrySaveButton.Bind(() => { if (CanClickScreen() && generation == Controller.Generation) Save(); }, UiContext);
            retrySaveButton.interactable = CanClickScreen();
            resultRows.Begin(detailsWidth - 16);
            resultRows.Text("Stopped", "Laboratory and outside activity have stopped.");
            if (Invasion.LegacyEndVersion > 0) resultRows.Text("LegacyResult", "This world ended in an earlier save. Activity records are unavailable.");
            else
            {
                resultRows.Text("WorldScore", "WORLD SCORE\n" + Invasion.Score + " / " + Invasion.Settings.TargetScore, 26);
                resultRows.Text("ActivityCount", "Completed activities: " + Invasion.ActivitySequence);
                resultRows.Text("OutsideCount", "Invaders outside: " + Simulation.Invasion.ExternalCount);
                resultRows.Text("ActiveTime", "Active time: " + Invasion.Elapsed.ToString("0.0") + " s");
            }
            if (!results) resultRows.Text("InProgress", "Final sequence in progress…");
            if (error)
            {
                resultRows.Text("EndingError", !string.IsNullOrEmpty(Controller.LastSaveError) ? "The run could not be saved.\n" + Controller.LastSaveError : "The saved run could not be loaded.\n" + Controller.LastError);
                resultRows.Text("SavePreserved", "The current run remains stopped. The last successful save is preserved.");
            }
            if (LastCommand != null && LastCommand.StartsWith("Start new run", StringComparison.Ordinal) && LastCommand.EndsWith(": rejected / failed", StringComparison.Ordinal))
                resultRows.Text("RestartError", LastCommand);
            resultRows.End(); endingScroll = resultScroll.content.anchoredPosition;
        }
    }
}
