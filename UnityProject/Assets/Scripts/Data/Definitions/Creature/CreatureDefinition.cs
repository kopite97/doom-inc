using System;
using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(fileName = "CreatureDefinition", menuName = "Mobile Game/Definitions/Creature")]
    public sealed class CreatureDefinition : ThingDefinition
    {
        [SerializeField] private int carryCapacity = 5;
        [SerializeField] private float moveSecondsPerCell = 0.25f;

        public override ThingKind Kind => ThingKind.Creature;
        public int CarryCapacity => carryCapacity;
        public double MoveSecondsPerCell => moveSecondsPerCell;

        public override void Validate()
        {
            base.Validate();
            if (carryCapacity <= 0)
                throw new ArgumentException("Carry capacity must be positive.");
            if (moveSecondsPerCell <= 0 || float.IsNaN(moveSecondsPerCell) || float.IsInfinity(moveSecondsPerCell))
                throw new ArgumentException("Movement duration must be positive and finite.");
        }
    }
}
