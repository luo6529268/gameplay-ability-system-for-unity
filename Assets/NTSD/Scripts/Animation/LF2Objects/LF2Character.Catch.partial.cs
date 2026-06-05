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

        public void caught_release()
        {
            Catching = null;
            ImmediateFrame(181);
            Effect.Dvx = 3;
            Effect.Dvy = -3;
            Effect.TimeIn = -1;
            Effect.TimeOut = 0;
            FrameDelay = -3;
        }

        private bool ProcessCatchingInput()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "input", CurrentFrameId);
            return ProcessCatchingActionSelection();
        }

        private bool State_Catching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    CatchingStateTU = true;
                    CaughtDuration = 300;
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "初始化抓取持续值 300");
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    Catching = null;
                    PS.zz = 0;
                    return false;

                case "frame":
                    return ProcessCatchingFrame();

                case "TU":
                    return ProcessCatchingTU();

                default:
                    return false;
            }
        }

        private bool ProcessCatchingFrame()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "frame", CurrentFrameId);
            return false;
        }

        private bool ProcessCatchingTU()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "TU", CurrentFrameId);

            if (Catching is not LF2Character victim)
                return false;

            var cpoint = Frame?.D?.cpoint;
            if (cpoint == null || cpoint.kind != 1)
            {
                BreakCatchingRelation(resetVictim: false);
                return true;
            }

            var victimCpoint = victim.Frame?.D?.cpoint;
            if (victimCpoint == null || victimCpoint.kind != 2 || victim.Catching != this)
            {
                BreakCatchingRelation(resetVictim: false);
                return true;
            }

            if (FrameDelay < 0)
                return false;

            if (CatchingStateTU)
                CatchingStateTU = false;

            if (ProcessCpointDecrease(victim))
                return true;

            if (ProcessCatchingActionSelection())
                return true;

            if (ProcessCatchingThrowIfNeeded(victim))
                return true;

            ApplyDirControl();
            return false;
        }

        private void BreakCatchingRelation(bool resetVictim)
        {
            ImmediateFrame(LF2StandardFrames.Standing);
            if (resetVictim && Catching is LF2Character victim)
                victim.Catching = null;

            Catching = null;
            CaughtDuration = 0;
        }

        private bool ProcessCpointDecrease(LF2Character victim)
        {
            var cpoint = Frame?.D?.cpoint;
            if (cpoint == null) return false;

            if (cpoint.decrease > 0)
            {
                CaughtDuration -= cpoint.decrease;
            }
            else if (cpoint.decrease < 0)
            {
                CaughtDuration += cpoint.decrease;
                if (CaughtDuration < 0)
                {
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "cpoint.decrease 触发逃脱");
                    ImmediateFrame(LF2StandardFrames.Standing);
                    victim.ImmediateFrame(181);
                    HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = PS.x > victim.PS.x ? -4f : 4f;
                    victim.KnockbackVy = -3f;
                    Catching = null;
                    victim.Catching = null;
                    CaughtDuration = 0;
                    return true;
                }
            }

            return false;
        }

        private bool ProcessCatchingActionSelection()
        {
            if (Catching is not LF2Character victim) return false;
            var cpoint = Frame?.D?.cpoint;
            if (cpoint == null) return false;

            bool attackReady = Controller?.IsAttack == true && (InputState?.AttackCooldown ?? 0) > 0;
            bool jumpReady = Controller?.IsJump == true && (InputState?.JumpCooldown ?? 0) > 0;
            bool hasDirection = Controller != null &&
                (Controller.IsLeft || Controller.IsRight || Controller.IsUp || Controller.IsDown);

            if (attackReady && cpoint.aaction != 0 && ((!Controller.IsLeft && !Controller.IsRight) || cpoint.taction == 0))
            {
                ApplyCatchingActionFrame(cpoint.aaction, victim);
                return true;
            }

            if (attackReady && hasDirection && cpoint.taction != 0)
            {
                ApplyCatchingActionFrame(cpoint.taction, victim);
                return true;
            }

            if (jumpReady && cpoint.jaction != 0)
            {
                ApplyCatchingActionFrame(cpoint.jaction, victim);
                return true;
            }

            return false;
        }

        private void ApplyCatchingActionFrame(int actionFrame, LF2Character victim)
        {
            if (actionFrame < 0)
            {
                SwitchDir(PS.dir == "right" ? "left" : "right");
                actionFrame = -actionFrame;
            }

            ImmediateFrame(actionFrame);
            var actionFrameData = FrameCache.GetFrameDataById(actionFrame);
            int victimAction = actionFrameData?.cpoint?.vaction ?? 0;
            victim.ImmediateFrame(victimAction);
            AttackingCounter = 0;
            victim.AttackingCounter = 0;
        }

        private bool ProcessCatchingThrowIfNeeded(LF2Character victim)
        {
            var cpoint = Frame?.D?.cpoint;
            if (cpoint == null || cpoint.throwvx == 0) return false;

            ApplyCatchingThrow(cpoint, victim);
            int nextFrame = Frame?.D?.next ?? 0;
            ImmediateFrame(nextFrame);
            Frame.PN = Frame.N;
            AttackingCounter = 0;
            return true;
        }

        private void ApplyCatchingThrow(CatchPoint cpoint, LF2Character victim)
        {
            if (cpoint.throwinjury > 0)
                victim.WeaponCount = cpoint.throwinjury;
            else if (cpoint.throwinjury == -1)
                ApplyCatchingTransformToVictimData(victim);

            SyncCaughtThrowPosition(cpoint, victim);

            int dir = PS.dir == "right" ? 1 : -1;
            victim.PS.vx = cpoint.throwvx * dir;
            victim.PS.vy = cpoint.throwvy;

            if (Controller?.IsUp == true && Controller.IsDown == false)
                victim.PS.vz = -cpoint.throwvz;
            else if (Controller?.IsUp == false && Controller.IsDown == true)
                victim.PS.vz = cpoint.throwvz;

            victim.ImmediateFrame(cpoint.vaction);
            victim.Frame.PN = cpoint.vaction;
            Catching = null;
            victim.Catching = null;
        }

        private void ApplyCatchingTransformToVictimData(LF2Character victim)
        {
            var victimConfig = CharacterAnimtorManager.Instance?.GetCharacterConfig(victim.ObjectId);
            if (victimConfig == null) return;

            FrameCache.Load(victimConfig);
            ObjectId = victim.ObjectId;
            ImmediateFrame(0);
            Frame.PN = Frame.N;
            PropagateCatchingTransformToOwnedObjects(victimConfig);
        }

        private void PropagateCatchingTransformToOwnedObjects(LF2CharacterDataWrapper wrapper)
        {
            var objects = new List<LF2Entity>();
            Match?.GetAllEntities(objects);

            for (int i = 0; i < objects.Count; i++)
            {
                var entity = objects[i];
                if (entity == null || entity == this) continue;
                if (entity.KillCount != StableId) continue;
                entity.FrameCache.Load(wrapper);
            }
        }

        private void ApplyDirControl()
        {
            var cpoint = Frame?.D?.cpoint;
            if (cpoint == null || Controller == null) return;
            if (AttackingCounter != 2) return;

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
                    ApplyCollisionCheck2PositionSync();
                    return false;

                default:
                    return false;
            }
        }

        private void ApplyCollisionCheck2PositionSync()
        {
            if (Catching is not LF2Character catcher) return;

            var catcherCpoint = catcher.Frame?.D?.cpoint;
            if (catcherCpoint == null || catcherCpoint.kind != 1) return;

            var selfCpoint = Frame?.D?.cpoint;
            if (selfCpoint == null || selfCpoint.kind != 2) return;

            ApplyBeingCaughtVaction(catcher, catcherCpoint);
            ApplyCpointInjuryFromCatcher(catcher, catcherCpoint);
            SyncBeingCaughtPosition(catcher, catcherCpoint);
        }

        private void ApplyBeingCaughtVaction(LF2Character catcher, CatchPoint catcherCpoint)
        {
            int vaction = catcherCpoint.vaction;
            if (vaction == 0) return;

            bool shouldSetFrame = (AttackingCounter == 0 && catcherCpoint.dircontrol == 1) || catcherCpoint.dircontrol == 0;
            if (!shouldSetFrame) return;

            int targetFrame = vaction;
            if (targetFrame < 0)
            {
                PS.dir = PS.dir == "right" ? "left" : "right";
                targetFrame = -targetFrame;
            }

            ImmediateFrame(targetFrame);
        }

        private void ApplyCpointInjuryFromCatcher(LF2Character catcher, CatchPoint catcherCpoint)
        {
            if (catcherCpoint.injury == 0) return;
            if (catcher.AttackingCounter != 0) return;

            if (catcherCpoint.injury > 0)
            {
                ApplyDirectInjury(catcherCpoint.injury);
                catcher.FrameDelay = 2;
                FrameDelay = -3;
                catcher.AttackingCounter = 1;
            }
            else
            {
                if (Health != null)
                {
                    Health.HP -= catcherCpoint.injury;
                    Health.HPBound -= catcherCpoint.injury / 3;
                }
                catcher.AttackingCounter = 1;
            }
        }

        private void SyncBeingCaughtPosition(LF2Character catcher, CatchPoint catcherCpoint)
        {
            var catcherFrame = catcher.Frame?.D;
            var selfFrame = Frame?.D;
            if (catcherFrame == null || selfFrame == null) return;

            int catcherAdir = catcher.PS.dir == "right" ? 1 : -1;
            float catcherCenterX = catcherFrame.centerx;
            float catcherCenterY = catcherFrame.centery;

            float attachX = catcher.PS.x + (catcherCpoint.x - catcherCenterX) * catcherAdir;
            float attachY = catcher.PS.y - catcherCenterY + catcherCpoint.y;

            var vactionFrame = FrameCache.GetFrameDataById(Mathf.Abs(catcherCpoint.vaction));
            var vactionCpoint = vactionFrame?.cpoint;
            float selfCpointX = vactionCpoint?.x ?? 0f;
            float selfCpointY = vactionCpoint?.y ?? 0f;
            float selfCenterX = selfFrame.centerx;
            float selfCenterY = selfFrame.centery;

            PS.x = PS.dir == "right"
                ? selfCenterX - selfCpointX + attachX
                : selfCpointX - selfCenterX + attachX;
            PS.y = selfCenterY - selfCpointY + attachY;
            PS.z = catcher.PS.z;

            int cover = catcherCpoint.cover;
            if (cover % 10 != 0)
            {
                PS.z += 1f;
                PS.y -= 1f;
            }
            else
            {
                PS.z -= 1f;
                PS.y += 1f;
            }

            int coverDir = cover / 10;
            if (coverDir == 1)
            {
                PS.dir = catcher.PS.dir;
            }
            else if (coverDir == 2)
            {
                PS.dir = catcher.PS.dir == "right" ? "left" : "right";
            }
        }

        private void SyncCaughtThrowPosition(CatchPoint cpoint, LF2Character victim)
        {
            var catcherFrame = Frame?.D;
            if (catcherFrame == null || victim?.PS == null) return;

            victim.PS.x = PS.dir == "right"
                ? PS.x - catcherFrame.centerx + cpoint.x
                : catcherFrame.centerx - cpoint.x + PS.x;
            victim.PS.y = PS.y - catcherFrame.centery + cpoint.y;
        }
    }
}
