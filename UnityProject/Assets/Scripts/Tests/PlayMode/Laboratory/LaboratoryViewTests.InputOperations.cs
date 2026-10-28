#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.GameFlow;
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
        public IEnumerator DeviceFacilityOperationsAndIndividualAssignmentsWorkInBothOrientations()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                // Completed facilities isolate UI wiring; the existing paid-cycle tests verify their construction.
                var storage = AddInputFacility(view, "storageId", new Vector2Int(8, 3));
                var processor = AddInputFacility(view, "processingId", new Vector2Int(3, 7));
                var cultivator = AddInputFacility(view, "cultivationId", new Vector2Int(8, 7));
                var prototype = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var support = GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId");
                var first = view.Simulation.Registry.Create(support); var second = view.Simulation.Registry.Create(support);
                Assert.That(view.Simulation.Map.Commands.Place(first.Id, prototype.Placement.Origin).Succeeded, Is.True);
                Assert.That(view.Simulation.Map.Commands.Place(second.Id, prototype.Placement.Origin).Succeeded, Is.True);
                var session = view.Controller.Current;

                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                {
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y,
                            "Input test Game View did not adopt " + size);
                        yield return null; yield return null;
                        Assert.That(view.Controller.Current, Is.SameAs(session));
                        yield return ClickVisibleButton(input, "Select_" + storage.Id.ToString("N"));
                        Assert.That(view.SelectedId, Is.EqualTo(storage.Id));
                        var original = storage.Placement;
                        var destination = original.Origin.x == 8 ? new Vector2Int(12, 3) : new Vector2Int(8, 3);
                        yield return ClickVisibleButton(input, "Move");
                        yield return input.Tap(30, input.Map.CellScreenPoint(destination));
                        yield return ClickVisibleButton(input, "Rotate");
                        var rotation = (QuarterTurn)(((int)original.Rotation + 1) % 4);
                        Assert.That(view.Simulation.Map.Commands.Edit.Origin, Is.EqualTo(destination));
                        Assert.That(view.Simulation.Map.Commands.Edit.Rotation, Is.EqualTo(rotation));
                        yield return ClickVisibleButton(input, "Cancel");
                        Assert.That(view.IsEditing, Is.False);
                        Assert.That(storage.Placement.Origin, Is.EqualTo(original.Origin));
                        Assert.That(storage.Placement.Rotation, Is.EqualTo(original.Rotation));
                        yield return ClickVisibleButton(input, "Move");
                        yield return input.Tap(31, input.Map.CellScreenPoint(destination));
                        yield return ClickVisibleButton(input, "Rotate");
                        yield return ClickVisibleButton(input, "Confirm");
                        Assert.That(view.IsEditing, Is.False);
                        Assert.That(storage.Placement.Origin, Is.EqualTo(destination));
                        Assert.That(storage.Placement.Rotation, Is.EqualTo(rotation));

                        var allowances = storage.GetComponent<StorageComponent>();
                        var organic = allowances.AllowsOrganic; var culture = allowances.AllowsCulture;
                        yield return ClickVisibleButton(input, "AllowOrganic");
                        Assert.That(allowances.AllowsOrganic, Is.EqualTo(!organic));
                        Assert.That(allowances.AllowsCulture, Is.EqualTo(culture));
                        yield return ClickVisibleButton(input, "AllowCulture");
                        Assert.That(allowances.AllowsCulture, Is.EqualTo(!culture));
                        yield return ClickVisibleButton(input, "Select_" + cultivator.Id.ToString("N"));
                        yield return ClickVisibleButton(input, "CultivateSupport");
                        Assert.That(cultivator.GetComponent<ProductionComponent>().SelectedTarget, Is.EqualTo(CultivationTarget.Support));
                        yield return ClickVisibleButton(input, "CultivateInvader");
                        Assert.That(cultivator.GetComponent<ProductionComponent>().SelectedTarget, Is.EqualTo(CultivationTarget.Invader));

                        // Roster IDs must identify every occupant of the same map cell in both layouts.
                        yield return ClickVisibleButton(input, "Select_" + first.Id.ToString("N"));
                        Assert.That(view.SelectedId, Is.EqualTo(first.Id));
                        yield return ClickVisibleButton(input, "Assign_" + processor.Id.ToString("N"));
                        Assert.That(first.GetComponent<WorkerComponent>().AssignedFacilityId, Is.EqualTo(processor.Id));
                        Assert.That(second.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
                        yield return ClickVisibleButton(input, "Select_" + second.Id.ToString("N"));
                        Assert.That(view.SelectedId, Is.EqualTo(second.Id));
                        yield return ClickVisibleButton(input, "Assign_" + cultivator.Id.ToString("N"));
                        Assert.That(second.GetComponent<WorkerComponent>().AssignedFacilityId, Is.EqualTo(cultivator.Id));
                        Assert.That(first.GetComponent<WorkerComponent>().AssignedFacilityId, Is.EqualTo(processor.Id));
                        yield return ClickVisibleButton(input, "Unassign");
                        Assert.That(second.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
                        yield return ClickVisibleButton(input, "Select_" + first.Id.ToString("N"));
                        yield return ClickVisibleButton(input, "Unassign");
                        Assert.That(first.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
                        Assert.That(prototype.GetComponent<WorkerComponent>().AssignedFacilityId, Is.Null);
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceReleaseReportsAndResultsRestartUseCurrentSessionCommands()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var store = GameDataStore.Instance;
                // Waiting invader fixture avoids repeating the production economy in an input wiring test.
                var invader = view.Simulation.Registry.Create(store.Catalog.Selected<CreatureData>("invaderId"));
                Assert.That(view.Simulation.Map.Commands.Place(invader.Id, Vector2Int.one).Succeeded, Is.True);
                yield return ClickVisibleButton(input, "Release");
                Assert.That(view.Simulation.Invasion.WaitingCount, Is.Zero);
                Assert.That(view.Simulation.Invasion.ExternalCount, Is.EqualTo(1));
                Assert.That(invader.IsPlaced, Is.False);
                view.Controller.Advance(view.Invasion.Settings.ActivitySeconds);
                Assert.That(view.Invasion.Reports.Count, Is.GreaterThan(0));
                yield return ClickVisibleButton(input, "Reports");
                Assert.That(view.ReportsExpanded, Is.True);
                var report = view.Invasion.Reports.Last();
                Assert.That(view.UiCanvas.GetComponentsInChildren<Text>().Any(label => label.text.Contains(report.Text)), Is.True,
                    "The expanded report UI must display the actual activity report.");
                yield return ClickVisibleButton(input, "Reports");
                Assert.That(view.ReportsExpanded, Is.False);

                // Move to the result boundary through the same simulation; only the restart is under input test.
                var ids = view.Simulation.Registry.Things.Select(t => t.Id).ToArray();
                var generation = view.Controller.Generation;
                view.Controller.Advance(1000);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Destruction));
                yield return WaitFor(() => view.Controller.Phase == SessionPhase.Results, "The actual ending did not reach its results UI.");
                yield return ClickVisibleButton(input, "RestartFromResults");
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Playing));
                Assert.That(view.Controller.Generation, Is.EqualTo(generation + 1), "One click must restart exactly once.");
                Assert.That(GameDataStore.Instance, Is.SameAs(store));
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id).Intersect(ids), Is.Empty);
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Creature), Is.EqualTo(1));
                Assert.That(view.Simulation.Registry.Things.Count(t => t.Definition.Kind == ThingKind.Facility), Is.Zero);
                Assert.That(view.Invasion.Score, Is.Zero);
                Assert.That(view.ReportsExpanded, Is.False);
                Assert.That(view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                    .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(4));
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator DeviceTouchScrollOwnsCrossBoundaryAndMixedContactsThenMapCanPanToLastCell()
        {
            using (var input = new PointerScene())
            using (new LaboratoryGameViewSize(720, 1280))
            {
                yield return WaitFor(() => Screen.width == 720 && Screen.height == 1280,
                    "The touch-scroll scenario did not adopt its portrait screen size.");
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var prototype = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var lastCell = new Vector2Int(view.Simulation.Map.Width - 1, view.Simulation.Map.Height - 1);
                var cornerWorker = view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
                Assert.That(view.Simulation.Map.Commands.Place(cornerWorker.Id, lastCell).Succeeded, Is.True);
                yield return input.Tap(50, map.CellScreenPoint(prototype.Placement.Origin));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));
                var accepted = map.AcceptedTaps;
                var scroll = view.UiCanvas.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "Controls");
                Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height));
                var controls = (RectTransform)scroll.transform;
                // The narrow panel border is a real raycast target outside every action button.
                var uiPoint = RectTransformUtility.WorldToScreenPoint(null,
                    controls.TransformPoint(new Vector2(controls.rect.xMax - 2, controls.rect.center.y)));
                var mapPoint = ScreenCenter(view.MapViewport);
                var initialOffset = scroll.content.anchoredPosition.y;
                var up = Vector2.up * scroll.viewport.rect.height * view.UiCanvas.scaleFactor * .3f;
                input.Touch(51, TouchPhase.Began, uiPoint); yield return null;
                input.Touch(51, TouchPhase.Moved, uiPoint + up * .5f); yield return null;
                input.Touch(51, TouchPhase.Moved, uiPoint + up); yield return null;
                Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(initialOffset + 10),
                    "A device touch drag must actually move the Controls ScrollRect content.");
                input.Touch(51, TouchPhase.Moved, mapPoint); yield return null;
                input.Touch(51, TouchPhase.Ended, mapPoint); yield return null; yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted), "A UI-owned drag cannot select after crossing into the map.");
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));

                var zoom = map.Zoom;
                input.Touch(52, TouchPhase.Began, uiPoint);
                input.Touch(53, TouchPhase.Began, mapPoint); yield return null;
                input.Touch(52, TouchPhase.Moved, uiPoint + up * .5f);
                input.Touch(53, TouchPhase.Moved, mapPoint + Vector2.right * 60); yield return null;
                input.Touch(52, TouchPhase.Ended, uiPoint + up * .5f); yield return null;
                input.Touch(53, TouchPhase.Moved, mapPoint - Vector2.right * 60); yield return null;
                input.Touch(53, TouchPhase.Ended, mapPoint - Vector2.right * 60); yield return null; yield return null;
                Assert.That(map.Zoom, Is.EqualTo(zoom), "A mixed UI/map pair must not zoom the map.");
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted));
                Assert.That(view.SelectedId, Is.EqualTo(prototype.Id));
                yield return input.Tap(54, map.CellScreenPoint(prototype.Placement.Origin));
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1), "A new tap must work after all mixed contacts lift.");

                yield return ClickVisibleButton(input, "ZoomIn");
                Assert.That(map.Zoom, Is.GreaterThan(zoom));
                var beforePanCorner = map.CellScreenPoint(lastCell);
                mapPoint = ScreenCenter(view.MapViewport);
                var panEnd = mapPoint - view.MapViewport.rect.size * view.UiCanvas.scaleFactor * .4f;
                input.Touch(55, TouchPhase.Began, mapPoint); yield return null;
                input.Touch(55, TouchPhase.Moved, panEnd); yield return null;
                input.Touch(55, TouchPhase.Ended, panEnd); yield return null; yield return null;
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 1));
                var cornerPoint = map.CellScreenPoint(lastCell);
                Assert.That(Vector2.Distance(cornerPoint, beforePanCorner), Is.GreaterThan(5), "A device map drag must move its projection.");
                Assert.That(RectTransformUtility.RectangleContainsScreenPoint(view.MapViewport, cornerPoint), Is.True,
                    "Panning the enlarged map must make the last grid cell reachable.");
                Assert.That(map.TryScreenCell(cornerPoint, out var actualCell), Is.True);
                Assert.That(actualCell, Is.EqualTo(lastCell));
                yield return input.Tap(56, cornerPoint);
                Assert.That(view.SelectedId, Is.EqualTo(cornerWorker.Id));
                Assert.That(map.AcceptedTaps, Is.EqualTo(accepted + 2));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Thing AddInputFacility(LaboratoryView view, string key, Vector2Int cell)
        {
            var definition = GameDataStore.Instance.Catalog.Selected<FacilityData>(key);
            var facility = view.Simulation.Registry.Create(definition);
            Assert.That(view.Simulation.Map.Commands.Place(facility.Id, cell).Succeeded, Is.True);
            var construction = facility.GetComponent<ConstructionComponent>();
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Delivered)).SetValue(construction, definition.BuildCost);
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.Progress)).SetValue(construction, (double)definition.BuildSeconds);
            typeof(ConstructionComponent).GetProperty(nameof(ConstructionComponent.IsComplete)).SetValue(construction, true);
            return facility;
        }
    }
}
#endif
