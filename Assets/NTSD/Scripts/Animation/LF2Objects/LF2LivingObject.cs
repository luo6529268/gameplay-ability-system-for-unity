using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Simulation;
using System.Collections.Generic;
using UnityEngine;
//using static NTSD.Animation.CharacterStates;

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

        // 震荡方向（内部使用）
        public int OscillateDirection { get; set; } = 1;
        
        // 闪烁计数（内部使用）
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
    /// LF2 所有活动对象的基类（纯 C# 类，不继承 MonoBehaviour）
    /// 实现 ILF2Object（包含 ISimObject）和 ILF2LivingObject
    /// 
    /// 完全对齐 FLF livingobject.js
    /// 所有活动对象（角色、武器、特效）都继承此类
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\livingobject.js
    /// </summary>
    public abstract class LF2LivingObject : ILF2Object, ILF2LivingObject
    {
        private delegate bool StateHandler(string eventType, object eventData);

        /// <summary>对象名称（对应 FLF $.name = data.bmp.name）</summary>
        public string Name { get; set; }

        /// <summary>唯一ID，由场景分配（对应 FLF $.uid）</summary>
        public int StableId { get; protected set; }

        /// <summary>对象ID（对应 FLF $.id = thisID）</summary>
        public int ObjectId { get; set; }

        /// <summary>对象数据引用（对应 FLF $.data）</summary>
        public LF2CharacterDataWrapper Data { get; protected set; }

        /// <summary>队伍 ID（对应 FLF $.team）</summary>
        public int Team { get; set; }

        /// <summary>所有者 ID（对应 FLF $.owner，用于飞行道具等）</summary>
        public int OwnerId { get; set; } = -1;

        /// <summary>状态内存，状态切换时自动清空（对应 FLF $.statemem）</summary>
        public Dictionary<string, object> StateMem { get; protected set; } = new Dictionary<string, object>();

        // ========== FLF livingobject 世界句柄 ==========

        /// <summary>匹配/世界句柄（对应 FLF $.match）</summary>
        public SimulationWorld Match => SimulationTickDriver.Instance?.World;

        // ========== FLF livingobject 核心模块 ==========

        /// <summary>精灵模块（对应 FLF $.sp）</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>生命值（对应 FLF $.health）</summary>
        public LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>帧信息（对应 FLF $.frame）</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>物理系统（对应 FLF $.mech）</summary>
        public CharacterMechanics Mech { get; protected set; }

        /// <summary>物理状态（对应 FLF $.PS = $.mech.create_metric()）</summary>
        public PhysicsState PS { get; protected set; }

        /// <summary>帧转换器（对应 FLF $.Trans）</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>交互冷却（对应 FLF $.itr）</summary>
        public LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>效果状态（对应 FLF $.effect）</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>帧数据缓存</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>角色统计数据（子类可重写）</summary>
        public virtual NTSDCharacterStats CharacterStats => null;

        /// <summary>击中计数器（子类可重写）</summary>
        public virtual LF2HitCountersModule HitCounters => null;

        /// <summary>Character Hub 引用（子类可重写）</summary>
        public Character _CharacterHub { get; protected set; }

        /// <summary>帧数据包装器（兼容 LF2CharacterAnimator._FrameDataWrapper）</summary>
        public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache?.Wrapper;

        // 状态处理器字典，Key 为状态 ID (State ID)，Value 为对应的处理函数委托
        private Dictionary<int, StateHandler> _StateHandlers = new Dictionary<int, StateHandler>(20);

        /// <summary>
        /// 通用状态处理（子类重写）
        /// </summary>
        protected virtual bool OnGenericStateEvent(string eventType, object eventData) => false;

        // ========== 兼容性方法（对齐 LF2CharacterAnimator）==========

        /// <summary>设置朝向方向</summary>
        public virtual void SetDirection(DIRECTION direction)
        {
            if (PS != null)
                PS.dir = direction == DIRECTION.RIGHT ? "right" : "left";
        }

        /// <summary>通过字符串设置朝向方向</summary>
        public virtual void SetDirectionByString(string dir)
        {
            if (PS != null)
                PS.dir = dir;
        }

        /// <summary>帧动画振荡（在指定帧范围内循环）</summary>
        public virtual void FrameAniOscillate(int from, int to)
        {
            int current = Frame?.N ?? 0;
            if (current < from || current > to)
            {
                Trans?.Frame(from, 0);
            }
        }

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

        /// <summary>根据状态获取第一个帧ID</summary>
        public virtual int GetFirstFrameByState(int state)
        {
            return FrameCache?.GetFirstFrameByState(state) ?? -1;
        }

        /// <summary>连招缓冲模块（子类可重写）</summary>
        public virtual LF2ComboBufferModule ComboBuffer => null;

        /// <summary>帧转换（兼容 LF2CharacterAnimator.TransitionToFrame）</summary>
        public virtual void TransitionToFrame(int frameId, int wait = 0)
        {
            Trans?.Frame(frameId, wait);
        }

        /// <summary>播放指定帧（兼容 LF2CharacterAnimator.PlayFrameByID）</summary>
        public virtual void PlayFrameByID(int frameId)
        {
            Trans?.Frame(frameId, 0);
        }

        /// <summary>Transit 阶段的物理和武器点处理（子类可重写）</summary>
        public virtual void Transit_DynamicsAndWPoint()
        {
            // 默认空实现，由子类（如 LF2Character）重写
        }

        /// <summary>根据帧ID获取帧数据（兼容 LF2CharacterAnimator.GetFrameDataById）</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
        {
            return FrameCache?.GetFrameDataById(frameId);
        }

        /// <summary>获取精灵宽度用于碰撞检测（子类可重写）</summary>
        public virtual float GetSpriteWidthPxForCollision()
        {
            return Sprite?.GetCurrentSpriteWidthPx() ?? 0f;
        }

        /// <summary>
        /// 状态更新 - 基类统一调度
        /// </summary>
        public virtual bool StateUpdate(string eventType, object eventData = null)
        {
            // 1. 先执行 generic
            bool res1 = OnGenericStateEvent(eventType, eventData);

            // 2. 再执行当前状态处理器
            bool res2 = false;
            if (_StateHandlers.TryGetValue(Frame.D.state, out var handler))
            {
                res2 = handler(eventType, eventData);
            }

            return res1 || res2;
        }

        // ========== FLF livingobject 状态字段 ==========

        /// <summary>抓取状态（对应 FLF $.catching）</summary>
        public int Catching { get; set; } = 0;

        /// <summary>允许切换方向（对应 FLF $.allow_switch_dir）</summary>
        public bool AllowSwitchDir { get; set; } = true;

        /// <summary>控制器（对应 FLF $.con）</summary>
        public ILF2Controller Controller { get; set; }

        /// <summary>是否死亡（对应 FLF $.dead）</summary>
        public bool Dead { get; set; } = false;

        // ========== ILF2Object 接口实现 ==========

        public abstract LF2ObjectType ObjectTypeEnum { get; }
        public int ObjectType => (int)ObjectTypeEnum;

        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);
        public abstract void Reset();

        /// <summary>
        /// 销毁对象（对应 FLF livingobject.prototype.destroy）
        /// 参考：FLF livingobject.js:89-94
        /// </summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        // ========== ISimObject 接口实现 ==========

        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx) { }
        public virtual void OnRemoved(SimContext ctx) { }

        /// <summary>
        /// Transit 阶段（对应 FLF livingobject.prototype.transit）
        /// 参考：FLF livingobject.js:319-340
        /// </summary>
        public virtual void SimTransit(int tickIndex)
        {
            // 如果被卡住，不执行转换
            if (Effect.TimeIn < 0 && Effect.Stuck)
                return;

            // 帧转换
            Trans?.Trans();

            // 效果时间递减
            Effect.TimeIn--;

            // 状态更新
            if (!(Effect.TimeIn < 0 && Effect.Stuck))
            {
                StateUpdate("transit");
            }
        }

        /// <summary>
        /// TU 阶段（对应 FLF livingobject.prototype.TU）
        /// 参考：FLF livingobject.js:310-317
        /// </summary>
        public virtual void SimTU(int tickIndex)
        {
            TUUpdate();
        }

        public virtual void SimLateTick(int tickIndex) { }

        // ========== 渲染器引用（临时，迁移完成后移除）==========

        protected LF2ObjectRenderer _renderer;
        // ========== FLF livingobject 核心方法 ==========

        /// <summary>
        /// 初始化设置（对应 FLF livingobject.prototype.setup）
        /// 参考：FLF livingobject.js:101-103
        /// </summary>
        public virtual void Setup()
        {
            StateUpdate("setup");
        }

        public virtual void Transit()
        {
            // 对齐 FLF: 每个 TU 递减 arest/vrest
            //ItrRest?.Tick();

            // 1. 处理输入和连招识别
            ComboUpdate();

            // 1.5 Phase 1: pre_interaction
            //LF2CollisionSystem.ProcessPreInteractionTick();

            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                //被卡住
            }
            else
                Trans.Trans(); // 2. 帧转换

            Effect.TimeIn--;
            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                //被卡住
            }
            else
                CharacterStates.Instance.HandleStateEvent(this, "transit"); // 3. 触发 transit 事件

            // 5. Phase 0: 碰撞检测
            //LF2CollisionSystem.ProcessPostInteractionTick();
        }

        protected virtual void ComboUpdate() 
        {
            
        }

        /// <summary>
        /// 帧更新（对应 FLF livingobject.prototype.frame_update）
        /// 参考：FLF livingobject.js:106-130
        /// </summary>
        public virtual void FrameUpdate()
        {
            if (Frame.D == null) return;

            // 显示帧图片
            Sprite?.ShowPic(Frame.D.pic);

            // 重置摩擦力
            if (PS != null) PS.fric = 1;

            // 应用帧力（如果状态没有处理）
            if (!StateUpdate("frame_force"))
            {
                FrameForce();
            }

            // 设置等待时间和下一帧
            Trans?.SetWait(Frame.D.wait, 99);
            Trans?.SetNext(Frame.D.next, 99);

            // 状态帧更新
            StateUpdate("frame");

            // 播放声音
            if (!string.IsNullOrEmpty(Frame.D.sound))
            {
                // TODO: 播放声音
            }
        }

        /// <summary>
        /// 应用帧力（对应 FLF livingobject.prototype.frame_force）
        /// 参考：FLF livingobject.js:133-148
        /// </summary>
        public virtual void FrameForce()
        {
            if (Frame.D == null || PS == null) return;

            // dvx 处理
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

            // dvz 处理
            if (Frame.D.dvz != 0)
            {
                PS.vz = Dirv() * Frame.D.dvz;
            }

            // dvy 处理
            if (Frame.D.dvy != 0)
            {
                PS.vy += Frame.D.dvy;
            }

            // 550 特殊值：停止移动
            if (Frame.D.dvx == 550) PS.vx = 0;
            if (Frame.D.dvy == 550) PS.vy = 0;
            if (Frame.D.dvz == 550) PS.vz = 0;
        }

        /// <summary>
        /// TU 更新（对应 FLF livingobject.prototype.TU_update）
        /// 参考：FLF livingobject.js:199-284
        /// </summary>
        public virtual void TUUpdate()
        {
            // 应用帧力（如果状态没有处理）
            if (!StateUpdate("TU_force"))
            {
                FrameForce();
            }

            // 处理效果
            ProcessEffects();

            // 如果被卡住，不执行状态更新
            if (Effect.TimeIn < 0 && Effect.Stuck)
            {
                // 卡住状态
            }
            else
            {
                StateUpdate("TU");
            }

            // 检查生命值
            if (Health.HP <= 0 && !Dead)
            {
                StateUpdate("die");
                Dead = true;
            }

            // 更新交互冷却
            ItrRest?.Tick();
        }

        /// <summary>
        /// 处理效果（对应 FLF livingobject.prototype.TU_update 中的效果处理部分）
        /// 参考：FLF livingobject.js:207-255
        /// </summary>
        protected virtual void ProcessEffects()
        {
            if (Effect.TimeIn >= 0) return;

            // 震荡效果
            if (Effect.Oscillate != 0)
            {
                Effect.OscillateDirection = Effect.OscillateDirection == 1 ? -1 : 1;
                Sprite?.SetXY(PS.sx + Effect.Oscillate * Effect.OscillateDirection, PS.sy + PS.sz);
            }
            // 闪烁效果
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

            // 效果结束
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
                // 持续效果
                if (Effect.Dvx != 0) PS.vx = Effect.Dvx;
                if (Effect.Dvy != 0) PS.vy = Effect.Dvy;
                Effect.Dvx = 0;
                Effect.Dvy = 0;
            }

            Effect.TimeOut--;
        }

        /// <summary>
        /// 状态更新分发（对应 FLF livingobject.prototype.state_update）
        /// 参考：FLF livingobject.js:286-301
        /// </summary>
        public virtual bool StateUpdate(string eventName)
        {
            // 子类重写以实现具体状态逻辑
            return false;
        }

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

        /// <summary>
        /// 获取水平方向（对应 FLF livingobject.prototype.dirh）
        /// 参考：FLF livingobject.js:523-526
        /// </summary>
        public int Dirh()
        {
            if (PS == null) return 1;
            return PS.dir == "left" ? -1 : 1;
        }

        /// <summary>
        /// 获取垂直方向（对应 FLF livingobject.prototype.dirv）
        /// 参考：FLF livingobject.js:529-537
        /// </summary>
        public virtual int Dirv()
        {
            int d = 0;
            if (Controller != null)
            {
                if (Controller.IsUp) d -= 1;
                if (Controller.IsDown) d += 1;
            }
            return d;
        }

        /// <summary>
        /// 创建效果（对应 FLF livingobject.prototype.effect_create）
        /// 参考：FLF livingobject.js:371-398
        /// </summary>
        public virtual void EffectCreate(int num, int duration, float dvx = 0, float dvy = 0)
        {
            if (num < Effect.Num) return;

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

        /// <summary>
        /// 测试攻击休息（对应 FLF livingobject.prototype.itr_arest_test）
        /// 参考：FLF livingobject.js:476-479
        /// </summary>
        public bool ItrArestTest()
        {
            return ItrRest == null || ItrRest.Arest <= 0;
        }

        /// <summary>
        /// 更新攻击休息（对应 FLF livingobject.prototype.itr_arest_update）
        /// 参考：FLF livingobject.js:482-489
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
        /// 参考：FLF livingobject.js:492-495
        /// </summary>
        public bool ItrVrestTest(int uid)
        {
            return ItrRest == null || !ItrRest.HasVrest(uid);
        }

        /// <summary>
        /// 更新受击休息（对应 FLF livingobject.prototype.itr_vrest_update）
        /// 参考：FLF livingobject.js:498-504
        /// </summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;

            if (itr.vrest > 0)
            {
                ItrRest.SetVrest(attackerUid, itr.vrest);
            }
        }

        /// <summary>
        /// 设置位置（对应 FLF livingobject.prototype.set_pos）
        /// 参考：FLF livingobject.js:343-345
        /// </summary>
        public void SetPos(float x, float y, float z)
        {
            if (PS == null) return;
            PS.x = x;
            PS.y = y;
            PS.z = z;
        }

        /// <summary>
        /// 获取当前状态（对应 FLF livingobject.prototype.state）
        /// 参考：FLF livingobject.js:362-364
        /// </summary>
        public int GetState()
        {
            return Frame.D?.state ?? 0;
        }

        // ========== StableId 分配 ==========

        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
        }

        protected void ResetStableId()
        {
            StableId = 0;
        }

        // ========== 辅助方法 ==========

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

        /// <summary>
        /// 清空状态内存（状态切换时调用）
        /// </summary>
        public void ClearStateMem()
        {
            StateMem.Clear();
        }
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
    }
}
