#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("PlacementPreview")]
        public IEnumerator PlacementPreviewStartsVisibleAndDragsInBothModesAndOrientations()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, false);
                var moving = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(2, 2), QuarterTurn.Clockwise90, true);
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    foreach (var key in new[] { "gatheringId", "processingId", "cultivationId", "storageId" })
                    {
                        map.Fit(); map.ZoomBy(3); yield return null;
                        Assert.That(view.BeginConstruction(GameDataStore.Instance.Catalog.Selected<FacilityData>(key)), Is.True);
                        yield return null; yield return null;
                        var origin = PreviewOrigin(view);
                        Assert.That(origin, Is.Not.EqualTo(Vector2Int.zero), "Start near the visible center, not the old (0,0) default.");
                        AssertPreviewVisible(input);
                        yield return CapturePlacement("initial-" + key + "-" + size.x);
                        Assert.That(view.CancelPlacement(), Is.True); yield return null; yield return null;
                    }
                    foreach (var move in new[] { false, true })
                    {
                        map.Fit(); yield return null;
                        if (move) { map.ZoomBy(4); view.SelectThing(moving.Id); Assert.That(view.BeginPlacement(), Is.True); }
                        else Assert.That(view.BeginStorageConstruction(), Is.True);
                        yield return null; yield return null;
                        AssertPreviewVisible(input);
                        if (move) Assert.That(PreviewOrigin(view), Is.EqualTo(moving.Placement.Origin));
                        var origin = PreviewOrigin(view); var cameraPoint = map.CellScreenPoint(Vector2.zero);
                        var start = map.CellScreenPoint((Vector2)origin + new Vector2(.2f, .3f));
                        var destination = start + map.CellScreenPoint(new Vector2(2, 0)) - map.CellScreenPoint(Vector2.zero);
                        yield return PlacementMouse(start, true);
                        yield return PlacementMouse(destination, true);
                        Assert.That(PreviewOrigin(view), Is.EqualTo(origin + new Vector2Int(2, 0)));
                        yield return PlacementMouse(destination, false);
                        Assert.That(Vector2.Distance(map.CellScreenPoint(Vector2.zero), cameraPoint), Is.LessThan(.01f));
                        Assert.That(view.IsEditing, Is.True, "Releasing is not confirmation.");
                        origin = PreviewOrigin(view); start = map.CellScreenPoint(origin);
                        destination = start + map.CellScreenPoint(new Vector2(0, 1)) - map.CellScreenPoint(Vector2.zero);
                        input.Touch(801, TouchPhase.Began, start); yield return null;
                        input.Touch(801, TouchPhase.Moved, destination); yield return null; yield return null;
                        input.Touch(801, TouchPhase.Ended, destination); yield return null; yield return null;
                        Assert.That(PreviewOrigin(view), Is.EqualTo(origin + Vector2Int.up));
                        Assert.That(moving.Placement.Origin, Is.EqualTo(new Vector2Int(2, 2)));
                        yield return CapturePlacement((move ? "move-drag-" : "build-drag-") + size.x);
                        Assert.That(view.CancelPlacement(), Is.True); yield return null; yield return null;
                        Assert.That(map.World.Root.Find("PlacementPreview").gameObject.activeSelf, Is.False);
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("PlacementPreview")]
        public IEnumerator PlacementPreviewShowsProblemCellsAndLocalizedReasonsForConstructionAndMove()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, true);
                var obstacle = CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(0, 3), QuarterTurn.None, true);
                var moving = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(7, 3), QuarterTurn.None, true);
                foreach (var language in new[] { "ko", "en" })
                {
                    yield return SelectLanguage(view, language); view.Popups.Top.Cancel(); yield return PopupSettled(view);
                    foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return SettleScreenStateSize(size);
                        foreach (var move in new[] { false, true })
                        {
                            if (move) { view.SelectThing(moving.Id); view.BeginPlacement(); }
                            else view.BeginStorageConstruction();
                            yield return null; yield return null;
                            map.Fit();
                            view.PreviewAt(obstacle.Placement.Origin); yield return null; yield return null;
                            var status = PlacementStatus(view);
                            Assert.That(status.text, Does.Contain(language == "ko" ? "다른 시설과 겹칩니다" : "Overlaps another facility"));
                            Assert.That(view.FindUiButton("Confirm").interactable, Is.False);
                            Assert.That(ProblemCells(map), Is.GreaterThan(0));
                            Assert.That(status.preferredHeight, Is.LessThanOrEqualTo(status.rectTransform.rect.height + 2));
                            yield return CapturePlacement((move ? "move-overlap-" : "build-overlap-") + language + "-" + size.x);
                            view.PreviewAt(new Vector2Int(-1, 3)); yield return null; yield return null;
                            Assert.That(status.text, Does.Contain(language == "ko" ? "건설 영역 밖" : "Outside the construction area"));
                            Assert.That(ProblemCells(map), Is.GreaterThan(0));
                            Assert.That(view.ConfirmPlacement(), Is.False);
                            yield return CapturePlacement((move ? "move-outside-" : "build-outside-") + language + "-" + size.x);
                            view.PreviewAt(move ? moving.Placement.Origin : new Vector2Int(4, 6));
                            for (var i = 0; i < 4; i++)
                            {
                                view.RotatePreview(); yield return null; yield return null;
                                Assert.That(ProblemCells(map), Is.Zero);
                                Assert.That(view.FindUiButton("Confirm").interactable, Is.True);
                                Assert.That(status.text, Does.Not.Contain(language == "ko" ? "겹칩니다" : "Overlaps"));
                            }
                            Assert.That(view.CancelPlacement(), Is.True); yield return null; yield return null;
                        }
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("PlacementPreview")]
        public IEnumerator PlacementDragPreservesCandidateAcrossUiPinchAndCanceledInput()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.BeginStorageConstruction(); yield return null; yield return null;
                map.Fit(); map.ZoomBy(2); view.PreviewAt(new Vector2Int(5, 4)); yield return null; yield return null;
                var origin = PreviewOrigin(view); var start = map.CellScreenPoint(origin);
                var end = map.CellScreenPoint(origin + Vector2Int.right);
                input.Touch(901, TouchPhase.Began, start); yield return null;
                input.Touch(901, TouchPhase.Moved, end); yield return null; yield return null;
                var draft = PreviewOrigin(view);
                Assert.That(draft, Is.EqualTo(origin + Vector2Int.right));
                var cancel = (RectTransform)view.FindUiButton("Cancel").transform;
                var uiPoint = RectTransformUtility.WorldToScreenPoint(null, cancel.TransformPoint(cancel.rect.center));
                input.Touch(901, TouchPhase.Moved, uiPoint); yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                input.Touch(901, TouchPhase.Ended, uiPoint); yield return null; yield return null;
                Assert.That(view.IsEditing, Is.True, "Drag release over Cancel must not click it.");
                input.Touch(905, TouchPhase.Began, uiPoint); yield return null;
                input.Touch(905, TouchPhase.Moved, map.CellScreenPoint(draft + Vector2Int.left)); yield return null;
                input.Touch(905, TouchPhase.Ended, map.CellScreenPoint(draft + Vector2Int.left)); yield return null; yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft), "A UI-owned gesture cannot become a placement drag.");
                start = map.CellScreenPoint(draft);
                input.Touch(906, TouchPhase.Began, start); yield return null;
                input.Touch(906, TouchPhase.Moved, new Vector2(-10, -10)); yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                input.Touch(906, TouchPhase.Moved, start); yield return null;
                input.Touch(906, TouchPhase.Ended, start); yield return null; yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft), "Leaving and returning must keep the original grab offset.");
                start = map.CellScreenPoint(draft);
                input.Touch(902, TouchPhase.Began, start); yield return null;
                input.Touch(903, TouchPhase.Began, start + new Vector2(100, 0)); yield return null;
                var zoom = map.Zoom;
                input.Touch(903, TouchPhase.Moved, start + new Vector2(140, 0)); yield return null; yield return null;
                Assert.That(map.Zoom, Is.GreaterThan(zoom));
                input.Touch(903, TouchPhase.Ended, start + new Vector2(140, 0)); yield return null;
                input.Touch(902, TouchPhase.Moved, start + new Vector2(40, 0)); yield return null;
                input.Touch(902, TouchPhase.Ended, start + new Vector2(40, 0)); yield return null; yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                var before = map.CellScreenPoint(Vector2.zero);
                start = RectTransformUtility.WorldToScreenPoint(null, view.MapViewport.TransformPoint(
                    view.MapViewport.rect.center - view.MapViewport.rect.size * .3f));
                yield return PlacementMouse(start, true); yield return PlacementMouse(start + new Vector2(30, 20), true);
                yield return PlacementMouse(start + new Vector2(30, 20), false);
                Assert.That(Vector2.Distance(before, map.CellScreenPoint(Vector2.zero)), Is.GreaterThan(1));
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                start = map.CellScreenPoint(draft);
                input.Touch(904, TouchPhase.Began, start); yield return null;
                map.CancelInput();
                input.Touch(904, TouchPhase.Ended, start + new Vector2(50, 0)); yield return null; yield return null;
                Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                using (new LaboratoryGameViewSize(720, 1280))
                {
                    yield return SettleScreenStateSize(new Vector2Int(720, 1280));
                    Assert.That(PreviewOrigin(view), Is.EqualTo(draft));
                }
                view.CancelPlacement(); yield return null; yield return null;
                Assert.That(map.World.Root.Find("PlacementPreview").gameObject.activeSelf, Is.False);
                view.BeginStorageConstruction(); yield return null; yield return null;
                yield return input.Reopen();
                Assert.That(input.View.IsEditing, Is.False);
                Assert.That(input.Map.World.Root.Find("PlacementPreview"), Is.Null);
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Vector2Int PreviewOrigin(LaboratoryView view) => view.Simulation.ConstructionDefinition != null
            ? view.Simulation.ConstructionOrigin : view.Simulation.Map.Commands.Edit.Origin;

        private static TextMeshProUGUI PlacementStatus(LaboratoryView view)
            => view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "PlacementStatus");

        private static int ProblemCells(LaboratoryMapView map) => map.World.Root.GetComponentsInChildren<SpriteRenderer>()
            .Count(r => r.name.StartsWith("PlacementCell_") && r.color.r > .9f && r.color.g < .3f);

        private static void AssertPreviewVisible(PointerScene input)
        {
            var preview = input.Map.World.Root.Find("PlacementPreview");
            Assert.That(preview, Is.Not.Null); Assert.That(preview.gameObject.activeInHierarchy, Is.True);
            var point = input.Map.World.Camera.WorldToScreenPoint(preview.position);
            Assert.That(RectTransformUtility.RectangleContainsScreenPoint(input.View.MapViewport, point, null), Is.True);
        }

        private static IEnumerator PlacementMouse(Vector2 point, bool held)
        {
            var state = new MouseState { position = point };
            if (held) state = state.WithButton(MouseButton.Left);
            InputSystem.QueueStateEvent(Mouse.current, state); yield return null; yield return null;
        }

        private static IEnumerator CapturePlacement(string name)
        {
            yield return WaitFor(() => !UnityEditor.ShaderUtil.anythingCompiling, "Placement shaders did not finish compiling.");
            yield return AppSceneFlowTests.Capture("placement-preview", Path.Combine(Application.dataPath, "../Temp/PLAN-031/screens", name + ".png"));
        }
    }
}
#endif
