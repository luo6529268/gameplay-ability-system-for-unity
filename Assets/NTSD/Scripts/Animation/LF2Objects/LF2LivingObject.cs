using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.App;
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

        /// <summary>PP（能量/体力），对应反汇编 entity+308h。饮料饮用时补充。</summary>
        public int PP { get; set; } = 0;

        /// <summary>PP 上限，对应反汇编 entity+300h。</summary>
        public int MaxPP { get; set; } = 500;

        /// <summary>PP 当前结算上限（可被消耗缩小），对应反汇编 entity+304h。</summary>
        public int PPBound { get; set; } = 500;

        /// <summary>累计受到的总伤害（对应 FLF $.health.hp_lost）</summary>
        public int HPLost { get; set; } = 0;

        /// <summary>HP 结算上限（对应 FLF $.health.hp_bound，随受伤递减）</summary>
        public int HPBound { get; set; } = 100;
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

    public enum DIRECTION 
    {
        RIGHT = 1,
        LEFT = -1,
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
    public abstract class LF2LivingObject : LF2Entity
    {
        #region 声明字段 - 身份标识（角色专属）

        /// <summary>
        /// 当前正在处理的 itr 在帧 itr 列表中的 index（0-based）。
        /// 对应反汇编 var_5C = [edx+esi+2D0h]（itr array index）。
        /// 由 Generic_PostInteraction 在调用 Hit() 前写入，SpawnSpark 读取用于 timer 计算。
        /// </summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>对象类型（对应 FLF livingobject.prototype.type）</summary>
        public virtual LF2ObjectType Type => ObjectTypeEnum;

        /// <summary>
        /// 每个状态是否允许切换方向（对应 FLF livingobject.prototype.states_switch_dir）
        /// 子类通过 InitializeStatesSwitchDir() 填充
        /// </summary>

        #endregion

        #region 声明字段 - 核心模块（角色专属）

        /// <summary>生命值（对应 FLF $.health）</summary>
        public LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>物理系统（对应 FLF $.mech）</summary>
        public CharacterMechanics Mech { get; protected set; }

        /// <summary>交互冷却（对应 FLF $.itr）</summary>
        public LF2ItrRestTracker ItrRest { get; protected set; }

        #endregion

        #region 声明字段 - 状态字段（角色专属）

        /// <summary>状态内存，状态切换时自动清空（对应 FLF $.statemem）</summary>
        public Dictionary<string, object> StateMem { get; protected set; } = new Dictionary<string, object>();

        /// <summary>抓取状态（对应 FLF $.catching）</summary>
        public LF2LivingObject Catching { get; set; } = null;

        /// <summary>允许切换方向（对应 FLF $.allow_switch_dir）</summary>
        public bool AllowSwitchDir { get; set; } = true;

        /// <summary>控制器（对应 FLF $.con）</summary>
        public ILF2Controller Controller { get; set; }

        /// <summary>是否死亡（对应 FLF $.dead）</summary>
        public bool Dead { get; set; } = false;

        /// <summary>最近一次造成受击的攻击者（对应 FLF $.itr.attacker）</summary>
        public LF2LivingObject Attacker { get; set; } = null;

        /// <summary>是否为NPC（对应 FLF $.is_npc）</summary>
        public bool IsNpc { get; set; } = false;

        /// <summary>
        /// 连续击飞命中计数（对应反汇编 entity+20h hit_count）。
        /// 每次击飞命中时递增，落地时重置为 1。
        /// </summary>
        public int HitCount { get; set; } = 1;

        /// <summary>
        /// 命中确认窗口计数（对应反汇编 entity+0EAh hit_confirm_ea）。
        /// kind=6 命中时设为 3，每帧 -1；Standing 状态按攻击键且 > 0 时跳转 frame 70。
        /// </summary>
        public int HitConfirmEa { get; set; } = 0;

        /// <summary>
        /// HP 恢复计时器（对应反汇编 entity+0E4h）。
        /// </summary>
        public int HealTimer { get; set; } = 0;

        /// <summary>NPC的控制者/父对象（对应 FLF $.parent，NPC stat 归属到 parent）</summary>
        public LF2LivingObject Parent { get; set; } = null;

        /// <summary>战斗统计（对应 FLF $.stat）</summary>
        public LF2BattleStat Stat { get; private set; } = new LF2BattleStat();

        #endregion

        #region 声明字段 - Unity 架构适配

        /// <summary>Character Hub 引用（子类可重写）</summary>
        public Character _CharacterHub { get; protected set; }

        /// <summary>帧数据包装器</summary>
        public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache?.Wrapper;

        #endregion

        #region 初始化函数

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

            // 反汇编：entity+0x28h (vx/knockback_vx) 只在 hit 处理里写入，dvx 不写该字段。
            // Falling 状态下 PS.vx 由击飞速度驱动，dvx 不得覆盖。
            bool isFalling = GetState() == LF2States.Falling;
            if (Frame.D.dvx != 0 && !isFalling)
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
            if (!isFalling && Frame.D.dvx == 550) PS.vx = 0;
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

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            int prevFn = Frame.N;
            Log.LogState(Name, "Frame", $"{Frame.N} → {targetFrameId}");
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

                // 对应反汇编 0x00415882/0x004158F5/0x00415B8E：状态转换时重置 HitStun
                HitStun = 0;

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
            Log.Info("下一帧：{0}", Frame.D.next);
            // 状态 frame 事件
            StateUpdate("frame");

            // 播放音效
            if (Frame.D != null && !string.IsNullOrEmpty(Frame.D.sound))
            {
                AppManager.Instance?.SoundPlayer?.PlaySfx(Frame.D.sound);
            }
        }

        public override bool GetStatesSwitchDir(int stateId) 
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

            // 对应反汇编 Entity_FrameAdvance sub_416240：FrameDelay 向 0 靠拢
            // 原版逻辑：dec/inc 后无论结果是否为 0，只要 prevDelay != 0 就 return（跳过本帧推进）
            // jge loc_416D9E（>0dec后→return）；inc后直接 retn（<0inc后→return）
            int prevDelay = FrameDelay;
            if (FrameDelay > 0) FrameDelay--;
            else if (FrameDelay < 0) FrameDelay++;

            if (prevDelay != 0)
                return;

            // 原版：prevDelay != 0 时一律 return（包括 -1→0 和 1→0 的那帧）
            if (prevDelay != 0) return;

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
            // 对应反汇编 Entity_Collision (0x4138F0) 帧推进开头的计数器递减序列：
            //   0x0041391A: [esi+0ECh] AttackExempt（最先）
            //   0x004139C7: [esi+0B8h] HitStateCount
            //   0x004139D8: [esi+0EAh] HitConfirmEa
            // 注：[esi+0B0h] 是 fall 累加器（HitCounters.Fall），由 RecoverFall() 处理
            if (HitCounters != null)
            {
                if (HitCounters.AttackExempt > 0) HitCounters.SetAttackExempt(HitCounters.AttackExempt - 1);
                if (HitCounters.HitStateCount > 0) HitCounters.SetHitStateCount(HitCounters.HitStateCount - 1);
            }
            if (HitConfirmEa > 0) HitConfirmEa--;

            // 反汇编 0x00423B0C-0x00423B71：HealTimer [+0E4h] 每帧递减，counter%8==0 时 HP+8（上限 HPBound）
            if (HealTimer > 0 && Health?.HP > 0)
            {
                HealTimer--;
                if ((HealTimer & 7) == 0)
                {
                    int newHp = Health.HP + 8;
                    if (newHp >= Health.HPBound)
                    {
                        Health.HP = Health.HPBound;
                        HealTimer = 0;
                    }
                    else
                    {
                        Health.HP = newHp;
                    }
                }
            }

            // 反汇编 Entity_Collision 0x413909-0x413918：this+8 向 0 收敛
            if (ShakeTimer > 0) ShakeTimer--;
            else if (ShakeTimer < 0) ShakeTimer++;

            // 反汇编 0x416254-0x41627C：FrameDelay 非零时跳过状态机 TU（hit_stop 冻结）
            if (FrameDelay != 0) return;

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
        public override bool StateUpdate(string eventType, object eventData = null)
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

        public override bool StateUpdate(string eventType, out int frameId, object eventData = null) 
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
        protected override bool OnGenericStateEvent(string eventType, object eventData) => false;

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
        ///
        /// NTSD 实现：命中时追加 spark slot，由 SparkRenderer.RenderAll() 在 LateUpdate 渲染。
        /// 对应反汇编 PostRender（0x41D830）spark blit 路径。
        /// 子类（如 LF2Character）通过 override 调用 AddSparkSlot()。
        /// </summary>
        public virtual void VisualEffectCreate(int num, PhysicsState.FlfVolume rect, bool righttip = false, int variant = 0, bool withSound = false)
        {
        }

        /// <summary>
        /// 创建破碎效果（对应 FLF livingobject.prototype.brokeneffect_create）
        /// NTSD 中不存在破碎特效（游戏目录无任何 broken dat，反汇编无对应路径）。
        /// 保留方法签名以对齐 FLF 接口，方法体为空。
        /// </summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
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
        public override void SwitchDir(string dir)
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

        public override void SwitchDir(DIRECTION rection) 
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
        /// 更新受击休息（对应反汇编 0x42DA00/0x42DA1F/0x42DA77：
        ///   基于 itr.injury 决定 vrest 持续 tick 数，与 itr.vrest 字段无关：
        ///     itr.injury >  40  → vrest = 19 tick (0x13)
        ///     itr.injury <= 40  → vrest = 3 tick  (0x03)
        /// 写入位置：[victim + attackerIdx*4 + 0xF0] = vrest byte，
        /// 每 tick 在 0x41BE6B 处 dec，归零后可再次命中。
        /// </summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;

            int vrest = (itr.injury > 40) ? 19 : 3;
            ItrRest.SetVrest(attackerUid, vrest);
        }

        /// <summary>
        /// 受击处理（对应 FLF character.prototype.hit）
        /// 基类仅执行 vrest 前置冷却检查（对应 FLF hit() 第一行 itr_vrest_test）。
        /// 完整的受击逻辑（fall 累加、防御判定、状态切换、vrest 写回）全部在子类 override 中实现。
        /// 注意：FLF 的 hit() 是独立的 prototype 方法，不经过 state_update 分发。
        /// </summary>
        public virtual bool Hit(InteractionArea itr, LF2LivingObject attacker, UnityEngine.Vector3 attackerPos, PhysicsState.FlfVolume vol)
        {
            // 前置 vrest 冷却检查：对应 FLF character.js:1896 if (!$.itr_vrest_test(att.uid)) return false
            // vrest 的实际写回（itr_vrest_update）由子类在确认 accepthit=true 后执行
            if (!ItrVrestTest(attacker.StableId)) return false;
            return true;
        }

        /// <summary>
        /// 攻击统计记录（对应 FLF character.prototype.attacked）
        /// FLF 中 attacked() 仅做 stat.attack 统计，不含任何伤害/速度/帧切换逻辑。
        /// 所有实际受击逻辑（伤害、击退、帧切换）均在 Hit() 内部完成。
        /// 调用方式：target.Attacked(itr, attacker) 在 Hit() 返回 true 后调用。
        /// </summary>
        public virtual bool Attacked(InteractionArea itr, LF2LivingObject attacker)
        {
            // FLF character.js:2161-2167
            // if (this.is_npc && this.parent) parent.stat.attack += inj
            // else this.stat.attack += inj
            return true;
        }

        /// <summary>
        /// 击杀统计（对应 FLF character.prototype.killed）
        /// </summary>
        public void Killed()
        {
            // FLF character.js:2182-2188
            if (IsNpc && Parent != null)
                Parent.Stat.Kill++;
            else
                Stat.Kill++;
        }

        /// <summary>
        /// 调整攻击统计（对应 FLF character.prototype.offset_attack）
        /// 当NPC被击杀时，从attacker的attack统计中减去injury
        /// </summary>
        public void OffsetAttack(int inj)
        {
            // FLF character.js:2175-2177
            Stat.Attack -= inj;
        }

        /// <summary>
        /// 伤害结算（对应 FLF character.prototype.injury）
        /// 子类可 override（如 NPC 需要回调攻击者的 offset_attack）
        /// </summary>
        protected virtual void Injury(int inj)
        {
            if (inj <= 0 || Health == null) return;
            Health.HP -= inj;
            Health.HPLost += inj;
            Health.HPBound -= Mathf.CeilToInt(inj / 3f);
        }

        /// <summary>
        /// ITR kind 类型匹配（对应 FLF global.js GC.match_itr_kind）
        /// 与 BruteForceSceneQuery.MatchItrKind 共享相同逻辑和查找表。
        /// </summary>
        public bool MatchItrKind(int itrKind, int targetKind)
        {
            if (s_itrTypeMap.TryGetValue(targetKind, out int[] types))
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == itrKind) return true;
                }
                return false;
            }
            return itrKind == targetKind;
        }

        // 对应 FLF global.js GC.match_itr_kind 查找表（与 BruteForceSceneQuery 保持同步）
        private static readonly Dictionary<int, int[]> s_itrTypeMap = new Dictionary<int, int[]>
        {
            { 2,  new[] { 2, 1, 4, 21, 5 } },
            { 1,  new[] { 1, 21, 17 } },
            { 4,  new[] { 4, 10, 19 } },
            { 5,  new[] { 5, 19 } },
            { 6,  new[] { 6, 18 } },
            { 7,  new[] { 7, 4, 10 } },
            { 9,  new[] { 9, 2 } },
            { 10, new[] { 10, 1 } },
            { 32, new[] { 32, 19 } },
            { 33, new[] { 33, 19, 16 } },
            { 34, new[] { 34, 10, 5, 14 } },
            { 36, new[] { 36, 16 } },
            { 39, new[] { 39, 10 } },
            { 50, new[] { 50, 4, 18, 7, 21, 5, 14, 17 } },
            { 51, new[] { 51, 2, 18, 7 } },
            { 52, new[] { 52, 1, 2, 21 } },
        };

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

        /// <summary>
        /// 立即切帧（对应反汇编直接写 entity+112=frameId）
        /// 不经过 Trans.wait 机制，直接触发 OnFrameTransit。
        /// 用于命中受击帧：反汇编命中时直接写帧号，不清零 wait，FrameDelay 冻结后续推进。
        /// 注意：只切帧和触发 state_entry/frame_update，不执行 frame 事件（frame 事件在下一个 TU 执行）
        /// </summary>
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N  = frameId;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateUpdate("state_exit");

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                StateMem.Clear();
                HitStun = 0;
                AllowSwitchDir = GetStatesSwitchDir(Frame.D.state);
                StateUpdate("state_entry");
            }

            // 设置 wait/next（对应反汇编 frame_update 中 entity+136/entity+112 的赋值）
            // 不执行 frame 事件——反汇编中直接写帧号后 frame 事件在下一帧 TU 执行
            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SetWait(Frame.D.wait, 99);
            Trans.SetNext(Frame.D.next, 99);
        }

        /// <summary>播放指定帧</summary>
        public virtual void PlayFrameByID(int frameId)
        {
            Trans?.Frame(frameId, 0);
        }

        #endregion

        #region 接口实现 - ILF2Poolable / ILF2Object

        public abstract override LF2ObjectType ObjectTypeEnum { get; }
        public abstract override void Reset();
        public abstract override void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>
        /// 销毁对象（对应 FLF livingobject.prototype.destroy）
        /// 参考：FLF livingobject.js:89-94
        /// </summary>
        public override void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>
        /// 当 FrameTransistor 检测到 next=1000 时调用
        /// 对应 FLF livingobject.js:655-658: state_update('destroy') + match.destroy_object($)
        /// </summary>
        public override void OnTransitDestroy()
        {
            StateUpdate("destroy");
            Destroy();
            // Release renderer (calls ResetState -> Reset -> Unregister)
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        #endregion

        #region 接口实现 - ISimObject（重写 LF2Entity 默认实现）

        public override void SimTransit(int tickIndex) => Transit();
        public override void SimTU(int tickIndex) => TUUpdate();

        /// <summary>
        /// PreInteraction 阶段（对应 NTSD 反汇编 GameMode_Process sub_41BDA0）
        /// 处理 kind=1/2/3/7 抓取/拾取碰撞，在所有对象 SerialTickAll 后统一执行。
        /// 子类（LF2Character）负责 override 并调用 Generic_PreInteraction()。
        /// </summary>
        public override void SimPreInteraction(int tickIndex) { }

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
