using System;

namespace NTSD.Simulation.Ecs
{
    internal enum BattlePickupTransactionOutcome
    {
        Unsupported,
        AppliedWithoutRelation,
        RelationEstablished,
    }

    internal enum BattlePickupWriteKind
    {
        SetTargetWeaponHp,
        SetHolderRelationCount,
        SetHolderRelation,
        SetTargetRelation,
        SetHolderLinkedChildSlot,
        SetTargetLinkedParentSlot,
        SetTargetOwnerSlot,
        SetTargetBattleGroup,
        SetHolderAction,
        SetHolderFrameCounter,
    }

    internal readonly struct BattlePickupWriteOperation
    {
        internal BattlePickupWriteOperation(BattlePickupWriteKind kind, int value)
        {
            Kind = kind;
            Value = value;
        }

        internal BattlePickupWriteKind Kind { get; }
        internal int Value { get; }
    }

    internal readonly struct BattlePickupTransactionInput
    {
        internal BattlePickupTransactionInput(int targetType, int targetObjectId, int targetHp,
            int targetWeaponHp, int holderPhysicalSlot, int targetPhysicalSlot, int holderBattleGroup,
            int holderRelation, int holderRelationCount, int holderAction, int holderFrameCounter,
            bool targetFramePresent, int targetWeaponAction)
        {
            TargetType = targetType;
            TargetObjectId = targetObjectId;
            TargetHp = targetHp;
            TargetWeaponHp = targetWeaponHp;
            HolderPhysicalSlot = holderPhysicalSlot;
            TargetPhysicalSlot = targetPhysicalSlot;
            HolderBattleGroup = holderBattleGroup;
            HolderRelation = holderRelation;
            HolderRelationCount = holderRelationCount;
            HolderAction = holderAction;
            HolderFrameCounter = holderFrameCounter;
            TargetFramePresent = targetFramePresent;
            TargetWeaponAction = targetWeaponAction;
        }

        internal int TargetType { get; }
        internal int TargetObjectId { get; }
        internal int TargetHp { get; }
        internal int TargetWeaponHp { get; }
        internal int HolderPhysicalSlot { get; }
        internal int TargetPhysicalSlot { get; }
        internal int HolderBattleGroup { get; }
        internal int HolderRelation { get; }
        internal int HolderRelationCount { get; }
        internal int HolderAction { get; }
        internal int HolderFrameCounter { get; }
        internal bool TargetFramePresent { get; }
        internal int TargetWeaponAction { get; }
    }

    internal readonly struct BattlePickupTransactionPlan
    {
        private readonly BattlePickupTransactionInput input;
        private readonly int holderRelation;
        private readonly int targetRelation;
        private readonly int initialHolderAction;
        private readonly bool clearWeaponHp;

        private BattlePickupTransactionPlan(in BattlePickupTransactionInput input,
            BattlePickupTransactionOutcome outcome, int holderRelation, int targetRelation,
            int initialHolderAction, bool clearWeaponHp)
        {
            this.input = input;
            Outcome = outcome;
            this.holderRelation = holderRelation;
            this.targetRelation = targetRelation;
            this.initialHolderAction = initialHolderAction;
            this.clearWeaponHp = clearWeaponHp;
        }

        internal BattlePickupTransactionOutcome Outcome { get; }
        internal bool Applied => Outcome != BattlePickupTransactionOutcome.Unsupported;
        internal bool RelationEstablished => Outcome == BattlePickupTransactionOutcome.RelationEstablished;
        private bool IncrementCount => RelationEstablished && input.HolderRelation == 0;
        private bool OverrideAction => Applied && input.TargetFramePresent && input.TargetWeaponAction != 0;
        internal int OperationCount => !Applied ? 0 : (clearWeaponHp ? 1 : 0) + (IncrementCount ? 1 : 0) +
            (RelationEstablished ? 7 : 0) + 1 + (OverrideAction ? 1 : 0);

        internal static BattlePickupTransactionPlan Create(in BattlePickupTransactionInput input,
            BattleLockedPickupWeaponThrowRules rules)
        {
            if (!rules.IsSupported || input.HolderPhysicalSlot < 0 || input.TargetPhysicalSlot < 0)
                return new BattlePickupTransactionPlan(input, BattlePickupTransactionOutcome.Unsupported, 0, 0, 0, false);

            int holderRelation = 0;
            int targetRelation = 0;
            int action = 115;
            bool clearWeaponHp = false;
            switch (input.TargetType)
            {
                case 1:
                    rules.TryResolveType1Relation(input.TargetObjectId, out holderRelation);
                    targetRelation = -1;
                    break;
                case 2:
                    holderRelation = 2;
                    targetRelation = -2;
                    action = 116;
                    break;
                case 4:
                    holderRelation = 4;
                    targetRelation = -4;
                    break;
                case 6:
                    holderRelation = input.TargetHp > 0 ? 6 : 4;
                    targetRelation = -holderRelation;
                    clearWeaponHp = input.TargetHp <= 0;
                    break;
            }

            // Alignment contract: NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001
            var outcome = holderRelation == 0
                ? BattlePickupTransactionOutcome.AppliedWithoutRelation
                : BattlePickupTransactionOutcome.RelationEstablished;
            return new BattlePickupTransactionPlan(input, outcome, holderRelation, targetRelation, action, clearWeaponHp);
        }

        internal BattlePickupWriteOperation GetOperation(int index)
        {
            if ((uint)index >= (uint)OperationCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (clearWeaponHp)
            {
                if (index == 0) return new BattlePickupWriteOperation(BattlePickupWriteKind.SetTargetWeaponHp, 0);
                index--;
            }
            if (IncrementCount)
            {
                if (index == 0) return new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderRelationCount, unchecked(input.HolderRelationCount + 1));
                index--;
            }
            if (RelationEstablished)
            {
                if (index < 7)
                {
                    return index switch
                    {
                        0 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderRelation, holderRelation),
                        1 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetTargetRelation, targetRelation),
                        2 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderLinkedChildSlot, input.TargetPhysicalSlot),
                        3 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetTargetLinkedParentSlot, input.HolderPhysicalSlot),
                        4 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetTargetOwnerSlot, input.HolderPhysicalSlot),
                        5 => new BattlePickupWriteOperation(BattlePickupWriteKind.SetTargetBattleGroup, input.HolderBattleGroup),
                        _ => new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderAction, initialHolderAction),
                    };
                }
                index -= 7;
            }
            return index == 0
                ? new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderFrameCounter, 0)
                : new BattlePickupWriteOperation(BattlePickupWriteKind.SetHolderAction, input.TargetWeaponAction);
        }
    }
}
