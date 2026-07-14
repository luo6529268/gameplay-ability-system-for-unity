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

            weapon.FrameDelay = holder.FrameDelay;
            weapon.ImmediateFrame(wpoint.weaponact);
            weapon.Runtime.WeaponState = LF2States.WeaponOnHand;

            LF2FrameData frame = weapon.Frame.D;
            WeaponPoint heldWPoint = frame?.wpoints != null && frame.wpoints.Count > 0
                ? frame.wpoints[0]
                : null;

            ApplyHeldWPointSync(holder, wpoint, holdpoint, heldWPoint);

            int heldState = frame?.state ?? -1;
            if (heldState == LF2States.Falling || heldState == LF2States.BeingCaught)
            {
                DropHeldWeaponFromDamagedFrame(holder, result);
                return result;
            }

            if (wpoint.dvx != 0)
            {
                int weaponType = weapon.WeaponType;
                bool isHeavyThrow = weaponType == 1 || weaponType == 4 || weaponType == 6;
                bool isLightThrow = weaponType == 2;

                if (isHeavyThrow)
                {
                    weapon.ImmediateFrame(40);
                    ThrowHeldWeapon(holder, wpoint);
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    weapon.ImmediateFrame(weapon.BattleRandInt(0, 6));
                    ThrowHeldWeapon(holder, wpoint);
                    result.Thrown = true;
                }
                else
                {
                    result.NeedsKind3Drop = true;
                    return result;
                }
            }

            int runtimeWeaponState = weapon.GetRuntimeWeaponState();
            if (runtimeWeaponState == LF2States.WeaponOnHand && weapon.IsLight && wpoint.attacking > 0)
                result.AttackResult = weapon.ProcessAttackInternal(holder, wpoint, frame);

            return result;
        }

        private void ThrowHeldWeapon(LF2Entity holder, WeaponPoint wpoint)
        {
            weapon.Runtime.WeaponState = LF2States.WeaponThrowing;
            weapon.Runtime.Vx = weapon.Dirh() * wpoint.dvx;
            weapon.Runtime.Vy = wpoint.dvy;

            if (wpoint.dvz != 0)
            {
                bool keyUp = (holder as LF2LivingObject)?.Controller?.IsUp ?? false;
                bool keyDown = (holder as LF2LivingObject)?.Controller?.IsDown ?? false;
                if (keyUp && !keyDown)
                    weapon.Runtime.Vz = -wpoint.dvz;
                else if (!keyUp && keyDown)
                    weapon.Runtime.Vz = wpoint.dvz;
            }

            weapon.ItrRest.Arest = 0;
            holder.ItrRest.Arest = 0;
            weapon.SpawnerEntityIndex = holder.Runtime?.SlotIndex ?? -1;
            weapon.PS.zz = 1;
            weapon.ReleaseHeldWeaponRuntimeInternal(holder);
            weapon.PickerStableId = holder.Runtime?.SlotIndex ?? -1;
            weapon.OnThrownInternal();
        }

        private void DropHeldWeaponFromDamagedFrame(LF2Entity holder, WeaponActResult result)
        {
            if (holder?.PS == null || weapon.PS == null)
                return;

            weapon.ItrRest.Arest = 0;
            holder.ItrRest.Arest = 0;
            weapon.ImmediateFrame(weapon.BattleRandInt(0, 16));
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

            int cover = holderWPoint.cover != 0 ? holderWPoint.cover : NTSDGlobal.Default.WPoint.Cover;
            int coverDiv = cover / 10;
            int coverRem = cover % 10;
            weapon.PS.zz = coverRem != 0 ? -1 : 1;

            weapon.SwitchDir(holder.PS.dir);
            int holderZ = holder.Runtime != null ? holder.Runtime.ZInt : (int)holder.PS.z;
            weapon.Runtime.Z = holderZ;
            weapon.PS.sz = holderZ;

            weapon.CoincideXYWithWPointInternal(holdpoint, heldWPoint);

            if (coverRem != 0)
            {
                weapon.Runtime.Z += 1.0;
                weapon.Runtime.Y -= 1.0;
            }
            else
            {
                weapon.Runtime.Z -= 1.0;
                weapon.Runtime.Y += 1.0;
            }

            if (coverDiv == 1)
                weapon.SwitchDir(holder.PS.dir);
            else if (coverDiv == 2)
                weapon.SwitchDir(holder.PS.dir == "right" ? "left" : "right");
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

            weapon.ImmediateFrame(0);
            weapon.Runtime.Vx = weapon.BattleRandInt(0, 7) - 3;
            weapon.Runtime.Vy = -8.0;
            weapon.Runtime.Vz = 0.0;
            weapon.PS.zz = 0;
            holder.ImmediateFrame(0);
            weapon.OnDrinkConsumedInternal();
            weapon.ReleaseHeldWeaponForConsumeInternal(holder);
            result.ForceDrop = true;
        }
    }
}
