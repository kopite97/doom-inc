#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator DeviceTouchOpensOverlappingCandidatesAndEditsOnlySelectedWorker()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var prototype = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var support = GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId");
                var first = view.Simulation.Registry.Create(support);
                var second = view.Simulation.Registry.Create(support);
                var cell = prototype.Placement.Origin;
                Assert.That(view.Simulation.Map.Commands.Place(first.Id, cell).Succeeded, Is.True);
                Assert.That(view.Simulation.Map.Commands.Place(second.Id, cell).Succeeded, Is.True);
                yield return null;
                var point = map.CellScreenPoint(cell);
                var accepted = map.AcceptedTaps;

                // Both events enter one input update; EnhancedTouch must retain this short contact.
                input.Touch(1, TouchPhase.Began, point);
                input.Touch(1, TouchPhase.Ended, point);
                yield return null; yield return null; yield return null;
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Candidates), "A same-update tap must open the actual overlap candidates.");
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty), "An ambiguous tap must not silently choose a different creature.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                foreach (var id in new[] { prototype.Id, first.Id, second.Id })
                    Assert.That(view.FindUiButton("Select_" + id.ToString("N")), Is.Not.Null);
                yield return ClickVisibleButton(input, "Select_" + prototype.Id.ToString("N"));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));
                yield return input.Tap(2, map.CellScreenPoint(cell));
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Candidates));
                yield return ClickVisibleButton(input, "Select_" + first.Id.ToString("N"));
                Assert.That(view.SelectedId, Is.EqualTo(first.Id));
                yield return ClickVisibleButton(input, "WorkerPriority_0");
                var firstPriorities = first.GetComponent<WorkerComponent>().Priorities.ToArray();
                Assert.That(first.GetComponent<WorkerComponent>().UsesPreset, Is.False);
                Assert.That(second.GetComponent<WorkerComponent>().UsesPreset, Is.True);
                Assert.That(prototype.GetComponent<WorkerComponent>().UsesPreset, Is.True);

                yield return input.Tap(3, map.CellScreenPoint(cell));
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Candidates));
                yield return ClickVisibleButton(input, "Select_" + second.Id.ToString("N"));
                Assert.That(view.SelectedId, Is.EqualTo(second.Id));
                yield return ClickVisibleButton(input, "WorkerPriority_0");
                Assert.That(second.GetComponent<WorkerComponent>().UsesPreset, Is.False, "First click on second worker: " + view.LastCommand);
                yield return ClickVisibleButton(input, "WorkerPriority_0");
                Assert.That(second.GetComponent<WorkerComponent>().UsesPreset, Is.False);
                Assert.That(second.GetComponent<WorkerComponent>().Priorities, Is.Not.EqualTo(firstPriorities));
                Assert.That(first.GetComponent<WorkerComponent>().Priorities, Is.EqualTo(firstPriorities));
                yield return input.Tap(4, map.CellScreenPoint(cell));
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Candidates));
                yield return ClickVisibleButton(input, "Select_" + prototype.Id.ToString("N"));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id), "Every overlapping occupant must remain individually selectable.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 4), "UI priority clicks must not reach the map.");
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceDragAndPinchNeverSelectOnRemainingFingerRelease()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var prototype = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                yield return input.Tap(1, map.CellScreenPoint(prototype.Placement.Origin));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));
                var accepted = map.AcceptedTaps;
                var center = ScreenCenter(view.MapViewport);
                var radius = Mathf.Min(view.MapViewport.rect.width, view.MapViewport.rect.height) * view.UiCanvas.scaleFactor * .12f;
                var left = center - Vector2.right * radius; var right = center + Vector2.right * radius;

                input.Touch(10, TouchPhase.Began, center); yield return null;
                input.Touch(10, TouchPhase.Moved, right + Vector2.right * 30); yield return null;
                input.Touch(10, TouchPhase.Ended, right + Vector2.right * 30); yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));

                var zoom = map.Zoom;
                input.Touch(11, TouchPhase.Began, left);
                input.Touch(12, TouchPhase.Began, right); yield return null;
                left = center - Vector2.right * radius * 2;
                right = center + Vector2.right * radius * 2;
                input.Touch(11, TouchPhase.Moved, left);
                input.Touch(12, TouchPhase.Moved, right); yield return null;
                Assert.That(map.Zoom, Is.GreaterThan(zoom), "Device pinch events must zoom the actual map.");
                input.Touch(12, TouchPhase.Ended, right); yield return null;
                input.Touch(11, TouchPhase.Moved, center); yield return null;
                input.Touch(11, TouchPhase.Ended, center); yield return null; yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));

                // All fingers have lifted; a new map gesture must be accepted normally.
                yield return input.Tap(13, center);
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceMouseBuildsOnceAndUiAndMapBoundariesDoNotShareClicks()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var accepted = map.AcceptedTaps;
                yield return ClickVisibleButton(input, "BuildGathering");
                Assert.That(view.IsEditing, Is.True, "A real UI pointer click must enter construction.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));

                var cell = new Vector2Int(3, 3);
                yield return input.Click(map.CellScreenPoint(cell));
                Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(cell));
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1), "One mouse release must produce one preview.");
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                yield return ClickVisibleButton(input, "Confirm");
                Assert.That(view.IsEditing, Is.False);
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.EqualTo(1));
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1), "Confirm must not also click the map.");
                var selected = view.SelectedId;
                var outside = RectTransformUtility.WorldToScreenPoint(null,
                    view.MapViewport.TransformPoint(new Vector2(view.MapViewport.rect.xMin, view.MapViewport.rect.center.y))) - Vector2.right * 2;
                yield return input.Click(outside);
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                Assert.That(view.SelectedId, Is.EqualTo(selected));
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.EqualTo(1));
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceHeldTouchIsCanceledAcrossImmediatePauseAndResume()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var point = map.CellScreenPoint(worker.Placement.Origin);
                var accepted = map.AcceptedTaps;
                input.Touch(40, TouchPhase.Began, point); yield return null;
                view.SendMessage("OnApplicationPause", true);
                view.SendMessage("OnApplicationPause", false);
                input.Touch(40, TouchPhase.Ended, point); yield return null; yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                input.Touch(41, TouchPhase.Began, point); yield return null;
                view.SendMessage("OnApplicationFocus", false);
                view.SendMessage("OnApplicationFocus", true);
                input.Touch(41, TouchPhase.Ended, point); yield return null; yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                yield return input.Tap(42, point);
                Assert.That(view.SelectedId, Is.EqualTo(worker.Id));
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Vector2 ScreenCenter(RectTransform rect) =>
            RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));

        private static IEnumerator ClickVisibleButton(PointerScene input, string key)
        {
            yield return OpenActionPath(input, key);
            yield return RevealVisibleButton(input, key);
            yield return input.Click(ScreenCenter((RectTransform)input.View.FindUiButton(key).transform));
        }

        // Navigation is part of the input scenario: never call OpenPanel/SelectThing to reach an action.
        private static IEnumerator OpenActionPath(PointerScene input, string key)
        {
            yield return null;
            var existing = input.View.FindUiButton(key);
            if (existing != null && existing.gameObject.activeInHierarchy) yield break;
            string entry = null;
            if (key.StartsWith("Build", StringComparison.Ordinal)) entry = "NavBuild";
            else if (key.StartsWith("Select_", StringComparison.Ordinal)
                && Guid.TryParseExact(key.Substring(7), "N", out var id)
                && input.View.Simulation.Registry.TryGet(id, out var thing))
                entry = thing.Definition.Kind == ThingKind.Facility ? "NavBuild" : "NavCreatures";
            else if (key == "Release" || key == "Reports" || key == "LatestReports") entry = "NavInvasion";
            else if (key == "Save" || key == "Reload" || key == "Restart" || key == "TextPreference"
                || key == "StatusPreference" || key == "FontLicense" || key == "RetryUiSettings") entry = "GameMenu";
            else if (key.StartsWith("Preset_", StringComparison.Ordinal)) entry = "NavCreatures";
            else if (input.View.OpenedPanel == LaboratoryPanel.Selection)
            {
                var details = input.View.FindUiButton("Details");
                if (details != null && details.gameObject.activeInHierarchy && key != "Details") entry = "Details";
            }
            if (entry != null)
            {
                yield return RevealVisibleButton(input, entry);
                yield return input.Click(ScreenCenter((RectTransform)input.View.FindUiButton(entry).transform));
                yield return null;
            }
        }

        private static IEnumerator RevealVisibleButton(PointerScene input, string key)
        {
            yield return null;
            var button = input.View.FindUiButton(key);
            Assert.That(button, Is.Not.Null, "Missing UI button: " + key);
            Assert.That(button.gameObject.activeInHierarchy && button.interactable, Is.True, "Unavailable UI button: " + key);
            var rect = (RectTransform)button.transform;
            var scroll = button.GetComponentInParent<ScrollRect>();
            if (scroll != null)
            {
                Canvas.ForceUpdateCanvases(); scroll.StopMovement();
                var local = scroll.content.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
                var overflow = scroll.content.rect.height - scroll.viewport.rect.height;
                var offset = scroll.content.rect.yMax - local.y - scroll.viewport.rect.height * .5f;
                scroll.verticalNormalizedPosition = overflow > 0 ? 1 - Mathf.Clamp01(offset / overflow) : 1;
                yield return null; yield return null;
                Assert.That(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, ScreenCenter(rect)), Is.True,
                    "The button must be inside its clipped viewport before a physical click: " + key);
            }
        }

        // Keep the real EventSystem and input actions. Pointer and Back-key devices are isolated
        // so hardware movement cannot contaminate these frame-by-frame integration scenarios.
        private sealed class PointerScene : IDisposable
        {
            private readonly InputDevice[] disabled;
            private readonly Mouse previousMouse;
            private readonly Touchscreen previousTouchscreen;
            private readonly Keyboard previousKeyboard;
            private readonly Mouse mouse;
            private readonly Touchscreen touchscreen;
            private readonly Keyboard keyboard;
            private readonly bool previousBackground;
            private readonly InputSettings.BackgroundBehavior previousInputBackground;
            private readonly InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
            private readonly string directory;
            public LaboratoryView View { get; private set; }
            public LaboratoryMapView Map => View.MapViewport.GetComponent<LaboratoryMapView>();

            public PointerScene()
            {
                previousBackground = Application.runInBackground; Application.runInBackground = true;
                // Host window focus must not disable these synthetic integration-test devices.
                // The lifecycle scenarios still exercise the production pause/focus callbacks explicitly.
                previousInputBackground = InputSystem.settings.backgroundBehavior;
                previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN013-Input-" + Guid.NewGuid().ToString("N"));
                previousMouse = Mouse.current; previousTouchscreen = Touchscreen.current; previousKeyboard = Keyboard.current;
                disabled = InputSystem.devices.Where(d => (d is UnityEngine.InputSystem.Pointer || d is Keyboard) && d.enabled).ToArray();
                foreach (var device in disabled) InputSystem.DisableDevice(device);
                mouse = InputSystem.AddDevice<Mouse>(); touchscreen = InputSystem.AddDevice<Touchscreen>(); keyboard = InputSystem.AddDevice<Keyboard>();
            }

            public IEnumerator Reopen()
            {
                if (View != null) UnityEngine.Object.DestroyImmediate(View.gameObject);
                View = null;
                yield return null;
                yield return Load();
            }

            public IEnumerator Escape()
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; yield return null;
            }

            public IEnumerator Load()
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                yield return LoadIsolatedScene(Path.Combine(directory, "session.json"), 30,
                    loaded => { View = loaded; View.SimulationRate = 0; });
                yield return WaitFor(() => View.MapViewport != null && View.MapViewport.rect.width > 0 && View.CanInteract,
                    "The actual input scene did not become interactive.");
                yield return null; yield return null;
                Assert.That(mouse.enabled && touchscreen.enabled && keyboard.enabled, Is.True,
                    "The isolated input devices must remain enabled even when the host Editor window is unfocused.");
            }

            public void Touch(int id, TouchPhase phase, Vector2 point) => InputSystem.QueueStateEvent(touchscreen,
                new TouchState { touchId = id, phase = phase, position = point, pressure = phase == TouchPhase.Ended ? 0 : 1 });

            public IEnumerator Tap(int id, Vector2 point)
            {
                Touch(id, TouchPhase.Began, point); yield return null;
                Touch(id, TouchPhase.Ended, point); yield return null; yield return null;
            }

            public IEnumerator Click(Vector2 point)
            {
                // Switching from touch to mouse recreates the module's pointer model. Send a real move
                // even when the next control occupies the same coordinates as a previous mouse click.
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point + Vector2.one * 2 }); yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left)); yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null; yield return null;
            }

            public void Dispose()
            {
                if (View != null) UnityEngine.Object.DestroyImmediate(View.gameObject);
                if (mouse.added) InputSystem.RemoveDevice(mouse);
                if (touchscreen.added) InputSystem.RemoveDevice(touchscreen);
                if (keyboard.added) InputSystem.RemoveDevice(keyboard);
                foreach (var device in disabled) if (device.added) InputSystem.EnableDevice(device);
                if (previousMouse?.added == true) previousMouse.MakeCurrent();
                if (previousTouchscreen?.added == true) previousTouchscreen.MakeCurrent();
                if (previousKeyboard?.added == true) previousKeyboard.MakeCurrent();
                InputSystem.settings.backgroundBehavior = previousInputBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
                Application.runInBackground = previousBackground;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
#endif
