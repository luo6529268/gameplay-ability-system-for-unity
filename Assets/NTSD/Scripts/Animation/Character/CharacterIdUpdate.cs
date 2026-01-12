using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// Character 的 id_update 管理器（Step D9R）
    ///
    /// 职责：
    /// - 对应 FLF 的 $.id_update(...) 方法
    /// - 作为 Character Hub 的子模块，负责调用角色特定逻辑
    /// - 使用实例字典管理 handlers（替代静态 Registry）
    ///
    /// 设计原则（D9R 重构）：
    /// - 宿主：Character Hub（方案 B）
    /// - hookName：string（贴近 FLF）
    /// - 实例字典：Dictionary<string, IdUpdateHandler> _handlers
    /// - RegisterDefaultHandlers(characterId)：类似 CharacterStates.RegisterDefaultHandlers
    /// - 默认无 handler 时返回 false（不改变现有行为）
    /// - 不引入 per-tick GC（复用 struct context）
    ///
    /// 使用方式：
    /// 1. Character.Initialization() 中创建：_IdUpdate = new CharacterIdUpdate(this)
    /// 2. 调用 _IdUpdate.RegisterDefaultHandlers(characterId) 注册该角色的 hooks
    /// 3. 通用逻辑中调用：if (character.IdUpdate.TryInvoke("generic_combo", ctx)) return;
    /// 4. 返回 true = 角色特例已处理，阻止默认逻辑
    /// 5. 返回 false = 未处理，继续默认逻辑
    /// </summary>
    public class CharacterIdUpdate
    {
        // ==================== Handler 委托定义 ====================

        /// <summary>
        /// id_update handler 委托
        /// </summary>
        /// <param name="ctx">上下文（只读引用）</param>
        /// <returns>true = 已处理（阻止默认逻辑），false = 未处理（继续默认逻辑）</returns>
        public delegate bool IdUpdateHandler(in IdUpdateContext ctx);

        // ==================== 依赖注入（Hub 注入规则）====================

        /// <summary>
        /// 宿主 Character Hub（只读引用）
        /// </summary>
        private readonly Character _hub;

        /// <summary>
        /// 缓存的 LF2CharacterAnimator（避免每次 TryInvoke 都查找）
        /// </summary>
        private readonly LF2CharacterAnimator _animator;

        /// <summary>
        /// 缓存的 PhysicsState（避免每次 TryInvoke 都查找）
        /// </summary>
        private readonly PhysicsState _ps;

        // ==================== 实例字典（Step D9R: 替代静态 Registry）====================

        /// <summary>
        /// Hook handlers 实例字典（hookName → handler）
        /// 对应 FLF 的 this.id_updates[characterId]
        /// 在 RegisterDefaultHandlers() 中填充
        /// </summary>
        private readonly Dictionary<string, IdUpdateHandler> _handlers = new Dictionary<string, IdUpdateHandler>();

        // ==================== 构造函数 ====================

        /// <summary>
        /// 构造函数（由 Character.Initialization() 调用）
        /// Step D9R: 所有依赖通过 Hub 注入，禁止 GetComponent
        /// </summary>
        /// <param name="hub">Character Hub</param>
        public CharacterIdUpdate(Character hub)
        {
            _hub = hub;

            // 缓存常用引用（Hub 注入规则：由 Character 统一缓存后注入）
            _animator = hub._LF2CharacterAnimator;
            _ps = _animator?.ps;

            if (_animator == null)
            {
                Debug.LogWarning($"[CharacterIdUpdate] Character {hub.name} has no LF2CharacterAnimator! id_update will always return false.");
            }
        }

        // ==================== RegisterDefaultHandlers（Step D9R 核心）====================

        /// <summary>
        /// 注册角色特定 handlers（对应 FLF character.js 的 id_updates[characterId] 初始化）
        ///
        /// FLF 语义：
        /// - id_updates[characterId] 只有特定角色有内容（例如 Deep/Davis/Rudolf）
        /// - 绝大多数角色没有任何 id_update handler（_handlers 保持为空）
        /// - 当 TryInvoke 找不到 handler 时返回 false，执行默认逻辑
        ///
        /// 参考 FLF character.js lines 1413-1590：
        /// - id_updates[1]: Deep 的特殊连招逻辑
        /// - id_updates[5]: Davis 的 TU handler
        /// - id_updates[11]: Rudolf 的变身逻辑
        /// - 其余角色：未定义（空）
        ///
        /// Phase 1: 骨架实现（所有角色暂无 handler，未来逐步迁移）
        /// </summary>
        /// <param name="characterId">角色配置ID（对应 FLF 的 id_updates[xxx] key）</param>
        public void RegisterDefaultHandlers(int characterId)
        {
            // Step D9R: 每次注册前清空，避免重复注册
            _handlers.Clear();

            // 按角色ID分发注册（对应 FLF 的 id_updates[characterId] 结构）
            switch (characterId)
            {
                case CharacterIds.Deep:  // id_updates[1]: Deep
                    // Future Phase 2: 迁移 Deep 的特殊连招逻辑
                    // _handlers[IdUpdateHooks.GenericCombo] = DeepGenericComboHandler;
                    break;

                case CharacterIds.Davis:  // id_updates[5]: Davis
                    // Future Phase 2: 迁移 Davis 的 TU handler
                    // _handlers[IdUpdateHooks.TU] = DavisTUHandler;
                    break;

                case CharacterIds.Rudolf:  // id_updates[11]: Rudolf
                    // Future Phase 2: 迁移 Rudolf 的变身逻辑
                    // _handlers[IdUpdateHooks.GenericCombo] = RudolfGenericComboHandler;
                    // _handlers["revert_transform"] = RudolfRevertTransformHandler;
                    break;

                default:
                    // FLF 语义：其余角色没有任何 id_update handler
                    // _handlers 保持为空，TryInvoke 直接返回 false
                    break;
            }

            // Debug 日志：显示注册的 handler 数量
            if (_handlers.Count > 0)
            {
                Debug.Log($"[CharacterIdUpdate] RegisterDefaultHandlers for characterId={characterId}, registered {_handlers.Count} hooks");
            }
            // 注意：绝大多数角色 _handlers.Count = 0，这是正常且符合 FLF 语义的
        }

        // ==================== 核心 API ====================

        /// <summary>
        /// 尝试调用 id_update handler（对应 FLF 的 $.id_update(hookName, ...)）
        ///
        /// Step D9R: 从实例字典查找，不再使用静态 Registry
        /// </summary>
        /// <param name="hookName">Hook 名称（例如 "generic_combo"）</param>
        /// <param name="ctx">上下文（struct，避免 GC）</param>
        /// <returns>true = 角色特例已处理（阻止默认逻辑），false = 未处理（继续默认逻辑）</returns>
        public bool TryInvoke(string hookName, in IdUpdateContext ctx)
        {
            // 1. 验证基本条件
            if (_hub == null || _animator == null)
            {
                // 无效状态，不调用任何 handler
                return false;
            }

            // 2. 从实例字典查找 handler（Step D9R: 不再查 Registry）
            if (!_handlers.TryGetValue(hookName, out var handler))
            {
                // 未注册 handler，默认行为（不阻止默认逻辑）
                return false;
            }

            // 3. 调用 handler
            try
            {
                bool handled = handler(in ctx);
                return handled;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[CharacterIdUpdate] Exception in handler (characterId={_hub.CharacterID}, {hookName}): {ex}");
                return false;  // 异常时不阻止默认逻辑
            }
        }

        // ==================== 便捷重载（避免每次手动构造 context）====================



        /// <summary>
        /// generic_combo hook 的便捷调用
        /// </summary>
        public bool TryInvokeGenericCombo(string comboKey, string comboTag, int targetFrame, int tickIndex = 0)
        {
            var ctx = new IdUpdateContext(
                _hub,
                _animator,
                _ps,
                comboKey,
                comboTag,
                targetFrame,
                tickIndex
            );
            return TryInvoke(IdUpdateHooks.GenericCombo, in ctx);
        }

        /// <summary>
        /// state_entry hook 的便捷调用
        /// </summary>
        public bool TryInvokeStateEntry(int state, int tickIndex = 0)
        {
            var ctx = new IdUpdateContext(
                _hub,
                _animator,
                _ps,
                state,
                tickIndex
            );
            return TryInvoke(IdUpdateHooks.StateEntry, in ctx);
        }

        /// <summary>
        /// state_exit hook 的便捷调用
        /// </summary>
        public bool TryInvokeStateExit(int state, int tickIndex = 0)
        {
            var ctx = new IdUpdateContext(
                _hub,
                _animator,
                _ps,
                state,
                tickIndex
            );
            return TryInvoke(IdUpdateHooks.StateExit, in ctx);
        }

        /// <summary>
        /// 通用 hook 的便捷调用（frame_force/TU/hit_stop 等）
        /// </summary>
        public bool TryInvokeGeneric(string hookName, int tickIndex = 0)
        {
            var ctx = new IdUpdateContext(
                _hub,
                _animator,
                _ps,
                tickIndex
            );
            return TryInvoke(hookName, in ctx);
        }

        // ==================== 角色特定 Handlers（Future Phase 2+）====================
        //
        // 说明：
        // - 只有特定角色需要覆盖默认行为时，才在此添加对应的 handler 方法
        // - 大多数角色不需要任何 handler（_handlers 保持为空）
        // - 示例结构（未来迁移时使用）：
        //
        // private bool DeepGenericComboHandler(in IdUpdateContext ctx)
        // {
        //     if (ctx.ComboKey == "D>A")
        //     {
        //         // Deep 的 D>A 特殊处理
        //         return true;  // 拦截默认逻辑
        //     }
        //     return false;  // 其他连招使用默认逻辑
        // }
        //
        // private bool DavisTUHandler(in IdUpdateContext ctx)
        // {
        //     // Davis 的每帧特殊逻辑
        //     return false;
        // }
    }
}
