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
            held.DirectWriteHeldFramePreserveWaitCounter(resolvedHolderWPoint.WeaponAct);
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
                if (result.TerminalDespawnRequested || result.RefillExhausted || result.UnsupportedWeaponAction ||
                    (result.Thrown && holderWPoint.Kind != 3))
                    return true;

                if (holderWPoint.Kind == 3)
                    DropRandomly(holder, held, holderWPoint);
                return true;
            }

            if (holderWPoint.WeaponAct >= 1000)
            {
                result.TerminalDespawnRequested = true;
                return true;
            }

            // Alignment contract: NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001.
            held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint.WeaponAct);
            if (held.FrameCache?.HasFrame(holderWPoint.WeaponAct) != true)
            {
                result.UnsupportedWeaponAction = true;
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
                    if (holderWPoint.Kind != 3)
                        return true;
                }

                if (heldType == (int)LF2ObjectType.HeavyWeapon)
                {
                    held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint.Kind == 3
                        ? holder.Match.NativeRandom.SynchronizedNext(0x0041865E, 6)
                        : holder.BattleRandInt(0, 6));
                    ThrowHeldObject(holder, held, holderWPoint);
                    result.Thrown = true;
                    if (holderWPoint.Kind != 3)
                        return true;
                }
            }

            if (holderWPoint.Kind == 3)
                DropRandomly(holder, held, holderWPoint);

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
            ClearLinks(holder, held);
        }

        private void DropRandomly(
            LF2Entity holder,
            LF2Entity held,
            BattleWeaponPointValue wpoint)
        {
            if (held is LF2WeaponBase weapon)
                weapon.ReleaseHeldWeaponRuntimeInternal(holder);
            else
                ClearLinks(holder, held);

            // Alignment contract: NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001.
            var random = holder.Match.NativeRandom;
            held.DirectWriteHeldFramePreserveWaitCounter(random.SynchronizedNext(0x00418726, 6));
            int randomX = random.SynchronizedNext(0x0041873A, 7) - 3;
            int randomY = -random.SynchronizedNext(0x00418756, 4);
            int randomZ = random.SynchronizedNext(0x00418772, 5) - 2;
            held.Runtime.Vx = wpoint.Dvx != 0 ? wpoint.Dvx : randomX;
            held.Runtime.Vy = wpoint.Dvy != 0 ? wpoint.Dvy : randomY;
            held.Runtime.Vz = wpoint.Dvz != 0 ? wpoint.Dvz : randomZ;
            held.Runtime.Zz = 0f;
        }

        private void ClearLinks(
            LF2Entity holder,
            LF2Entity held)
        {
            holder.Runtime.LinkState = 0;
            if (holder.Runtime.HeldWeaponStableId == held.Runtime.SlotIndex)
            {
                holder.Runtime.HeldWeaponStableId = -1;
                holder.Runtime.ThrowFrameGuard = -1;
            }

            if (holder is LF2Character character)
                character.HeldWeaponReferenceInternal = null;

            held.Runtime.LinkState = 0;
        }
    }
}
