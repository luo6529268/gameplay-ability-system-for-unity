using NTSD.App;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// 爬起状态处理器 (state = 6)
        /// 对应 FLF js:656-681
        /// </summary>
        private bool State_Rowing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Log.Info("[State {0}:TU] ", eventType);

                    if (CurrentFrameId == LF2StandardFrames.Rowing ||
                        CurrentFrameId == LF2StandardFrames.RowingBack)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", $"爬起暂停 Frame={CurrentFrameId}");
                        PS.vy = 0;
                    }
                    return false;

                case "frame":
                    Log.Info("[State {0}:frame] ", eventType);

                    if (CurrentFrameId == LF2StandardFrames.Rowing ||
                        CurrentFrameId == LF2StandardFrames.RowingBack)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "设置爬起等待时间");
                        Trans.SetWait(LF2StateConstants.RowingWaitTime);
                        return true;
                    }
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:fall_onto_ground] ", eventType);

                    if (CurrentFrameId == LF2StandardFrames.Rowing1 ||
                        CurrentFrameId == LF2StandardFrames.RowingBack1)
                    {
                        Log.Info("爬起结束落地");
                        Log.Info("TransitionTo: Frame {0} ({1})", LF2StandardFrames.Crouch, "落地 → 蹲姿");
                        TransitionToFrame(LF2StandardFrames.Crouch, 0);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 防御被破状态处理器 (state = 8)
        /// 对应 FLF js:698-719
        /// </summary>
        private bool State_BrokenDefend(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame_force":
                case "TU_force":
                    Log.Info("[State {0}:{1}] Event={2}", 8, "BrokenDefend", eventType);

                    var D = Frame.D;
                    if (D.dvx != 0)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 8, "BrokenDefend", $"防御被破 dvx={D.dvx} → 修正移动方向");
                        if ((PS.vx > 0 ? 1 : -1) != Dirh())
                        {
                            float avx = PS.vx > 0 ? PS.vx : -PS.vx;
                            float dirx = 2 * (PS.vx > 0 ? 1 : -1);
                            if (PS.y < 0 || avx < D.dvx)
                                PS.vx = dirx * D.dvx;

                            if (D.dvx < 0)
                                PS.vx -= dirx;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 受伤状态处理器 (state = 11)
        /// 对应 FLF js:942-960
        /// </summary>
        private bool State_Injured(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Trans.IncWait(0, 20);
                    return false;

                case "frame":
                    int frameId = CurrentFrameId;
                    if (frameId == LF2StandardFrames.Injured1 ||
                        frameId == LF2StandardFrames.Injured3 ||
                        frameId == LF2StandardFrames.Injured5)
                    {
                        Trans.SetNext(LF2StandardFrames.LoopToStart);
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 受伤2状态处理器 (state = 16)
        /// 对应 FLF js:1230-1235
        /// </summary>
        private bool State_Injured2(string eventType, object eventData)
        {
            return false;
        }

        private bool State_Falling(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    int fn = Frame.N;
                    if (Effect.Dvy <= 0f)
                    {
                        switch (fn)
                        {
                            case LF2StandardFrames.FallingFront:
                                Trans.SetNext(LF2StandardFrames.FallingFront1);
                                Trans.SetWait((int)NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FallWait180, Effect.Dvy));
                                break;
                            case LF2StandardFrames.FallingFront1:
                                Trans.SetNext(LF2StandardFrames.FallingFront2);
                                float vy181 = PS.vy == 0f ? 5f : Mathf.Abs(PS.vy);
                                if (PS.vy == 0f) PS.vy = 5f;
                                if      (vy181 <= 4f) Trans.SetWait(2);
                                else if (vy181 <  7f) Trans.SetWait(3);
                                else                  Trans.SetWait(4);
                                break;
                            case LF2StandardFrames.FallingFront2:
                                Trans.SetNext(LF2StandardFrames.FallingFront3);
                                break;
                            case LF2StandardFrames.FallingBack:
                                if (PS.vy == 0f) PS.vy = 5f;
                                Trans.SetNext(LF2StandardFrames.FallingBack1);
                                break;
                            case LF2StandardFrames.FallingBack1:
                                Trans.SetNext(LF2StandardFrames.FallingBack2);
                                break;
                            case LF2StandardFrames.FallingBack2:
                                Trans.SetNext(LF2StandardFrames.FallingBack3);
                                break;
                        }
                    }
                    else
                    {
                        switch (fn)
                        {
                            case LF2StandardFrames.FallingFront:
                                Trans.SetNext(LF2StandardFrames.FallingFront5);
                                Trans.SetWait(1);
                                break;
                            case LF2StandardFrames.FallingBack:
                                Trans.SetNext(LF2StandardFrames.FallingBack5);
                                break;
                        }
                    }
                    return false;

                case "TU":
                    if (HitCounters.Fall > 0)
                        HitCounters.AddFall(-1);

                    if (PS.y < 0f && PS.y + PS.vy < -0.0001f)
                    {
                        int curFn = Frame.N;
                        if (curFn < LF2StandardFrames.FallingFront5)
                        {
                            int newFn;
                            if      (PS.vy < -8f) newFn = LF2StandardFrames.FallingFront;
                            else if (PS.vy <  1f) newFn = LF2StandardFrames.FallingFront1;
                            else if (PS.vy <  8f) newFn = LF2StandardFrames.FallingFront2;
                            else                  newFn = LF2StandardFrames.FallingFront3;
                            if (newFn != curFn) ImmediateFrame(newFn);
                        }
                        else if (curFn > LF2StandardFrames.FallingFront5 &&
                                 curFn < LF2StandardFrames.FallingBack5)
                        {
                            int newFn;
                            if      (PS.vy < -8f) newFn = LF2StandardFrames.FallingBack;
                            else if (PS.vy <  1f) newFn = LF2StandardFrames.FallingBack1;
                            else if (PS.vy <  8f) newFn = LF2StandardFrames.FallingBack2;
                            else                  newFn = LF2StandardFrames.FallingBack3;
                            if (newFn != curFn) ImmediateFrame(newFn);
                        }
                    }
                    return false;

                case "combo":
                    return ProcessFallingInputCommand(eventData as string);

                case "fell_onto_ground":
                case "fall_onto_ground":
                {
                    AppManager.Instance?.SoundPlayer?.PlaySfx(NTSDGlobal.Sound.FallLand);

                    float vyBeforeLand = PS.vy;
                    PS.y  = 0f;
                    PS.vy = 0f;
                    PS.vz = 0f;

                    int curState = GetState();
                    if (curState == LF2States.Falling || curState == LF2States.Burning)
                    {
                        KnockbackVx = 0f;
                        KnockbackVy = 0f;
                        HitCount    = 0;

                        bool highSpeed = vyBeforeLand > 11.0f || PS.vx > 9.0f || PS.vx < -9.0f
                                         || curState == LF2States.Burning;

                        if (highSpeed)
                        {
                            PS.vy = -3.5f;
                            if (PS.vx > 7f)  PS.vx = 7f;
                            if (PS.vx < -7f) PS.vx = -7f;
                            int bounceFrame = (Frame.N >= LF2StandardFrames.FallingBack)
                                ? LF2StandardFrames.FallingBack5
                                : LF2StandardFrames.FallingFront5;
                            TransitionToFrame(bounceFrame, 15);
                        }
                        else
                        {
                            PS.vx *= (1f / 3f);
                            int landFrame = (Frame.N >= LF2StandardFrames.FallingBack)
                                ? LF2StandardFrames.LyingBack
                                : LF2StandardFrames.Lying;
                            TransitionToFrame(landFrame, 15);
                        }
                    }
                    else
                    {
                        PS.vx *= 0.7f;
                        KnockbackVx = 0f;
                        KnockbackVy = 0f;
                        HitCount    = 0;

                        if (caught_throwinjury.HasValue && caught_throwinjury.Value > 0)
                        {
                            Injury(caught_throwinjury.Value);
                            caught_throwinjury = 0;
                        }

                        int landFrame;
                        int curFrameState = GetState();
                        if (curFrameState == LF2States.CustomSkill1)
                            landFrame = 94;
                        else if (Frame.N == LF2StandardFrames.JumpingAir || curFrameState == LF2States.Rowing)
                            landFrame = LF2StandardFrames.Crouch;
                        else
                            landFrame = LF2StandardFrames.Crouch2;

                        TransitionToFrame(landFrame, 15);
                    }
                    return true;
                }

                default:
                    return false;
            }
        }

        private bool State_Frozen(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    BrokenEffectCreate(212);
                    AppManager.Instance?.SoundPlayer?.PlaySfx(NTSDGlobal.Sound.IceShatter);
                    return false;

                default:
                    return false;
            }
        }

        private bool State_Lying(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    HitCounters.ResetFall();
                    HitCounters.ResetBdefend();

                    if (Health.HP <= 0)
                    {
                        if (!IsNpc && Attacker != null)
                            Attacker.Killed();
                        Dead = true;
                        if (_deadBlinkCount < 0)
                            _deadBlinkCount = 0;
                    }
                    return false;

                case "state_exit":
                    Effect.TimeIn  = 0;
                    Effect.TimeOut = 30;
                    Effect.Blink   = true;
                    Effect.Super   = true;
                    return false;

                default:
                    return false;
            }
        }

        private bool State_Burning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    BrokenEffectCreate(302, 1);
                    return false;

                case "fall_onto_ground":
                    BrokenEffectCreate(302);
                    goto case "fell_onto_ground";

                case "fell_onto_ground":
                    return State_Falling("fell_onto_ground", eventData);

                default:
                    return false;
            }
        }
    }
}
