using System;
using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        private Vector2 endingScroll;
        // Temp geometric artwork; replace DrawEndingWorld for final art without changing the flow.
        private void DrawEnding()
        {
            GUI.enabled = true;
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.025f, 0.035f, 0.055f));
            var results = Controller.Phase == SessionPhase.Results;
            var progress = results ? 1f : (float)Ending.Progress;
            var title = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            var center = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            var muted = new GUIStyle(center); muted.normal.textColor = new Color(0.65f, 0.71f, 0.78f);
            title.normal.textColor = new Color(1f, 0.67f, 0.45f);
            center.normal.textColor = Color.white;
            var left = new Rect(24, 26, Screen.width * 0.48f - 24, Screen.height - 52);
            var right = new Rect(Screen.width * 0.52f, 26, Screen.width * 0.48f - 24, Screen.height - 52);
            GUI.Label(new Rect(left.x, left.y, left.width, 40), results ? "WORLD ENDED" : "THE FINAL SIGNAL", title);
            var size = Mathf.Min(left.width - 24, left.height - 150, 370);
            DrawEndingWorld(new Rect(left.center.x - size / 2, left.y + 62, size, size), progress);
            GUI.Label(new Rect(left.x, left.y + 72 + size, left.width, 42),
                results ? "No signals remain." : progress < 0.33f ? "The last defenses fall."
                    : progress < 0.72f ? "The world fractures." : "The signals fade into silence.", center);
            Fill(new Rect(left.x + 20, left.yMax - 30, left.width - 40, 4), new Color(0.14f, 0.18f, 0.24f));
            Fill(new Rect(left.x + 20, left.yMax - 30, (left.width - 40) * progress, 4), new Color(0.95f, 0.4f, 0.22f));
            GUILayout.BeginArea(right);
            endingScroll = GUILayout.BeginScrollView(endingScroll);
            GUILayout.Space(18);
            GUILayout.Label(results ? "RUN COMPLETE" : "GOAL REACHED", title);
            GUILayout.Space(16);
            GUILayout.Label("Laboratory and outside activity have stopped.", muted);
            GUILayout.Space(20);
            if (Invasion.LegacyEndVersion > 0)
                GUILayout.Label("This world ended in an earlier save.\nActivity records are unavailable.", center);
            else
            {
                GUILayout.Label("WORLD SCORE", muted);
                GUILayout.Label(Invasion.Score + " / " + Invasion.Settings.TargetScore, title);
                GUILayout.Space(14);
                GUILayout.Label("Completed activities: " + Invasion.ActivitySequence, center);
                GUILayout.Label("Invaders outside: " + Simulation.Invasion.ExternalCount, center);
                GUILayout.Label("Active time: " + Invasion.Elapsed.ToString("0.0") + " s", center);
            }
            GUILayout.Space(12);
            GUILayout.EndScrollView();
            if (results)
            {
                // Capture this screen's lease, not a delegate that resolves a future session.
                var request = Ending.Request;
                if (GUILayout.Button("START A NEW RUN", GUILayout.Height(44))) RestartFromResults(request);
                GUILayout.Label("Start with one prototype and initial resources.\nThe saved run is replaced after saving succeeds.", muted);
            }
            else GUILayout.Label("Final sequence in progress…", muted);
            if (!string.IsNullOrEmpty(Controller.LastSaveError) || !string.IsNullOrEmpty(Controller.LastError))
            {
                GUILayout.Space(12);
                GUILayout.Label(!string.IsNullOrEmpty(Controller.LastSaveError)
                    ? "The run could not be saved." : "The saved run could not be loaded.", center);
                GUILayout.Label("The current run remains stopped. The last successful save is preserved.", muted);
                if (GUILayout.Button("Retry save", GUILayout.Height(30))) Save();
            }
            if (LastCommand != null && LastCommand.StartsWith("Start new run", StringComparison.Ordinal)
                && LastCommand.EndsWith(": rejected / failed", StringComparison.Ordinal))
                GUILayout.Label(LastCommand, muted);
            GUILayout.EndArea();
            GUI.color = Color.white;
        }

        private static void DrawEndingWorld(Rect rect, float progress)
        {
            const int width = 19;
            var cell = rect.width / width;
            for (var y = 0; y < width; y++)
                for (var x = 0; x < width; x++)
                {
                    var dx = x - 9; var dy = y - 9;
                    if (dx * dx + dy * dy > 80) continue;
                    var pattern = ((x * 31 + y * 17) % 29) / 29f;
                    var burning = progress > pattern * 0.7f;
                    var color = burning ? Color.Lerp(new Color(1f, 0.42f, 0.12f), new Color(0.12f, 0.07f, 0.08f), progress)
                        : pattern < 0.5f ? new Color(0.2f, 0.55f, 0.42f) : new Color(0.13f, 0.3f, 0.55f);
                    var scatter = Mathf.Max(0, progress - 0.35f) * 0.22f;
                    Fill(new Rect(rect.x + (x + dx * scatter) * cell, rect.y + (y + dy * scatter) * cell,
                        cell * (1 - progress * 0.4f), cell * (1 - progress * 0.4f)), color);
                }
        }

        private static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
