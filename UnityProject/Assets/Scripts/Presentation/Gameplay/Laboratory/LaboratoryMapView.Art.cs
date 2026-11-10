using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private enum ArtMotion { Idle, Move, Work, Complete }
        private sealed class ThingArt
        {
            public RectTransform Root;
            public Image Body, Cargo;
            public Image[] Border;
            public TextMeshProUGUI Count;
            public ArtMotion Motion;
            public int Direction, Frame;
            public double Time;
            public Vector2Int Cell;
            public bool Initialized;
        }

        private readonly Dictionary<Guid, ThingArt> thingArt = new Dictionary<Guid, ThingArt>();
        private readonly Dictionary<Guid, double> gatheringCompletions = new Dictionary<Guid, double>();
        private LaboratoryManager artSimulation;
        private RectTransform artLayer;
        private ThingArt previewArt;
        private double artClock;

        private void BindArtSimulation()
        {
            if (ReferenceEquals(artSimulation, view.Simulation)) return;
            if (artSimulation != null) artSimulation.GatheringCompleted -= OnGatheringCompleted;
            foreach (var item in thingArt.Values) Destroy(item.Root.gameObject);
            thingArt.Clear(); gatheringCompletions.Clear();
            artSimulation = view.Simulation;
            artClock = artSimulation?.ElapsedSeconds ?? 0;
            if (artSimulation != null) artSimulation.GatheringCompleted += OnGatheringCompleted;
        }

        private void OnGatheringCompleted(Guid id) => gatheringCompletions[id] = artSimulation.ElapsedSeconds;
        private void OnDestroy()
        {
            if (artSimulation != null) artSimulation.GatheringCompleted -= OnGatheringCompleted;
            artSimulation = null;
        }

        private bool HasSuppliedArt(Thing thing) => view.UiAssets?.Art != null
            && (view.UiAssets.Art.HasGathering(thing.Definition) || view.UiAssets.Art.HasPrototype(thing.Definition));

        private void DrawArt(FacilityData previewDefinition, Vector2Int? previewOrigin, QuarterTurn rotation, bool valid)
        {
            BindArtSimulation();
            if (artLayer == null)
            {
                artLayer = UiWidgets.Rect("LaboratoryArt", viewport);
                artLayer.anchorMin = Vector2.zero; artLayer.anchorMax = Vector2.one;
                artLayer.offsetMin = artLayer.offsetMax = Vector2.zero;
            }
            artLayer.SetAsLastSibling();
            var assets = view.UiAssets?.Art;
            var delta = Math.Max(0, artSimulation.ElapsedSeconds - artClock);
            artClock = artSimulation.ElapsedSeconds;
            foreach (var id in thingArt.Keys.ToArray())
                if (!artSimulation.Registry.TryGet(id, out var current) || !current.IsPlaced || !HasSuppliedArt(current))
                { Destroy(thingArt[id].Root.gameObject); thingArt.Remove(id); gatheringCompletions.Remove(id); }
            var placed = artSimulation.Registry.Things.Where(t => t.IsPlaced).ToArray();
            foreach (var thing in placed.Where(HasSuppliedArt).OrderByDescending(t => t.Placement.Origin.y).ThenBy(t => t.CreationOrder))
            {
                if (!thingArt.TryGetValue(thing.Id, out var item))
                    thingArt.Add(thing.Id, item = CreateThingArt("ThingArt_" + thing.Id.ToString("N")));
                item.Root.SetAsLastSibling();
                var facility = thing.Definition as FacilityData;
                var size = facility == null ? Vector2.one : (Vector2)FacilityGeometry.GetSize(facility, thing.Placement.Rotation);
                item.Root.anchoredPosition = Origin - viewport.rect.min + ((Vector2)thing.Placement.Origin + size * .5f) * CellSize;
                item.Root.sizeDelta = size * CellSize;
                if (facility != null) DrawGathering(thing, item, assets, delta);
                else DrawPrototype(thing, item, assets, delta);
                DrawBorder(item, thing.Id == view.SelectedId, new Color(1, .88f, .35f));
                var count = facility == null ? placed.Count(t => t.Definition.Kind == ThingKind.Creature && t.Placement.Origin == thing.Placement.Origin) : 0;
                item.Count.gameObject.SetActive(count > 1);
                if (count > 1) item.Count.text = count.ToString();
                item.Initialized = true; item.Cell = thing.Placement.Origin;
            }
            foreach (var id in gatheringCompletions.Keys.Where(id => artClock - gatheringCompletions[id] >= 1.2).ToArray()) gatheringCompletions.Remove(id);
            DrawArtPreview(assets, previewDefinition, previewOrigin, rotation, valid);
        }

        private void DrawGathering(Thing thing, ThingArt item, LaboratoryArtAssets assets, double delta)
        {
            if (!item.Initialized || delta > 0)
            {
                var complete = gatheringCompletions.TryGetValue(thing.Id, out var at) && artClock - at < 1.2;
                var state = complete ? ArtMotion.Complete : view.StatusFor(thing).Kind == FacilityStatusKind.GatheringWorking ? ArtMotion.Work : ArtMotion.Idle;
                AdvanceArt(item, state, delta);
                if (complete) item.Time = Math.Max(0, artClock - at);
                item.Frame = (state == ArtMotion.Work ? 12 : state == ArtMotion.Complete ? 24 : 0)
                    + (state == ArtMotion.Complete ? Math.Min(11, (int)(item.Time / .1)) : (int)(item.Time / (state == ArtMotion.Idle ? .2 : .1)) % 12);
            }
            item.Body.sprite = assets.GatheringFrame(item.Frame) ?? assets.GatheringFrame(0);
            item.Body.rectTransform.anchoredPosition = Vector2.zero;
            item.Body.rectTransform.sizeDelta = Vector2.one * (2 * CellSize);
            item.Body.rectTransform.localRotation = Quaternion.Euler(0, 0, -90 * (int)thing.Placement.Rotation);
            item.Cargo.gameObject.SetActive(false);
        }

        private void DrawPrototype(Thing thing, ThingArt item, LaboratoryArtAssets assets, double delta)
        {
            var worker = thing.GetComponent<WorkerComponent>();
            if (!item.Initialized || delta > 0)
            {
                var state = ArtMotion.Idle; Vector2Int? direction = null;
                var nextCell = worker.NextMoveCell;
                if (!item.Initialized && !nextCell.HasValue && worker.MoveProgress > 0 && worker.WaitReason == WorkerWaitReason.None
                    && worker.Job != null && (worker.Job.Stage == JobStage.ToSupply || worker.Job.Stage == JobStage.ToDestination))
                {
                    // Route caches are intentionally absent from saves. Reconstruct this first display
                    // with the same pathfinder without changing the worker, job or simulation position.
                    var targetId = worker.Job.Stage == JobStage.ToSupply ? worker.Job.SourceId : worker.Job.TargetId;
                    if (targetId.HasValue && artSimulation.Registry.TryGet(targetId.Value, out var destination))
                    {
                        var goal = artSimulation.TargetCell(destination);
                        var route = goal.HasValue ? GridPathfinder.FindPath(artSimulation.Map, thing.Placement.Origin, goal.Value) : null;
                        if (route != null && route.Count > 0) nextCell = route[0];
                    }
                }
                if (nextCell.HasValue)
                { state = ArtMotion.Move; direction = nextCell.Value - thing.Placement.Origin; }
                else if (worker.Job != null && worker.Job.Stage == JobStage.Working && worker.WaitReason == WorkerWaitReason.None
                    && (worker.Job.Kind == WorkKind.Construction || worker.Job.Kind == WorkKind.Gathering)
                    && artSimulation.Registry.TryGet(worker.Job.TargetId, out var target)
                    && target.IsPlaced && thing.Placement.Origin == artSimulation.TargetCell(target))
                {
                    state = ArtMotion.Work;
                    // Face the closest occupied cell, not the facility's distant origin.
                    var bounds = FacilityGeometry.GetSize((FacilityData)target.Definition, target.Placement.Rotation);
                    var cell = thing.Placement.Origin; var origin = target.Placement.Origin;
                    direction = new Vector2Int(Mathf.Clamp(cell.x, origin.x, origin.x + bounds.x - 1),
                        Mathf.Clamp(cell.y, origin.y, origin.y + bounds.y - 1)) - cell;
                }
                else if (item.Initialized && item.Cell != thing.Placement.Origin)
                { state = ArtMotion.Move; direction = thing.Placement.Origin - item.Cell; }
                if (direction.HasValue && direction.Value != Vector2Int.zero) item.Direction = ArtDirection(direction.Value);
                AdvanceArt(item, state, delta);
                var count = state == ArtMotion.Idle ? 4 : 6;
                var seconds = state == ArtMotion.Idle ? .25 : state == ArtMotion.Move ? .1 : .12;
                item.Frame = item.Direction * 16 + (state == ArtMotion.Idle ? 0 : state == ArtMotion.Move ? 4 : 10) + (int)(item.Time / seconds) % count;
            }
            item.Body.sprite = assets.PrototypeFrame(item.Frame) ?? assets.PrototypeFrame(0);
            var pixel = CellSize / 64;
            // Common (32,54) ground point stays at the logical cell center; pivot remains centered.
            item.Body.rectTransform.anchoredPosition = new Vector2(0, 22) * pixel;
            item.Body.rectTransform.sizeDelta = Vector2.one * CellSize;
            item.Body.rectTransform.localRotation = Quaternion.identity;
            var cargo = CarriedResource(thing, worker);
            item.Cargo.gameObject.SetActive(cargo?.Definition.Assets.Icon != null);
            if (cargo != null)
            {
                item.Cargo.sprite = cargo.Definition.Assets.Icon;
                var head = assets.PrototypeHead(item.Frame);
                // OrganicMatter bottom support is (18,28), so its center is head + (-2,+12).
                item.Cargo.rectTransform.anchoredPosition = new Vector2(head.x - 32 - 2, 54 - head.y + 12) * pixel;
                item.Cargo.rectTransform.sizeDelta = Vector2.one * (32 * pixel);
            }
        }

        private Thing CarriedResource(Thing thing, WorkerComponent worker)
        {
            var job = worker.Job;
            if (job?.ResourceId == null || job.Amount <= 0 || job.Stage == JobStage.ToSupply
                || !artSimulation.Registry.TryGet(job.ResourceId.Value, out var resource)
                || resource.HolderId != thing.Id || resource.Definition != artSimulation.Organic
                || resource.GetComponent<ResourceStackComponent>().Quantity <= 0) return null;
            var pickedUp = artSimulation.Reservations.Any(r => r.WorkerId == thing.Id && r.ResourceId == resource.Id && r.PickedUp);
            var waiting = job.Kind == WorkKind.Hauling && job.Stage == JobStage.Waiting;
            return pickedUp || waiting ? resource : null;
        }

        private static int ArtDirection(Vector2Int direction) => Math.Abs(direction.x) > Math.Abs(direction.y)
            ? direction.x < 0 ? 1 : 2 : direction.y > 0 ? 3 : 0;
        private static void AdvanceArt(ThingArt item, ArtMotion motion, double delta)
        {
            if (!item.Initialized || item.Motion != motion) { item.Motion = motion; item.Time = 0; }
            else item.Time += delta;
        }

        private void DrawArtPreview(LaboratoryArtAssets assets, FacilityData definition, Vector2Int? origin, QuarterTurn rotation, bool valid)
        {
            var visible = assets != null && definition != null && origin.HasValue && assets.HasGathering(definition);
            if (!visible) { if (previewArt != null) previewArt.Root.gameObject.SetActive(false); return; }
            if (previewArt == null) previewArt = CreateThingArt("GatheringPreview");
            previewArt.Root.gameObject.SetActive(true); previewArt.Root.SetAsLastSibling();
            var size = (Vector2)FacilityGeometry.GetSize(definition, rotation);
            previewArt.Root.anchoredPosition = Origin - viewport.rect.min + ((Vector2)origin.Value + size * .5f) * CellSize;
            previewArt.Root.sizeDelta = size * CellSize;
            previewArt.Body.sprite = assets.GatheringFrame(0);
            previewArt.Body.rectTransform.sizeDelta = Vector2.one * (2 * CellSize);
            previewArt.Body.rectTransform.localRotation = Quaternion.Euler(0, 0, -90 * (int)rotation);
            previewArt.Body.color = new Color(1, 1, 1, .7f);
            previewArt.Cargo.gameObject.SetActive(false); previewArt.Count.gameObject.SetActive(false);
            DrawBorder(previewArt, true, valid ? new Color(.2f, 1, .5f) : new Color(1, .25f, .25f));
        }

        private ThingArt CreateThingArt(string name)
        {
            var root = UiWidgets.Rect(name, artLayer); root.anchorMin = root.anchorMax = Vector2.zero; root.pivot = Vector2.one * .5f;
            var item = new ThingArt { Root = root, Body = CreateArtImage("Body", root), Cargo = CreateArtImage("Cargo", root), Border = new Image[4] };
            for (var i = 0; i < 4; i++) item.Border[i] = CreateArtImage("Selection_" + i, root);
            item.Count = UiWidgets.Label("OverlapCount", root, 14); item.Count.font = font;
            item.Count.alignment = TextAlignmentOptions.TopRight; item.Count.raycastTarget = false;
            var label = item.Count.rectTransform; label.anchorMin = Vector2.zero; label.anchorMax = Vector2.one; label.offsetMin = label.offsetMax = Vector2.zero;
            return item;
        }

        private static Image CreateArtImage(string name, RectTransform parent)
        {
            var image = UiWidgets.Panel(name, parent, Color.white); image.raycastTarget = false; image.preserveAspect = true;
            var rect = image.rectTransform; rect.anchorMin = rect.anchorMax = Vector2.one * .5f; rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = Vector2.zero;
            return image;
        }

        private static void DrawBorder(ThingArt item, bool visible, Color color)
        {
            var size = item.Root.sizeDelta; const float stroke = 2;
            for (var i = 0; i < 4; i++)
            {
                var image = item.Border[i]; image.gameObject.SetActive(visible); if (!visible) continue;
                image.color = color;
                image.rectTransform.sizeDelta = i < 2 ? new Vector2(size.x, stroke) : new Vector2(stroke, size.y);
                image.rectTransform.anchoredPosition = i < 2 ? new Vector2(0, (i == 0 ? -1 : 1) * (size.y - stroke) * .5f)
                    : new Vector2((i == 2 ? -1 : 1) * (size.x - stroke) * .5f, 0);
            }
        }

        private void ReserveCreatureArtBounds()
        {
            foreach (var pair in thingArt)
            {
                if (!artSimulation.Registry.TryGet(pair.Key, out var thing) || thing.Definition.Kind != ThingKind.Creature) continue;
                var item = pair.Value;
                var body = item.Body.rectTransform;
                occupiedStatusBounds.Add(new Rect(viewport.rect.min + item.Root.anchoredPosition + body.anchoredPosition - body.sizeDelta * .5f, body.sizeDelta));
                if (!item.Cargo.gameObject.activeSelf) continue;
                var cargo = item.Cargo.rectTransform;
                occupiedStatusBounds.Add(new Rect(viewport.rect.min + item.Root.anchoredPosition + cargo.anchoredPosition - cargo.sizeDelta * .5f, cargo.sizeDelta));
            }
        }
    }
}
