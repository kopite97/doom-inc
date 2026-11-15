#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("SelectionOutline")]
        public IEnumerator SelectionOutlineFollowsBodiesAndRestoresSharedMaterials()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, false);
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var facilities = new[] {
                    CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(3, 3), QuarterTurn.None, true),
                    CreateStatusLayoutFacility(view, "processingId", new Vector2Int(7, 3), QuarterTurn.None, true),
                    CreateStatusLayoutFacility(view, "cultivationId", new Vector2Int(3, 7), QuarterTurn.None, true),
                    CreateStatusLayoutFacility(view, "storageId", new Vector2Int(7, 7), QuarterTurn.None, true) };
                var fallback = view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
                Assert.That(view.Simulation.Map.Commands.Place(fallback.Id, new Vector2Int(10, 5)).Succeeded, Is.True);
                yield return null; yield return null;
                var original = ArtSprite(map, worker, "Body").sharedMaterial;
                var selection = view.UiAssets.SelectionMaterial;
                Assert.That(selection, Is.Not.Null);
                Assert.That(selection.shader.isSupported, Is.True);
                Assert.That(selection.IsKeywordEnabled("OUTBASE_ON") && selection.IsKeywordEnabled("OUTBASEPIXELPERF_ON"), Is.True);
                foreach (var selected in facilities.Concat(new[] { worker, fallback }))
                {
                    Assert.That(view.SelectThing(selected.Id), Is.True);
                    map.Fit(); map.ZoomBy(3); map.RequestSelectionVisibility();
                    yield return new WaitForSecondsRealtime(.3f); yield return null;
                    foreach (var target in facilities.Concat(new[] { worker, fallback }))
                    {
                        Assert.That(ArtSprite(map, target, "Body").sharedMaterial, Is.SameAs(target == selected ? selection : original));
                        Assert.That(ArtSprite(map, target, "Cargo").sharedMaterial, Is.SameAs(original));
                    }
                    Assert.That(map.World.Floor.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().sharedMaterial, Is.SameAs(original));
                    Assert.That(map.World.Root.GetComponentsInChildren<SpriteRenderer>(true).Any(r => r.name.StartsWith("Selection_")), Is.False);
                    yield return CaptureSelection("body-" + selected.Definition.DefinitionId);
                }
                view.ClearSelection(); yield return null; yield return null;
                Assert.That(ArtSprite(map, fallback, "Body").sharedMaterial, Is.SameAs(original));
                Assert.That(selection.GetFloat("_OutlineAlpha"), Is.EqualTo(1), "Clearing must not mutate the shared selection asset.");
                Assert.That(view.SelectThing(worker.Id), Is.True);
                var frames = new System.Collections.Generic.HashSet<Sprite>();
                for (var i = 0; i < 12; i++)
                {
                    view.AdvanceFrame(.1, 0); yield return null;
                    var body = ArtSprite(map, worker, "Body"); frames.Add(body.sprite);
                    Assert.That(body.sharedMaterial, Is.SameAs(selection));
                }
                Assert.That(frames.Count, Is.GreaterThan(1));
                foreach (var zoom in new[] { 1, 8 })
                {
                    map.Fit(); map.ZoomBy(zoom); map.RequestSelectionVisibility();
                    yield return new WaitForSecondsRealtime(.3f);
                    yield return CaptureSelection("animated-zoom-" + zoom);
                }
                var oldTexture = map.World.FallbackBody.texture;
                yield return input.Reopen();
                Assert.That(oldTexture == null, Is.True, "Screen-owned fallback texture must be released.");
                Assert.That(input.View.SelectedId, Is.EqualTo(Guid.Empty));
                Assert.That(input.Map.World.Root.GetComponentsInChildren<SpriteRenderer>().Any(r => r.sharedMaterial == selection), Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("SelectionOutline")]
        public IEnumerator SelectionOutlineClearsOnGroundWithMouseAndTouchInBothLayouts()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    Assert.That(view.SelectThing(worker.Id), Is.True);
                    map.Fit(); yield return null; yield return null;
                    yield return CaptureSelection("selected-" + size.x);
                    yield return input.Click(SelectionGround(input));
                    AssertSelectionCleared(input);
                    yield return CaptureSelection("cleared-" + size.x);
                    Assert.That(view.SelectThing(worker.Id), Is.True); view.ExpandSelection();
                    map.Fit(); yield return null; yield return null;
                    yield return input.Tap(701, SelectionGround(input));
                    AssertSelectionCleared(input);
                    var other = view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
                    Assert.That(view.Simulation.Map.Commands.Place(other.Id, worker.Placement.Origin).Succeeded, Is.True);
                    view.ShowSelectionCandidates(new[] { worker.Id, other.Id }); yield return null; yield return null;
                    Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Candidates));
                    yield return input.Tap(702, SelectionGround(input));
                    AssertSelectionCleared(input);
                    Assert.That(view.Simulation.Map.Commands.Place(other.Id, new Vector2Int(10, 5)).Succeeded, Is.True);
                    Assert.That(view.SelectThing(worker.Id), Is.True);
                    view.OpenPanel(LaboratoryPanel.Creatures); yield return null; yield return null;
                    yield return input.Click(SelectionGround(input));
                    Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty));
                    Assert.That(view.OpenedPanel, Is.EqualTo(LaboratoryPanel.Creatures), "Ground must not close an unrelated menu.");
                    view.Back(); yield return null; yield return null;
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("SelectionOutline")]
        public IEnumerator SelectionOutlinePreservesEditingAndCancelsRevealOnDeselect()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, false);
                var facility = CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(3, 3), QuarterTurn.None, true);
                yield return null; yield return null;
                Assert.That(view.SelectThing(facility.Id), Is.True); yield return null; yield return null;
                Assert.That(view.BeginPlacement(), Is.True);
                yield return null; yield return null;
                yield return input.Click(map.CellScreenPoint(new Vector2Int(8, 5)));
                Assert.That(view.SelectedId, Is.EqualTo(facility.Id));
                Assert.That(view.Simulation.Map.Commands.Edit.Origin, Is.EqualTo(new Vector2Int(8, 5)));
                Assert.That(map.World.Root.GetComponentsInChildren<SpriteRenderer>().Any(r => r.name.StartsWith("PlacementBorder_") && r.gameObject.activeInHierarchy), Is.True);
                Assert.That(view.CancelPlacement(), Is.True); yield return null; yield return null;
                map.Fit(); map.ZoomBy(8); yield return null; yield return null;
                view.SelectThing(facility.Id); yield return null;
                view.ClearSelection(); yield return null; yield return null;
                var point = map.CellScreenPoint(Vector2.zero);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(Vector2.Distance(map.CellScreenPoint(Vector2.zero), point), Is.LessThan(.01f));
                AssertSelectionCleared(input);
                view.SelectThing(facility.Id); yield return null;
                Assert.That(view.Restart(), Is.True); yield return null; yield return null;
                Assert.That(map.World.Root.Find("ThingArt_" + facility.Id.ToString("N")), Is.Null);
                AssertSelectionCleared(input);
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Vector2 SelectionGround(PointerScene input)
        {
            var view = input.View;
            for (var y = 1; y < view.Simulation.Map.Height - 1; y++)
                for (var x = 1; x < view.Simulation.Map.Width - 1; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (view.Simulation.Registry.Things.Any(t => t.IsPlaced && Vector2.Distance(t.Placement.Origin, cell) < 3)) continue;
                    var point = input.Map.CellScreenPoint(cell);
                    if (RectTransformUtility.RectangleContainsScreenPoint(view.MapViewport, point, null)) return point;
                }
            throw new AssertionException("No visible empty ground available in fixture.");
        }

        private static void AssertSelectionCleared(PointerScene input)
        {
            Assert.That(input.View.SelectedId, Is.EqualTo(Guid.Empty));
            Assert.That(input.View.OpenedPanel, Is.EqualTo(LaboratoryPanel.None));
            Assert.That(input.Map.World.Root.GetComponentsInChildren<SpriteRenderer>().Any(r => r.sharedMaterial == input.View.UiAssets.SelectionMaterial), Is.False);
        }

        private static IEnumerator CaptureSelection(string name)
        {
            yield return null;
            yield return WaitFor(() => !UnityEditor.ShaderUtil.anythingCompiling, "Selection shader compilation did not settle.");
            yield return null;
            yield return AppSceneFlowTests.Capture("selection-outline",
                Path.Combine(Application.dataPath, "../Temp/PLAN-030/screens", name + ".png"));
        }
    }
}
#endif
