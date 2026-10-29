#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator DenseFacilityStatusesRemainDistinctAtFitEdgesAndMaximumZoomInBothOrientations()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                using (new LaboratoryGameViewSize(1280, 720))
                {
                    yield return SettleStatusLayoutSize(new Vector2Int(1280, 720));
                    var normal = CreateStatusLayoutFacility(view, "processingId", new Vector2Int(6, 4), QuarterTurn.None, true);
                    view.SelectThing(normal.Id); view.ExpandSelection(); map.Fit();
                    Assert.That(view.SetUiPreferences(true, false), Is.True);
                    yield return SettleStatusLayout();
                    yield return CaptureStatusLayoutImage("normal-on");
                    Assert.That(view.SetUiPreferences(false, false), Is.True);
                    yield return SettleStatusLayout();
                    yield return CaptureStatusLayoutImage("normal-off");
                    view.Simulation.Registry.Remove(normal);
                    yield return SettleStatusLayout();
                }
                var facilities = CreateStatusLayoutFacilities(view);
                var selected = facilities[0];
                var rotatedSite = facilities[18];
                var completeStorage = facilities[19];
                Assert.That(view.SelectThing(selected.Id), Is.True); view.ExpandSelection();
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleStatusLayoutSize(size);
                    foreach (var large in new[] { false, true })
                    {
                        Assert.That(view.SetUiPreferences(true, large), Is.True);
                        map.Fit(); yield return SettleStatusLayout();
                        Assert.That(map.Zoom, Is.EqualTo(1));
                        Assert.That(map.VisibleStatusCount, Is.EqualTo(19), "All fitted facility statuses must remain identifiable; completed storage is excluded.");
                        Assert.That(map.TryStatusScreenPoint(completeStorage.Id, out _), Is.False);
                        AssertStatusMarkerGeometry(view, map, facilities);
                        AssertSelectedStatusCaption(view, selected.Id, large);
                        if (!large) yield return CaptureStatusLayoutImage("dense-on");

                        Assert.That(view.SetUiPreferences(false, large), Is.True);
                        yield return SettleStatusLayout();
                        Assert.That(map.VisibleStatusCount, Is.Zero);
                        Assert.That(StatusMarkerRoots(view, false), Is.Empty);
                        foreach (var facility in facilities) Assert.That(map.TryStatusScreenPoint(facility.Id, out _), Is.False);
                        Assert.That(view.SelectedId, Is.EqualTo(selected.Id));
                        Assert.That(view.DetailsExpanded, Is.True);
                        Assert.That(view.FindUiButton("Move").gameObject.activeInHierarchy, Is.True, "OFF only hides overhead status, not selected controls.");
                        if (!large) yield return CaptureStatusLayoutImage("dense-off");

                        Assert.That(view.SetUiPreferences(true, large), Is.True);
                        map.ZoomBy(100); yield return SettleStatusLayout();
                        Assert.That(map.Zoom, Is.EqualTo(8));
                        foreach (var edge in new[] { selected, rotatedSite })
                        {
                            Assert.That(view.SelectThing(edge.Id), Is.True); view.ExpandSelection();
                            yield return SettleStatusLayout();
                            CenterStatusLayoutFixture(map, edge);
                            yield return SettleStatusLayout();
                            Assert.That(map.TryStatusScreenPoint(edge.Id, out _), Is.True, "Selected edge facility must retain its own status after test camera positioning.");
                            Assert.That(map.VisibleStatusCount, Is.GreaterThan(0).And.LessThan(19), "Offscreen facilities must not leave floating statuses.");
                            AssertStatusMarkerGeometry(view, map, facilities);
                            AssertSelectedStatusCaption(view, edge.Id, large);
                        }
                        Assert.That(view.SelectThing(selected.Id), Is.True); view.ExpandSelection();
                        map.Fit(); yield return SettleStatusLayout();
                        Assert.That(map.VisibleStatusCount, Is.EqualTo(19));
                        LogStatusEvidence("PLAN014_STATUS_LAYOUT " + size.x + "x" + size.y + " large=" + large
                            + " targets=19 fit=19 maxZoom=8 off=0 selected=" + view.SelectedId);
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator RepeatedStatusPanelOrientationAndSessionChangesDoNotAccumulateUiObjects()
        {
            using (var input = new PointerScene())
            using (new LaboratoryGameViewSize(1280, 720))
            {
                yield return input.Load();
                yield return SettleStatusLayoutSize(new Vector2Int(1280, 720));
                var view = input.View; var map = input.Map;
                var facilities = CreateStatusLayoutFacilities(view);
                var oldIds = facilities.Select(t => t.Id.ToString("N")).ToArray();
                Assert.That(view.SetUiPreferences(true, false), Is.True);
                Assert.That(view.SelectThing(facilities[0].Id), Is.True); view.ExpandSelection();
                map.Fit(); yield return SettleStatusLayout();
                var baselineButtons = StatusLayoutButtonCount(view);
                var baselineMarkers = StatusMarkerRoots(view, true).Length;
                Assert.That(baselineMarkers, Is.EqualTo(19));
                for (var cycle = 0; cycle < 10; cycle++)
                {
                    foreach (var panel in new[] { LaboratoryPanel.Build, LaboratoryPanel.Creatures, LaboratoryPanel.Invasion, LaboratoryPanel.Game })
                    { view.OpenPanel(panel); yield return SettleStatusLayout(); }
                    Assert.That(view.SetUiPreferences(false, cycle % 2 == 0), Is.True);
                    yield return SettleStatusLayout();
                    Assert.That(map.VisibleStatusCount, Is.Zero);
                    using (new LaboratoryGameViewSize(720, 1280))
                    {
                        yield return SettleStatusLayoutSize(new Vector2Int(720, 1280));
                        Assert.That(view.SetUiPreferences(true, true), Is.True);
                        view.SelectThing(facilities[0].Id); view.ExpandSelection(); map.Fit();
                        yield return SettleStatusLayout();
                        Assert.That(StatusMarkerRoots(view, true).Length, Is.LessThanOrEqualTo(19));
                    }
                    yield return SettleStatusLayoutSize(new Vector2Int(1280, 720));
                    Assert.That(view.SetUiPreferences(true, false), Is.True);
                    view.SelectThing(facilities[0].Id); view.ExpandSelection(); map.Fit();
                    yield return SettleStatusLayout();
                    Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(baselineMarkers), "Marker objects accumulated after cycle " + cycle);
                    Assert.That(StatusLayoutButtonCount(view), Is.EqualTo(baselineButtons), "Unused button rows accumulated after cycle " + cycle);
                    AssertUniqueStatusAndButtonObjects(view);
                }
                for (var run = 0; run < 3; run++)
                {
                    Assert.That(view.Restart(), Is.True, view.Controller.LastError);
                    yield return SettleStatusLayout();
                    Assert.That(StatusMarkerRoots(view, true), Is.Empty, "A new initial session has no facilities and must discard all old markers.");
                    Assert.That(view.ShowFacilityStatus, Is.True, "Run reset must preserve the separate user preference.");
                    Assert.That(view.GetComponentsInChildren<LaboratoryUiButton>(true).Any(b => oldIds.Any(id => b.name.Contains(id))), Is.False);
                    facilities = CreateStatusLayoutFacilities(view);
                    oldIds = facilities.Select(t => t.Id.ToString("N")).ToArray();
                    view.SelectThing(facilities[0].Id); view.ExpandSelection(); map.Fit();
                    yield return SettleStatusLayout();
                    Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(baselineMarkers));
                    Assert.That(StatusLayoutButtonCount(view), Is.EqualTo(baselineButtons));
                    AssertUniqueStatusAndButtonObjects(view);
                }
                LogStatusEvidence("PLAN014_STATUS_OBJECTS cycles=10 sessions=3 markers=" + baselineMarkers + " buttons=" + baselineButtons + " accumulated=0");
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator TwentyFacilityStatusRefreshReportsEditorCpuReferenceWithoutTimingPassClaim()
        {
            using (var input = new PointerScene())
            using (new LaboratoryGameViewSize(1280, 720))
            {
                yield return input.Load();
                yield return SettleStatusLayoutSize(new Vector2Int(1280, 720));
                var view = input.View; var map = input.Map;
                CreateStatusLayoutFacilities(view);
                map.Fit(); yield return SettleStatusLayout();
                var method = typeof(LaboratoryMapView).GetMethod("RefreshStatusOverlays", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                var refresh = (Action)Delegate.CreateDelegate(typeof(Action), map, method);
                var off = new double[120]; var on = new double[120];
                var evidence = new StatusCpuReference { facilities = 20, markerTargets = 19, warmupFrames = 60, sampleFrames = 120,
                    width = Screen.width, height = Screen.height, referenceBudgetMs = 2,
                    scope = "Editor synchronous RefreshStatusOverlays only; not total frame CPU, GPU, allocations or device performance" };
                Assert.That(view.SetUiPreferences(false, false), Is.True);
                yield return SampleStatusRefresh(refresh, off);
                evidence.offMedianMs = MedianStatusSamples(off);
                evidence.managedHeapBeforeOn = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
                Assert.That(view.SetUiPreferences(true, false), Is.True);
                yield return SampleStatusRefresh(refresh, on);
                evidence.onMedianMs = MedianStatusSamples(on);
                evidence.managedHeapAfterOn = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
                evidence.medianIncreaseMs = evidence.onMedianMs - evidence.offMedianMs;
                evidence.withinReferenceBudget = evidence.medianIncreaseMs <= evidence.referenceBudgetMs;
                evidence.liveMarkerObjects = StatusMarkerRoots(view, true).Length;
                Assert.That(evidence.liveMarkerObjects, Is.LessThanOrEqualTo(evidence.markerTargets));
                // Timing is evidence for the owner to judge; noisy Editor measurements do not fail functional tests.
                LogStatusEvidence("PLAN014_STATUS_CPU_REFERENCE " + JsonUtility.ToJson(evidence));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static List<Thing> CreateStatusLayoutFacilities(LaboratoryView view)
        {
            Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
            var result = new List<Thing>();
            // Dense legal placements include all four map borders. This is prepared state, not a paid building scenario.
            foreach (var y in new[] { 0, 3, 6, 8 })
            foreach (var x in new[] { 0, 3, 6, 9, 12 })
                if (result.Count < 18) result.Add(CreateStatusLayoutFacility(view, "processingId", new Vector2Int(x, y), QuarterTurn.None, true));
            result.Add(CreateStatusLayoutFacility(view, "storageId", new Vector2Int(14, 0), QuarterTurn.Clockwise90, false));
            result.Add(CreateStatusLayoutFacility(view, "storageId", new Vector2Int(14, 4), QuarterTurn.Clockwise90, true));
            return result;
        }

        private static Thing CreateStatusLayoutFacility(LaboratoryView view, string definitionKey, Vector2Int cell, QuarterTurn rotation, bool complete)
        {
            var definition = GameDataStore.Instance.Catalog.Selected<FacilityData>(definitionKey);
            var thing = view.Simulation.Registry.Create(definition);
            Assert.That(view.Simulation.Map.Commands.Place(thing.Id, cell, rotation).Succeeded, Is.True, "Fixture footprint is invalid: " + cell);
            if (complete)
            {
                var construction = thing.GetComponent<ConstructionComponent>();
                typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Delivered)).SetValue(construction, definition.BuildCost);
                typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Progress)).SetValue(construction, (double)definition.BuildSeconds);
                typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.IsComplete)).SetValue(construction, true);
            }
            return thing;
        }

        private static IEnumerator SettleStatusLayoutSize(Vector2Int size)
        {
            yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y, "Status layout Game View did not adopt " + size);
            yield return SettleStatusLayout();
        }

        private static IEnumerator SettleStatusLayout()
        { yield return null; yield return null; yield return null; Canvas.ForceUpdateCanvases(); }

        private static RectTransform[] StatusMarkerRoots(LaboratoryView view, bool includeInactive) => view.MapViewport
            .GetComponentsInChildren<RectTransform>(includeInactive).Where(t => t.name.StartsWith("Temp_FacilityStatus_", StringComparison.Ordinal)).ToArray();

        private static Rect StatusScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var points = corners.Select(c => RectTransformUtility.WorldToScreenPoint(null, c)).ToArray();
            return Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
        }

        private static void AssertStatusMarkerGeometry(LaboratoryView view, LaboratoryMapView map, List<Thing> facilities)
        {
            var roots = StatusMarkerRoots(view, false);
            Assert.That(roots.Length, Is.EqualTo(map.VisibleStatusCount));
            var viewport = StatusScreenRect(view.MapViewport);
            var bounds = roots.Select(StatusScreenRect).ToArray();
            for (var i = 0; i < roots.Length; i++)
            {
                var id = Guid.ParseExact(roots[i].name.Substring("Temp_FacilityStatus_".Length), "N");
                Assert.That(facilities.Any(t => t.Id == id), Is.True, "Marker identity belongs to a different session.");
                Assert.That(map.TryStatusScreenPoint(id, out var point), Is.True);
                Assert.That(bounds[i].Contains(point), Is.True);
                Assert.That(bounds[i].xMin, Is.GreaterThanOrEqualTo(viewport.xMin - 1));
                Assert.That(bounds[i].xMax, Is.LessThanOrEqualTo(viewport.xMax + 1));
                Assert.That(bounds[i].yMin, Is.GreaterThanOrEqualTo(viewport.yMin - 1));
                Assert.That(bounds[i].yMax, Is.LessThanOrEqualTo(viewport.yMax + 1));
                Assert.That(roots[i].GetComponentsInChildren<Graphic>().All(g => !g.raycastTarget), Is.True, "Status visuals must use the map's gesture route.");
                for (var j = 0; j < i; j++) Assert.That(bounds[i].Overlaps(bounds[j]), Is.False, "Two facility markers obscure each other.");
            }
            Assert.That(roots.Select(r => r.name).Distinct().Count(), Is.EqualTo(roots.Length));
        }

        private static void AssertSelectedStatusCaption(LaboratoryView view, Guid id, bool large)
        {
            var marker = StatusMarkerRoots(view, false).Single(t => t.name == "Temp_FacilityStatus_" + id.ToString("N"));
            var caption = marker.GetComponentsInChildren<Text>().Single(t => t.name == "Caption");
            Assert.That(caption.text, Does.Contain("진행"));
            Assert.That(caption.font, Is.SameAs(view.UiFont));
            Assert.That(caption.fontSize, Is.GreaterThanOrEqualTo(large ? 15 : 12));
            Assert.That(caption.preferredHeight, Is.LessThanOrEqualTo(caption.rectTransform.rect.height + 1), "Selected Korean status caption is vertically clipped.");
            Assert.That(marker.GetComponentsInChildren<RectTransform>().Any(t => t.name == "Track"), Is.True);
        }

        private static void CenterStatusLayoutFixture(LaboratoryMapView map, Thing facility)
        {
            // Camera positioning is test setup; device pan/pinch semantics are tested in the input fixture.
            var size = FacilityGeometry.GetSize((FacilityData)facility.Definition, facility.Placement.Rotation);
            var center = (Vector2)facility.Placement.Origin + ((Vector2)size - Vector2.one) * .5f;
            var viewportCenter = RectTransformUtility.WorldToScreenPoint(null, map.Viewport.TransformPoint(map.Viewport.rect.center));
            typeof(LaboratoryMapView).GetMethod("Pan", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(map, new object[] { viewportCenter - map.CellScreenPoint(center) });
        }

        private static int StatusLayoutButtonCount(LaboratoryView view) => view.GetComponentsInChildren<LaboratoryUiButton>(true).Length;
        private static void AssertUniqueStatusAndButtonObjects(LaboratoryView view)
        {
            var names = StatusMarkerRoots(view, true).Select(t => t.name).ToArray();
            Assert.That(names.Distinct().Count(), Is.EqualTo(names.Length));
            foreach (var siblings in view.GetComponentsInChildren<LaboratoryUiButton>(true).GroupBy(b => b.transform.parent))
                Assert.That(siblings.Select(b => b.name).Distinct().Count(), Is.EqualTo(siblings.Count()), "A semantic row was duplicated under one content parent.");
        }

        private static IEnumerator SampleStatusRefresh(Action refresh, double[] samples)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 60;
            for (var frame = 0; frame < 60; frame++) { refresh(); yield return null; }
            var watch = new Stopwatch();
            for (var frame = 0; frame < samples.Length; frame++)
            {
                Assert.That(Time.realtimeSinceStartupAsDouble, Is.LessThan(deadline), "Status sampling exceeded its bounded run; stop and inspect the Editor.");
                watch.Restart(); refresh(); watch.Stop();
                samples[frame] = watch.Elapsed.TotalMilliseconds;
                yield return null;
            }
        }

        private static double MedianStatusSamples(double[] samples)
        { var sorted = samples.OrderBy(v => v).ToArray(); return (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) * .5; }

        private static void LogStatusEvidence(string message)
        { LogAssert.Expect(LogType.Log, message); Debug.Log(message); }

        private static IEnumerator CaptureStatusLayoutImage(string state)
        {
            yield return new WaitForEndOfFrame();
            var filename = "plan014-status-" + state + "-" + Screen.width + "x" + Screen.height + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".png";
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs", filename));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
            yield return WaitFor(() => File.Exists(path) && new FileInfo(path).Length > 0, "Status screenshot was not written: " + filename);
            LogStatusEvidence("PLAN014_STATUS_SCREENSHOT " + path);
        }

        [Serializable]
        private sealed class StatusCpuReference
        {
            public string scope;
            public int facilities, markerTargets, warmupFrames, sampleFrames, width, height, liveMarkerObjects;
            public double offMedianMs, onMedianMs, medianIncreaseMs, referenceBudgetMs;
            public bool withinReferenceBudget;
            public long managedHeapBeforeOn, managedHeapAfterOn;
        }
    }
}
#endif
