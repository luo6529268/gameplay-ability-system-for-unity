using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2CharacterDamageStateResolver
    {
        private readonly LF2Character _character;

        /// <summary>
        /// 受击后的状态分流器。
        /// 这里不做“命中是否成立”的判断，只负责把已命中的角色导向正确的后续状态。
        /// </summary>
        public LF2CharacterDamageStateResolver(LF2Character character)
        {
            _character = character;
        }

        /// <summary>
        /// 跑步/滚动类状态的专用分支。
        /// 这类状态的关键不是掉血，而是控制帧停顿和落地后的衔接。
        /// </summary>
        public bool StateRowing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Log.Info("[State {0}:TU] ", eventType);

                    if (_character.CurrentFrameId == LF2StandardFrames.Rowing ||
                        _character.CurrentFrameId == LF2StandardFrames.RowingBack)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "frame hold");
                        _character.Runtime.Vy = 0f;
                    }
                    return false;

                case "frame":
                    Log.Info("[State {0}:frame] ", eventType);

                    if (_character.CurrentFrameId == LF2StandardFrames.Rowing ||
                        _character.CurrentFrameId == LF2StandardFrames.RowingBack)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "set rowing wait");
                        _character.Trans.SetWait(LF2StateConstants.RowingWaitTime);
                        return true;
                    }
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:fall_onto_ground] ", eventType);

                    if (_character.CurrentFrameId == LF2StandardFrames.Rowing1 ||
                        _character.CurrentFrameId == LF2StandardFrames.RowingBack1)
                    {
                        Log.Info("rowing end land");
                        Log.Info("ImmediateFrame: Frame {0} ({1})", LF2StandardFrames.Crouch, "land to crouch");
                        _character.ImmediateFrame(LF2StandardFrames.Crouch);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 轻伤状态的帧级处理。
        /// 主要负责把受击动画推进到循环尾，并决定何时继续下一段受击。
        /// </summary>
        public bool StateInjured(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    _character.Trans.IncWait(0);
                    return false;

                case "frame":
                    int frameId = _character.CurrentFrameId;
                    if (frameId == LF2StandardFrames.Injured1 ||
                        frameId == LF2StandardFrames.Injured3 ||
                        frameId == LF2StandardFrames.Injured5)
                    {
                        _character.Trans.SetNext(LF2StandardFrames.LoopToStart);
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 落地统一入口。
        /// 先看当前是不是冻结、燃烧或跑步类状态，再把结果交给对应分支。
        /// </summary>
        public void HandleLandingEvent(double vyBeforeLand) // P0-f-2b B2-1: float→double
        {
            var cpoint = _character.Frame?.D?.cpoint;
            if (cpoint != null && cpoint.kind == 2)
                return;

            int curState = _character.GetState();
            if (curState == LF2States.Frozen && vyBeforeLand > 0.0001f)
            {
                ApplyFrozenLanding(vyBeforeLand);
                return;
            }

            if (curState == LF2States.Burning)
            {
                _character.BrokenEffectCreate(302);
                StateFalling("fell_onto_ground", vyBeforeLand);
                return;
            }

            if (curState == LF2States.Rowing &&
                StateRowing("fall_onto_ground", vyBeforeLand))
            {
                return;
            }

            StateFalling("fall_onto_ground", vyBeforeLand);
        }

        /// <summary>
        /// 倒地/受击/燃烧后的落地状态机。
        /// 这里负责决定是继续滚、直接躺、还是重置到普通站立/蹲伏。
        /// </summary>
        public bool StateFalling(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    return HandleFallingFrameEvent();

                case "TU":
                    return HandleFallingTuEvent();

                case "fell_onto_ground":
                case "fall_onto_ground":
                    return HandleFallingGroundEvent(eventData);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 冰冻状态的附加行为。
        /// 目前主要在状态退出时播放碎冰效果和音效。
        /// </summary>
        public bool StateFrozen(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    _character.BrokenEffectCreate(212);
                    _character.QueueBattleSound(NTSDGlobal.Sound.IceShatter);
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 躺地状态的进入/退出处理。
        /// 这里会清掉部分受击计数，并在血量归零时标记死亡。
        /// </summary>
        public bool StateLying(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    _character.HitCounters.ResetFall();
                    _character.HitCounters.ResetBdefend();

                    if (_character.Health.HP <= 0)
                    {
                        _character.Dead = true;
                        if (_character.DeadBlinkCountInternal < 0)
                            _character.DeadBlinkCountInternal = 0;
                    }
                    return false;

                case "state_exit":
                    _character.Effect.TimeIn = 0;
                    _character.Effect.TimeOut = 30;
                    _character.Effect.Blink = true;
                    _character.Effect.Super = true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 燃烧状态的特殊处理。
        /// 燃烧会不断刷碎裂效果，并在落地时转去摔落分支。
        /// </summary>
        public bool StateBurning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    _character.BrokenEffectCreate(302, 1);
                    return false;

                case "fall_onto_ground":
                    _character.BrokenEffectCreate(302);
                    goto case "fell_onto_ground";

                case "fell_onto_ground":
                    return StateFalling("fell_onto_ground", eventData);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 冰冻落地时的特殊伤害与反弹处理。
        /// 冻结对象不会直接按普通受击逻辑走，所以这里单独处理。
        /// </summary>
        private void ApplyFrozenLanding(double vyBeforeLand) // P0-f-2b B2-1: float→double
        {
            _character.Runtime.Y = 0f;

            if (vyBeforeLand <= 17f && _character.Runtime.Vx <= 9f && _character.Runtime.Vx >= -9f)
            {
                _character.Runtime.Vx *= 0.3333333333333333; // P0-f-2b B2-2: VALUE-BUG 1f/3f→0.3333333333333333 (baseline Physics.cs Vx*=0.3333333333333333)
                _character.Runtime.Vy = 0f;
                return;
            }

            int injury = _character.FallDamageDiv == 0 ? 10 : 1000 / _character.FallDamageDiv;
            _character.Health.HP -= injury;

            _character.Runtime.Vy = -3.5f;
            if (_character.Runtime.Vx > 7f)
                _character.Runtime.Vx = 7f;
            if (_character.Runtime.Vx < -7f)
                _character.Runtime.Vx = -7f;
            _character.ImmediateFrame(LF2StandardFrames.FallingFront5);
        }

        /// <summary>
        /// 落地时把武器携带重量换算成额外伤害。
        /// 这是 LF2 里“拿着太重的东西会摔得更惨”的那部分规则。
        /// </summary>
        private void ApplyLandingWeaponCountDamage()
        {
            if (_character.WeaponCount == 0 || _character.Health == null)
                return;

            int damage = _character.WeaponCount < 0 ? -_character.WeaponCount : _character.WeaponCount;
            if (_character.FallDamageDiv > 0)
                damage = damage * 100 / _character.FallDamageDiv;

            _character.Health.HP -= damage;
            _character.Health.HPBound -= damage;
            _character.WeaponCount = 0;
        }

        /// <summary>
        /// 摔落状态的 frame 事件。
        /// 负责从 FallingFront / FallingBack 逐步切到后续帧段。
        /// </summary>
        private bool HandleFallingFrameEvent()
        {
            int fn = _character.Frame.N;
            if (_character.Effect.Dvy <= 0f)
            {
                switch (fn)
                {
                    case LF2StandardFrames.FallingFront:
                        _character.Trans.SetNext(LF2StandardFrames.FallingFront1);
                        _character.Trans.SetWait((int)NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FallWait180, _character.Effect.Dvy));
                        break;
                    case LF2StandardFrames.FallingFront1:
                        _character.Trans.SetNext(LF2StandardFrames.FallingFront2);
                        double vy181 = _character.Runtime.Vy == 0f ? 5f : System.Math.Abs(_character.Runtime.Vy);
                        if (_character.Runtime.Vy == 0f)
                            _character.Runtime.Vy = 5f;
                        if (vy181 <= 4f)
                            _character.Trans.SetWait(2);
                        else if (vy181 < 7f)
                            _character.Trans.SetWait(3);
                        else
                            _character.Trans.SetWait(4);
                        break;
                    case LF2StandardFrames.FallingFront2:
                        _character.Trans.SetNext(LF2StandardFrames.FallingFront3);
                        break;
                    case LF2StandardFrames.FallingBack:
                        if (_character.Runtime.Vy == 0f)
                            _character.Runtime.Vy = 5f;
                        _character.Trans.SetNext(LF2StandardFrames.FallingBack1);
                        break;
                    case LF2StandardFrames.FallingBack1:
                        _character.Trans.SetNext(LF2StandardFrames.FallingBack2);
                        break;
                    case LF2StandardFrames.FallingBack2:
                        _character.Trans.SetNext(LF2StandardFrames.FallingBack3);
                        break;
                }
            }
            else
            {
                switch (fn)
                {
                    case LF2StandardFrames.FallingFront:
                        _character.Trans.SetNext(LF2StandardFrames.FallingFront5);
                        _character.Trans.SetWait(1);
                        break;
                    case LF2StandardFrames.FallingBack:
                        _character.Trans.SetNext(LF2StandardFrames.FallingBack5);
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// 摔落状态的 TU 事件。
        /// 负责根据当前速度微调受击帧，让动画跟着真实运动变化。
        /// </summary>
        private bool HandleFallingTuEvent()
        {
            if (_character.HitCounters.Fall > 0)
                _character.HitCounters.AddFall(-1);

            if (_character.Runtime.Y < 0f && _character.Runtime.Y + _character.Runtime.Vy < -0.0001f)
            {
                int curFn = _character.Frame.N;
                if (curFn < LF2StandardFrames.FallingFront5)
                {
                    int newFn;
                    if (_character.Runtime.Vy < -8f)
                        newFn = LF2StandardFrames.FallingFront;
                    else if (_character.Runtime.Vy < 1f)
                        newFn = LF2StandardFrames.FallingFront1;
                    else if (_character.Runtime.Vy < 8f)
                        newFn = LF2StandardFrames.FallingFront2;
                    else
                        newFn = LF2StandardFrames.FallingFront3;
                    if (newFn != curFn)
                        _character.ImmediateFrame(newFn);
                }
                else if (curFn > LF2StandardFrames.FallingFront5 &&
                         curFn < LF2StandardFrames.FallingBack5)
                {
                    int newFn;
                    if (_character.Runtime.Vy < -8f)
                        newFn = LF2StandardFrames.FallingBack;
                    else if (_character.Runtime.Vy < 1f)
                        newFn = LF2StandardFrames.FallingBack1;
                    else if (_character.Runtime.Vy < 8f)
                        newFn = LF2StandardFrames.FallingBack2;
                    else
                        newFn = LF2StandardFrames.FallingBack3;
                    if (newFn != curFn)
                        _character.ImmediateFrame(newFn);
                }
            }

            return false;
        }

        /// <summary>
        /// 摔到地面时的最终结算。
        /// 这里决定是弹起、躺地，还是回到普通蹲伏。
        /// </summary>
        private bool HandleFallingGroundEvent(object eventData)
        {
            // P0-f-2b B2-1: landing Vy is now double (CharacterMechanics.verticalVelocityBeforeLanding double).
            // Accept double (primary, new path) AND float (defensive: any residual boxed-float caller) so the
            // unbox never silently falls back to Runtime.Vy on a type mismatch. Fallback unchanged otherwise.
            double vy = eventData is double landedVy ? landedVy
                        : (eventData is float landedVyF ? landedVyF : _character.Runtime.Vy);
            _character.Runtime.Y = 0f;
            _character.Runtime.Vy = 0f;
            int curState = _character.GetState();
            if (curState == LF2States.Falling || curState == LF2States.Burning)
            {
                if (curState == LF2States.Falling || curState == LF2States.Burning)
                {
                    _character.QueueBattleSound("SFX_006");
                    ApplyLandingWeaponCountDamage();

                    bool highSpeed = vy > 11.0f || _character.Runtime.Vx > 9.0f || _character.Runtime.Vx < -9.0f
                                     || curState == LF2States.Burning;

                    if (highSpeed)
                    {
                        _character.Runtime.Vy = -3.5f;
                        if (_character.Runtime.Vx > 7f)
                            _character.Runtime.Vx = 7f;
                        if (_character.Runtime.Vx < -7f)
                            _character.Runtime.Vx = -7f;
                        int bounceFrame = (_character.Frame.N >= LF2StandardFrames.FallingBack &&
                                           curState != LF2States.Burning)
                            ? LF2StandardFrames.FallingBack5
                            : LF2StandardFrames.FallingFront5;
                        _character.ImmediateFrame(bounceFrame);
                    }
                    else
                    {
                        _character.Runtime.Vx *= 0.3333333333333333; // P0-f-2b B2-2: VALUE-BUG 1f/3f→0.3333333333333333 (baseline Physics.cs Vx*=0.3333333333333333)
                        _character.AttackingCounter = 0;
                        int landFrame = (_character.Frame.N >= LF2StandardFrames.FallingBack)
                            ? LF2StandardFrames.LyingBack
                            : LF2StandardFrames.Lying;
                        _character.ImmediateFrame(landFrame);
                    }
                }
            }
            else
            {
                _character.Runtime.Vx *= 0.3333333333333333; // P0-f-2b B2-2: VALUE-BUG 1f/3f→0.3333333333333333 (baseline Physics.cs Vx*=0.3333333333333333)
                _character.AttackingCounter = 0;

                int landFrame;
                int curFrameState = _character.GetState();
                if (curFrameState == LF2States.CustomSkill1)
                    landFrame = 94;
                else if (_character.Frame.N == LF2StandardFrames.JumpingAir || curFrameState == LF2States.Rowing)
                    landFrame = LF2StandardFrames.Crouch;
                else
                    landFrame = LF2StandardFrames.Crouch2;

                _character.ImmediateFrame(landFrame);
            }

            return true;
        }
    }
}
