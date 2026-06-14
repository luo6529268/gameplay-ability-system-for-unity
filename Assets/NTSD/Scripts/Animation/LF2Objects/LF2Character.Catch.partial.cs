using NTSD.Animation;
using NTSD.Input;
using NTSD.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        public int caught_cpointkind()
        {
            var cpoint = CurrentFrame?.cpoint;
            return cpoint?.kind ?? 0;
        }

        public bool caught_cpointhurtable()
        {
            var cpoint = CurrentFrame?.cpoint;
            if (cpoint == null) return true;
            return cpoint.hurtable != 0;
        }

        private bool ProcessCatchingInput()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "input", CurrentFrameId);
            // C++ release 的抓取动作选择在全局 step10 的 cpoint_check 统一推进。
            // 输入阶段只保留按键状态，不在这里直接跳帧。
            return false;
        }

        private bool State_Catching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    CaughtDuration = 300;
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    Catching = null;
                    CaughtSlotIndex = -1;
                    PS.zz = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    return false;

                case "TU":
                    // C++ release 的抓取推进在全局 step10 执行。
                    return false;

                default:
                    return false;
            }
        }

        private bool State_BeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);
                    Catching = null;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);
                    Trans.SetWait(99);
                    return false;

                case "TU":
                    // C++ release 的被抓同步在全局 step10.5 执行。
                    return false;

                default:
                    return false;
            }
        }

        private void ApplyCatchingTransformToVictimData(LF2Character victim)
        {
            var victimConfig = CharacterAnimtorManager.Instance?.GetCharacterConfig(victim.ObjectId);
            if (victimConfig == null) return;

            TransformOriginalObjectId = ObjectId;
            TransformTargetObjectId = victim.ObjectId;
            FrameCache.Load(victimConfig);
            ObjectId = victim.ObjectId;
            ImmediateFrame(0);
            Frame.PN = Frame.N;
            PropagateCatchingTransformToOwnedObjects(victimConfig, victim.ObjectId);
        }

        private void PropagateCatchingTransformToOwnedObjects(LF2CharacterDataWrapper wrapper, int targetObjectId)
        {
            var objects = new List<LF2Entity>();
            Match?.GetAllEntities(objects);
            int selfSlotIndex = Runtime?.SlotIndex ?? -1;
            if (selfSlotIndex < 0) return;

            for (int i = 0; i < objects.Count; i++)
            {
                var entity = objects[i];
                if (entity == null || entity == this) continue;
                // C++ throwinjury == -1 传播看的是 kill_count == catcher_slot。
                if (entity.KillCount != selfSlotIndex) continue;
                entity.FrameCache.Load(wrapper);
                entity.ObjectId = targetObjectId;
            }
        }

        protected override void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (victimEntity is not LF2Character victim)
                return;

            bool attackReady = Controller?.IsAttack == true && (InputState?.AttackCooldown ?? 0) > 0;
            bool jumpReady = Controller?.IsJump == true && (InputState?.JumpCooldown ?? 0) > 0;

            if (attackReady && cpoint.aaction != 0)
            {
                bool dirOk = Controller == null || ((!Controller.IsLeft && !Controller.IsRight) || cpoint.taction == 0);
                if (dirOk)
                    ApplyCpointActionStep10(cpoint.aaction, victim);
            }

            if (attackReady && cpoint.taction != 0)
            {
                bool anyDir = Controller != null &&
                    (Controller.IsLeft || Controller.IsRight || Controller.IsUp || Controller.IsDown);
                if (anyDir)
                    ApplyCpointActionStep10(cpoint.taction, victim);
            }

            if (jumpReady && cpoint.jaction != 0)
                ApplyCpointActionStep10(cpoint.jaction, victim);
        }

        private void ApplyCpointActionStep10(int actionFrame, LF2Character victim)
        {
            ApplySignedCpointFrame(actionFrame);
            int victimAction = Frame?.D?.cpoint?.vaction ?? 0;
            victim.ApplySignedCpointFrame(victimAction);
            victim.AttackingCounter = 0;
            AttackingCounter = 0;
        }

        protected override void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (victimEntity is not LF2Character victim)
            {
                base.ApplyCpointThrowStep10(cpoint, victimEntity);
                return;
            }

            if (cpoint.throwinjury == -1)
                ApplyCatchingTransformToVictimData(victim);

            base.ApplyCpointThrowStep10(cpoint, victimEntity);
        }

        protected override void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            if (victim?.PS == null)
                return;

            if (Controller?.IsUp == true && Controller.IsDown == false)
                victim.PS.vz = -cpoint.throwvz;
            else if (Controller?.IsUp == false && Controller.IsDown == true)
                victim.PS.vz = cpoint.throwvz;
        }

        protected override void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            if (Controller == null || AttackingCounter != 2)
                return;

            if (cpoint.dircontrol == 1)
            {
                if (Controller.IsRight && !Controller.IsLeft) SwitchDir("right");
                else if (!Controller.IsRight && Controller.IsLeft) SwitchDir("left");
            }
            else if (cpoint.dircontrol == -1)
            {
                if (Controller.IsRight && !Controller.IsLeft) SwitchDir("left");
                else if (!Controller.IsRight && Controller.IsLeft) SwitchDir("right");
            }
        }

        protected override void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            if (victimEntity is not LF2Character victim || victim.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victim.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victim.FallDamageDiv;

                victim.Health.HP -= actualInjury;
                victim.Health.HPLost += actualInjury;
                victim.Health.HPBound -= actualInjury / 3;
                AttackingCounter = 1;
                FrameDelay = 2;
                victim.FrameDelay = -3;
                victim.WeaponCount += actualInjury;
                return;
            }

            // C++ Collision_Check2 的 wp.attacking < 0 走回血分支：
            // victim.hp += attacking; victim.hp_max += attacking / 3; attacker.attacking = 1。
            victim.Health.HP += injury;
            victim.Health.HPBound += injury / 3;
            AttackingCounter = 1;
        }

        protected override void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            if (victimEntity is not LF2Character victim)
                return;

            if (victim.PS == null || catcherFrame == null || catcherCpoint == null)
                return;

            int catcherX = Runtime != null ? Runtime.XInt : (int)PS.x;
            int catcherY = Runtime != null ? Runtime.YInt : (int)PS.y;
            int catcherZ = Runtime != null ? Runtime.ZInt : (int)PS.z;
            int dx = PS.dir == "right"
                ? catcherX - catcherFrame.centerx + catcherCpoint.x
                : catcherFrame.centerx - catcherCpoint.x + catcherX;
            int dy = catcherY - catcherFrame.centery + catcherCpoint.y;

            LF2FrameData vactionFrame = victim.FrameCache.GetFrameDataById(Mathf.Abs(catcherCpoint.vaction));
            int victimCpointX = vactionFrame?.cpoint?.x ?? 0;
            int victimCpointY = vactionFrame?.cpoint?.y ?? 0;
            // C++ Collision_Check2 这里使用的是 victim 的 vaction 目标帧几何，
            // 不是 victim 当前帧的 centerx/centery。
            int victimCenterX = vactionFrame?.centerx ?? 0;
            int victimCenterY = vactionFrame?.centery ?? 0;

            victim.PS.x = victim.PS.dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victim.PS.y = victimCenterY - victimCpointY + dy;
            victim.PS.z = catcherZ;

            int coverDiv = catcherCpoint.cover / 10;
            int coverRem = catcherCpoint.cover % 10;
            if (coverRem != 0)
            {
                victim.PS.z += 1f;
                victim.PS.y -= 1f;
            }
            else
            {
                victim.PS.z -= 1f;
                victim.PS.y += 1f;
            }

            if (coverDiv == 1)
                victim.SwitchDir(PS.dir);
            else if (coverDiv == 2)
                victim.SwitchDir(PS.dir == "right" ? "left" : "right");
        }
    }
}
