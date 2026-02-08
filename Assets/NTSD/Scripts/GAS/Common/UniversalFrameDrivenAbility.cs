using GAS.Runtime;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;
using System;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using BeatEmUpTemplate2D;

namespace NTSD.GAS
{
    public class UniversalFrameDrivenAbility : AbstractAbility<AAUniversalFrameDriven>
    {
        public UniversalFrameDrivenAbility(AAUniversalFrameDriven abilityAsset) : base(abilityAsset)
        {

        }

        public override AbilitySpec CreateSpec(AbilitySystemComponent owner)
        {
            return new UniversalFrameDrivenAbilitySpec(this, owner);
        }
    }

    /// <summary>
    /// 通用帧驱动技能 - 完全遵循 FLF 设计理念
    ///
    /// 设计原则（基于 FLF specialattack.js）：
    /// 1. 单一通用类 - 所有技能使用同一个类
    /// 2. 状态驱动 - 通过 state 值查找处理函数
    /// 3. 事件系统 - TU、frame、hit、hit_others 等事件
    /// 4. 配置驱动 - 所有行为通过 frame_data.json 配置
    ///
    /// 使用方式：
    /// - 90% 的技能：只需配置 state 和 frame_data，不需要任何代码
    /// - 8% 的技能：通过 AbilityConfig ScriptableObject 配置音效、特效
    /// - 2% 的技能：通过 StateHandlers 字典注册自定义处理函数
    /// </summary>
    public class UniversalFrameDrivenAbilitySpec : AbilitySpec
    {
        // ==================== 核心组件 ====================

        /// <summary>
        /// LF2角色动画播放器
        /// </summary>
        protected LF2LivingObject Animator { get; private set; }

        /// <summary>
        /// 技能对应的状态值（301-999）
        /// </summary>
        protected int AbilityState { get; private set; }

        /// <summary>
        /// 技能起始帧ID
        /// </summary>
        protected int StartFrameId { get; private set; }

        // ==================== 状态处理器字典（类似 FLF 的 states 对象）====================

        /// <summary>
        /// 全局状态处理器字典 - 类似 FLF 的 states 对象
        /// 键：state 值（如 "generic", "300X", "3000"）
        /// 值：处理函数（返回 object - 可以是 bool 表示是否处理，或 null）
        /// </summary>
        private static Dictionary<string, Func<UniversalFrameDrivenAbilitySpec, AbilityEvent, object>> StateHandlers
            = new Dictionary<string, Func<UniversalFrameDrivenAbilitySpec, AbilityEvent, object>>();

        // ==================== 构造函数 ====================

        public UniversalFrameDrivenAbilitySpec(AbstractAbility ability,
            AbilitySystemComponent owner) : base(ability, owner)
        {
        }

        // ==================== 公开 API ====================

        /// <summary>
        /// 注册自定义状态处理器（供外部使用）
        /// </summary>
        /// <param name="stateKey">状态键（如 "3001", "generic", "300X"）</param>
        /// <param name="handler">处理函数（返回 true 阻止默认处理，返回 null 或 false 继续传播）</param>
        /// <example>
        /// <code>
        /// // 在 GameManager.Awake 中注册
        /// UniversalFrameDrivenAbilitySpec.RegisterStateHandler("3001", (ability, evt) => {
        ///     if (evt.type == AbilityEventType.HitOthers) {
        ///         // 自定义逻辑
        ///         return true;  // 阻止默认处理
        ///     }
        ///     return null;
        /// });
        /// </code>
        /// </example>
        public static void RegisterStateHandler(string stateKey, Func<UniversalFrameDrivenAbilitySpec, AbilityEvent, object> handler)
        {
            StateHandlers[stateKey] = handler;
        }

        /// <summary>
        /// 移除自定义状态处理器
        /// </summary>
        public static void UnregisterStateHandler(string stateKey)
        {
            StateHandlers.Remove(stateKey);
        }

        /// <summary>
        /// 创建通用帧驱动技能实例
        /// </summary>
        /// <param name="ability">技能数据</param>
        /// <param name="owner">技能拥有者</param>
        /// <param name="abilityState">技能状态值（301-999）</param>
        /// <param name="config">可选的技能配置（用于特效、音效等）</param>
        public UniversalFrameDrivenAbilitySpec(
            AbstractAbility ability,
            AbilitySystemComponent owner,
            int abilityState)
            : base(ability, owner)
        {
            // 验证状态值范围
            if (!LF2States.IsAbilityState(abilityState))
            {
                Debug.LogWarning($"[UniversalFrameDrivenAbility] State {abilityState} is not in ability range (301-999)!");
            }

            AbilityState = abilityState;

            // 获取 LF2LivingObject（优先从 Character Hub 获取）
            var character = owner.GetComponent<Character>();
            if (character?._LF2Character != null)
                Animator = character._LF2Character;
            
            if (Animator == null)
            {
                Debug.LogError($"[UniversalFrameDrivenAbility] LF2LivingObject not found on {owner.name}!");
                return;
            }

            // 查找技能起始帧ID
            StartFrameId = Animator.GetFirstFrameByState(abilityState);
            if (StartFrameId < 0)
            {
                Debug.LogError($"[UniversalFrameDrivenAbility] No frame found for state {abilityState}!");
            }

            // 注册默认状态处理器（首次使用时）
            if (StateHandlers.Count == 0)
            {
                RegisterDefaultStateHandlers();
            }
        }


        // ==================== 技能生命周期 ====================

        /// <summary>
        /// 激活技能
        /// </summary>
        public override void ActivateAbility()
        {
            if (Animator == null || StartFrameId < 0)
            {
                Debug.LogError($"[UniversalFrameDrivenAbility] Cannot activate ability - invalid animator or frame!");
                TryEndAbility();
                return;
            }

            // 1. 播放技能起始帧
            Animator.PlayFrameByID(StartFrameId);

            // 3. 触发 state_entry 事件
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.StateEntry,
                state = AbilityState,
                frameData = Animator.Frame.D
            });

            // 4. 应用消耗和冷却
            DoCost();
        }

        /// <summary>
        /// 技能 Tick - 每帧调用
        /// 对应 FLF 的 TU_update()
        /// </summary>
        protected override void AbilityTick()
        {
            base.AbilityTick();

            if (Animator == null) return;

            // 1. 触发 TU_Force 事件（对应 FLF 的 TU_force，用于强制应用帧力）
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.TU_Force,
                state = Animator.Frame.D.state,
                frameData = Animator.Frame.D
            });

            // 2. 触发 TU (Time Update) 事件（对应 FLF 的 TU）
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.TU,
                state = Animator.Frame.D.state,
                frameData = Animator.Frame.D
            });

            // 3. 检查是否返回到非技能状态（自动结束技能）
            int currentState = Animator.Frame.D.state;
            if (!LF2States.IsAbilityState(currentState))
            {
                TryEndAbility();
            }
        }

        /// <summary>
        /// 取消技能
        /// </summary>
        public override void CancelAbility()
        {
            if (Animator != null)
            {
                // 强制返回站立状态
                Animator.PlayFrameByID(LF2StandardFrames.Standing);
            }

            // 触发 state_exit 事件
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.StateExit,
                state = AbilityState,
                frameData = Animator?.Frame.D
            });
        }

        /// <summary>
        /// 结束技能
        /// </summary>
        public override void EndAbility()
        {
            // 触发 state_exit 事件
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.StateExit,
                state = AbilityState,
                frameData = Animator?.Frame.D
            });
        }

        // ==================== 事件系统（类似 FLF）====================

        /// <summary>
        /// 帧变化回调（对应 FLF 的 frame_update）
        /// 这是最核心的回调！当切换到新帧时触发
        /// </summary>
        /// <param name="frameId">新帧的 ID</param>
        private void OnFrameChanged(int frameId)
        {
            if (Animator == null) return;

            // 获取当前帧数据
            var frameData = Animator.Frame.D;
            if (frameData == null) return;

            // 1. 触发 Frame_Force 事件（对应 FLF 的 frame_force，用于强制应用帧力）
            bool frameForceHandled = DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.Frame_Force,
                state = Animator.Frame.D.state,
                frameData = frameData
            });

            // 2. 如果没有被处理，自动应用帧力（类似 FLF 的默认 frame_force）
            if (!frameForceHandled)
            {
                ApplyFrameForce(frameData);
            }

            // 3. 触发 Frame 事件（对应 FLF 的 frame）
            DispatchEvent(new AbilityEvent
            {
                type = AbilityEventType.Frame,
                state = Animator.Frame.D.state,
                frameData = frameData
            });
        }

        /// <summary>
        /// 状态变化回调
        /// </summary>
        private void OnStateChanged(int newState)
        {
            // 如果退出技能状态，自动结束技能
            if (!LF2States.IsAbilityState(newState))
            {
                TryEndAbility();
            }
        }

        /// <summary>
        /// 自动应用帧力（对应 FLF 的 frame_force）
        /// 这是 FLF 最核心的机制之一！
        ///
        /// Step D7: 优先使用 PhysicsState (ps)，Rigidbody2D 作为 fallback
        /// </summary>
        /// <param name="frameData">当前帧数据</param>
        private void ApplyFrameForce(LF2FrameData frameData)
        {
            if (frameData == null || Owner == null) return;

            // 获取角色朝向（1 = 右，-1 = 左）
            var character = Owner.GetComponent<Character>();
            //int dirH = character?._CharacterDirection == DIRECTION.RIGHT ? 1 : -1;
            int dirH = -1;

            // Step D7: 优先使用 LF2Character.ps（Plan B）
            LF2LivingObject animator = character._LF2Character;
            if (animator != null && animator.PS != null)
            {
                var ps = animator.PS;

                // ==================== 应用 dvx（水平速度）====================
                // 对应 FLF livingobject.js:135-142
                if (frameData.dvx == 550)
                {
                    // FLF 特殊值：550 表示停止移动
                    ps.vx = 0;
                }
                else if (frameData.dvx != 0)
                {
                    float currentVx = Mathf.Abs(ps.vx);

                    // FLF 逻辑：if ($.ps.y < 0 || avx < $.frame.D.dvx)
                    bool isInAir = ps.y < 0;  // FLF Y-axis: 负数为空中

                    if (isInAir || currentVx < frameData.dvx)
                    {
                        // 设置水平速度（考虑朝向）
                        ps.vx = dirH * frameData.dvx;
                    }

                    // FLF 特殊处理：dvx < 0 表示减速
                    if (frameData.dvx < 0)
                    {
                        ps.vx -= dirH;
                    }
                }

                // ==================== 应用 dvy（垂直速度）====================
                // 对应 FLF livingobject.js:145
                if (frameData.dvy == 550)
                {
                    // FLF 特殊值：550 表示停止
                    ps.vy = 0;
                }
                else if (frameData.dvy != 0)
                {
                    // FLF 语义：dvy 是 delta velocity（-dvy 因为 FLF Y 轴向下为正）
                    ps.vy += -frameData.dvy;
                }

                // ==================== 应用 dvz（深度速度）====================
                // 对应 FLF livingobject.js:144
                if (frameData.dvz != 0)
                {
                    // Step D7: 使用 tick 语义（离散推进），不使用 Time.fixedDeltaTime
                    // dvz 是每 tick 的深度速度变化
                    ps.vz += frameData.dvz;
                }

                return;  // Plan A 路径完成，不 fallback 到 Rigidbody2D
            }

            // ==================== Legacy Fallback: Rigidbody2D ====================
            var rb = Owner.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // 应用 dvx（水平速度）
            // 对应 FLF livingobject.js:135-142
            if (frameData.dvx != 0)
            {
                float currentVx = Mathf.Abs(rb.velocity.x);

                // FLF 逻辑：if ($.ps.y < 0 || avx < $.frame.D.dvx)
                bool isInAir = rb.velocity.y != 0;  // 简化判断：y速度不为0即为空中

                if (isInAir || currentVx < frameData.dvx)
                {
                    // 设置水平速度（考虑朝向）
                    rb.velocity = new Vector2(dirH * frameData.dvx, rb.velocity.y);
                }

                // FLF 特殊处理：dvx < 0 表示减速
                if (frameData.dvx < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x - dirH, rb.velocity.y);
                }
            }

            // FLF 特殊值：550 表示停止移动
            if (frameData.dvx == 550) rb.velocity = new Vector2(0, rb.velocity.y);
            if (frameData.dvy == 550) rb.velocity = new Vector2(rb.velocity.x, 0);

            // 应用 dvy（垂直速度）
            // 对应 FLF livingobject.js:145
            if (frameData.dvy != 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + frameData.dvy);
            }

            // 应用 dvz（深度速度）
            // 对应 FLF livingobject.js:144
            // Step D7: Legacy 路径不使用 Time.fixedDeltaTime（已移除）
            if (frameData.dvz != 0)
            {
                var unitActions = Owner.GetComponent<UnitActions>();
                if (unitActions != null)
                {
                    // Legacy 路径：直接累加 dvz（假设每帧调用）
                    unitActions.groundPos += frameData.dvz;
                }
            }
        }

        /// <summary>
        /// 分发事件到状态处理器
        /// 类似 FLF 的 state_update(event)
        /// </summary>
        /// <returns>返回事件是否被处理（任一处理器返回 true）</returns>
        private bool DispatchEvent(AbilityEvent abilityEvent)
        {
            bool handled = false;

            // 1. 调用 generic 处理器（所有技能共享）
            // 对应 FLF 的 $.states.generic.call($, event)
            if (StateHandlers.TryGetValue("generic", out var genericHandler))
            {
                var result = genericHandler?.Invoke(this, abilityEvent);
                if (result != null && result is bool boolResult && boolResult)
                    handled = true;
            }

            // 2. 调用 300X 处理器（所有 30XX 技能共享）
            // 对应 FLF specialattack.js 中的 states['300X']
            if (AbilityState >= 300 && AbilityState <= 399)
            {
                if (StateHandlers.TryGetValue("300X", out var baseHandler))
                {
                    var result = baseHandler?.Invoke(this, abilityEvent);
                    if (result != null && result is bool boolResult && boolResult)
                        handled = true;
                }
            }

            // 3. 调用特定 state 处理器（特殊技能逻辑）
            // 对应 FLF 的 $.states[$.frame.D.state].call($, event)
            string stateKey = AbilityState.ToString();
            if (StateHandlers.TryGetValue(stateKey, out var specificHandler))
            {
                var result = specificHandler?.Invoke(this, abilityEvent);
                if (result != null && result is bool boolResult && boolResult)
                    handled = true;
            }

            return handled;
        }

        // ==================== 默认状态处理器（类似 FLF 的 states 对象）====================

        /// <summary>
        /// 注册默认状态处理器
        /// 类似 FLF 的 states = { generic: ..., '300X': ..., '3000': ... }
        /// </summary>
        private static void RegisterDefaultStateHandlers()
        {
            // ==================== generic: 通用处理器（所有技能共享）====================
            // 对应 FLF specialattack.js:15-62 和 livingobject.js generic 处理
            StateHandlers["generic"] = (ability, evt) =>
            {
                switch (evt.type)
                {
                    // Frame 事件：切换到新帧时触发
                    case AbilityEventType.Frame:
                        // 1. 自动创建对象（opoint）
                        // 对应 FLF specialattack.js:28-30
                        if (evt.frameData?.opoint != null)
                        {
                            CreateObject(ability, evt.frameData.opoint);
                        }

                        // 2. 自动播放音效
                        // 对应 FLF specialattack.js:31-33
                        if (!string.IsNullOrEmpty(evt.frameData?.sound))
                        {
                            PlaySound(ability, evt.frameData.sound);
                        }
                        break;

                    // TU 事件：每帧更新
                    case AbilityEventType.TU:
                        // 处理交互（伤害检测等）
                        // 对应 FLF specialattack.js:18-20
                        // ability.interaction();
                        // ability.mech.dynamics();

                        // 处理 hit_a（减少 HP）
                        // 对应 FLF specialattack.js:22-24
                        if (evt.frameData?.hit_a != 0)
                        {
                            // var health = ability.Owner.GetComponent<HealthSystem>();
                            // if (health != null)
                            // {
                            //     health.CurrentHealth -= evt.frameData.hit_a;
                            // }
                        }
                        break;

                    // Hit 事件：被击中
                    case AbilityEventType.Hit:
                    case AbilityEventType.HitOthers:
                        // 播放击中音效
                        // 对应 FLF specialattack.js:54-56
                        // PlaySound(ability, "weapon_broken_sound");
                        break;

                    // Die 事件：死亡
                    case AbilityEventType.Die:
                        // 跳转到 hit_d 指定的帧
                        // 对应 FLF specialattack.js:58-60
                        if (evt.frameData?.hit_d != 0 && ability.Animator != null)
                        {
                            ability.Animator.PlayFrameByID(evt.frameData.hit_d);
                        }
                        break;

                    // Leaving 事件：离开场景边界
                    case AbilityEventType.Leaving:
                        // 销毁对象
                        // 对应 FLF specialattack.js:48-51
                        if (ability.Animator != null)
                        {
                            ability.Animator.PlayFrameByID(1000);  // 1000 = destroy
                        }
                        break;
                }

                // 调用 300X 处理器（对应 FLF specialattack.js:62）
                // 注意：这里不需要手动调用，DispatchEvent 会自动处理

                return null;  // 不阻止事件传播
            };

            // ==================== 300X: 基础技能状态处理器（301-399 共享）====================
            // 对应 FLF specialattack.js:69-99（Ball States）
            StateHandlers["300X"] = (ability, evt) =>
            {
                switch (evt.type)
                {
                    case AbilityEventType.TU:
                        var frameData = evt.frameData;
                        if (frameData == null) break;

                        // 追踪弹幕逻辑（hit_Fa = 1 或 2）
                        // 对应 FLF specialattack.js:72-92
                        if (frameData.hit_Fa == 1 || frameData.hit_Fa == 2)
                        {
                            // TODO: 实现完整的追踪逻辑
                            // 1. 调用 chase_target() 选择目标
                            // 2. 根据目标位置调整速度（vx, vz）
                            // 3. 切换朝向
                            Debug.Log($"[300X] TU: chasing target (hit_Fa={frameData.hit_Fa})");

                            // 简化示例：
                            var rb = ability.Owner.GetComponent<Rigidbody2D>();
                            if (rb != null)
                            {
                                // 最大 x 速度：14，加速度：0.7
                                // 最大 z 速度：2.2，加速度：0.4
                                // 这里需要根据目标位置计算
                            }
                        }
                        // 直线飞行（hit_Fa = 10）
                        // 对应 FLF specialattack.js:93-96
                        else if (frameData.hit_Fa == 10)
                        {
                            var rb = ability.Owner.GetComponent<Rigidbody2D>();
                            if (rb != null)
                            {
                                // 设置固定速度：17
                                int dir = rb.velocity.x > 0 ? 1 : -1;
                                rb.velocity = new Vector2(dir * 17, rb.velocity.y);
                            }
                        }
                        break;
                }

                return null;
            };

            // ==================== 可以在这里注册更多特定 state 的处理器 ====================

            // 示例：3000 - Ball Flying
            // StateHandlers["3000"] = (ability, evt) =>
            // {
            //     switch (evt.type)
            //     {
            //         case AbilityEventType.HitOthers:
            //             // 击中其他物体时的处理
            //             ability.Animator?.PlayFrameByID(10);  // 跳转到 hitting frame
            //             break;
            //         case AbilityEventType.Hit:
            //             // 被击中时的处理
            //             ability.Animator?.PlayFrameByID(20);  // 跳转到 hit frame
            //             return true;  // 阻止默认处理
            //     }
            //     return null;
            // };
        }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// 创建对象（对应 FLF 的 match.create_object）
        /// </summary>
        private static void CreateObject(UniversalFrameDrivenAbilitySpec ability, ObjectPoint opoint)
        {
            // TODO: 实现完整的对象创建逻辑
            // 1. 根据 opoint.oid 加载预制体
            // 2. 设置位置（考虑 opoint.x, y, facing）
            // 3. 设置速度（dvx, dvy）
            // 4. 设置朝向
            Debug.Log($"[CreateObject] oid={opoint.oid}, action={opoint.action}, x={opoint.x}, y={opoint.y}");
        }

        /// <summary>
        /// 播放音效（对应 FLF 的 match.sound.play）
        /// </summary>
        private static void PlaySound(UniversalFrameDrivenAbilitySpec ability, string soundName)
        {
            // TODO: 接入你的音频系统
            // AudioManager.Play(soundName);
            Debug.Log($"[PlaySound] {soundName}");
        }
    }

    // ==================== 辅助类 ====================

    /// <summary>
    /// 技能事件类型（类似 FLF 的事件系统）
    /// </summary>
    public enum AbilityEventType
    {
        // 核心事件（对应 FLF livingobject.js）
        TU,              // Time Update - 每帧更新（对应 FLF 的 TU）
        TU_Force,        // Time Update 强制阶段 - 应用帧力（对应 FLF 的 TU_force）
        Frame,           // 切换到新帧时触发（对应 FLF 的 frame）
        Frame_Force,     // Frame 强制阶段 - 应用帧力（对应 FLF 的 frame_force）
        Transit,         // 帧转换时触发（对应 FLF 的 transit）

        // 状态事件
        StateEntry,      // 状态进入（对应 FLF 的 state_entry）
        StateExit,       // 状态退出（对应 FLF 的 state_exit）

        // 交互事件
        Hit,             // 被击中时触发（对应 FLF 的 hit）
        HitOthers,       // 击中他人时触发（对应 FLF 的 hit_others）

        // 物理事件
        FellOntoGround,  // 已落地（对应 FLF 的 fell_onto_ground）
        FallOntoGround,  // 即将落地（对应 FLF 的 fall_onto_ground）

        // 连招事件
        Combo,           // 连招输入（对应 FLF 的 combo）
        PostCombo,       // 连招后处理（对应 FLF 的 post_combo）
        PreInteraction,  // 交互前处理（对应 FLF character.js 的 pre_interaction）
        PostInteraction, // 交互后处理（对应 FLF character.js 的 post_interaction）

        // 其他
        Leaving,         // 离开场景边界（对应 FLF 的 leaving）
        Die,             // 死亡（对应 FLF 的 die）
        Destroy          // 销毁（对应 FLF 的 destroy）
    }

    /// <summary>
    /// 技能事件数据（对应 FLF 的 event 参数和上下文）
    /// </summary>
    public class AbilityEvent
    {
        // 基础信息
        public AbilityEventType type;           // 事件类型
        public int state;                       // 当前状态值
        public LF2FrameData frameData;          // 当前帧数据
        public LF2FrameData previousFrameData;  // 上一帧数据（用于判断帧转换）

        // 交互信息（hit/hit_others 事件使用）
        public InteractionArea itr;             // 交互区域（ITR）
        public GameObject attacker;             // 攻击者
        public GameObject target;               // 目标
        public Vector3 attackerPosition;        // 攻击者位置
        public Bounds hitRect;                  // 击中区域

        // 连招信息（combo 事件使用）
        public string comboKey;                 // 连招键（如 "D>A", "D>J"）

        // 物理信息（落地事件使用）
        public float fallSpeed;                 // 落地速度
        public Vector2 groundPosition;          // 落地位置
    }

    /// <summary>
    /// 可选的技能配置 ScriptableObject
    /// 用于配置音效、特效等（无需编写代码）
    /// </summary>
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "NTSD/Ability Config")]
    public class AbilityConfig : ScriptableObject
    {
        [Header("音效配置")]
        public AudioClip startSound;
        public AudioClip[] frameSounds;  // 按帧索引播放

        [Header("特效配置")]
        public GameObject startVFX;
        public GameObject[] frameVFX;    // 按帧索引生成

        [Header("镜头效果")]
        public bool enableCameraShake;
        public float shakeDuration = 0.2f;
        public float shakeIntensity = 0.1f;

        /// <summary>
        /// 处理事件（播放音效、特效等）
        /// </summary>
        public void HandleEvent(UniversalFrameDrivenAbility ability, AbilityEvent evt)
        {
            switch (evt.type)
            {
                case AbilityEventType.StateEntry:
                    // 播放开始音效
                    if (startSound != null)
                    {
                        // TODO: AudioManager.Play(startSound)
                        Debug.Log($"[AbilityConfig] Play start sound: {startSound.name}");
                    }

                    // 生成开始特效
                    if (startVFX != null)
                    {
                        // TODO: VFXManager.PlayEffect(startVFX, position)
                        Debug.Log($"[AbilityConfig] Play start VFX: {startVFX.name}");
                    }

                    // 镜头震动
                    if (enableCameraShake)
                    {
                        // TODO: CameraShake.Shake(duration, intensity)
                        Debug.Log($"[AbilityConfig] Camera shake: {shakeDuration}s, {shakeIntensity}");
                    }
                    break;

                case AbilityEventType.Frame:
                    // 播放帧音效
                    int frameIndex = evt.frameData.frameId;
                    if (frameSounds != null && frameIndex < frameSounds.Length && frameSounds[frameIndex] != null)
                    {
                        // TODO: AudioManager.Play(frameSounds[frameIndex])
                        Debug.Log($"[AbilityConfig] Play frame sound at {frameIndex}: {frameSounds[frameIndex].name}");
                    }

                    // 生成帧特效
                    if (frameVFX != null && frameIndex < frameVFX.Length && frameVFX[frameIndex] != null)
                    {
                        // TODO: VFXManager.PlayEffect(frameVFX[frameIndex], position)
                        Debug.Log($"[AbilityConfig] Play frame VFX at {frameIndex}: {frameVFX[frameIndex].name}");
                    }
                    break;
            }
        }
    }
}
