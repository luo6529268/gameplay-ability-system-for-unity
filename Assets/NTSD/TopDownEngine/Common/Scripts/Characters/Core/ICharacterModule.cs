using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 方案C：使用角色作为模块容器，并采用分阶段生命周期管理
    /// 
    /// Unity仍然会调用MonoBehaviour的Awake/Start方法，但所有游戏相关的初始化都应该通过Character
    /// 按照这些阶段来驱动，以避免不确定的执行顺序并支持运行时CharacterID的变更
    /// </summary>
    public interface ICharacterModule
    {
        /// <summary>
        /// 当Character收集模块时调用一次（通常在Character.Awake中）
        /// 应该只用于缓存引用和设置轻量级的不变量
        /// </summary>
        void ModuleSetup(Character character);

        /// <summary>
        /// 当Character初始化时调用一次（在任何数据绑定之前）
        /// 不应该读取或依赖于CharacterID驱动的数据
        /// </summary>
        void ModuleInitialize();

        /// <summary>
        /// 当Character绑定或重新绑定其CharacterID驱动的数据时调用
        /// 必须是幂等的/可重入的（支持运行时转换）
        /// </summary>
        void ModuleBind();

        /// <summary>
        /// 在重新绑定之前调用（当CharacterID在运行时更改时）
        /// 应该释放CharacterID驱动的数据，并在需要时分离订阅
        /// </summary>
        void ModuleUnbind();
    }
}

