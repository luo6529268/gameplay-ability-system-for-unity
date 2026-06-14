using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        public override float GetSpriteWidthPxForCollision()
        {
            return ResolveCurrentSpriteFileWidthPx();
        }

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;

        public virtual WeaponActResult Act(LF2LivingObject holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            var result = new WeaponActResult();
            if (Frame.D == null) return result;

            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, result);
                if (result.ForceDrop)
                    return result;
            }

            FrameDelay = holder.FrameDelay;
            ImmediateFrame(wpoint.weaponact);
            Runtime.WeaponState = LF2States.WeaponOnHand;

            var fD = Frame.D;
            var fwpoint = fD?.wpoints != null && fD.wpoints.Count > 0
                ? fD.wpoints[0]
                : null;

            ApplyHeldWPointSync(holder, wpoint, holdpoint, fwpoint);

            int heldState = fD?.state ?? -1;
            if (heldState == LF2States.Falling || heldState == LF2States.BeingCaught)
            {
                DropHeldWeaponFromDamagedFrame(holder, result);
                return result;
            }

            if (wpoint.dvx != 0)
            {
                int wt = WeaponType;
                bool isHeavyThrow = wt == 1 || wt == 4 || wt == 6;
                bool isLightThrow = wt == 2;

                if (isHeavyThrow)
                {
                    ImmediateFrame(40);
                    Runtime.WeaponState = LF2States.WeaponThrowing;
                    PS.vx = Dirh() * wpoint.dvx;
                    PS.vy = wpoint.dvy;
                    if (wpoint.dvz != 0)
                    {
                        bool keyUp = holder.Controller?.IsUp ?? false;
                        bool keyDown = holder.Controller?.IsDown ?? false;
                        if (keyUp && !keyDown) PS.vz = -wpoint.dvz;
                        else if (!keyUp && keyDown) PS.vz = wpoint.dvz;
                    }
                    ItrRest.Arest = 0;
                    holder.ItrRest.Arest = 0;
                    SpawnerEntityIndex = (holder as LF2Entity)?.Runtime?.SlotIndex ?? -1;
                    PS.zz = 1;
                    _holdObj = null;
                    (holder as LF2Character)?.HoldWeapon(null);
                    GrabbedBy = 0;
                    Runtime.LinkState = 0;
                    Runtime.HolderStableId = -1;
                    PickerStableId = (holder as LF2Entity)?.Runtime?.SlotIndex ?? -1;
                    OnThrown();
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    ImmediateFrame(RandInt(0, 6));
                    Runtime.WeaponState = LF2States.WeaponThrowing;
                    PS.vx = Dirh() * wpoint.dvx;
                    PS.vy = wpoint.dvy;
                    if (wpoint.dvz != 0)
                    {
                        bool keyUp = holder.Controller?.IsUp ?? false;
                        bool keyDown = holder.Controller?.IsDown ?? false;
                        if (keyUp && !keyDown) PS.vz = -wpoint.dvz;
                        else if (!keyUp && keyDown) PS.vz = wpoint.dvz;
                    }
                    ItrRest.Arest = 0;
                    holder.ItrRest.Arest = 0;
                    SpawnerEntityIndex = (holder as LF2Entity)?.Runtime?.SlotIndex ?? -1;
                    PS.zz = 1;
                    _holdObj = null;
                    (holder as LF2Character)?.HoldWeapon(null);
                    GrabbedBy = 0;
                    Runtime.LinkState = 0;
                    Runtime.HolderStableId = -1;
                    PickerStableId = (holder as LF2Entity)?.Runtime?.SlotIndex ?? -1;
                    OnThrown();
                    result.Thrown = true;
                }
                else
                {
                    result.NeedsKind3Drop = true;
                    return result;
                }
            }

            int runtimeWeaponState = ResolveRuntimeWeaponState();

            if (runtimeWeaponState == LF2States.WeaponOnHand && IsLight && wpoint.attacking > 0)
            {
                result.AttackResult = ProcessAttack(holder, wpoint, fD);
            }

            return result;
        }

        private void DropHeldWeaponFromDamagedFrame(LF2LivingObject holder, WeaponActResult result)
        {
            if (holder?.PS == null || PS == null)
                return;

            ItrRest.Arest = 0;
            holder.ItrRest.Arest = 0;

            ImmediateFrame(RandInt(0, 16));
            Runtime.WeaponState = 0;

            const float kVelFactor = 1f / 3f;
            if (holder.HitCount == 1)
            {
                PS.vx = holder.KnockbackVx * kVelFactor;
                PS.vy = holder.KnockbackVy;
                PS.vz = holder.KnockbackVz;
            }
            else
            {
                PS.vx = holder.PS.vx * kVelFactor;
                PS.vy = holder.PS.vy;
                PS.vz = holder.PS.vz;
            }

            if (PS.y < -2.0f)
                PS.y = -2.0f;

            GrabbedBy = 0;
            if (holder is LF2Character holderCharacter)
                holderCharacter.GrabbedBy = 0;

            _holdObj = null;
            (holder as LF2Character)?.HoldWeapon(null);
            Runtime.LinkState = 0;
            Runtime.HolderStableId = -1;
            result.ForceDrop = true;
        }

        private void ApplyHeldWPointSync(LF2LivingObject holder, WeaponPoint holderWPoint, Vector3 holdpoint, WeaponPoint heldWPoint)
        {
            if (holder?.PS == null || PS == null)
                return;

            int cover = holderWPoint.cover != 0 ? holderWPoint.cover : NTSDGlobal.Default.WPoint.Cover;
            int coverDiv = cover / 10;
            int coverRem = cover % 10;
            PS.zz = (coverRem != 0) ? -1 : 1;

            SwitchDir(holder.PS.dir);
            int holderZ = holder.Runtime != null ? holder.Runtime.ZInt : Mathf.RoundToInt(holder.PS.z);
            PS.sz = PS.z = holderZ;

            CoincideXYWithWPoint(holdpoint, heldWPoint);

            if (coverRem != 0)
            {
                PS.z += 1f;
                PS.y -= 1f;
            }
            else
            {
                PS.z -= 1f;
                PS.y += 1f;
            }

            if (coverDiv == 1)
                SwitchDir(holder.PS.dir);
            else if (coverDiv == 2)
                SwitchDir(holder.PS.dir == "right" ? "left" : "right");
        }

        public void ForceClearHolder(bool preserveRuntimeOwnerFields = false)
        {
            _holdObj = null;
            GrabbedBy = 0;
            Runtime.LinkState = 0;
            if (!preserveRuntimeOwnerFields)
            {
                Runtime.HolderStableId = -1;
                HolderCopySlot = -1;
            }
        }

        public virtual void Drop(float dvx, float dvy)
        {
            Team = 0;
            _holdObj = null;
            Runtime.HolderStableId = -1;
            Runtime.LinkState = 0;
            Runtime.WeaponState = 0;
            GrabbedBy = 0;

            ImmediateFrame(RandInt(0, 16));
            Runtime.WeaponState = 0;
            PS.vx = dvx * (1f / 3f);
            PS.vy = dvy;

            if (PS.y < -2.0f) PS.y = -2.0f;

            PS.zz = 0;
        }

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();
            Runtime.HolderStableId = (_holdObj as LF2Entity)?.Runtime?.SlotIndex ?? -1;
            Runtime.PickerStableId = PickerStableId;
            Runtime.WeaponDropHurt = WeaponDropHurt;
        }

        protected virtual void ProcessDrinkConsumption(LF2LivingObject holder, WeaponActResult result)
        {
            if (holder?.Health == null) return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int typeSub = charData?.type_sub ?? 0;

            if (typeSub == 0x7A)
            {
                if (Health.HP <= 0) return;

                Health.HP--;

                if (Health.HP % 5 == 0)
                {
                    holder.Health.HPBound += 2;
                    holder.Health.HP += 4;
                    if (holder.Health.HPBound > holder.Health.HP3)
                        holder.Health.HPBound = holder.Health.HP3;
                    if (holder.Health.HP > holder.Health.HPBound)
                        holder.Health.HP = holder.Health.HPBound;
                }

                if (Health.HP % 6 == 0)
                {
                    holder.Health.PP += 5;
                    if (holder.Health.PP > NTSDGlobal.Gameplay.DrinkPPCap)
                        holder.Health.PP = NTSDGlobal.Gameplay.DrinkPPCap;
                }
            }
            else if (typeSub == 0x7B)
            {
                if (Health.HP <= 0) return;

                Health.HP -= 2;

                holder.Health.PP += 3;
                if (holder.Health.PP > NTSDGlobal.Gameplay.DrinkPPCap)
                    holder.Health.PP = NTSDGlobal.Gameplay.DrinkPPCap;

                if (KillCount > -1 && Health.PP > NTSDGlobal.Gameplay.PpRecoverLowLimit)
                    holder.Health.PP = NTSDGlobal.Gameplay.PpRecoverLowLimit;
            }
            else
            {
                return;
            }

            if (Health.HP > 0) return;

            if (holder is LF2Character holderChar) holderChar.GrabbedBy = 0;
            GrabbedBy = 0;
            ImmediateFrame(0);
            PS.vx = RandInt(0, 7) - 3f;
            PS.vy = -8.0f;
            PS.vz = 0f;
            PS.zz = 0;
            holder.ImmediateFrame(0);
            OnDrinkConsumed();
            _holdObj = null;
            (holder as LF2Character)?.HoldWeapon(null);
            Runtime.LinkState = 0;
            Runtime.HolderStableId = -1;
            HolderCopySlot = -1;
            result.ForceDrop = true;
        }

        public void SetWeaponStrengthList(List<WeaponStrengthEntry> list)
        {
            _weaponStrengthList = list;
        }

        protected WeaponStrengthEntry GetStrengthEntry(int attackingIndex)
        {
            if (_weaponStrengthList == null || attackingIndex <= 0) return null;
            return _weaponStrengthList.Find(e => e.index == attackingIndex);
        }
    }
}
