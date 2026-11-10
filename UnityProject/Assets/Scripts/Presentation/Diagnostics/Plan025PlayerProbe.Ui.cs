#if UNITY_EDITOR || (UNITY_ANDROID && DEVELOPMENT_BUILD)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MobileGame.Presentation
{
    // Explicit diagnostic requests only. Uses the product popup manager, prefabs and input.
    public sealed partial class Plan025PlayerProbe
    {
        [Serializable] private sealed class UiRequest { public string id, mode; }
        [Serializable] private sealed class UiEvent
        {
            public string utc, reason, parentState, childState, parentResult, childResult;
            public double realtime;
            public int frame, count, confirms, cancels;
            public bool foreground, blocked, ownerAlive;
        }
        [Serializable] private sealed class UiEvidence
        {
            public UiRequest request;
            public List<UiEvent> events = new List<UiEvent>();
        }
        private PopupScope uiOwner;
        private PopupHandle uiParent, uiChild;
        private UiEvidence uiEvidence;
        private string uiDirectory, uiLastState;
        private double uiPoll, uiStarted, uiCompleted = -1;
        private int uiConfirms, uiCancels;

        private bool PollUiScenario()
        {
            var now = Time.realtimeSinceStartupAsDouble;
            var flow = SceneFlowManager.Instance;
            if (uiDirectory != null)
            {
                if (uiChild == null && uiParent.State == PopupState.Open
                    && (uiEvidence.request.mode == "stack" || uiEvidence.request.mode == "nondismiss"))
                {
                    uiChild = flow.Popups.Show(new DialogPopupRequest(uiParent.Children, "plan025-child",
                        new LocalizedMessage("Common", "settings"), new LocalizedMessage("Laboratory", "map_help"),
                        new LocalizedMessage("Common", "confirm"), parent: uiParent,
                        canDismiss: uiEvidence.request.mode != "nondismiss"), () => uiConfirms++, () => uiCancels++);
                }
                var state = $"{uiParent.State}/{uiParent.Result}/{uiChild?.State}/{uiChild?.Result}/{flow.Popups.Count}/{uiConfirms}/{uiCancels}";
                if (state != uiLastState) { uiLastState = state; RecordUi("state"); }
                if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) RecordUi("android-back-key");
                if (uiParent.IsCompleted && uiCompleted < 0) uiCompleted = now;
                if ((uiCompleted >= 0 && now - uiCompleted > 5) || now - uiStarted > 180)
                {
                    RecordUi(uiCompleted >= 0 ? "completed" : "timeout");
                    uiOwner.Dispose(); uiOwner = null; uiDirectory = null; uiEvidence = null;
                    uiParent = uiChild = null;
                }
                return true;
            }
            if (now < uiPoll) return false;
            uiPoll = now + 1;
            var input = Path.Combine(root, "ui-request.json");
            if (!File.Exists(input)) return false;
            try
            {
                var request = JsonUtility.FromJson<UiRequest>(File.ReadAllText(input));
                if (request == null || string.IsNullOrEmpty(request.id) || request.id.Length > 80
                    || request.id.Any(c => !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '-' && c != '_')
                    || !new[] { "stack", "nondismiss", "intent", "owner-end" }.Contains(request.mode))
                    throw new InvalidOperationException("Invalid PLAN-025 UI request.");
                if (flow?.Popups?.Ready != true || flow.BlocksScreenInput || !flow.Popups.Foreground)
                    throw new InvalidOperationException("UI scenario requires an idle foreground product screen.");
                var directory = Path.Combine(root, request.id);
                if (Directory.Exists(directory)) throw new InvalidOperationException("UI scenario ID already exists.");
                Directory.CreateDirectory(directory); File.Move(input, Path.Combine(directory, "ui-request.json"));
                uiDirectory = directory; uiEvidence = new UiEvidence { request = request };
                uiOwner = new PopupScope(flow.ScenePopupScope); uiConfirms = uiCancels = 0;
                uiStarted = now; uiCompleted = -1; uiLastState = null;
                uiParent = request.mode == "stack" || request.mode == "nondismiss"
                    ? flow.Popups.Show(new SettingsPopupRequest(uiOwner, flow.Settings))
                    : flow.Popups.ShowConfirm(uiOwner, "plan025-intent", new LocalizedMessage("Common", "confirm"),
                        new LocalizedMessage("Loading", "return_prompt"), () => uiConfirms++, () => uiCancels++);
                RecordUi("started");
            }
            catch (Exception error)
            {
                uiOwner?.Dispose(); uiOwner = null; uiDirectory = null; uiEvidence = null;
                File.WriteAllText(Path.Combine(root, "ui-error.txt"), error.ToString());
                if (File.Exists(input)) File.Move(input, Path.Combine(root, "rejected-ui-" + DateTime.UtcNow.Ticks + ".json"));
            }
            return uiDirectory != null;
        }
        private void RecordUiPause(bool paused)
        {
            if (uiDirectory == null) return;
            RecordUi(paused ? "application-pause" : "application-resume");
            if (paused && uiEvidence.request.mode == "owner-end")
            { uiOwner.Dispose(); RecordUi("owner-disposed-in-background"); }
        }
        private void OnApplicationFocus(bool focused)
        { if (uiDirectory != null) RecordUi(focused ? "focus-gained" : "focus-lost"); }
        private void RecordUi(string reason)
        {
            if (uiDirectory == null) return;
            var manager = SceneFlowManager.Instance?.Popups;
            uiEvidence.events.Add(new UiEvent { utc = DateTime.UtcNow.ToString("O"), reason = reason,
                realtime = Time.realtimeSinceStartupAsDouble, frame = Time.frameCount,
                parentState = uiParent?.State.ToString(), childState = uiChild?.State.ToString(),
                parentResult = uiParent?.Result?.ToString(), childResult = uiChild?.Result?.ToString(),
                count = manager?.Count ?? -1, confirms = uiConfirms, cancels = uiCancels,
                foreground = manager?.Foreground ?? false, blocked = manager?.BlocksInput ?? false,
                ownerAlive = uiOwner?.IsAlive ?? false });
            File.WriteAllText(Path.Combine(uiDirectory, "ui-events.json"), JsonUtility.ToJson(uiEvidence, true));
        }
    }
}
#endif
