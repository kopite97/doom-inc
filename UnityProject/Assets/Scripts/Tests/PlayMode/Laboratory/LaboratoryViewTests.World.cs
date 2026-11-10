#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator WorldRenderingPreservesGridProjectionAllRotationsKindsAndLifetime()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                for (var r = 0; r < 4; r++)
                {
                    CreateStatusLayoutFacility(view, "storageId", new Vector2Int(1 + r * 4, 1), (QuarterTurn)r, true);
                    CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(1 + r * 4, 6), (QuarterTurn)r, true);
                }
                foreach (var definition in GameDataStore.Instance.Catalog.Definitions.Values.OfType<CreatureData>().Where(d => !view.UiAssets.Art.HasPrototype(d)))
                {
                    var creature = view.Simulation.Registry.Create(definition);
                    Assert.That(view.Simulation.Map.Commands.Place(creature.Id, new Vector2Int(7, 4)).Succeeded, Is.True);
                }
                yield return null; yield return null;
                var codec = new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                var before = codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase));
                Assert.That(map.GetComponentsInChildren<Image>().Any(i => i.name == "Temp_MapCell" || i.name == "FloorCell"), Is.False);
                Assert.That(map.Viewport.GetComponent<Image>().color.a, Is.Zero);
                Assert.That(map.World.Camera.cullingMask, Is.EqualTo(1 << LayerMask.NameToLayer(LaboratoryWorldMap.LayerName)));
                Assert.That(Camera.main, Is.SameAs(map.World.Camera));
                Assert.That(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.isActiveAndEnabled), Is.EqualTo(1));
                Assert.That(map.World.Root.GetComponentsInChildren<AudioListener>().Length, Is.Zero);
                var placed = view.Simulation.Registry.Things.Where(t => t.IsPlaced && t.Definition.Kind != ThingKind.Resource).ToArray();
                Assert.That(map.World.Root.GetComponentsInChildren<SortingGroup>().Length, Is.EqualTo(placed.Length));
                foreach (var thing in placed)
                {
                    var body = ArtSprite(map, thing, "Body");
                    Assert.That(body.sprite, Is.Not.Null);
                    var size = thing.Definition is FacilityData facility ? (Vector2)FacilityGeometry.GetSize(facility, thing.Placement.Rotation) : Vector2.one;
                    Assert.That((Vector2)body.transform.parent.position,
                        Is.EqualTo(((Vector2)thing.Placement.Origin + size * .5f) * 2));
                    if (!(thing.Definition is FacilityData definition)) continue;
                    Assert.That(Vector2.Distance(body.bounds.size, size * 2), Is.LessThan(.09f),
                        "Supplied and fallback facility footprints preserve rotated dimensions.");
                    var interaction = body.transform.parent.Find("InteractionCell");
                    var entrance = FacilityGeometry.GetInteractionCell(definition, thing.Placement.Origin, thing.Placement.Rotation);
                    Assert.That((Vector2)interaction.position, Is.EqualTo(((Vector2)entrance + Vector2.one * .5f) * 2));
                }
                foreach (var resolution in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(resolution.x, resolution.y))
                {
                    yield return SettleScreenStateSize(resolution);
                    foreach (var zoom in new[] { 1f, 2.35f, 3f, 8f })
                    {
                        map.Fit(); map.ZoomBy(zoom); yield return null; yield return null;
                        var floor = map.World.Floor;
                        for (var y = 0; y < view.Simulation.Map.Height; y++)
                        for (var x = 0; x < view.Simulation.Map.Width; x++)
                        {
                            var cell = new Vector2Int(x, y);
                            Assert.That(floor.GetSprite(new Vector3Int(x, y, 0)), Is.SameAs(view.UiAssets.GetFloorTile(LaboratoryWorldMap.FloorVariant(cell))));
                            foreach (var offset in new[] { Vector2.zero, new Vector2(-.499f,-.499f), new Vector2(.499f,-.499f), new Vector2(-.499f,.499f), new Vector2(.499f,.499f) })
                            {
                                var screen = map.CellScreenPoint((Vector2)cell + offset);
                                var worldPoint = ((Vector2)cell + offset + Vector2.one * .5f) * 2;
                                Assert.That(Vector2.Distance(screen, map.World.Camera.WorldToScreenPoint(worldPoint)), Is.LessThan(.002f), "Projection: " + resolution + " zoom=" + zoom + " cell=" + cell + " offset=" + offset + " pixelRect=" + map.World.Camera.pixelRect);
                                var visible = RectTransformUtility.RectangleContainsScreenPoint(map.Viewport, screen, null);
                                Assert.That(map.TryScreenCell(screen, out var actual), Is.EqualTo(visible));
                                if (visible) Assert.That(actual, Is.EqualTo(cell));
                            }
                        }
                        Assert.That(map.TryScreenCell(new Vector2(-1, -1), out _), Is.False);
                    }
                }
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)), Is.EqualTo(before));
                var zero = new GameObject("ZeroViewport", typeof(RectTransform)).GetComponent<RectTransform>();
                zero.sizeDelta = Vector2.zero;
                map.World.SyncCamera(zero, Vector2.zero, 1); Assert.That(map.World.Camera.enabled, Is.False);
                UnityEngine.Object.Destroy(zero.gameObject);
                yield return null; yield return null;
                Assert.That(map.World.Camera.enabled, Is.True);
                var root = map.World.Root;
                UnityEngine.Object.Destroy(view.gameObject); yield return null; yield return null;
                Assert.That(root == null, Is.True, "World objects cannot outlive their screen.");
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator WorldRenderingCarriesCultureFallbackThroughActualPickupAndRestore()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View;
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                foreach (var resource in worker.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                    view.Simulation.Registry.ConsumeResource(resource, resource.GetComponent<ResourceStackComponent>().Quantity);
                var source = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(3,3), QuarterTurn.None, true);
                CreateStatusLayoutFacility(view, "storageId", new Vector2Int(10,3), QuarterTurn.None, true);
                var culture = GameDataStore.Instance.Catalog.Selected<ResourceData>("cultureId");
                var shipment = view.Simulation.Registry.CreateResource(culture, 5, source.GetComponent<ResourceContainerComponent>());
                Assert.That(view.SetAllowed(source.Id, culture, false), Is.True);
                yield return null; yield return null;
                Assert.That(ArtSprite(input.Map, worker, "Cargo").gameObject.activeSelf, Is.False);
                yield return AdvanceArtUntil(view, () => shipment.HolderId == worker.Id, 30);
                yield return null;
                var cargo = ArtSprite(input.Map, worker, "Cargo");
                Assert.That(cargo.gameObject.activeSelf, Is.True);
                Assert.That(cargo.sprite, Is.SameAs(culture.Assets.Icon != null ? culture.Assets.Icon : input.Map.World.Solid));
                Assert.That(cargo.transform.parent.name, Is.EqualTo("CarryAnchor"));
                var body = ArtSprite(input.Map, worker, "Body");
                var position = cargo.transform.position; var scale = cargo.transform.lossyScale;
                body.transform.localScale = new Vector3(.5f, 1.3f, 1); body.transform.localRotation = Quaternion.Euler(0,0,45);
                Assert.That(cargo.transform.position, Is.EqualTo(position)); Assert.That(cargo.transform.lossyScale, Is.EqualTo(scale));
                Assert.That(view.Save(), Is.True); Assert.That(view.Reload(), Is.True); yield return null; yield return null;
                view.Simulation.Registry.TryGet(worker.Id, out worker);
                view.Simulation.Registry.TryGet(shipment.Id, out shipment);
                Assert.That(ArtSprite(input.Map, worker, "Cargo").gameObject.activeSelf, Is.True);
                yield return AdvanceArtUntil(view, () => shipment.HolderId != worker.Id, 30);
                yield return null;
                Assert.That(ArtSprite(input.Map, worker, "Cargo").gameObject.activeSelf, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }
    }
}
#endif
