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
            WeaponPoint holderWPoint = null)
        {
            if (holder?.Runtime == null || holder.PS == null || holder.Frame?.D == null)
                return false;
            if (held?.Runtime == null || held.PS == null)
                return false;

            LF2FrameData holderFrame = holder.Frame.D;
            holderWPoint ??= holderFrame.wpoints != null && holderFrame.wpoints.Count > 0
                ? holderFrame.wpoints[0]
                : null;

            Vector3 holdpoint = CalculateHoldPoint(holder, holderWPoint);
            SyncHeldFrameAndPosition(holder, held, holderWPoint, holdpoint);
            return held.Frame?.D != null;
        }

        internal bool RunStep12(
            LF2Entity holder,
            LF2Entity held,
            WeaponPoint holderWPoint,
            out WeaponActResult result)
        {
            result = default;
            if (holder?.Runtime == null || holder.PS == null || holder.Frame?.D == null)
                return false;
            if (held?.Runtime == null || held.PS == null || holderWPoint == null)
                return false;

            Vector3 holdpoint = CalculateHoldPoint(holder, holderWPoint);
            if (held is LF2WeaponBase weapon)
            {
                result = weapon.Act(holder, holderWPoint, holdpoint);
                if (result.Thrown)
                    return true;

                if (holderWPoint.kind == 3)
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

            if (holderWPoint.dvx != 0)
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

            if (holderWPoint.kind == 3)
                DropRandomly(holder, held);

            return true;
        }

        private Vector3 CalculateHoldPoint(LF2Entity holder, WeaponPoint wpoint)
        {
            LF2FrameData frame = holder.Frame.D;
            int holderX = holder.Runtime.XInt;
            int holderY = holder.Runtime.YInt;
            int holderZ = holder.Runtime.ZInt;
            int wpointX = wpoint?.x ?? 0;
            int wpointY = wpoint?.y ?? 0;
            float x = holder.Runtime.Dir == "right"
                ? holderX - frame.centerx + wpointX
                : holderX + frame.centerx - wpointX;
            float y = holderY - frame.centery + wpointY;
            return new Vector3(x, y, holderZ);
        }

        private void SyncHeldFrameAndPosition(
            LF2Entity holder,
            LF2Entity held,
            WeaponPoint holderWPoint,
            Vector3 holdpoint)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint?.weaponact ?? 0);
            held.SwitchDir(holder.Runtime.Dir);
            held.FrameDelay = holder.FrameDelay;

            LF2FrameData heldFrame = held.Frame?.D;
            WeaponPoint heldWPoint = heldFrame?.wpoints != null && heldFrame.wpoints.Count > 0
                ? heldFrame.wpoints[0]
                : null;
            int heldCenterX = heldFrame?.centerx ?? 0;
            int heldCenterY = heldFrame?.centery ?? 0;
            int heldWPointX = heldWPoint?.x ?? 0;
            int heldWPointY = heldWPoint?.y ?? 0;

            held.Runtime.X = held.Runtime.Dir == "right"
                ? holdpoint.x + heldCenterX - heldWPointX
                : holdpoint.x + heldWPointX - heldCenterX;
            held.Runtime.Y = holdpoint.y + heldCenterY - heldWPointY;
            held.Runtime.Z = holder.Runtime.ZInt;
            held.Runtime.Zz = 0f;

            if ((holderWPoint?.cover ?? 0) == 0)
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

        private void ThrowHeldObject(LF2Entity holder, LF2Entity held, WeaponPoint wpoint)
        {
            held.Runtime.Vx = holder.Runtime.Dir == "left" ? -wpoint.dvx : wpoint.dvx;
            held.Runtime.Vy = wpoint.dvy;
            held.Runtime.Vz = 0.0;
            if (holder.Runtime.KeyUp != 0 && holder.Runtime.KeyDown == 0)
                held.Runtime.Vz = -wpoint.dvz;
            else if (holder.Runtime.KeyUp == 0 && holder.Runtime.KeyDown != 0)
                held.Runtime.Vz = wpoint.dvz;
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
