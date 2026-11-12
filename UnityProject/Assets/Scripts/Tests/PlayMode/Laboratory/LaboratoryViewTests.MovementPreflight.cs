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
        [UnityTest, Category("MovementPreflight"), Explicit("Prepare and validate a controlled measurement fixture before selecting the long comparison.")]
        public IEnumerator ControlledMovementFixturesContainRealMovementAndCargo()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load();
                Directory.CreateDirectory(MovementEvidenceRoot);
                var results = new object[3];
                var movingPresent = new bool[3];
                var carryingPresent = new bool[3];
                for (var level = 0; level < 3; level++)
                {
                    var path = Path.Combine(MovementEvidenceRoot, "movement-fixture-" + level + ".json");
                    if (!File.Exists(path)) File.WriteAllText(path, PrepareMovementFixture(level));
                    File.WriteAllText(input.View.SavePath, File.ReadAllText(path));
                    Assert.That(input.View.Reload(), Is.True);
                    input.View.SimulationRate = 0;
                    var simulation = input.View.Simulation;
                    var workers = simulation.Registry.Things.Where(thing => thing.IsPlaced && thing.TryGetComponent<WorkerComponent>(out _))
                        .OrderBy(thing => thing.CreationOrder).ToArray();
                    Assert.That(workers.Length, Is.EqualTo(level == 0 ? 1 : level == 1 ? 4 : 24));
                    var previousCells = workers.Select(thing => thing.Placement.Origin).ToArray();
                    long movingSamples = 0, carryingSamples = 0, waitingSamples = 0, workingSamples = 0, logicalTransitions = 0;
                    var waitReasons = new long[Enum.GetValues(typeof(WorkerWaitReason)).Length];
                    var initial = MovementWorkloadDiagnostic(simulation);
                    var maxMoving = 0; var maxCarrying = 0;
                    for (var step = 0; step < 5400; step++)
                    {
                        AdvanceMovementCommand(input.View, step);
                        var moving = 0; var carrying = 0; var waiting = 0; var working = 0;
                        for (var i = 0; i < workers.Length; i++)
                        {
                            var thing = workers[i];
                            var worker = thing.GetComponent<WorkerComponent>();
                            var transitioned = thing.Placement.Origin != previousCells[i];
                            if (transitioned && step >= 1800) logicalTransitions++;
                            if (transitioned || worker.MoveProgress > 0) moving++;
                            previousCells[i] = thing.Placement.Origin;
                            if (worker.WaitReason != WorkerWaitReason.None || worker.Job?.Stage == JobStage.Waiting) waiting++;
                            if (worker.Job?.Stage == JobStage.Working && worker.WaitReason == WorkerWaitReason.None) working++;
                            if (step >= 1800) waitReasons[(int)worker.WaitReason]++;
                            if (worker.Job?.ResourceId != null && simulation.Registry.TryGet(worker.Job.ResourceId.Value, out var cargo)
                                && cargo.HolderId == thing.Id && cargo.GetComponent<ResourceStackComponent>().Quantity > 0) carrying++;
                        }
                        if (step >= 1800)
                        {
                            movingSamples += moving; carryingSamples += carrying; waitingSamples += waiting; workingSamples += working;
                            maxMoving = Math.Max(maxMoving, moving); maxCarrying = Math.Max(maxCarrying, carrying);
                        }
                        if (step % 60 == 0) yield return null;
                    }
                    simulation.ValidateState();
                    results[level] = new { level, workers = workers.Length, warmupSteps = 1800, sampleSteps = 3600,
                        logicalTransitions, meanMoving = movingSamples / 3600d, meanCarrying = carryingSamples / 3600d,
                        meanWorking = workingSamples / 3600d, waitReasons = Enum.GetNames(typeof(WorkerWaitReason))
                            .Select((name, index) => new { name, samples = waitReasons[index] }).ToArray(), initial, final = MovementWorkloadDiagnostic(simulation),
                        meanWaiting = waitingSamples / 3600d, maxMoving, maxCarrying,
                        paths = simulation.Map.PathQueries, visited = simulation.Map.VisitedCells };
                    File.WriteAllText(Path.Combine(MovementEvidenceRoot, "movement-fixture-preflight.json"),
                        (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true)
                            .GetMethod("SerializeObject", new[] { typeof(object) }).Invoke(null, new object[] { new { results,
                                note = "Functional preflight with the exact 5400 fixed commands of a controlled measurement; rendering yields only every 60 commands, so this is workload validation, not timing or visual-smoothness evidence. Null entries mean not reached yet." } }));
                    movingPresent[level] = movingSamples > 0;
                    carryingPresent[level] = carryingSamples > 0;
                }
                Assert.That(movingPresent, Is.All.True, "Each fixture must contain real movement in the measured interval.");
                Assert.That(carryingPresent, Is.All.True, "Each operational fixture must contain real carried cargo in the measured interval.");
            }
            LogAssert.NoUnexpectedReceived();
        }

        private static object MovementWorkloadDiagnostic(LaboratoryManager simulation)
        {
            return new { elapsed = simulation.ElapsedSeconds, preset = simulation.Preset, externalInvaders = simulation.Invasion.ExternalCount,
                reservations = simulation.Reservations.Select(item => new { item.WorkerId, item.SourceId, item.DestinationId, item.ResourceId, item.Amount, item.PickedUp }).ToArray(),
                workers = simulation.Registry.Things.Where(thing => thing.TryGetComponent<WorkerComponent>(out _))
                    .OrderBy(thing => thing.CreationOrder).Select(thing => {
                        var worker = thing.GetComponent<WorkerComponent>();
                        return new { thing.Id, thing.CreationOrder, origin = thing.Placement?.Origin, worker.AssignedFacilityId,
                            worker.MoveProgress, wait = worker.WaitReason.ToString(), worker.Priorities, worker.UsesPreset,
                            job = worker.Job == null ? null : new { kind = worker.Job.Kind.ToString(), stage = worker.Job.Stage.ToString(),
                                worker.Job.TargetId, worker.Job.SourceId, worker.Job.ResourceId, worker.Job.Amount } };
                    }).ToArray(),
                stock = simulation.Registry.Things.Where(thing => thing.Definition.Kind == ThingKind.Resource)
                    .OrderBy(thing => thing.CreationOrder).Select(thing => new { thing.Id, thing.CreationOrder,
                        definition = thing.Definition.DefinitionId, thing.HolderId, quantity = thing.GetComponent<ResourceStackComponent>().Quantity }).ToArray(),
                facilities = simulation.Registry.Things.Where(thing => thing.Definition is FacilityData)
                    .OrderBy(thing => thing.CreationOrder).Select(thing => {
                        var definition = (FacilityData)thing.Definition;
                        var gathering = thing.TryGetComponent<GatheringComponent>(out var gathered) ? gathered : null;
                        var production = thing.TryGetComponent<ProductionComponent>(out var produced) ? produced : null;
                        return new { thing.Id, thing.CreationOrder, role = definition.Role.ToString(), origin = thing.Placement.Origin,
                            inventory = simulation.TotalAt(thing.Id), capacity = definition.StorageCapacity,
                            gathering = gathering == null ? null : new { gathering.Progress, gathering.Outstanding },
                            production = production == null ? null : new { production.SelectedTarget, production.ActiveTarget,
                                production.InProgress, production.Progress, production.Outstanding, readiness = simulation.Production.Evaluate(thing).Reason.ToString() } };
                    }).ToArray() };
        }
    }
}
#endif
