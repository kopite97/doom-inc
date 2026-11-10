using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private enum ArtMotion { Idle, Move, Work, Complete }
        private sealed class ThingArt
        {
            public Transform Root, CarryAnchor;
            public SortingGroup Group;
            public SpriteRenderer Body, Cargo, Interaction;
            public SpriteRenderer[] Border;
            public TextMeshProUGUI Count;
            public Vector2 Size;
            public string FallbackLabel;
            public int LabelCount = -1;
            public bool LabelSupplied;
            public ArtMotion Motion;
            public int Direction, Frame;
            public double Time;
            public Vector2Int Cell;
            public bool Initialized;
        }
        private readonly Dictionary<Guid, ThingArt> thingArt = new Dictionary<Guid, ThingArt>();
        private readonly Dictionary<Guid, double> gatheringCompletions = new Dictionary<Guid, double>();
        private readonly List<Guid> removedArt = new List<Guid>();
        private readonly List<Thing> placedArt = new List<Thing>();
        private readonly Dictionary<Vector2Int, int> creatureCounts = new Dictionary<Vector2Int, int>();
        private readonly HashSet<Vector2Int> annotatedCreatureCells = new HashSet<Vector2Int>();
        private readonly HashSet<Guid> carriedResources = new HashSet<Guid>();
        private LaboratoryManager artSimulation;
        private RectTransform artLabels;
        private ThingArt previewArt;
        private double artClock;

        private void BindArtSimulation()
        {
            if (ReferenceEquals(artSimulation, view.Simulation)) return;
            if (artSimulation != null) artSimulation.GatheringCompleted -= OnGatheringCompleted;
            foreach (var item in thingArt.Values) RemoveArt(item);
            thingArt.Clear(); gatheringCompletions.Clear();
            if (previewArt != null) { RemoveArt(previewArt); previewArt = null; }
            artSimulation = view.Simulation;
            artClock = artSimulation?.ElapsedSeconds ?? 0;
            if (artSimulation != null) artSimulation.GatheringCompleted += OnGatheringCompleted;
        }
        private void OnGatheringCompleted(Guid id) => gatheringCompletions[id] = artSimulation.ElapsedSeconds;
        private void OnDestroy()
        {
            if (artSimulation != null) artSimulation.GatheringCompleted -= OnGatheringCompleted;
            artSimulation = null;
            world?.Dispose(); world = null;
        }
        private static void RemoveArt(ThingArt item)
        {
            item.Root.gameObject.SetActive(false);
            Destroy(item.Root.gameObject); Destroy(item.Count.gameObject);
        }
        private bool HasSuppliedArt(Thing thing) => view.UiAssets?.Art != null
            && (view.UiAssets.Art.HasGathering(thing.Definition) || view.UiAssets.Art.HasPrototype(thing.Definition));
        private static int CompareArt(Thing a, Thing b)
        {
            var y = b.Placement.Origin.y.CompareTo(a.Placement.Origin.y);
            return y != 0 ? y : a.CreationOrder.CompareTo(b.CreationOrder);
        }
        private void DrawArt(FacilityData previewDefinition, Vector2Int? previewOrigin, QuarterTurn rotation, bool valid)
        {
            BindArtSimulation();
            if (artLabels == null)
            {
                artLabels = UiWidgets.Rect("LaboratoryAnnotations", viewport);
                artLabels.anchorMin = artLabels.anchorMax = artLabels.pivot = Vector2.zero;
            }
            artLabels.sizeDelta = viewport.rect.size;
            var assets = view.UiAssets?.Art;
            var delta = Math.Max(0, artSimulation.ElapsedSeconds - artClock);
            artClock = artSimulation.ElapsedSeconds;
            removedArt.Clear();
            foreach (var pair in thingArt)
                if (!artSimulation.Registry.TryGet(pair.Key, out var current) || !current.IsPlaced)
                    removedArt.Add(pair.Key);
            foreach (var id in removedArt) { RemoveArt(thingArt[id]); thingArt.Remove(id); gatheringCompletions.Remove(id); }
            placedArt.Clear(); creatureCounts.Clear(); carriedResources.Clear(); annotatedCreatureCells.Clear();
            foreach (var reservation in artSimulation.Reservations)
                if (reservation.PickedUp) carriedResources.Add(reservation.ResourceId);
            foreach (var thing in artSimulation.Registry.Things)
            {
                if (!thing.IsPlaced || thing.Definition.Kind == ThingKind.Resource) continue;
                placedArt.Add(thing);
                if (thing.Definition.Kind == ThingKind.Creature)
                {
                    creatureCounts.TryGetValue(thing.Placement.Origin, out var n);
                    creatureCounts[thing.Placement.Origin] = n + 1;
                }
            }
            placedArt.Sort(CompareArt);
            for (var i = 0; i < placedArt.Count; i++)
            {
                var thing = placedArt[i];
                if (!thingArt.TryGetValue(thing.Id, out var item))
                    thingArt.Add(thing.Id, item = CreateThingArt("ThingArt_" + thing.Id.ToString("N")));
                var facility = thing.Definition as FacilityData;
                var size = facility == null ? Vector2.one : (Vector2)FacilityGeometry.GetSize(facility, thing.Placement.Rotation);
                item.Size = size * LaboratoryWorldMap.UnitsPerCell;
                item.Root.position = LaboratoryWorldMap.CellToWorld((Vector2)thing.Placement.Origin + size * .5f);
                item.Body.color = Color.white;
                if (assets != null && assets.HasGathering(thing.Definition)) DrawGathering(thing, item, assets, delta);
                else if (assets != null && assets.HasPrototype(thing.Definition)) DrawPrototype(thing, item, assets, delta);
                else DrawFallback(thing, item);
                DrawBorder(item, thing.Id == view.SelectedId, new Color(1, .88f, .35f));
                DrawInteraction(item, facility, thing.Placement.Origin, thing.Placement.Rotation);
                SetArtOrder(item, i * 4);
                creatureCounts.TryGetValue(thing.Placement.Origin, out var count);
                var supplied = HasSuppliedArt(thing);
                var annotate = facility != null || annotatedCreatureCells.Add(thing.Placement.Origin);
                var showLabel = annotate && (!supplied || (facility == null && count > 1));
                item.Count.gameObject.SetActive(showLabel);
                if (showLabel)
                {
                    if (item.FallbackLabel == null)
                    {
                        var role = facility != null ? facility.Role.ToString() : ((CreatureData)thing.Definition).Role.ToString();
                        item.FallbackLabel = role.Substring(0, 1) + thing.CreationOrder;
                    }
                    if (item.LabelCount != count || item.LabelSupplied != supplied)
                    {
                        item.Count.text = supplied ? count.ToString() : item.FallbackLabel + (facility == null && count > 1 ? "+" : "");
                        item.LabelCount = count; item.LabelSupplied = supplied;
                    }
                    var local = LaboratoryWorldMap.WorldToLocal(item.Root.position, Origin, CellSize) - viewport.rect.min;
                    item.Count.rectTransform.anchoredPosition = local + (supplied ? size * CellSize * .4f : Vector2.zero);
                    item.Count.rectTransform.sizeDelta = size * CellSize;
                    item.Count.fontSize = Mathf.Clamp(CellSize * .25f, 9, 18);
                }
                item.Initialized = true; item.Cell = thing.Placement.Origin;
            }
            removedArt.Clear();
            foreach (var pair in gatheringCompletions) if (artClock - pair.Value >= 1.2) removedArt.Add(pair.Key);
            foreach (var id in removedArt) gatheringCompletions.Remove(id);
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
            item.Body.transform.localPosition = Vector3.zero;
            item.Body.transform.localScale = Vector3.one;
            item.Body.transform.localRotation = Quaternion.Euler(0, 0, -90 * (int)thing.Placement.Rotation);
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
            item.Body.transform.localPosition = new Vector3(0, 22f / 32, 0);
            item.Body.transform.localScale = Vector3.one;
            item.Body.transform.localRotation = Quaternion.identity;
            DrawCargo(thing, item, worker, assets.PrototypeHead(item.Frame));
        }
        private void DrawFallback(Thing thing, ThingArt item)
        {
            item.Body.sprite = world.Solid;
            item.Body.transform.localPosition = Vector3.zero;
            item.Body.transform.localScale = new Vector3(item.Size.x - .06f, item.Size.y - .06f, 1);
            item.Body.transform.localRotation = Quaternion.identity;
            var complete = !thing.TryGetComponent<ConstructionComponent>(out var construction) || construction.IsComplete;
            item.Body.color = thing.Definition.Kind == ThingKind.Facility
                ? complete ? new Color(.18f, .42f, .62f) : new Color(.56f, .43f, .12f) : new Color(.7f, .4f, .15f);
            if (thing.TryGetComponent<WorkerComponent>(out var worker)) DrawCargo(thing, item, worker, new Vector2(32, 22));
            else item.Cargo.gameObject.SetActive(false);
        }
        private void DrawCargo(Thing thing, ThingArt item, WorkerComponent worker, Vector2 head)
        {
            var cargo = CarriedResource(thing, worker);
            item.Cargo.gameObject.SetActive(cargo != null);
            if (cargo == null) { item.Cargo.sprite = null; return; }
            var sprite = cargo.Definition.Assets.Icon;
            item.Cargo.sprite = sprite != null ? sprite : world.Solid;
            item.Cargo.color = sprite != null ? Color.white : new Color(.8f, .8f, .4f);
            item.CarryAnchor.localPosition = new Vector3(head.x - 32, 54 - head.y, 0) / 32;
            var sourceSize = sprite != null ? sprite.rect.size : Vector2.one;
            var extent = Mathf.Max(sourceSize.x, sourceSize.y);
            var support = sprite != null && view.UiAssets.Art != null ? view.UiAssets.Art.CargoSupport(sprite)
                : new Vector2(sourceSize.x * .5f, sourceSize.y);
            item.Cargo.transform.localPosition = new Vector3(sourceSize.x * .5f - support.x, support.y - sourceSize.y * .5f, 0) / extent;
            item.Cargo.transform.localScale = Vector3.one * (sprite != null ? sprite.pixelsPerUnit / extent : 1);
            item.Cargo.transform.localRotation = Quaternion.identity;
        }
        private Thing CarriedResource(Thing thing, WorkerComponent worker)
        {
            var job = worker.Job;
            if (job?.ResourceId == null || job.Amount <= 0 || job.Stage == JobStage.ToSupply
                || !artSimulation.Registry.TryGet(job.ResourceId.Value, out var resource)
                || resource.HolderId != thing.Id || resource.Definition.Kind != ThingKind.Resource
                || resource.GetComponent<ResourceStackComponent>().Quantity <= 0) return null;
            var waiting = job.Kind == WorkKind.Hauling && job.Stage == JobStage.Waiting;
            return carriedResources.Contains(resource.Id) || waiting ? resource : null;
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
            if (definition == null || !origin.HasValue)
            { if (previewArt != null) previewArt.Root.gameObject.SetActive(false); return; }
            if (previewArt == null) previewArt = CreateThingArt("PlacementPreview");
            previewArt.Root.gameObject.SetActive(true);
            var size = (Vector2)FacilityGeometry.GetSize(definition, rotation);
            previewArt.Size = size * 2;
            previewArt.Root.position = LaboratoryWorldMap.CellToWorld((Vector2)origin.Value + size * .5f);
            var supplied = assets != null && assets.HasGathering(definition);
            previewArt.Body.sprite = supplied ? assets.GatheringFrame(0) : world.Solid;
            previewArt.Body.transform.localPosition = Vector3.zero;
            previewArt.Body.transform.localScale = supplied ? Vector3.one : (Vector3)(size * 2) + Vector3.forward;
            previewArt.Body.transform.localRotation = supplied ? Quaternion.Euler(0, 0, -90 * (int)rotation) : Quaternion.identity;
            previewArt.Body.color = supplied ? new Color(1, 1, 1, .7f) : valid ? new Color(.12f, .55f, .32f, .8f) : new Color(.65f, .15f, .18f, .8f);
            previewArt.Cargo.gameObject.SetActive(false); previewArt.Count.gameObject.SetActive(false);
            DrawBorder(previewArt, true, valid ? new Color(.2f, 1, .5f) : new Color(1, .25f, .25f));
            DrawInteraction(previewArt, definition, origin.Value, rotation);
            SetArtOrder(previewArt, 30000);
        }
        private static void SetArtOrder(ThingArt item, int order)
        {
            // A single visible sprite can batch directly. Group only composites that must stay together.
            item.Group.enabled = item.Cargo.gameObject.activeSelf || item.Interaction.gameObject.activeSelf || item.Border[0].gameObject.activeSelf;
            item.Group.sortingOrder = order;
            item.Body.sortingOrder = order;
            item.Cargo.sortingOrder = order + 1;
            item.Interaction.sortingOrder = order - 1;
            foreach (var border in item.Border) border.sortingOrder = order + 2;
        }
        private ThingArt CreateThingArt(string name)
        {
            var root = new GameObject(name, typeof(SortingGroup)).transform;
            root.SetParent(world.Root, false); root.gameObject.layer = world.Root.gameObject.layer;
            var anchor = new GameObject("CarryAnchor").transform; anchor.SetParent(root, false);
            var item = new ThingArt { Root = root, CarryAnchor = anchor, Group = root.GetComponent<SortingGroup>(),
                Body = world.CreateSprite("Body", root), Cargo = world.CreateSprite("Cargo", anchor, 1),
                Interaction = world.CreateSprite("InteractionCell", root, -1), Border = new SpriteRenderer[4] };
            for (var i = 0; i < 4; i++) { item.Border[i] = world.CreateSprite("Selection_" + i, root, 2); item.Border[i].sprite = world.Solid; }
            item.Interaction.sprite = world.Solid; item.Interaction.color = new Color(.5f, .4f, .65f, .65f);
            item.Count = UiWidgets.Label("ThingAnnotation", artLabels, 14); item.Count.font = font;
            item.Count.alignment = TextAlignmentOptions.Center; item.Count.raycastTarget = false;
            item.Count.textWrappingMode = TextWrappingModes.NoWrap; item.Count.overflowMode = TextOverflowModes.Truncate;
            item.Count.rectTransform.anchorMin = item.Count.rectTransform.anchorMax = Vector2.zero;
            return item;
        }
        private void DrawInteraction(ThingArt item, FacilityData definition, Vector2Int origin, QuarterTurn rotation)
        {
            item.Interaction.gameObject.SetActive(definition != null);
            if (definition == null) return;
            var cell = FacilityGeometry.GetInteractionCell(definition, origin, rotation);
            item.Interaction.gameObject.SetActive(item == previewArt || !creatureCounts.ContainsKey(cell));
            item.Interaction.transform.position = LaboratoryWorldMap.CellToWorld((Vector2)cell + Vector2.one * .5f);
            item.Interaction.transform.localScale = new Vector3(1.85f, 1.85f, 1);
        }
        private void DrawBorder(ThingArt item, bool visible, Color color)
        {
            var size = item.Size; var stroke = 4 / CellSize;
            for (var i = 0; i < 4; i++)
            {
                var image = item.Border[i]; image.gameObject.SetActive(visible); if (!visible) continue;
                image.color = color;
                image.transform.localScale = i < 2 ? new Vector3(size.x, stroke, 1) : new Vector3(stroke, size.y, 1);
                image.transform.localPosition = i < 2 ? new Vector3(0, (i == 0 ? -1 : 1) * (size.y - stroke) * .5f)
                    : new Vector3((i == 2 ? -1 : 1) * (size.x - stroke) * .5f, 0);
            }
        }
        private void ReserveCreatureArtBounds()
        {
            foreach (var pair in thingArt)
            {
                if (!artSimulation.Registry.TryGet(pair.Key, out var thing) || thing.Definition.Kind != ThingKind.Creature) continue;
                ReserveSpriteBounds(pair.Value.Body);
                if (pair.Value.Cargo.gameObject.activeSelf) ReserveSpriteBounds(pair.Value.Cargo);
            }
        }
        private void ReserveSpriteBounds(SpriteRenderer sprite)
        {
            var min = LaboratoryWorldMap.WorldToLocal(sprite.bounds.min, Origin, CellSize);
            var max = LaboratoryWorldMap.WorldToLocal(sprite.bounds.max, Origin, CellSize);
            occupiedStatusBounds.Add(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
        }
    }
}
