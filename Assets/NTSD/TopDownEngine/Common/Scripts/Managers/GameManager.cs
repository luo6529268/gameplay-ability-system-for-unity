using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;
using static MoreMountains.Tools.MMSceneLoadingManager;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// TopDown Engine基础事件类型枚举
    /// 包含游戏中可能触发的基础事件
    /// </summary>
    public enum TopDownEngineEventTypes
    {
        SpawnCharacterStarts,  // 角色生成开始
        LevelStart,           // 关卡开始
        LevelComplete,        // 关卡完成
        LevelEnd,            // 关卡结束
        Pause,               // 暂停
        UnPause,             // 取消暂停
        PlayerDeath,         // 玩家死亡
        SpawnComplete,       // 生成完成
        RespawnStarted,      // 重生开始
        RespawnComplete,     // 重生完成
        StarPicked,          // 拾取星星
        GameOver,            // 游戏结束
        CharacterSwap,       // 角色交换
        CharacterSwitch,     // 角色切换
        Repaint,             // 重绘UI
        TogglePause,         // 切换暂停状态
        LoadNextScene,       // 加载下一场景
        PauseNoMenu,         // 无菜单暂停
        ShootAmo,            // 射击弹药
        InitEnemyInfo,       // 初始化敌人信息
    }

    /// <summary>
    /// 用于触发游戏事件的结构体
    /// 包含事件类型、触发角色和数值
    /// </summary>
    public struct TopDownEngineEvent
    {
        public TopDownEngineEventTypes EventType;  // 事件类型
        public Character OriginCharacter;          // 触发事件的角色
        public float Value;                        // 事件相关数值

        /// <summary>
        /// 初始化TopDownEngineEvent结构体
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="originCharacter">触发角色</param>
        /// <param name="_Value">事件数值</param>
        public TopDownEngineEvent(TopDownEngineEventTypes eventType, Character originCharacter = null, float _Value = 0)
        {
            EventType = eventType;
            OriginCharacter = originCharacter;
            this.Value = _Value;
        }

        static TopDownEngineEvent e;
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="originCharacter">触发角色</param>
        /// <param name="_Value">事件数值</param>
        public static void Trigger(TopDownEngineEventTypes eventType, Character originCharacter = null, float _Value = 0)
        {
            e.EventType = eventType;
            e.OriginCharacter = originCharacter;
            e.Value = _Value;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 分数修改方法枚举
    /// </summary>
    public enum PointsMethods
    {
        Add,  // 添加分数
        Set   // 设置分数
    }

    /// <summary>
    /// 用于触发分数变化事件的结构体
    /// </summary>
    public struct TopDownEnginePointEvent
    {
        public PointsMethods PointsMethod;  // 分数修改方法
        public int Points;                 // 分数值

        /// <summary>
        /// 初始化TopDownEnginePointEvent结构体
        /// </summary>
        /// <param name="pointsMethod">分数修改方法</param>
        /// <param name="points">分数值</param>
        public TopDownEnginePointEvent(PointsMethods pointsMethod, int points)
        {
            PointsMethod = pointsMethod;
            Points = points;
        }

        static TopDownEnginePointEvent e;
        /// <summary>
        /// 触发分数事件
        /// </summary>
        /// <param name="pointsMethod">分数修改方法</param>
        /// <param name="points">分数值</param>
        public static void Trigger(PointsMethods pointsMethod, int points)
        {
            e.PointsMethod = pointsMethod;
            e.Points = points;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 暂停方法枚举
    /// </summary>
    public enum PauseMethods
    {
        PauseMenu,    // 带暂停菜单的暂停
        NoPauseMenu   // 无菜单的暂停
    }

    /// <summary>
    /// 用于存储关卡进入点的类
    /// 每个关卡对应一个进入点
    /// </summary>
    public class PointsOfEntryStorage
    {
        public string LevelName;          // 关卡名称
        public int PointOfEntryIndex;     // 进入点索引

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="levelName">关卡名称</param>
        /// <param name="pointOfEntryIndex">进入点索引</param>
        public PointsOfEntryStorage(string levelName, int pointOfEntryIndex)
        {
            LevelName = levelName;
            PointOfEntryIndex = pointOfEntryIndex;
        }
    }

    /// <summary>
    /// 游戏管理器类
    /// 处理游戏的核心功能，包括分数、生命值、暂停等
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Game Manager")]
    public class GameManager : MMPersistentSingleton<GameManager>, 
        MMEventListener<MMGameEvent>, 
        MMEventListener<TopDownEngineEvent>, 
        MMEventListener<TopDownEnginePointEvent>,
        MMEventListener<LoadingSceneEvent>
    {
        [Header("Settings")]
        [Tooltip("游戏的目标帧率")]
        public int TargetFrameRate = 300;  // 游戏目标帧率

        [Header("Lives")]
        [Tooltip("角色当前可以拥有的最大生命值数量")]
        public int MaximumLives = 0;       // 最大生命值
        [Tooltip("当前的生命值数量")]
        public int CurrentLives = 0;       // 当前生命值

        [Header("Bindings")]
        [Tooltip("当所有生命值耗尽时重定向到的场景名称")]
        public string GameOverScene;       // 游戏结束场景

        [Header("Points")]
        [MMReadOnly]
        [Tooltip("当前的game points数量")]
        public int Points;                 // 当前分数

        [Header("Pause")]
        [Tooltip("如果为真，打开库存时游戏将自动暂停")]
        public bool PauseGameWhenInventoryOpens = true;  // 打开库存时是否暂停

        // 私有字段
        private SAP2DPathfinder _sap2DPath;  // 寻路系统
        private Dictionary<string, Inventory> _totalInventory;  // 总库存
        protected bool _inventoryOpen = false;  // 库存是否打开
        protected bool _pauseMenuOpen = false;  // 暂停菜单是否打开
        protected InventoryInputManager _inventoryInputManager;  // 库存输入管理器
        protected int _initialMaximumLives;  // 初始最大生命值
        protected int _initialCurrentLives;  // 初始当前生命值

        // 公共属性
        public virtual bool Paused { get; set; }  // 游戏是否暂停
        public virtual bool StoredLevelMapPosition { get; set; }  // 是否存储了关卡地图位置
        public virtual Vector2 LevelMapPosition { get; set; }  // 关卡地图位置
        public virtual Character PersistentCharacter { get; set; }  // 持久化角色
        public virtual Character StoredCharacter { get; set; }  // 存储的角色
        public List<PointsOfEntryStorage> PointsOfEntry;  // 进入点列表
        public Dictionary<string, Inventory> TotalInventory { get { return _totalInventory; } }  // 总库存属性

        /// <summary>
        /// 初始化方法
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            // 初始化进入点列表
            PointsOfEntry = new List<PointsOfEntryStorage>();
            // 初始化库存字典
            _totalInventory = new Dictionary<string, Inventory>();
        }

        /// <summary>
        /// 开始方法
        /// </summary>
        protected virtual void Start()
        {
            // 设置目标帧率
            Application.targetFrameRate = TargetFrameRate;
            // 保存初始生命值
            _initialCurrentLives = CurrentLives;
            _initialMaximumLives = MaximumLives;
            // 添加库存信息
            OnAddInventoryInfo();
        }

        /// <summary>
        /// 重置游戏管理器
        /// </summary>
        public virtual void Reset()
        {
            Points = 0;
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
            Paused = false;
        }

        /// <summary>
        /// 减少生命值
        /// </summary>
        public virtual void LoseLife()
        {
            CurrentLives--;
        }

        /// <summary>
        /// 增加生命值
        /// </summary>
        /// <param name="lives">要增加的生命值数量</param>
        public virtual void GainLives(int lives)
        {
            CurrentLives += lives;
            if (CurrentLives > MaximumLives)
            {
                CurrentLives = MaximumLives;
            }
        }

        /// <summary>
        /// 添加生命值
        /// </summary>
        /// <param name="lives">要添加的生命值数量</param>
        /// <param name="increaseCurrent">是否增加当前生命值</param>
        public virtual void AddLives(int lives, bool increaseCurrent)
        {
            MaximumLives += lives;
            if (increaseCurrent)
            {
                CurrentLives += lives;
            }
        }

        /// <summary>
        /// 重置生命值到初始值
        /// </summary>
        public virtual void ResetLives()
        {
            CurrentLives = _initialCurrentLives;
            MaximumLives = _initialMaximumLives;
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        /// <param name="pointsToAdd">要添加的分数</param>
        public virtual void AddPoints(int pointsToAdd)
        {
            Points += pointsToAdd;
        }

        /// <summary>
        /// 设置分数
        /// </summary>
        /// <param name="points">要设置的分数值</param>
        public virtual void SetPoints(int points)
        {
            Points = points;
        }

        /// <summary>
        /// 设置库存输入管理器状态
        /// </summary>
        /// <param name="status">是否启用</param>
        protected virtual void SetActiveInventoryInputManager(bool status)
        {
            _inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager>();
            if (_inventoryInputManager != null)
            {
                _inventoryInputManager.enabled = status;
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        /// <param name="pauseMethod">暂停方法</param>
        /// <param name="unpauseIfPaused">如果已暂停是否取消暂停</param>
        public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu, bool unpauseIfPaused = true)
        {
            // 如果库存打开且使用暂停菜单方法，直接返回
            if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen)
            {
                return;
            }

            // 如果游戏未暂停
            if (Time.timeScale > 0.0f)
            {
                // 触发时间缩放事件，暂停游戏
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
                Instance.Paused = true;
                
                // 如果使用无菜单方法，标记库存为打开状态
                if (pauseMethod == PauseMethods.NoPauseMenu)
                {
                    _inventoryOpen = true;
                }
            }
            else if (unpauseIfPaused)
            {
                // 如果已暂停且允许取消暂停，则取消暂停
                UnPause(pauseMethod);
            }
        }

        /// <summary>
        /// 取消暂停游戏
        /// </summary>
        /// <param name="pauseMethod">暂停方法</param>
        public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            // 触发时间缩放事件，恢复游戏
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            Instance.Paused = false;
            
            // 重置库存打开状态
            if (_inventoryOpen)
            {
                _inventoryOpen = false;
            }
        }

        /// <summary>
        /// 获取指定关卡的进入点信息
        /// </summary>
        /// <param name="levelName">关卡名称</param>
        /// <returns>进入点信息</returns>
        public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName)
        {
            if (PointsOfEntry.Count > 0)
            {
                foreach (PointsOfEntryStorage point in PointsOfEntry)
                {
                    if (point.LevelName == levelName)
                    {
                        return point;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 清除指定关卡的进入点信息
        /// </summary>
        /// <param name="levelName">关卡名称</param>
        public virtual void ClearPointOfEntry(string levelName)
        {
            if (PointsOfEntry.Count > 0)
            {
                foreach (PointsOfEntryStorage point in PointsOfEntry)
                {
                    if (point.LevelName == levelName)
                    {
                        PointsOfEntry.Remove(point);
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有进入点信息
        /// </summary>
        public virtual void ClearAllPointsOfEntry()
        {
            PointsOfEntry.Clear();
        }

        /// <summary>
        /// 重置所有存档
        /// </summary>
        public virtual void ResetAllSaves()
        {
            MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
            MMSaveLoadManager.DeleteSaveFolder("TopDownEngine");
            MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
        }

        /// <summary>
        /// 存储选定的角色
        /// </summary>
        /// <param name="selectedCharacter">要存储的角色</param>
        public virtual void StoreSelectedCharacter(Character selectedCharacter)
        {
            StoredCharacter = selectedCharacter;
        }

        /// <summary>
        /// 清除存储的角色
        /// </summary>
        public virtual void ClearSelectedCharacter()
        {
            StoredCharacter = null;
        }

        /// <summary>
        /// 设置持久化角色
        /// </summary>
        /// <param name="newCharacter">新的持久化角色</param>
        public virtual void SetPersistentCharacter(Character newCharacter)
        {
            PersistentCharacter = newCharacter;
        }

        /// <summary>
        /// 销毁持久化角色
        /// </summary>
        public virtual void DestroyPersistentCharacter()
        {
            if (PersistentCharacter != null)
            {
                Destroy(PersistentCharacter.gameObject);
                SetPersistentCharacter(null);
            }
        }

        /// <summary>
        /// 根据名称获取库存
        /// </summary>
        /// <param name="Name">库存名称</param>
        /// <returns>库存对象</returns>
        public virtual Inventory OnGetInventoryByName(string Name)
        {
            Inventory inventory = null;
            if (!_totalInventory.ContainsKey(Name))
            {
                OnAddInventoryInfo();
            }

            _totalInventory.TryGetValue(Name, out inventory);
            return inventory;
        }

        /// <summary>
        /// 添加库存信息
        /// </summary>
        void OnAddInventoryInfo()
        {
            foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
            {
                _totalInventory.TryAdd(inventory.name, inventory);
            }
        }

        /// <summary>
        /// 处理MMGameEvent事件
        /// </summary>
        /// <param name="gameEvent">游戏事件</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            switch (gameEvent.EventName)
            {
                case "inventoryOpens":
                    if (PauseGameWhenInventoryOpens)
                    {
                        Pause(PauseMethods.NoPauseMenu, false);
                    }
                    break;

                case "inventoryCloses":
                    if (PauseGameWhenInventoryOpens)
                    {
                        UnPause(PauseMethods.NoPauseMenu);
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理TopDownEngineEvent事件
        /// </summary>
        /// <param name="engineEvent">引擎事件</param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.TogglePause:
                    if (Paused)
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
                    }
                    else
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    }
                    break;
                case TopDownEngineEventTypes.Pause:
                    Pause();
                    break;
                case TopDownEngineEventTypes.UnPause:
                    UnPause();
                    break;
                case TopDownEngineEventTypes.PauseNoMenu:
                    Pause(PauseMethods.NoPauseMenu, false);
                    break;
            }
        }

        /// <summary>
        /// 处理TopDownEnginePointEvent事件
        /// </summary>
        /// <param name="pointEvent">分数事件</param>
        public virtual void OnMMEvent(TopDownEnginePointEvent pointEvent)
        {
            switch (pointEvent.PointsMethod)
            {
                case PointsMethods.Set:
                    SetPoints(pointEvent.Points);
                    break;

                case PointsMethods.Add:
                    AddPoints(pointEvent.Points);
                    break;
            }
        }

        /// <summary>
        /// 启用时开始监听事件
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<TopDownEngineEvent>();
            this.MMEventStartListening<TopDownEnginePointEvent>();
            this.MMEventStartListening<LoadingSceneEvent>();
        }

        /// <summary>
        /// 禁用时停止监听事件
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<TopDownEngineEvent>();
            this.MMEventStopListening<TopDownEnginePointEvent>();
            this.MMEventStopListening<LoadingSceneEvent>();
        }

        /// <summary>
        /// 处理场景加载事件
        /// </summary>
        /// <param name="eventType">场景加载事件</param>
        public void OnMMEvent(LoadingSceneEvent eventType)
        {
            if (eventType.Status != LoadingStatus.UnloadSceneLoader)
                return;

            _totalInventory.Clear();
            OnAddInventoryInfo();
        }
    }
}
