using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 泛型单例模式基类，用于Unity中实现单例模式
    /// 继承自MonoBehaviour，确保可以在游戏对象中使用
    /// </summary>
    /// <typeparam name="T">需要实现单例的组件类型</typeparam>
    public class MMSingleton<T> : MonoBehaviour where T : Component
    {
        // 单例实例的静态变量
        protected static T _instance;

        // 检查是否存在单例实例
        public static bool HasInstance => _instance != null;

        // 尝试获取单例实例，如果不存在则返回null
        public static T TryGetInstance() => HasInstance ? _instance : null;

        // 获取当前单例实例
        public static T Current => _instance;

        /// <summary>
        /// 单例模式的属性访问器
        /// 如果实例不存在，会自动查找或创建新的实例
        /// </summary>
        public static T Instance
        {
            get
            {
                // 如果实例为空，尝试在场景中查找
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    // 如果场景中没找到，创建新的游戏对象并添加组件
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        // 设置新游戏对象的名称为类型名加上"_AutoCreated"后缀
                        obj.name = typeof(T).Name + "_AutoCreated";
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Unity的Awake方法，在对象初始化时调用
        /// 用于初始化单例实例
        /// 如果需要在子类中重写Awake，确保调用base.Awake()
        /// </summary>
        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        /// <summary>
        /// 初始化单例实例的方法
        /// 确保只在游戏运行时进行初始化
        /// </summary>
        protected virtual void InitializeSingleton()
        {
            // 如果不在游戏运行状态，直接返回
            if (!Application.isPlaying)
            {
                return;
            }

            // 将当前对象赋值给单例实例
            _instance = this as T;
        }
    }

}