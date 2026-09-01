using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns the canonical held-object writes performed by the held-object pass.
    /// The caller remains responsible for runtime-slot traversal order.
    /// </summary>
    internal sealed class BattleHeldObjectWriter
    {
        internal bool SyncHeldPose(
            LF2Entity holder,
            LF2Entity held,
            BattleWeaponPointValue? holderWPoint = null)
        {
            if (holder?.Runtime == null || holder.PS == null || holder.Frame?.D == null)
                return false;
            if (held?.Runtime == null || held.PS == null)
                return false;

            LF2FrameData holderFrame = holder.Frame.D;
            BattleWeaponPointValue resolvedHolderWPoint =
                holderWPoint ?? holderFrame.PrimaryWeaponPoint;

            Vector3 holdpoint = CalculateHoldPoint(holder, resolvedHolderWPoint);
            SyncHeldFrameAndPosition(holder, held, resolvedHolderWPoint, holdpoint);
            return held.Frame?.D != null;
        }

        internal bool RunStep12(
            LF2Entity holder,
            LF2Entity held,
            BattleWeaponPointValue holderWPoint,
            out WeaponActResult result)
        {
            result = default;
            if (holder?.Runtime == null || holder.PS == null || holder.Frame?.D == null)
                return false;
            if (held?.Runtime == null || held.PS == null)
                return false;

            Vector3 holdpoint = CalculateHoldPoint(holder, holderWPoint);
            if (held is LF2WeaponBase weapon)
            {
                result = weapon.Act(holder, holderWPoint, holdpoint);
                if (result.Thrown)
                    return true;

                if (holderWPoint.Kind == 3)
                    DropRandomly(holder, held);
                return true;
            }

            SyncHeldFrameAndPosition(holder, held, holderWPoint, holdpoint);

            int heldState = held.Frame?.D?.state ?? -1;
            if (heldState == LF2States.Falling || heldState == LF2States.BeingCaught)
            {
                DropFromDamagedHolder(holder, held);
                result.ForceDrop = true;
            }

            if (holderWPoint.Dvx != 0)
            {
                int heldType = held.GetCurrentDataObjectTypeForSimulation();
                if (heldType == (int)LF2ObjectType.LightWeapon ||
                    heldType == (int)LF2ObjectType.ThrowWeapon ||
                    heldType == (int)LF2ObjectType.Drink)
                {
                    held.DirectWriteHeldFramePreserveWaitCounter(40);
                    ThrowHeldObject(holder, held, holderWPoint);
                    result.Thrown = true;
                    return true;
                }

                if (heldType == (int)LF2ObjectType.HeavyWeapon)
                {
                    held.DirectWriteHeldFramePreserveWaitCounter(holder.BattleRandInt(0, 6));
                    ThrowHeldObject(holder, held, holderWPoint);
                    result.Thrown = true;
                    return true;
                }
            }

            if (holderWPoint.Kind == 3)
                DropRandomly(holder, held);

            return true;
        }

        private Vector3 CalculateHoldPoint(
            LF2Entity holder,
            BattleWeaponPointValue wpoint)
        {
            LF2FrameData frame = holder.Frame.D;
            int holderX = holder.Runtime.XInt;
            int holderY = holder.Runtime.YInt;
            int holderZ = holder.Runtime.ZInt;
            int wpointX = wpoint.X;
            int wpointY = wpoint.Y;
            float x = holder.Runtime.Dir == "right"
                ? holderX - frame.centerx + wpointX
                : holderX + frame.centerx - wpointX;
            float y = holderY - frame.centery + wpointY;
            return new Vector3(x, y, holderZ);
        }

        private void SyncHeldFrameAndPosition(
            LF2Entity holder,
            LF2Entity held,
            BattleWeaponPointValue holderWPoint,
            Vector3 holdpoint)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint.WeaponAct);
            held.SwitchDir(holder.Runtime.Dir);
            held.FrameDelay = holder.FrameDelay;

            LF2FrameData heldFrame = held.Frame?.D;
            BattleWeaponPointValue heldWPoint = heldFrame != null
                ? heldFrame.PrimaryWeaponPoint
                : default;
            int heldCenterX = heldFrame?.centerx ?? 0;
            int heldCenterY = heldFrame?.centery ?? 0;
            int heldWPointX = heldWPoint.X;
            int heldWPointY = heldWPoint.Y;

            held.Runtime.X = held.Runtime.Dir == "right"
                ? holdpoint.x + heldCenterX - heldWPointX
                : holdpoint.x + heldWPointX - heldCenterX;
            held.Runtime.Y = holdpoint.y + heldCenterY - heldWPointY;
            held.Runtime.Z = holder.Runtime.ZInt;
            held.Runtime.Zz = 0f;

            if (holderWPoint.Cover == 0)
            {
                held.Runtime.Z += 1.0;
                held.Runtime.Y -= 1.0;
            }
            else
            {
                held.Runtime.Z -= 1.0;
                held.Runtime.Y += 1.0;
            }

            held.Runtime.SyncIntegerPosition();
        }

        private void DropFromDamagedHolder(LF2Entity holder, LF2Entity held)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(holder.BattleRandInt(0, 16));
            if (holder.HitCount == 1)
            {
                held.Runtime.Vx = holder.KnockbackVx / 3.0;
                held.Runtime.Vy = holder.KnockbackVy;
                held.Runtime.Vz = holder.KnockbackVz;
            }
            else
            {
                held.Runtime.Vx = holder.Runtime.Vx / 3.0;
                held.Runtime.Vy = holder.Runtime.Vy;
                held.Runtime.Vz = holder.Runtime.Vz;
            }

            if (held.Runtime.Y < -2.0)
                held.Runtime.Y = -2.0;
            ClearLinks(holder, held);
        }

        private void ThrowHeldObject(
            LF2Entity holder,
            LF2Entity held,
            BattleWeaponPointValue wpoint)
        {
            held.Runtime.Vx = holder.Runtime.Dir == "left" ? -wpoint.Dvx : wpoint.Dvx;
            held.Runtime.Vy = wpoint.Dvy;
            held.Runtime.Vz = 0.0;
            if (holder.Runtime.KeyUp != 0 && holder.Runtime.KeyDown == 0)
                held.Runtime.Vz = -wpoint.Dvz;
            else if (holder.Runtime.KeyUp == 0 && holder.Runtime.KeyDown != 0)
                held.Runtime.Vz = wpoint.Dvz;
            held.Runtime.Zz = 0f;
            ClearLinks(holder, held, stampReleaseTick: true);
        }

        private void DropRandomly(LF2Entity holder, LF2Entity held)
        {
            if (held is LF2WeaponBase weapon)
                weapon.ReleaseHeldWeaponRuntimeInternal(holder, stampReleaseTick: true);
            else
                ClearLinks(holder, held, stampReleaseTick: true);

            held.DirectWriteHeldFramePreserveWaitCounter(holder.BattleRandInt(0, 6));
            held.Runtime.Vx = holder.BattleRandInt(0, 7) - 3;
            held.Runtime.Vy = -holder.BattleRandInt(0, 4);
            held.Runtime.Vz = (holder.BattleRandInt(0, 5) - 2) * 0.2;
            held.Runtime.Zz = 0f;
        }

        private void ClearLinks(
            LF2Entity holder,
            LF2Entity held,
            bool stampReleaseTick = false)
        {
            if (stampReleaseTick)
            {
                held.Runtime.ReleaseTick =
                    held.Match?.CurrentTickIndex ?? holder.Match?.CurrentTickIndex ?? 0;
            }

            holder.Runtime.LinkState = 0;
            if (holder.Runtime.HeldWeaponStableId == held.Runtime.SlotIndex)
            {
                holder.Runtime.HeldWeaponStableId = -1;
                holder.Runtime.ThrowFrameGuard = -1;
            }

            if (holder is LF2Character character)
                character.HeldWeaponReferenceInternal = null;

            held.GrabbedBy = 0;
            held.Runtime.LinkState = 0;
        }
    }
}
