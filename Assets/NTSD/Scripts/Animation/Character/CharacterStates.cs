using BeatEmUpTemplate2D;
using GAS.Runtime;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Tools;
using Sirenix.OdinInspector.Editor.Validation;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{
    //    /// <summary>
    //    /// 角色状态处理器 - 对应 LF2 原版代码中 character.js 的 states 对象
    //    /// 
    //    /// <para>设计理念：</para>
    //    /// <list type="bullet">
    //    /// <item>1. 单例模式 (Singleton)：所有角色共享同一个 CharacterStates 实例，避免重复创建逻辑类。</item>
    //    /// <item>2. 核心逻辑：只包含 LF2 通用的基础状态逻辑（State 0-19）以及部分 NTSD 扩展状态。</item>
    //    /// <item>3. 事件驱动：通过 HandleStateEvent 分发 'frame', 'TU', 'combo' 等事件。</item>
    //    /// <item>4. 数据驱动：复杂的帧数据（Frame Data）通过配置读取，而非硬编码。</item>
    //    /// </list>
    //    /// 
    //    /// <para>对应关系：</para>
    //    /// <list type="bullet">
    //    /// <item>LF2: character.js 的 states 对象</item>
    //    /// <item>Unity: CharacterStates 类</item>
    //    /// </list>
    //    /// </summary>
    public class CharacterStates : MMSingleton<CharacterStates>
    {
        // 状态处理器字典，Key 为状态 ID (State ID)，Value 为对应的处理函数委托
        private Dictionary<int, StateHandler> stateHandlers;

        // 状态切换方向字典，记录哪些状态允许自由切换朝向 (Left/Right)
        private Dictionary<int, bool> States_Switch_Dir;

        // 通用状态处理器 (Generic Handler)，处理所有状态共有的逻辑（如受击、通用连招等）
        private StateHandler genericHandler;

        //        /// <summary>
        //        /// StateUpdate 写回容器 - 对齐 FLF state_update 的返回策略
        //        /// 必须使用 class（引用类型），否则 handler 无法写回
        //        /// </summary>
        //        public sealed class StateUpdateData
        //        {
        //            public bool handled;
        //            public int? frameId;

        //            public void Reset()
        //            {
        //                handled = false;
        //                frameId = null;
        //            }
        //        }

        //        // 缓存的 StateUpdateData 实例，避免每次调用 StateUpdate 时分配
        //        private readonly StateUpdateData _genericUpdateData = new StateUpdateData();
        //        private readonly StateUpdateData _specificUpdateData = new StateUpdateData();


        /// <summary>
        /// 状态处理器委托定义
        /// 对应 LF2 的 states[0] = function(event, K) { } 签名
        /// </summary>
        /// <param name="character">当前执行逻辑的角色实例</param>
        /// <param name="eventType">事件类型 (如: "frame", "combo", "TU", "transit", "hit")</param>
        /// <param name="eventData">事件附带的数据 (如按键键名、碰撞信息等)</param>
        /// <returns>返回 true 表示事件已被处理，不需要继续传递</returns>
        public delegate bool StateHandler(ILF2LivingObject character, string eventType, object eventData);

        //        private CharacterStates()
        //        {
        //            stateHandlers = new Dictionary<int, StateHandler>(20);
        //            RegisterDefaultHandlers();
        //            RegisterMixedStateHandlers();
        //        }

        //        /// <summary>
        //        /// 注册默认的状态处理器
        //        /// 涵盖 LF2 character.js 的基础状态 (0-19) 以及 NTSD 的扩展状态
        //        /// </summary>
        //        private void RegisterDefaultHandlers()
        //        {
        //            // 注册通用处理器：任何状态下都会执行的保底逻辑
        //            genericHandler = GenericStateHandler;

        //            // === 基础状态 (0-19) ===
        //            stateHandlers[LF2States.Standing] = StandingStateHandler;         // 0: 站立 (Standing)
        //            stateHandlers[LF2States.Walking] = WalkingStateHandler;           // 1: 行走 (Walking)
        //            stateHandlers[LF2States.Running] = RunningStateHandler;           // 2: 奔跑 (Running)
        //            stateHandlers[LF2States.Attack] = AttackStateHandler;             // 3: 攻击 (Attack)
        //            stateHandlers[LF2States.Jump] = JumpStateHandler;                 // 4: 跳跃 (Jump)
        //            stateHandlers[LF2States.Dash] = DashStateHandler;                 // 5: 冲刺 (Dash)
        //            stateHandlers[LF2States.Rowing] = RowingStateHandler;             // 6: 划船/受身 (Rowing) - 被击飞后的空中姿态
        //            stateHandlers[LF2States.Defending] = DefendingStateHandler;       // 7: 防御 (Defending)
        //            stateHandlers[LF2States.BrokenDefend] = BrokenDefendStateHandler; // 8: 防御崩坏 (Broken Defend)
        //            stateHandlers[LF2States.Catching] = CatchingStateHandler;         // 9: 抓人 (Catching)
        //            stateHandlers[LF2States.BeingCaught] = BeingCaughtStateHandler;   // 10: 被抓 (Being Caught)
        //            stateHandlers[LF2States.Injured] = InjuredStateHandler;           // 11: 受伤 (Injured) - 轻微受伤
        //            stateHandlers[LF2States.Falling] = FallingStateHandler;           // 12: 跌倒/浮空 (Falling) - 包含倒地判定的复杂状态
        //            stateHandlers[LF2States.Frozen] = FrozenStateHandler;             // 13: 冰冻 (Frozen)
        //            stateHandlers[LF2States.Lying] = LyingStateHandler;               // 14: 倒地 (Lying) - 躺在地上
        //            stateHandlers[LF2States.StopRunning] = MixedStateHandler;         // 15: 混合状态 (Mixed) - 包含停止奔跑、蹲下等多种逻辑
        //            stateHandlers[LF2States.Injured2] = Injured2StateHandler;         // 16: 受伤2 (Injured 2) - 也就是 Dance of Pain
        //            stateHandlers[LF2States.Charging] = ChargingStateHandler;         // 17: 聚气 (Charging) - NTSD 扩展
        //            stateHandlers[LF2States.Burning] = BurningStateHandler;           // 18: 燃烧 (Burning)
        //            stateHandlers[LF2States.FirenSpecific] = FirenSpecificStateHandler; // 19: Firen特殊状态 (Firen Specific)

        //            // === 注册 NTSD 扩展状态处理器 ===
        //            RegisterNTSDStateHandlers();

        //            // === 注册技能状态处理器 (301-999) ===
        //            RegisterAbilityStateHandlers();
        //        }

        /// <summary>
        /// 配置各状态是否允许通过输入改变朝向
        /// </summary>
        private void RegisterMixedStateHandlers()
        {
            States_Switch_Dir = new Dictionary<int, bool>(16);
            States_Switch_Dir[LF2States.Standing] = true;        // 站立：允许转身
            States_Switch_Dir[LF2States.Walking] = true;         // 行走：允许转身
            States_Switch_Dir[LF2States.Running] = false;        // 奔跑：锁定方向
            States_Switch_Dir[LF2States.Attack] = false;         // 攻击：锁定方向
            States_Switch_Dir[LF2States.Jump] = true;            // 跳跃：允许空中转身 (LF2机制)
            States_Switch_Dir[LF2States.Dash] = false;           // 冲刺：锁定方向
            States_Switch_Dir[LF2States.Rowing] = false;         // 划船：锁定方向
            States_Switch_Dir[LF2States.Defending] = true;       // 防御：允许转身
            States_Switch_Dir[LF2States.BrokenDefend] = false;   // 防破：锁定方向
            States_Switch_Dir[LF2States.Catching] = false;       // 抓人：锁定方向
            States_Switch_Dir[LF2States.BeingCaught] = false;    // 被抓：锁定方向
            States_Switch_Dir[LF2States.Injured] = false;        // 受伤：锁定方向
            States_Switch_Dir[LF2States.Falling] = false;        // 跌倒：锁定方向
            States_Switch_Dir[LF2States.Frozen] = false;         // 冰冻：锁定方向
            States_Switch_Dir[LF2States.Lying] = false;          // 倒地：锁定方向
            States_Switch_Dir[LF2States.StopRunning] = false;    // 停跑：锁定方向
            States_Switch_Dir[LF2States.Injured2] = false;       // 受伤2：锁定方向
        }

        //        /// <summary>
        //        /// 注册 NTSD 项目特有的扩展状态 (武器、飞行道具、特效等)
        //        /// </summary>
        //        private void RegisterNTSDStateHandlers()
        //        {
        //            // === 武器状态 (1000-1999) ===
        //            // 所有武器状态共用一个处理器
        //            stateHandlers[LF2States.WeaponInSky] = WeaponStateHandler;          // 1000: 武器在空中
        //            stateHandlers[LF2States.WeaponOnHand] = WeaponStateHandler;         // 1001: 武器在手中
        //            stateHandlers[LF2States.WeaponThrowing] = WeaponStateHandler;       // 1002: 武器投掷中
        //            stateHandlers[LF2States.WeaponJustOnGround] = WeaponStateHandler;   // 1003: 武器刚落地
        //            stateHandlers[LF2States.WeaponOnGround] = WeaponStateHandler;       // 1004: 武器在地面

        //            // === 投射物状态 (3000-3003) ===
        //            // 所有投射物状态共用一个处理器
        //            stateHandlers[LF2States.ProjectileFlying] = ProjectileStateHandler;     // 3000: 投射物飞行
        //            stateHandlers[LF2States.ProjectileHiting] = ProjectileStateHandler;     // 3001: 投射物命中中
        //            stateHandlers[LF2States.ProjectileHit] = ProjectileStateHandler;        // 3002: 投射物命中后(消失)
        //            stateHandlers[LF2States.ProjectileTeleport] = ProjectileStateHandler;   // 3003: 投射物瞬移

        //            // === 对象/特殊状态 (3005-3006) ===
        //            // 這些是高频使用的状态，用于分身、特效等
        //            stateHandlers[LF2States.ObjectFlying] = ObjectFlyingStateHandler;       // 3005: 对象飞行/替身术 (最高频: 407次调用)
        //            stateHandlers[LF2States.ObjectExpanding] = ObjectExpandingStateHandler; // 3006: 对象扩散/膨胀 (152次)

        //            // === 特效状态 (9000+) ===
        //            stateHandlers[LF2States.EffectPlaying] = EffectPlayingStateHandler;     // 9997: 特效播放
        //            stateHandlers[LF2States.SpecialEffect] = SpecialEffectStateHandler;     // 30005: 特殊效果
        //        }

        //        /// <summary>
        //        /// 注册技能状态处理器 (State 301-999)
        //        /// 目前所有技能共用一个 AbilityStateHandler，因为技能通常由数据驱动
        //        /// </summary>
        //        private void RegisterAbilityStateHandlers()
        //        {
        //            // 循环注册所有技能段的状态 ID
        //            //for (int state = LF2States.AbilityStart; state <= LF2States.AbilityEnd; state++)
        //            //{
        //            //    stateHandlers[state] = AbilityStateHandler;
        //            //}
        //        }

        public bool GetStatesSwitchDir(int state)
        {
            if (state < 0 || state >= States_Switch_Dir.Count)
                return false;

            return States_Switch_Dir[state];
        }

        /// <summary>
        /// 核心方法：处理状态事件
        /// 对应 LF2 源码中的 combo_update() 流程
        /// 
        /// <para>优先级逻辑：</para>
        /// <list type="number">
        /// <item>1. 尝试使用当前状态特定的处理器 (Specific Handler)</item>
        /// <item>2. 如果特定处理器未处理 (返回 false)，则使用通用处理器 (Generic Handler)</item>
        /// <item>3. 如果通用处理器也未处理，则返回 false</item>
        /// </list>
        /// </summary>
        /// <param name="character">目标角色</param>
        /// <param name="eventType">事件名称</param>
        /// <param name="eventData">事件数据</param>
        /// <returns>是否已处理</returns>
        public bool HandleStateEvent(ILF2LivingObject character, string eventType, object eventData = null, bool isComboUpdate = false)
        {
            if (character == null || character.Frame.D == null) return false;

            int currentState = character.Frame.D.state;
            bool handled = false;
            bool handled1 = false;
            StateHandler specificHandler = null;

            if (!isComboUpdate)
            {
                if (genericHandler != null)
                    handled = genericHandler(character, eventType, eventData);

                if (stateHandlers.TryGetValue(currentState, out specificHandler))
                {
                    handled1 = specificHandler(character, eventType, eventData);
                }

                return handled || handled1;
            }
            else
            {
                // Combo Update 特例：先调用 specific，再调用 generic
                // 1. 优先调用当前状态的特定处理逻辑
                if (stateHandlers.TryGetValue(currentState, out specificHandler))
                {
                    handled = specificHandler(character, eventType, eventData);
                }

                // 2. 通用处理：
                // - 默认：只有在 specific 未处理时才调用 generic（原有优先级规则）
                // - 对齐 FLF：transit 阶段必须始终执行 mech.dynamics()（不应被状态处理器“拦截”）
                if (!handled && genericHandler != null)
                {
                    handled = genericHandler(character, eventType, eventData);
                }
            }



            return handled;
        }

        //        // ==================== 通用状态处理器 (Generic Handler) ====================

        //        /// <summary>
        //        /// 通用状态处理器
        //        /// 对应 LF2 源码的 states.generic
        //        /// 处理所有状态共享的逻辑，如物理更新、输入缓冲、全局受击判定等
        //        /// </summary>
        //        private bool GenericStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "hit":
        //                    // 💥 处理受击逻辑 (对应 FLF character.js hit处理)
        //                    return HandleGenericHit(character, eventData);

        //                case "frame":
        //                    // 🖼️ 每帧执行的通用逻辑 (MP/HP恢复, OPoint生成等) (对应 FLF character.js:14-52)
        //                    return HandleGenericFrame(character);

        //                case "TU":
        //                    // ⏱️ 时间单元(Time Unit)更新 (状态机, buff更新, 物理重置) (对应 FLF character.js:54-183)
        //                    return HandleGenericTU(character);

        //                case "transit":
        //                    // 🚀 动态物理更新 (摩擦力, 位置更新) (对应 FLF character.js:185-190)
        //                    return HandleGenericTransit(character);

        //                case "combo":
        //                    // 🎮 通用输入处理 (多键连招映射, 方向键处理) (对应 FLF character.js:191-215)
        //                    return HandleGenericCombo(character, eventData as string);

        //                case "post_combo":
        //                    // 🛑 连招后处理 (清理缓存等) (对应 FLF character.js:217-220)
        //                    // TODO: 实现 pre_interaction() - 预处理交互 (武器拾取, 对象交互)
        //                    return false;

        //                case "state_exit":
        //                    // 🚪 状态退出清理 (清理连招缓冲) (对应 FLF character.js:221-228)
        //                    return HandleGenericStateExit(character, eventData as string);

        //                case "get_next_frame":
        //                    // 获取下一帧，默认返回 null 让系统使用配置文件中的 next
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        // ==================== 物理与受击常量定义 (参照 FLF global.js) ====================
        //        private const int FLF_DEFAULT_FALL_VALUE = 20; // 默认击倒值
        //        private const int FLF_FALL_KO = 60;            // 击倒值超过此数则被击飞 (KO)
        //        private const int FLF_DEFEND_BREAK_LIMIT = 40; // 防御崩坏阈值

        //        /// <summary>
        //        /// 通用受击处理
        //        /// 处理角色被攻击时的反应：扣血、增加击倒值(Fall)、进入受伤或击飞状态
        //        /// </summary>
        //        private bool HandleGenericHit(ILF2LivingObject target, object eventData)
        //        {
        //            if (target == null || target.PS == null || target.Frame.D == null) return false;
        //            if (eventData is not LF2CollisionSystem.HitEvent evt) return false;
        //            if (evt.attacker == null || evt.attacker.PS == null) return false;
        //            if (evt.itr == null) return false;

        //            // FLF 规则: State 14 (倒地) 状态下忽略受击 (无敌)
        //            if (target.Frame.D.state == LF2States.Lying) return false;

        //            InteractionArea itr = evt.itr;
        //            ILF2LivingObject attacker = evt.attacker;

        //            // Phase 1: 仅处理通用的攻击类型 (0:普通, 4:防御无视?, 9, 15, 16)
        //            if (!(itr.kind == 0 || itr.kind == 4 || itr.kind == 9 || itr.kind == 15 || itr.kind == 16))
        //                return false;

        //            // 判断攻击是否来自正面
        //            bool attackedFromFront = (attacker.PS.x > target.PS.x) == (target.PS.dir == "right");

        //            // 计算攻击者方向 (用于击退计算)
        //            // FLF: attdir = (att.PS.vx===0 ? att.dirh() : sign(att.PS.vx))
        //            int attackerDir;
        //            if (Mathf.Abs(attacker.PS.vx) > 0.0001f) attackerDir = attacker.PS.vx > 0 ? 1 : -1;
        //            else attackerDir = attacker.PS.dir == "right" ? 1 : -1;

        //            int compen = Mathf.Approximately(target.PS.y, 0f) ? 1 : 0;
        //            float efDvx = itr.dvx != 0 ? attackerDir * (itr.dvx - compen) : 0f;
        //            float efDvy = itr.dvy != 0 ? itr.dvy : 0f;

        //            // === 防御逻辑分支 ===
        //            // 如果处于防御状态且从正面被攻击 -> 判定为防御成功或防御崩坏
        //            if (target.Frame.D.state == LF2States.Defending && attackedFromFront)
        //            {
        //                if (itr.bdefend != 0)
        //                    target.HitCounters.AddBdefend(Mathf.Abs(itr.bdefend));

        //                // 简单的防御后退与吸收逻辑 (参照 FLF GC.defend.absorb)
        //                if (!Mathf.Approximately(efDvx, 0f))
        //                {
        //                    float abs = Mathf.Abs(efDvx);
        //                    float absorb = abs >= 15f ? 5f : 0f;
        //                    efDvx += (efDvx > 0f ? -1f : 1f) * absorb;
        //                }
        //                efDvy = 0f;

        //                // 判断是否防御崩坏 (Broken Defend)
        //                int defendFrame = target.HitCounters.Bdefend > FLF_DEFEND_BREAK_LIMIT
        //                    ? LF2StandardFrames.DefendBroken
        //                    : LF2StandardFrames.Defend1;

        //                target.Trans.Frame(defendFrame, 20);
        //                return true;
        //            }

        //            // === 受伤累积与状态选择 ===
        //            int addFall = itr.fall != 0 ? itr.fall : FLF_DEFAULT_FALL_VALUE;
        //            target.HitCounters.AddFall(addFall);

        //            // 判定是否进入击飞/跌倒状态 (Falling)
        //            // 条件：在空中、有垂直速度、或 Fall 值超过 KO 阈值
        //            if (target.PS.y < 0f || target.PS.vy < 0f || target.HitCounters.Fall > FLF_FALL_KO)
        //            {
        //                target.HitCounters.ResetFall();
        //                // FLF 规则: 进入 Falling 时重置 vy
        //                target.PS.vy = 0f;
        //                // 根据攻击方向选择向前跌倒还是向后跌倒
        //                target.Trans.Frame(attackedFromFront ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack, 21);
        //                return true;
        //            }

        //            // 否则进入普通受伤状态 (Injured)，根据 Fall 值选择不同程度的受伤帧
        //            int fall = target.HitCounters.Fall;
        //            int injuredFrame;
        //            if (fall > 0 && fall <= 20) injuredFrame = LF2StandardFrames.Injured;
        //            else if (fall > 20 && fall <= 30) injuredFrame = LF2StandardFrames.Injured2;
        //            else if (fall > 30 && fall <= 40) injuredFrame = LF2StandardFrames.Injured4;
        //            else if (fall > 40 && fall <= 60) injuredFrame = LF2StandardFrames.Injured6;
        //            else
        //            {
        //                // Fall 过高，强制跌倒
        //                target.HitCounters.ResetFall();
        //                target.PS.vy = 0f;
        //                target.Trans.Frame(attackedFromFront ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack, 21);
        //                return true;
        //            }

        //            target.Trans.Frame(injuredFrame, 20);
        //            return true;
        //        }

        //        /// <summary>
        //        /// 通用帧逻辑 (Frame)
        //        /// 对应 FLF character.js:14-52
        //        /// </summary>
        //        private bool HandleGenericFrame(ILF2LivingObject character)
        //        {
        //            var D = character.Frame.D;
        //            if (D == null) return false;

        //            // 1. MP 消耗与 HP 扣除逻辑
        //            // 对应 FLF character.js:15-48
        //            if (D.mp != 0)
        //            {
        //                // TODO: 待属性系统完善后启用
        //            }

        //            // 2. OPoint (Object Point) 处理 - 用于生成武器、投射物
        //            // 对应 FLF character.js:52
        //            if (D.opoint != null)
        //            {
        //                if (D.opoint.oid == 5) 
        //                {
        //                    float number_of_character = Mathf.Floor(Mathf.Abs(D.opoint.facing) / 10);
        //                    for (int i = 0; i < number_of_character; i++) 
        //                    {

        //                    }
        //                }

        //                // TODO: 待对象生成系统完善后启用
        //            }

        //            return false;
        //        }

        //        // ==================== StateUpdate 通道（对齐 FLF state_update）====================

        //        /// <summary>
        //        /// StateUpdate - 对齐 FLF livingobject.state_update 的顺序与返回策略
        //        /// 通用入口，支持任意 eventType
        //        /// 顺序：先 generic，再 specific；返回策略：res1 || res2（generic 优先）
        //        /// </summary>
        //        private StateUpdateData StateUpdate(ILF2LivingObject character, string eventType, object eventData = null)
        //        {
        //            // 重置缓存的容器
        //            _genericUpdateData.Reset();
        //            _specificUpdateData.Reset();

        //            if (character == null || character.Frame.D == null)
        //                return _genericUpdateData; // 返回空结果

        //            // 1) generic 先执行
        //            InvokeGenericStateUpdate(character, eventType, _genericUpdateData);

        //            // 2) specific 再执行
        //            InvokeSpecificStateUpdate(character, eventType, _specificUpdateData);

        //            // 3) 返回策略对齐 FLF：res1 || res2（generic 的 frameId 优先）
        //            // 合并到 _genericUpdateData 作为最终结果
        //            if (!_genericUpdateData.frameId.HasValue && _specificUpdateData.frameId.HasValue)
        //                _genericUpdateData.frameId = _specificUpdateData.frameId;
        //            _genericUpdateData.handled = _genericUpdateData.handled || _specificUpdateData.handled;

        //            return _genericUpdateData;
        //        }

        //        /// <summary>
        //        /// Generic 层的 state_update 处理
        //        /// 当前默认不覆盖，由调用方的默认分支处理
        //        /// </summary>
        //        private void InvokeGenericStateUpdate(ILF2LivingObject character, string eventType, StateUpdateData data)
        //        {
        //            // Generic 层目前不覆盖逻辑，保持 data 为默认值
        //            // 未来可在此添加 generic 层对特定 eventType 的处理
        //        }

        //        /// <summary>
        //        /// Specific 层的 state_update 处理
        //        /// 调用当前 state 的 handler，允许写回 frameId 或 handled
        //        /// </summary>
        //        private void InvokeSpecificStateUpdate(ILF2LivingObject character, string eventType, StateUpdateData data)
        //        {
        //            int currentState = character.Frame.D.state;
        //            if (!stateHandlers.TryGetValue(currentState, out var handler))
        //                return;

        //            // handler 可以通过 data 写回 frameId/handled
        //            bool handled = handler(character, eventType, data);
        //            data.handled = data.handled || handled;
        //        }

        //        /// <summary>
        //        /// 通用时间单元更新 (TU)
        //        /// 对应 FLF character.js:54-183
        //        /// 负责处理周期性的逻辑，如 Buff 消失、状态恢复、物理 Tick
        //        /// </summary>
        //        private bool HandleGenericTU(ILF2LivingObject character)
        //        {
        //            // 1. 消失效果状态机 (FLF:56-82)
        //            // TODO: 需要特效系统

        //            // 2. 死亡闪烁效果 (FLF:84-102)
        //            // TODO: 需要特效系统

        //            // 3. 状态更新与物理处理 (FLF:104-141)
        //            // TODO: post_interaction, 落地检测等

        //            // 4. 生命值自然恢复 (每12帧) (FLF:145-149)
        //            // TODO: 需要 HP 系统

        //            // 5. 治疗效果处理 (每8帧) (FLF:152-160)
        //            // TODO: 需要效果系统

        //            // 6. 魔法值自然恢复 (每?帧) (FLF:163-167)
        //            // TODO: 需要 MP 系统

        //            // 7. 状态恢复 (FLF:170-171)
        //            if (character.PS.y == 0 && character.PS.vy == 0 && character.Frame.N == LF2StandardFrames.JumpingAir && character.Frame.PN != LF2StandardFrames.JumpingUp)
        //            {
        //                character.TransitionToFrame(999);
        //            }
        //            // A) fell_onto_ground（PS.y==0 && PS.vy>0）- 对齐 FLF character.js:115-126
        //            else if (character.PS.y == 0 && character.PS.vy > 0)
        //            {
        //                var res = StateUpdate(character, "fell_onto_ground");
        //                if (res.frameId.HasValue)
        //                {
        //                    character.TransitionToFrame(res.frameId.Value, 15);
        //                }
        //                else if (!res.handled)
        //                {
        //                    // 默认分支：PS.vy=0 + 落地瞬间摩擦
        //                    character.PS.vy = 0;
        //                    float fricX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, character.PS.vx);
        //                    float fricZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, character.PS.vz);
        //                    CharacterMechanics.LinearFriction(character.PS, fricX, fricZ);
        //                }
        //            }
        //            // B) fall_onto_ground（PS.y+PS.vy>=0 && PS.vy>0）- 对齐 FLF character.js:127-141
        //            else if ((character.PS.y + character.PS.vy) >= 0 && character.PS.vy > 0)
        //            {
        //                var res = StateUpdate(character, "fall_onto_ground");
        //                if (res.frameId.HasValue)
        //                {
        //                    character.TransitionToFrame(res.frameId.Value, 15);
        //                }
        //                else if (!res.handled)
        //                {
        //                    // 默认分支：Frozen 不动；JumpingAir→Crouch；其它→Crouch2
        //                    if (character.Frame.D.state == LF2States.Frozen)
        //                    {
        //                        // 冰冻状态不处理
        //                    }
        //                    else if (character.Frame.N == LF2StandardFrames.JumpingAir)
        //                    {
        //                        character.TransitionToFrame(LF2StandardFrames.Crouch, 15);
        //                    }
        //                    else
        //                    {
        //                        character.TransitionToFrame(LF2StandardFrames.Crouch2, 15);
        //                    }
        //                }
        //            }
        //                // TODO: 需要 fall/bdefend 系统 (如防御值随时间恢复)

        //                // 8. 连击缓冲系统 (FLF:174-182)
        //                // TODO: 需要连击缓冲系统

        //                character.ComboBuffer?.ReduceTimeout();

        //            return false;
        //        }

        //        /// <summary>
        //        /// 通用物理转换 (Transit)
        //        /// 对应 FLF character.js:185-190
        //        /// </summary>
        //        private bool HandleGenericTransit(ILF2LivingObject character)
        //        {
        //            // 对齐 FLF character.js:185-190
        //            // case 'transit':
        //            //   $.mech.dynamics()
        //            //   $.wpoint()
        //            //
        //            // 在本项目中，dynamics 的实现位于 LF2CharacterAnimator.ApplyDynamics()，
        //            // 并通过 Transit_DynamicsAndWPoint() 统一执行，避免 Rigidbody 路径。
        //            character.Transit_DynamicsAndWPoint();
        //            return true;
        //        }

        //        /// <summary>
        //        /// 通用状态退出清理
        //        /// 对应 FLF character.js:221-228
        //        /// </summary>
        //        private bool HandleGenericStateExit(ILF2LivingObject character, string combo)
        //        {
        //            // 清除双击指令缓存 (防止状态切换后误触发跑动)
        //            // 对应 FLF:222-227
        //            switch (combo)
        //            {
        //                case "left-left":
        //                case "right-right":
        //                    character.ComboBuffer?.ReduceTimeout();
        //                    break;
        //            }
        //            return false;
        //        }

        //        /// <summary>
        //        /// 通用连招处理器 (Generic Combo)
        //        /// 对应 FLF character.js line 191-215 的 generic case 'combo'
        //        /// 
        //        /// <para>工作流程：</para>
        //        /// <list type="number">
        //        /// <item>1. 处理单键输入 (硬编码)：如 left, right, jump 等基础移动逻辑。</item>
        //        /// <item>2. 处理多键连招：通过 Tag 映射 (如 D>A 映射到 Tag "Fa")。</item>
        //        /// <item>3. 调用 id_update：允许角色脚本覆盖通用逻辑 (id_update('generic_combo'))。</item>
        //        /// <item>4. 处理方向切换：如输入 D>A 强制角色转向右侧。</item>
        //        /// <item>5. 执行跳转：根据 Frame Data 中的 Tag 跳转到目标帧。</item>
        //        /// </list>
        //        /// </summary>
        //        private bool HandleGenericCombo(ILF2LivingObject character, string combo)
        //        {
        //            if (string.IsNullOrEmpty(combo))
        //                return false;

        //            // === 1. 处理单键连招 (硬编码逻辑) ===
        //            // 对应 FLF character.js:239-338 State 0 的 case 'combo' 部分逻辑
        //            switch (combo)
        //            {
        //                case "left":
        //                case "right":
        //                case "left-left":
        //                case "right-right":
        //                    // 这些基础移动指令通常由 Standing/Walking 状态自行处理，通用逻辑直接返回
        //                    return false;

        //                default:
        //                    // 特殊处理: Rudolf 的 DJA 变身
        //                    if (combo == "DJA")
        //                    {
        //                        // TODO: Rudolf 变身检查逻辑
        //                        // if (character.transform_character != null && character.transform_character.is_rudolf_transform) { ... }
        //                    }
        //                    break;
        //            }

        //            // === 2. 处理多键连招 (Tag 映射机制) ===
        //            // 对应 FLF character.js:191-215

        //            // Step 1: 将输入序列 (如 "D>A") 映射为内部 Tag (如 "Fa")
        //            string tag = GetComboTag(combo);
        //            if (string.IsNullOrEmpty(tag))
        //                return false;

        //            // Step 2: 检查当前帧的数据中是否定义了该 Tag 的跳转目标 (hit_Fa: 123)
        //            int targetFrame = character.Frame.D.Hit[tag];
        //            if (targetFrame <= 0)
        //                return false;

        //            // Step 3: 尝试调用角色特定逻辑 (id_update) 进行拦截
        //            // 对应 FLF: if (!$.id_update('generic_combo', K, tag))
        //            //if (character._Character != null && character._Character._IdUpdate != null)
        //            //{
        //            //    if (character._Character._IdUpdate.TryInvokeGenericCombo(combo, tag, targetFrame))
        //            //    {
        //            //        return true;  // 角色特定逻辑已处理，不再执行默认跳转
        //            //    }
        //            //}

        //            // Step 4: 处理连招的方向要求 (如 D>A 要求必须朝右)
        //            string dir = GetComboDirection(combo);
        //            if (!string.IsNullOrEmpty(dir))
        //            {
        //                character.SetDirectionByString(dir);
        //            }

        //            // Step 5: 执行跳转
        //            character.TransitionToFrame(targetFrame, LF2StateConstants.GenericComboWait);
        //            return true;
        //        }

        //        /// <summary>
        //        /// 将组合键映射为帧数据 Tag
        //        /// 对应 FLF global.js:42-58 的 G.combo_tag
        //        /// 
        //        /// <para>映射规则示例：</para>
        //        /// <list type="bullet">
        //        /// <item>D>A (防+前+攻) -> "Fa"</item>
        //        /// <item>D^J (防+上+跳) -> "Uj"</item>
        //        /// </list>
        //        /// </summary>
        //        private string GetComboTag(string combo)
        //        {
        //            switch (combo)
        //            {
        //                // === 基础单键 ===
        //                case "att": return "a";      // 攻击 -> hit_a
        //                case "jump": return "j";      // 跳跃 -> hit_j
        //                case "def": return "d";      // 防御 -> hit_d

        //                // === 三键连招 (防+向+攻/跳) ===
        //                case "D<A": return "Fa";     // 防前攻 -> hit_Fa
        //                case "D>A": return "Fa";
        //                case "DvA": return "Da";     // 防下攻 -> hit_Da
        //                case "D^A": return "Ua";     // 防上攻 -> hit_Ua

        //                case "D<J": return "Fj";     // 防前跳 -> hit_Fj
        //                case "D>J": return "Fj";
        //                case "DvJ": return "Dj";     // 防下跳 -> hit_Dj
        //                case "D^J": return "Uj";     // 防上跳 -> hit_Uj

        //                // === 四键连招 ===
        //                case "D<AJ": return "Fj";     // 防前攻跳 -> hit_Fj
        //                case "D>AJ": return "Fj";
        //                case "DJA": return "ja";     // 防跳攻 -> hit_ja

        //                default: return null;
        //            }
        //        }

        //        /// <summary>
        //        /// 获取连招强制要求的方向
        //        /// 对应 FLF global.js:59-67 的 G.combo_dir
        //        /// </summary>
        //        private string GetComboDirection(string combo)
        //        {
        //            switch (combo)
        //            {
        //                case "D<A":
        //                case "D<J":
        //                case "D<AJ":
        //                    return "left";

        //                case "D>A":
        //                case "D>J":
        //                case "D>AJ":
        //                    return "right";

        //                default:
        //                    return null; // 无方向强制要求
        //            }
        //        }

        //        // ==================== 辅助方法 ====================

        //        /// <summary>
        //        /// 获取移动输入 (对应 FLF character.js:345-350)
        //        /// 用于 WalkingStateHandler 在函数开头计算 dx, dz
        //        /// </summary>
        //        /// <returns>(dx, dz) - 方向输入值 (-1/0/1)</returns>
        //        private (int dx, int dz) GetMoveInput(ILF2LivingObject character)
        //        {
        //            int dx = 0, dz = 0;
        //            if (character._Character?._CharacterInput != null)
        //            {
        //                Vector2 moveInput = character._Character._CharacterInput.CurrentMoveInput;
        //                if (moveInput.x < -0.1f) dx -= 1;
        //                if (moveInput.x > 0.1f) dx += 1;
        //                if (moveInput.y < -0.1f) dz -= 1;
        //                if (moveInput.y > 0.1f) dz += 1;
        //            }
        //            return (dx, dz);
        //        }


        //        // ==================== 基础状态处理器 (0-15) ====================

        //        /// <summary>
        //        /// 站立状态处理器 (State 0)
        //        /// 对应 FLF character.js:244-338
        //        /// 处理角色的静止、基础按键响应
        //        /// </summary>
        //        private bool StandingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}] Event={1}", "Standing", eventType);
        //                    // 检查是否持有重型武器，若是则切换到持重物站立帧 (Frame 12)
        //                    // TODO: 需要武器系统
        //                    return false;

        //                case "combo":
        //                    // 站立状态的输入响应 (对应 FLF Line 250-338)
        //                    string comboKey = eventData as string;
        //                    Log.Info("[State {0}] Event={1}", "Standing", eventType);
        //                    // === 方向键与跳跃键处理 (FLF Line 253-272) ===
        //                    switch (comboKey)
        //                    {
        //                        case "left":
        //                        case "right":
        //                        case "up":
        //                        case "down":
        //                        case "jump":
        //                        case "":
        //                        case null:
        //                            // 检查是否有实际方向输入
        //                            {
        //                                bool hasDx = character._Character._CharacterInput.IsLeft != character._Character._CharacterInput.IsRight;
        //                                bool hasDz = character._Character._CharacterInput.IsTop != character._Character._CharacterInput.IsDown;
        //                                if (hasDx || hasDz)
        //                                {
        //                                    // 除非按下的是跳跃键，否则切换到行走状态
        //                                    if (comboKey != "jump")
        //                                    {
        //                                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.WalkingStart, "方向键按下 -> 行走");
        //                                        character.TransitionToFrame(LF2StandardFrames.WalkingStart, 5);
        //                                    }

        //                                    // 设置速度 (对应 FLF Line 265-270)
        //                                    // 注意: FLF 在 Standing 状态不使用 xFactor (斜向减速)，只有 Walking 状态使用
        //                                    var characterData = character._FrameDataWrapper?.characterData;
        //                                    if (characterData == null) return false;
        //                                    var PS = character.PS;

        //                                    if (hasDx) PS.vx = PS.Dirh() * characterData.walking_speed;
        //                                    PS.vz = character._Character._CharacterInput.Dirv * characterData.walking_speedz;

        //                                }
        //                            }
        //                            break;
        //                    }

        //                    // === 动作键处理 ===
        //                    switch (comboKey)
        //                    {
        //                        case "left-left":
        //                        case "right-right":
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.RunningStart, "双击方向键 -> 奔跑");
        //                            character.TransitionToFrame(LF2StandardFrames.RunningStart, LF2StateConstants.ComboTransitionWait);
        //                            return true;

        //                        case "def":
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Defend, "防御键 -> 防御");
        //                            character.TransitionToFrame(LF2StandardFrames.Defend, LF2StateConstants.ComboTransitionWait);
        //                            return true;

        //                        case "jump":
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Jumping, "跳跃键 -> 跳跃");
        //                            character.TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
        //                            return true;

        //                        case "att":
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Punch, "攻击键 -> 挥拳");
        //                            // TODO: 武器逻辑 (轻重武器判定、投掷判定)

        //                            // 随机选择挥拳动画 (60 或 65)
        //                            int punchFrame = Random.value < 0.5f ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
        //                            character.TransitionToFrame(punchFrame, LF2StateConstants.ComboTransitionWait);
        //                            return true;
        //                    }

        //                    break;
        //            }
        //            return false;
        //        }

        //        /// <summary>
        //        /// 行走状态处理器 (State 1)
        //        /// 对应 FLF character.js:341-400
        //        /// 
        //        /// <para>特性：</para>
        //        /// <list type="bullet">
        //        /// <item>在函数开头计算输入 (dx, dz)。</item>
        //        /// <item>TU 事件中更新速度，包含斜向移动减速 (xFactor)。</item>
        //        /// <item>Combo 事件处理转向和停止。</item>
        //        /// </list>
        //        /// </summary>
        //        private bool WalkingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            var (dx, dz) = GetMoveInput(character);

        //            switch (eventType)
        //            {
        //                case "frame":
        //                    // 循环播放行走动画 (5 -> 8 -> 5)
        //                    character.FrameAniOscillate(LF2StandardFrames.WalkingStart, LF2StandardFrames.WalkingEnd);
        //                    if (character._FrameDataWrapper?.characterData == null) return false;
        //                    character.Trans.SetWait(character._FrameDataWrapper.characterData.walking_frame_rate - 1);
        //                    return true;

        //                case "TU":
        //                    // 移动速度应用 (对应 FLF Line 367-382)
        //                    if (character._Character != null)
        //                    {
        //                        var characterData = character._FrameDataWrapper?.characterData;
        //                        if (characterData == null) return false;
        //                        var characterInput = character._Character._CharacterInput;
        //                        var PS = character.PS;

        //                        // 斜向移动时的速度补偿系数 (约 0.7)
        //                        var xfactor = 1 - (characterInput.Dirv != 0 ? 1 : 0) * (2f / 7f);

        //                        if (dx != 0) PS.vx = PS.Dirh() * characterData.walking_speed * xfactor;
        //                        PS.vz = characterInput.Dirv * characterData.walking_speedz;

        //                        // 如果完全停止且动画不在循环起点，重置回循环起点
        //                        if (dx == 0 && dz == 0 && character.Trans.Next != LF2StandardFrames.LoopToStart)
        //                        {
        //                            character.Trans.SetNext(LF2StandardFrames.LoopToStart);
        //                            character.Trans.SetWait(1, 1, 2);
        //                        }
        //                    }
        //                    return false;

        //                case "state_entry":
        //                    character.Trans.SetWait(0);
        //                    return false;

        //                case "combo":
        //                    // 行走中的输入处理
        //                    string comboKey = eventData as string;

        //                    // 1. 处理转向
        //                    if (dx != 0 && dx != character.PS.Dirh())
        //                    {
        //                        DIRECTION newDir = character.PS.dir == "right" ? DIRECTION.LEFT : DIRECTION.RIGHT;
        //                        character.SetDirection(newDir);
        //                    }

        //                    // 2. 停止移动时应用一次性减速 (Friction)
        //                    if (dx == 0 && dz == 0 && !character.StateMem.ContainsKey("released"))
        //                    {
        //                        character.StateMem["released"] = true;
        //                        // Step 2: 移除 unitActions.ApplyUnitFriction，摩擦力由 PS 系统处理
        //                    }

        //                    // 3. 按键处理委托给 StandingStateHandler (如跳跃、攻击逻辑相同)
        //                    if (!string.IsNullOrEmpty(comboKey))
        //                    {
        //                        return StandingStateHandler(character, "combo", comboKey);
        //                    }
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 奔跑状态处理器 (State 2)
        //        /// 对应 FLF character.js:403-486
        //        /// <para>注意：Frame 事件没有 break，会穿透执行 TU 逻辑 (模拟 switch fallthrough)。</para>
        //        /// </summary>
        //        private bool RunningStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    // 循环播放奔跑动画
        //                    character.FrameAniOscillate(LF2StandardFrames.RunningStart, LF2StandardFrames.RunningEnd);
        //                    if (character._FrameDataWrapper?.characterData == null) return false;
        //                    character.Trans.SetWait(character._FrameDataWrapper.characterData.running_frame_rate);
        //                    // ⚠️ 注意: 模拟 switch fallthrough，继续执行 TU
        //                    goto case "TU";

        //                case "TU":
        //                    // 维持奔跑速度
        //                    if (character._Character != null)
        //                    {
        //                        var characterInput = character._Character._CharacterInput;
        //                        var xfactor = 1 - (characterInput.Dirv != 0 ? 1 : 0) * (1f / 7f);
        //                        var characterData = character._FrameDataWrapper?.characterData;
        //                        if (characterData == null) return false;
        //                        var PS = character.PS;

        //                        PS.vx = xfactor * PS.Dirh() * characterData.running_speed;
        //                        PS.vz = characterInput.Dirv * characterData.running_speedz;
        //                    }
        //                    return false;

        //                case "combo":
        //                    string comboKey = eventData as string;

        //                    if (!string.IsNullOrEmpty(comboKey))
        //                    {
        //                        // 1. 反向输入检测 -> 停止奔跑 (急停)
        //                        if (comboKey == "left" || comboKey == "right" || comboKey == "left-left" || comboKey == "right-right")
        //                        {
        //                            string inputDir = comboKey.Split('-')[0];

        //                            if (inputDir != character.PS.dir)
        //                            {
        //                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.StopRunning, "反向输入 -> 急停");
        //                                character.TransitionToFrame(LF2StandardFrames.StopRunning, 10);
        //                                return true;
        //                            }
        //                        }
        //                        // 2. 奔跑防御
        //                        else if (comboKey == "def")
        //                        {
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", 102, "防御 -> 奔跑防御");
        //                            character.TransitionToFrame(102, 10);
        //                            return true;
        //                        }
        //                        // 3. 奔跑跳跃 -> 冲刺 (Dash)
        //                        else if (comboKey == "jump")
        //                        {
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.DashForward, "跳跃 -> 冲刺");
        //                            character.TransitionToFrame(LF2StandardFrames.DashForward, 10);
        //                            return true;
        //                        }
        //                        // 4. 奔跑攻击
        //                        else if (comboKey == "att")
        //                        {
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.RunAttack, "攻击 -> 奔跑攻击");
        //                            character.TransitionToFrame(LF2StandardFrames.RunAttack, 10);
        //                            return true;
        //                        }
        //                    }
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 攻击状态处理器 (State 3)
        //        /// 对应 FLF character.js:489-549
        //        /// 处理所有攻击动作 (普通、跳跃、冲刺攻击) 的通用逻辑
        //        /// </summary>
        //        private bool AttackStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    // 空中攻击保持逻辑: 如果攻击结束时还在空中，强制切回跳跃状态
        //                    var D = character.Frame.D;
        //                    if (D.next == LF2StandardFrames.LoopToStart && character.PS.vy < 0)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "空中攻击结束 -> 返回跳跃");
        //                        character.Trans.SetNext(LF2StandardFrames.JumpingAir);
        //                    }
        //                    return false;

        //                case "hit_stop":
        //                    // 命中停顿 (卡肉) 效果
        //                    // 部分攻击帧 (如 86, 87, 91) 在命中时会延长当前帧时间
        //                    if (character.CurrentFrameId == 86 || character.CurrentFrameId == 87 || character.CurrentFrameId == 91)
        //                    {
        //                        character.Trans.IncWait(1, 10);
        //                        return true;
        //                    }
        //                    return false;

        //                case "TU":
        //                    // 范围攻击/骰子攻击 (Kind 10/11) 的特殊检测逻辑
        //                    // 对应 FLF:511-547
        //                    var frameDataTU = character.Frame.D;
        //                    if (frameDataTU.itrs != null)
        //                    {
        //                        foreach (var itr in frameDataTU.itrs)
        //                        {
        //                            if ((itr.kind == 10 || itr.kind == 11) && Time.frameCount % 2 == 0)
        //                            {
        //                                // TODO: 实现场景查询系统，对范围内敌人应用 Frame 251 的 ITR
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 跳跃状态处理器 (State 4)
        //        /// 对应 FLF character.js:552-602
        //        /// </summary>
        //        private bool JumpStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    // 标记 frameTU，用于 TU 事件中处理起跳物理
        //                    character.SetStateMemory("frameTU", true);

        //                    // 攻击锁定: 防止连续跳跃攻击 (Jump Attack 后有 2 帧锁定)
        //                    if (character.Frame.PN == LF2StandardFrames.JumpAttack ||
        //                        character.Frame.PN == LF2StandardFrames.JumpAttack + 1)
        //                    {
        //                        character.SetStateMemory("attlock", 2);
        //                    }
        //                    return false;

        //                case "TU":
        //                    // 1. 起跳速度设置 (Frame 211 -> 212)
        //                    if (character.GetStateMemory("frameTU", out bool frameTUValue) && frameTUValue)
        //                    {
        //                        character.SetStateMemory("frameTU", false);
        //                        if (character.Frame.N == LF2StandardFrames.JumpingAir &&
        //                            character.Frame.PN == LF2StandardFrames.JumpingUp)
        //                        {
        //                            var (dx, dz) = GetMoveInput(character);
        //                            var characterData = character._FrameDataWrapper?.characterData;
        //                            if (characterData == null) return false;
        //                            var characterInput = character._Character._CharacterInput;
        //                            var PS = character.PS;

        //                            // 应用跳跃速度
        //                            PS.vx = dx * (characterData.jump_distance - 1);
        //                            PS.vz = characterInput.Dirv * (characterData.jump_distancez - 1);
        //                            PS.vy = characterData.jump_height;
        //                        }
        //                    }

        //                    // 2. 更新攻击锁定计时器
        //                    if (character.GetStateMemory("attlock",out int lockVal))
        //                    {
        //                        character.StateMem["attlock"] = lockVal - 1;
        //                    }
        //                    return false;

        //                case "combo":
        //                    string comboKey = eventData as string;
        //                    // 跳跃攻击逻辑
        //                    if ((comboKey == "att" || character._Character._CharacterInput.IsAtt) && !character.GetStateMemory("attlock",out int attlockValue))
        //                    {
        //                        if (character.Frame.N == LF2StandardFrames.JumpingAir)
        //                        {
        //                            if (false)
        //                            {
        //                                //持有武器的对象
        //                                bool Hasdx = character._Character._CharacterInput.IsLeft != character._Character._CharacterInput.IsRight;
        //                                if (Hasdx)
        //                                {
        //                                    // 空中投掷轻型武器
        //                                }
        //                                else if (false)
        //                                {
        //                                    // 轻型武器攻击
        //                                }
        //                            }
        //                            else 
        //                            {
        //                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 4, "Jump", LF2StandardFrames.JumpAttack, "跳跃攻击");
        //                                character.TransitionToFrame(LF2StandardFrames.JumpAttack, 10);
        //                                return true;
        //                            }
        //                        }
        //                    }
        //                    return false;
        //            }
        //            return false;
        //        }

        //        /// <summary>
        //        /// 冲刺状态处理器 (State 5)
        //        /// 对应 FLF character.js:605-651
        //        /// </summary>
        //        private bool DashStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_entry":
        //                    // 从奔跑或蹲下进入冲刺时，设置初始冲刺速度
        //                    if ((character.Frame.PN >= LF2StandardFrames.RunningStart &&
        //                         character.Frame.PN <= LF2StandardFrames.RunningEnd) ||
        //                        character.Frame.PN == LF2StandardFrames.Crouch)
        //                    {
        //                        var PS = character.PS;
        //                        var characterData = character._FrameDataWrapper?.characterData;
        //                        if (characterData == null) return false;
        //                        var characterInput = character._Character._CharacterInput;

        //                        PS.vx = PS.Dirh() * (characterData.dash_distance - 1) * (character.Frame.N == LF2StandardFrames.DashForward ? 1 : -1);
        //                        PS.vz = characterInput.Dirv * (characterData.dash_distancez - 1);
        //                        PS.vy = characterData.dash_height;
        //                    }
        //                    return false;

        //                case "combo":
        //                    string comboKey = eventData as string;
        //                    // 1. 冲刺攻击
        //                    if (comboKey == "att" || character._Character._CharacterInput.IsAtt)
        //                    {
        //                        // 背后攻击
        //                        if (false || character.PS.Dirh() == (character.PS.vx > 0 ? 1 : -1)) // 非背向冲刺
        //                        {
        //                            //持有武器的对象
        //                            if (false) 
        //                            {
        //                                character.TransitionToFrame(LF2StandardFrames.DashWeaponAtck, 10);
        //                            }
        //                            else
        //                            {
        //                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 5, "Dash", LF2StandardFrames.DashAttack, "冲刺攻击");
        //                                character.TransitionToFrame(LF2StandardFrames.DashAttack, 10);
        //                            }   
        //                        }
        //                        character._AllowSwitchDir = false;
        //                        if (comboKey == "att")
        //                            return true;
        //                    }
        //                    // 2. 冲刺转身
        //                    if (comboKey == "left" || comboKey == "right")
        //                    {
        //                        if (comboKey != character.PS.dir)
        //                        {
        //                            if (character.PS.Dirh() == (character.PS.vx > 0 ? 1 : -1))
        //                            {
        //                                // 转身
        //                                if (character.Frame.N == LF2StandardFrames.DashForward)
        //                                    character.TransitionToFrame(LF2StandardFrames.DashForward2, 0);

        //                                if (character.Frame.N == LF2StandardFrames.DashBack)
        //                                    character.TransitionToFrame(LF2StandardFrames.DashBack2, 0);

        //                                character.SetDirectionByString(comboKey);
        //                            }
        //                            else
        //                            {
        //                                // 转向
        //                                if (character.Frame.N == LF2StandardFrames.DashForward2)
        //                                    character.TransitionToFrame(LF2StandardFrames.DashForward, 0);

        //                                if (character.Frame.N == LF2StandardFrames.DashBack2)
        //                                    character.TransitionToFrame(LF2StandardFrames.DashBack, 0);

        //                                character.SetDirectionByString(comboKey);
        //                            }
        //                            return true;
        //                        }

        //                    }
        //                    break;
        //            }
        //            return false;
        //        }

        //        /// <summary>
        //        /// 防御状态处理器 (state = 7)
        //        /// 对应 FLF character.js:684-695
        //        /// 
        //        /// 功能：处理防御相关逻辑
        //        /// 关键帧：
        //        /// - 110: 防御起始帧
        //        /// - 111: 防御成功（受击时转入）
        //        /// - 112: 防御被破（defend超过上限时转入）
        //        /// </summary>
        //        private bool DefendingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}", 7, "Defending", eventType);

        //                    // ✓ 防御等待时间延长（对应 FLF Line 688-693）
        //                    // 给予视觉反馈，让玩家感知到成功防御
        //                    if (character.Frame.N == LF2StandardFrames.Defend1)  // 111: 防御成功帧
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 7, "Defending", "防御成功 → 延长等待时间");
        //                        // 增加4帧等待时间（延长防御状态）
        //                        character.Trans.IncWait(LF2StateConstants.DefendSuccessWaitBonus);
        //                    }
        //                    break;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// 受伤状态处理器 (state = 11)
        //        /// 对应 FLF character.js:942-960
        //        /// 
        //        /// 功能：处理硬直受伤的表现（不倒地）
        //        /// 关键特性：
        //        /// 1. 延长受伤动作的持续时间（给予视觉反馈）
        //        /// 2. 受伤等级帧自动返回站姿
        //        /// 3. 受伤等级：220/221（轻度）、222/223（中度）、224/225（重度）、226（超重）
        //        /// </summary>
        //        private bool InjuredStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_entry":
        //                    // ✓ 增加等待时间（对应 FLF Line 946-948）
        //                    int currentWait = character.Trans.Wait;
        //                    character.Trans.SetWait(Mathf.Min(currentWait + 1, 20));  // 上限20帧
        //                    return false;

        //                case "frame":
        //                    // ✓ 受伤动画结束处理（对应 FLF Line 949-958）
        //                    // 受伤结束帧（奇数帧 221/223/225）返回站姿
        //                    int frameId = character.CurrentFrameId;
        //                    if (frameId == LF2StandardFrames.Injured1 ||       // 221
        //                        frameId == LF2StandardFrames.Injured3 ||       // 223
        //                        frameId == LF2StandardFrames.Injured5)         // 225
        //                    {
        //                        character.Trans.SetNext(LF2StandardFrames.LoopToStart);  // 999
        //                        return true;
        //                    }

        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 倒地状态处理器 (state = 12)
        //        /// 对应 FLF character.js:963-1089
        //        /// 
        //        /// 这是一个高优先级状态，包含复杂的倒地逻辑：
        //        /// 1. 基于垂直速度的动画状态机（上浮/下落不同帧序列）
        //        /// 2. 爬起/直接躺地的判定（基于总速度）
        //        /// 3. 倒地无敌时间管理（fall值减少）
        //        /// 4. 按键起身逻辑（帧182/188 + fall<KO + hp>0）
        //        /// 5. 摔落伤害结算（落地时生效）
        //        /// 
        //        /// 关键帧序列：
        //        /// - 正面：180 → 181 → 182 → 183 / 185（上浮/下落）
        //        /// - 背面：186 → 187 → 188 → 189 / 191（上浮/下落）
        //        /// - 爬起/躺地判定在 fell_onto_ground 事件
        //        /// </summary>
        //        private bool FallingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, character.CurrentFrameId);

        //                    // 倒地动画状态机（FLF:969-1020）
        //                    // TODO: 实现基于垂直速度的动画序列切换（需要effect.dvy系统）
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, character.CurrentFrameId);

        //                    // fall值减少和倒地无敌时间（FLF:1038-1057）
        //                    // TODO: 实现fall值减少系统（需要health.fall系统）
        //                    return false;

        //                case "combo":
        //                    // ✓ 按连招键起身（对应 FLF Line 1059-1066）
        //                    string comboKey = eventData as string;
        //                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 12, "Falling", eventType, comboKey, character.CurrentFrameId);

        //                    int frameId = character.CurrentFrameId;

        //                    // 只在帧182/188（转折点）响应
        //                    if (frameId == 182 || frameId == 188)
        //                    {
        //                        if (comboKey == "jump")
        //                        {
        //                            Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", $"Frame {frameId} jump getup");
        //                            // TODO: 检查fall值和HP（需要health系统）
        //                            // if (character.health.fall < GC.fall.KO && character.health.hp > 0)

        //                            // 选择起身帧（正面/背面区别）
        //                            int rowingFrame = (frameId == 182)
        //                                ? LF2StandardFrames.Rowing       // 100: 正面起身
        //                                : LF2StandardFrames.RowingBack;  // 108: 背面起身

        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 12, "Falling", rowingFrame, "起身 → 爬起");
        //                            character.TransitionToFrame(rowingFrame, 10);

        //                            // TODO: 设置起身最小速度（需要velocity系统）
        //                            // if (character.PS.vx != 0) character.PS.vx = 5 * sign(vx)
        //                            // if (character.PS.vy == 0) character.PS.vy = 5 * sign(vy)
        //                            // if (character.PS.vz != 0) character.PS.vz = 2 * sign(vz)

        //                            return true;
        //                        }
        //                    }

        //                    // 倒地期间屏蔽所有其他输入（FLF Line 1159）
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", "倒地期间屏蔽输入");
        //                    return true;

        //                case "transit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, character.CurrentFrameId);

        //                    // 爬起逻辑（FLF:1068-1082）
        //                    // TODO: 实现爬起逻辑（需要速度系统）
        //                    return false;

        //                case "fell_onto_ground":
        //                case "fall_onto_ground":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 12, "Falling", eventType, character.CurrentFrameId);

        //                    // 落地处理（FLF:1022-1036）
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 12, "Falling", "落地 → 爬起/躺地判定");
        //                    // TODO: 实现爬起/躺地判定系统（需要velocity和throw_injury系统）
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        // ==================== 技能状态处理器 (301-999) ====================

        //        /// <summary>
        //        /// 技能状态通用处理器 (state 301-999)
        //        /// 自动处理 ITR、OPoint、next 等帧数据
        //        /// 所有技能共用这个处理器
        //        /// </summary>
        //        private bool AbilityStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    var D = character.Frame.D;

        //                    // ✓ TODO: 处理 ITR（伤害判定） - 自动处理
        //                    if (D.itrs != null && D.itrs.Count > 0)
        //                    {
        //                        // 遍历所有 ITR 区域
        //                        foreach (var itr in D.itrs)
        //                        {
        //                            // ITRProcessor.ProcessITR(itr, character);
        //                        }
        //                    }

        //                    // ✓ TODO: 处理 OPoint（投掷物/召唤） - 自动处理
        //                    if (D.opoint != null)
        //                    {
        //                        // OPointProcessor.ProcessOPoint(D.opoint, character);
        //                    }

        //                    // ✓ 检查技能是否结束 (next = 999)
        //                    if (D.next == LF2StandardFrames.LoopToStart)
        //                    {
        //                        // 自动返回站立状态
        //                        character.PlayFrameByID(LF2StandardFrames.Standing);
        //                        return true;
        //                    }
        //                    break;

        //                case "state_entry":
        //                    // 技能进入时的逻辑（如播放音效、粒子效果）
        //                    // TODO: 添加技能特效
        //                    break;

        //                case "state_exit":
        //                    // 技能退出时的逻辑（如清理效果）
        //                    break;
        //            }

        //            return false;
        //        }

        //        // ==================== 缺失的基础状态处理器 (5, 8-10, 13-16, 18-19) ====================

        //        /// <summary>
        //        /// 爬起状态处理器 (state = 6)
        //        /// 对应 FLF character.js:656-681
        //        ///
        //        /// 功能：处理被击飞后的爬起动作（减速下落）
        //        /// 关键帧：
        //        /// - 100: 正面爬起暂停
        //        /// - 101: 正面爬起结束
        //        /// - 108: 背面爬起暂停
        //        /// - 109: 背面爬起结束
        //        /// </summary>
        //        private bool RowingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "TU":
        //                    Log.Info("[State {0}:TU] ", eventType);

        //                    // ✓ 垂直速度重置（对应 FLF Line 660-664）
        //                    // 特定帧的重置垂直速度，使角色停止在空中
        //                    if (character.CurrentFrameId == LF2StandardFrames.Rowing ||      // 100: 正面爬起
        //                        character.CurrentFrameId == LF2StandardFrames.RowingBack)    // 108: 背面爬起
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", $"爬起暂停 Frame={character.CurrentFrameId}");
        //                        character.PS.vy = 0;
        //                    }
        //                    return false;

        //                case "frame":
        //                    Log.Info("[State {0}:frame] ", eventType);

        //                    // ✓ 等待时间设置（对应 FLF Line 667-671）
        //                    // 延长爬起动作的持续时间
        //                    if (character.CurrentFrameId == LF2StandardFrames.Rowing ||      // 100
        //                        character.CurrentFrameId == LF2StandardFrames.RowingBack)    // 108
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "设置爬起等待时间");
        //                        character.Trans.SetWait(LF2StateConstants.RowingWaitTime);  // 1 帧
        //                        return true;
        //                    }
        //                    return false;

        //                case "fall_onto_ground":
        //                    Log.Info("[State {0}:fall_onto_ground] ", eventType);

        //                    // ✓ 落地处理（对应 FLF Line 674-679）
        //                    // 落地时的状态转换：爬起结束 → 蹲姿
        //                    if (character.CurrentFrameId == LF2StandardFrames.Rowing1 ||     // 101: 正面爬起结束
        //                        character.CurrentFrameId == LF2StandardFrames.RowingBack1)   // 109: 背面爬起结束
        //                    {
        //                        Log.Info("爬起结束落地");
        //                        Log.Info("TransitionTo: Frame {0} ({1})", LF2StandardFrames.Crouch, "落地 → 蹲姿");
        //                        character.TransitionToFrame(LF2StandardFrames.Crouch, 0);  // 215: 蹲姿帧
        //                        return true;
        //                    }
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        /// <summary>
        //        /// 防御被破状态处理器 (state = 8)
        //        /// 对应 FLF character.js:698-719
        //        ///
        //        /// 功能：处理防御被破后的特殊移动逻辑
        //        /// 关键机制：修复弱击倒移动时的方向问题
        //        /// 问题：防御被破时，角色被击退方向可能与朝向方向相反
        //        /// 解决：在空中或速度不足时，强制按帧定义的dvx设置速度
        //        /// </summary>
        //        private bool BrokenDefendStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame_force":
        //                case "TU_force":
        //                    Log.Info("[State {0}:{1}] Event={2}", 8, "BrokenDefend", eventType);

        //                    // ✓ 强制移动方向修正（对应 FLF Line 702-717）

        //                    var D = character.Frame.D;
        //                    var PS = character.PS;
        //                    if (D.dvx != 0)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 8, "BrokenDefend", $"防御被破 dvx={D.dvx} → 修正移动方向");
        //                        if ((PS.vx > 0 ? 1 : -1) != PS.Dirh())
        //                        {
        //                            float avx = PS.vx > 0 ? PS.vx : -PS.vx;
        //                            float dirx = 2 * (PS.vx > 0 ? 1 : -1);
        //                            if (PS.y < 0 || avx < D.dvx)
        //                                PS.vx = dirx * D.dvx;

        //                            if (D.dvx < 0)
        //                                PS.vx -= dirx;

        //                        }
        //                    }
        //                    break;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// 抓取状态处理器 (state = 9)
        //        /// 对应 FLF character.js:722-853
        //        ///
        //        /// 功能：处理抓取敌人和投掷动作
        //        /// 关键特性：
        //        /// 1. 抓取计数器系统（counter 从43递减到0）
        //        /// 2. 攻击次数记录（每次成功攻击延长抓取时间）
        //        /// 3. 位置同步（每帧更新被抓对象位置到cpoint）
        //        /// 4. 伤害处理（通过cpoint.injury）
        //        /// 5. Z轴层级控制（cover参数）
        //        /// 6. 方向控制（dircontrol参数）
        //        /// 7. 投掷/攻击/跳跃动作（taction/aaction/jaction）
        //        /// </summary>
        //        private bool CatchingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_entry":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, character.CurrentFrameId);

        //                    // ✓ 初始化抓取状态（对应 FLF Line 570-573）
        //                    character.StateMem["stateTU"] = true;
        //                    character.StateMem["counter"] = 43;    // 初始计数43帧
        //                    character.StateMem["attacks"] = 0;     // 攻击次数计数
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "初始化抓取状态 counter=43, attacks=0");
        //                    return false;

        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, character.CurrentFrameId);

        //                    // ✓ 清理抓取状态（对应 FLF Line 577-580）
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "Clear catching state");
        //                    // TODO: 需要实现抓取系统
        //                    // character.Catching = null;    // 清空抓取目标
        //                    // character.Ps.Zz = 0;          // 重置Z轴覆盖
        //                    return false;

        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, character.CurrentFrameId);

        //                    // ✓ 抓取帧处理（对应 FLF Line 584-614）
        //                    int frameId = character.CurrentFrameId;
        //                    var D = character.Frame.D;

        //                    // ==================== 特殊帧处理 ====================

        //                    // 帧123（成功攻击）：增加attacks计数器，延长抓取时间3帧
        //                    if (frameId == 123)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧123 成功攻击 → 延长抓取时间");
        //                        // TODO: 需要实现抓取系统后取消注释
        //                        // character.StateMem["attacks"] = (int)character.StateMem["attacks"] + 1;  // 攻击次数+1
        //                        // character.StateMem["counter"] = (int)character.StateMem["counter"] + 3;  // 延长3帧
        //                        // character.Trans.SetWait(character.Trans.Wait() + 1);  // 增加等待1帧
        //                        // return true;
        //                    }

        //                    // 帧233/234：减少等待时间（1帧）
        //                    if (frameId == 233 || frameId == 234)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", $"Frame {frameId} -> decrease wait");
        //                        // TODO: 需要实现抓取系统后取消注释
        //                        // character.Trans.SetWait(character.Trans.Wait() - 1);
        //                        // return true;
        //                    }

        //                    // 帧240：Rudolf特殊变身
        //                    if (frameId == 240)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧240 Rudolf特殊变身");
        //                        // TODO: 需要实现id_update机制
        //                        // character.CallIdUpdate("rudolf_transform");
        //                        // return true;
        //                    }

        //                    // ==================== 位置同步 ====================
        //                    // 更新被抓取对象的位置到cpoint定义的相对位置
        //                    // 对应 FLF Line 605-613

        //                    // TODO: 需要实现抓取系统后取消注释
        //                    // if (character.Catching != null && D.cpoint != null)
        //                    // {
        //                    //     // 计算抓点世界坐标
        //                    //     Vector3 cpointWorldPos = character.transform.TransformPoint(
        //                    //         new Vector3(D.cpoint.x, D.cpoint.y, D.cpoint.z)
        //                    //     );
        //                    //
        //                    //     // 通知被抓对象更新位置
        //                    //     character.Catching.caught_b(
        //                    //         cpointWorldPos,           // 抓点世界坐标
        //                    //         D.cpoint,         // cpoint数据
        //                    //         character.unitActions.dir == DIRECTION.RIGHT ? "right" : "left",  // 朝向方向
        //                    //         /* dirv() */              // 垂直方向（需要实现）
        //                    //     );
        //                    // }

        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, character.CurrentFrameId);

        //                    // ✓ 抓取伤害与覆盖处理（对应 FLF Line 622-657）

        //                    // TODO: 需要实现抓取系统后取消注释
        //                    //
        //                    // 完整逻辑（对应 FLF Line 622-657）：
        //                    //
        //                    // // 检查抓取状态有效性（双向验证）
        //                    // if (character.Catching != null &&
        //                    //     character.caught_cpointkind() == 1 &&           // 自身是抓取者
        //                    //     character.Catching.caught_cpointkind() == 2)    // 对方是被抓者
        //                    // {
        //                    //     if ((bool)character.StateMem["stateTU"])
        //                    //     {
        //                    //         character.StateMem["stateTU"] = false;
        //                    //
        //                    //         var cpoint = character.Frame.D.cpoint;
        //                    //
        //                    //         // ==================== 处理伤害 ====================
        //                    //         // 对应 FLF Line 632-637
        //                    //         if (cpoint.injury != 0)
        //                    //         {
        //                    //             // 对被抓取对象造成伤害
        //                    //             if (character.Catching.TakeDamage(cpoint.injury))
        //                    //             {
        //                    //                 // 延长等待时间（击中反馈）
        //                    //                 character.Trans.SetWait(
        //                    //                     Mathf.Min(character.Trans.Wait() + 1, 99)
        //                    //                 );
        //                    //             }
        //                    //         }
        //                    //
        //                    //         // ==================== 处理覆盖（Z轴层级）====================
        //                    //         // 对应 FLF Line 639-649
        //                    //         int cover = 0;  // 默认值
        //                    //         if (cpoint.cover != 0)
        //                    //         {
        //                    //             cover = cpoint.cover;
        //                    //         }
        //                    //
        //                    //         if (cover == 0 || cover == 10)
        //                    //         {
        //                    //             character.Ps.Zz = 1;   // 在被抓者前面
        //                    //         }
        //                    //         else
        //                    //         {
        //                    //             character.Ps.Zz = -1;  // 在被抓者后面
        //                    //         }
        //                    //
        //                    //         // ==================== 方向控制 ====================
        //                    //         // 对应 FLF Line 651-655
        //                    //         if (cpoint.dircontrol == 1)
        //                    //         {
        //                    //             // 允许玩家改变朝向
        //                    //             if (character._Character._CharacterInput.CurrentMoveInput.x < -0.1f)
        //                    //             {
        //                    //                 character.unitActions.TurnToDir(DIRECTION.LEFT);
        //                    //             }
        //                    //             else if (character._Character._CharacterInput.CurrentMoveInput.x > 0.1f)
        //                    //             {
        //                    //                 character.unitActions.TurnToDir(DIRECTION.RIGHT);
        //                    //             }
        //                    //         }
        //                    //     }
        //                    // }

        //                    return false;

        //                case "post_combo":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, character.CurrentFrameId);

        //                    // 抓取计数器减少（FLF:669-685）
        //                    // TODO: 实现抓取计数器递减和释放逻辑
        //                    return false;

        //                case "combo":
        //                    // ✓ 抓取中的攻击与连招（对应 FLF Line 692-747）
        //                    string comboKey = eventData as string;
        //                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 9, "Catching", eventType, comboKey, character.CurrentFrameId);

        //                    if (string.IsNullOrEmpty(comboKey))
        //                        return false;

        //                    // ==================== 攻击键处理 ====================
        //                    if (comboKey == "att")
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "抓取中攻击 → 投掷/攻击动作");
        //                        // 投掷/攻击动作处理（FLF:696-725）
        //                        // TODO: 实现 cpoint.taction/aaction 投掷和攻击逻辑
        //                        return true;
        //                    }

        //                    // ==================== 跳跃键处理 ====================
        //                    if (comboKey == "jump")
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "抓取中跳跃 → 跳跃动作");
        //                        // 抓取跳跃动作（FLF:740-746）
        //                        // TODO: 实现 cpoint.jaction 跳跃动作
        //                    }

        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        /// <summary>
        //        /// 被抓取状态处理器 (state = 10)
        //        /// 对应 FLF character.js:856-939
        //        ///
        //        /// 功能：处理被敌人抓取时的表现
        //        /// 关键特性：
        //        /// 1. 位置同步到抓取者的 cpoint
        //        /// 2. 被投掷时的速度设置（throwvx/vy/vz）
        //        /// 3. 方向处理（cover 参数控制）
        //        /// 4. 投掷伤害记录（落地时生效）
        //        /// 5. 抓取状态验证（双向检查）
        //        /// </summary>
        //        private bool BeingCaughtStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "Clear being-caught state");
        //                    // 清理被抓状态（FLF:781-787）
        //                    // TODO: 实现抓取系统（清空抓取者引用、抓点数据、方向数据）
        //                    return false;

        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, character.CurrentFrameId);

        //                    // ✓ 被抓帧处理（对应 FLF Line 792-794）
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "设置长时间等待（由抓取者控制）");
        //                    character.StateMem["frameTU"] = true;
        //                    character.Trans.SetWait(99);  // 长时间等待（由抓取者控制）
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, character.CurrentFrameId);

        //                    // ✓ 被抓时的处理（对应 FLF Line 803-880）

        //                    // ==================== 帧135时消除重力 ====================
        //                    // 对应 FLF Line 804-807
        //                    if (character.CurrentFrameId == 135)
        //                    {
        //                        // Step 2: 使用 PS.vy 替代 unitActions.yForce
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "帧135 暂停（消除重力）");
        //                        character.PS.vy = 0;  // 暂停
        //                    }

        //                    // 被投掷：位置同步处理（FLF:809-880）
        //                    // TODO: 实现抓取系统（双向验证、速度设置、位置同步、状态失效处理）

        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 冰冻状态处理器 (state = 13)
        //        /// 对应 FLF character.js:1097-1106
        //        ///
        //        /// 功能：处理冰冻效果
        //        /// 关键特性：
        //        /// 1. 离开冰冻状态时创建冰块碎裂效果
        //        /// 2. 冰冻状态期间：
        //        ///    - 角色完全停止（无法移动、攻击、连招）
        //        ///    - 受到攻击时会碎裂（转入倒地状态）
        //        /// 3. 关键帧：200（冰冻帧）
        //        ///
        //        /// 冰冻机制（来自 hit 函数）：
        //        /// - effectnum = 3/30: 冰冻攻击
        //        /// - 未冰冻 → 转到帧200（冰冻）
        //        /// - 已冰冻 → 碎裂倒地（转到帧182）
        //        /// - 强制丢弃武器
        //        /// </summary>
        //        private bool FrozenStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 13, "Frozen", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 13, "Frozen", "冰冻结束 → 创建碎裂效果");
        //                    // 创建冰块碎裂效果（FLF:1101-1104）
        //                    // TODO: 实现特效系统（ID 212 碎裂效果，音效 1/066）
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 躺地状态处理器 (state = 14)
        //        /// 对应 FLF character.js:1113-1138
        //        ///
        //        /// 功能：处理落地后的躺地状态和死亡判定
        //        /// 关键特性：
        //        /// 1. state_entry：
        //        ///    - 重置 fall 和 bdefend 值（清空临时状态）
        //        ///    - 检测死亡（hp ≤ 0）并触发 die()
        //        ///    - NPC 死亡时启动玩家闪烁计数（30帧后销毁）
        //        /// 2. state_exit：
        //        ///    - 爬起后获得 30 帧无敌时间
        //        ///    - 启用透明效果提示玩家无敌状态
        //        ///    - 设置 super 状态（超级护甲）
        //        /// 3. 关键帧：
        //        ///    - 230: 正面躺地
        //        ///    - 231: 背面躺地
        //        ///
        //        /// 死亡流程（来自 generic 状态 TU 事件）：
        //        /// - 死亡闪烁到 4 阶段：
        //        ///   1. dead_blink_count = 0: 开启闪烁
        //        ///   2. 0 < count < 30: 持续闪烁
        //        ///   3. count >= 30: 关闭闪烁，隐藏精灵和影子
        //        ///   4. count = -1: 销毁对象
        //        /// </summary>
        //        private bool LyingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_entry":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 14, "Lying", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 14, "Lying", "Reset state & death check");
        //                    // 重置状态与死亡检测（FLF:1117-1129）
        //                    // TODO: 实现角色属性系统（重置 fall/bdefend、死亡判定、NPC 死亡闪烁）
        //                    return false;

        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 14, "Lying", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 14, "Lying", "Getup -> 30 frames invincible");
        //                    // 爬起无敌效果（FLF:1130-1137）
        //                    // TODO: 实现特效系统（30帧无敌、闪烁效果、super 状态）
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 综合状态处理器 (state = 15)
        //        /// 对应 FLF character.js:1145-1223
        //        ///
        //        /// 功能：处理多种复杂状态（停止奔跑、蹲下、冲刺攻击、武器投掷等）
        //        /// 关键特性：
        //        /// 1. frame 事件：处理多种帧的特殊逻辑
        //        ///    - 帧9: 重武器停止奔跑 → 检查重武器，转到帧12
        //        ///    - 帧215: 蹲下 → 减少等待时间 1 帧
        //        ///    - 帧219: 蹲下 → 调用 id_update 或根据前帧应用冲刺力
        //        ///    - 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
        //        ///    - 帧257: Rudolf 消失帧 → 调用变身逻辑
        //        /// 2. combo 事件：蹲下二段跳（仅帧215）：
        //        ///    - 防御键 → 转到帧102（奔跑防御）
        //        ///    - 跳跃键 → 根据方向和速度决定跳跃类型：
        //        ///      * 有方向输入 → 该方向跳跃（帧213）
        //        ///      * 静止不动 → 垂直跳跃（帧210）
        //        ///      * 有速度同向 → 前冲刺（帧213）
        //        ///      * 有速度反向 → 后冲刺（帧214）
        //        ///
        //        /// 覆盖的状态类型：
        //        /// - 停止奔跑（stop_running）
        //        /// - 蹲下（crouch） 帧215
        //        /// - 蹲下2（crouch2） 帧219
        //        /// - 冲刺攻击（dash_attack）
        //        /// - 轻武器投掷（light_weapon_thw）
        //        /// - 重武器投掷（heavy_weapon_thw）
        //        /// - 重武器停止奔跑（heavy_stop_run） 帧9
        //        /// - 空中轻武器投掷（sky_lgt_wp_thw） 帧54
        //        /// - 消失（disappear） 帧257（Rudolf 特有）
        //        /// </summary>
        //        private bool MixedStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}", 15, "Mixed", eventType);
        //                    // 多帧特殊处理（FLF:1149-1188）
        //                    int frameId = character.Frame.N;

        //                    if (frameId == LF2StandardFrames.TreeJump2)
        //                    {
        //                        // 重武器停止奔跑
        //                        // 检查是否持有重武器
        //                    }
        //                    else if (frameId == LF2StandardFrames.Crouch)  // 215
        //                    {
        //                        // 帧215: 蹲下 → 减少等待时间
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧215 蹲下 → 减少等待时间");
        //                        character.Trans.IncWait(-1);
        //                        return false;
        //                    }
        //                    else if (frameId == LF2StandardFrames.Crouch2)
        //                    {
        //                        // 蹲下
        //                        if (!character._Character._IdUpdate.TryInvokeGeneric(IdUpdateHooks.State15_Crouch))
        //                        {
        //                            switch (character.Frame.PN) // 上一帧编号
        //                            {
        //                                case LF2StandardFrames.Rowing5:
        //                                    // 划船后
        //                                    // 应用摩擦力
        //                                    CharacterMechanics.UnitFriction(character.PS);
        //                                    break;

        //                                case LF2StandardFrames.DashBack: // 冲刺后
        //                                case LF2StandardFrames.DashAttack:
        //                                case LF2StandardFrames.DashAttack + 1:
        //                                case LF2StandardFrames.DashAttack + 2: // 冲刺攻击
        //                                                                       // 减少等待时间
        //                                    character.Trans.IncWait(-1);
        //                                    break;
        //                            }
        //                        }
        //                    }
        //                    else if (frameId == LF2StandardFrames.SkyLgtWpThw3)
        //                    {
        //                        // 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
        //                        var D = character.Frame.D;
        //                        if (D.next == LF2StandardFrames.LoopToStart && character.PS.y < 0)
        //                        {
        //                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧54 空中轻武器投掷结束 → 返回跳跃");
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.JumpingAir, "空中投掷完成");
        //                            character.Trans.SetNext(LF2StandardFrames.JumpingAir);  // 212
        //                        }
        //                    }
        //                    else if (frameId == LF2StandardFrames.Disappear)
        //                    {
        //                        // 帧257: Rudolf 消失帧 → 调用变身逻辑

        //                        // 其他特殊帧需要武器系统
        //                    }
        //                    break;
        //                case "combo":
        //                    // ✓ 蹲下二段跳（对应 FLF Line 1190-1221）
        //                    string comboKey = eventData as string;
        //                    Log.Info("[State {0}:{1}] Event={2}, Key={3}", 15, "Mixed", eventType, comboKey);

        //                    // 只在蹲下帧215响应
        //                    if (character.Frame.N == LF2StandardFrames.Crouch)  // 215
        //                    {
        //                        if (string.IsNullOrEmpty(comboKey))
        //                            break;

        //                        // 防御键 → 奔跑防御
        //                        if (comboKey == "def")
        //                        {
        //                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下 + 防御 → 奔跑防御");
        //                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", 102, "奔跑防御");
        //                            character.TransitionToFrame(LF2StandardFrames.Rowing2, 10);
        //                            return true;
        //                        }

        //                        // 跳跃键 → 4种跳跃类型
        //                        if (comboKey == "jump")
        //                        {
        //                            var (dx, dz) = GetMoveInput(character);
        //                            {
        //                                // 1. 有方向输入 → 该方向跳跃
        //                                if (dx != 0)
        //                                {
        //                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", $"蹲下二段跳 dx={dx} → 方向跳跃");
        //                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward, "方向跳跃");
        //                                    character.TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
        //                                    character.SetDirection(dx == 1 ? DIRECTION.RIGHT : DIRECTION.LEFT);
        //                                }
        //                                else if (character.PS.vx == 0)
        //                                {
        //                                    character.Trans.IncWait(2, 10, 99);
        //                                    character.Trans.SetNext(LF2StandardFrames.Jumping, 10);
        //                                }
        //                                else if ((character.PS.vx > 0 ? 1 : -1) == character.PS.Dirh())
        //                                {
        //                                    character.TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
        //                                }
        //                                else
        //                                {
        //                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下二段跳 → 前冲刺2");
        //                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward2, "前冲刺2");
        //                                    // 检查角色是否静止（无水平速度）
        //                                    // 简化实现：直接跳到垂直跳跃
        //                                    character.TransitionToFrame(LF2StandardFrames.DashForward2, 10);  // 214
        //                                }
        //                            }

        //                            return true;
        //                        }
        //                    }
        //                    break;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// 受伤2状态处理器 (state = 16)
        //        /// 对应 FLF character.js:1230-1235
        //        ///
        //        /// 功能：痛苦之舞（Dance of Pain）状态
        //        /// 关键特性：
        //        /// 1. 空实现：无任何特殊逻辑
        //        /// 2. 所有行为由帧数据驱动（动画自动播放）
        //        /// 3. 可能是预留状态或由角色特定逻辑覆盖
        //        ///
        //        /// 推测用途：
        //        /// - 被抓取前的准备状态
        //        /// - 或某些特殊受击动作的状态标记
        //        /// - FLF 中也是空实现，表示所有逻辑都在帧数据中
        //        /// </summary>
        //        private bool Injured2StateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            // ✓ 无特殊事件处理（对应 FLF Line 1230-1235）
        //            // FLF 中也是空实现，所有逻辑由帧数据驱动
        //            return false;
        //        }


        //        /// <summary>
        //        /// 燃烧状态处理器 (state = 18)
        //        /// 对应 FLF character.js:1242-1258
        //        ///
        //        /// 功能：处理燃烧效果
        //        /// 关键特性：
        //        /// 1. frame 事件：每帧创建燃烧特效（持续燃烧视觉效果）
        //        /// 2. fall_onto_ground 事件：落地瞬间创建燃烧效果
        //        /// 3. fell_onto_ground 事件：复用 State 12 的落地逻辑（弹起/躺地判定）
        //        /// 4. 关键帧：203-206（燃烧落地帧）
        //        ///
        //        /// 燃烧机制（来自 hit 函数）：
        //        /// - effectnum = 2/20/21/22/23: 火焰攻击
        //        /// - 转到帧203（燃烧状态）
        //        /// - 高级火焰（21/22/23）弱化投掷判定器
        //        /// - 燃烧状态防止急火击中（effectnum=20/21）
        //        /// - 燃烧状态21/22不会伤害队友
        //        /// </summary>
        //        private bool BurningStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "持续燃烧 → 每帧创建燃烧特效");
        //                    // 每帧创建燃烧特效（FLF:1246-1249）
        //                    // TODO: 实现特效系统（ID 302，持续模式）
        //                    return false;

        //                case "fall_onto_ground":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧落地 → 创建落地燃烧特效");
        //                    // 落地时创建燃烧特效（FLF:1250-1252）
        //                    // TODO: 实现特效系统（ID 302，一次性模式）
        //                    return false;

        //                case "fell_onto_ground":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧倒地 → 复用State 12落地逻辑");
        //                    // 复用State 12落地逻辑（FLF:1253-1256）
        //                    return FallingStateHandler(character, "fell_onto_ground", eventData);

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// Firen特有状态处理器 (state = 19)
        //        /// 对应 FLF character.js:1265-1274
        //        ///
        //        /// 功能：Firen角色的特殊状态（可能是火焰奔跑）
        //        /// 关键特性：
        //        /// 1. TU 事件：弱化设置Z轴速度为奔跑速度
        //        /// 2. 可能用于火焰奔跑时的持续移动
        //        /// 3. 特殊免疫：State 19时防止State 3000的击中（可能是火焰奔跑时的无敌帧）
        //        ///
        //        /// 特殊免疫规则（来自 hit 函数）：
        //        /// - if ($.state() === 19 && att.state() === 3000) { return false }
        //        /// - State 19时防止State 3000（可能是某种特殊对象）的攻击
        //        /// - 可能是火焰奔跑时的无敌帧机制
        //        ///
        //        /// 与其他状态的交互：
        //        /// - 未明确定义进入/退出逻辑
        //        /// - 可能由Firen特定技能触发
        //        /// </summary>
        //        private bool FirenSpecificStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 19, "FirenSpecific", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 19, "FirenSpecific", "Firen特殊状态 → 强制Z轴奔跑速度");
        //                    // 强制设置Z轴速度为奔跑速度（FLF:1269-1272）
        //                    // TODO: 实现深度移动系统（vz = dirv() * running_speedz）
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        // ==================== NTSD 扩展状态处理器 ====================

        //        /// <summary>
        //        /// 蓄力状态处理器 (state = 17)
        //        /// 用途：角色进行技能蓄力时的状态
        //        /// 出现次数：16
        //        /// </summary>
        //        private bool ChargingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "state_entry":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, character.CurrentFrameId);

        //                    // ✓ 初始化蓄力状态
        //                    character.StateMem["chargeTime"] = 0;
        //                    character.StateMem["maxChargeTime"] = 60;  // 60帧 = 2秒（30fps）
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "初始化蓄力 chargeTime=0, maxChargeTime=60");
        //                    return false;

        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, character.CurrentFrameId);

        //                    // ✓ 蓄力状态的帧处理
        //                    // 蓄力等级判定和特效播放由外部系统处理
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, character.CurrentFrameId);

        //                    // ✓ 蓄力时间更新
        //                    if (character.StateMem.ContainsKey("chargeTime"))
        //                    {
        //                        int chargeTime = (int)character.StateMem["chargeTime"];
        //                        int maxChargeTime = (int)character.StateMem["maxChargeTime"];

        //                        // 递增蓄力时间，但不超过上限
        //                        if (chargeTime < maxChargeTime)
        //                        {
        //                            character.StateMem["chargeTime"] = chargeTime + 1;
        //                            if (chargeTime % 10 == 0)  // 每10帧输出一次日志
        //                            {
        //                                Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中 chargeTime={chargeTime}/{maxChargeTime}");
        //                            }
        //                        }
        //                    }
        //                    return false;

        //                case "combo":
        //                    // ✓ 蓄力中的输入处理
        //                    string comboKey = eventData as string;
        //                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 17, "Charging", eventType, comboKey, character.CurrentFrameId);

        //                    // 任何按键输入都会结束蓄力状态
        //                    // 具体的技能释放逻辑由技能系统处理
        //                    if (!string.IsNullOrEmpty(comboKey))
        //                    {
        //                        int chargeTime = character.StateMem.ContainsKey("chargeTime") ? (int)character.StateMem["chargeTime"] : 0;
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中断 按键={comboKey}, 蓄力时间={chargeTime}");
        //                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 17, "Charging", LF2StandardFrames.Standing, "蓄力中断");
        //                        // 返回站立状态，让技能系统接管
        //                        character.TransitionToFrame(LF2StandardFrames.Standing, 10);
        //                        return true;
        //                    }
        //                    return false;

        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, character.CurrentFrameId);

        //                    // ✓ 清理蓄力状态内存
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "Clear charging state mem");
        //                    character.StateMem.Remove("chargeTime");
        //                    character.StateMem.Remove("maxChargeTime");
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        /// <summary>
        //        /// 武器状态处理器 (states 1000-1004)
        //        /// 用途：处理武器的各种状态（飞行、持有、落地等）
        //        /// 出现次数：128（合计）
        //        /// </summary>
        //        private bool WeaponStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    var D = character.Frame.D;
        //                    int currentState = D.state;
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", currentState, "Weapon", eventType, character.CurrentFrameId);

        //                    // ✓ 根据具体状态处理
        //                    if (currentState == LF2States.WeaponInSky)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Weapon", "Weapon flying");
        //                        // TODO: 武器在空中旋进
        //                        // - 应用飞行速度
        //                        // - 碰撞检测
        //                        // - 击中判定
        //                    }
        //                    else if (currentState == LF2States.WeaponOnHand)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Weapon", "Weapon on hand");
        //                        // TODO: 武器在手中
        //                        // - 跟随角色移动
        //                        // - 等待投掷指令
        //                    }
        //                    else if (currentState == LF2States.WeaponThrowing)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Weapon", "Weapon throwing");
        //                        // TODO: 武器投掷中
        //                        // - 设置初始速度
        //                        // - 转换到旋进状态
        //                    }
        //                    else if (currentState == LF2States.WeaponJustOnGround || currentState == LF2States.WeaponOnGround)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Weapon", "武器落地");
        //                        // TODO: 武器落地
        //                        // - 播放落地音效
        //                        // - 允许拾取
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", character.Frame.D.state, "Weapon", eventType, character.CurrentFrameId);

        //                    // ✓ 武器物理更新
        //                    // TODO: 实现武器物理
        //                    // - 应用重力
        //                    // - 应用摩擦力
        //                    // - 碰撞检测
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 投射物状态处理器 (states 3000-3003)
        //        /// 用途：处理投射物的飞行、命中等待状态
        //        /// 出现次数：190（合计）
        //        /// </summary>
        //        private bool ProjectileStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    var D = character.Frame.D;
        //                    int currentState = D.state;
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", currentState, "Projectile", eventType, character.CurrentFrameId);

        //                    // ✓ 根据具体状态处理
        //                    if (currentState == LF2States.ProjectileFlying)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Projectile", "投射物飞行中");
        //                        // TODO: 投射物飞行（如千鸟、冰剑）
        //                        // - 应用飞行速度（通常较快）
        //                        // - 飞行轨迹计算
        //                        // - 碰撞检测
        //                    }
        //                    else if (currentState == LF2States.ProjectileHiting)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Projectile", "投射物命中中");
        //                        // TODO: 投射物命中中
        //                        // - 播放命中动画
        //                        // - 应用伤害
        //                        // - 击退效果
        //                    }
        //                    else if (currentState == LF2States.ProjectileHit)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Projectile", "投射物命中后");
        //                        // TODO: 投射物命中后
        //                        // - 播放爆炸/消失效果
        //                        // - 销毁投射物
        //                    }
        //                    else if (currentState == LF2States.ProjectileTeleport)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", currentState, "Projectile", "Projectile teleport");
        //                        // TODO: 投射物瞬移（如天遁）
        //                        // - 瞬移到目标位置
        //                        // - 播放瞬移效果
        //                        // - 立即击中判定
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", character.Frame.D.state, "Projectile", eventType, character.CurrentFrameId);

        //                    // ✓ 投射物物理更新
        //                    // TODO: 实现投射物物理
        //                    // - 应用速度（投射物通常不受重力影响）
        //                    // - 追踪目标（如追踪符）
        //                    // - 生命周期检查
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        /// <summary>
        //        /// 对象飞行状态处理器 (state = 3005)
        //        /// 用途：通用对象飞行状态，也用于变身术
        //        /// 这是使用最频繁的状态之一（307次）
        //        /// </summary>
        //        private bool ObjectFlyingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3005, "ObjectFlying", eventType, character.CurrentFrameId);

        //                    // ✓ 对象飞行帧处理
        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "ObjectFlying frame update");
        //                    // TODO: 实现对象飞行逻辑
        //                    // - 投掷物：对象向目标位置旋进
        //                    // - 召唤物：对象跟随主人移动
        //                    // - 特效对象：按轨迹旋进
        //                    var D = character.Frame.D;

        //                    // ✓ TODO: 处理 ITR（伤害判定）- 飞行对象可能有攻击判定
        //                    if (D.itrs != null && D.itrs.Count > 0)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "处理飞行对象攻击判定");
        //                        // ITRProcessor.ProcessITR(itr, character);
        //                    }

        //                    // ✓ TODO: 处理 OPoint（投掷物/召唤）
        //                    if (D.opoint != null)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "Process object spawn point");
        //                        // OPointProcessor.ProcessOPoint(D.opoint, character);
        //                    }

        //                    // ✓ 检查对象是否结束旋进 (next = 999)
        //                    if (D.next == LF2StandardFrames.LoopToStart)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "对象飞行结束");
        //                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3005, "ObjectFlying", LF2StandardFrames.Standing, "飞行结束");
        //                        // 对象飞行结束，返回站姿或消失
        //                        character.PlayFrameByID(LF2StandardFrames.Standing);
        //                        return true;
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3005, "ObjectFlying", eventType, character.CurrentFrameId);

        //                    // ✓ 对象飞行物理更新
        //                    // TODO: 实现对象飞行物理
        //                    // - 应用飞行速度
        //                    // - 轨迹计算（直线、曲线、弧线等）
        //                    // - 碰撞检测
        //                    return false;

        //                case "state_entry":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3005, "ObjectFlying", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "Enter ObjectFlying");
        //                    // ✓ 进入对象飞行状态
        //                    // TODO: 初始化旋进参数
        //                    // - 设置初始速度
        //                    // - 设置飞行目标
        //                    // - 播放飞行特效
        //                    return false;

        //                case "state_exit":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3005, "ObjectFlying", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 3005, "ObjectFlying", "Exit ObjectFlying");
        //                    // ✓ 退出对象旋进状态
        //                    // TODO: 清理飞行状态
        //                    // - 清除飞行特效
        //                    // - 重置速度
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 对象扩散状态处理器 (state = 3006)
        //        /// 用途：技能特效包裹或爆炸动画（如爆炸效果）
        //        /// 出现次数：152
        //        /// </summary>
        //        private bool ObjectExpandingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3006, "ObjectExpanding", eventType, character.CurrentFrameId);

        //                    // ✓ 对象扩散帧处理
        //                    var D = character.Frame.D;

        //                    // ✓ TODO: 处理扩散范围伤害
        //                    if (D.itrs != null && D.itrs.Count > 0)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 3006, "ObjectExpanding", "处理扩散范围伤害");
        //                        // 扩散效果通常有持续伤害或范围伤害
        //                        // ITRProcessor.ProcessITR(itr, character);
        //                    }

        //                    // ✓ TODO: 处理扩散效果
        //                    // - 缩放变化（Scale）
        //                    // - 透明度变化（Alpha）
        //                    // - 粒子效果

        //                    // ✓ 检查包裹是否结束 (next = 999)
        //                    if (D.next == LF2StandardFrames.LoopToStart)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 3006, "ObjectExpanding", "ObjectExpanding done -> destroy");
        //                        // 扩散结束，销毁对象
        //                        // character.Destroy();
        //                        return true;
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3006, "ObjectExpanding", eventType, character.CurrentFrameId);

        //                    // ✓ 对象扩散更新
        //                    // TODO: 实现扩散动画
        //                    // - 缩放增大
        //                    // - 透明度降低
        //                    // - 范围伤害检测
        //                    return false;

        //                case "state_entry":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 3006, "ObjectExpanding", eventType, character.CurrentFrameId);

        //                    Log.Info("[State {0}:{1}] -> Branch: {2}", 3006, "ObjectExpanding", "Enter ObjectExpanding");
        //                    // ✓ 进入对象扩散状态
        //                    // TODO: 初始化包裹参数
        //                    // - 设置初始缩放
        //                    // - 设置扩散速率
        //                    // - 播放扩散音效
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 特效播放状态处理器 (state = 9997)
        //        /// 用途：视觉特效播放（如能量波特效）
        //        /// 出现次数：47
        //        /// </summary>
        //        private bool EffectPlayingStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9997, "EffectPlaying", eventType, character.CurrentFrameId);

        //                    // ✓ 特效播放帧处理
        //                    var D = character.Frame.D;

        //                    // ✓ TODO: 播放特效
        //                    // - 粒子效果
        //                    // - 屏幕效果（闪光、模糊等）
        //                    // - UI 效果

        //                    // ✓ 检查特效是否结束 (next = 999)
        //                    if (D.next == LF2StandardFrames.LoopToStart)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9997, "EffectPlaying", "EffectPlaying done -> destroy");
        //                        // 特效播放结束，销毁对象
        //                        // character.Destroy();
        //                        return true;
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9997, "EffectPlaying", eventType, character.CurrentFrameId);

        //                    // ✓ 特效更新
        //                    // TODO: 实现特效动画
        //                    // - 特效帧更新
        //                    // - 特效参数变化
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }


        //        /// <summary>
        //        /// 特殊效果状态处理器 (state = 30005)
        //        /// 用途：特殊效果（如旋风手里剑的特殊效果）
        //        /// 出现次数：2
        //        /// </summary>
        //        private bool SpecialEffectStateHandler(ILF2LivingObject character, string eventType, object eventData)
        //        {
        //            switch (eventType)
        //            {
        //                case "frame":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 30005, "SpecialEffect", eventType, character.CurrentFrameId);

        //                    // ✓ 特殊效果帧处理
        //                    var D = character.Frame.D;

        //                    // ✓ TODO: 处理特殊效果逻辑
        //                    // - 特定技能的独特效果
        //                    // - 可能包括伤害、位移、状态变化等

        //                    // ✓ TODO: 处理 ITR（伤害判定）
        //                    if (D.itrs != null && D.itrs.Count > 0)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 30005, "SpecialEffect", "处理特殊效果伤害判定");
        //                        // ITRProcessor.ProcessITR(itr, character);
        //                    }

        //                    // ✓ 检查效果是否结束 (next = 999)
        //                    if (D.next == LF2StandardFrames.LoopToStart)
        //                    {
        //                        Log.Info("[State {0}:{1}] -> Branch: {2}", 30005, "SpecialEffect", "SpecialEffect done -> destroy");
        //                        // 特殊效果结束，销毁对象
        //                        // character.Destroy();
        //                        return true;
        //                    }
        //                    return false;

        //                case "TU":
        //                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 30005, "SpecialEffect", eventType, character.CurrentFrameId);

        //                    // ✓ 特殊效果更新
        //                    // TODO: 实现特殊效果逻辑
        //                    // - 效果参数更新
        //                    // - 持续时间检查
        //                    return false;

        //                default:
        //                    return false;
        //            }
        //        }

        //        /// <summary>
        //        /// 允许外部注册自定义状态处理器
        //        /// 用于角色特定状态（100+）
        //        /// </summary>
        //        public void RegisterCustomHandler(int state, StateHandler handler)
        //        {
        //            stateHandlers[state] = handler;
        //        }

        //        /// <summary>
        //        /// 移除自定义状态处理器
        //        /// </summary>
        //        public void UnregisterCustomHandler(int state)
        //        {
        //            if (state >= 100)  // 只允许移除自定义状态
        //                stateHandlers.Remove(state);
        //        }

        //        /// <summary>
        //        /// 下一帧数据（用于状态处理器返回）
        //        /// </summary>
        //        public class NextFrameData
        //        {
        //            public int? nextFrameId;
        //        }

    }
}
