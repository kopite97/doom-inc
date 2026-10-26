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
        private readonly SessionController controller;
        public LaboratoryCommands(SessionController controller)
        { this.controller = controller ?? throw new ArgumentNullException(nameof(controller)); }
        private LaboratorySimulation Current => controller.Phase == SessionPhase.Playing
            ? controller.Current?.Runtime as LaboratorySimulation : null;
        public bool BeginConstruction(FacilityDefinition definition) => Current?.BeginConstruction(definition) == true;
        public bool PreviewConstruction(Vector2Int origin, QuarterTurn rotation) => Current?.PreviewConstruction(origin, rotation) == true;
        public bool ConfirmConstruction() => Current?.ConfirmConstruction() == true;
        public bool CancelConstruction() =>
            (controller.Current?.Runtime as LaboratorySimulation)?.CancelConstruction() == true;
        public bool SetAllowed(Guid id, bool allowed) => Current?.SetAllowed(id, allowed) == true;
        public bool SetPreset(int[] priorities) => Current?.SetPreset(priorities) == true;
        public bool SetWorkerPriorities(Guid id, int[] priorities, bool usePreset = false) =>
            Current?.SetWorkerPriorities(id, priorities, usePreset) == true;
    }
}
