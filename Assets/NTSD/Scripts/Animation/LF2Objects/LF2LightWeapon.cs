using UnityEngine;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public class LF2LightWeapon : LF2WeaponBase
    {
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;
        public override bool IsLight => true;
        public override bool IsHeavy => false;

        protected override bool State_WeaponJustOnGround(string eventType, object eventData)
        {
            if (eventType == "frame")
            {
                if (Frame.N == 70)
                {
                    var frame = Frame.D;
                    if (frame == null || string.IsNullOrEmpty(frame.sound))
                    {
                        PlaySound(WeaponDropSound);
                    }
                }
                return true;
            }
            return false;
        }

        protected override bool State_WeaponOnGround(string eventType, object eventData)
        {
            if (eventType == "frame")
            {
                if (Frame.N == 64)
                {
                    Team = 0;
                }
                return true;
            }
            return false;
        }

        public override bool Hit(InteractionArea itr, LF2LivingObject attacker)
        {
            if (HoldObj != null) return false;
            if (IsVRest(attacker)) return false;

            if (itr.kind == 15)
            {
                WhirlwindForce(itr);
                return true;
            }
            if (itr.kind == 10 || itr.kind == 11)
            {
                FluteForce();
                return true;
            }

            bool accept = false;
            int state = GetState();

            if (state == LF2States.WeaponThrowing)
            {
                accept = true;
                var attackerPs = attacker?.PS;
                if (attackerPs != null)
                {
                    if ((Dirh() > 0) != (PS.vx > 0))
                    {
                        PS.vx *= NTSDGlobal.Gameplay.WeaponReverseFactorVx;
                    }
                }
                PS.vy *= NTSDGlobal.Gameplay.WeaponReverseFactorVy;
                PS.vz *= NTSDGlobal.Gameplay.WeaponReverseFactorVz;
                Team = attacker?.Team ?? 0;
            }
            else if (state == LF2States.WeaponOnGround)
            {
                var attackerWeapon = attacker as LF2WeaponBase;
                if (attackerWeapon != null)
                {
                    accept = true;
                    var aps = attackerWeapon.PS;
                    PS.vx = (aps.vx != 0 ? Mathf.Sign(aps.vx) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    PS.vz = (aps.vz != 0 ? Mathf.Sign(aps.vz) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                }
            }

            if (accept)
            {
                ApplyHitEffects(itr, attacker);
            }

            return accept;
        }

        private void ApplyHitEffects(InteractionArea itr, LF2LivingObject attacker)
        {
            if (itr.vrest > 0)
            {
                SetVRest(attacker, itr.vrest);
            }
            if (itr.injury > 0)
            {
                Health.HP -= itr.injury;
            }
            PlaySound(WeaponHitSound);
        }

        public override void Interaction()
        {
            if (Team == 0) return;

            int state = GetState();
            if (state != LF2States.WeaponThrowing) return;
        }
    }
}
