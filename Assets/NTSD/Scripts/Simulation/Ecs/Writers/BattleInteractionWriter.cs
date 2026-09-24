using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleLockedPickupWeaponThrowRules
    {
        private BattleLockedPickupWeaponThrowRules(bool supported)
        {
            IsSupported = supported;
        }

        internal bool IsSupported { get; }
        internal static BattleLockedPickupWeaponThrowRules Locked => new BattleLockedPickupWeaponThrowRules(true);

        internal static BattleLockedPickupWeaponThrowRules FromAuditedTable(bool audited, IReadOnlyList<int> objectIds)
        {
            if (!audited || objectIds == null || objectIds.Count != 2)
                return default;
            bool locked = (objectIds[0] == 120 && objectIds[1] == 124) ||
                          (objectIds[0] == 124 && objectIds[1] == 120);
            return new BattleLockedPickupWeaponThrowRules(locked);
        }

        internal bool TryResolveType1Relation(int objectId, out int relation)
        {
            relation = IsSupported ? (objectId == 120 || objectId == 124 ? 101 : 1) : 0;
            return IsSupported;
        }
    }

    /// <summary>
    /// Owns the canonical cpoint, held and link writes produced while consuming
    /// interaction candidates. Candidate ordering remains owned by the battle
    /// pipeline; Unity shells only adapt their concrete hit implementation.
    /// </summary>
    internal sealed class BattleInteractionWriter
    {
        internal bool TryApplyGrab(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr,
            int kind)
        {
            if (attacker?.Runtime == null || victim?.Runtime == null || itr == null)
                return false;
            if (kind != 1 && kind != 3)
                return false;
            // Alignment contract: R4-COL-005A. C++ gates kind3 (and kind8
            // during collection) to character targets, but kind1 enters the
            // common Entity grab writer without an extra target-type reject.
            if (kind == 3 &&
                LF2Entity.ResolveCurrentDataObjectType(victim) !=
                (int)LF2ObjectType.Character)
            {
                return false;
            }

            if (kind == 3)
                return TryApplyKind3Grab(attacker, victim, itr);

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.Catching;
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0
                ? itr.caughtact[0]
                : LF2StandardFrames.PickedCaught;

            attacker.Runtime.Vx = 0.0;
            victim.Runtime.Vx = 0.0;

            int attackerXInt = attacker.Runtime.XInt;
            int attackerYInt = attacker.Runtime.YInt;
            int victimXInt = victim.Runtime.XInt;
            // Alignment contract: NTSD28-USER-SOURCE-GRAB-FACING-001.
            bool attackerFacesLeft = attacker.Runtime.SourceRulePositionInitialized &&
                victim.Runtime.SourceRulePositionInitialized
                    ? attacker.Runtime.SourceRuleXInt > victim.Runtime.SourceRuleXInt
                    : attackerXInt > victimXInt;
            attacker.SwitchDir(attackerFacesLeft ? "left" : "right");
            victim.SwitchDir(attackerFacesLeft ? "right" : "left");

            attacker.SetCpointRawFramePreserveWait(catchingFrame);
            victim.SetCpointRawFramePreserveWait(caughtFrame);

            victim.Runtime.X = victimXInt;
            victim.Runtime.Y = victim.Runtime.YInt;

            AlignGrabPair(
                attacker,
                victim,
                attackerXInt,
                attackerYInt,
                victimXInt);

            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.Runtime.CaughtDuration = 300;
            victim.FallCounter = 0;
            attacker.RefreshRuntimeSnapshot();
            victim.RefreshRuntimeSnapshot();
            return true;
        }

        private bool TryApplyKind3Grab(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            int attackerSlot = attacker.Runtime.SlotIndex;
            int victimSlot = victim.Runtime.SlotIndex;
            if (attackerSlot < 0 || victimSlot < 0)
                return false;

            int attackerXInt = attacker.Runtime.XInt;
            int attackerYInt = attacker.Runtime.YInt;
            int victimXInt = victim.Runtime.XInt;
            // Alignment contract: NTSD28-USER-SOURCE-GRAB-FACING-001.
            bool attackerFacesLeft = attacker.Runtime.SourceRulePositionInitialized &&
                victim.Runtime.SourceRulePositionInitialized
                    ? attacker.Runtime.SourceRuleXInt > victim.Runtime.SourceRuleXInt
                    : attackerXInt > victimXInt;
            bool victimFacesLeft = !attackerFacesLeft;
            int catchingFrame = ResolveRelationAction(
                itr.catchingact,
                ref attackerFacesLeft);
            int caughtFrame = ResolveRelationAction(
                itr.caughtact,
                ref victimFacesLeft);

            // Alignment contract: NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001.
            LF2FrameData attackerFrame = attacker.FrameCache?.GetNativeFrameDataById(catchingFrame);
            LF2FrameData victimFrame = victim.FrameCache?.GetNativeFrameDataById(caughtFrame);
            if (attackerFrame == null || victimFrame == null)
            {
                return false;
            }

            attacker.Runtime.Vx = 0.0;
            victim.Runtime.Vx = 0.0;
            attacker.SwitchDir(attackerFacesLeft ? "left" : "right");
            victim.SwitchDir(victimFacesLeft ? "left" : "right");
            attacker.SetCpointRawFramePreserveWait(catchingFrame, attackerFrame);
            victim.SetCpointRawFramePreserveWait(caughtFrame, victimFrame);
            attacker.Runtime.X = attackerXInt;
            attacker.Runtime.Y = attackerYInt;

            AlignGrabPair(
                attacker,
                victim,
                attackerXInt,
                attackerYInt,
                victimXInt);

            attacker.CaughtSlotIndex = victimSlot;
            victim.Runtime.CatchSourceSlot90 = attackerSlot;
            victim.CatcherSlotIndex = attackerSlot;
            attacker.Runtime.CaughtDuration = itr.respond == 0 ? 300 : itr.respond;
            victim.FallCounter = 0;
            attacker.RefreshRuntimeSnapshot();
            victim.RefreshRuntimeSnapshot();
            return true;
        }

        private static int ResolveRelationAction(
            int[] encodedActions,
            ref bool facesLeft)
        {
            int action = encodedActions != null && encodedActions.Length > 0
                ? encodedActions[0]
                : 0;
            if (action >= 0)
                return action;

            facesLeft = !facesLeft;
            return -action;
        }

        internal bool TryApplyPickup(
            LF2Entity attacker,
            LF2Entity target,
            int kind)
        {
            return TryApplyPickup(attacker, target, kind, BattleLockedPickupWeaponThrowRules.Locked);
        }

        internal bool TryApplyPickup(
            LF2Entity attacker,
            LF2Entity target,
            int kind,
            BattleLockedPickupWeaponThrowRules rules)
        {
            if (attacker?.Runtime == null || target?.Runtime == null || attacker.Frame == null || kind != 2)
                return false;

            LF2FrameData targetFrame = target.Frame != null
                ? target.FrameCache?.GetNativeFrameDataById(target.Frame.N)
                : null;
            var input = new BattlePickupTransactionInput(
                target.GetCurrentDataObjectTypeForSimulation(),
                target.FrameCache?.Wrapper?.characterId ?? target.ObjectId,
                target.Health?.HP ?? 0,
                target.Runtime.WeaponFlightCounter,
                attacker.Runtime.SlotIndex,
                target.Runtime.SlotIndex,
                attacker.RelationTeam,
                attacker.Runtime.LinkState,
                attacker.Runtime.PickupCount,
                attacker.Frame.N,
                attacker.AttackingCounter,
                targetFrame != null,
                targetFrame?.PrimaryWeaponPoint.WeaponAct ?? 0);
            BattlePickupTransactionPlan plan = BattlePickupTransactionPlan.Create(input, rules);
            if (!plan.Applied)
                return false;

            // Alignment contract: NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001.
            for (int i = 0; i < plan.OperationCount; i++)
            {
                BattlePickupWriteOperation operation = plan.GetOperation(i);
                switch (operation.Kind)
                {
                    case BattlePickupWriteKind.SetTargetWeaponHp:
                        target.Runtime.WeaponFlightCounter = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderRelationCount:
                        attacker.Runtime.PickupCount = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderRelation:
                        attacker.Runtime.LinkState = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetRelation:
                        target.Runtime.LinkState = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderLinkedChildSlot:
                        attacker.Runtime.TargetSlotIndex = operation.Value;
                        attacker.Runtime.HeldWeaponStableId = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetLinkedParentSlot:
                        target.Runtime.HolderStableId = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetOwnerSlot:
                        target.Runtime.OwnerSlotIndex = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetBattleGroup:
                        target.RelationTeam = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderAction:
                        attacker.DirectWriteNativeRawFramePreserveWaitCounter(operation.Value);
                        break;
                    case BattlePickupWriteKind.SetHolderFrameCounter:
                        attacker.AttackingCounter = operation.Value;
                        break;
                }
            }

            if (plan.RelationEstablished && attacker is LF2Character character)
                character.HeldWeaponReferenceInternal = target;
            return true;
        }

        private void AlignGrabPair(
            LF2Entity attacker,
            LF2Entity victim,
            int attackerXInt,
            int attackerYInt,
            int victimXInt)
        {
            LF2FrameData attackerFrame = attacker.Frame?.D;
            LF2FrameData victimFrame = victim.Frame?.D;

            int attackerWact = attackerFrame?.PrimaryCatchPoint.X ?? 0;
            int victimWact = victimFrame?.PrimaryCatchPoint.X ?? 0;
            int attackerCx = attackerFrame?.centerx ?? 0;
            int attackerCy = attackerFrame?.centery ?? 0;
            int victimCx = victimFrame?.centerx ?? 0;
            int victimCy = victimFrame?.centery ?? 0;

            victim.Runtime.X = attacker.Runtime.Dir == "right"
                ? attackerXInt - attackerCx - victimCx + attackerWact + victimWact
                : attackerCx + victimCx + attackerXInt - attackerWact - victimWact;
            victim.Runtime.Y = victimCy - attackerCy + attackerYInt;

            double lerp = (victimXInt - victim.Runtime.X) * 0.5;
            victim.Runtime.X += lerp;
            attacker.Runtime.X += lerp;
            victim.Runtime.XInt = (int)victim.Runtime.X;
            attacker.Runtime.XInt = (int)attacker.Runtime.X;

            if (attacker.Runtime.SourceRulePositionInitialized &&
                victim.Runtime.SourceRulePositionInitialized)
            {
                // Alignment contract: NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001.
                int sourceAttackerX = attacker.Runtime.SourceRuleXInt;
                int sourceVictimX = victim.Runtime.SourceRuleXInt;
                double sourceVictimAnchor = attacker.Runtime.Dir == "right"
                    ? sourceAttackerX - attackerCx - victimCx + attackerWact + victimWact
                    : attackerCx + victimCx + sourceAttackerX - attackerWact - victimWact;
                double sourceBlend = (sourceVictimX - sourceVictimAnchor) * 0.5;
                victim.Runtime.SourceRuleX = sourceVictimAnchor + sourceBlend;
                attacker.Runtime.SourceRuleX = sourceAttackerX + sourceBlend;
                victim.Runtime.SourceRuleXInt = (int)victim.Runtime.SourceRuleX;
                attacker.Runtime.SourceRuleXInt = (int)attacker.Runtime.SourceRuleX;
            }
        }
    }
}
