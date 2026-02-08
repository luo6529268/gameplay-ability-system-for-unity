using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色专用逻辑（继承 LF2LivingObject，对应 FLF character.js）
    /// 包含连招、OPoint、WPoint、武器持有等角色特有功能
    /// 
    /// 继承层次：LF2LivingObject → LF2Character
    /// 对应 FLF：livingobject → character
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\character.js
    /// </summary>
    public class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

        // ========== 角色专用模块 ==========

        /// <summary>
        /// 连招缓冲区（对应 FLF $.combo_buffer）
        /// </summary>
        public LF2ComboBufferModule ComboBuffer { get; private set; }

        /// <summary>
        /// OPoint 模块（对应 FLF character.opoint）
        /// </summary>
        public LF2ObjectPointModule ObjectPointModule { get; private set; }

        /// <summary>
        /// WPoint 模块（对应 FLF character.wpoint）
        /// </summary>
        public LF2WeaponPointModule WeaponPointModule { get; private set; }

        /// <summary>
        /// 受击计数器（对应 FLF $.health.fall/bdefend）
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;

        /// <summary>
        /// 角色属性（HP/MP 等）
        /// </summary>
        private readonly NTSDCharacterStats _characterStats;
        public override NTSDCharacterStats CharacterStats => _characterStats;

        // ========== 武器持有（对应 FLF $.hold）==========

        /// <summary>
        /// 当前持有的武器（对应 FLF $.hold.obj）
        /// </summary>
        private LF2ObjectRenderer _heldWeapon;

        // ========== Unity 组件引用 ==========
        private List<Sprite> _sprites;
        private Transform _groundTransform;
        private Vector3 _baseLocalPosition;

        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;

        // ========== 调试 ==========

        private bool _debugCollisionLog = false;
        private bool _debugComboLog = false;

        // ========== 构造函数 ==========

        public LF2Character(MoreMountains.TopDownEngine.Character hub)
        {
            _CharacterHub = hub;

            // 创建角色专用模块
            ComboBuffer = new LF2ComboBufferModule();
            ObjectPointModule = new LF2ObjectPointModule();
            WeaponPointModule = new LF2WeaponPointModule();
            _hitCounters = new LF2HitCountersModule();
            _characterStats = new NTSDCharacterStats();
            
            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            PS = new PhysicsState();
            Trans = new FrameTransistor();
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Sprite = new LF2Sprite();

            // 设置帧转换回调
            Trans.SetFrameTransitCallback(OnFrameTransit);

            // 初始化状态处理器
            InitializeStates();
        }

        // ========== 状态机初始化 ==========

        protected override void InitializeStates()
        {
            // 注册基础状态处理器 (0-17)
            _states[LF2States.Standing] = State_Standing;
            _states[LF2States.Walking] = State_Walking;
            _states[LF2States.Running] = State_Running;
            _states[LF2States.Attack] = State_Attack;
            _states[LF2States.Jump] = State_Jump;
            _states[LF2States.Dash] = State_Dash;
            _states[LF2States.Rowing] = State_Rowing;
            _states[LF2States.Defending] = State_Defending;
            _states[LF2States.BrokenDefend] = State_BrokenDefend;
            _states[LF2States.Catching] = State_Catching;
            _states[LF2States.BeingCaught] = State_BeingCaught;
            _states[LF2States.Injured] = State_Injured;
            _states[LF2States.Falling] = State_Falling;
            _states[LF2States.Frozen] = State_Frozen;
            _states[LF2States.Lying] = State_Lying;
            _states[LF2States.StopRunning] = State_StopRunning;
            _states[LF2States.Injured2] = State_Injured2;
            _states[LF2States.Charging] = State_Charging;
            _states[LF2States.Burning] = State_Burning;
        }

        protected override bool OnGenericStateEvent(string eventType, object eventData = null)
        {
            switch (eventType)
            {
                case "TU":
                    return Generic_TU();
                case "transit":
                    return Generic_Transit();
                case "frame":
                    return Generic_Frame();
                case "combo":
                    return Generic_Combo(eventData as string);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 通用状态处理器
        /// 对应 LF2 源码的 states.generic
        /// 处理所有状态共享的逻辑，如物理更新、输入缓冲、全局受击判定等
        /// </summary>
        private bool GenericStateHandler(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "hit":
                    // 💥 处理受击逻辑 (对应 FLF character.js hit处理)
                    return Generic_Hit(eventData);

                case "frame":
                    // 🖼️ 每帧执行的通用逻辑 (MP/HP恢复, OPoint生成等) (对应 FLF character.js:14-52)
                    return Generic_Frame();

                case "TU":
                    // ⏱️ 时间单元(Time Unit)更新 (状态机, buff更新, 物理重置) (对应 FLF character.js:54-183)
                    return Generic_TU();

                case "transit":
                    // 🚀 动态物理更新 (摩擦力, 位置更新) (对应 FLF character.js:185-190)
                    return Generic_Transit();

                case "combo":
                    // 🎮 通用输入处理 (多键连招映射, 方向键处理) (对应 FLF character.js:191-215)
                    return Generic_Combo( eventData as string);

                case "post_combo":
                    // 🛑 连招后处理 (清理缓存等) (对应 FLF character.js:217-220)
                    // TODO: 实现 pre_interaction() - 预处理交互 (武器拾取, 对象交互)
                    return false;

                case "state_exit":
                    // 🚪 状态退出清理 (清理连招缓冲) (对应 FLF character.js:221-228)
                    return HandleGenericStateExit(character, eventData as string);

                case "get_next_frame":
                    // 获取下一帧，默认返回 null 让系统使用配置文件中的 next
                    return false;

                default:
                    return false;
            }
        }


        #region Generic State Handlers

        /// <summary>
        /// 通用受击处理
        /// 处理角色被攻击时的反应：扣血、增加击倒值(Fall)、进入受伤或击飞状态
        /// </summary>
        private bool Generic_Hit(object eventData)
        {
            //if (target == null || target.PS == null || target.Frame.D == null) return false;
            //if (eventData is not LF2CollisionSystem.HitEvent evt) return false;
            //if (evt.attacker == null || evt.attacker.PS == null) return false;
            //if (evt.itr == null) return false;

            //// FLF 规则: State 14 (倒地) 状态下忽略受击 (无敌)
            //if (target.Frame.D.state == LF2States.Lying) return false;

            //InteractionArea itr = evt.itr;
            //LF2LivingObject attacker = evt.attacker;

            //// Phase 1: 仅处理通用的攻击类型 (0:普通, 4:防御无视?, 9, 15, 16)
            //if (!(itr.kind == 0 || itr.kind == 4 || itr.kind == 9 || itr.kind == 15 || itr.kind == 16))
            //    return false;

            //// 判断攻击是否来自正面
            //bool attackedFromFront = (attacker.PS.x > target.PS.x) == (target.PS.dir == "right");

            //// 计算攻击者方向 (用于击退计算)
            //// FLF: attdir = (att.PS.vx===0 ? att.dirh() : sign(att.PS.vx))
            //int attackerDir;
            //if (Mathf.Abs(attacker.PS.vx) > 0.0001f) attackerDir = attacker.PS.vx > 0 ? 1 : -1;
            //else attackerDir = attacker.PS.dir == "right" ? 1 : -1;

            //int compen = Mathf.Approximately(target.PS.y, 0f) ? 1 : 0;
            //float efDvx = itr.dvx != 0 ? attackerDir * (itr.dvx - compen) : 0f;
            //float efDvy = itr.dvy != 0 ? itr.dvy : 0f;

            //// === 防御逻辑分支 ===
            //// 如果处于防御状态且从正面被攻击 -> 判定为防御成功或防御崩坏
            //if (target.Frame.D.state == LF2States.Defending && attackedFromFront)
            //{
            //    if (itr.bdefend != 0)
            //        target.HitCounters.AddBdefend(Mathf.Abs(itr.bdefend));

            //    // 简单的防御后退与吸收逻辑 (参照 FLF GC.defend.absorb)
            //    if (!Mathf.Approximately(efDvx, 0f))
            //    {
            //        float abs = Mathf.Abs(efDvx);
            //        float absorb = abs >= 15f ? 5f : 0f;
            //        efDvx += (efDvx > 0f ? -1f : 1f) * absorb;
            //    }
            //    efDvy = 0f;

            //    // 判断是否防御崩坏 (Broken Defend)
            //    int defendFrame = target.HitCounters.Bdefend > FLF_DEFEND_BREAK_LIMIT
            //        ? LF2StandardFrames.DefendBroken
            //        : LF2StandardFrames.Defend1;

            //    target.Trans.Frame(defendFrame, 20);
            //    return true;
            //}

            //// === 受伤累积与状态选择 ===
            //int addFall = itr.fall != 0 ? itr.fall : FLF_DEFAULT_FALL_VALUE;
            //target.HitCounters.AddFall(addFall);

            //// 判定是否进入击飞/跌倒状态 (Falling)
            //// 条件：在空中、有垂直速度、或 Fall 值超过 KO 阈值
            //if (target.PS.y < 0f || target.PS.vy < 0f || target.HitCounters.Fall > FLF_FALL_KO)
            //{
            //    target.HitCounters.ResetFall();
            //    // FLF 规则: 进入 Falling 时重置 vy
            //    target.PS.vy = 0f;
            //    // 根据攻击方向选择向前跌倒还是向后跌倒
            //    target.Trans.Frame(attackedFromFront ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack, 21);
            //    return true;
            //}

            //// 否则进入普通受伤状态 (Injured)，根据 Fall 值选择不同程度的受伤帧
            //int fall = target.HitCounters.Fall;
            //int injuredFrame;
            //if (fall > 0 && fall <= 20) injuredFrame = LF2StandardFrames.Injured;
            //else if (fall > 20 && fall <= 30) injuredFrame = LF2StandardFrames.Injured2;
            //else if (fall > 30 && fall <= 40) injuredFrame = LF2StandardFrames.Injured4;
            //else if (fall > 40 && fall <= 60) injuredFrame = LF2StandardFrames.Injured6;
            //else
            //{
            //    // Fall 过高，强制跌倒
            //    target.HitCounters.ResetFall();
            //    target.PS.vy = 0f;
            //    target.Trans.Frame(attackedFromFront ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack, 21);
            //    return true;
            //}

            //target.Trans.Frame(injuredFrame, 20);
            return true;
        }

        /// 通用时间单元更新 (TU)
        /// 对应 FLF character.js:54-183
        /// 负责处理周期性的逻辑，如 Buff 消失、状态恢复、物理 Tick
        /// </summary>
        private bool Generic_TU()
        {
            // 1. 消失效果状态机 (FLF:56-82)
            // TODO: 需要特效系统

            // 2. 死亡闪烁效果 (FLF:84-102)
            // TODO: 需要特效系统

            // 3. 状态更新与物理处理 (FLF:104-141)
            // TODO: post_interaction, 落地检测等

            // 4. 生命值自然恢复 (每12帧) (FLF:145-149)
            // TODO: 需要 HP 系统

            // 5. 治疗效果处理 (每8帧) (FLF:152-160)
            // TODO: 需要效果系统

            // 6. 魔法值自然恢复 (每?帧) (FLF:163-167)
            // TODO: 需要 MP 系统

            // 7. 状态恢复 (FLF:170-171)
            if (PS.y == 0 && PS.vy == 0 && Frame.N == LF2StandardFrames.JumpingAir && Frame.PN != LF2StandardFrames.JumpingUp)
            {
                TransitionToFrame(999);
            }
            // A) fell_onto_ground（PS.y==0 && PS.vy>0）- 对齐 FLF js:115-126
            else if (PS.y == 0 && PS.vy > 0)
            {
                var res = StateUpdate("fell_onto_ground",out int frameId);
                if (res && frameId > 0)
                {
                    TransitionToFrame(frameId, 15);
                }
                else
                {
                    // 默认分支：PS.vy=0 + 落地瞬间摩擦
                    PS.vy = 0;
                    float fricX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vx);
                    float fricZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vz);
                    CharacterMechanics.LinearFriction(PS, fricX, fricZ);
                }
            }
            // B) fall_onto_ground（PS.y+PS.vy>=0 && PS.vy>0）- 对齐 FLF js:127-141
            else if ((PS.y + PS.vy) >= 0 && PS.vy > 0)
            {
                var res = StateUpdate("fall_onto_ground", out int frameId);
                if (res && frameId > 0)
                {
                    TransitionToFrame(frameId, 15);
                }
                else
                {
                    // 默认分支：Frozen 不动；JumpingAir→Crouch；其它→Crouch2
                    if (Frame.D.state == LF2States.Frozen)
                    {
                        // 冰冻状态不处理
                    }
                    else if (Frame.N == LF2StandardFrames.JumpingAir)
                    {
                        TransitionToFrame(LF2StandardFrames.Crouch, 15);
                    }
                    else
                    {
                        TransitionToFrame(LF2StandardFrames.Crouch2, 15);
                    }
                }
            }
            // TODO: 需要 fall/bdefend 系统 (如防御值随时间恢复)

            // 8. 连击缓冲系统 (FLF:174-182)
            // TODO: 需要连击缓冲系统

            ComboBuffer?.ReduceTimeout();

            return false;
        }

        private bool Generic_Transit()
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, "transit", null);
        }

        private bool Generic_Frame()
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, "frame", null);
        }

        private bool Generic_Combo(string combo)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, "combo", combo);
        }

        #endregion

        #region Specific State Handlers

        // 状态 0: 站立
        private bool State_Standing(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 1: 行走
        private bool State_Walking(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 2: 奔跑
        private bool State_Running(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 3: 攻击
        private bool State_Attack(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 4: 跳跃
        private bool State_Jump(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 5: 冲刺
        private bool State_Dash(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 6: 划船
        private bool State_Rowing(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 7: 防御
        private bool State_Defending(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 8: 防御崩坏
        private bool State_BrokenDefend(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 9: 抓取
        private bool State_Catching(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 10: 被抓取
        private bool State_BeingCaught(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 11: 受伤
        private bool State_Injured(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 12: 跌倒
        private bool State_Falling(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 13: 冰冻
        private bool State_Frozen(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 14: 躺地
        private bool State_Lying(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 15: 停止奔跑/混合状态
        private bool State_StopRunning(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 16: 受伤2 (Dance of Pain)
        private bool State_Injured2(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 17: 蓄力
        private bool State_Charging(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        // 状态 18: 燃烧
        private bool State_Burning(string eventType, object eventData)
        {
            // 桥接到 CharacterStates（渐进式迁移）
            return CharacterStates.Instance.HandleStateEvent(this, eventType, eventData);
        }

        #endregion

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            // 角色通过 Character Hub 初始化，不使用此方法
        }

        public override void Reset()
        {
            ComboBuffer?.Reset();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            _heldWeapon = null;
        }

        public override void Destroy()
        {
            Reset();
        }

        // ========== 初始化（由 Character Hub 调用）==========

        /// <summary>
        /// 模块初始化（对应 LF2CharacterAnimator.ModuleInitialize）
        /// </summary>
        public void ModuleInitialize(SpriteRenderer spriteRenderer,List<Sprite> sprites,Transform groundTransform,Vector3 baseLocalPosition)
        {
            _sprites = sprites;
            _groundTransform = groundTransform;
            _baseLocalPosition = baseLocalPosition;

            // 初始化物理计算层
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null ? BoundaryWallManager.Instance.IsPointWalkable : null;

            // 初始化物理状态
            PS.FromUnityPosition(groundTransform.position);
            PS.vx = 0;
            PS.vy = 0;
            PS.vz = 0;

            // 初始化精灵模块
            Sprite.Initialize(spriteRenderer, sprites);

            AllowSwitchDir = true;
        }

        /// <summary>
        /// 模块绑定（对应 LF2CharacterAnimator.ModuleBind）
        /// </summary>
        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            // 加载帧数据
            FrameCache.Load(frameDataWrapper);

            // 初始化帧信息
            Frame.D = FrameCache.GetFrameDataById(0);
            Frame.PN = 0;
            Frame.N = 0;

            // 重置模块
            ComboBuffer?.Reset();
            ItrRest?.Reset();
            _hitCounters?.Reset();

            // 绑定 mass
            _mass = NTSDSpec.GetMassOrDefault(characterId);

            // 绑定 OPoint Factory
            if (ObjectPointModule != null && ObjectPointModule.Factory == null && LF2ObjectPointFactory.Instance != null)
            {
                ObjectPointModule.SetFactory(LF2ObjectPointFactory.Instance);
            }
        }

        /// <summary>
        /// 初始化角色属性
        /// </summary>
        public void Initialize(int maxHp, int maxMp)
        {
            CharacterStats.Initialize(maxHp, maxMp);
            Health.HP = maxHp;
            Health.MP = maxMp;
            ComboBuffer.Reset();
            HitCounters.Reset();
            ItrRest.Reset();
        }

        /// <summary>
        /// 绑定 OPoint Factory
        /// </summary>
        public void BindOPointFactory(LF2ObjectPointFactory factory)
        {
            ObjectPointModule.SetFactory(factory);
        }

        // ========== 核心生命周期（对应 FLF livingobject/character）==========

        /// <summary>
        /// TU Update - 每个时间单位的主循环
        /// 对应 FLF livingobject.TU_update()
        /// </summary>
        public override void TUUpdate()
        {
            // 重置摩擦力
            PS?.ResetFriction();

            // TU 事件
            CharacterStates.Instance.HandleStateEvent(this, "TU", null);
        }

        /// <summary>
        /// 连招更新 - 对应 FLF character.combo_update()
        /// 参考：FLF character.js:1800-1846
        /// </summary>
        protected override void ComboUpdate()
        {
            string rawCombo = ComboBuffer?.Combo;
            string K = rawCombo;
            if (string.IsNullOrEmpty(K)) { K = null; }

            // 特殊处理：跳跃攻击组合
            if (rawCombo == "jump-att") { K = "jump"; }

            StateHandler CurStateHandler = _states[Frame.D.state];

            bool CurStateResult = CurStateHandler?.Invoke("combo", K) ?? false;
            bool generalResult = false;
            if (!CurStateResult)
            {
                generalResult = OnGenericStateEvent("combo", K);
            }

            CurStateHandler?.Invoke("post_combo");
            OnGenericStateEvent("post_combo");

            ComboBuffer?.AfterComboUpdate(CurStateResult, generalResult, rawCombo: rawCombo, mappedCombo: K);
        }

        /// <summary>
        /// 物理+武器点更新
        /// 对应 FLF transit 阶段的 mech.dynamics() + wpoint()
        /// </summary>
        public void TransitDynamicsAndWPoint()
        {
            ApplyDynamics();
            WPointUpdate();
        }

        /// <summary>
        /// Transit 阶段的物理和武器点处理（兼容 LF2CharacterAnimator）
        /// </summary>
        public override void Transit_DynamicsAndWPoint()
        {
            TransitDynamicsAndWPoint();
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            LF2DynamicsApplier.Apply(
                _character: _CharacterHub,
                mechanics: _mech,
                mass: _mass,
                isPointWalkable: _cachedIsPointWalkable,
                logWarning: _debugCollisionLog ? s => Log.Warn(s) : (Action<string>)null,
                debugCollisionLog: _debugCollisionLog,
                groundTransform: _groundTransform,
                baseLocalPosition: _baseLocalPosition
            );
        }

        /// <summary>
        /// 武器点更新
        /// 对应 FLF wpoint()
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 帧转换回调 ==========

        /// <summary>
        /// 帧转换回调（由 FrameTransistor 调用）
        /// 对应 FLF trans.trans() 中的切帧逻辑
        /// </summary>
        private void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
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
                CharacterStates.Instance.HandleStateEvent(this, "state_exit", ComboBuffer?.Combo);
            }

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                StateMem.Clear();

                bool oldSwitchDir = AllowSwitchDir;
                AllowSwitchDir = CharacterStates.Instance.GetStatesSwitchDir(Frame.D.state);

                CharacterStates.Instance.HandleStateEvent(this, "state_entry", null);

                if (!switchDirAfterTrans)
                {
                    if (AllowSwitchDir && !oldSwitchDir)
                    {
                        var input = _CharacterHub?._CharacterInput;
                        if (input != null)
                        {
                            if (input.IsLeft)
                                SetDirection(DIRECTION.LEFT);
                            if (input.IsRight)
                                SetDirection(DIRECTION.RIGHT);
                        }
                    }
                }
            }

            if (switchDirAfterTrans)
            {
                DIRECTION currentDir = PS.dir == "left" ? DIRECTION.LEFT : DIRECTION.RIGHT;
                SetDirection(currentDir == DIRECTION.RIGHT ? DIRECTION.LEFT : DIRECTION.RIGHT);
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
            if (_sprites != null && Frame.D != null)
            {
                int picIndex = Frame.D.pic;
                if (picIndex >= 0 && picIndex < _sprites.Count)
                {
                    Sprite.ShowPic(picIndex);
                }
            }

            // 应用帧力
            if (!CharacterStates.Instance.HandleStateEvent(this, "frame_force", null))
            {
                FrameForceInternal();
            }

            // 设置等待和下一帧
            Trans.SetWait(Frame.D?.wait ?? 1, 99);
            Trans.SetNext(Frame.D?.next ?? 0, 99);

            // 状态 frame 事件
            CharacterStates.Instance.HandleStateEvent(this, "frame", null);

            // 播放音效
            if (Frame.D != null && !string.IsNullOrEmpty(Frame.D.sound))
            {
                // TODO: 播放音效
            }
        }

        /// <summary>
        /// 帧力应用（内部）
        /// 对应 FLF frame_force()
        /// </summary>
        private void FrameForceInternal()
        {
            if (Frame.D == null) return;
            int dirv = _CharacterHub?._CharacterInput?.Dirv ?? 0;
            LF2FrameForceApplier.Apply(PS, Frame.D, dirv);
        }

        // ========== 方向控制 ==========

        /// <summary>
        /// 设置方向
        /// </summary>
        public override void SetDirection(DIRECTION direction)
        {
            // 表现层：翻转角色
            if (_groundTransform != null)
            {
                _groundTransform.localRotation = (direction == DIRECTION.LEFT)
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
            }

            base.SetDirection(direction);
        }

        /// <summary>
        /// 获取当前朝向
        /// </summary>
        public DIRECTION FacingDir
        {
            get
            {
                if (PS != null && !string.IsNullOrEmpty(PS.dir))
                {
                    return PS.dir == "left" ? DIRECTION.LEFT : DIRECTION.RIGHT;
                }
                return DIRECTION.RIGHT;
            }
        }

        // ========== 连招处理 ==========

        /// <summary>
        /// 连招检测回调
        /// 参考：FLF character.js:1684-1700
        /// </summary>
        public void OnComboDetected(ComboConfig.ComboDefinition combo)
        {
            if (Frame.D == null) return;

            ComboBuffer.OnComboDetected(
                combo: combo,
                allowSwitchDir: AllowSwitchDir,
                setDirectionByString: SwitchDir,
                timeoutFrames: NTSDGlobal.Combo.Timeout,
                debugLog: _debugComboLog,
                stableId: StableId
            );
        }

        // ========== 武器持有 ==========

        /// <summary>
        /// 持有武器（对应 FLF character.prototype.hold_weapon）
        /// </summary>
        public void HoldWeapon(LF2ObjectRenderer weapon)
        {
            _heldWeapon = weapon;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public LF2ObjectRenderer GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 丢弃武器（对应 FLF character.prototype.drop_weapon）
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            if (_heldWeapon?.LogicObject is LF2LightWeapon lightWeapon)
            {
                lightWeapon.Drop(dvx, dvy);
            }
            else if (_heldWeapon?.LogicObject is LF2HeavyWeapon heavyWeapon)
            {
                heavyWeapon.Drop(dvx, dvy);
            }

            _heldWeapon = null;
        }

        // ========== 帧播放接口 ==========

        /// <summary>
        /// 转换到指定帧
        /// </summary>
        public override void TransitionToFrame(int frameId, int authority = 20)
        {
            Trans.Frame(frameId, authority);
        }

        /// <summary>
        /// 设置下一帧
        /// </summary>
        public void SetNextFrame(int frameId, int authority = 99)
        {
            Trans.SetNext(frameId, authority);
        }

        /// <summary>
        /// 设置等待时间
        /// </summary>
        public void SetWait(int value, int authority = 99)
        {
            Trans.SetWait(value, authority);
        }

        /// <summary>
        /// 执行帧转换
        /// </summary>
        public void TransTrans()
        {
            Trans.Trans();
        }

        // ========== 状态内存 ==========

        public bool GetStateMemory<T>(string key, out T value)
        {
            if (StateMem.TryGetValue(key, out object obj) && obj is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }

        public void SetStateMemory<T>(string key, T value)
        {
            StateMem[key] = value;
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 通过State值获取第一帧ID
        /// </summary>
        public int GetFirstFrameByState(int targetState)
        {
            int id = FrameCache.GetFirstFrameByState(targetState);
            if (id < 0)
            {
                Log.Warn("[LF2Character] State {0} not found!", targetState);
            }
            return id;
        }

        /// <summary>
        /// 获取帧数据
        /// </summary>
        public LF2FrameData GetFrameDataById(int frameId)
        {
            return FrameCache.GetFrameDataById(frameId);
        }

        // ========== 当前帧信息（兼容属性）==========

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;
        public int CurrentState => Frame.D?.state ?? 0;

        // ========== 额外方法 ==========

        /// <summary>
        /// 重新加载角色帧数据
        /// </summary>
        public void ReloadCharacterFrameData()
        {
            if (_CharacterHub == null) return;

            // 重新绑定帧数据
            if (_FrameDataWrapper != null)
            {
                FrameCache.Load(_FrameDataWrapper);
            }

            // 重置到第一帧
            Frame.D = FrameCache.GetFrameDataById(0);
            Frame.PN = 0;
            Frame.N = 0;
        }

        /// <summary>
        /// 帧动画振荡（在指定帧范围内来回播放）
        /// </summary>
        public void FrameAniOscillate(int from, int to)
        {
            if (_animationInfo.frameIndex < from || _animationInfo.frameIndex > to)
            {
                _animationInfo.IsUp = true;
                _animationInfo.frameIndex = from + 1;
            }

            if (_animationInfo.frameIndex < to && _animationInfo.IsUp)
                Trans.SetNext(_animationInfo.frameIndex++);
            else if (_animationInfo.frameIndex > from && !_animationInfo.IsUp)
                Trans.SetNext(_animationInfo.frameIndex--);

            if (_animationInfo.frameIndex == to)
                _animationInfo.IsUp = false;
            if (_animationInfo.frameIndex == from)
                _animationInfo.IsUp = true;
        }

        /// <summary>
        /// 动画信息（用于振荡动画）
        /// </summary>
        private struct AnimationInfo
        {
            public int frameIndex;
            public bool IsUp;
        }
        private AnimationInfo _animationInfo;
    }
}
