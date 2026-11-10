#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator ApprovedArtFollowsRealWorkCargoPauseRestoreAndSceneLifetime()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View;
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(ArtImage(input.Map, worker, "Body").sprite.name, Is.EqualTo("PrototypeSupport_Idle_Down_01"));
                Assert.That(ArtImage(input.Map, worker, "Cargo").gameObject.activeSelf, Is.False, "Initial inventory is not a carried shipment.");
                Assert.That(view.BeginGatheringConstruction(), Is.True); Place(view, 3, 3);
                Assert.That(view.BeginStorageConstruction(), Is.True); Place(view, 8, 3);
                yield return null; yield return null;
                var gathering = view.Simulation.Registry.Things.Single(t => t.Definition is FacilityData d && d.Role == FacilityRole.Gathering);
                var store = view.Simulation.Registry.Things.Single(t => t.Definition is FacilityData d && d.Role == FacilityRole.Storage);
                var observed = new HashSet<string>(); var gatheringEvents = 0; var previousComplete = false; var completeEntries = 0;
                view.Simulation.GatheringCompleted += id => { if (id == gathering.Id) gatheringEvents++; };
                var carried = false;
                for (var tick = 0; tick < 1200; tick++)
                {
                    view.AdvanceFrame(.1, 0); yield return null;
                    var body = ArtImage(input.Map, worker, "Body"); var cargo = ArtImage(input.Map, worker, "Cargo");
                    observed.Add(body.sprite.name.Split('_')[1]);
                    var facility = ArtImage(input.Map, gathering, "Body");
                    observed.Add(facility.sprite.name.Split('_')[1]);
                    var completing = facility.sprite.name.StartsWith("Gathering_Complete_");
                    if (completing && !previousComplete) completeEntries++;
                    previousComplete = completing;
                    Assert.That(completeEntries, Is.LessThanOrEqualTo(gatheringEvents), "Only an actual output event may start Complete.");
                    Assert.That(body.color, Is.EqualTo(Color.white)); Assert.That(facility.color, Is.EqualTo(Color.white));
                    Assert.That(body.rectTransform.localRotation, Is.EqualTo(Quaternion.identity));
                    if (cargo.gameObject.activeSelf)
                    {
                        var job = worker.GetComponent<WorkerComponent>().Job;
                        Assert.That(job.ResourceId.HasValue, Is.True);
                        Assert.That(view.Simulation.Registry.TryGet(job.ResourceId.Value, out var resource), Is.True);
                        Assert.That(resource.HolderId, Is.EqualTo(worker.Id));
                        Assert.That(cargo.sprite, Is.SameAs(resource.Definition.Assets.Icon));
                        var frames = Enumerable.Range(0,64).Select(view.UiAssets.Art.PrototypeFrame).ToArray();
                        var head = view.UiAssets.Art.PrototypeHead(Array.IndexOf(frames, body.sprite));
                        var pixel = body.rectTransform.rect.width / 64;
                        var resourceSupport = cargo.rectTransform.anchoredPosition + new Vector2(2, -12) * pixel;
                        Assert.That(Vector2.Distance(resourceSupport, new Vector2(head.x - 32, 54 - head.y) * pixel), Is.LessThan(.001f));
                        Assert.That(cargo.rectTransform.localRotation, Is.EqualTo(Quaternion.identity));
                        Assert.That(cargo.rectTransform.localScale, Is.EqualTo(Vector3.one));
                        if (job.Kind == WorkKind.Hauling && gatheringEvents > 0)
                        {
                            carried = true; yield return CaptureIntegratedArt("actual-cargo");
                            break;
                        }
                    }
                }
                Assert.That(carried, Is.True, "A real gathering batch must be picked up and displayed.");
                Assert.That(observed, Does.Contain("Idle").And.Contain("Move").And.Contain("Work").And.Contain("Working").And.Contain("Complete"));
                var frozenBody = ArtImage(input.Map, worker, "Body").sprite;
                var frozenBuilding = ArtImage(input.Map, gathering, "Body").sprite;
                var frozenCargo = ArtImage(input.Map, worker, "Cargo").rectTransform.anchoredPosition
                    / ArtImage(input.Map, worker, "Body").rectTransform.rect.width;
                var time = view.Simulation.ElapsedSeconds;
                Assert.That(view.BeginStorageConstruction(), Is.True);
                for (var i = 0; i < 8; i++) { view.AdvanceFrame(.5, 0); yield return null; }
                Assert.That(view.Simulation.ElapsedSeconds, Is.EqualTo(time));
                Assert.That(ArtImage(input.Map, worker, "Body").sprite, Is.SameAs(frozenBody));
                Assert.That(ArtImage(input.Map, gathering, "Body").sprite, Is.SameAs(frozenBuilding));
                Assert.That(Vector2.Distance(ArtImage(input.Map, worker, "Cargo").rectTransform.anchoredPosition
                    / ArtImage(input.Map, worker, "Body").rectTransform.rect.width, frozenCargo), Is.LessThan(.0001f),
                    "Opening placement tools may resize the viewport, but must preserve the source-pixel attachment.");
                Assert.That(view.CancelPlacement(), Is.True);
                Assert.That(view.Save(), Is.True);
                var json = File.ReadAllText(view.SavePath); var workerId = worker.Id;
                yield return input.Reopen(); view = input.View;
                Assert.That(File.ReadAllText(view.SavePath), Is.EqualTo(json));
                Assert.That(view.Simulation.Registry.TryGet(workerId, out worker), Is.True);
                Assert.That(ArtImage(input.Map, worker, "Cargo").gameObject.activeSelf, Is.True, "Restored cargo follows actual ownership, not a stale display cache.");
                Assert.That(input.Map.transform.Find("LaboratoryArt").childCount, Is.EqualTo(2), "Only Gathering and Prototype have supplied map art; storage retains its existing display.");
                yield return AdvanceArtUntil(view, () => worker.GetComponent<WorkerComponent>().Job?.Kind != WorkKind.Hauling, 40);
                yield return null;
                Assert.That(ArtImage(input.Map, worker, "Cargo").gameObject.activeSelf, Is.False, "Delivery must clear the overlay.");
                Assert.That(view.Restart(), Is.True); yield return null; yield return null;
                Assert.That(input.Map.transform.Find("LaboratoryArt").childCount, Is.EqualTo(1));
                Assert.That(input.Map.GetComponentsInChildren<Image>().Any(i => i.name == "Cargo" && i.gameObject.activeSelf), Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator ApprovedArtFacesRotatedWorkAndCapturesBothLayoutsWithoutChangingSave()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View;
                var expected = new[] { "Up", "Right", "Down", "Left" };
                for (var rotation = 0; rotation < 4; rotation++)
                {
                    if (rotation > 0) { Assert.That(view.Restart(), Is.True); yield return null; yield return null; }
                    Assert.That(view.BeginGatheringConstruction(), Is.True);
                    for (var r = 0; r < rotation; r++) Assert.That(view.RotatePreview(), Is.True);
                    Place(view, 6, 4);
                    var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                    var gathering = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Facility);
                    yield return AdvanceArtUntil(view, () => worker.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Gathering
                        && worker.GetComponent<WorkerComponent>().Job.Stage == JobStage.Working, 50);
                    yield return null;
                    Assert.That(ArtImage(input.Map, worker, "Body").sprite.name, Does.StartWith("PrototypeSupport_Work_" + expected[rotation] + "_"));
                    var distinct = new HashSet<Sprite>();
                    for (var frame = 0; frame < 24; frame++)
                    { view.AdvanceFrame(.06, 0); yield return null; distinct.Add(ArtImage(input.Map, worker, "Body").sprite); }
                    Assert.That(distinct.Count, Is.EqualTo(6), "Every Work frame must appear at the supplied timing.");
                    Assert.That(view.SelectThing(gathering.Id), Is.True);
                    input.Map.Fit(); input.Map.ZoomBy(2); yield return null; yield return null;
                    yield return CaptureIntegratedArt("work-rotation-" + rotation + "-" + expected[rotation]);
                    if (rotation == 0)
                    {
                        var codec = new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                        var before = codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase));
                        var random = UnityEngine.Random.state;
                        foreach (var size in new[] { new Vector2Int(1280,720), new Vector2Int(720,1280) })
                        using (new LaboratoryGameViewSize(size.x, size.y))
                        {
                            yield return SettleScreenStateSize(size);
                            foreach (var zoom in new[] { 1, 3, 8 })
                            {
                                input.Map.Fit(); input.Map.ZoomBy(zoom); yield return null; yield return null;
                                yield return CaptureIntegratedArt("mixed-" + size.x + "x" + size.y + "-zoom" + zoom);
                                var build = view.FindUiButton("NavBuild").GetComponentsInChildren<Image>().Single(i => i.sprite == view.UiAssets.GetIcon(LaboratoryIcon.Build));
                                Assert.That(build.color, Is.EqualTo(Color.white)); Assert.That(build.preserveAspect && !build.raycastTarget, Is.True);
                                Assert.That(build.rectTransform.sizeDelta, Is.EqualTo(new Vector2(22,22)));
                                Assert.That(view.UiCanvas.GetComponentsInChildren<Image>().Single(i => i.name == "OrganicMatterQuantityIcon").sprite,
                                    Is.SameAs(view.Simulation.Organic.Assets.Icon));
                            }
                        }
                        Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)), Is.EqualTo(before));
                        Assert.That(UnityEngine.Random.state, Is.EqualTo(random));
                        var assets = view.UiAssets;
                        try
                        {
                            view.ConfigureUiAssets(new LaboratoryUiAssets(assets.Font, Enum.GetValues(typeof(LaboratoryIcon)).Cast<LaboratoryIcon>().Select(assets.GetIcon).ToArray(),
                                assets.FontLicense, assets.ReleaseSound, new[] { assets.GetFloorTile(0) }, assets.Art));
                            input.Map.Fit(); yield return null; yield return null;
                            yield return CaptureIntegratedArt("base-only-1280x720-zoom1");
                        }
                        finally { view.ConfigureUiAssets(assets); }
                    }
                    // The already-produced batch remains outstanding, but Complete must finish once.
                    yield return AdvanceArtUntil(view, () => gathering.GetComponent<GatheringComponent>().Outstanding > 0, 10);
                    for (var i = 0; i < 30; i++) { view.AdvanceFrame(.1, 0); yield return null; }
                    Assert.That(ArtImage(input.Map, gathering, "Body").sprite.name, Does.StartWith("Gathering_Idle_"));
                }
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator ApprovedArtPlaysAllTwelveTagsAndOneCompletionPerBatch()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View;
                var names = new[] { "Up", "Right", "Down", "Left" };
                var axes = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
                for (var rotation = 0; rotation < 4; rotation++)
                {
                    if (rotation > 0) { Assert.That(view.Restart(), Is.True); yield return null; yield return null; }
                    var facility = CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(6,4), (QuarterTurn)rotation, true);
                    var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                    var destination = facility.Placement.InteractionCell.Value;
                    Assert.That(view.Simulation.Map.Commands.Place(worker.Id, destination - axes[rotation] * 3).Succeeded, Is.True);
                    Assert.That(view.SetWorkerPriorities(worker.Id, new[] { 0, 1, 0, 0 }), Is.True);
                    view.Simulation.OnMapChanged(); yield return null; yield return null;
                    var moves = new HashSet<Sprite>();
                    for (var i = 0; i < 40 && worker.GetComponent<WorkerComponent>().Job?.Stage != JobStage.Working; i++)
                    {
                        view.AdvanceFrame(.05,0); yield return null; yield return null;
                        var sprite = ArtImage(input.Map,worker,"Body").sprite;
                        if (sprite.name.Contains("_Move_"))
                        { Assert.That(sprite.name, Does.StartWith("PrototypeSupport_Move_" + names[rotation] + "_")); moves.Add(sprite); }
                    }
                    Assert.That(moves.Count, Is.EqualTo(6), "All six movement frames must be sampled along the actual path.");
                    Assert.That(ArtImage(input.Map,worker,"Body").sprite.name, Is.EqualTo("PrototypeSupport_Work_" + names[rotation] + "_01"));
                    var works = new HashSet<Sprite>(); var working = new HashSet<Sprite>();
                    input.Map.Fit(); input.Map.ZoomBy(1.5f); yield return null;
                    for (var i = 0; i < 24; i++)
                    {
                        view.AdvanceFrame(.06,0); yield return null; yield return null;
                        works.Add(ArtImage(input.Map,worker,"Body").sprite); working.Add(ArtImage(input.Map,facility,"Body").sprite);
                        if (rotation == 0 && i % 2 == 0) yield return CaptureIntegratedArt("animation/working/frame-" + (i/2).ToString("000"));
                    }
                    Assert.That(works.Count, Is.EqualTo(6)); Assert.That(working.Count, Is.EqualTo(12));
                    yield return AdvanceArtUntil(view, () => facility.GetComponent<GatheringComponent>().Outstanding > 0, 10);
                    var idle = new HashSet<Sprite>(); var completed = new HashSet<Sprite>();
                    for (var i = 0; i < 24; i++)
                    {
                        var body = ArtImage(input.Map,worker,"Body").sprite;
                        Assert.That(body.name, Does.StartWith("PrototypeSupport_Idle_" + names[rotation] + "_")); idle.Add(body);
                        var sprite = ArtImage(input.Map,facility,"Body").sprite;
                        if (sprite.name.Contains("_Complete_")) completed.Add(sprite);
                        if (rotation == 0 && i % 2 == 0) yield return CaptureIntegratedArt("animation/complete/frame-" + (i/2).ToString("000"));
                        view.AdvanceFrame(.05,0); yield return null; yield return null;
                    }
                    Assert.That(idle.Count, Is.EqualTo(4)); Assert.That(completed.Count, Is.EqualTo(12));
                    Assert.That(ArtImage(input.Map,facility,"Body").sprite.name, Does.StartWith("Gathering_Idle_"));
                    var inventory = view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                        .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity);
                    for (var i = 0; i < 26; i++)
                    {
                        view.AdvanceFrame(.1,0); yield return null;
                        Assert.That(ArtImage(input.Map,facility,"Body").sprite.name, Does.StartWith("Gathering_Idle_"));
                    }
                    Assert.That(view.Simulation.Registry.Things.Where(t => t.Definition.Kind == ThingKind.Resource)
                        .Sum(t => t.GetComponent<ResourceStackComponent>().Quantity), Is.EqualTo(inventory), "Replaying art must never create output.");
                }
            }
        }

        [UnityTest]
        public IEnumerator ApprovedArtKeepsOnlyRemainingCargoAfterPartialAdmissionAndHandlesMissingArt()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View;
                var facility = CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(3,3), QuarterTurn.None, true);
                var store = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(8,3), QuarterTurn.None, true);
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                // This fixture starts with already-built facilities: spend the bootstrap materials
                // that their normal construction would have consumed before testing a five-unit load.
                foreach (var initial in worker.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                    view.Simulation.Registry.ConsumeResource(initial, initial.GetComponent<ResourceStackComponent>().Quantity);
                view.Simulation.OnMapChanged(); yield return null;
                yield return AdvanceArtUntil(view, () => worker.GetComponent<WorkerComponent>().Job?.Kind == WorkKind.Hauling
                    && worker.GetComponent<WorkerComponent>().Job.Stage == JobStage.ToDestination, 40);
                var cargoId = worker.GetComponent<WorkerComponent>().Job.ResourceId.Value;
                var stock = view.Simulation.Registry.CreateResource(view.Simulation.Organic, ((FacilityData)store.Definition).StorageCapacity - 2,
                    store.GetComponent<ResourceContainerComponent>());
                view.Simulation.OnMapChanged();
                yield return AdvanceArtUntil(view, () => worker.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Waiting
                    && view.Simulation.Registry.TryGet(cargoId, out var remainder) && remainder.GetComponent<ResourceStackComponent>().Quantity == 3, 40);
                Assert.That(view.Simulation.Registry.TryGet(cargoId, out var held), Is.True);
                Assert.That(held.HolderId, Is.EqualTo(worker.Id));
                Assert.That(held.GetComponent<ResourceStackComponent>().Quantity, Is.EqualTo(3));
                Assert.That(ArtImage(input.Map,worker,"Cargo").gameObject.activeSelf, Is.True);
                var hud = view.UiCanvas.GetComponentsInChildren<Image>().Single(i => i.name == "OrganicMatterQuantityIcon");
                Assert.That(ArtImage(input.Map,worker,"Cargo").sprite, Is.SameAs(hud.sprite));
                input.Map.Fit(); input.Map.ZoomBy(2); yield return null; yield return null;
                yield return CaptureIntegratedArt("partial-admission-waiting-cargo");
                Assert.That(view.Save(), Is.True); Assert.That(view.Reload(), Is.True); yield return null; yield return null;
                view.Simulation.Registry.TryGet(worker.Id, out worker); view.Simulation.Registry.TryGet(stock.Id, out stock);
                Assert.That(ArtImage(input.Map,worker,"Cargo").gameObject.activeSelf, Is.True);
                var assets = view.UiAssets;
                try
                {
                    view.ConfigureUiAssets(new LaboratoryUiAssets(assets.Font, Enum.GetValues(typeof(LaboratoryIcon)).Cast<LaboratoryIcon>().Select(assets.GetIcon).ToArray(),
                        assets.FontLicense, assets.ReleaseSound, Enumerable.Range(0,4).Select(assets.GetFloorTile).ToArray()));
                    yield return null; yield return null;
                    Assert.That(input.Map.GetComponentsInChildren<TextMeshProUGUI>().Any(t => t.name == "Temp_CellLabel" && t.text.StartsWith("P")), Is.True);
                    yield return input.Tap(93, input.Map.CellScreenPoint(worker.Placement.Origin));
                    Assert.That(view.SelectedId, Is.EqualTo(worker.Id), "Missing art must preserve selection.");
                }
                finally { view.ConfigureUiAssets(assets); }
                yield return null; yield return null;
                view.Simulation.Registry.ConsumeResource(stock, 3); view.Simulation.OnMapChanged();
                yield return AdvanceArtUntil(view, () => !view.Simulation.Registry.TryGet(cargoId, out var remaining) || remaining.HolderId != worker.Id, 40);
                yield return null;
                Assert.That(ArtImage(input.Map,worker,"Cargo").gameObject.activeSelf, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest]
        public IEnumerator ApprovedArtKeepsMovePhaseAtPathCornerAndIndependentInstances()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View;
                CreateStatusLayoutFacility(view, "gatheringId", new Vector2Int(6,4), QuarterTurn.None, true);
                var worker = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(view.Simulation.Map.Commands.Place(worker.Id, new Vector2Int(4,1)).Succeeded, Is.True);
                view.Simulation.OnMapChanged(); yield return null; yield return null;
                string previousDirection = null; var previousFrame = 0; var sawTurn = false;
                for (var i = 0; i < 30 && worker.GetComponent<WorkerComponent>().Job?.Stage != JobStage.Working; i++)
                {
                    view.AdvanceFrame(.05,0); yield return null; yield return null;
                    var parts = ArtImage(input.Map,worker,"Body").sprite.name.Split('_');
                    if (parts[1] != "Move") continue;
                    var frame = int.Parse(parts[3]);
                    if (previousDirection != null && previousDirection != parts[2])
                    {
                        sawTurn = true;
                        Assert.That((frame - previousFrame + 6) % 6, Is.InRange(0,1), "A direction change must keep the current Move phase.");
                        Assert.That(frame, Is.Not.EqualTo(1), "This prepared turn is halfway through the cycle, not at the loop boundary.");
                    }
                    previousDirection = parts[2]; previousFrame = frame;
                }
                Assert.That(sawTurn, Is.True);
                yield return AdvanceArtUntil(view, () => worker.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Working, 3);
                view.AdvanceFrame(.25,0); yield return null; yield return null;
                var firstFrame = ArtImage(input.Map,worker,"Body").sprite;
                var second = view.Simulation.Registry.Create(worker.Definition);
                Assert.That(view.Simulation.Map.Commands.Place(second.Id, new Vector2Int(12,7)).Succeeded, Is.True);
                yield return null; yield return null;
                Assert.That(ArtImage(input.Map,second,"Body").sprite.name, Is.EqualTo("PrototypeSupport_Idle_Down_01"));
                Assert.That(ArtImage(input.Map,worker,"Body").sprite, Is.SameAs(firstFrame), "Creating another instance cannot reset existing playback.");
                Assert.That(ArtImage(input.Map,second,"Body").transform.parent, Is.Not.SameAs(ArtImage(input.Map,worker,"Body").transform.parent));
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static Image ArtImage(LaboratoryMapView map, Thing thing, string name)
            => map.transform.Find("LaboratoryArt/ThingArt_" + thing.Id.ToString("N") + "/" + name).GetComponent<Image>();
        private static IEnumerator AdvanceArtUntil(LaboratoryView view, Func<bool> condition, double maximum)
        {
            for (var i = 0; i < maximum * 10 && !condition(); i++) { view.AdvanceFrame(.1, 0); yield return null; yield return null; }
            Assert.That(condition(), Is.True, "Art scenario exceeded " + maximum + " simulated seconds. "
                + string.Join("; ", view.Simulation.Registry.Things.Where(t => t.TryGetComponent<WorkerComponent>(out _)).Select(t =>
                    t.GetComponent<WorkerComponent>().Job?.Kind + "/" + t.GetComponent<WorkerComponent>().Job?.Stage + " " + t.GetComponent<WorkerComponent>().WaitReason)));
        }
        private static IEnumerator CaptureIntegratedArt(string name)
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/plans/evidence/PLAN-026/integration-2026-11-10/screens/" + name + ".png"));
            yield return AppSceneFlowTests.Capture("PLAN026-art", path);
        }
    }
}
#endif
