using NTSD.Animation;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Handles the per-pass behavior of a weapon while it is held.
    /// </summary>
    internal sealed class LF2WeaponHeldStateResolver
    {
        private readonly LF2WeaponBase weapon;

        public LF2WeaponHeldStateResolver(LF2WeaponBase weapon)
        {
            this.weapon = weapon;
        }

        public WeaponActResult Act(LF2Entity holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            var result = new WeaponActResult();
            if (weapon.Frame.D == null)
                return result;

            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, result);
                if (result.ForceDrop)
                    return result;
            }

            weapon.DirectWriteHeldFramePreserveWaitCounter(wpoint.weaponact);
            weapon.SwitchDir(holder.Runtime.Dir);
            weapon.FrameDelay = holder.FrameDelay;
            weapon.Runtime.WeaponState = LF2States.WeaponOnHand;

            LF2FrameData frame = weapon.Frame.D;
            WeaponPoint heldWPoint = frame?.wpoints != null && frame.wpoints.Count > 0
                ? frame.wpoints[0]
                : null;

            ApplyHeldWPointSync(holder, wpoint, holdpoint, heldWPoint);

            int heldState = frame?.state ?? -1;
            if (heldState == LF2States.Falling || heldState == LF2States.BeingCaught)
                DropHeldWeaponFromDamagedFrame(holder, result);

            if (wpoint.dvx != 0)
            {
                int weaponType = weapon.WeaponType;
                bool isHeavyThrow = weaponType == 1 || weaponType == 4 || weaponType == 6;
                bool isLightThrow = weaponType == 2;

                if (isHeavyThrow)
                {
                    weapon.DirectWriteHeldFramePreserveWaitCounter(40);
                    ThrowHeldWeapon(holder, wpoint);
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    weapon.DirectWriteHeldFramePreserveWaitCounter(weapon.BattleRandInt(0, 6));
                    weapon.FrameDelay = 1;
                    ThrowHeldWeapon(holder, wpoint);
                    result.Thrown = true;
                }
                else
                {
                    result.NeedsKind3Drop = true;
                    return result;
                }
            }

            return result;
        }

        private void ThrowHeldWeapon(LF2Entity holder, WeaponPoint wpoint)
        {
            weapon.Runtime.WeaponState = LF2States.WeaponThrowing;
            weapon.Runtime.Vx = weapon.Dirh() * wpoint.dvx;
            weapon.Runtime.Vy = wpoint.dvy;

            if (wpoint.dvz != 0)
            {
                bool keyUp = holder.Runtime.KeyUp != 0;
                bool keyDown = holder.Runtime.KeyDown != 0;
                if (keyUp && !keyDown)
                    weapon.Runtime.Vz = -wpoint.dvz;
                else if (!keyUp && keyDown)
                    weapon.Runtime.Vz = wpoint.dvz;
            }

            weapon.SpawnerEntityIndex = holder.Runtime?.SlotIndex ?? -1;
            weapon.PS.zz = 0;
            weapon.ReleaseHeldWeaponRuntimeInternal(holder, stampReleaseTick: true);
            weapon.PickerStableId = holder.Runtime?.SlotIndex ?? -1;
            weapon.OnThrownInternal();
        }

        private void DropHeldWeaponFromDamagedFrame(LF2Entity holder, WeaponActResult result)
        {
            if (holder?.PS == null || weapon.PS == null)
                return;

            weapon.DirectWriteHeldFramePreserveWaitCounter(weapon.BattleRandInt(0, 16));
            weapon.Runtime.WeaponState = 0;

            const double velocityFactor = 1.0 / 3.0;
            if (holder.HitCount == 1)
            {
                weapon.Runtime.Vx = holder.KnockbackVx * velocityFactor;
                weapon.Runtime.Vy = holder.KnockbackVy;
                weapon.Runtime.Vz = holder.KnockbackVz;
            }
            else
            {
                weapon.Runtime.Vx = holder.Runtime.Vx * velocityFactor;
                weapon.Runtime.Vy = holder.Runtime.Vy;
                weapon.Runtime.Vz = holder.Runtime.Vz;
            }

            if (weapon.Runtime.Y < -2.0)
                weapon.Runtime.Y = -2.0;

            if (holder is LF2Character holderCharacter)
                holderCharacter.GrabbedBy = 0;

            weapon.ReleaseHeldWeaponRuntimeInternal(holder);
            result.ForceDrop = true;
        }

        public void ApplyHeldWPointSync(
            LF2Entity holder,
            WeaponPoint holderWPoint,
            Vector3 holdpoint,
            WeaponPoint heldWPoint)
        {
            if (holder?.PS == null || weapon.PS == null)
                return;

            int cover = holderWPoint.cover;
            // C# WeaponRuntime uses held Z directly for cover ordering. A second zz
            // offset would cancel that Z delta in Unity's sorting order.
            weapon.PS.zz = 0f;

            int holderZ = holder.Runtime != null ? holder.Runtime.ZInt : (int)holder.PS.z;
            weapon.Runtime.Z = holderZ;
            weapon.PS.sz = holderZ;

            weapon.CoincideXYWithWPointInternal(holdpoint, heldWPoint);

            if (cover == 0)
            {
                weapon.Runtime.Z += 1.0;
                weapon.Runtime.Y -= 1.0;
            }
            else
            {
                weapon.Runtime.Z -= 1.0;
                weapon.Runtime.Y += 1.0;
            }

            weapon.Runtime.SyncIntegerPosition();
        }

        public void ProcessDrinkConsumption(LF2Entity holder, WeaponActResult result)
        {
            if (holder?.Health == null)
                return;

            LF2CharacterData charData = CharacterAnimtorManager.Instance?.GetCharacterData(weapon.ObjectId);
            int typeSub = charData?.type_sub ?? 0;

            if (typeSub == 0x7A)
            {
                if (weapon.Health.HP <= 0)
                    return;

                weapon.Health.HP--;
                if (weapon.Health.HP % 5 == 0)
                {
                    holder.Health.HPBound += 2;
                    holder.Health.HP += 4;
                    if (holder.Health.HPBound > holder.Health.HP3)
                        holder.Health.HPBound = holder.Health.HP3;
                    if (holder.Health.HP > holder.Health.HPBound)
                        holder.Health.HP = holder.Health.HPBound;
                }

                if (weapon.Health.HP % 6 == 0)
                {
                    holder.Health.PP += 5;
                    if (holder.Health.PP > NTSDGlobal.Gameplay.DrinkPPCap)
                        holder.Health.PP = NTSDGlobal.Gameplay.DrinkPPCap;
                }
            }
            else if (typeSub == 0x7B)
            {
                if (weapon.Health.HP <= 0)
                    return;

                weapon.Health.HP -= 2;
                holder.Health.PP += 3;
                if (holder.Health.PP > NTSDGlobal.Gameplay.DrinkPPCap)
                    holder.Health.PP = NTSDGlobal.Gameplay.DrinkPPCap;

                if (weapon.KillCount > -1 && weapon.Health.PP > NTSDGlobal.Gameplay.PpRecoverLowLimit)
                    holder.Health.PP = NTSDGlobal.Gameplay.PpRecoverLowLimit;
            }
            else
            {
                return;
            }

            if (weapon.Health.HP > 0)
                return;

            if (holder is LF2Character holderCharacter)
                holderCharacter.GrabbedBy = 0;

            weapon.DirectWriteHeldFramePreserveWaitCounter(0);
            weapon.Runtime.Vx = weapon.BattleRandInt(0, 7) - 3;
            weapon.Runtime.Vy = -8.0;
            weapon.Runtime.Vz = 0.0;
            weapon.PS.zz = 0;
            holder.DirectWriteHeldFramePreserveWaitCounter(0);
            weapon.OnDrinkConsumedInternal();
            weapon.ReleaseHeldWeaponForConsumeInternal(holder);
            result.ForceDrop = true;
        }
    }

    internal static class LF2HeldObjectRuntime
    {
        public static bool SyncHeldPose(
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
                : new WeaponPoint();

            Vector3 holdpoint = CalculateHoldPoint(holder, holderWPoint);
            SyncHeldFrameAndPosition(holder, held, holderWPoint, holdpoint);
            return held.Frame?.D != null;
        }

        public static bool RunStep12(
            LF2Entity holder,
            LF2Entity held,
            WeaponPoint holderWPoint,
            out WeaponActResult result)
        {
            result = new WeaponActResult();
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
                    held.FrameDelay = 1;
                    ThrowHeldObject(holder, held, holderWPoint);
                    result.Thrown = true;
                    return true;
                }
            }

            if (holderWPoint.kind == 3)
                DropRandomly(holder, held);

            return true;
        }

        private static Vector3 CalculateHoldPoint(LF2Entity holder, WeaponPoint wpoint)
        {
            LF2FrameData frame = holder.Frame.D;
            int holderX = holder.Runtime.XInt;
            int holderY = holder.Runtime.YInt;
            int holderZ = holder.Runtime.ZInt;
            float x = holder.Runtime.Dir == "right"
                ? holderX - frame.centerx + wpoint.x
                : holderX + frame.centerx - wpoint.x;
            float y = holderY - frame.centery + wpoint.y;
            return new Vector3(x, y, holderZ);
        }

        private static void SyncHeldFrameAndPosition(
            LF2Entity holder,
            LF2Entity held,
            WeaponPoint holderWPoint,
            Vector3 holdpoint)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint.weaponact);
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

            if (holderWPoint.cover == 0)
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

        private static void DropFromDamagedHolder(LF2Entity holder, LF2Entity held)
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

        private static void ThrowHeldObject(LF2Entity holder, LF2Entity held, WeaponPoint wpoint)
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

        private static void DropRandomly(LF2Entity holder, LF2Entity held)
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

        private static void ClearLinks(
            LF2Entity holder,
            LF2Entity held,
            bool stampReleaseTick = false)
        {
            if (stampReleaseTick)
                held.Runtime.ReleaseTick = held.Match?.CurrentTickIndex ?? holder.Match?.CurrentTickIndex ?? 0;

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
