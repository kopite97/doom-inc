#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator ResponsiveLayoutsPreserveSessionAndDraftAcrossSixScreenSizes()
        {
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN013-Layout-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                yield return LoadIsolatedScene(Path.Combine(directory, "session.json"), 30, loaded => { view = loaded; view.SimulationRate = 0; });
                var session = view.Controller.Current; var store = GameDataStore.Instance; var data = store.Catalog;
                var worker = view.Simulation.Registry.Things.First(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(view.SelectThing(worker.Id), Is.True);
                Assert.That(view.BeginStorageConstruction(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(4, 3)), Is.True);
                Assert.That(view.RotatePreview(), Is.True);
                foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280), new Vector2Int(1600, 720),
                    new Vector2Int(720, 1600), new Vector2Int(1024, 768), new Vector2Int(768, 1024) })
                {
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y, "Game View did not adopt requested size " + size);
                        yield return null; yield return null;
                        Assert.That(view.Controller.Current, Is.SameAs(session)); Assert.That(GameDataStore.Instance, Is.SameAs(store));
                        Assert.That(store.Catalog, Is.SameAs(data)); Assert.That(view.SelectedId, Is.EqualTo(worker.Id));
                        Assert.That(view.Simulation.ConstructionOrigin, Is.EqualTo(new Vector2Int(4, 3)));
                        Assert.That(view.Simulation.ConstructionRotation, Is.EqualTo(QuarterTurn.Clockwise90));
                        Assert.That(view.Controller.CanAdvanceLaboratory, Is.False);
                        Assert.That(view.MapViewport.rect.width, Is.GreaterThan(100)); Assert.That(view.MapViewport.rect.height, Is.GreaterThan(100));
                        var corners = new Vector3[4]; view.UiSafeRoot.GetWorldCorners(corners);
                        Assert.That(corners.Min(p => p.x), Is.GreaterThanOrEqualTo(-1));
                        Assert.That(corners.Max(p => p.x), Is.LessThanOrEqualTo(Screen.width + 1));
                        Assert.That(corners.Min(p => p.y), Is.GreaterThanOrEqualTo(-1));
                        Assert.That(corners.Max(p => p.y), Is.LessThanOrEqualTo(Screen.height + 1));
                        var map = view.MapViewport.GetComponent<LaboratoryMapView>();
                        foreach (var cell in new[] { Vector2Int.zero, new Vector2Int(15, 9), new Vector2Int(7, 4) })
                        {
                            Assert.That(map.TryScreenCell(map.CellScreenPoint(cell), out var actual), Is.True);
                            Assert.That(actual, Is.EqualTo(cell));
                        }
                        yield return new WaitForEndOfFrame();
                        var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/plan013-layout-" + size.x + "x" + size.y + ".png"));
                        Directory.CreateDirectory(Path.GetDirectoryName(path)); ScreenCapture.CaptureScreenshot(path); yield return null;
                    }
                }
                Assert.That(view.CancelPlacement(), Is.True);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
#endif
