using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;

namespace NTSD.Simulation.Ecs
{
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
            bool attackerFacesLeft = attackerXInt > victimXInt;
            attacker.SwitchDir(attackerFacesLeft ? "left" : "right");
            victim.SwitchDir(attackerFacesLeft ? "right" : "left");

            attacker.SetCpointRawFramePreserveWait(catchingFrame);
            victim.SetCpointRawFramePreserveWait(caughtFrame);

            if (kind == 1)
            {
                victim.Runtime.X = victimXInt;
                victim.Runtime.Y = victim.Runtime.YInt;
            }
            else
            {
                attacker.Runtime.X = attackerXInt;
                attacker.Runtime.Y = attackerYInt;
            }

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

        internal bool TryApplyPickup(
            LF2Entity attacker,
            LF2Entity target,
            int kind)
        {
            if (attacker?.Runtime == null || target?.Runtime == null)
                return false;
            if (kind != 2 && kind != 7)
                return false;
            if (kind == 7 && attacker.Runtime.LinkState != 0)
                return false;

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (attackerSlot < 0 || targetSlot < 0)
                return false;

            int linkState;
            int targetLinkState;
            if (kind == 7)
            {
                int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
                linkState = 1;
                targetLinkState = -1;
                if (targetOid == 120 || targetOid == 124)
                    linkState = 101;
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    linkState = 4;
                    targetLinkState = -4;
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    linkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                    targetLinkState = -linkState;
                }
            }
            else
            {
                if (targetType == (int)LF2ObjectType.LightWeapon)
                {
                    linkState = 1;
                    attacker.DirectWriteRawFramePreserveWaitCounter(
                        LF2StandardFrames.PickingLight);
                }
                else if (targetType == (int)LF2ObjectType.HeavyWeapon)
                {
                    linkState = 2;
                    attacker.DirectWriteRawFramePreserveWaitCounter(
                        LF2StandardFrames.PickingHeavy);
                }
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    linkState = 4;
                    attacker.DirectWriteRawFramePreserveWaitCounter(
                        LF2StandardFrames.PickingLight);
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    linkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                    attacker.DirectWriteRawFramePreserveWaitCounter(
                        LF2StandardFrames.PickingLight);
                    if (target.Health == null || target.Health.HP <= 0)
                        target.Runtime.WeaponFlightCounter = 0;
                }
                else
                {
                    return false;
                }

                attacker.AttackingCounter = 0;
                targetLinkState = -linkState;
            }

            attacker.Runtime.LinkState = linkState;
            target.Runtime.LinkState = targetLinkState;
            target.RelationTeam = attacker.RelationTeam;
            attacker.Runtime.TargetSlotIndex = targetSlot;
            attacker.Runtime.HeldWeaponStableId = targetSlot;
            target.Runtime.HolderStableId = attackerSlot;
            target.HolderCopySlot = attackerSlot;
            attacker.Runtime.PickupCount++;
            if (targetType == (int)LF2ObjectType.Drink &&
                (target.Health == null || target.Health.HP <= 0))
            {
                target.Runtime.WeaponFlightCounter = 0;
            }

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
        }
    }
}
