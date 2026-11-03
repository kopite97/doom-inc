using System;
using System.Collections.Generic;

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

        private static readonly Dictionary<string, IInvaderActivityBehavior> behaviors =
            new Dictionary<string, IInvaderActivityBehavior>
            {
                { "Standard", new StandardInvaderBehavior() },
                { "Raider", new RaiderInvaderBehavior() }
            };
        public IInvaderActivityBehavior Behavior { get; }
        public string ActivityBehavior { get; }
        public int SmallScore { get; }
        public int MajorScore { get; }
        public int SupplyEvery { get; }
        public int SupplyAmount { get; }
        public string SupplyResource { get; }

        private CreatureData() { ActivityBehavior = "None"; }
        public CreatureData(CreatureDto row, CreatureDefinition asset, GameSettings settings) : base(row, asset, settings)
        {
            carryCapacity = row.carryCapacity;
            moveSecondsPerCell = (float)row.moveSecondsPerCell;
            role = row.role;
            ActivityBehavior = row.activityBehavior;
            if (behaviors.TryGetValue(ActivityBehavior, out var behavior)) Behavior = behavior;
            SmallScore = row.smallScore;
            MajorScore = row.majorScore;
            SupplyEvery = row.supplyEvery;
            SupplyAmount = row.supplyAmount;
            SupplyResource = row.supplyResource;
        }

        public override ThingKind Kind => ThingKind.Creature;
        public int CarryCapacity => carryCapacity;
        public double MoveSecondsPerCell => moveSecondsPerCell;

        public override void Validate()
        {
            base.Validate();
            if (!Enum.IsDefined(typeof(CreatureRole), role)) throw new ArgumentException("Unknown creature role.");
            if (role == CreatureRole.Invader ? Behavior == null : ActivityBehavior != "None")
                throw new ArgumentException("Invalid activity behavior for creature role.");
            if (ActivityBehavior == "Raider")
            {
                if (SmallScore <= 0 || MajorScore < SmallScore || SupplyEvery <= 0 || SupplyAmount <= 0 || string.IsNullOrEmpty(SupplyResource))
                    throw new ArgumentException("Invalid raider activity settings.");
            }
            else if (SmallScore != 0 || MajorScore != 0 || SupplyEvery != 0 || SupplyAmount != 0 || !string.IsNullOrEmpty(SupplyResource))
                throw new ArgumentException("Unexpected raider settings.");
            if (carryCapacity <= 0)
                throw new ArgumentException("Carry capacity must be positive.");
            if (moveSecondsPerCell <= 0 || float.IsNaN(moveSecondsPerCell) || float.IsInfinity(moveSecondsPerCell))
                throw new ArgumentException("Movement duration must be positive and finite.");
        }
    }
}
