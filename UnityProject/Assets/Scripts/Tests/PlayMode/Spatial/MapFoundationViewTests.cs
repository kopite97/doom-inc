#if UNITY_EDITOR
using MobileGame.TestSupport;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed class MapFoundationViewTests
    {
        private static void AssertSameLayout(GridMap uiMap, GridMap harnessMap)
        {
            var ui = uiMap.GetPlacements();
            var harness = harnessMap.GetPlacements();
            Assert.That(ui.Count, Is.EqualTo(harness.Count));
            for (var i = 0; i < ui.Count; i++)
            {
                Assert.That(ui[i].CreationOrder, Is.EqualTo(harness[i].CreationOrder));
                Assert.That(ui[i].Origin, Is.EqualTo(harness[i].Origin));
                Assert.That(ui[i].Rotation, Is.EqualTo(harness[i].Rotation));
                Assert.That(ui[i].Footprint, Is.EqualTo(harness[i].Footprint));
                Assert.That(ui[i].InteractionCell, Is.EqualTo(harness[i].InteractionCell));
            }
        }

        [UnityTest]
        public IEnumerator SceneViewAndHarnessShareMoveRotateRejectCancelAndRenderActualState()
        {
            var previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            try
            {
                Assert.That(EditorApplication.ExecuteMenuItem("Window/General/Game"), Is.True);
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Diagnostics/Map/MapFoundation.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
                var deadline = Time.realtimeSinceStartupAsDouble + 15;
                while (!load.isDone && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Assert.That(load.isDone, Is.True);
                yield return null;
                var view = UnityEngine.Object.FindFirstObjectByType<MapFoundationView>();
                Assert.That(view, Is.Not.Null, "The saved scene must deserialize the real view.");
                Assert.That(view.Session, Is.Not.Null, "Scene SO references must create the fixture.");
                var storage = DataTestAssets.LoadAssetAtPath<FacilityData>("Assets/SO/Facility/Storage.asset");
                var creature = DataTestAssets.LoadAssetAtPath<CreatureData>("Assets/SO/Creature/PrototypeSupport.asset");
                var resource = DataTestAssets.LoadAssetAtPath<ResourceData>("Assets/SO/Resource/OrganicMatter.asset");
                var definitionBefore = EditorJsonUtility.ToJson(storage);
                var harness = new MapFoundationScenario(storage, creature, resource);
                var uiThing = view.Session.Facilities[0];
                var harnessThing = harness.Facilities[0];
                var uiId = uiThing.Id;
                var uiOrder = uiThing.CreationOrder;
                Assert.That(view.SelectThing(uiId), Is.True);
                Assert.That(view.BeginPlacement().Error, Is.EqualTo(harness.Map.Commands.BeginEdit(harnessThing.Id).Error));
                Assert.That(view.RotatePreview().Error,
                    Is.EqualTo(harness.Map.Commands.Preview(harnessThing.Placement.Origin, QuarterTurn.Clockwise90).Error));
                Assert.That(view.PreviewAt(new Vector2Int(4, 3)).Error,
                    Is.EqualTo(harness.Map.Commands.Preview(new Vector2Int(4, 3), QuarterTurn.Clockwise90).Error));
                AssertSameLayout(view.Session.Map, harness.Map);
                Assert.That(uiThing.Placement.Origin, Is.EqualTo(new Vector2Int(1, 2)), "Preview must not apply.");
                Assert.That(view.ConfirmPlacement().Succeeded, Is.True);
                Assert.That(harness.Map.Commands.Confirm().Succeeded, Is.True);
                AssertSameLayout(view.Session.Map, harness.Map);
                Assert.That(uiThing.Placement.Origin, Is.EqualTo(new Vector2Int(4, 3)));

                view.BeginPlacement();
                harness.Map.Commands.BeginEdit(harnessThing.Id);
                var uiRejected = view.PreviewAt(new Vector2Int(7, 3));
                var harnessRejected = harness.Map.Commands.Preview(new Vector2Int(7, 3), QuarterTurn.Clockwise90);
                Assert.That(uiRejected.Error, Is.EqualTo(PlacementError.FacilityOverlap));
                Assert.That(uiRejected.Error, Is.EqualTo(harnessRejected.Error));
                Assert.That(view.ConfirmPlacement().Error, Is.EqualTo(harness.Map.Commands.Confirm().Error));
                Assert.That(view.Session.Map.Commands.IsEditing, Is.True);
                AssertSameLayout(view.Session.Map, harness.Map);
                Assert.That(view.CancelPlacement().Error, Is.EqualTo(harness.Map.Commands.Cancel().Error));
                Assert.That(view.Session.Map.Commands.IsEditing, Is.False);
                Assert.That(uiThing.Id, Is.EqualTo(uiId));
                Assert.That(uiThing.CreationOrder, Is.EqualTo(uiOrder));
                Assert.That(view.Session.Resource.HolderId, Is.EqualTo(uiId));
                Assert.That(view.Session.Resource.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(5));
                Assert.That(EditorJsonUtility.ToJson(storage), Is.EqualTo(definitionBefore));
                Assert.That(view.Session.Creatures.Select(t => t.Placement.Origin).Distinct().Count(), Is.EqualTo(1));

                var beforeRepaint = view.RepaintCount;
                deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (view.RepaintCount <= beforeRepaint && Time.realtimeSinceStartupAsDouble < deadline)
                    yield return null;
                Assert.That(view.RepaintCount, Is.GreaterThan(beforeRepaint), "Actual OnGUI repaint must occur.");
                var screenshot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/MapFoundation-verification.png"));
                if (File.Exists(screenshot)) File.Delete(screenshot);
                ScreenCapture.CaptureScreenshot(screenshot);
                deadline = Time.realtimeSinceStartupAsDouble + 10;
                while ((!File.Exists(screenshot) || new FileInfo(screenshot).Length == 0)
                    && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Assert.That(File.Exists(screenshot), Is.True, "Game View screenshot must be written.");
                Assert.That(new FileInfo(screenshot).Length, Is.GreaterThan(0));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Application.runInBackground = previousBackground;
            }
        }
    }
}
#endif
