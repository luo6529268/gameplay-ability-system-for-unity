using BeatEmUpTemplate2D;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Pool;

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
        private ILF2Object _heldWeapon;

        // ========== Unity 组件引用 ==========
        private Vector3 _baseLocalPosition;

        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;

        // ========== 抓取系统字段（Catching System Fields）==========

        protected Vector3 caught_b_holdpoint;
        protected CatchPoint caught_b_cpoint;
        protected int caught_b_adir;
        protected int caught_b_vdir;
        protected int? caught_throwz;
        protected int? caught_throwinjury;
        protected int caught_decrease_counter;  // 抓取递减计数器（被抓者按键时递减，归零释放）

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
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);

            // 初始化状态处理器
            InitializeStates();
        }

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
        public void ModuleInitialize(SpriteRenderer spriteRenderer,List<Sprite> sprites,Vector3 baseLocalPosition)
        {
            _baseLocalPosition = baseLocalPosition;

            // 初始化物理计算层
            _mech = new CharacterMechanics();
            Controller = _CharacterHub._CharacterInput;
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null ? BoundaryWallManager.Instance.IsPointWalkable : null;

            // 初始化物理状态
            PS.FromUnityPosition(_CharacterHub.transform.position);
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

        /// <summary>
        /// 通用状态处理器
        /// 对应 LF2 源码的 states.generic
        /// 处理所有状态共享的逻辑，如物理更新、输入缓冲、全局受击判定等
        /// </summary>
        protected override bool OnGenericStateEvent(string eventType, object eventData = null)
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
                    return Generic_Combo(eventData as string);

                case "post_combo":
                    // 🛑 连招后处理 (清理缓存等) (对应 FLF character.js:217-220)
                    // TODO: 实现 pre_interaction() - 预处理交互 (武器拾取, 对象交互)
                    Generic_PreInteraction();
                    return false;

                case "state_exit":
                    // 🚪 状态退出清理 (清理连招缓冲) (对应 FLF character.js:221-228)
                    return Generic_StateExit();
            }

            return false;
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
                var res = StateUpdate("fell_onto_ground", out int frameId);
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

        /// <summary>
        /// 通用物理转换 (Transit)
        /// 对应 FLF character.js:185-190
        /// </summary>
        private bool Generic_Transit()
        {
            // dynamics: position, friction, gravity
            // 更新动态物理效果（位置、摩擦力、重力）
            // 任何位置变更将在下一个时间单位(TU)更新到屏幕
            // 更新武器位置，使其跟随角色移动
            ApplyDynamics();
            WPointUpdate();
            return false;
        }

        /// <summary>
        /// 通用帧逻辑 (Frame)
        /// 对应 FLF character.js:14-52
        /// </summary>
        private bool Generic_Frame()
        {

            // 处理生命值减少的逻辑
            if (Frame.D.mp != 0)
            {
                // 检查当前帧是否有MP变化值
                // 检查当前帧是否由前一帧的next属性触发

                if (FrameCache.GetFrameDataById(Frame.PN).next == Frame.N)
                {
                    // 如果MP变化值为负数（消耗MP）
                    if (Frame.D.mp <= 0)
                    {
                        // 如果不在F6模式（非特殊模式）

                        // 记录MP使用量
                        // 如果MP值小于0
                        // 将MP值设为0
                        // 触发受击动画帧

                    }
                }
                else
                {
                    // 计算MP变化值（取模1000得到实际MP变化）
                    int dmp = Frame.D.mp & 1000;
                    float dhp = Mathf.Floor(Frame.D.mp / 1000) * 10;
                    // 如果不在F6模式

                    // 记录MP使用量
                    // 处理伤害
                }
            }

            // 2. OPoint (Object Point) 处理 - 用于生成武器、投射物
            // 对应 FLF character.js:52
            //ObjectPointModule.ProcessTransit(this);
            return false;
        }

        /// <summary>
        /// 通用连招处理器 (Generic Combo)
        /// 对应 FLF character.js line 191-215 的 generic case 'combo'
        /// 
        /// <para>工作流程：</para>
        /// <list type="number">
        /// <item>1. 处理单键输入 (硬编码)：如 left, right, jump 等基础移动逻辑。</item>
        /// <item>2. 处理多键连招：通过 Tag 映射 (如 D>A 映射到 Tag "Fa")。</item>
        /// <item>3. 调用 id_update：允许角色脚本覆盖通用逻辑 (id_update('generic_combo'))。</item>
        /// <item>4. 处理方向切换：如输入 D>A 强制角色转向右侧。</item>
        /// <item>5. 执行跳转：根据 Frame Data 中的 Tag 跳转到目标帧。</item>
        /// </list>
        /// </summary>
        private bool Generic_Combo(string combo)
        {
            if (string.IsNullOrEmpty(combo))
                return false;

            // === 1. 处理单键连招 (硬编码逻辑) ===
            // 对应 FLF character.js:239-338 State 0 的 case 'combo' 部分逻辑
            switch (combo)
            {
                case "left":
                case "right":
                case "left-left":
                case "right-right":
                    // 这些基础移动指令通常由 Standing/Walking 状态自行处理，通用逻辑直接返回
                    return false;

                default:
                    // 特殊处理: Rudolf 的 DJA 变身
                    if (combo == "DJA")
                    {
                        // TODO: Rudolf 变身检查逻辑
                        // if (character.transform_character != null && character.transform_character.is_rudolf_transform) { ... }
                    }
                    break;
            }

            // === 2. 处理多键连招 (Tag 映射机制) ===
            // 对应 FLF character.js:191-215

            // Step 1: 将输入序列 (如 "D>A") 映射为内部 Tag (如 "Fa")
            string tag = ComboConfig.GetComboTag(combo);
            if (string.IsNullOrEmpty(tag))
                return false;

            // Step 2: 检查当前帧的数据中是否定义了该 Tag 的跳转目标 (hit_Fa: 123)
            int targetFrame = Frame.D.Hit[tag];
            if (targetFrame < 0)
                return false;

            // 检查连招是否有效
            // Step 3: 尝试调用角色特定逻辑 (id_update) 进行拦截
            // 对应 FLF: if (!$.id_update('generic_combo', K, tag))
            //if (character._Character != null && character._Character._IdUpdate != null)
            //{
            //    if (character._Character._IdUpdate.TryInvokeGenericCombo(combo, tag, targetFrame))
            //    {
            //        return true;  // 角色特定逻辑已处理，不再执行默认跳转
            //    }
            //}

            // 如果不是通用连招
            // 获取连招方向
            // Step 4: 处理连招的方向要求 (如 D>A 要求必须朝右)
            string dir = ComboConfig.GetComboDirection(combo);
            if (!string.IsNullOrEmpty(dir))
            {
                // 切换方向
                SwitchDir(dir);
            }

            // 执行连招动画
            // 返回成功状态
            TransitionToFrame(targetFrame, LF2StateConstants.GenericComboWait);
            StateReturnFrame = 1;
            return true;
        }

        private bool Generic_PreInteraction() 
        {
            LF2FrameData frame = FrameCache.GetFrameDataById(Frame.N);
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame == null || sceneQuery == null) return false;
            if (PS == null) return false;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return false;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return false;

            var preItrs = ListPool<InteractionArea>.Get();
            preItrs.Capacity = 4;

            for (int i = 0; i < itrs.Count; i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;
                if (!kindService.IsPreInteractionKind(itr.kind)) continue;
                preItrs.Add(itr);
            }

            if (preItrs.Count == 0)
            {
                ListPool<InteractionArea>.Release(preItrs);
                return false;
            }


            var itrVolumes = PS.GetItrVolumes(preItrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: NTSDGlobal.Default.Itr.ZWidth);
            int count = Mathf.Min(preItrs.Count, itrVolumes.Count);
            for (int i = 0; i < count; i++)
            {
                var itr = preItrs[i];
                var vol = itrVolumes[i];

                var candidates = sceneQuery.QueryBodies(vol, this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (!CanPreInteractTarget(kindService, itr, target)) continue;

                    if (!DispatchPreInteractionByKind(kindService, itr, target)) continue;

                    //target.ItrVrestUpdate(StableId, itr);
                    ListPool<InteractionArea>.Release(preItrs);
                    return true;
                }
            }

            ListPool<InteractionArea>.Release(preItrs);
            return false;
        }

        private bool CanPreInteractTarget(INTSDItrKindService kindService, InteractionArea itr, LF2LivingObject target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (kindService == null) return false;

            return true;
        }

        private bool DispatchPreInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2LivingObject target)
        {
            if (kindService == null) return false;

            switch (itr.kind)
            {
                case 1:
                case 3:
                    return HandlePreInteractionKind(itr, target);
                case 2:
                    return HandlePreInteractionKind2(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理抓取类型的预交互（itr kind 1/3）
        /// 对应 FLF character.js:2216-2246 pre_interaction
        /// </summary>
        private bool HandlePreInteractionKind(InteractionArea itr, LF2LivingObject target)
        {
            // 只处理角色类型
            if (target.Type != LF2ObjectType.Character)
                return false;

            // 检查抓取条件：(kind==1 && 目标处于 Injured2) || kind==3
            bool canCatch = (itr.kind == 1 && target.GetState() == LF2States.Injured2) || itr.kind == 3;
            if (!canCatch)
                return false;

            // 检查 itr arest（防止重复抓取）
            if (!ItrArestTest())
                return false;

            // 转换为 LF2Character 以调用 CaughtA
            var targetChar = target as LF2Character;
            if (targetChar == null)
                return false;

            // 调用被抓者的 CaughtA，获取抓取方向
            string dir = targetChar.CaughtA(itr, this, new Vector3(PS.x, PS.y, PS.z));
            if (dir == null)
                return false;

            // 抓取成功，更新 itr arest
            ItrArestUpdate(itr);

            // 原版 LF2：抓取者固定切换到帧 120（正面/背面相同）
            TransitionToFrame(LF2StandardFrames.Catching, 10);

            // 设置抓取目标
            Catching = target;

            return true;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2LivingObject target)
        {
            if (_heldWeapon != null)
                return false;

            if (target.Type != LF2ObjectType.LightWeapon && target.Type != LF2ObjectType.HeavyWeapon)
                return false;

            var weapon = target as LF2WeaponBase;
            if (weapon == null || !weapon.Pick(this))
                return false;

            ItrArestUpdate(itr);

            if (target.Type == LF2ObjectType.LightWeapon)
                TransitionToFrame(LF2StandardFrames.PickingLight, 10);
            else if (target.Type == LF2ObjectType.HeavyWeapon)
                TransitionToFrame(LF2StandardFrames.PickingHeavy, 10);

            HoldWeapon(weapon);
            return true;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2LivingObject target)
        {
            // 检查是否处于攻击状态
            if (Controller == null || !Controller.IsAttack)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// 通用状态退出清理
        /// 对应 FLF character.js:221-228
        /// </summary>
        private bool Generic_StateExit()
        {
            // 清除双击指令缓存 (防止状态切换后误触发跑动)
            // 对应 FLF:222-227
            switch (ComboBuffer?.Combo)
            {
                case "left-left":
                case "right-right":
                    ComboBuffer?.OnClearCombo();
                    break;
            }
            return false;
        }

        /// <summary>
        /// 被抓取处理（对应 FLF character.prototype.caught_a）
        /// 由抓取者调用，在被抓目标身上执行。
        /// </summary>
        /// <param name="itr">抓取者的 itr 数据</param>
        /// <param name="attacker">抓取者</param>
        /// <param name="attackerPos">抓取者位置</param>
        /// <returns>"front"/"back" 表示抓取方向，null 表示抓取失败</returns>
        public string CaughtA(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos)
        {
            // FLF:2457 - 再次验证抓取条件
            if (!((itr.kind == 1 && GetState() == LF2States.Injured2) || itr.kind == 3))
                return null;

            // FLF:2459 - 判断正面/背面
            bool isFront = (attackerPos.x > PS.x) == (PS.dir == "right");

            // 原版 LF2：被抓者固定切换到帧 130（PickedCaught）
            // 正面/背面的区分由 cpoint 的 fronthurtact/backhurtact 控制
            TransitionToFrame(LF2StandardFrames.PickedCaught, 22);

            // FLF:2464 - 重置倒地值
            //if (Health != null) Health.Fall = 0;

            // FLF:2465-2466 - 记录抓取者
            Catching = attacker;

            // FLF:2467 - 丢弃武器
            DropWeapon();

            return isFront ? "front" : "back";
        }

        #endregion

        #region Specific State Handlers

        public override bool GetStatesSwitchDir(int stateId)
        {
            switch (stateId)
            {
                case LF2States.Standing:       // 站立：允许转身
                case LF2States.Walking:        // 行走：允许转身
                case LF2States.Jump:         // 跳跃：允许空中转身 (LF2机制)
                case LF2States.Defending:     // 防御：允许转身
                    return true;

                case LF2States.Attack: // 攻击：锁定方向
                case LF2States.Running: // 奔跑：锁定方向
                case LF2States.Dash:// 冲刺：锁定方向
                case LF2States.Rowing: // 划船：锁定方向
                case LF2States.BrokenDefend:  // 防破：锁定方向
                case LF2States.Catching:  // 抓人：锁定方向
                case LF2States.BeingCaught:  // 被抓：锁定方向
                case LF2States.Injured: // 受伤：锁定方向
                case LF2States.Falling: // 跌倒：锁定方向
                case LF2States.Frozen:// 冰冻：锁定方向
                case LF2States.Lying: // 倒地：锁定方向
                case LF2States.StopRunning: // 停跑：锁定方向
                case LF2States.Injured2: // 受伤2：锁定方向
                    break;

                default:
                    break;
            }

            return false;
        }


        /// <summary>
        /// 站立状态处理器 (State 0)
        /// 对应 FLF character.js:244-338
        /// 处理角色的静止、基础按键响应
        /// </summary>
        private bool State_Standing(string eventType, object eventData)
        {
            {
                switch (eventType)
                {
                    case "frame":
                        if(IsHeavyWeapon())
                            TransitionToFrame(LF2StandardFrames.HeavyObjWalk0);

                        break;

                    case "combo":
                        // 站立状态的输入响应 (对应 FLF Line 250-338)
                        string comboKey = eventData as string;
                        Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "Standing", eventType,comboKey);
                        // === 方向键与跳跃键处理 (FLF Line 253-272) ===
                        switch (comboKey)
                        {
                            case "left":
                            case "right":
                            case "up":
                            case "down":
                            case "jump":
                            case "":
                            case null:
                                // 检查是否有实际方向输入
                                {
                                    bool hasDx = Controller.IsLeft != Controller.IsRight;
                                    bool hasDz = Controller.IsUp != Controller.IsDown;
                                    if (hasDx || hasDz)
                                    {
                                        if (IsHeavyWeapon())
                                        {
                                            if (hasDx) PS.vx = Dirh() * _FrameDataWrapper.characterData.heavy_walking_speed;
                                            PS.vz = Dirv() * _FrameDataWrapper.characterData.heavy_walking_speedz;
                                        }
                                        else
                                        {
                                            // 除非按下的是跳跃键，否则切换到行走状态
                                            if (comboKey != "jump")
                                            {
                                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.WalkingStart, "方向键按下 -> 行走");
                                                TransitionToFrame(LF2StandardFrames.WalkingStart, 5);
                                            }

                                            // 设置速度 (对应 FLF Line 265-270)
                                            // 注意: FLF 在 Standing 状态不使用 xFactor (斜向减速)，只有 Walking 状态使用
                                            var characterData = _FrameDataWrapper?.characterData;
                                            if (characterData == null) return false;

                                            if (hasDx) PS.vx = Dirh() * characterData.walking_speed;
                                            PS.vz = Dirv() * characterData.walking_speedz;
                                        }

                                    }
                                }
                                break;
                        }

                        // === 动作键处理 ===
                        switch (comboKey)
                        {
                            case "left-left":
                            case "right-right":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.RunningStart, "双击方向键 -> 奔跑");
                                if (IsHeavyWeapon())
                                    TransitionToFrame(LF2StandardFrames.HeavyObjRun, LF2StateConstants.ComboTransitionWait);
                                else
                                    TransitionToFrame(LF2StandardFrames.RunningStart, LF2StateConstants.ComboTransitionWait);

                                StateReturnFrame = 1;
                                return true;

                            case "def":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Defend, "防御键 -> 防御");
                                if (IsHeavyWeapon()) 
                                {
                                    StateReturnFrame = 1;
                                    return true;
                                }

                                TransitionToFrame(LF2StandardFrames.Defend, LF2StateConstants.ComboTransitionWait);
                                StateReturnFrame = 1;
                                return true;

                            case "jump":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Jumping, "跳跃键 -> 跳跃");
                                if (IsHeavyWeapon())
                                {
                                    //                                return true;

                                }
                                else
                                {
                                    //TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
                                    //                                return true;

                                }

                                TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
                                StateReturnFrame = 1;
                                return true;

                            case "att":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Punch, "攻击键 -> 挥拳");
                                if (_heldWeapon != null)
                                {
                                    bool hasDx = Controller.IsLeft != Controller.IsRight;

                                    if (IsHeavyWeapon())
                                    {
                                        TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "just_throw")) 
                                    {
                                        TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "stand_throw"))
                                    {
                                        TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                    {
                                        int NormalWeaponAtck = UnityEngine.Random.value < 0.5f ? LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
                                        TransitionToFrame(NormalWeaponAtck, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                }

                                // 重拳检测：用帧72/73的itr范围检测前方是否有近身敌人（itr kind:6）
                                var sceneQuery = Match?.SceneQuery;
                                if (sceneQuery != null)
                                {
                                    var superPunchFrame = FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch2)
                                                       ?? FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch2 + 1);
                                    if (superPunchFrame?.itrs != null && superPunchFrame.itrs.Count > 0)
                                    {
                                        float spriteW = GetSpriteWidthPxForCollision();
                                        var itrVol = PS.GetItrVolume(superPunchFrame.itrs[0], superPunchFrame.centerx, superPunchFrame.centery, spriteW);
                                        var hits = sceneQuery.QueryItrs(itrVol, this, 6, Team);
                                        if (hits != null && hits.Count > 0)
                                        {
                                            TransitionToFrame(LF2StandardFrames.SuperPunch, LF2StateConstants.ComboTransitionWait);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                    }
                                }

                                // 随机选择挥拳动画 (60 或 65)
                                int punchFrame = UnityEngine.Random.value < 0.5f ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                                TransitionToFrame(punchFrame, LF2StateConstants.ComboTransitionWait);
                                return true;
                        }

                        break;
                }
                return false;
            }
        }


        /// <summary>
        /// 行走状态处理器 (State 1)
        /// 对应 FLF character.js:341-400
        /// 
        /// <para>特性：</para>
        /// <list type="bullet">
        /// <item>在函数开头计算输入 (dx, dz)。</item>
        /// <item>TU 事件中更新速度，包含斜向移动减速 (xFactor)。</item>
        /// <item>Combo 事件处理转向和停止。</item>
        /// </list>
        /// </summary>
        private bool State_Walking(string eventType, object eventData)
        {

             (int dx, int dz) = Controller.GetMoveInput();

            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "walking", eventType, eventData is string);
                    if (IsHeavyWeapon())
                    {
                        if (dx != 0 || dz != 0)
                            FrameAniOscillate(LF2StandardFrames.HeavyObjWalk0, LF2StandardFrames.HeavyObjWalk3);
                        else
                            Trans.SetNext(Frame.N);
                    }
                    else
                        FrameAniOscillate(LF2StandardFrames.WalkingStart, LF2StandardFrames.WalkingEnd);
                    Trans.SetWait(_FrameDataWrapper.characterData.walking_frame_rate - 1);
                    return false;

                case "TU":
                    {
                        var characterData = _FrameDataWrapper?.characterData;
                        if (characterData == null) return false;

                        var xfactor = 1 - (Dirv() != 0 ? 1 : 0) * (2f / 7f);

                        if (IsHeavyWeapon())
                        {
                            if (dx != 0) PS.vx = Dirh() * characterData.heavy_walking_speed * xfactor;
                            PS.vz = Dirv() * characterData.heavy_walking_speedz;
                        }
                        else
                        {
                            if (dx != 0) PS.vx = Dirh() * characterData.walking_speed * xfactor;
                            PS.vz = Dirv() * characterData.walking_speedz;
                        }

                        if (dx == 0 && dz == 0 && Trans.Next != LF2StandardFrames.LoopToStart)
                        {
                            Trans.SetNext(LF2StandardFrames.LoopToStart);
                            Trans.SetWait(1, 1, 2);
                        }
                    }
                    return false;

                case "state_entry":
                    Trans.SetWait(0);
                    return false;

                case "combo":
                    // 行走中的输入处理
                    string comboKey = eventData as string;

                    // 1. 处理转向
                    if (dx != 0 && dx != Dirh())
                    {
                        SwitchDir(PS.dir == "right" ? "left" : "right");
                    }

                    // 2. 停止移动时应用一次性减速 (Friction)
                    if (dx == 0 && dz == 0 && !StateMem.ContainsKey("released"))
                    {
                        StateMem["released"] = true;
                        // Step 2: 移除 unitActions.ApplyUnitFriction，摩擦力由 PS 系统处理
                    }

                    // 3. 按键处理委托给 StandingStateHandler (如跳跃、攻击逻辑相同)
                    if (!string.IsNullOrEmpty(comboKey))
                    {
                        return State_Standing("combo", comboKey);
                    }
                    return false;

                default:
                    return false;
            }
        }

        //        /// <summary>
        //        /// 奔跑状态处理器 (State 2)
        //        /// 对应 FLF character.js:403-486
        //        /// <para>注意：Frame 事件没有 break，会穿透执行 TU 逻辑 (模拟 switch fallthrough)。</para>
        //        /// </summary>
        private bool State_Running(string eventType, object eventData)
        {
            {
                switch (eventType)
                {
                    case "frame":
                        Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "running", eventType, eventData is string);
                        if (IsHeavyWeapon())
                            FrameAniOscillate(LF2StandardFrames.HeavyObjRun, LF2StandardFrames.TreeJump1);
                        else
                            FrameAniOscillate(LF2StandardFrames.RunningStart, LF2StandardFrames.RunningEnd);
                        if (_FrameDataWrapper?.characterData == null) return false;
                        Trans.SetWait(_FrameDataWrapper.characterData.running_frame_rate);
                        goto case "TU";

                    case "TU":
                        {
                            var xfactor = 1 - (Dirv() != 0 ? 1 : 0) * (1f / 7f);
                            var characterData = _FrameDataWrapper?.characterData;
                            if (characterData == null) return false;

                            if (IsHeavyWeapon())
                            {
                                PS.vx = xfactor * Dirh() * characterData.heavy_running_speed;
                                PS.vz = Dirv() * characterData.heavy_running_speedz;
                            }
                            else
                            {
                                PS.vx = xfactor * Dirh() * characterData.running_speed;
                                PS.vz = Dirv() * characterData.running_speedz;
                            }
                        }
                        return false;

                    case "combo":
                        string comboKey = eventData as string;

                        if (!string.IsNullOrEmpty(comboKey))
                        {
                            // 1. 反向输入检测 -> 停止奔跑 (急停)
                            if (comboKey == "left" || comboKey == "right" || comboKey == "left-left" || comboKey == "right-right")
                            {
                                string inputDir = comboKey.Split('-')[0];

                                if (inputDir != PS.dir)
                                {
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.StopRunning, "反向输入 -> 急停");
                                    if (IsHeavyWeapon())
                                        TransitionToFrame(LF2StandardFrames.TreeJump2, 10);
                                    else
                                        TransitionToFrame(LF2StandardFrames.StopRunning, 10);

                                    StateReturnFrame = 1;
                                    return true;
                                }
                            }
                            // 2. 奔跑防御
                            else if (comboKey == "def")
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", 102, "防御 -> 奔跑防御");
                                if (IsHeavyWeapon())
                                {
                                    StateReturnFrame = 1;
                                    return true;
                                }
                                TransitionToFrame(102, 10);
                                return true;
                            }
                            // 3. 奔跑跳跃 -> 冲刺 (Dash)
                            else if (comboKey == "jump")
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.DashForward, "跳跃 -> 冲刺");
                                if (IsHeavyWeapon())
                                {
                                    if ((bool)Proper("heavy_weapon_dash"))
                                    {
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        TransitionToFrame((int)Proper("heavy_weapon_dash"), 10);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                }

                                TransitionToFrame(LF2StandardFrames.DashForward, 10);
                                StateReturnFrame = 1;
                                return true;
                            }
                            // 4. 奔跑攻击
                            else if (comboKey == "att")
                            {
                                if (_heldWeapon != null)
                                {

                                    if (_heldWeapon is LF2HeavyWeapon)
                                    {
                                        TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, 10);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        bool hasDx = Controller.IsLeft != Controller.IsRight;
                                        if (hasDx && (bool)Proper(_heldWeapon.ObjectId, "run_throw"))
                                        {
                                            TransitionToFrame(LF2StandardFrames.LightWeaponThw, 10);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                        else if ((bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                        {
                                            TransitionToFrame(LF2StandardFrames.RunWeaponAtck, 10);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                    }
                                }

                                TransitionToFrame(LF2StandardFrames.RunAttack, 10);
                                StateReturnFrame = 1;
                                return true;
                            }
                        }
                        return false;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// 攻击状态处理器 (State 3)
        /// 对应 FLF character.js:489-549
        /// 处理所有攻击动作 (普通、跳跃、冲刺攻击) 的通用逻辑
        /// </summary>
        // 状态 3: 攻击
        private bool State_Attack(string eventType, object eventData)
        {

            switch (eventType)
            {
                case "frame":
                    // 空中攻击保持逻辑: 如果攻击结束时还在空中，强制切回跳跃状态
                    var D = Frame.D;
                    if (D.next == LF2StandardFrames.LoopToStart && PS.vy < 0)
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "空中攻击结束 -> 返回跳跃");
                        Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    return false;

                case "hit_stop":
                    // 命中停顿 (卡肉) 效果
                    // 部分攻击帧 (如 86, 87, 91) 在命中时会延长当前帧时间
                    if (CurrentFrameId == 86 || CurrentFrameId == 87 || CurrentFrameId == 91)
                    {
                        Trans.IncWait(1, 10);
                        return true;
                    }
                    return false;

                case "TU":
                    var frameDataTU = Frame.D;
                    if (frameDataTU.itrs != null)
                    {
                        foreach (var itr in frameDataTU.itrs)
                        {
                            if ((itr.kind == 10 || itr.kind == 11) && Time.frameCount % 2 == 0)
                            {
                                var sceneQueryTU = Match?.SceneQuery;
                                if (sceneQueryTU == null) break;

                                var frame251 = FrameCache.GetFrameDataById(LF2StandardFrames.FluteAttackDamage);
                                if (frame251?.itrs == null || frame251.itrs.Count == 0) break;

                                var itr251 = frame251.itrs[0];
                                float spriteWTU = GetSpriteWidthPxForCollision();
                                var vol251 = PS.GetItrVolume(itr251, frame251.centerx, frame251.centery, spriteWTU);

                                List<LF2LivingObject> allObjects = new List<LF2LivingObject>();
                                Match.GetAllLivingObjects(allObjects);

                                for (int i = 0; i < allObjects.Count; i++)
                                {
                                    var target = allObjects[i];
                                    if (target == this) continue;
                                    if (target.PS == null) continue;

                                    float zDiff = Mathf.Abs(target.PS.z - PS.z);
                                    float xDiff = Mathf.Abs(target.PS.x - PS.x);
                                    // 椭圆范围检测（150像素半径）
                                    if (xDiff * xDiff + 4 * zDiff * zDiff < 150 * 150)
                                    {
                                        if (target.PS.y < 0 ||
                                            target.Type == LF2ObjectType.Character ||
                                            (target.PS.y >= 0 && UnityEngine.Random.value < 0.15f))
                                        {
                                            if (target is LF2Character targetChar && targetChar.GetHeldWeapon() != null)
                                            {
                                                targetChar.DropWeapon(0, 0);
                                            }

                                            if (target.Hit(itr251, this, new Vector3(PS.x, PS.y, PS.z), vol251))
                                            {
                                                target.Attacked(itr251, this);
                                                target.ItrArestUpdate(itr251);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 跳跃状态处理器 (State 4)
        /// 对应 FLF js:552-602
        /// </summary>
        private bool State_Jump(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    // 标记 frameTU，用于 TU 事件中处理起跳物理
                    SetStateMemory("frameTU", true);

                    // 攻击锁定: 防止连续跳跃攻击 (Jump Attack 后有 2 帧锁定)
                    if (Frame.PN == LF2StandardFrames.JumpAttack ||
                        Frame.PN == LF2StandardFrames.JumpAttack + 1)
                    {
                        SetStateMemory("attlock", 2);
                    }
                    return false;

                case "TU":
                    // 1. 起跳速度设置 (Frame 211 -> 212)
                    if (GetStateMemory("frameTU", out bool frameTUValue) && frameTUValue)
                    {
                        SetStateMemory("frameTU", false);
                        if (Frame.N == LF2StandardFrames.JumpingAir &&
                            Frame.PN == LF2StandardFrames.JumpingUp)
                        {
                            var (dx, dz) = Controller.GetMoveInput();
                            var characterData = _FrameDataWrapper?.characterData;
                            if (characterData == null) return false;

                            // 应用跳跃速度
                            PS.vx = dx * (characterData.jump_distance - 1);
                            PS.vz = Dirv() * (characterData.jump_distancez - 1);
                            PS.vy = characterData.jump_height;
                        }
                    }

                    // 2. 更新攻击锁定计时器
                    if (GetStateMemory("attlock", out int lockVal))
                    {
                        StateMem["attlock"] = lockVal - 1;
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    if ((comboKey == "att" || Controller.IsAttack) && !GetStateMemory("attlock", out int attlockValue))
                    {
                        if (Frame.N == LF2StandardFrames.JumpingAir)
                        {
                            if (_heldWeapon != null)
                            {
                                bool Hasdx = Controller.IsLeft != Controller.IsRight;
                                if (Hasdx && (bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                {
                                    TransitionToFrame(LF2StandardFrames.SkyLgtWpThw, 10);
                                    // 空中投掷轻型武器
                                }
                                else if ((bool)(Proper(_heldWeapon.ObjectId, "attackable")))
                                {
                                    TransitionToFrame(LF2StandardFrames.JumpWeaponAtck, 10);
                                }
                            }
                            else
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 4, "Jump", LF2StandardFrames.JumpAttack, "跳跃攻击");
                                TransitionToFrame(LF2StandardFrames.JumpAttack, 10);
                            }

                            StateReturnFrame = 1;
                            return true;
                        }
                    }
                    return false;
            }
            return false;

        }

        /// <summary>
        /// 冲刺状态处理器 (State 5)
        /// 对应 FLF js:605-651
        /// </summary>
        private bool State_Dash(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    // 从奔跑或蹲下进入冲刺时，设置初始冲刺速度
                    if ((Frame.PN >= LF2StandardFrames.RunningStart &&
                         Frame.PN <= LF2StandardFrames.RunningEnd) ||
                        Frame.PN == LF2StandardFrames.Crouch)
                    {
                        var characterData = _FrameDataWrapper?.characterData;
                        if (characterData == null) return false;

                        PS.vx = Dirh() * (characterData.dash_distance - 1) * (Frame.N == LF2StandardFrames.DashForward ? 1 : -1);
                        PS.vz = Dirv() * (characterData.dash_distancez - 1);
                        PS.vy = characterData.dash_height;
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    // 1. 冲刺攻击
                    if (comboKey == "att" || Controller.IsAttack)
                    {
                        if ((bool)Proper("dash_backattack") || Dirh() == (PS.vx > 0 ? 1 : -1))
                        {
                            if (_heldWeapon != null && (bool)Proper(_heldWeapon.ObjectId, "attackable"))
                            {
                                TransitionToFrame(LF2StandardFrames.DashWeaponAtck, 10);
                            }
                            else
                            {
                                TransitionToFrame(LF2StandardFrames.DashAttack, 10);
                            }
                        }
                        AllowSwitchDir = false;
                        if (comboKey == "att")
                        {
                            StateReturnFrame = 1;
                            return true;
                        }
                    }
                    // 2. 冲刺转身
                    if (comboKey == "left" || comboKey == "right")
                    {
                        if (comboKey != PS.dir)
                        {
                            if (Dirh() == (PS.vx > 0 ? 1 : -1))
                            {
                                // 转身
                                if (Frame.N == LF2StandardFrames.DashForward)
                                    TransitionToFrame(LF2StandardFrames.DashForward2, 0);

                                if (Frame.N == LF2StandardFrames.DashBack)
                                    TransitionToFrame(LF2StandardFrames.DashBack2, 0);

                                SwitchDir(comboKey);
                            }
                            else
                            {
                                // 转向
                                if (Frame.N == LF2StandardFrames.DashForward2)
                                    TransitionToFrame(LF2StandardFrames.DashForward, 0);

                                if (Frame.N == LF2StandardFrames.DashBack2)
                                    TransitionToFrame(LF2StandardFrames.DashBack, 0);

                                SwitchDir(comboKey);
                            }
                            return true;
                        }

                    }
                    break;
            }
            return false;

        }

        /// <summary>
        /// 爬起状态处理器 (state = 6)
        /// 对应 FLF js:656-681
        ///
        /// 功能：处理被击飞后的爬起动作（减速下落）
        /// 关键帧：
        /// - 100: 正面爬起暂停
        /// - 101: 正面爬起结束
        /// - 108: 背面爬起暂停
        /// - 109: 背面爬起结束
        /// </summary>
        private bool State_Rowing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Log.Info("[State {0}:TU] ", eventType);

                    // ✓ 垂直速度重置（对应 FLF Line 660-664）
                    // 特定帧的重置垂直速度，使角色停止在空中
                    if (CurrentFrameId == LF2StandardFrames.Rowing ||      // 100: 正面爬起
                        CurrentFrameId == LF2StandardFrames.RowingBack)    // 108: 背面爬起
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", $"爬起暂停 Frame={CurrentFrameId}");
                        PS.vy = 0;
                    }
                    return false;

                case "frame":
                    Log.Info("[State {0}:frame] ", eventType);

                    // ✓ 等待时间设置（对应 FLF Line 667-671）
                    // 延长爬起动作的持续时间
                    if (CurrentFrameId == LF2StandardFrames.Rowing ||      // 100
                        CurrentFrameId == LF2StandardFrames.RowingBack)    // 108
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "设置爬起等待时间");
                        Trans.SetWait(LF2StateConstants.RowingWaitTime);  // 1 帧
                        return true;
                    }
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:fall_onto_ground] ", eventType);

                    // ✓ 落地处理（对应 FLF Line 674-679）
                    // 落地时的状态转换：爬起结束 → 蹲姿
                    if (CurrentFrameId == LF2StandardFrames.Rowing1 ||     // 101: 正面爬起结束
                        CurrentFrameId == LF2StandardFrames.RowingBack1)   // 109: 背面爬起结束
                    {
                        Log.Info("爬起结束落地");
                        Log.Info("TransitionTo: Frame {0} ({1})", LF2StandardFrames.Crouch, "落地 → 蹲姿");
                        TransitionToFrame(LF2StandardFrames.Crouch, 0);  // 215: 蹲姿帧
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 防御状态处理器 (state = 7)
        /// 对应 FLF js:684-695
        /// 
        /// 功能：处理防御相关逻辑
        /// 关键帧：
        /// - 110: 防御起始帧
        /// - 111: 防御成功（受击时转入）
        /// - 112: 防御被破（defend超过上限时转入）
        /// </summary>
        private bool State_Defending(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 7, "Defending", eventType);

                    // ✓ 防御等待时间延长（对应 FLF Line 688-693）
                    // 给予视觉反馈，让玩家感知到成功防御
                    if (Frame.N == LF2StandardFrames.Defend1)  // 111: 防御成功帧
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 7, "Defending", "防御成功 → 延长等待时间");
                        // 增加4帧等待时间（延长防御状态）
                        Trans.IncWait(LF2StateConstants.DefendSuccessWaitBonus);
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// 防御被破状态处理器 (state = 8)
        /// 对应 FLF js:698-719
        ///
        /// 功能：处理防御被破后的特殊移动逻辑
        /// 关键机制：修复弱击倒移动时的方向问题
        /// 问题：防御被破时，角色被击退方向可能与朝向方向相反
        /// 解决：在空中或速度不足时，强制按帧定义的dvx设置速度
        /// </summary>
        private bool State_BrokenDefend(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame_force":
                case "TU_force":
                    Log.Info("[State {0}:{1}] Event={2}", 8, "BrokenDefend", eventType);

                    // ✓ 强制移动方向修正（对应 FLF Line 702-717）

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
        /// 抓取状态处理器 (state = 9)
        /// 对应 FLF js:722-853
        ///
        /// 功能：处理抓取敌人和投掷动作
        /// 关键特性：
        /// 1. 抓取计数器系统（counter 从43递减到0）
        /// 2. 攻击次数记录（每次成功攻击延长抓取时间）
        /// 3. 位置同步（每帧更新被抓对象位置到cpoint）
        /// 4. 伤害处理（通过cpoint.injury）
        /// 5. Z轴层级控制（cover参数）
        /// 6. 方向控制（dircontrol参数）
        /// 7. 投掷/攻击/跳跃动作（taction/aaction/jaction）
        /// </summary>
        private bool State_Catching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // ✓ 初始化抓取状态（对应 FLF Line 570-573）
                    StateMem["stateTU"] = true;
                    StateMem["counter"] = 43;    // 初始计数43帧
                    StateMem["attacks"] = 0;     // 攻击次数计数
                    caught_decrease_counter = 99; // 默认值99（从反汇编确认）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "初始化抓取状态 counter=43, attacks=0, decrease=99");
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "Clear catching state");
                    Catching = null;
                    PS.zz = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // ✓ 抓取帧处理（对应 FLF Line 584-614）
                    int frameId = CurrentFrameId;
                    var D = Frame.D;

                    // ==================== 特殊帧处理 ====================

                    // 帧123（成功攻击）：增加attacks计数器，延长抓取时间3帧
                    if (frameId == 123)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧123 成功攻击 → 延长抓取时间");
                        StateMem["attacks"] = (int)StateMem["attacks"] + 1;
                        StateMem["counter"] = (int)StateMem["counter"] + 3;
                        Trans.SetWait(Trans.Wait + 1);
                        return true;
                    }

                    // 帧233/234：减少等待时间（1帧）
                    if (frameId == 233 || frameId == 234)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", $"Frame {frameId} -> decrease wait");
                        Trans.SetWait(Trans.Wait - 1);
                        return true;
                    }

                    // 帧240：Rudolf特殊变身
                    if (frameId == 240)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧240 Rudolf特殊变身");
                        // TODO: 需要实现id_update机制
                        // CallIdUpdate("rudolf_transform");
                        // return true;
                    }

                    // 位置同步
                    if (Catching is LF2Character caughtChar && D.cpoint != null)
                    {
                        // 从 cpoint.decrease 初始化计数器（仅首次）
                        if (D.cpoint.decrease > 0 && caught_decrease_counter == 99)
                        {
                            caught_decrease_counter = D.cpoint.decrease;
                        }
                        
                        int adir = (PS.dir == "right") ? 1 : -1;
                        int vdir = 1;
                        Vector3 holdpoint = new Vector3(
                            PS.x + D.cpoint.x * adir,
                            PS.y + D.cpoint.y,
                            PS.z
                        );
                        caughtChar.caught_b(holdpoint, D.cpoint, adir, vdir);
                    }

                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    if (Catching != null &&
                        caught_cpointkind() == 1 &&
                        ((LF2Character)Catching).caught_cpointkind() == 2)
                    {
                        if (StateMem.ContainsKey("stateTU") && (bool)StateMem["stateTU"])
                        {
                            StateMem["stateTU"] = false;

                            var cpoint = Frame.D.cpoint;

                            if (cpoint.injury != 0)
                            {
                                NTSDDamageCalculator.ApplyDamage(Catching, cpoint.injury);
                            }

                            int cover = cpoint.cover;
                            if (cover == 0 || cover == 10)
                            {
                                PS.zz = 1;
                            }
                            else
                            {
                                PS.zz = -1;
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

                case "post_combo":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // 抓取计数器递减（被抓者按方向键时递减）
                    if (Catching != null && Catching.Controller != null)
                    {
                        var caughtCtrl = Catching.Controller;
                        if (caughtCtrl.IsLeft || caughtCtrl.IsRight || caughtCtrl.IsUp || caughtCtrl.IsDown)
                        {
                            caught_decrease_counter--;
                            Log.Info("[State {0}:{1}] -> Branch: {2}, Counter={3}", 9, "Catching", "被抓者按键，计数器递减", caught_decrease_counter);
                            
                            if (caught_decrease_counter <= 0)
                            {
                                Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "计数器归零，释放被抓者");
                                if (Catching is LF2Character victim)
                                {
                                    victim.caught_release();
                                }
                                Catching = null;
                                TransitionToFrame(LF2StandardFrames.Standing, 22);
                                return true;
                            }
                        }
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 9, "Catching", eventType, comboKey, CurrentFrameId);

                    if (string.IsNullOrEmpty(comboKey))
                        return false;

                    var comboCpoint = Frame.D?.cpoint;
                    if (comboCpoint == null)
                        return false;

                    if (comboKey == "att")
                    {
                        // 投掷动作优先于攻击动作
                        if (comboCpoint.taction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "taction 投掷");
                            if (Catching is LF2Character throwTarget)
                            {
                                int vdir = (PS.dir == "right") ? 1 : -1;
                                throwTarget.caught_throw(comboCpoint, vdir);
                                
                                // 设置被抓者速度
                                throwTarget.PS.vx = comboCpoint.throwvx * vdir;
                                throwTarget.PS.vy = comboCpoint.throwvy;
                                throwTarget.PS.vz = comboCpoint.throwvz;
                                
                                // 设置投掷伤害（落地时生效）
                                if (comboCpoint.throwinjury != 0)
                                {
                                    throwTarget.caught_throwinjury = comboCpoint.throwinjury;
                                }
                            }
                            TransitionToFrame(comboCpoint.taction, 22);
                            Catching = null;
                            return true;
                        }
                        else if (comboCpoint.aaction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "aaction 攻击");
                            TransitionToFrame(comboCpoint.aaction, 22);
                            return true;
                        }
                        return false;
                    }

                    if (comboKey == "jump")
                    {
                        if (comboCpoint.jaction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "jaction 跳跃");
                            TransitionToFrame(comboCpoint.jaction, 22);
                            return true;
                        }
                        return false;
                    }

                    if (comboKey == "def")
                    {
                        if (comboCpoint.daction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "daction 防御");
                            TransitionToFrame(comboCpoint.daction, 22);
                            return true;
                        }
                        return false;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 被抓取状态处理器 (state = 10)
        /// 对应 FLF js:856-939
        ///
        /// 功能：处理被敌人抓取时的表现
        /// 关键特性：
        /// 1. 位置同步到抓取者的 cpoint
        /// 2. 被投掷时的速度设置（throwvx/vy/vz）
        /// 3. 方向处理（cover 参数控制）
        /// 4. 投掷伤害记录（落地时生效）
        /// 5. 抓取状态验证（双向检查）
        /// </summary>
        private bool State_BeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "Clear being-caught state");
                    // 清理被抓状态（FLF:781-787）
                    Catching = null;
                    caught_b_holdpoint = Vector3.zero;
                    caught_b_cpoint = null;
                    caught_b_adir = 0;
                    caught_b_vdir = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    // ✓ 被抓帧处理（对应 FLF Line 792-794）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "设置长时间等待（由抓取者控制）");
                    StateMem["frameTU"] = true;
                    Trans.SetWait(99);  // 长时间等待（由抓取者控制）
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    // ✓ 被抓时的处理（对应 FLF Line 803-880）

                    // ==================== 帧135时消除重力 ====================
                    // 对应 FLF Line 804-807
                    if (CurrentFrameId == 135)
                    {
                        // Step 2: 使用 PS.vy 替代 unitActions.yForce
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "帧135 暂停（消除重力）");
                        PS.vy = 0;  // 暂停
                    }

                    // NTSD 2.4 速度处理（基于反汇编 loc_404E78）
                    
                    // vx 摩擦（向0靠近）
                    if (PS.vx < 0)
                        PS.vx += 1.1f;
                    else if (PS.vx > 0)
                        PS.vx -= 1.1f;

                    // vx 边界（±30）
                    PS.vx = Mathf.Clamp(PS.vx, -30f, 30f);

                    // 位置追踪
                    if (Catching != null)
                    {
                        if (Catching.PS.x > PS.x)
                            PS.vx += 0.85f;
                        else if (Catching.PS.x < PS.x)
                            PS.vx -= 0.85f;

                        if (Catching.PS.z > PS.z + 7)
                            PS.vz += 0.3f;
                        else if (Catching.PS.z < PS.z - 7)
                            PS.vz -= 0.3f;

                        PS.vy *= 0.714f;
                    }

                    // vx 边界（±13）
                    PS.vx = Mathf.Clamp(PS.vx, -13f, 13f);
                    // vz 边界（±2）
                    PS.vz = Mathf.Clamp(PS.vz, -2f, 2f);

                    // 根据 vx 设置朝向
                    if (PS.vx > 0)
                        PS.dir = "right";
                    else if (PS.vx < 0)
                        PS.dir = "left";

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 受伤状态处理器 (state = 11)
        /// 对应 FLF js:942-960
        /// 
        /// 功能：处理硬直受伤的表现（不倒地）
        /// 关键特性：
        /// 1. 延长受伤动作的持续时间（给予视觉反馈）
        /// 2. 受伤等级帧自动返回站姿
        /// 3. 受伤等级：220/221（轻度）、222/223（中度）、224/225（重度）、226（超重）
        /// </summary>
        private bool State_Injured(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    // ✓ 增加等待时间（对应 FLF Line 946-948）
                    int currentWait = Trans.Wait;
                    Trans.SetWait(Mathf.Min(currentWait + 1, 20));  // 上限20帧
                    return false;

                case "frame":
                    // ✓ 受伤动画结束处理（对应 FLF Line 949-958）
                    // 受伤结束帧（奇数帧 221/223/225）返回站姿
                    int frameId = CurrentFrameId;
                    if (frameId == LF2StandardFrames.Injured1 ||       // 221
                        frameId == LF2StandardFrames.Injured3 ||       // 223
                        frameId == LF2StandardFrames.Injured5)         // 225
                    {
                        Trans.SetNext(LF2StandardFrames.LoopToStart);  // 999
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 倒地状态处理器 (state = 12)
        /// 对应 FLF js:963-1089
        /// 
        /// 这是一个高优先级状态，包含复杂的倒地逻辑：
        /// 1. 基于垂直速度的动画状态机（上浮/下落不同帧序列）
        /// 2. 爬起/直接躺地的判定（基于总速度）
        /// 3. 倒地无敌时间管理（fall值减少）
        /// 4. 按键起身逻辑（帧182/188 + fall<KO + hp>0）
        /// 5. 摔落伤害结算（落地时生效）
        /// 
        /// 关键帧序列：
        /// - 正面：180 → 181 → 182 → 183 / 185（上浮/下落）
        /// - 背面：186 → 187 → 188 → 189 / 191（上浮/下落）
        /// - 爬起/躺地判定在 fell_onto_ground 事件
        /// </summary>
        private bool State_Falling(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, CurrentFrameId);

                    // 倒地动画状态机（FLF:969-1020）
                    // TODO: 实现基于垂直速度的动画序列切换（需要effect.dvy系统）
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, CurrentFrameId);

                    // fall值减少和倒地无敌时间（FLF:1038-1057）
                    // TODO: 实现fall值减少系统（需要health.fall系统）
                    return false;

                case "combo":
                    // ✓ 按连招键起身（对应 FLF Line 1059-1066）
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 12, "Falling", eventType, comboKey, CurrentFrameId);

                    int frameId = CurrentFrameId;

                    // 只在帧182/188（转折点）响应
                    if (frameId == 182 || frameId == 188)
                    {
                        if (comboKey == "jump")
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", $"Frame {frameId} jump getup");
                            // TODO: 检查fall值和HP（需要health系统）
                            // if (health.fall < GC.fall.KO && health.hp > 0)

                            // 选择起身帧（正面/背面区别）
                            int rowingFrame = (frameId == 182)
                                ? LF2StandardFrames.Rowing       // 100: 正面起身
                                : LF2StandardFrames.RowingBack;  // 108: 背面起身

                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 12, "Falling", rowingFrame, "起身 → 爬起");
                            TransitionToFrame(rowingFrame, 10);

                            // TODO: 设置起身最小速度（需要velocity系统）
                            // if (PS.vx != 0) PS.vx = 5 * sign(vx)
                            // if (PS.vy == 0) PS.vy = 5 * sign(vy)
                            // if (PS.vz != 0) PS.vz = 2 * sign(vz)

                            return true;
                        }
                    }

                    // 倒地期间屏蔽所有其他输入（FLF Line 1159）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", "倒地期间屏蔽输入");
                    return true;

                case "transit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, CurrentFrameId);

                    // 爬起逻辑（FLF:1068-1082）
                    // TODO: 实现爬起逻辑（需要速度系统）
                    return false;

                case "fell_onto_ground":
                case "fall_onto_ground":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, CurrentFrameId);

                    // 落地处理（FLF:1022-1036）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", "落地 → 爬起/躺地判定");
                    // TODO: 实现爬起/躺地判定系统（需要velocity和throw_injury系统）
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 冰冻状态处理器 (state = 13)
        /// 对应 FLF js:1097-1106
        ///
        /// 功能：处理冰冻效果
        /// 关键特性：
        /// 1. 离开冰冻状态时创建冰块碎裂效果
        /// 2. 冰冻状态期间：
        ///    - 角色完全停止（无法移动、攻击、连招）
        ///    - 受到攻击时会碎裂（转入倒地状态）
        /// 3. 关键帧：200（冰冻帧）
        ///
        /// 冰冻机制（来自 hit 函数）：
        /// - effectnum = 3/30: 冰冻攻击
        /// - 未冰冻 → 转到帧200（冰冻）
        /// - 已冰冻 → 碎裂倒地（转到帧182）
        /// - 强制丢弃武器
        /// </summary>
        private bool State_Frozen(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 13, "Frozen", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 13, "Frozen", "冰冻结束 → 创建碎裂效果");
                    // 创建冰块碎裂效果（FLF:1101-1104）
                    // TODO: 实现特效系统（ID 212 碎裂效果，音效 1/066）
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 躺地状态处理器 (state = 14)
        /// 对应 FLF js:1113-1138
        ///
        /// 功能：处理落地后的躺地状态和死亡判定
        /// 关键特性：
        /// 1. state_entry：
        ///    - 重置 fall 和 bdefend 值（清空临时状态）
        ///    - 检测死亡（hp ≤ 0）并触发 die()
        ///    - NPC 死亡时启动玩家闪烁计数（30帧后销毁）
        /// 2. state_exit：
        ///    - 爬起后获得 30 帧无敌时间
        ///    - 启用透明效果提示玩家无敌状态
        ///    - 设置 super 状态（超级护甲）
        /// 3. 关键帧：
        ///    - 230: 正面躺地
        ///    - 231: 背面躺地
        ///
        /// 死亡流程（来自 generic 状态 TU 事件）：
        /// - 死亡闪烁到 4 阶段：
        ///   1. dead_blink_count = 0: 开启闪烁
        ///   2. 0 < count < 30: 持续闪烁
        ///   3. count >= 30: 关闭闪烁，隐藏精灵和影子
        ///   4. count = -1: 销毁对象
        /// </summary>
        private bool State_Lying(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 14, "Lying", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 14, "Lying", "Reset state & death check");
                    // 重置状态与死亡检测（FLF:1117-1129）
                    // TODO: 实现角色属性系统（重置 fall/bdefend、死亡判定、NPC 死亡闪烁）
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 14, "Lying", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 14, "Lying", "Getup -> 30 frames invincible");
                    // 爬起无敌效果（FLF:1130-1137）
                    // TODO: 实现特效系统（30帧无敌、闪烁效果、super 状态）
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 综合状态处理器 (state = 15)
        /// 对应 FLF js:1145-1223
        ///
        /// 功能：处理多种复杂状态（停止奔跑、蹲下、冲刺攻击、武器投掷等）
        /// 关键特性：
        /// 1. frame 事件：处理多种帧的特殊逻辑
        ///    - 帧9: 重武器停止奔跑 → 检查重武器，转到帧12
        ///    - 帧215: 蹲下 → 减少等待时间 1 帧
        ///    - 帧219: 蹲下 → 调用 id_update 或根据前帧应用冲刺力
        ///    - 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
        ///    - 帧257: Rudolf 消失帧 → 调用变身逻辑
        /// 2. combo 事件：蹲下二段跳（仅帧215）：
        ///    - 防御键 → 转到帧102（奔跑防御）
        ///    - 跳跃键 → 根据方向和速度决定跳跃类型：
        ///      * 有方向输入 → 该方向跳跃（帧213）
        ///      * 静止不动 → 垂直跳跃（帧210）
        ///      * 有速度同向 → 前冲刺（帧213）
        ///      * 有速度反向 → 后冲刺（帧214）
        ///
        /// 覆盖的状态类型：
        /// - 停止奔跑（stop_running）
        /// - 蹲下（crouch） 帧215
        /// - 蹲下2（crouch2） 帧219
        /// - 冲刺攻击（dash_attack）
        /// - 轻武器投掷（light_weapon_thw）
        /// - 重武器投掷（heavy_weapon_thw）
        /// - 重武器停止奔跑（heavy_stop_run） 帧9
        /// - 空中轻武器投掷（sky_lgt_wp_thw） 帧54
        /// - 消失（disappear） 帧257（Rudolf 特有）
        /// </summary>
        private bool State_StopRunning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 15, "Mixed", eventType);
                    // 多帧特殊处理（FLF:1149-1188）
                    int frameId = Frame.N;

                    if (frameId == LF2StandardFrames.TreeJump2)
                    {
                        if (IsHeavyWeapon())
                            Trans.SetNext(LF2StandardFrames.HeavyObjWalk0);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch)  // 215
                    {
                        // 帧215: 蹲下 → 减少等待时间
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧215 蹲下 → 减少等待时间");
                        Trans.IncWait(-1);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch2)
                    {
                        // 蹲下
                        if (!_CharacterHub._IdUpdate.TryInvokeGeneric(IdUpdateHooks.State15_Crouch))
                        {
                            switch (Frame.PN) // 上一帧编号
                            {
                                case LF2StandardFrames.Rowing5:
                                    // 划船后
                                    // 应用摩擦力
                                    CharacterMechanics.UnitFriction(PS);
                                    break;

                                case LF2StandardFrames.DashBack: // 冲刺后
                                case LF2StandardFrames.DashAttack:
                                case LF2StandardFrames.DashAttack + 1:
                                case LF2StandardFrames.DashAttack + 2: // 冲刺攻击
                                                                       // 减少等待时间
                                    Trans.IncWait(-1);
                                    break;
                            }
                        }
                    }
                    else if (frameId == LF2StandardFrames.SkyLgtWpThw3)
                    {
                        // 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
                        var D = Frame.D;
                        if (D.next == LF2StandardFrames.LoopToStart && PS.y < 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧54 空中轻武器投掷结束 → 返回跳跃");
                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.JumpingAir, "空中投掷完成");
                            Trans.SetNext(LF2StandardFrames.JumpingAir);  // 212
                        }
                    }
                    else if (frameId == LF2StandardFrames.Disappear)
                    {
                        // 帧257: Rudolf 消失帧 → 调用变身逻辑

                        // 其他特殊帧需要武器系统
                    }
                    break;
                case "combo":
                    // ✓ 蹲下二段跳（对应 FLF Line 1190-1221）
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}", 15, "Mixed", eventType, comboKey);

                    // 只在蹲下帧215响应
                    if (Frame.N == LF2StandardFrames.Crouch)  // 215
                    {
                        if (string.IsNullOrEmpty(comboKey))
                            break;

                        // 防御键 → 奔跑防御
                        if (comboKey == "def")
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下 + 防御 → 奔跑防御");
                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", 102, "奔跑防御");
                            TransitionToFrame(LF2StandardFrames.Rowing2, 10);
                            return true;
                        }

                        // 跳跃键 → 4种跳跃类型
                        if (comboKey == "jump")
                        {
                            var (dx, dz) = Controller.GetMoveInput();
                            {
                                // 1. 有方向输入 → 该方向跳跃
                                if (dx != 0)
                                {
                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", $"蹲下二段跳 dx={dx} → 方向跳跃");
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward, "方向跳跃");
                                    TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
                                    SwitchDir(dx == 1 ? DIRECTION.RIGHT : DIRECTION.LEFT);
                                }
                                else if (PS.vx == 0)
                                {
                                    Trans.IncWait(2, 10, 99);
                                    Trans.SetNext(LF2StandardFrames.Jumping, 10);
                                }
                                else if ((PS.vx > 0 ? 1 : -1) == Dirh())
                                {
                                    TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
                                }
                                else
                                {
                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下二段跳 → 前冲刺2");
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward2, "前冲刺2");
                                    // 检查角色是否静止（无水平速度）
                                    // 简化实现：直接跳到垂直跳跃
                                    TransitionToFrame(LF2StandardFrames.DashForward2, 10);  // 214
                                }
                            }

                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 受伤2状态处理器 (state = 16)
        /// 对应 FLF js:1230-1235
        ///
        /// 功能：痛苦之舞（Dance of Pain）状态
        /// 关键特性：
        /// 1. 空实现：无任何特殊逻辑
        /// 2. 所有行为由帧数据驱动（动画自动播放）
        /// 3. 可能是预留状态或由角色特定逻辑覆盖
        ///
        /// 推测用途：
        /// - 被抓取前的准备状态
        /// - 或某些特殊受击动作的状态标记
        /// - FLF 中也是空实现，表示所有逻辑都在帧数据中
        /// </summary>
        private bool State_Injured2(string eventType, object eventData)
        {
            // ✓ 无特殊事件处理（对应 FLF Line 1230-1235）
            // FLF 中也是空实现，所有逻辑由帧数据驱动
            return false;
        }

        /// <summary>
        /// 蓄力状态处理器 (state = 17)
        /// 用途：角色进行技能蓄力时的状态
        /// 出现次数：16
        /// </summary>
        private bool State_Charging(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 初始化蓄力状态
                    StateMem["chargeTime"] = 0;
                    StateMem["maxChargeTime"] = 60;  // 60帧 = 2秒（30fps）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "初始化蓄力 chargeTime=0, maxChargeTime=60");
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 蓄力状态的帧处理
                    // 蓄力等级判定和特效播放由外部系统处理
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 蓄力时间更新
                    if (StateMem.ContainsKey("chargeTime"))
                    {
                        int chargeTime = (int)StateMem["chargeTime"];
                        int maxChargeTime = (int)StateMem["maxChargeTime"];

                        // 递增蓄力时间，但不超过上限
                        if (chargeTime < maxChargeTime)
                        {
                            StateMem["chargeTime"] = chargeTime + 1;
                            if (chargeTime % 10 == 0)  // 每10帧输出一次日志
                            {
                                Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中 chargeTime={chargeTime}/{maxChargeTime}");
                            }
                        }
                    }
                    return false;

                case "combo":
                    // ✓ 蓄力中的输入处理
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 17, "Charging", eventType, comboKey, CurrentFrameId);

                    // 任何按键输入都会结束蓄力状态
                    // 具体的技能释放逻辑由技能系统处理
                    if (!string.IsNullOrEmpty(comboKey))
                    {
                        int chargeTime = StateMem.ContainsKey("chargeTime") ? (int)StateMem["chargeTime"] : 0;
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中断 按键={comboKey}, 蓄力时间={chargeTime}");
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 17, "Charging", LF2StandardFrames.Standing, "蓄力中断");
                        // 返回站立状态，让技能系统接管
                        TransitionToFrame(LF2StandardFrames.Standing, 10);
                        return true;
                    }
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 清理蓄力状态内存
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "Clear charging state mem");
                    StateMem.Remove("chargeTime");
                    StateMem.Remove("maxChargeTime");
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 燃烧状态处理器 (state = 18)
        /// 对应 FLF js:1242-1258
        ///
        /// 功能：处理燃烧效果
        /// 关键特性：
        /// 1. frame 事件：每帧创建燃烧特效（持续燃烧视觉效果）
        /// 2. fall_onto_ground 事件：落地瞬间创建燃烧效果
        /// 3. fell_onto_ground 事件：复用 State 12 的落地逻辑（弹起/躺地判定）
        /// 4. 关键帧：203-206（燃烧落地帧）
        ///
        /// 燃烧机制（来自 hit 函数）：
        /// - effectnum = 2/20/21/22/23: 火焰攻击
        /// - 转到帧203（燃烧状态）
        /// - 高级火焰（21/22/23）弱化投掷判定器
        /// - 燃烧状态防止急火击中（effectnum=20/21）
        /// - 燃烧状态21/22不会伤害队友
        /// </summary>
        private bool State_Burning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "持续燃烧 → 每帧创建燃烧特效");
                    // 每帧创建燃烧特效（FLF:1246-1249）
                    // TODO: 实现特效系统（ID 302，持续模式）
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧落地 → 创建落地燃烧特效");
                    // 落地时创建燃烧特效（FLF:1250-1252）
                    // TODO: 实现特效系统（ID 302，一次性模式）
                    return false;

                case "fell_onto_ground":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧倒地 → 复用State 12落地逻辑");
                    // 复用State 12落地逻辑（FLF:1253-1256）
                    return State_Falling("fell_onto_ground", eventData);

                default:
                    return false;
            }
        }

        #endregion


        // ========== 核心生命周期（对应 FLF livingobject/character）==========

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
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            float blockedMoveScale = Match?.SceneQuery?.TestBlockingXZ(this, PS.vx, PS.vz) == true ? 0.1f : 1f;

            bool hasStageBounds = false;
            LF2StageBoundsPx stageBoundsPx = default;
            var boundsProvider = NTSD.LevelEditor.BoundaryWallManager.Instance;
            if (boundsProvider != null && boundsProvider.TryGetStageBoundsPx(out stageBoundsPx))
            {
                hasStageBounds = true;
            }

            var ctx = new CharacterMechanicsContext(
                PS,
                Frame.D,
                GetSpriteWidthPxForCollision(),
                hasStageBounds,
                stageBoundsPx,
                _mass,
                 NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                blockedMoveScale,
                _cachedIsPointWalkable
            );

            var result = _mech.Step(ctx);

            if (_debugCollisionLog && result.boundaryMode != BoundaryResolveMode.None)
            {
                Tools.Log.Info("[Boundary] ResolveMode={0}", result.boundaryMode);
            }

            // ground plane（Unity X/Y）写回
            _CharacterHub.transform.position = new Vector3(
                result.groundPlanePos.x,
                result.groundPlanePos.y,
                _CharacterHub.transform.position.z
            );

            //// 视觉高度偏移（Unity local Y）
            _CharacterHub._ModeTrans.localPosition = _baseLocalPosition + new Vector3(0f, result.visualYOffset, 0f);

            _CharacterHub.SetGrounding(_CharacterHub.transform.position.y, result.grounded);

        }

        /// <summary>
        /// 武器点更新
        /// 对应 FLF wpoint()
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 方向控制 ==========

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
        public void HoldWeapon(ILF2Object weapon)
        {
            _heldWeapon = weapon;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public ILF2Object GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 重型武器
        /// </summary>
        /// <returns></returns>
        public bool IsHeavyWeapon() 
        {
            if(_heldWeapon == null)
                return false;

            return _heldWeapon is LF2HeavyWeapon;
        }

        /// <summary>
        /// 丢弃武器（对应 FLF character.prototype.drop_weapon）
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            if (_heldWeapon is LF2LightWeapon lightWeapon)
            {
                lightWeapon.Drop(dvx, dvy);
            }
            else if (_heldWeapon is LF2HeavyWeapon heavyWeapon)
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

       
        // ========== 抓取系统（Catching System）==========

        /// <summary>
        /// 每帧被抓取者接收位置同步数据（对应 FLF character.js:2475-2481 caught_b）
        /// 由抓取者在 state 9 的 TU 事件中每帧调用
        /// </summary>
        public void caught_b(Vector3 holdpoint, CatchPoint cpoint, int adir, int vdir)
        {
            caught_b_holdpoint = holdpoint;
            caught_b_cpoint = cpoint;
            caught_b_adir = adir;
            caught_b_vdir = vdir;
        }

        /// <summary>
        /// 返回当前帧的 cpoint.kind（对应 FLF character.js:2486-2489 caught_cpointkind）
        /// 用于抓取者/被抓者双向验证 cpoint 匹配
        /// </summary>
        public int caught_cpointkind()
        {
            var cpoint = CurrentFrame?.cpoint;
            return cpoint?.kind ?? 0;
        }

        /// <summary>
        /// 检查被抓时是否可受伤（对应 FLF character.js:2494-2501 caught_cpointhurtable）
        /// </summary>
        public bool caught_cpointhurtable()
        {
            var cpoint = CurrentFrame?.cpoint;
            if (cpoint == null) return true;
            return cpoint.hurtable != 0;
        }

        /// <summary>
        /// 被投掷时的处理（对应 FLF character.js:2506-2514 caught_throw）
        /// 由抓取者在投掷时调用
        /// </summary>
        public void caught_throw(CatchPoint cpoint, int vdir)
        {
            if (cpoint.vaction != 0)
            {
                TransitionToFrame(cpoint.vaction, 22);
            }
            else
            {
                TransitionToFrame(LF2StandardFrames.JumpingAir, 22);
            }
            caught_throwz = vdir;
        }

        /// <summary>
        /// 被释放时的处理（对应 FLF character.js:2519-2527 caught_release）
        /// 由抓取者在释放时调用
        /// </summary>
        public void caught_release()
        {
            Catching = null;
            TransitionToFrame(181, 22);
            Effect.Dvx = 3;
            Effect.Dvy = -3;
            Effect.TimeIn = -1;
            Effect.TimeOut = 0;
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
    }
}
