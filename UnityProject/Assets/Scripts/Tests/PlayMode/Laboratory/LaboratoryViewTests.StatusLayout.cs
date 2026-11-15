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
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("FacilityProgress")]
        public IEnumerator FacilityProgressBarsKeepSmallPassiveGeometryAcrossDisplayChanges()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var facilities = CreateStatusLayoutFacilities(view);
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280), new Vector2Int(640, 360) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleStatusLayoutSize(size);
                    foreach (var large in new[] { false, true })
                    {
                        Assert.That(view.SetUiPreferences(false, large), Is.True);
                        view.OpenPanel(LaboratoryPanel.None); map.Fit(); yield return SettleStatusLayout();
                        Assert.That(map.VisibleStatusCount, Is.EqualTo(19), "Legacy OFF must not hide progress; completed storage has no cycle.");
                        Assert.That(map.TryStatusScreenPoint(facilities[19].Id, out _), Is.False);
                        AssertStatusMarkerGeometry(view, map, facilities);
                        Assert.That(view.FindUiButton("ToggleStatus"), Is.Null);
                        if (!large) yield return CaptureStatusLayoutImage("fit");
                        view.SelectThing(facilities[6].Id); view.ExpandSelection();
                        foreach (var zoom in new[] { 1.5f, 8f })
                        {
                            map.Fit(); map.ZoomBy(zoom); yield return SettleStatusLayout();
                            CenterStatusLayoutFixture(map, facilities[6]); yield return SettleStatusLayout();
                            Assert.That(map.TryStatusScreenPoint(facilities[6].Id, out _), Is.True);
                            AssertStatusMarkerGeometry(view, map, facilities);
                            if (!large && zoom == 8) yield return CaptureStatusLayoutImage("selected-zoom8");
                        }
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("FacilityProgress")]
        public IEnumerator FacilityProgressBarsFollowDomainValuesAndDisposeWithFacilityAndSession()
        {
            using (var input = new PointerScene())
            using (new LaboratoryGameViewSize(1280, 720))
            {
                yield return input.Load();
                yield return SettleStatusLayoutSize(new Vector2Int(1280, 720));
                var view = input.View; var map = input.Map;
                var site = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(2, 3), QuarterTurn.None, false);
                var processing = CreateStatusLayoutFacility(view, "processingId", new Vector2Int(6, 3), QuarterTurn.None, true);
                var cultivation = CreateStatusLayoutFacility(view, "cultivationId", new Vector2Int(10, 3), QuarterTurn.None, true);
                var gathering = CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(3, 7), QuarterTurn.None, true);
                var facilities = new List<Thing> { site, processing, cultivation, gathering };
                view.OpenPanel(LaboratoryPanel.None); map.Fit(); yield return SettleStatusLayout();
                Assert.That(map.VisibleStatusCount, Is.EqualTo(4));
                AssertProgressMatches(view, facilities);
                var production = processing.GetComponent<ProductionComponent>();
                typeof(ProductionComponent).GetProperty(nameof(ProductionComponent.InProgress)).SetValue(production, true);
                typeof(ProductionComponent).GetProperty(nameof(ProductionComponent.Progress)).SetValue(production, ((FacilityData)processing.Definition).ProductionSeconds * .5);
                var construction = site.GetComponent<ConstructionComponent>();
                typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Progress)).SetValue(construction, ((FacilityData)site.Definition).BuildSeconds * .5);
                yield return SettleStatusLayout();
                Assert.That(view.StatusFor(processing).Kind, Is.EqualTo(FacilityStatusKind.ProductionAutomatic));
                AssertProgressMatches(view, facilities);
                yield return CaptureStatusLayoutImage("partial-and-waiting");
                typeof(ProductionComponent).GetProperty(nameof(ProductionComponent.Outstanding)).SetValue(production, 1);
                yield return SettleStatusLayout();
                Assert.That(view.StatusFor(processing).ProgressFraction, Is.EqualTo(1));
                AssertProgressMatches(view, facilities);
                yield return CaptureStatusLayoutImage("output-waiting");
                typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.IsComplete)).SetValue(construction, true);
                yield return SettleStatusLayout();
                Assert.That(map.TryStatusScreenPoint(site.Id, out _), Is.False);
                Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(3));
                view.Simulation.Registry.Remove(cultivation); yield return SettleStatusLayout();
                Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(2));
                for (var cycle = 0; cycle < 3; cycle++)
                {
                    view.OpenPanel(LaboratoryPanel.Build); yield return SettleStatusLayout();
                    view.OpenPanel(LaboratoryPanel.None); map.Fit(); yield return SettleStatusLayout();
                    Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(2));
                    AssertUniqueStatusAndButtonObjects(view);
                }
                Assert.That(view.Restart(), Is.True); yield return SettleStatusLayout();
                Assert.That(StatusMarkerRoots(view, true), Is.Empty);
                Assert.That(map.VisibleStatusCount, Is.Zero);
                CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(3, 3), QuarterTurn.None, true);
                yield return SettleStatusLayout();
                Assert.That(StatusMarkerRoots(view, true).Length, Is.EqualTo(1));
                AssertUniqueStatusAndButtonObjects(view);
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static void AssertProgressMatches(LaboratoryView view, IEnumerable<Thing> facilities)
        {
            foreach (var facility in facilities)
            {
                var status = view.StatusFor(facility);
                var root = StatusMarkerRoots(view, false).Single(r => r.name.EndsWith(facility.Id.ToString("N")));
                var track = root.Find("Track").GetComponent<Image>();
                var fill = track.transform.Find("Progress").GetComponent<Image>();
                Assert.That(fill.rectTransform.rect.width, Is.EqualTo(34 * status.ProgressFraction).Within(.01));
                var working = status.Kind == FacilityStatusKind.ConstructionWorking || status.Kind == FacilityStatusKind.GatheringWorking
                    || status.Kind == FacilityStatusKind.ProductionWorking || status.Kind == FacilityStatusKind.ProductionAutomatic;
                Assert.That(fill.color, Is.EqualTo(working ? new Color(.38f, .88f, .7f) : new Color(.95f, .68f, .30f)));
                if (!working && status.ProgressFraction == 0)
                    Assert.That(track.color.r, Is.GreaterThan(track.color.g), "Zero progress waits remain visible through the track, not a fake fill.");
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
                Assert.That(facilities.Any(t => t.Id == id), Is.True);
                Assert.That(map.TryStatusScreenPoint(id, out var point), Is.True);
                Assert.That(bounds[i].Contains(point), Is.True);
                Assert.That(bounds[i].Overlaps(viewport), Is.True, "Offscreen bars must be hidden instead of clamped to the edge.");
                Assert.That(roots[i].rect.size, Is.EqualTo(new Vector2(36, 4)));
                Assert.That(roots[i].GetComponentsInChildren<Graphic>().Length, Is.EqualTo(2));
                Assert.That(roots[i].GetComponentsInChildren<Graphic>().All(g => !g.raycastTarget), Is.True);
                Assert.That(roots[i].GetComponentsInChildren<TextMeshProUGUI>(), Is.Empty);
                Assert.That(bounds[i].xMin, Is.EqualTo(Mathf.Round(bounds[i].xMin)).Within(.01));
                Assert.That(bounds[i].yMin, Is.EqualTo(Mathf.Round(bounds[i].yMin)).Within(.01));
                for (var j = 0; j < i; j++) Assert.That(bounds[i].Overlaps(bounds[j]), Is.False, "Adjacent facility bars overlap.");
            }
            Assert.That(roots.Select(r => r.name).Distinct().Count(), Is.EqualTo(roots.Length));
        }

        private static void CenterStatusLayoutFixture(LaboratoryMapView map, Thing facility)
        {
            // Camera positioning is test setup; device pan/pinch semantics are tested in the input fixture.
            var size = FacilityGeometry.GetSize((FacilityData)facility.Definition, facility.Placement.Rotation);
            // At maximum zoom the footprint can exceed the viewport height. Frame its top anchor.
            var center = (Vector2)facility.Placement.Origin + new Vector2((size.x - 1) * .5f, size.y - .5f);
            var viewportCenter = RectTransformUtility.WorldToScreenPoint(null, map.Viewport.TransformPoint(map.Viewport.rect.center));
            typeof(LaboratoryMapView).GetMethod("Pan", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(map, new object[] { viewportCenter - map.CellScreenPoint(center) });
        }

        private static int StatusLayoutButtonCount(LaboratoryView view) => view.GetComponentsInChildren<UiButton>(true).Length;
        private static void AssertUniqueStatusAndButtonObjects(LaboratoryView view)
        {
            var names = StatusMarkerRoots(view, true).Select(t => t.name).ToArray();
            Assert.That(names.Distinct().Count(), Is.EqualTo(names.Length));
            foreach (var siblings in view.GetComponentsInChildren<UiButton>(true).GroupBy(b => b.transform.parent))
                Assert.That(siblings.Select(b => b.name).Distinct().Count(), Is.EqualTo(siblings.Count()), "A semantic row was duplicated under one content parent.");
        }

        private static void LogStatusEvidence(string message)
        { LogAssert.Expect(LogType.Log, message); Debug.Log(message); }

        private static IEnumerator CaptureStatusLayoutImage(string state) => AppSceneFlowTests.Capture("facility-progress",
            Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/PLAN-032/screens", state + "-" + Screen.width + "x" + Screen.height + ".png")));
    }
}
#endif
