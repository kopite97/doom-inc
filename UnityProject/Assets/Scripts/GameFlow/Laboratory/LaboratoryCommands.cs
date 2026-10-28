using System;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.GameFlow
{
    // Thin lifecycle gate shared by presentation and harness. Never caches a runtime
    // across Reload/Restart, where the controller publishes a different object graph.
    public sealed class LaboratoryCommands
    {
        private readonly GameFlowManager controller;
        public LaboratoryCommands(GameFlowManager controller)
        { this.controller = controller ?? throw new ArgumentNullException(nameof(controller)); }
        private LaboratoryManager Current => controller.Phase == SessionPhase.Playing
            ? controller.Current?.Runtime as LaboratoryManager : null;
        public bool ReleaseInvaders() => controller.CanAdvanceExternal && Current?.Invasion.ReleaseAll() == true;
        public bool BeginConstruction(FacilityDefinition definition) => Current?.BeginConstruction(definition) == true;
        public bool PreviewConstruction(Vector2Int origin, QuarterTurn rotation) => Current?.PreviewConstruction(origin, rotation) == true;
        public bool ConfirmConstruction() => Current?.ConfirmConstruction() == true;
        public bool CancelConstruction() =>
            (controller.Current?.Runtime as LaboratoryManager)?.CancelConstruction() == true;
        public bool SetAllowed(Guid id, bool allowed) => Current?.SetAllowed(id, allowed) == true;
        public bool SetAllowed(Guid id, ResourceDefinition resource, bool allowed) => Current?.SetAllowed(id, resource, allowed) == true;
        public bool SelectCultivation(Guid id, CultivationTarget target) => Current?.SelectCultivation(id, target) == true;
        public bool AssignWorker(Guid id, Guid? facility) => Current?.AssignWorker(id, facility) == true;
        public bool SetPreset(int[] priorities) => Current?.SetPreset(priorities) == true;
        public bool SetWorkerPriorities(Guid id, int[] priorities, bool usePreset = false) =>
            Current?.SetWorkerPriorities(id, priorities, usePreset) == true;
    }
}
