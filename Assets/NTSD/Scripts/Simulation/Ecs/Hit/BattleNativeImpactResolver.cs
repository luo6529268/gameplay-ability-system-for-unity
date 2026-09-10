using System;

namespace NTSD.Simulation.Ecs
{
    internal enum BattleNativeImpactWriteKind
    {
        Environment, CatchSource, ImpactSource, Action, Vx, Vz, PendingX, PendingZ, YInt, Y, Vy, PendingY,
    }

    internal readonly struct BattleNativeImpactOperation
    {
        internal BattleNativeImpactOperation(BattleNativeImpactWriteKind kind, double value)
        {
            Kind = kind;
            Value = value;
        }

        internal BattleNativeImpactWriteKind Kind { get; }
        internal double Value { get; }
    }

    internal struct BattleNativeImpactInput
    {
        internal int Kind, TargetType, TargetState, TargetAction, TargetObjectId;
        internal int Environment, CatchSource, ImpactSource, Respond;
        internal bool FirstOwnerValid, CreditOwnerValid;
        internal int FirstOwnerSlot, CreditSlot, AttackerSlot, Rule94;
        internal bool ImmunityAudited;
        internal int[] ImmuneObjectIds;
        internal int YInt;
        internal double Y, Vx, Vy, Vz, PendingX, PendingY, PendingZ;
    }

    internal readonly struct BattleNativeImpactPlan
    {
        private readonly BattleNativeImpactInput input;

        internal BattleNativeImpactPlan(in BattleNativeImpactInput input)
        {
            this.input = input;
            Applied = true;
        }

        internal bool Applied { get; }
        private bool Character => input.TargetType == 0;
        private bool ObjectAction => !Character && input.TargetState != (input.TargetType == 2 ? 2000 : 1000);
        private bool ClampY => input.YInt >= -2;
        private bool AdjustY => !ClampY && input.Vy > -6.0;
        internal int OperationCount => !Applied ? 0 :
            (Character ? 8 : ObjectAction ? 5 : 4) + (ClampY ? 3 : AdjustY ? 2 : 0);
        internal int RngDrawCount => 0;

        internal BattleNativeImpactOperation GetOperation(int index)
        {
            if ((uint)index >= (uint)OperationCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (Character)
            {
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.Environment, -(input.Rule94 > 0 ? input.Rule94 : 20));
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.CatchSource, 0x2000 + input.CreditSlot);
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.ImpactSource, input.AttackerSlot);
            }
            else if (ObjectAction)
            {
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.Action, 0);
            }

            // Alignment contract: NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001
            if (index-- == 0) return Op(BattleNativeImpactWriteKind.Vx, input.Vx / 1.07);
            if (index-- == 0) return Op(BattleNativeImpactWriteKind.Vz, input.Vz / 1.07);
            if (index-- == 0) return Op(BattleNativeImpactWriteKind.PendingX, input.Vx / 1.07);
            if (index-- == 0) return Op(BattleNativeImpactWriteKind.PendingZ, input.Vz / 1.07);
            if (Character && index-- == 0)
                return Op(BattleNativeImpactWriteKind.Action, input.Respond == 0 ? 182 : input.Respond);
            if (ClampY)
            {
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.YInt, -2);
                if (index-- == 0) return Op(BattleNativeImpactWriteKind.Y, -2.0);
                return Op(BattleNativeImpactWriteKind.Vy, -6.0);
            }

            double adjustedVy = input.Vy - (Character ? 3.0 : 2.3);
            return Op(index == 0 ? BattleNativeImpactWriteKind.Vy : BattleNativeImpactWriteKind.PendingY, adjustedVy);
        }

        private static BattleNativeImpactOperation Op(BattleNativeImpactWriteKind kind, double value)
        {
            return new BattleNativeImpactOperation(kind, value);
        }
    }

    internal static class BattleNativeImpactResolver
    {
        internal static BattleNativeImpactPlan Resolve(in BattleNativeImpactInput input)
        {
            if (!IsImpactKind(input.Kind) || (input.Kind == 11 && input.Environment >= 0))
                return default;
            bool character = input.TargetType == 0;
            if ((input.Kind == 17 && !character) || (input.Kind == 18 && character))
                return default;
            if (character)
            {
                if (!input.FirstOwnerValid || input.FirstOwnerSlot < 0 ||
                    !input.CreditOwnerValid || input.CreditSlot < 0)
                    return default;
            }
            else if (input.TargetType != 2)
            {
                if (input.TargetType != 1 && input.TargetType != 4 && input.TargetType != 6)
                    return default;
                if (!input.ImmunityAudited || input.ImmuneObjectIds == null || input.ImmuneObjectIds.Length == 0)
                    return default;
                for (int i = 0; i < input.ImmuneObjectIds.Length; i++)
                {
                    if (input.ImmuneObjectIds[i] == input.TargetObjectId)
                        return default;
                }
            }
            return new BattleNativeImpactPlan(in input);
        }

        internal static bool IsImpactKind(int kind)
        {
            return kind == 10 || kind == 11 || kind == 17 || kind == 18;
        }
    }
}
