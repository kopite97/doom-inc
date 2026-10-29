#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
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
        public IEnumerator NewReportsPreserveReadingPositionAndPrunedAnchorShowsNearestRetainedReport()
        {
            using (var input = new PointerScene())
            using (new LaboratoryGameViewSize(720, 1280))
            {
                yield return input.Load();
                yield return SettleStatusLayoutSize(new Vector2Int(720, 1280));
                var view = input.View;
                var sourceSettings = GameDataStore.Instance.Catalog.Settings.Invasion;
                var originalTarget = sourceSettings.TargetScore;
                var settings = view.Invasion.Settings;
                Assert.That(settings, Is.Not.SameAs(sourceSettings));
                // Rare UI fixture: prolong only this isolated run, so pruning can be observed before the ending.
                // Reports themselves still come from the normal release, activity and retention code.
                typeof(InvasionSettings).GetField("targetScore", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(settings, 10000L);
                typeof(InvasionSettings).GetField("detailedUntilScore", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(settings, 10000L);
                settings.Validate();
                var invader = view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("invaderId"));
                Assert.That(view.Simulation.Map.Commands.Place(invader.Id, Vector2Int.one).Succeeded, Is.True);
                Assert.That(view.ReleaseInvaders(), Is.True);
                view.Controller.Advance(settings.ActivitySeconds * settings.ReportLimit);
                Assert.That(view.Invasion.Reports.Count, Is.EqualTo(settings.ReportLimit));
                Assert.That(view.Controller.LastSaveError, Is.Null.Or.Empty);

                yield return ClickVisibleButton(input, "Reports");
                Assert.That(view.ReportsExpanded, Is.True);
                yield return SettleStatusLayout();
                var scroll = view.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "Controls");
                var anchorSequence = view.Invasion.Reports[view.Invasion.Reports.Count / 2].Sequence;
                var anchor = ReportReadingRow(view, anchorSequence);
                var desired = -anchor.rectTransform.anchoredPosition.y + 6;
                Assert.That(desired, Is.LessThan(scroll.content.rect.height - scroll.viewport.rect.height), "Report fixture needs an interior reading anchor.");
                scroll.StopMovement();
                scroll.content.anchoredPosition = new Vector2(0, desired);
                yield return SettleStatusLayout();
                var beforeRect = StatusScreenRect(anchor.rectTransform);
                Assert.That(beforeRect.Overlaps(StatusScreenRect(scroll.viewport)), Is.True);
                var scrollBefore = scroll.content.anchoredPosition.y;
                var scoreBefore = view.Invasion.Score;
                yield return SettleStatusLayout();
                Assert.That(view.Invasion.Score, Is.EqualTo(scoreBefore), "Reading must not accumulate score.");

                view.Controller.Advance(settings.ActivitySeconds * 2);
                yield return SettleStatusLayout();
                var kept = ReportReadingRow(view, anchorSequence);
                Assert.That(kept, Is.SameAs(anchor), "The same semantic report row should survive insertion.");
                Assert.That(StatusScreenRect(kept.rectTransform).yMax, Is.EqualTo(beforeRect.yMax).Within(1.5f), "Newer reports moved the report being read.");
                Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(scrollBefore), "The content offset should absorb the inserted rows.");
                Assert.That(view.Invasion.Reports.Any(r => r.Sequence == anchorSequence), Is.True);

                var toPrune = checked((int)(anchorSequence - view.Invasion.Reports.First().Sequence + 1));
                Assert.That(toPrune, Is.GreaterThan(0));
                view.Controller.Advance(settings.ActivitySeconds * toPrune);
                yield return SettleStatusLayout();
                Assert.That(view.Invasion.Reports.Any(r => r.Sequence == anchorSequence), Is.False);
                var nearest = view.Invasion.Reports.First().Sequence;
                Assert.That(nearest, Is.EqualTo(anchorSequence + 1));
                var nearestRow = ReportReadingRow(view, nearest);
                Assert.That(StatusScreenRect(nearestRow.rectTransform).Overlaps(StatusScreenRect(scroll.viewport)), Is.True, "The nearest remaining report must be visible after pruning.");
                var feedback = view.GetComponentsInChildren<Text>().Single(t => t.name == "Feedback");
                Assert.That(feedback.text, Does.Contain("남은 기록"));
                Assert.That(view.GetComponentsInChildren<Text>().Any(t => t.name == "Report_" + anchorSequence), Is.False);
                var scoreAfterActivities = view.Invasion.Score;
                yield return ClickVisibleButton(input, "LatestReports");
                yield return SettleStatusLayout();
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(0).Within(1));
                Assert.That(view.Invasion.Score, Is.EqualTo(scoreAfterActivities));
                Assert.That(sourceSettings.TargetScore, Is.EqualTo(originalTarget), "The fixture must never alter shared definitions.");
                view.Simulation.ValidateState();
                LogStatusEvidence("PLAN014_REPORT_READING anchor=" + anchorSequence + " retainedAfterInsert=true nearestAfterPrune=" + nearest + " latestY=0");
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator SelectingOffscreenFacilityRevealsItOnceAndDoesNotUndoSubsequentDevicePan()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var facility = AddInputFacility(view, "processingId", new Vector2Int(13, 7));
                var middleCell = (Vector2)facility.Placement.Origin + Vector2.one * .5f;
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleStatusLayoutSize(size);
                    yield return ClickVisibleButton(input, "NavBuild");
                    map.Fit(); map.ZoomBy(100);
                    yield return SettleStatusLayout();
                    Assert.That(RectTransformUtility.RectangleContainsScreenPoint(view.MapViewport, map.CellScreenPoint(middleCell)), Is.False, "Fixture must start with the selected destination outside the viewport.");

                    yield return ClickVisibleButton(input, "Select_" + facility.Id.ToString("N"));
                    yield return ExpandSelectedIfNeeded(input);
                    yield return SettleStatusLayout();
                    Assert.That(view.SelectedId, Is.EqualTo(facility.Id));
                    Assert.That(RectTransformUtility.RectangleContainsScreenPoint(view.MapViewport, map.CellScreenPoint(middleCell)), Is.True, "Choosing an offscreen roster entry should reveal it.");
                    var accepted = map.AcceptedTaps;
                    for (var drag = 0; drag < 3; drag++)
                    {
                        var middle = ScreenCenter(view.MapViewport);
                        var radius = view.MapViewport.rect.width * view.UiCanvas.scaleFactor * .28f;
                        var start = middle - Vector2.right * radius;
                        var end = middle + Vector2.right * radius;
                        input.Touch(180 + drag, TouchPhase.Began, start); yield return null;
                        input.Touch(180 + drag, TouchPhase.Moved, end); yield return null;
                        input.Touch(180 + drag, TouchPhase.Ended, end); yield return null; yield return null;
                    }
                    Assert.That(map.AcceptedTaps, Is.EqualTo(accepted), "Manual pans must not become new selections.");
                    Assert.That(view.SelectedId, Is.EqualTo(facility.Id));
                    var panned = map.CellScreenPoint(middleCell);
                    Assert.That(RectTransformUtility.RectangleContainsScreenPoint(view.MapViewport, panned), Is.False, "Manual pans must be free to leave the selected facility behind.");
                    for (var frame = 0; frame < 12; frame++)
                    {
                        yield return null;
                        Assert.That(Vector2.Distance(map.CellScreenPoint(middleCell), panned), Is.LessThan(.1f), "Selection visibility kept pulling the camera back after manual pan.");
                    }
                    LogStatusEvidence("PLAN014_SELECTION_VISIBILITY " + size.x + "x" + size.y + " revealOnce=true manualPanHeldFrames=12");
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceRelatedTargetLinksSelectRevealAndExpandCurrentTargetsInBothOrientations()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                // Completed facilities isolate link navigation; construction and paid production have separate coverage.
                var processor = AddInputFacility(view, "processingId", new Vector2Int(13, 7));
                var storage = AddInputFacility(view, "storageId", new Vector2Int(3, 3));
                var worker = view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
                Assert.That(view.Simulation.Map.Commands.Place(worker.Id, Vector2Int.one).Succeeded, Is.True);
                Assert.That(view.StatusFor(processor).Kind, Is.EqualTo(FacilityStatusKind.MaterialWaiting));

                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleStatusLayoutSize(size);
                    map.Fit(); map.ZoomBy(100);
                    yield return ClickVisibleButton(input, "Select_" + processor.Id.ToString("N"));
                    yield return ExpandSelectedIfNeeded(input);
                    var mapTaps = map.AcceptedTaps;
                    yield return ClickVisibleButton(input, "InspectStorage_" + storage.Id.ToString("N"));
                    yield return SettleStatusLayout();
                    AssertRelatedTargetSelection(input, storage, (Vector2)storage.Placement.Origin + new Vector2(1, .5f), mapTaps);
                    var allowsCulture = storage.GetComponent<StorageComponent>().AllowsCulture;
                    yield return ClickVisibleButton(input, "AllowCulture");
                    Assert.That(storage.GetComponent<StorageComponent>().AllowsCulture, Is.EqualTo(!allowsCulture), "The related destination must expose its own controls.");
                    yield return ClickVisibleButton(input, "AllowCulture");
                    Assert.That(storage.GetComponent<StorageComponent>().AllowsCulture, Is.EqualTo(allowsCulture));

                    yield return ClickVisibleButton(input, "Select_" + worker.Id.ToString("N"));
                    yield return ClickVisibleButton(input, "Assign_" + processor.Id.ToString("N"));
                    Assert.That(worker.GetComponent<WorkerComponent>().AssignedFacilityId, Is.EqualTo(processor.Id));
                    mapTaps = map.AcceptedTaps;
                    yield return ClickVisibleButton(input, "AssignedFacility_" + processor.Id.ToString("N"));
                    yield return SettleStatusLayout();
                    AssertRelatedTargetSelection(input, processor, (Vector2)processor.Placement.Origin + Vector2.one * .5f, mapTaps);
                    yield return ClickVisibleButton(input, "AssignedWorker_" + worker.Id.ToString("N") + "_" + worker.Id.ToString("N"));
                    yield return SettleStatusLayout();
                    AssertRelatedTargetSelection(input, worker, worker.Placement.Origin, mapTaps);
                    Assert.That(worker.GetComponent<WorkerComponent>().AssignedFacilityId, Is.EqualTo(processor.Id), "Navigating the relationship must not change the assignment.");
                    LogStatusEvidence("PLAN014_RELATED_TARGET " + size.x + "x" + size.y + " materialWaitToStorage=true destinationControls=true assignedFacilityAndWorker=true exactId=true expanded=true revealed=true mapTapThrough=false");
                }
                view.Simulation.ValidateState();
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static void AssertRelatedTargetSelection(PointerScene input, Thing target, Vector2 cell, int mapTaps)
        {
            Assert.That(input.View.SelectedId, Is.EqualTo(target.Id));
            Assert.That(input.View.OpenedPanel, Is.EqualTo(LaboratoryPanel.Selection));
            Assert.That(input.View.DetailsExpanded, Is.True, "Related targets must open their own detailed controls in either orientation.");
            Assert.That(RectTransformUtility.RectangleContainsScreenPoint(input.View.MapViewport, input.Map.CellScreenPoint(cell)), Is.True, "Following a related target must reveal its map position.");
            Assert.That(input.Map.AcceptedTaps, Is.EqualTo(mapTaps), "The related-target button must not also tap the map.");
        }

        private static Text ReportReadingRow(LaboratoryView view, long sequence)
            => view.GetComponentsInChildren<Text>().Single(t => t.name == "Report_" + sequence);
    }
}
#endif
