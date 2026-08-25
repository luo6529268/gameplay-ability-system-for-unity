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

        public void Drop(double dvx, double dvy)
        {
            LF2Entity holder = weapon.ResolveRuntimeHolderEntityForOwnedModule();
            weapon.Team = 0;
            weapon.ForceClearHolder();

            if (holder?.Runtime != null)
            {
                holder.Runtime.LinkState = 0;
                holder.Runtime.TargetSlotIndex = -1;
                holder.Runtime.HeldWeaponStableId = -1;
                holder.Runtime.ThrowFrameGuard = -1;
            }

            weapon.Runtime.WeaponState = 0;
            weapon.ImmediateFrame(weapon.BattleRandInt(0, 16));
            weapon.Runtime.WeaponState = 0;
            weapon.Runtime.Vx = dvx * (1.0 / 3.0);
            weapon.Runtime.Vy = dvy;

            if (weapon.Runtime.Y < -2.0)
                weapon.Runtime.Y = -2.0;

            weapon.Runtime.Zz = 0f;
            weapon.PS.zz = 0;
        }

        public WeaponActResult Act(LF2Entity holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            WeaponActResult result = default;
            if (weapon.Frame.D == null)
                return result;

            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, ref result);
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
                DropHeldWeaponFromDamagedFrame(holder, ref result);

            if (wpoint.dvx != 0)
            {
                int weaponType = weapon.WeaponType;
                bool isHeavyThrow = weaponType == 1 || weaponType == 4 || weaponType == 6;
                bool isLightThrow = weaponType == 2;

                if (isHeavyThrow)
                {
                    weapon.DirectWriteHeldFramePreserveWaitCounter(40);
                    ThrowHeldWeapon(holder, wpoint, stampSpawnerSlot: true);
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    weapon.DirectWriteHeldFramePreserveWaitCounter(weapon.BattleRandInt(0, 6));
                    ThrowHeldWeapon(holder, wpoint, stampSpawnerSlot: false);
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

        private void ThrowHeldWeapon(LF2Entity holder, WeaponPoint wpoint, bool stampSpawnerSlot)
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

            if (stampSpawnerSlot)
                weapon.SpawnerEntityIndex = holder.Runtime?.SlotIndex ?? -1;
            weapon.PS.zz = 0;
            weapon.ReleaseHeldWeaponRuntimeInternal(holder, stampReleaseTick: true);
            weapon.OnThrownInternal();
        }

        private void DropHeldWeaponFromDamagedFrame(
            LF2Entity holder,
            ref WeaponActResult result)
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

        public void ProcessDrinkConsumption(
            LF2Entity holder,
            ref WeaponActResult result)
        {
            if (holder?.Health == null)
                return;

            LF2CharacterData charData =
                weapon.ResolveRuntimeCharacterData(weapon.ObjectId);
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

}
