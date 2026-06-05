using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 当前对象的帧运行时信息，对应 C++ release 实体的 frame/prev_frame 字段语义。
    /// </summary>
    public class LF2FrameInfo
    {
        /// <summary>上一帧编号。</summary>
        public int PN { get; set; } = 0;

        /// <summary>当前帧编号。</summary>
        public int N { get; set; } = 0;

        /// <summary>当前帧 DAT 数据。</summary>
        public LF2FrameData D { get; set; }

    }

    /// <summary>
    /// 对象身上的临时效果状态，对应正式版的击中特效、闪烁、震动、定身等效果。
    /// </summary>
    public class LF2EffectState
    {
        /// <summary>当前效果编号，-99 表示无效果。</summary>
        public int Num { get; set; } = -99;

        /// <summary>效果结束时写入的 X 速度。</summary>
        public float Dvx { get; set; } = 0;

        /// <summary>效果结束时写入的 Y 速度。</summary>
        public float Dvy { get; set; } = 0;

        /// <summary>是否处于定身/卡住效果。</summary>
        public bool Stuck { get; set; } = false;

        /// <summary>精灵震荡幅度。</summary>
        public int Oscillate { get; set; } = 0;

        /// <summary>是否启用闪烁效果。</summary>
        public bool Blink { get; set; } = false;

        /// <summary>是否处于无敌/特殊保护效果。</summary>
        public bool Super { get; set; } = false;

        /// <summary>效果进入计时。</summary>
        public int TimeIn { get; set; } = 0;

        /// <summary>效果剩余时间。</summary>
        public int TimeOut { get; set; } = 0;

        public int OscillateDirection { get; set; } = 1;
        public int BlinkCounter { get; set; } = 0;

        public void Reset()
        {
            Num = -99;
            Dvx = 0;
            Dvy = 0;
            Stuck = false;
            Oscillate = 0;
            Blink = false;
            Super = false;
            TimeIn = 0;
            TimeOut = 0;
            OscillateDirection = 1;
            BlinkCounter = 0;
        }
    }

    /// <summary>
    /// 生命和资源数值。HP/MP/PP 的含义按正式版对象字段使用。
    /// </summary>
    public class LF2Health
    {
        private NTSDEntityRuntime _runtime;
        private int _hp = 100;
        private int _mp = 100;
        private int _pp = 0;
        private int _maxPP = 500;
        private int _ppBound = 500;
        private int _hpLost = 0;
        private int _hpBound = 100;
        private int _maxMP = 0;

        /// <summary>
        /// 绑定正式版实体运行时字段。绑定后 HP/MP/PP 读写都落到 Runtime，避免同一状态出现多份副本。
        /// </summary>
        public void BindRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null) return;

            runtime.HP = HP;
            runtime.MP = MP;
            runtime.PP = PP;
            runtime.PPMax = MaxPP;
            runtime.PPBound = PPBound;
            runtime.HPLost = HPLost;
            runtime.HPBound = HPBound;
            runtime.MPMax = MaxMP;
            _runtime = runtime;
        }

        /// <summary>当前生命值。</summary>
        public int HP { get => _runtime?.HP ?? _hp; set { if (_runtime != null) _runtime.HP = value; else _hp = value; } }
        /// <summary>当前 MP 值。</summary>
        public int MP { get => _runtime?.MP ?? _mp; set { if (_runtime != null) _runtime.MP = value; else _mp = value; } }

        /// <summary>PP 当前值。</summary>
        public int PP { get => _runtime?.PP ?? _pp; set { if (_runtime != null) _runtime.PP = value; else _pp = value; } }

        /// <summary>PP 最大值。</summary>
        public int MaxPP { get => _runtime?.PPMax ?? _maxPP; set { if (_runtime != null) _runtime.PPMax = value; else _maxPP = value; } }

        /// <summary>PP 当前恢复上限。</summary>
        public int PPBound { get => _runtime?.PPBound ?? _ppBound; set { if (_runtime != null) _runtime.PPBound = value; else _ppBound = value; } }

        /// <summary>累计受到的 HP 伤害。</summary>
        public int HPLost { get => _runtime?.HPLost ?? _hpLost; set { if (_runtime != null) _runtime.HPLost = value; else _hpLost = value; } }

        /// <summary>HP 当前恢复上限。</summary>
        public int HPBound { get => _runtime?.HPBound ?? _hpBound; set { if (_runtime != null) _runtime.HPBound = value; else _hpBound = value; } }
        /// <summary>MP 最大值，用于部分伤害/资源换算。</summary>
        public int MaxMP { get => _runtime?.MPMax ?? _maxMP; set { if (_runtime != null) _runtime.MPMax = value; else _maxMP = value; } }
    }

    /// <summary>
    /// LF2 控制输入接口，角色、AI、回放输入都通过该接口提供方向和按键状态。
    /// </summary>
    public interface ILF2Controller
    {
        bool IsUp { get; }
        bool IsDown { get; }
        bool IsLeft { get; }
        bool IsRight { get; }
        bool IsAttack { get; }
        bool IsJump { get; }
        bool IsDefend { get; }
        public SimInputBuffer InputBuffer { get; set; }
        int Dirv();
        (int dx, int dz) GetMoveInput();
        void SetInputID(int inputId);

    }

    public enum DIRECTION 
    {
        RIGHT = 1,
        LEFT = -1,
    }

    /// <summary>
    /// 所有可参与战斗模拟的 LF2 活动对象基类。
    /// 角色、武器、技能对象共享帧推进、状态事件、效果处理、碰撞体积和模拟生命周期。
    /// </summary>
    public abstract class LF2LivingObject : LF2Entity
    {

        #region 基础模块字段

        /// <summary>生命和资源状态。</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>itr 命中冷却跟踪器。</summary>
        public override LF2ItrRestTracker ItrRest { get; protected set; }

        #endregion

        #region 状态字段

        /// <summary>当前被本对象抓取的目标。</summary>
        public LF2LivingObject Catching { get; set; } = null;

        /// <summary>当前控制器。</summary>
        public ILF2Controller Controller { get; set; }

        /// <summary>对象是否已经死亡。</summary>
        public bool Dead { get; set; } = false;

        /// <summary>最近一次命中本对象的攻击者。</summary>
        public LF2LivingObject Attacker { get; set; } = null;

        /// <summary>
        /// 本帧累计命中次数。正式版在帧后处理阶段用它计算平均击退速度。
        /// </summary>
        public int HitCount
        {
            get => Runtime.HitCount;
            set => Runtime.HitCount = value;
        }

        /// <summary>
        /// 命中确认窗口计数。部分 kind 命中会短时间影响后续帧切换。
        /// </summary>
        public int HitConfirmEa
        {
            get => Runtime.HitConfirmEa;
            set => Runtime.HitConfirmEa = value;
        }

        #endregion

        #region Unity 架构字段

        /// <summary>当前帧数据包装器。</summary>
        public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache?.Wrapper;

        #endregion

        #region 帧系统

        /// <summary>
        /// 应用当前帧的 dvx/dvy/dvz 到物理速度。
        /// dvx/dvy/dvz 大于 500 时按正式版特殊编码转换为直接速度。
        /// </summary>
        public virtual void FrameForce()
        {
            if (Frame.D == null || PS == null) return;

            bool isFalling = GetState() == LF2States.Falling;
            float dvx = Frame.D.dvx;
            float dvy = Frame.D.dvy;
            float dvz = Frame.D.dvz;

            if (dvx != 0 && !isFalling)
            {
                if (dvx > 500f)
                {
                    PS.vx = dvx - 550f;
                }
                else if (dvx > 0f)
                {
                    float avx = Mathf.Abs(PS.vx);
                    if (PS.y < 0 || avx < dvx)
                        PS.vx = Dirh() * dvx;
                }
                else // dvx < 0
                {
                    PS.vx -= Dirh();
                }
            }

            if (dvz != 0f)
            {
                if (dvz > 500f)
                    PS.vz = dvz - 550f;
                else
                    PS.vz = Dirv() * dvz;
            }

            if (dvy != 0f)
            {
                if (dvy > 500f)
                    PS.vy = dvy - 550f;
                else
                    PS.vy += dvy;
            }
        }

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            int prevFn = Frame.N;
            Log.LogState(Name, "Frame", $"{Frame.N} -> {targetFrameId}");
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;
            AttackingCounter = 0;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
            {
                Log.Warn("[LF2Character] Invalid frame ID: {0}", targetFrameId);
                return;
            }

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
            {
                StateExitEvent();
            }

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                ResetStateRuntime();

                AttackingCounter = 0;

                StateEntryEvent();

            }

            if (switchDirAfterTrans)
            {
                SwitchDir(PS.dir == "right"?"left":"right");
            }

            FrameUpdateInternal();
        }

        /// <summary>
        /// 完成一次帧切换后的内部刷新：显示图片、应用帧速度、设置 wait/next、触发帧事件和音效。
        /// </summary>
        private void FrameUpdateInternal()
        {
            if (Frame.D != null && Frame.D.pic >= 0)
            {
                Sprite.ShowPic(Frame.D.pic);
            }

            FrameForce();

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (Frame.D != null && !string.IsNullOrEmpty(Frame.D.sound))
            {
                AppManager.Instance?.SoundPlayer?.PlaySfx(Frame.D.sound);
            }
        }

        #endregion

        #region Tick 循环

        /// <summary>
        /// Transit 阶段：先处理 combo，再根据 FrameDelay 决定是否推进帧和状态事件。
        /// </summary>
        public virtual void Transit()
        {
            ComboUpdate();

            int prevDelay = FrameDelay;
            if (FrameDelay > 0) FrameDelay--;
            else if (FrameDelay < 0) FrameDelay++;

            if (prevDelay != 0)
                return;

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
            }
            else
            {
                Trans.Trans();
            }

            Effect.TimeIn--;

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
            }
            else
            {
                TransitEvent();
            }
        }

        /// <summary>
        /// TU 阶段：推进命中计数、治疗、震动、帧强制、效果和对象主逻辑。
        /// </summary>
        public override void TUUpdate()
        {
            //   0x004139C7: [esi+0B8h] HitStateCount
            //   0x004139D8: [esi+0EAh] HitConfirmEa
            if (HitCounters != null)
            {
                if (HitCounters.HitStateCount > 0) HitCounters.SetHitStateCount(HitCounters.HitStateCount - 1);
            }
            if (HitConfirmEa > 0) HitConfirmEa--;

            if (HealTimer / 1000 == 1 && Health?.HP > 0)
            {
                HealTimer--;
                if ((HealTimer & 7) == 0)
                {
                    if (Health.HP < Health.HPBound)
                    {
                        int newHp = Health.HP + 8;
                        Health.HP = newHp > Health.HPBound ? Health.HPBound : newHp;
                    }
                    else
                    {
                        HealTimer = 0;
                    }
                }
                if (HealTimer % 1000 == 0)
                    HealTimer = 0;
            }

            if (ShakeTimer > 0) ShakeTimer--;
            else if (ShakeTimer < 0) ShakeTimer++;

            if (FrameDelay != 0) return;

            FrameForce();

            ProcessEffects();

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
            }
            else
            {
                TUEvent();
            }

            if (Health.HP <= 0 && !Dead)
            {
                DieEvent();
                Dead = true;
            }

        }

        /// <summary>
        /// 连招输入更新入口。角色子类按需要重写。
        /// </summary>
        protected virtual void ComboUpdate()
        {
        }

        #endregion

        #region 效果系统

        /// <summary>
        /// 创建或覆盖当前受击效果状态。
        /// 高优先级效果可以覆盖低优先级效果；已有有效效果会延长持续时间。
        /// </summary>
        public virtual void EffectCreate(int num, int duration, float dvx = 0, float dvy = 0)
        {
            if (num < Effect.Num) return;

            int efid = num + NTSDGlobal.Gameplay.EffectNumToId;
            int oscillate = NTSDSpec.GetOscillateOrDefault(efid);
            if (oscillate != 0)
            {
                Effect.Oscillate = oscillate;
            }

            Effect.Stuck = true;
            if (dvx != 0) Effect.Dvx = dvx;
            if (dvy != 0) Effect.Dvy = dvy;

            if (Effect.Num >= 0)
            {
                if (Effect.TimeIn > 0) Effect.TimeIn = 0;
                if (duration > Effect.TimeOut) Effect.TimeOut = duration;
            }
            else
            {
                Effect.TimeIn = 0;
                Effect.TimeOut = duration;
            }

            Effect.Num = num;
        }

        /// <summary>
        /// 创建视觉效果。基类不直接生成 spark，角色子类可按需要重写。
        /// </summary>
        public virtual void VisualEffectCreate(int num, PhysicsState.BattleVolume rect, bool righttip = false, int variant = 0, bool withSound = false)
        {
        }
        #endregion

        #region 方向系统

        /// <summary>
        /// 获取垂直输入方向，交给当前控制器实现。
        /// </summary>
        public override int Dirv()
        {
            return Controller.Dirv();
        }
        
        #endregion

        #region 命中和统计

        /// <summary>
        /// 基类命中入口，只做 vrest 冷却检查；实际受击逻辑由角色或具体对象重写。
        /// </summary>
        public virtual bool Hit(InteractionArea itr, LF2Entity attacker, UnityEngine.Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!ItrVrestTest(attacker.StableId)) return false;
            return true;
        }

        /// <summary>
        /// 应用基础伤害：扣 HP、累计 HPLost，并降低 HPBound。
        /// </summary>
        public void ApplyDirectInjury(int inj)
        {
            Injury(inj);
        }

        /// <summary>
        /// 应用基础伤害：扣 HP、累计 HPLost，并降低 HPBound。
        /// </summary>
        protected virtual void Injury(int inj)
        {
            if (inj <= 0 || Health == null) return;
            Health.HP -= inj;
            Health.HPLost += inj;
            Health.HPBound -= Mathf.CeilToInt(inj / 3f);
        }
#endregion

        #region 位置和帧数据查询
        public override float GetSpriteWidthPxForCollision()
        {
            if (_FrameDataWrapper?.characterData?.files == null || _FrameDataWrapper.characterData.files.Count == 0)
                return 0f;
            return _FrameDataWrapper.characterData.files[0].width + 1;
        }

        #endregion

        protected virtual void ResetStateRuntime()
        {
        }

        #region 直接帧切换

        /// <summary>
        /// 立即写入指定帧，不经过 wait/next 推进，也不触发状态出入事件。
        /// C++ release 的命中、抓取、cpoint 等路径多为直接写 frame 字段。
        /// </summary>
        public override void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N  = frameId;
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        #endregion

        #region 对象生命周期

        public abstract override LF2ObjectType ObjectTypeEnum { get; }
        public abstract override void Reset();
        public abstract override void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>
        /// 销毁当前对象的可视表现。
        /// </summary>
        public override void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>
        /// next=1000 时的销毁流程：触发 destroy 事件、隐藏对象、释放渲染器和逻辑对象。
        /// </summary>
        public override void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
            // 释放渲染器时会触发 ResetState -> Reset -> Unregister。
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        #endregion

        #region 模拟接口

        public override void SimTransit(int tickIndex) => Transit();
        public override void SimTU(int tickIndex) => TUUpdate();

        /// <summary>
        /// PreInteraction 默认不处理，角色、武器或技能按需要重写。
        /// </summary>
        public override void SimPreInteraction(int tickIndex) { }

        #endregion

        #region 可选角色模块

        public virtual LF2HitCountersModule HitCounters => null;

        public override int AttackExempt
        {
            get => HitCounters?.AttackExempt ?? 0;
            set => HitCounters?.SetAttackExempt(value);
        }

        #endregion

        #region 效果内部处理

        /// <summary>
        /// 推进当前效果状态：震荡、闪烁、效果结束和延迟速度写入。
        /// </summary>
        protected virtual void ProcessEffects()
        {
            if (Effect.TimeIn >= 0) return;

            if (Effect.Oscillate != 0)
            {
                Effect.OscillateDirection = Effect.OscillateDirection == 1 ? -1 : 1;
                Sprite?.SetXY(PS.sx + Effect.Oscillate * Effect.OscillateDirection, PS.sy);
            }
            else if (Effect.Blink)
            {
                switch (Effect.BlinkCounter % 4)
                {
                    case 0:
                    case 1:
                        Sprite?.Hide();
                        break;
                    case 2:
                    case 3:
                        Sprite?.Show();
                        break;
                }
                Effect.BlinkCounter++;
            }

            if (Effect.TimeOut == 0)
            {
                Effect.Num = -99;
                if (Effect.Stuck) Effect.Stuck = false;
                if (Effect.Oscillate != 0)
                {
                    Effect.Oscillate = 0;
                    Sprite?.SetXY(PS.sx, PS.sy);
                }
                if (Effect.Blink)
                {
                    Effect.Blink = false;
                    Effect.BlinkCounter = 0;
                    Sprite?.Show();
                }
                if (Effect.Super) Effect.Super = false;
            }
            else if (Effect.TimeOut == -1)
            {
                if (Effect.Dvx != 0) PS.vx = Effect.Dvx;
                if (Effect.Dvy != 0) PS.vy = Effect.Dvy;
                Effect.Dvx = 0;
                Effect.Dvy = 0;
            }

            Effect.TimeOut--;
        }

        /// <summary>
        /// 根据 opoint facing 和父对象方向计算新对象朝向。
        /// </summary>
        protected override string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return (parentDir == "right") ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }

        #endregion
    }
}

