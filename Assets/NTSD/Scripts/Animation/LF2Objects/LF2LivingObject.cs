using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Tools;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 帧信息结构（对应 FLF $.frame）
    /// </summary>
    public class LF2FrameInfo
    {
        /// <summary>上一帧编号（对应 FLF $.frame.PN）</summary>
        public int PN { get; set; } = 0;

        /// <summary>当前帧编号（对应 FLF $.frame.N）</summary>
        public int N { get; set; } = 0;

        /// <summary>当前帧数据（对应 FLF $.frame.D）</summary>
        public LF2FrameData D { get; set; }

        /// <summary>动画状态（对应 FLF $.frame.ani）</summary>
        public LF2AnimationState Ani { get; set; } = new LF2AnimationState();
    }

    /// <summary>
    /// 动画状态（对应 FLF $.frame.ani）
    /// </summary>
    public class LF2AnimationState
    {
        public int i { get; set; } = 0;
        public bool up { get; set; } = true;
    }

    /// <summary>
    /// 效果状态（对应 FLF $.effect）
    /// </summary>
    public class LF2EffectState
    {
        /// <summary>效果编号（对应 FLF $.effect.num）</summary>
        public int Num { get; set; } = -99;

        /// <summary>X 方向速度变化（对应 FLF $.effect.dvx）</summary>
        public float Dvx { get; set; } = 0;

        /// <summary>Y 方向速度变化（对应 FLF $.effect.dvy）</summary>
        public float Dvy { get; set; } = 0;

        /// <summary>是否被卡住（对应 FLF $.effect.stuck）</summary>
        public bool Stuck { get; set; } = false;

        /// <summary>震荡幅度（对应 FLF $.effect.oscillate）</summary>
        public int Oscillate { get; set; } = 0;

        /// <summary>闪烁效果（对应 FLF $.effect.blink）</summary>
        public bool Blink { get; set; } = false;

        /// <summary>无敌状态（对应 FLF $.effect.super）</summary>
        public bool Super { get; set; } = false;

        /// <summary>效果生效时间（对应 FLF $.effect.timein）</summary>
        public int TimeIn { get; set; } = 0;

        /// <summary>效果消失时间（对应 FLF $.effect.timeout）</summary>
        public int TimeOut { get; set; } = 0;

        /// <summary>治疗效果（对应 FLF $.effect.heal）</summary>
        public object Heal { get; set; } = null;

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
            Heal = null;
            OscillateDirection = 1;
            BlinkCounter = 0;
        }
    }

    /// <summary>
    /// 生命值结构（对应 FLF $.health）
    /// </summary>
    public class LF2Health
    {
        public int HP { get; set; } = 100;
        public int MP { get; set; } = 100;
    }

    /// <summary>
    /// 控制器接口（对应 FLF $.con）
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
        int Dirv();
        (int dx, int dz) GetMoveInput();
    }

    /// <summary>
    /// LF2 所有活动对象的基类（纯 C# 类，不继承 MonoBehaviour）
    /// 实现 ILF2Object（包含 ISimObject）
    ///
    /// 完全对齐 FLF livingobject.js
    /// 所有活动对象（角色、武器、特效）都继承此类
    ///
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\livingobject.js
    /// </summary>
    public abstract class LF2LivingObject : ILF2Object
    {
        #region 声明字段 - 身份标识

        /// <summary>对象名称（对应 FLF $.name = data.bmp.name）</summary>
        public string Name { get; set; }

        /// <summary>唯一ID，由场景分配（对应 FLF $.uid）</summary>
        public int StableId { get; protected set; }

        /// <summary>对象ID（对应 FLF $.id = thisID）</summary>
        public int ObjectId { get; set; }

        /// <summary>队伍 ID（对应 FLF $.team）</summary>
        public int Team { get; set; }

        /// <summary>所有者 ID（对应 FLF $.owner，用于飞行道具等）</summary>
        public int OwnerId { get; set; } = -1;

        /// <summary>对象类型字符串（对应 FLF livingobject.prototype.type）</summary>
        public virtual LF2ObjectType Type => (LF2ObjectType)GameDataManager.Instance?.GetObjectById(ObjectId)?.type;

        /// <summary>
        /// 每个状态是否允许切换方向（对应 FLF livingobject.prototype.states_switch_dir）
        /// 子类通过 InitializeStatesSwitchDir() 填充
        /// </summary>
        protected Dictionary<int, bool> _statesSwitchDir;

        #endregion

        #region 声明字段 - 世界句柄

        /// <summary>匹配/世界句柄（对应 FLF $.match）</summary>
        public SimulationWorld Match => SimulationTickDriver.Instance?.World;

        #endregion

        #region 声明字段 - 核心模块

        /// <summary>精灵模块（对应 FLF $.sp）</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>生命值（对应 FLF $.health）</summary>
        public LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>帧信息（对应 FLF $.frame）</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>物理系统（对应 FLF $.mech）</summary>
        public CharacterMechanics Mech { get; protected set; }

        /// <summary>物理状态（对应 FLF $.ps = $.mech.create_metric()）</summary>
        public PhysicsState PS { get; protected set; }

        /// <summary>帧转换器（对应 FLF $.trans）</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>交互冷却（对应 FLF $.itr）</summary>
        public LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>效果状态（对应 FLF $.effect）</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>帧数据缓存</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        #endregion

        #region 声明字段 - 状态字段

        /// <summary>状态内存，状态切换时自动清空（对应 FLF $.statemem）</summary>
        public Dictionary<string, object> StateMem { get; protected set; } = new Dictionary<string, object>();

        /// <summary>抓取状态（对应 FLF $.catching）</summary>
        public int Catching { get; set; } = 0;

        /// <summary>允许切换方向（对应 FLF $.allow_switch_dir）</summary>
        public bool AllowSwitchDir { get; set; } = true;

        /// <summary>控制器（对应 FLF $.con）</summary>
        public ILF2Controller Controller { get; set; }

        /// <summary>是否死亡（对应 FLF $.dead）</summary>
        public bool Dead { get; set; } = false;

        #endregion

        #region 声明字段 - 状态处理器

        /// <summary>
        /// 状态处理器委托（对应 FLF states[N] = function(event, K) {}）
        /// </summary>
        protected delegate bool StateHandler(string eventType, object eventData = null);

        /// <summary>
        /// 状态处理器字典 - 子类通过 InitializeStates() 注册
        /// </summary>
        protected Dictionary<int, StateHandler> _states = new Dictionary<int, StateHandler>(20);

        /// <summary>
        /// 状态处理器的帧号返回通道（对齐 FLF state_update 可返回 int 的特性）
        /// handler 写入 > 0 的值表示要跳转的帧号，调用方读完后必须清零
        /// </summary>
        public int StateReturnFrame { get; protected set; } = 0;

        #endregion

        #region 声明字段 - Unity 架构适配

        /// <summary>Character Hub 引用（子类可重写）</summary>
        public Character _CharacterHub { get; protected set; }

        /// <summary>帧数据包装器</summary>
        public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache?.Wrapper;

        #endregion

        #region 初始化函数

        /// <summary>
        /// 初始化状态处理器 - 子类必须实现
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// 初始化设置（对应 FLF livingobject.prototype.setup）
        /// 参考：FLF livingobject.js:101-103
        /// </summary>
        public virtual void Setup()
        {
            StateUpdate("setup");
        }

        #endregion

        #region 功能逻辑 - 体积查询

        /// <summary>
        /// 获取碰撞体积（对应 FLF livingobject.prototype.vol_body）
        /// 参考：FLF livingobject.js vol_body
        /// </summary>
        public List<PhysicsState.FlfVolume> VolBody()
        {
            if (PS == null || Frame.D == null) return new List<PhysicsState.FlfVolume>();
            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return new List<PhysicsState.FlfVolume>();
            return PS.GetBodyVolumes(Frame.D.bodies, Frame.D.centerx, Frame.D.centery, spriteWidthPx);
        }

        /// <summary>
        /// 获取交互体积（对应 FLF livingobject.prototype.vol_itr）
        /// 按 kind 过滤
        /// 参考：FLF livingobject.js vol_itr
        /// </summary>
        public List<PhysicsState.FlfVolume> VolItr(int kind)
        {
            if (PS == null || Frame.D == null) return new List<PhysicsState.FlfVolume>();
            var itrs = Frame.D.itrs;
            if (itrs == null || itrs.Count == 0) return new List<PhysicsState.FlfVolume>();

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return new List<PhysicsState.FlfVolume>();

            var filtered = new List<InteractionArea>();
            for (int i = 0; i < itrs.Count; i++)
            {
                if (itrs[i].kind == kind) filtered.Add(itrs[i]);
            }

            if (filtered.Count == 0) return new List<PhysicsState.FlfVolume>();
            return PS.GetItrVolumes(filtered, Frame.D.centerx, Frame.D.centery, spriteWidthPx);
        }

        #endregion

        #region 功能逻辑 - 外力系统

        /// <summary>
        /// 旋风力（对应 FLF livingobject.prototype.whirlwind_force）
        /// 参考：FLF livingobject.js whirlwind_force
        /// </summary>
        public virtual void WhirlwindForce(PhysicsState.FlfVolume rect)
        {
            if (PS == null || Mech == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);
            PS.vy -= 2f / mass;
            float cx = rect.x + rect.vx + rect.w * 0.5f;
            float cz = rect.z;
            PS.vx -= Sign(PS.x - cx) * 2f / mass;
            PS.vz -= Sign(PS.z - cz) * 0.5f / mass;
        }

        /// <summary>
        /// 笛子力（对应 FLF livingobject.prototype.flute_force）
        /// 参考：FLF livingobject.js flute_force
        /// </summary>
        public virtual void FluteForce()
        {
            if (PS == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            const float lowLevel = -140f;
            const float midLevel = -160f;
            const float highLevel = -180f;

            Effect.Super = true;
            PS.vx = 0;
            PS.vz = 0;

            if (PS.y > lowLevel)
            {
                PS.vy = (PS.vy <= 0) ? -7.5f : -PS.vy / 2f;
            }
            else if (PS.y <= lowLevel && PS.y > midLevel)
            {
                PS.vy -= mass / 2f;
            }
            else if (PS.y <= midLevel && PS.y > highLevel)
            {
                PS.vy += mass / 2f;
            }

            switch (Type)
            {
                case LF2ObjectType.Character:
                    if (Frame.N >= 55) TransitionToFrame(40, 20);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) TransitionToFrame(1, 20);
                    break;
                //case "character":
                //    Trans?.Frame(PS.vy > 0 ? 181 : 182, 20);
                //    break;
            }
        }

        private static float Sign(float x) => x > 0 ? 1f : -1f;

        #endregion

        #region 功能逻辑 - 属性查询

        /// <summary>
        /// 查询对象属性（对应 FLF livingobject.prototype.proper）
        /// 单参数版本：查询自身 ID 的属性
        /// 参考：FLF livingobject.js proper
        /// </summary>
        public object Proper(string prop)
        {
            return NTSDSpec.Proper(ObjectId, prop);
        }

        /// <summary>
        /// 查询对象属性（对应 FLF livingobject.prototype.proper）
        /// 双参数版本：查询指定 ID 的属性
        /// </summary>
        public object Proper(int id, string prop)
        {
            return NTSDSpec.Proper(id, prop);
        }

        #endregion

        #region 功能逻辑 - 帧系统

        /// <summary>
        /// 应用帧力（对应 FLF livingobject.prototype.frame_force）
        /// 参考：FLF livingobject.js:133-148
        /// </summary>
        public virtual void FrameForce()
        {
            if (Frame.D == null || PS == null) return;

            if (Frame.D.dvx != 0)
            {
                float avx = Mathf.Abs(PS.vx);
                if (PS.y < 0 || avx < Frame.D.dvx)
                {
                    PS.vx = Dirh() * Frame.D.dvx;
                }
                if (Frame.D.dvx < 0)
                {
                    PS.vx = PS.vx - Dirh();
                }
            }

            if (Frame.D.dvz != 0) PS.vz = Dirv() * Frame.D.dvz;
            if (Frame.D.dvy != 0) PS.vy += Frame.D.dvy;
            if (Frame.D.dvx == 550) PS.vx = 0;
            if (Frame.D.dvy == 550) PS.vy = 0;
            if (Frame.D.dvz == 550) PS.vz = 0;
        }

        /// <summary>
        /// 帧动画振荡（对应 FLF livingobject.prototype.frame_ani_oscillate）
        /// 在 [a, b] 范围内来回播放帧
        /// 参考：FLF livingobject.js frame_ani_oscillate
        /// </summary>
        public virtual void FrameAniOscillate(int a, int b)
        {
            var ani = Frame?.Ani;
            if (ani == null) return;

            if (ani.i < a || ani.i > b)
            {
                ani.up = true;
                ani.i = a + 1;
            }

            if (ani.i < b && ani.up)
            {
                Trans?.SetNext(ani.i++);
            }
            else if (ani.i > a && !ani.up)
            {
                Trans?.SetNext(ani.i--);
            }

            if (ani.i == b) ani.up = false;
            if (ani.i == a) ani.up = true;
        }

        /// <summary>
        /// 帧动画序列（对应 FLF livingobject.prototype.frame_ani_sequence）
        /// 在 [a, b] 范围内循环播放帧
        /// 参考：FLF livingobject.js frame_ani_sequence
        /// </summary>
        public virtual void FrameAniSequence(int a, int b)
        {
            var ani = Frame?.Ani;
            if (ani == null) return;

            if (ani.i < a || ani.i > b)
            {
                ani.i = a + 1;
            }

            Trans?.SetNext(ani.i++);

            if (ani.i > b)
            {
                ani.i = a;
            }
        }

        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) 
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
            {
                Log.Warn("[LF2Character] Invalid frame ID: {0}", targetFrameId);
                return;
            }

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
            {
                // 状态退出事件
                StateUpdate("state_exit");
            }

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                StateMem.Clear();

                bool oldSwitchDir = AllowSwitchDir;
                AllowSwitchDir = GetStatesSwitchDir(Frame.D.state);

                StateUpdate("state_entry");

                if (!switchDirAfterTrans)
                {
                    if (AllowSwitchDir && !oldSwitchDir)
                    {
                        if (Controller.IsLeft)
                            SwitchDir(DIRECTION.LEFT);
                        if (Controller.IsRight)
                            SwitchDir(DIRECTION.RIGHT);
                    }
                }
            }

            if (switchDirAfterTrans)
            {
                SwitchDir(PS.dir == "right"?"left":"right");
            }

            FrameUpdateInternal();
        }

        /// <summary>
        /// 帧更新（内部）
        /// 对应 FLF frame_update()
        /// </summary>
        private void FrameUpdateInternal()
        {
            // 更新精灵
            if (Frame.D != null && Frame.D.pic >= 0)
            {
                Sprite.ShowPic(Frame.D.pic);
            }

            // 应用帧力
            if (!StateUpdate("frame_force"))
            {
                FrameForce();
            }

            Trans.SetWait(Frame.D.wait, 99);
            Trans.SetNext(Frame.D.next, 99);

            // 状态 frame 事件
            StateUpdate("frame");

            // 播放音效
            if (Frame.D != null && !string.IsNullOrEmpty(Frame.D.sound))
            {
                // TODO: 播放音效
            }
        }

        public virtual bool GetStatesSwitchDir(int stateId) 
        {
            return false;
        }

        #endregion

        #region 功能逻辑 - Tick 循环

            /// <summary>
            /// Transit 阶段（对应 FLF livingobject.prototype.transit）
            /// 参考：FLF livingobject.js:319-340
            /// </summary>
        public virtual void Transit()
        {
            ComboUpdate();

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                // 被卡住，不执行帧转换
            }
            else
            {
                Trans.Trans();
            }

            Effect.TimeIn--;

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                // 被卡住，不执行状态更新
            }
            else
            {
                StateUpdate("transit");
            }
        }

        /// <summary>
        /// TU 更新（对应 FLF livingobject.prototype.TU_update）
        /// 参考：FLF livingobject.js:199-284
        /// </summary>
        public virtual void TUUpdate()
        {
            if (!StateUpdate("TU_force"))
            {
                FrameForce();
            }

            ProcessEffects();

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                // 卡住状态
            }
            else
            {
                StateUpdate("TU");
            }

            if (Health.HP <= 0 && !Dead)
            {
                StateUpdate("die");
                Dead = true;
            }

            // 检查是否离开场景
            //一般是用于飞行道具，离开场景后销毁


            ItrRest?.Tick();
        }

        /// <summary>
        /// 连招更新（子类可重写，对应 FLF $.combo_update）
        /// </summary>
        protected virtual void ComboUpdate()
        {
        }

        #endregion

        #region 功能逻辑 - 状态系统

        /// <summary>
        /// 状态更新分发（对应 FLF livingobject.prototype.state_update）
        /// 顺序：先执行 generic，再执行当前状态的 specific 处理器
        /// 参考：FLF livingobject.js:286-301
        /// </summary>
        public virtual bool StateUpdate(string eventType, object eventData = null)
        {
            bool res1 = OnGenericStateEvent(eventType, eventData);

            bool res2 = false;
            int currentState = Frame.D?.state ?? -1;
            if (currentState >= 0 && _states.TryGetValue(currentState, out var handler))
            {
                res2 = handler(eventType, eventData);
            }

            return res1 || res2;
        }

        public virtual bool StateUpdate(string eventType, out int frameId, object eventData = null) 
        {
            frameId = 0;
            // 调用 handler，handler 通过写 _stateReturnFrame 传帧号
            bool handled = StateUpdate(eventType, eventData);
            if (StateReturnFrame > 0)
            {
                frameId = StateReturnFrame;
                StateReturnFrame = 0;
            }
            return handled;
        }

        /// <summary>
        /// 通用状态事件处理 - 所有状态共享的逻辑（子类重写）
        /// </summary>
        protected virtual bool OnGenericStateEvent(string eventType, object eventData) => false;

        /// <summary>获取当前状态（对应 FLF livingobject.prototype.state）</summary>
        public int GetState()
        {
            return Frame.D?.state ?? 0;
        }

        #endregion

        #region 功能逻辑 - 效果系统

        /// <summary>
        /// 获取效果ID（对应 FLF livingobject.prototype.effect_id）
        /// 参考：FLF livingobject.js effect_id
        /// </summary>
        public int EffectId(int num)
        {
            return num + NTSDGlobal.Gameplay.EffectNumToId;
        }

        /// <summary>
        /// 创建效果（对应 FLF livingobject.prototype.effect_create）
        /// 参考：FLF livingobject.js:371-398
        /// </summary>
        public virtual void EffectCreate(int num, int duration, float dvx = 0, float dvy = 0)
        {
            if (num < Effect.Num) return;

            int efid = num + NTSDGlobal.Gameplay.EffectNumToId;
            int? oscillate = NTSDSpec.Proper<int?>(efid, "oscillate");
            if (oscillate.HasValue)
            {
                Effect.Oscillate = oscillate.Value;
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
        /// 创建视觉效果（对应 FLF livingobject.prototype.visualeffect_create）
        /// 参考：FLF livingobject.js visualeffect_create
        /// </summary>
        public virtual void VisualEffectCreate(int num, PhysicsState.FlfVolume rect, bool righttip = false, int variant = 0, bool withSound = false)
        {
            int efid = num + NTSDGlobal.Gameplay.EffectNumToId;
            float posX = rect.x + rect.vx + (righttip ? rect.w : 0);
            float posY = rect.y + rect.vy + rect.h / 2f;
            float posZ = rect.z > PS.z ? rect.z : PS.z;

            // TODO: Match.VisualEffect.Create(efid, pos, variant, withSound)
        }

        /// <summary>
        /// 创建破碎效果（对应 FLF livingobject.prototype.brokeneffect_create）
        /// 参考：FLF livingobject.js brokeneffect_create
        /// </summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            var bodies = VolBody();
            if (bodies == null || bodies.Count == 0) return;

            // TODO: Match.BrokenEffect.Create(320, pos, id, i, staticBody)
        }

        /// <summary>
        /// 卡住效果（对应 FLF livingobject.prototype.effect_stuck）
        /// 参考：FLF livingobject.js:401-410
        /// </summary>
        public virtual void EffectStuck(int timeIn, int timeOut)
        {
            if (Effect.Stuck && Effect.Num > -1) return;

            Effect.Num = -1;
            Effect.Stuck = true;
            Effect.TimeIn = timeIn;
            Effect.TimeOut = timeOut;
        }

        #endregion

        #region 功能逻辑 - 方向系统

        /// <summary>
        /// 切换方向（对应 FLF livingobject.prototype.switch_dir）
        /// 参考：FLF livingobject.js:510-520
        /// </summary>
        public virtual void SwitchDir(string dir)
        {
            if (PS == null) return;

            if (PS.dir == "left" && dir == "right")
            {
                PS.dir = "right";
                Sprite?.SwitchLR("right");
            }
            else if (PS.dir == "right" && dir == "left")
            {
                PS.dir = "left";
                Sprite?.SwitchLR("left");
            }
        }

        public virtual void SwitchDir(DIRECTION rection) 
        {
            SwitchDir(rection == DIRECTION.LEFT ? "left" : "right");
        }

        /// <summary>
        /// 获取水平方向（对应 FLF livingobject.prototype.dirh）
        /// </summary>
        public int Dirh()
        {
            if (PS == null) return 1;
            return PS.dir == "left" ? -1 : 1;
        }

        /// <summary>
        /// 获取垂直方向（对应 FLF livingobject.prototype.dirv）
        /// </summary>
        public virtual int Dirv()
        {
            return Controller.Dirv();
        }
        
        #endregion

        #region 功能逻辑 - 交互冷却系统

        /// <summary>
        /// 测试攻击休息（对应 FLF livingobject.prototype.itr_arest_test）
        /// </summary>
        public bool ItrArestTest()
        {
            return ItrRest == null || ItrRest.Arest <= 0;
        }

        /// <summary>
        /// 更新攻击休息（对应 FLF livingobject.prototype.itr_arest_update）
        /// </summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;

            if (itr != null && itr.arest > 0)
            {
                ItrRest.Arest = itr.arest;
            }
            else if (itr == null || itr.vrest <= 0)
            {
                ItrRest.Arest = NTSDGlobal.Default.Character.ARest;
            }
        }

        /// <summary>
        /// 测试受击休息（对应 FLF livingobject.prototype.itr_vrest_test）
        /// </summary>
        public bool ItrVrestTest(int uid)
        {
            return ItrRest == null || !ItrRest.HasVrest(uid);
        }

        /// <summary>
        /// 更新受击休息（对应 FLF livingobject.prototype.itr_vrest_update）
        /// </summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;

            if (itr.vrest > 0)
            {
                ItrRest.SetVrest(attackerUid, itr.vrest);
            }
        }

        #endregion

        #region 功能逻辑 - 位置与数据查询

        /// <summary>
        /// 设置位置（对应 FLF livingobject.prototype.set_pos）
        /// </summary>
        public void SetPos(float x, float y, float z)
        {
            if (PS == null) return;
            PS.x = x;
            PS.y = y;
            PS.z = z;
        }

        /// <summary>根据帧ID获取帧数据</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
        {
            return FrameCache?.GetFrameDataById(frameId);
        }

        /// <summary>根据状态获取第一个帧ID</summary>
        public virtual int GetFirstFrameByState(int state)
        {
            return FrameCache?.GetFirstFrameByState(state) ?? -1;
        }

        /// <summary>获取精灵宽度用于碰撞检测（子类可重写）</summary>
        public virtual float GetSpriteWidthPxForCollision()
        {
            return _FrameDataWrapper.characterData.files[0].width + 1;
        }

        #endregion

        #region 功能逻辑 - 状态内存

        /// <summary>获取状态内存</summary>
        public bool GetStateMemory<T>(string key, out T value)
        {
            value = default;
            if (StateMem == null || !StateMem.TryGetValue(key, out var obj))
                return false;
            if (obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            return false;
        }

        /// <summary>设置状态内存</summary>
        public void SetStateMemory<T>(string key, T value)
        {
            StateMem ??= new Dictionary<string, object>();
            StateMem[key] = value;
        }

        /// <summary>清空状态内存（状态切换时调用）</summary>
        public void ClearStateMem()
        {
            StateMem.Clear();
        }

        #endregion

        #region 功能逻辑 - 帧转换便捷方法

        /// <summary>帧转换</summary>
        public virtual void TransitionToFrame(int frameId, int wait = 0)
        {
            Trans?.Frame(frameId, wait);
        }

        /// <summary>播放指定帧</summary>
        public virtual void PlayFrameByID(int frameId)
        {
            Trans?.Frame(frameId, 0);
        }

        #endregion

        #region 接口实现 - ILF2Poolable

        public abstract LF2ObjectType ObjectTypeEnum { get; }
        public int ObjectType => (int)ObjectTypeEnum;
        public abstract void Reset();

        #endregion

        #region 接口实现 - ILF2Object

        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>
        /// 销毁对象（对应 FLF livingobject.prototype.destroy）
        /// 参考：FLF livingobject.js:89-94
        /// </summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        #endregion

        #region 接口实现 - ISimObject

        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx) { }
        public virtual void OnRemoved(SimContext ctx) { }

        public virtual void SimTransit(int tickIndex)
        {
            Transit();
        }

        public virtual void SimTU(int tickIndex)
        {
            TUUpdate();
        }

        public virtual void SimLateTick(int tickIndex) { }

        #endregion

        #region 派生函数（子类可重写的虚属性）

        /// <summary>角色统计数据（子类可重写）</summary>
        public virtual NTSDCharacterStats CharacterStats => null;

        /// <summary>击中计数器（子类可重写）</summary>
        public virtual LF2HitCountersModule HitCounters => null;

        #endregion

        #region 私有/保护函数

        /// <summary>
        /// 处理效果（对应 FLF livingobject.prototype.TU_update 中的效果处理部分）
        /// 参考：FLF livingobject.js:207-255
        /// </summary>
        protected virtual void ProcessEffects()
        {
            if (Effect.TimeIn >= 0) return;

            if (Effect.Oscillate != 0)
            {
                Effect.OscillateDirection = Effect.OscillateDirection == 1 ? -1 : 1;
                Sprite?.SetXY(PS.sx + Effect.Oscillate * Effect.OscillateDirection, PS.sy + PS.sz);
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
                    Sprite?.SetXY(PS.sx, PS.sy + PS.sz);
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

        /// <summary>分配 StableId</summary>
        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
        }

        /// <summary>重置 StableId</summary>
        protected void ResetStableId()
        {
            StableId = 0;
        }

        /// <summary>
        /// 计算方向（对应 FLF facing 逻辑）
        /// </summary>
        protected string CalculateDirection(int facing, string parentDir)
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
