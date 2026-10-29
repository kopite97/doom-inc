#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator DeviceZeroMaterialConstructionStillAcceptsValidSiteAndExplainsWaiting()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                // Fixture setup only: UI must permit designation even when there is no available material.
                foreach (var resource in view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource).ToArray())
                    view.Simulation.Registry.ConsumeResource(resource, resource.GetComponent<ResourceStackComponent>().Quantity);
                Assert.That(view.Simulation.Registry.Things.Any(t => t.Definition.Kind == ThingKind.Resource), Is.False);

                yield return ClickVisibleButton(input, "BuildStorage");
                Assert.That(view.IsEditing, Is.True);
                var origin = new Vector2Int(4, 3);
                yield return input.Tap(60, input.Map.CellScreenPoint(origin));
                Assert.That(view.Simulation.ConstructionValidation.Succeeded, Is.True);
                Assert.That(view.FindUiButton("Confirm").interactable, Is.True,
                    "Material shortage must not disable a spatially valid construction designation.");
                yield return ClickVisibleButton(input, "Confirm");
                var facility = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility);
                Assert.That(facility.Placement.Origin, Is.EqualTo(origin));
                Assert.That(view.IsEditing, Is.False);
                view.Controller.Advance(10);
                var construction = facility.GetComponent<ConstructionComponent>();
                Assert.That(construction.IsComplete, Is.False);
                Assert.That(construction.Delivered, Is.Zero);
                Assert.That(construction.Progress, Is.Zero);
                Assert.That(view.StatusFor(facility).Kind, Is.EqualTo(FacilityStatusKind.ConstructionMaterials));
                Assert.That(view.StatusFor(facility).Materials.WaitKind, Is.EqualTo(FacilityMaterialWaitKind.InsufficientTotal));
                yield return ClickVisibleButton(input, "Select_" + facility.Id.ToString("N"));
                yield return ExpandSelectedIfNeeded(input);
                Assert.That(VisibleUiText(view), Does.Contain("자재 대기"));
                Assert.That(VisibleUiText(view), Does.Contain("전체 자재 부족"));
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceStatusTapUsesSelectionNormallyAndPreviewInBothEditingModesWithOnAndOff()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var source = AddInputFacility(view, "gatheringId", new Vector2Int(3, 5));
                var moving = AddInputFacility(view, "storageId", new Vector2Int(10, 3));
                yield return WaitFor(() => map.TryStatusScreenPoint(source.Id, out _), "The actual facility status did not become visible.");
                Assert.That(map.TryStatusScreenPoint(source.Id, out var selectionPoint), Is.True);
                yield return input.Tap(61, selectionPoint);
                Assert.That(view.SelectedId, Is.EqualTo(source.Id), "An ordinary status tap must select its own facility.");
                Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeInHierarchy, Is.True,
                    "The first status tap opens that facility's caption.");
                yield return ExpandSelectedIfNeeded(input);
                yield return RevealVisibleButton(input, "Move");
                var scroll = view.UiCanvas.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "Controls");
                scroll.StopMovement(); yield return null;
                var selectedPanel = view.OpenedPanel; var expanded = view.DetailsExpanded;
                var scrollBeforeToggle = scroll.content.anchoredPosition;
                var progressBeforeToggle = FacilityStatusCaption(view, source.Id).transform.parent.Find("Track").gameObject.activeSelf;
                var acceptedBeforeToggle = map.AcceptedTaps;
                Assert.That(map.TryStatusScreenPoint(source.Id, out selectionPoint), Is.True);
                yield return input.Tap(68, selectionPoint);
                Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeSelf, Is.False,
                    "A second tap on the selected facility hides only its status caption.");
                Assert.That(FacilityStatusCaption(view, source.Id).transform.parent.Find("Icon").gameObject.activeInHierarchy, Is.True);
                Assert.That(FacilityStatusCaption(view, source.Id).transform.parent.Find("Track").gameObject.activeSelf, Is.EqualTo(progressBeforeToggle),
                    "Hiding caption text must retain any existing progress bar.");
                Assert.That(view.SelectedId, Is.EqualTo(source.Id));
                Assert.That(view.OpenedPanel, Is.EqualTo(selectedPanel));
                Assert.That(view.DetailsExpanded, Is.EqualTo(expanded));
                Assert.That(Vector2.Distance(scroll.content.anchoredPosition, scrollBeforeToggle), Is.LessThan(.1f),
                    "A status caption toggle must not reset the selected panel's reading position.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(acceptedBeforeToggle + 1));
                Assert.That(map.TryStatusScreenPoint(source.Id, out selectionPoint), Is.True);
                yield return input.Tap(69, selectionPoint);
                Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeInHierarchy, Is.True,
                    "A third tap reopens the same caption.");
                Assert.That(view.SelectedId, Is.EqualTo(source.Id));
                Assert.That(view.OpenedPanel, Is.EqualTo(selectedPanel));
                Assert.That(view.DetailsExpanded, Is.EqualTo(expanded));
                Assert.That(Vector2.Distance(scroll.content.anchoredPosition, scrollBeforeToggle), Is.LessThan(.1f));
                Assert.That(map.AcceptedTaps, Is.EqualTo(acceptedBeforeToggle + 2));

                Assert.That(map.TryStatusScreenPoint(source.Id, out selectionPoint), Is.True);
                yield return input.Tap(70, selectionPoint);
                yield return ClickVisibleButton(input, "ZoomIn");
                yield return ClickVisibleButton(input, "ZoomIn");
                Assert.That(map.Zoom, Is.GreaterThanOrEqualTo(1.65f));
                Assert.That(map.TryStatusScreenPoint(source.Id, out _), Is.True);
                Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeSelf, Is.False,
                    "Explicitly collapsed captions remain hidden at the near-zoom automatic-caption threshold.");
                var rotatedSize = Screen.width > Screen.height ? new Vector2Int(720, 1280) : new Vector2Int(1280, 720);
                using (new LaboratoryGameViewSize(rotatedSize.x, rotatedSize.y))
                {
                    yield return SettleScreenStateSize(rotatedSize);
                    Assert.That(map.TryStatusScreenPoint(source.Id, out _), Is.True);
                    Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeSelf, Is.False);
                    Assert.That(view.SelectedId, Is.EqualTo(source.Id));
                    Assert.That(view.OpenedPanel, Is.EqualTo(selectedPanel));
                    Assert.That(view.DetailsExpanded, Is.EqualTo(expanded));
                }
                yield return null; yield return null;
                yield return ClickVisibleButton(input, "FitMap");
                Assert.That(map.TryStatusScreenPoint(source.Id, out selectionPoint), Is.True);
                yield return input.Tap(71, selectionPoint);
                Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeInHierarchy, Is.True);

                foreach (var move in new[] { false, true })
                foreach (var show in new[] { true, false })
                {
                    if (!view.ShowFacilityStatus) yield return ClickVisibleButton(input, "ToggleStatus");
                    if (move)
                    {
                        yield return ClickVisibleButton(input, "Select_" + moving.Id.ToString("N"));
                        yield return ClickVisibleButton(input, "Move");
                    }
                    else yield return ClickVisibleButton(input, "BuildStorage");
                    Assert.That(view.IsEditing, Is.True);
                    yield return WaitFor(() => map.TryStatusScreenPoint(source.Id, out _), "Status disappeared from the editing viewport.");
                    Assert.That(map.TryStatusScreenPoint(source.Id, out var previewPoint), Is.True);
                    Assert.That(map.TryScreenCell(previewPoint, out var expectedCell), Is.True,
                        "This fixture deliberately taps a status label above an interior map cell.");
                    if (!show)
                    {
                        yield return ClickVisibleButton(input, "ToggleStatus");
                        Assert.That(view.ShowFacilityStatus, Is.False);
                        Assert.That(map.VisibleStatusCount, Is.Zero);
                    }
                    var selected = view.SelectedId; var accepted = map.AcceptedTaps;
                    var original = moving.Placement;
                    var captionBeforePreview = FacilityStatusCaption(view, source.Id).gameObject.activeSelf;
                    yield return input.Tap(62, previewPoint);
                    Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1), "A status-area tap must dispatch one map gesture.");
                    Assert.That(view.SelectedId, Is.EqualTo(selected), "Editing status taps must not select another facility.");
                    Assert.That(view.IsEditing, Is.True);
                    Assert.That(move ? view.Simulation.Map.Commands.Edit.Origin : view.Simulation.ConstructionOrigin,
                        Is.EqualTo(expectedCell), "ON/OFF must not change the preview coordinate chosen by this screen tap.");
                    Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.EqualTo(2));
                    Assert.That(FacilityStatusCaption(view, source.Id).gameObject.activeSelf, Is.EqualTo(captionBeforePreview),
                        "A status-area tap during placement changes the draft, never the caption's expansion state.");
                    yield return ClickVisibleButton(input, "Cancel");
                    Assert.That(moving.Placement.Origin, Is.EqualTo(original.Origin));
                    Assert.That(moving.Placement.Rotation, Is.EqualTo(original.Rotation));
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceGameMenuSaveAndReloadRequireExplicitConfirmationAndCancelPreservesDraft()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                AddInputFacility(view, "storageId", new Vector2Int(10, 3));
                var session = view.Controller.Current;
                var saves = view.Controller.SuccessfulSaves;
                yield return ClickVisibleButton(input, "Save");
                Assert.That(view.Controller.SuccessfulSaves, Is.EqualTo(saves + 1));
                var savedIds = view.Simulation.Registry.Things.Select(t => t.Id).ToArray();
                var savedBytes = File.ReadAllText(view.SavePath);
                var generation = view.Controller.Generation;

                yield return ClickVisibleButton(input, "BuildStorage");
                var draft = new Vector2Int(5, 6);
                yield return input.Tap(63, input.Map.CellScreenPoint(draft));
                yield return ClickVisibleButton(input, "Reload");
                Assert.That(view.PendingSessionAction, Is.EqualTo(LaboratorySessionAction.Reload));
                Assert.That(view.Controller.Current, Is.SameAs(session));
                Assert.That(view.CanUseMap, Is.False);
                var accepted = input.Map.AcceptedTaps;
                yield return input.Tap(64, input.Map.CellScreenPoint(new Vector2Int(1, 1)));
                Assert.That(input.Map.AcceptedTaps, Is.EqualTo(accepted), "Map input is blocked behind the pending decision.");
                Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(draft));
                yield return ClickVisibleButton(input, "CancelSessionAction");
                Assert.That(view.PendingSessionAction, Is.EqualTo(LaboratorySessionAction.None));
                Assert.That(view.Controller.Current, Is.SameAs(session));
                Assert.That(view.Controller.Generation, Is.EqualTo(generation));
                Assert.That(view.IsEditing, Is.True);
                Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(draft));
                Assert.That(File.ReadAllText(view.SavePath), Is.EqualTo(savedBytes));

                yield return ClickVisibleButton(input, "Reload");
                yield return ClickVisibleButton(input, "ConfirmSessionAction");
                Assert.That(view.Controller.Generation, Is.EqualTo(generation + 1));
                Assert.That(view.Controller.Current, Is.Not.SameAs(session));
                Assert.That(view.IsEditing, Is.False, "Only committed state is restored from the slot.");
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id), Is.EquivalentTo(savedIds));
                Assert.That(view.PendingSessionAction, Is.EqualTo(LaboratorySessionAction.None));
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceBackCancelsPendingRestartAndInvalidatesHeldConfirmWithoutLosingDraft()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                yield return ClickVisibleButton(input, "BuildStorage");
                var draft = new Vector2Int(4, 3);
                yield return input.Tap(65, input.Map.CellScreenPoint(draft));
                var session = view.Controller.Current; var generation = view.Controller.Generation;
                var ids = view.Simulation.Registry.Things.Select(t => t.Id).ToArray();
                yield return ClickVisibleButton(input, "Restart");
                Assert.That(view.PendingSessionAction, Is.EqualTo(LaboratorySessionAction.Restart));
                yield return RevealVisibleButton(input, "ConfirmSessionAction");
                var heldPoint = ScreenCenter((RectTransform)view.FindUiButton("ConfirmSessionAction").transform);
                input.Touch(66, TouchPhase.Began, heldPoint); yield return null;
                yield return input.Escape();
                input.Touch(66, TouchPhase.Ended, heldPoint); yield return null; yield return null;
                Assert.That(view.PendingSessionAction, Is.EqualTo(LaboratorySessionAction.None));
                Assert.That(view.Controller.Current, Is.SameAs(session));
                Assert.That(view.Controller.Generation, Is.EqualTo(generation));
                Assert.That(view.IsEditing, Is.True);
                Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(draft));

                yield return input.Escape(); // Close the game menu; editing is still pending.
                Assert.That(view.IsEditing, Is.True);
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.False);
                yield return input.Escape(); // Now cancel the placement itself.
                Assert.That(view.IsEditing, Is.False);
                Assert.That(view.Controller.CanAdvanceLaboratory, Is.True);
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Build), "Cancel returns to the panel that started the placement.");
                yield return input.Escape(); // Close the restored construction panel.
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.None));
                yield return input.Escape(); // Base-screen Back opens the game menu, never restarts.
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Game));
                Assert.That(view.Controller.Generation, Is.EqualTo(generation));
                yield return ClickVisibleButton(input, "Restart");
                yield return ClickVisibleButton(input, "ConfirmSessionAction");
                Assert.That(view.Controller.Generation, Is.EqualTo(generation + 1));
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id).Intersect(ids), Is.Empty);
                Assert.That(view.IsEditing, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceDisplayPreferencesSurviveNewRunAndSceneReopenWithoutChangingGameData()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                yield return ClickVisibleButton(input, "Save");
                var gameBytes = File.ReadAllText(view.SavePath);
                var dataHash = GameDataStore.Instance.Catalog.Settings.CompatibilityHash;
                yield return ClickVisibleButton(input, "ClosePanel");
                yield return ClickVisibleButton(input, "ToggleStatus");
                Assert.That(view.ShowFacilityStatus, Is.False);
                yield return ClickVisibleButton(input, "TextPreference");
                Assert.That(view.LargeText, Is.True);
                var preferencesPath = UiSettingsStore.PathForGameSave(view.SavePath);
                Assert.That(File.Exists(preferencesPath), Is.True);
                Assert.That(File.ReadAllText(view.SavePath), Is.EqualTo(gameBytes), "Display settings must not rewrite the gameplay slot.");
                Assert.That(GameDataStore.Instance.Catalog.Settings.CompatibilityHash, Is.EqualTo(dataHash));
                var preferences = File.ReadAllText(preferencesPath);

                yield return ClickVisibleButton(input, "Restart");
                yield return ClickVisibleButton(input, "ConfirmSessionAction");
                Assert.That(view.ShowFacilityStatus, Is.False); Assert.That(view.LargeText, Is.True);
                Assert.That(File.ReadAllText(preferencesPath), Is.EqualTo(preferences));
                yield return input.Reopen();
                view = input.View;
                Assert.That(view.ShowFacilityStatus, Is.False); Assert.That(view.LargeText, Is.True);
                Assert.That(input.Map.VisibleStatusCount, Is.Zero);
                Assert.That(GameDataStore.Instance.Catalog.Settings.CompatibilityHash, Is.EqualTo(dataHash));
                yield return ClickVisibleButton(input, "GameMenu");
                Assert.That(view.UiCanvas.GetComponentsInChildren<Text>().Single(t => t.name == "GameTitle").fontSize,
                    Is.EqualTo(30), "The persisted 125% text setting must affect newly created labels.");
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceLargeTextIconsAndBundledKoreanRemainUsableInBothOrientations()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                Assert.That(view.UiAssets, Is.Not.Null); Assert.That(view.UiAssets.IsComplete, Is.True);
                Assert.That(view.UiAssets.FontLicense.text, Does.Contain("SIL OPEN FONT LICENSE"));
                yield return ClickVisibleButton(input, "GameMenu");
                var normalSize = view.UiCanvas.GetComponentsInChildren<Text>().Single(t => t.name == "GameTitle").fontSize;
                yield return ClickVisibleButton(input, "TextPreference");
                Assert.That(view.LargeText, Is.True);
                Assert.That(view.UiCanvas.GetComponentsInChildren<Text>().Single(t => t.name == "GameTitle").fontSize,
                    Is.EqualTo(Mathf.RoundToInt(normalSize * 1.25f)));
                const string korean = "가로세로건설생명체침공보고배양재료입고대기자동저장취소확정";
                view.UiFont.RequestCharactersInTexture(korean, 24, FontStyle.Normal);
                foreach (var letter in korean.Distinct())
                    Assert.That(view.UiFont.GetCharacterInfo(letter, out var glyph, 24, FontStyle.Normal) && glyph.glyphWidth > 0,
                        Is.True, "Bundled Korean glyph is unavailable at runtime: " + letter);

                // The third size checks a wide phone-shaped viewport; Editor input does not model Android density.
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280), new Vector2Int(2160, 1080) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(view.LargeText, Is.True);
                    foreach (var key in new[] { "ZoomOut", "ZoomIn", "FitMap", "ToggleStatus", "NavBuild", "NavCreatures", "NavInvasion", "GameMenu" })
                    {
                        var button = view.FindUiButton(key);
                        AssertScreenStateButtonReachable(button);
                        var rect = (RectTransform)button.transform;
                        Assert.That(rect.rect.width, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), key);
                        Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), key);
                        var icon = button.GetComponentsInChildren<Image>().SingleOrDefault(image => image.name == "Icon");
                        Assert.That(icon != null && icon.sprite != null, Is.True, "Icon reference missing: " + key);
                        if (key == "ZoomOut" || key == "ZoomIn" || key == "FitMap" || key == "ToggleStatus")
                        {
                            var label = button.GetComponentInChildren<Text>(true);
                            Assert.That(label, Is.Not.Null);
                            Assert.That(label.enabled, Is.False, "A bound camera icon replaces its visible caption: " + key);
                            Assert.That(label.text, Is.Not.Empty, "The fallback must retain the control's function name: " + key);
                            Assert.That(icon.rectTransform.rect.width, Is.LessThan(rect.rect.width * .75f),
                                "Compact camera artwork must leave the full-size touch target intact: " + key);
                            Assert.That(icon.rectTransform.rect.height, Is.LessThan(rect.rect.height * .75f), key);
                        }
                        else AssertMobileCaptionFits(view, button);
                    }
                    AssertCameraToolsAndGameplayBounds(view);
                    AssertScreenStateButtonReachable(view.FindUiButton("ClosePanel"));
                    AssertMobileCaptionFits(view, view.FindUiButton("ClosePanel"));
                    AssertScreenStateButtonReachable(view.FindUiButton("ToggleControls"));
                    AssertMobileCaptionFits(view, view.FindUiButton("ToggleControls"));
                    yield return ClickVisibleButton(input, "ToggleControls");
                    Assert.That(view.ControlsCollapsed, Is.True);
                    AssertMobileCaptionFits(view, view.FindUiButton("ToggleControls"));
                    yield return ClickVisibleButton(input, "ToggleControls");
                    Assert.That(view.ControlsCollapsed, Is.False);
                    yield return ClickVisibleButton(input, "BuildStorage");
                    yield return input.Tap(67, input.Map.CellScreenPoint(new Vector2Int(4, 3)));
                    foreach (var key in new[] { "Cancel", "Rotate", "Confirm" }) AssertScreenStateButtonReachable(view.FindUiButton(key));
                    AssertCameraToolsAndGameplayBounds(view);
                    yield return ClickVisibleButton(input, "Cancel");
                    var creature = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                    yield return ClickVisibleButton(input, "Select_" + creature.Id.ToString("N"));
                    if (size.y > size.x)
                    {
                        AssertScreenStateButtonReachable(view.FindUiButton("Details"));
                        AssertMobileCaptionFits(view, view.FindUiButton("Details"));
                        AssertScreenStateButtonReachable(view.FindUiButton("ClosePanel"));
                        AssertMobileCaptionFits(view, view.FindUiButton("ClosePanel"));
                    }
                }
                yield return ClickVisibleButton(input, "FontLicense");
                Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.License));
                Assert.That(VisibleUiText(view), Does.Contain("SIL OPEN FONT LICENSE"));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static IEnumerator ExpandSelectedIfNeeded(PointerScene input)
        {
            yield return null;
            var details = input.View.FindUiButton("Details");
            if (details != null && details.gameObject.activeInHierarchy) yield return ClickVisibleButton(input, "Details");
        }

        private static Text FacilityStatusCaption(LaboratoryView view, Guid id) => view.MapViewport.GetComponentsInChildren<Text>(true)
            .Single(text => text.name == "Caption" && text.transform.parent.name == "Temp_FacilityStatus_" + id.ToString("N"));

        private static void AssertMobileCaptionFits(LaboratoryView view, LaboratoryUiButton button)
        {
            Assert.That(button != null && button.gameObject.activeInHierarchy, Is.True, "Caption checks require a visible button.");
            var label = button.GetComponentInChildren<Text>();
            Assert.That(label, Is.Not.Null); Assert.That(label.font, Is.SameAs(view.UiFont));
            Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height + 1), "Large caption height clips: " + button.name);
            Assert.That(label.preferredWidth, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1), "Large caption width clips: " + button.name);
        }

        private static void AssertCameraToolsAndGameplayBounds(LaboratoryView view)
        {
            var camera = view.UiCanvas.GetComponentsInChildren<RectTransform>().Single(rect => rect.name == "CameraTools");
            var cameraBounds = MobileScreenBounds(camera); var mapBounds = MobileScreenBounds(view.MapViewport);
            Assert.That(cameraBounds.Overlaps(mapBounds), Is.False, "Camera controls must not cover the gameplay viewport.");
            var keys = new[] { "ZoomOut", "ZoomIn", "FitMap", "ToggleStatus" };
            var bounds = new Rect[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var button = view.FindUiButton(keys[i]);
                AssertScreenStateButtonReachable(button);
                var rect = (RectTransform)button.transform;
                Assert.That(rect.rect.width, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), keys[i]);
                Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), keys[i]);
                bounds[i] = MobileScreenBounds(rect);
                for (var j = 0; j < i; j++) Assert.That(bounds[i].Overlaps(bounds[j]), Is.False,
                    "Camera touch targets overlap: " + keys[j] + " / " + keys[i]);
            }
            if (Screen.width <= Screen.height) return;
            Assert.That(cameraBounds.yMin, Is.GreaterThanOrEqualTo(mapBounds.yMax - 1),
                "Landscape camera controls belong above the map, without reserving a lower camera row.");
            var safeBounds = MobileScreenBounds(view.UiSafeRoot);
            var minimumMapFraction = view.IsEditing ? .48f : .55f;
            Assert.That(mapBounds.height / safeBounds.height, Is.GreaterThanOrEqualTo(minimumMapFraction),
                "Landscape gameplay must retain a useful share of the safe-area height, including large text and open panels.");
            var hud = view.UiCanvas.GetComponentsInChildren<Text>().Single(label => label.name == "Resources");
            Assert.That(hud.preferredWidth, Is.LessThanOrEqualTo(hud.rectTransform.rect.width + 1), "Landscape HUD text clips horizontally.");
            Assert.That(hud.preferredHeight, Is.LessThanOrEqualTo(hud.rectTransform.rect.height + 1), "Landscape HUD text clips vertically.");
            Assert.That(cameraBounds.Overlaps(MobileScreenBounds(hud.rectTransform)), Is.False, "Camera controls must leave the resource HUD readable.");
            foreach (var key in new[] { "ToggleControls", "GameMenu" })
                Assert.That(cameraBounds.Overlaps(MobileScreenBounds((RectTransform)view.FindUiButton(key).transform)), Is.False,
                    "Camera controls overlap another header action: " + key);
        }

        private static Rect MobileScreenBounds(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var points = corners.Select(corner => RectTransformUtility.WorldToScreenPoint(null, corner)).ToArray();
            return Rect.MinMaxRect(points.Min(point => point.x), points.Min(point => point.y),
                points.Max(point => point.x), points.Max(point => point.y));
        }

        private static string VisibleUiText(LaboratoryView view) => string.Join("\n",
            view.UiCanvas.GetComponentsInChildren<Text>().Select(label => label.text));
    }
}
#endif
