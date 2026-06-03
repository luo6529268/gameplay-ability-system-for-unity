using NTSD.Animation;
using NTSD.Extensions;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        public void caught_b(Vector3 holdpoint, CatchPoint cpoint, int adir, int vdir)
        {
            caught_b_holdpoint = holdpoint;
            caught_b_cpoint = cpoint;
            caught_b_adir = adir;
            caught_b_vdir = vdir;
        }

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

        public void caught_throw(CatchPoint cpoint, int vdir)
        {
            if (cpoint.vaction != 0)
            {
                ImmediateFrame(cpoint.vaction);
            }
            else
            {
                ImmediateFrame(LF2StandardFrames.JumpingAir);
            }
            caught_throwz = vdir;
            FrameDelay = -5;
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

        private bool ProcessCatchingInputCommand(string comboKey)
        {
            Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 9, "Catching", "combo", comboKey, CurrentFrameId);

            if (string.IsNullOrEmpty(comboKey))
                return false;

            var cpoint = Frame.D?.cpoint;
            if (cpoint == null)
                return false;

            if (comboKey == "att")
            {
                if (cpoint.taction != 0)
                {
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "taction throw");
                    ApplyCatchingThrowAction(cpoint);
                    TransitionToFrame(cpoint.taction, 22);
                    Catching = null;
                    return true;
                }

                if (cpoint.aaction != 0)
                {
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "aaction attack");
                    TransitionToFrame(cpoint.aaction, 22);
                    return true;
                }

                return false;
            }

            if (comboKey == "jump")
            {
                if (cpoint.jaction != 0)
                {
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "jaction jump");
                    TransitionToFrame(cpoint.jaction, 22);
                    return true;
                }

                return false;
            }

            if (comboKey == "def")
            {
                if (cpoint.daction != 0)
                {
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "daction defend");
                    TransitionToFrame(cpoint.daction, 22);
                    return true;
                }

                return false;
            }

            return false;
        }

        private void ApplyCatchingThrowAction(CatchPoint cpoint)
        {
            if (!(Catching is LF2Character throwTarget))
                return;

            int vdir = PS.dir == "right" ? 1 : -1;
            throwTarget.caught_throw(cpoint, vdir);
            throwTarget.PS.vx = cpoint.throwvx * vdir;
            throwTarget.PS.vy = cpoint.throwvy;
            throwTarget.PS.vz = cpoint.throwvz;

            if (cpoint.throwinjury != 0)
                throwTarget.caught_throwinjury = cpoint.throwinjury;
        }

        private bool State_Catching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);
                    _catchingStateTU = true;
                    _catchingCounter = 43;
                    _catchingAttacks = 0;
                    caught_decrease_counter = 99;
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "init catching state counter=43, attacks=0, decrease=99");
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

                case "post_combo":
                    return ProcessCatchingPostCombo();

                case "combo":
                    return ProcessCatchingInputCommand(eventData as string);

                default:
                    return false;
            }
        }

        private bool ProcessCatchingFrame()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "frame", CurrentFrameId);

            int frameId = CurrentFrameId;
            var frame = Frame.D;

            if (frameId == 123)
            {
                Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "frame 123 success attack extends catch");
                _catchingAttacks++;
                _catchingCounter += 3;
                Trans.SetWait(Trans.Wait + 1);
                return true;
            }

            if (frameId == 233 || frameId == 234)
            {
                Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", $"Frame {frameId} -> decrease wait");
                Trans.SetWait(Trans.Wait - 1);
                return true;
            }

            if (Catching is LF2Character caughtChar && frame.cpoint != null)
            {
                if (frame.cpoint.decrease > 0 && caught_decrease_counter == 99)
                {
                    caught_decrease_counter = frame.cpoint.decrease;
                }

                int adir = PS.dir == "right" ? 1 : -1;
                var holdpoint = new Vector3(
                    PS.x + frame.cpoint.x * adir,
                    PS.y + frame.cpoint.y,
                    PS.z
                );
                caughtChar.caught_b(holdpoint, frame.cpoint, adir, 1);
            }

            return false;
        }

        private bool ProcessCatchingTU()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "TU", CurrentFrameId);

            if (Catching != null &&
                caught_cpointkind() == 1 &&
                ((LF2Character)Catching).caught_cpointkind() == 2)
            {
                if (_catchingStateTU)
                {
                    _catchingStateTU = false;

                    var cpoint = Frame.D.cpoint;
                    if (cpoint.injury != 0 && FrameDelay == 0)
                    {
                        NTSDDamageCalculator.ApplyDamage(Catching, cpoint.injury);
                        FrameDelay = 2;
                        if (Catching is LF2Character caughtCh)
                            caughtCh.FrameDelay = -3;
                    }

                    if (cpoint.dircontrol == 1 && Controller != null)
                    {
                        if (Controller.IsLeft)
                        {
                            SwitchDir("left");
                        }
                        else if (Controller.IsRight)
                        {
                            SwitchDir("right");
                        }
                    }
                }
            }

            return false;
        }

        private bool ProcessCatchingPostCombo()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "post_combo", CurrentFrameId);

            if (Catching != null && Catching.Controller != null)
            {
                var caughtCtrl = Catching.Controller;
                if (caughtCtrl.IsLeft || caughtCtrl.IsRight || caughtCtrl.IsUp || caughtCtrl.IsDown)
                {
                    caught_decrease_counter--;
                    Log.Info("[State {0}:{1}] -> Branch: {2}, Counter={3}", 9, "Catching", "caught input decreases counter", caught_decrease_counter);

                    if (caught_decrease_counter <= 0)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "counter reached zero, release caught target");
                        if (Catching is LF2Character victim)
                            victim.caught_release();

                        Catching = null;
                        TransitionToFrame(LF2StandardFrames.Standing, 22);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool State_BeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    _caughtDecayAccum = 300;
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);
                    Catching = null;
                    caught_b_holdpoint = Vector3.zero;
                    caught_b_cpoint = null;
                    caught_b_adir = 0;
                    caught_b_vdir = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);
                    Trans.SetWait(99);
                    return false;

                case "TU":
                    ApplyCollisionCheck2PositionSync();
                    ApplyCollisionCheck1CaughtLogic();
                    return false;

                default:
                    return false;
            }
        }

        private void ApplyCollisionCheck2PositionSync()
        {
            if (Catching == null) return;
            var catcher = Catching as LF2Character;
            if (catcher == null) return;

            var catcherCpoint = catcher.Frame?.D?.cpoint;
            if (catcherCpoint == null || catcherCpoint.kind != 1) return;

            var selfCpoint = Frame?.D?.cpoint;
            if (selfCpoint == null || selfCpoint.kind != 2) return;

            int vaction = catcherCpoint.vaction;
            if (vaction != 0)
            {
                bool shouldSetFrame = (HitStun == 0 && catcherCpoint.dircontrol == 1) || catcherCpoint.dircontrol == 0;
                if (shouldSetFrame)
                {
                    int targetFrame = vaction;
                    if (targetFrame < 0)
                    {
                        PS.dir = PS.dir == "right" ? "left" : "right";
                        targetFrame = -targetFrame;
                    }
                    Trans.Frame(targetFrame, 0);
                }
            }

            int catcherAdir = catcher.PS.dir == "right" ? 1 : -1;
            var catcherFrame = catcher.Frame?.D;
            var selfFrame = Frame?.D;
            float catcherCenterX = catcherFrame?.centerx ?? 0f;
            float catcherCenterY = catcherFrame?.centery ?? 0f;
            float selfCenterX = selfFrame?.centerx ?? 0f;
            float selfCenterY = selfFrame?.centery ?? 0f;

            PS.x = catcher.PS.x + (catcherCpoint.x - catcherCenterX + selfCenterX) * catcherAdir;
            PS.z = catcher.PS.z + catcherCpoint.y - catcherCenterY + selfCenterY;
            PS.y = catcher.PS.y;

            int cover = catcherCpoint.cover;
            if (cover % 10 != 0)
            {
                PS.y += 1f;
                PS.z -= 1f;
            }
            else
            {
                PS.y -= 1f;
                PS.z += 1f;
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

        private void ApplyCollisionCheck1CaughtLogic()
        {
            if (Catching == null) return;
            var catcher = Catching as LF2Character;
            if (catcher == null) return;

            var selfCpoint = Frame?.D?.cpoint;
            if (selfCpoint == null || selfCpoint.kind != 1) return;

            var catcherCpoint = catcher.Frame?.D?.cpoint;
            if (catcherCpoint == null || catcherCpoint.kind != 2) return;

            int decrease = selfCpoint.decrease;
            if (decrease > 0)
            {
                _caughtDecayAccum -= decrease;
            }
            else if (decrease < 0)
            {
                _caughtDecayAccum += decrease;
                if (_caughtDecayAccum < 0)
                {
                    Trans.Frame(0, 0);
                    catcher.Trans.Frame(0, 0);
                    catcher.PS.vx = 0f;
                    catcher.PS.vy = PS.x <= catcher.PS.x ? 2.25f : -2.25f;
                    PS.vx = 0f;
                    PS.vy = -2.125f;
                    catcher.Trans.Frame(181, 0);
                    catcher.Catching = null;
                    Catching = null;
                    _caughtDecayAccum = 0;
                    return;
                }
            }

            int dircontrol = selfCpoint.dircontrol;
            if (dircontrol != 0 && Trans.Wait == 2 && Controller != null)
            {
                bool pressingRight = Controller.IsRight;
                bool pressingLeft = Controller.IsLeft;
                if (dircontrol == 1)
                {
                    if (pressingRight && !pressingLeft) PS.dir = "right";
                    if (!pressingRight && pressingLeft) PS.dir = "left";
                }
                else if (dircontrol == -1)
                {
                    if (pressingRight && !pressingLeft) PS.dir = "left";
                    if (!pressingRight && pressingLeft) PS.dir = "right";
                }
            }

            if (selfCpoint.throwvx != 0)
            {
                int throwinjury = selfCpoint.throwinjury;
                if (throwinjury > 0)
                {
                    catcher.HealTimer = throwinjury;
                }
                else if (throwinjury == -1)
                {
                    var catcherConfig = CharacterAnimtorManager.Instance?.GetCharacterConfig(catcher.ObjectId);
                    if (catcherConfig != null)
                    {
                        FrameCache.Load(catcherConfig);
                        Trans.Frame(0, 0);
                    }
                    Catching = null;
                    catcher.Catching = null;
                }
            }
        }
    }
}
