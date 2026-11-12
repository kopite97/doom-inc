#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator FloorArtKeepsCellIdentityInputAndSaveAcrossLayoutAndSceneChanges()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                var assets = view.UiAssets;
                var tiles = FloorSprites(input.Map);
                Assert.That(tiles.Length, Is.EqualTo(view.Simulation.Map.Width * view.Simulation.Map.Height));
                var original = tiles;
                Assert.That(original.Distinct().Count(), Is.EqualTo(4));
                Assert.That(original.Count(s => s == assets.GetFloorTile(0)) / (float)original.Length, Is.InRange(.6f, .8f));
                Assert.That(original.Select(s => s.texture).Distinct().Count(), Is.EqualTo(1));
                Assert.That(input.Map.World.Floor.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>(), Is.Null);
                Assert.That(input.Map.World.Floor.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().sortingOrder, Is.LessThan(0));
                var codec = new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                var before = codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase));
                var random = UnityEngine.Random.state;
                typeof(LaboratoryMapView).GetMethod("Draw", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(input.Map, null);
                Assert.That(UnityEngine.Random.state, Is.EqualTo(random), "Drawing must not consume gameplay randomness.");

                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(size.x, size.y))
                {
                    yield return SettleScreenStateSize(size);
                    foreach (var zoom in new[] { 1, 3, 8 })
                    {
                        input.Map.Fit(); input.Map.ZoomBy(zoom);
                        yield return null; yield return null;
                        CollectionAssert.AreEqual(original, FloorSprites(input.Map));
                        var floor = input.Map.World.Floor;
                        Assert.That(floor.GetCellCenterWorld(Vector3Int.right) - floor.GetCellCenterWorld(Vector3Int.zero), Is.EqualTo(Vector3.right * 2));
                        Assert.That(floor.GetCellCenterWorld(Vector3Int.up) - floor.GetCellCenterWorld(Vector3Int.zero), Is.EqualTo(Vector3.up * 2));
                        Assert.That(tiles[0].bounds.size.x, Is.EqualTo(2), "Tile sprite width matches cell spacing without gutters.");
                        yield return CaptureFloorArt("mixed-" + size.x + "x" + size.y + "-zoom" + zoom);
                    }
                }
                input.Map.Fit(); yield return null; yield return null;
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)), Is.EqualTo(before),
                    "Floor layout changes must preserve the entire saved simulation state.");

                try
                {
                    view.ConfigureUiAssets(CopyFloorAssets(assets, new[] { assets.GetFloorTile(0) }));
                    yield return null; yield return null;
                    Assert.That(FloorSprites(input.Map).All(i => i == assets.GetFloorTile(0)), Is.True);
                    yield return CaptureFloorArt("base-only-1280x720-zoom1");
                }
                finally { view.ConfigureUiAssets(assets); }
                var evidence = Path.Combine(RegressionEvidenceRoot, "floor", "distribution.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(evidence));
                File.WriteAllLines(evidence, original.GroupBy(s => s.name).OrderBy(g => g.Key).Select(g => g.Key + "=" + g.Count()));

                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                yield return input.Tap(71, input.Map.CellScreenPoint(worker.Placement.Origin));
                Assert.That(view.SelectedId, Is.EqualTo(worker.Id), "Floor graphics must not intercept map selection.");
                yield return ClickVisibleButton(input, "BuildGathering");
                yield return input.Click(input.Map.CellScreenPoint(new Vector2Int(3, 3)));
                Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(new Vector2Int(3, 3)));
                Assert.That(input.Map.World.Root.GetComponentsInChildren<SpriteRenderer>().Any(i => i.name.StartsWith("Selection_") && i.color.g > .9f && i.color.a > .9f),
                    Is.True, "The valid construction overlay must remain visible above art.");
                yield return CaptureFloorArt("construction-preview");
                yield return input.Escape();

                try
                {
                    view.ConfigureUiAssets(CopyFloorAssets(assets, null));
                    yield return null; yield return null;
                    Assert.That(FloorSprites(input.Map).All(i => i == input.Map.World.Solid), Is.True,
                        "Missing floor art uses the shared solid fallback, with no UI grid behind it.");
                    Assert.That(input.Map.GetComponentsInChildren<Image>().Any(i => i.name == "Temp_MapCell"), Is.False);
                    var taps = input.Map.AcceptedTaps;
                    yield return input.Tap(72, input.Map.CellScreenPoint(new Vector2Int(4, 4)));
                    Assert.That(input.Map.AcceptedTaps, Is.EqualTo(taps + 1));
                }
                finally { view.ConfigureUiAssets(assets); }
                yield return null; yield return null;
                CollectionAssert.AreEqual(original, FloorSprites(input.Map));
                Assert.That(view.Save(), Is.True);
                var saved = File.ReadAllText(view.SavePath);
                yield return input.Reopen();
                CollectionAssert.AreEqual(original, FloorSprites(input.Map));
                Assert.That(File.ReadAllText(input.View.SavePath), Is.EqualTo(saved), "Opening the scene must not rewrite the save for art.");
                Assert.That(input.View.Restart(), Is.True);
                yield return null; yield return null;
                CollectionAssert.AreEqual(original, FloorSprites(input.Map));
                Assert.That(input.Map.World.Root.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>().Length, Is.EqualTo(1));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Sprite[] FloorSprites(LaboratoryMapView map)
        {
            var bounds = map.World.Floor.cellBounds;
            return Enumerable.Range(0, bounds.size.x * bounds.size.y)
                .Select(i => map.World.Floor.GetSprite(new Vector3Int(i % bounds.size.x, i / bounds.size.x, 0))).ToArray();
        }

        private static LaboratoryUiAssets CopyFloorAssets(LaboratoryUiAssets assets, Sprite[] floor)
        {
            var icons = System.Enum.GetValues(typeof(LaboratoryIcon)).Cast<LaboratoryIcon>().Select(assets.GetIcon).ToArray();
            return new LaboratoryUiAssets(assets.Font, icons, assets.FontLicense, assets.ReleaseSound, floor, assets.Art, assets.WorldMaterial);
        }

        private static IEnumerator CaptureFloorArt(string name)
        {
            var path = Path.Combine(RegressionEvidenceRoot, "floor", name + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return WaitFor(() => File.Exists(path) && new FileInfo(path).Length > 0, "Floor screenshot was not written.");
        }
    }
}
#endif
