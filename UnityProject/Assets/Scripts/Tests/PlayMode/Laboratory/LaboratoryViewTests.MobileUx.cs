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
using TMPro;
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

        [UnityTest, Category("FacilityProgress")]
        public IEnumerator DeviceProgressBarPassesTapsToGroundAndBothPlacementModes()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var source = AddInputFacility(view, "gatheringId", new Vector2Int(3, 5));
                var moving = AddInputFacility(view, "storageId", new Vector2Int(10, 3));
                yield return SettleStatusLayout();
                yield return input.Tap(61, map.CellScreenPoint(source.Placement.Origin));
                Assert.That(view.SelectedId, Is.EqualTo(source.Id));
                yield return ExpandSelectedIfNeeded(input);
                Assert.That(view.FindUiButton("Move").gameObject.activeInHierarchy, Is.True);
                Assert.That(map.TryStatusScreenPoint(source.Id, out var point), Is.True);
                Assert.That(map.TryScreenCell(point, out var ground), Is.True);
                Assert.That(ground.y, Is.GreaterThanOrEqualTo(source.Placement.Origin.y + FacilityGeometry.GetSize((FacilityData)source.Definition, source.Placement.Rotation).y));
                var accepted = map.AcceptedTaps;
                yield return input.Tap(62, point);
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty), "A bar above empty ground must not select its facility.");
                Assert.That(map.TryStatusScreenPoint(source.Id, out _), Is.True);
                map.ZoomBy(2); yield return SettleStatusLayout();
                Assert.That(map.TryStatusScreenPoint(source.Id, out point), Is.True);
                yield return input.Tap(64, point);
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty), "Repeated bar taps do not open a caption or select the source.");
                var panBefore = map.CellScreenPoint(Vector2.zero); accepted = map.AcceptedTaps;
                input.Touch(65, TouchPhase.Began, point); yield return null;
                input.Touch(65, TouchPhase.Moved, point + Vector2.right * 40); yield return null;
                input.Touch(65, TouchPhase.Ended, point + Vector2.right * 40); yield return SettleStatusLayout();
                Assert.That(map.CellScreenPoint(Vector2.zero), Is.Not.EqualTo(panBefore), "A drag starting on the bar pans the map.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                Assert.That(StatusMarkerRoots(view, false).Single(r => r.name.EndsWith(source.Id.ToString("N"))).rect.size, Is.EqualTo(new Vector2(36, 4)));
                map.Fit(); yield return SettleStatusLayout();
                foreach (var move in new[] { false, true })
                foreach (var legacyShow in new[] { false, true })
                {
                    Assert.That(view.SetUiPreferences(legacyShow, false), Is.True);
                    if (move)
                    {
                        yield return ClickVisibleButton(input, "Select_" + moving.Id.ToString("N"));
                        yield return ClickVisibleButton(input, "Move");
                    }
                    else yield return ClickVisibleButton(input, "BuildStorage");
                    yield return SettleStatusLayout();
                    Assert.That(map.TryStatusScreenPoint(source.Id, out point), Is.True);
                    Assert.That(map.TryScreenCell(point, out var expectedCell), Is.True);
                    var selected = view.SelectedId; accepted = map.AcceptedTaps;
                    var original = moving.Placement;
                    yield return input.Tap(63, point);
                    Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                    Assert.That(view.SelectedId, Is.EqualTo(selected));
                    Assert.That(view.IsEditing, Is.True);
                    Assert.That(move ? view.Simulation.Map.Commands.Edit.Origin : view.Simulation.ConstructionOrigin, Is.EqualTo(expectedCell));
                    Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.EqualTo(2));
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
                var heldPoint = ScreenCenter((RectTransform)FindActionButton(view, "ConfirmSessionAction").transform);
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
                Assert.That(view.SetUiPreferences(false, false), Is.True);
                Assert.That(view.FindUiButton("ToggleStatus"), Is.Null);
                Assert.That(view.ShowFacilityStatus, Is.False);
                yield return ClickVisibleButton(input, "TextPreference");
                Assert.That(view.LargeText, Is.True);
                Assert.That(view.GetComponentsInChildren<SettingsPopup>().Single().GetComponentsInChildren<UiButton>().Any(b => b.name == "Status"), Is.False);
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
                AddInputFacility(view, "gatheringId", new Vector2Int(3, 3));
                yield return SettleStatusLayout();
                Assert.That(input.Map.VisibleStatusCount, Is.EqualTo(1), "Legacy OFF survives storage but must not hide new bars.");
                Assert.That(GameDataStore.Instance.Catalog.Settings.CompatibilityHash, Is.EqualTo(dataHash));
                yield return ClickVisibleButton(input, "GameMenu");
                Assert.That(view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "GameTitle").fontSize,
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
                AppSceneFlowTests.AssertSharedProductFont(view, view.UiFont);
                Assert.That(view.UiAssets.FontLicense.text, Does.Contain("SIL OPEN FONT LICENSE"));
                yield return ClickVisibleButton(input, "GameMenu");
                var normalSize = view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "GameTitle").fontSize;
                yield return ClickVisibleButton(input, "TextPreference");
                Assert.That(view.LargeText, Is.True);
                Assert.That(view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "GameTitle").fontSize,
                    Is.EqualTo(Mathf.RoundToInt(normalSize * 1.25f)));
                yield return input.Escape();
                const string korean = "가로세로건설생명체침공보고배양재료입고대기자동저장취소확정";
                Assert.That(view.UiFont.atlasPopulationMode, Is.EqualTo(TMPro.AtlasPopulationMode.Dynamic));
                Assert.That(view.UiFont.sourceFontFile.name, Is.EqualTo("Mulmaru"));
                foreach (var letter in korean.Distinct())
                    Assert.That(view.UiFont.HasCharacter(letter, searchFallbacks: false, tryAddCharacter: true)
                        && view.UiFont.characterLookupTable[letter].glyph.metrics.width > 0,
                        Is.True, "Bundled Korean glyph is unavailable at runtime: " + letter);

                // The third size checks a wide phone-shaped viewport; Editor input does not model Android density.
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280), new Vector2Int(2160, 1080) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(view.LargeText, Is.True);
                    foreach (var key in new[] { "ZoomOut", "ZoomIn", "FitMap", "NavBuild", "NavCreatures", "NavInvasion", "GameMenu" })
                    {
                        var button = view.FindUiButton(key);
                        AssertScreenStateButtonReachable(button);
                        var rect = (RectTransform)button.transform;
                        Assert.That(rect.rect.width, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), key);
                        Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(view.MinimumTouchSize - .1f), key);
                        var icon = button.GetComponentsInChildren<Image>().SingleOrDefault(image => image.name == "Icon");
                        Assert.That(icon != null && icon.sprite != null, Is.True, "Icon reference missing: " + key);
                        if (key == "ZoomOut" || key == "ZoomIn" || key == "FitMap")
                        {
                            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
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

        private static void AssertMobileCaptionFits(LaboratoryView view, UiButton button)
        {
            Assert.That(button != null && button.gameObject.activeInHierarchy, Is.True, "Caption checks require a visible button.");
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            Assert.That(label, Is.Not.Null); Assert.That(label.font, Is.SameAs(view.UiFont));
            Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height + 1), "Large caption height clips: " + button.name);
            Assert.That(label.preferredWidth, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1), "Large caption width clips: " + button.name);
        }

        private static void AssertCameraToolsAndGameplayBounds(LaboratoryView view)
        {
            var camera = view.UiCanvas.GetComponentsInChildren<RectTransform>().Single(rect => rect.name == "CameraTools");
            var cameraBounds = MobileScreenBounds(camera); var mapBounds = MobileScreenBounds(view.MapViewport);
            Assert.That(cameraBounds.Overlaps(mapBounds), Is.False, "Camera controls must not cover the gameplay viewport.");
            var keys = new[] { "ZoomOut", "ZoomIn", "FitMap" };
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
            var hud = view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(label => label.name == "Resources");
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
            view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Select(label => label.text));
    }
}
#endif
