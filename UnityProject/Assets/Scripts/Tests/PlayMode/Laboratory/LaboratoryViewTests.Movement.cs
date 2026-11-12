#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("MovementPresentation")]
        public IEnumerator MovingBodyCargoSelectionAndPauseUseOnePublishedPosition()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, false);
                var worker = view.Simulation.Registry.Things.Single(t => t.TryGetComponent<WorkerComponent>(out _));
                foreach (var resource in worker.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                    view.Simulation.Registry.ConsumeResource(resource, resource.GetComponent<ResourceStackComponent>().Quantity);
                var source = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(3, 3), QuarterTurn.None, true);
                CreateStatusLayoutFacility(view, "storageId", new Vector2Int(10, 3), QuarterTurn.None, true);
                var organic = GameDataStore.Instance.Catalog.Selected<ResourceData>("organicId");
                var shipment = view.Simulation.Registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
                Assert.That(view.SetAllowed(source.Id, organic, false), Is.True);
                WorkerMoveSegment segment = default;
                var found = false;
                for (var step = 0; step < 800; step++)
                {
                    view.AdvanceFrame(.025, 0); yield return null;
                    if (shipment.HolderId == worker.Id && WorkerManager.TryGetMoveSegment(view.Simulation, worker, out segment)
                        && segment.From.y == segment.To.y
                        && segment.ProgressSeconds / segment.DurationSeconds > .65
                        && segment.ProgressSeconds / segment.DurationSeconds < .9) { found = true; break; }
                }
                Assert.That(found, Is.True, "The actual hauling scenario must expose an in-between carrying segment.");
                var expected = (Vector2)segment.From + (Vector2)(segment.To - segment.From)
                    * (float)(segment.ProgressSeconds / segment.DurationSeconds);
                Assert.That(map.TryDisplayedPosition(worker.Id, out var displayed), Is.True);
                Assert.That(Vector2.Distance(displayed, expected), Is.LessThan(.0001f));
                Assert.That(Vector2.Distance(displayed, worker.Placement.Origin), Is.GreaterThan(.1f));
                var body = ArtSprite(map, worker, "Body"); var cargo = ArtSprite(map, worker, "Cargo");
                Assert.That(cargo.gameObject.activeSelf, Is.True);
                Assert.That(cargo.transform.parent.parent, Is.SameAs(body.transform.parent));
                Assert.That(Vector2.Distance(body.transform.parent.position, (displayed + Vector2.one * .5f) * 2), Is.LessThan(.0001f));
                var codec = new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                var before = codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase));
                for (var i = 0; i < 10; i++) map.PublishFrame();
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current, view.Controller.Phase)), Is.EqualTo(before));
                yield return input.Tap(400, map.CellScreenPoint(worker.Placement.Origin));
                Assert.That(view.SelectedId, Is.EqualTo(Guid.Empty), "An old logical cell must not leave an invisible creature hit area.");
                yield return input.Tap(401, map.CellScreenPoint(displayed));
                Assert.That(view.SelectedId, Is.EqualTo(worker.Id));
                Assert.That(body.transform.parent.Find("Selection_0").gameObject.activeSelf, Is.True);
                var at = view.Simulation.ElapsedSeconds;
                var frame = body.sprite;
                Assert.That(view.BeginGatheringConstruction(), Is.True);
                for (var i = 0; i < 6; i++) { view.AdvanceFrame(.2, 0); yield return null; }
                Assert.That(view.Simulation.ElapsedSeconds, Is.EqualTo(at));
                Assert.That(map.TryDisplayedPosition(worker.Id, out var paused), Is.True);
                Assert.That(Vector2.Distance(paused, displayed), Is.LessThan(.0001f));
                Assert.That(body.sprite, Is.SameAs(frame));
                map.ZoomBy(1.3f); yield return null; yield return null;
                Assert.That(map.TryDisplayedPosition(worker.Id, out paused), Is.True);
                Assert.That(Vector2.Distance(paused, displayed), Is.LessThan(.0001f));
                Assert.That(view.CancelPlacement(), Is.True);
                Assert.That(view.Save(), Is.True); Assert.That(view.Reload(), Is.True);
                yield return null; yield return null;
                Assert.That(map.TryDisplayedPosition(worker.Id, out var restored), Is.True);
                Assert.That(Vector2.Distance(restored, displayed), Is.LessThan(.0001f));
                Assert.That(ArtSprite(map, view.Simulation.Registry.Things.Single(t => t.Id == worker.Id), "Cargo").gameObject.activeSelf, Is.True);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("MovementPresentation")]
        public IEnumerator DisplayEvaluationFrequencyLeavesLogicalSimulationAndSavesIdentical()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                var view = input.View; var map = input.Map;
                var codec = new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values, true);
                var fixture = Plan020Probe.Prepare(1);
                var referencePath = Path.Combine(Path.GetDirectoryName(view.SavePath), "reference-session.json");
                File.WriteAllText(referencePath, fixture);
                var reference = new GameFlowManager(codec, new JsonFileSessionStore(referencePath),
                    () => codec.Restore(codec.Deserialize(fixture)), 30);
                Assert.That(reference.StartOrContinue(), Is.True);
                File.WriteAllText(view.SavePath, fixture); Assert.That(view.Reload(), Is.True);
                var paths = view.Simulation.Map.PathQueries;
                var referenceSimulation = (LaboratoryManager)reference.Current.Runtime;
                var referencePaths = referenceSimulation.Map.PathQueries;
                yield return null; yield return null;
                var initialIds = new HashSet<Guid>(reference.Current.Registry.Things.Select(thing => thing.Id));
                for (var step = 0; step < 300; step++)
                {
                    reference.Advance(.025); view.AdvanceFrame(.025, 0);
                    for (var repeat = 0; repeat < step % 4; repeat++) map.PublishFrame();
                    if (step % 3 == 0) yield return null;
                }
                foreach (var id in initialIds)
                    Assert.That(view.Simulation.Registry.TryGet(id, out _), Is.EqualTo(reference.Current.Registry.TryGet(id, out _)),
                        "Existing saved identities must survive or retire identically.");
                Assert.That(CanonicalMovementSave(codec, view.Controller.Current, view.Controller.Phase, initialIds),
                    Is.EqualTo(CanonicalMovementSave(codec, reference.Current, view.Controller.Phase, initialIds)),
                    "Rendering frequency must not change placements, jobs, resources, reservations or save state.");
                Assert.That(view.Simulation.Map.PathQueries - paths, Is.EqualTo(referenceSimulation.Map.PathQueries - referencePaths),
                    "Normal rendering must not add path searches.");
                LogAssert.NoUnexpectedReceived();
            }
        }

        private static string CanonicalMovementSave(SessionSnapshotCodec codec, SessionState state, SessionPhase phase, ISet<Guid> initialIds)
        {
            var text = codec.Serialize(codec.Capture(state, phase));
            // Runtime-created resource IDs use Guid.NewGuid in both independent simulations.
            // Stable creation order identifies the same logical new object; all state and relationships remain compared.
            foreach (var thing in state.Registry.Things.OrderBy(thing => thing.CreationOrder))
                if (!initialIds.Contains(thing.Id))
                    text = text.Replace(thing.Id.ToString("D"), "thing-" + thing.CreationOrder)
                        .Replace(thing.Id.ToString("N"), "thing-" + thing.CreationOrder);
            return text;
        }

        [UnityTest, Category("MovementPresentation")]
        public IEnumerator MovingFallbackAndPublishedSelectionFollowViewportAndLifecycle()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var map = input.Map;
                view.SetUiPreferences(false, false);
                var definition = GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId");
                var worker = view.Simulation.Registry.Create(definition);
                Assert.That(view.Simulation.Map.Commands.Place(worker.Id, new Vector2Int(7, 4)).Succeeded, Is.True);
                yield return null; yield return null;
                var sawFraction = false;
                for (var step = 0; step < 160; step++)
                {
                    view.AdvanceFrame(.025, 0); yield return null;
                    Assert.That(map.TryDisplayedPosition(worker.Id, out var position), Is.True);
                    if (Vector2.Distance(position, worker.Placement.Origin) > .1f) { sawFraction = true; break; }
                }
                Assert.That(sawFraction, Is.True, "A fallback creature must also show intermediate wandering positions.");
                Assert.That(ArtSprite(map, worker, "Body").sprite, Is.SameAs(map.World.Solid));
                Assert.That(map.TryDisplayedPosition(worker.Id, out var selectedPosition), Is.True);
                yield return input.Tap(402, map.CellScreenPoint(selectedPosition));
                Assert.That(view.SelectedId, Is.EqualTo(worker.Id));
                foreach (var resolution in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                using (new LaboratoryGameViewSize(resolution.x, resolution.y))
                {
                    yield return SettleScreenStateSize(resolution);
                    foreach (var zoom in new[] { 1f, 2.35f, 3f, 8f })
                    {
                        map.Fit(); map.ZoomBy(zoom); yield return null; yield return null;
                        Assert.That(map.TryDisplayedPosition(worker.Id, out var position), Is.True);
                        var root = ArtSprite(map, worker, "Body").transform.parent;
                        Assert.That(Vector2.Distance(root.position, (position + Vector2.one * .5f) * 2), Is.LessThan(.0001f));
                        Assert.That(Vector2.Distance(map.CellScreenPoint(position), map.World.Camera.WorldToScreenPoint(root.position)), Is.LessThan(.002f));
                    }
                }
                var oldId = worker.Id;
                Assert.That(view.Restart(), Is.True); yield return null; yield return null;
                Assert.That(map.TryDisplayedPosition(oldId, out _), Is.False);
                Assert.That(map.World.Root.Find("ThingArt_" + oldId.ToString("N")) == null, Is.True);
                LogAssert.NoUnexpectedReceived();
            }
        }
    }
}
#endif
