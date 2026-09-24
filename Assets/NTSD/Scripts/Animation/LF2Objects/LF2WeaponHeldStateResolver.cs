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

            weapon.ImmediateFrame(weapon.BattleRandInt(0, 16));
            weapon.Runtime.Vx = dvx * (1.0 / 3.0);
            weapon.Runtime.Vy = dvy;

            if (weapon.Runtime.Y < -2.0)
                weapon.Runtime.Y = -2.0;

            weapon.Runtime.Zz = 0f;
            weapon.PS.zz = 0;
        }

        public WeaponActResult Act(
            LF2Entity holder,
            BattleWeaponPointValue wpoint,
            Vector3 holdpoint)
        {
            WeaponActResult result = default;
            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, ref result);
                if (result.ForceDrop)
                    return result;
            }

            // Alignment contract: NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001.
            if (wpoint.WeaponAct >= 1000)
            {
                result.TerminalDespawnRequested = true;
                return result;
            }

            weapon.DirectWriteNativeRawFramePreserveWaitCounter(wpoint.WeaponAct);
            // Alignment contract: NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001.
            if (weapon.FrameCache?.HasNativeFrame(wpoint.WeaponAct) != true)
            {
                result.UnsupportedWeaponAction = true;
                return result;
            }
            weapon.SwitchDir(holder.Runtime.Dir);
            weapon.FrameDelay = holder.FrameDelay;
            LF2FrameData frame = weapon.Frame.D;
            BattleWeaponPointValue heldWPoint = frame != null
                ? frame.PrimaryWeaponPoint
                : default;

            ApplyHeldWPointSync(holder, wpoint, holdpoint, heldWPoint);

            int heldState = frame?.state ?? -1;
            if (heldState == LF2States.Falling || heldState == LF2States.BeingCaught)
                DropHeldWeaponFromDamagedFrame(holder, ref result);

            if (wpoint.Dvx != 0)
            {
                int weaponType = weapon.WeaponType;
                bool isHeavyThrow = weaponType == 1 || weaponType == 4 || weaponType == 6;
                bool isLightThrow = weaponType == 2;

                if (isHeavyThrow)
                {
                    weapon.DirectWriteNativeRawFramePreserveWaitCounter(40);
                    ThrowHeldWeapon(holder, wpoint, stampAiExclusionSourceSlot: true);
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    weapon.DirectWriteNativeRawFramePreserveWaitCounter(
                        holder.Match.NativeRandom.SynchronizedNext(0x0041865E, 6));
                    ThrowHeldWeapon(holder, wpoint, stampAiExclusionSourceSlot: false);
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

        private void ThrowHeldWeapon(
            LF2Entity holder,
            BattleWeaponPointValue wpoint,
            bool stampAiExclusionSourceSlot)
        {
            weapon.Runtime.Vx = weapon.Dirh() * wpoint.Dvx;
            weapon.Runtime.Vy = wpoint.Dvy;

            bool keyUp = holder.Runtime.KeyUp != 0;
            bool keyDown = holder.Runtime.KeyDown != 0;
            if (keyUp && !keyDown)
                weapon.Runtime.Vz = -wpoint.Dvz;
            else if (!keyUp && keyDown)
                weapon.Runtime.Vz = wpoint.Dvz;

            if (stampAiExclusionSourceSlot)
                weapon.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = holder.Runtime.SlotIndex;
            weapon.PS.zz = 0;
            weapon.ReleaseHeldWeaponRuntimeInternal(holder);
            // Alignment contract: NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001.
            if (wpoint.Kind == 3)
                weapon.OnThrownInternal();
        }

        private void DropHeldWeaponFromDamagedFrame(
            LF2Entity holder,
            ref WeaponActResult result)
        {
            if (holder?.PS == null || weapon.PS == null)
                return;

            weapon.DirectWriteHeldFramePreserveWaitCounter(weapon.BattleRandInt(0, 16));
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

            weapon.ReleaseHeldWeaponRuntimeInternal(holder);
            result.ForceDrop = true;
        }

        public void ApplyHeldWPointSync(
            LF2Entity holder,
            BattleWeaponPointValue holderWPoint,
            Vector3 holdpoint,
            BattleWeaponPointValue heldWPoint)
        {
            if (holder?.PS == null || weapon.PS == null)
                return;

            int cover = holderWPoint.Cover;
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

            if (holder.Runtime.SourceRulePositionInitialized &&
                weapon.Runtime.SourceRulePositionInitialized)
            {
                // Alignment contract: NTSD28-USER-SOURCE-WPOINT-HELD-POSITION-001.
                LF2FrameData holderFrame = holder.Frame.D;
                LF2FrameData heldFrame = weapon.Frame.D;
                int sourceAnchorX = holder.Runtime.Dir == "right"
                    ? holder.Runtime.SourceRuleXInt - holderFrame.centerx + holderWPoint.X
                    : holder.Runtime.SourceRuleXInt + holderFrame.centerx - holderWPoint.X;
                int sourceX = weapon.Runtime.Dir == "right"
                    ? sourceAnchorX + (heldFrame?.centerx ?? 0) - heldWPoint.X
                    : sourceAnchorX + heldWPoint.X - (heldFrame?.centerx ?? 0);
                int sourceZ = holder.Runtime.SourceRuleZInt;
                if (cover != 2)
                    sourceZ += cover == 0 ? 1 : -1;
                weapon.Runtime.SourceRuleX = sourceX;
                weapon.Runtime.SourceRuleZ = sourceZ;
                weapon.Runtime.SyncSourceRuleIntegerPosition();
            }

            weapon.Runtime.SyncIntegerPosition();
        }

        public void ProcessDrinkConsumption(
            LF2Entity holder,
            ref WeaponActResult result)
        {
            ProcessNativeRefillConsumption(holder, weapon, ref result);
        }

        internal static void ProcessNativeRefillConsumption(
            LF2Entity holder,
            LF2Entity held,
            ref WeaponActResult result)
        {
            if (holder?.Frame?.D?.state != 17 || held?.Runtime == null)
                return;
            bool hpRefill = held.ObjectId == 122;
            bool mpRefill = held.ObjectId == 123;
            if (!hpRefill && !mpRefill)
                return;
            var world = holder.Match;
            if (world == null || held.Match != world)
                throw new System.InvalidOperationException("Native held refill requires a shared registered world.");

            var parent = holder.Runtime;
            var child = held.Runtime;
            if (hpRefill)
            {
                if (child.HP <= 0)
                    return;
                child.HP--;
                if (child.HP % 5 == 0)
                {
                    parent.HPBound = System.Math.Min(parent.HPBound + 2, parent.HP3);
                    parent.HP = System.Math.Min(parent.HP + 4, parent.HPBound);
                }
                if (child.HP % 6 == 0)
                    parent.PP = System.Math.Min(parent.PP + 5, 500);
            }
            else
            {
                child.HP -= 2;
                parent.PP = System.Math.Min(parent.PP + 3, 500);
                if (child.OrdinaryCreditGate2F4 >= 0 && child.PP > 150)
                    child.PP = 150;
            }
            if (child.HP > 0)
                return;

            // Alignment contract: NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001.
            if (held is LF2WeaponBase heldWeapon)
            {
                heldWeapon.ReleaseHeldWeaponForConsumeInternal(holder);
                heldWeapon.PS.zz = 0;
            }
            else
            {
                parent.LinkState = 0;
                parent.TargetSlotIndex = 0;
                child.LinkState = 0;
                child.HolderStableId = 0;
                if (parent.HeldWeaponStableId == child.SlotIndex)
                {
                    parent.HeldWeaponStableId = -1;
                    parent.ThrowFrameGuard = -1;
                }
                if (holder is LF2Character character)
                    character.HeldWeaponReferenceInternal = null;
            }
            held.DirectWriteNativeRawFramePreserveWaitCounter(0);
            held.AttackingCounter = 0;
            child.Vy = 0;
            child.Vx = world.NativeRandom.SynchronizedNext(hpRefill ? 0x004181C9u : 0x004182C0u, 7) - 3;
            holder.DirectWriteNativeRawFramePreserveWaitCounter(0);
            holder.AttackingCounter = 0;
            child.WeaponFlightCounter = 0;
            if (held is LF2WeaponBase consumedWeapon)
                consumedWeapon.OnDrinkConsumedInternal();
            result.ForceDrop = true;
            result.RefillExhausted = true;
        }
    }
}
