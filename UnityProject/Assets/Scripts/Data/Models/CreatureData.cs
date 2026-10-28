using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    public sealed class CreatureData : ThingData
    {
        private readonly int carryCapacity;
        private readonly float moveSecondsPerCell;

        private readonly CreatureRole role;
        public CreatureRole Role => role;
        public bool CanWork => role != CreatureRole.Invader;
        public bool CanOperate => role == CreatureRole.Support;

        private CreatureData() { }
        public CreatureData(CsvRow row, CreatureDefinition asset, GameSettings settings) : base(row, asset, settings)
        { carryCapacity = row.Int("carryCapacity"); moveSecondsPerCell = row.Float("moveSecondsPerCell"); role = row.EnumValue<CreatureRole>("role"); }

        public override ThingKind Kind => ThingKind.Creature;
        public int CarryCapacity => carryCapacity;
        public double MoveSecondsPerCell => moveSecondsPerCell;

        public override void Validate()
        {
            base.Validate();
            if (!Enum.IsDefined(typeof(CreatureRole), role)) throw new ArgumentException("Unknown creature role.");
            if (carryCapacity <= 0)
                throw new ArgumentException("Carry capacity must be positive.");
            if (moveSecondsPerCell <= 0 || float.IsNaN(moveSecondsPerCell) || float.IsInfinity(moveSecondsPerCell))
                throw new ArgumentException("Movement duration must be positive and finite.");
        }
    }
}
