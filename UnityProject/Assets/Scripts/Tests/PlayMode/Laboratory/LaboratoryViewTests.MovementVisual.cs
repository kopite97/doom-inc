#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        private struct MovementVisualFrame
        {
            public int index, renderedFrame, logicalX, logicalY;
            public double wallSeconds, simulationSeconds, moveProgress;
            public float bodyWorldX, bodyWorldY, cargoWorldX, cargoWorldY;
            public float cameraX, cameraY, cameraWidth, cameraHeight;
            public float bodyMinX, bodyMinY, bodyMaxX, bodyMaxY;
            public bool cargoVisible, bodyFullyInsideViewport;
            public string jobStage;
        }

        [UnityTest, Category("MovementVisual"), Explicit("Select only when writing a new before/after movement recording.")]
        public IEnumerator MovementRecordingUsesActualEditorUpdateAtThirtyFramesPerSecond()
        {
            const int frameCount = 150;
            const double framesPerSecond = 30;
            var stagePath = Path.Combine(MovementEvidenceRoot, "measurement-stage.txt");
            Assert.That(File.Exists(stagePath), Is.True);
            var stage = File.ReadAllText(stagePath).Trim();
            Assert.That(stage, Is.EqualTo("baseline").Or.EqualTo("after"));
            var fixturePath = Path.Combine(MovementEvidenceRoot, "movement-fixture-1.json");
            Assert.That(File.Exists(fixturePath), Is.True, "Reuse the exact medium performance fixture.");
            var directory = Path.Combine(MovementEvidenceRoot, "movement-" + stage + "-zoom2-v2-frames");
            Directory.CreateDirectory(directory);
            using (var input = new PointerScene())
            {
                yield return input.Load();
                File.WriteAllText(input.View.SavePath, File.ReadAllText(fixturePath));
                Assert.That(input.View.Reload(), Is.True);
                input.View.SimulationRate = 0;
                input.View.SetUiPreferences(true, false);
                // The new session generation resets camera geometry in CheckLayout.
                yield return null;
                var simulation = input.View.Simulation;
                var worker = simulation.Registry.Things.Where(thing => thing.IsPlaced && thing.TryGetComponent<WorkerComponent>(out _))
                    .OrderBy(thing => thing.CreationOrder).First();
                Assert.That(input.View.UiAssets.Art.HasPrototype(worker.Definition), Is.True, "Record the supplied prototype artwork.");
                var workerState = worker.GetComponent<WorkerComponent>();
                var primingCommands = 0;
                var primed = false;
                while (primingCommands < 3600)
                {
                    AdvanceMovementCommand(input.View, primingCommands++);
                    var job = workerState.Job;
                    if (job?.ResourceId != null && job.Stage == JobStage.ToDestination && workerState.MoveProgress > 0
                        && simulation.Registry.TryGet(job.ResourceId.Value, out var resource) && resource.HolderId == worker.Id)
                    { primed = true; break; }
                }
                Assert.That(primed, Is.True, "The actual prototype must enter a carried-resource delivery segment.");
                var primingEnd = simulation.ElapsedSeconds;
                var startLogical = worker.Placement.Origin;
                var destinationId = workerState.Job.TargetId;
                Assert.That(simulation.Registry.TryGet(destinationId, out var destination), Is.True);
                var destinationCell = simulation.TargetCell(destination).Value;
                Assert.That(input.View.SelectThing(worker.Id), Is.True);
                yield return new WaitForSecondsRealtime(.4f);
                input.Map.Fit();
                input.Map.ZoomBy(2);
                yield return null;
                // One preparation-only pan through the existing map method. Camera tracking is not added to the product.
                // CellScreenPoint already adds the root's (.5,.5). Add .4 cell for body/head height,
                // making the camera center the logical route midpoint plus (.5,.9).
                var midpoint = ((Vector2)startLogical + destinationCell) * .5f + Vector2.up * .4f;
                var midpointScreen = input.Map.CellScreenPoint(midpoint);
                var viewportCenter = RectTransformUtility.WorldToScreenPoint(null, input.Map.Viewport.TransformPoint(input.Map.Viewport.rect.center));
                var panPixels = viewportCenter - midpointScreen;
                typeof(MobileGame.Presentation.LaboratoryMapView).GetMethod("Pan",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(input.Map, new object[] { panPixels });
                yield return null;
                Assert.That(input.Map.Zoom, Is.EqualTo(2));
                var body = ArtSprite(input.Map, worker, "Body");
                var cargo = ArtSprite(input.Map, worker, "Cargo");
                var rows = new MovementVisualFrame[frameCount];
                var files = new string[frameCount];
                for (var i = 0; i < frameCount; i++) files[i] = Path.Combine(directory, "frame-" + i.ToString("000") + ".png");
                var startFrame = Time.frameCount;
                var startGameTime = simulation.ElapsedSeconds;
                var startWall = Time.realtimeSinceStartupAsDouble;
                input.View.SimulationRate = 1;
                for (var i = 0; i < frameCount; i++)
                {
                    // Only actual Update supplies model deltas during recording.
                    while (Time.realtimeSinceStartupAsDouble < startWall + i / framesPerSecond) yield return null;
                    yield return new WaitForEndOfFrame();
                    var camera = input.Map.World.Camera;
                    var clip = camera.pixelRect;
                    var min = camera.WorldToScreenPoint(body.bounds.min);
                    var max = camera.WorldToScreenPoint(body.bounds.max);
                    var bodyRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
                    rows[i] = new MovementVisualFrame { index = i, renderedFrame = Time.frameCount,
                        wallSeconds = Time.realtimeSinceStartupAsDouble - startWall, simulationSeconds = simulation.ElapsedSeconds,
                        moveProgress = workerState.MoveProgress, logicalX = worker.Placement.Origin.x, logicalY = worker.Placement.Origin.y,
                        bodyWorldX = body.transform.position.x, bodyWorldY = body.transform.position.y,
                        cargoWorldX = cargo.transform.position.x, cargoWorldY = cargo.transform.position.y,
                        cameraX = clip.x, cameraY = clip.y, cameraWidth = clip.width, cameraHeight = clip.height,
                        bodyMinX = bodyRect.xMin, bodyMinY = bodyRect.yMin, bodyMaxX = bodyRect.xMax, bodyMaxY = bodyRect.yMax,
                        bodyFullyInsideViewport = bodyRect.xMin >= clip.xMin && bodyRect.xMax <= clip.xMax
                            && bodyRect.yMin >= clip.yMin && bodyRect.yMax <= clip.yMax,
                        cargoVisible = cargo.gameObject.activeInHierarchy, jobStage = workerState.Job?.Stage.ToString() };
                    ScreenCapture.CaptureScreenshot(files[i]);
                }
                input.View.SimulationRate = 0;
                var endFrame = Time.frameCount;
                var endGameTime = simulation.ElapsedSeconds;
                yield return WaitFor(() => files.All(file => File.Exists(file) && new FileInfo(file).Length > 0),
                    "Every movement recording frame must be written by the Editor.");
                var allBodiesInside = rows.All(row => row.bodyFullyInsideViewport);
                var data = new { stage, frameCount, framesPerSecond, workerId = worker.Id,
                    definitionId = worker.Definition.DefinitionId, primingCommands, primingEnd,
                    startLogicalX = startLogical.x, startLogicalY = startLogical.y, destinationId,
                    destinationX = destinationCell.x, destinationY = destinationCell.y,
                    cameraMidpointX = midpoint.x, cameraMidpointY = midpoint.y, panPixelsX = panPixels.x, panPixelsY = panPixels.y,
                    resolution = new[] { Screen.width, Screen.height }, zoom = input.Map.Zoom,
                    allBodiesInside, startFrame, endFrame, startGameTime, endGameTime, rows,
                    note = "Actual Editor Update with SimulationRate=1; no AdvanceFrame commands during recording. The same medium fixture is primed with 1/60 commands until its real prototype first carries cargo in ToDestination with positive movement progress. Before/after priming identity and command count must match. One setup-only pan centers the logical route midpoint plus (.5,.9) to include body/head height at fixed zoom2; every PNG records its actual camera clip and body bounds. Capture targets 30fps for five seconds; actual wall/model times expose PNG-capture overhead. This is visual evidence, not fixed-delta performance or device proof." };
                File.WriteAllText(Path.Combine(MovementEvidenceRoot, "movement-" + stage + "-zoom2-v2-recording.json"),
                    (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                        .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { data }));
                Assert.That(endFrame - startFrame, Is.GreaterThan(100), "Confirm the game actually rendered throughout the recording.");
                Assert.That(endGameTime - startGameTime, Is.GreaterThan(2));
                Assert.That(rows[frameCount - 1].wallSeconds - rows[0].wallSeconds, Is.GreaterThan(4));
                Assert.That(allBodiesInside, Is.True, "Keep the prototype body fully inside the fixed map viewport for the whole representative recording.");
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
