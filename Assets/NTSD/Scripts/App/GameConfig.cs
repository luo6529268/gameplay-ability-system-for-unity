using UnityEngine;

namespace NTSD.App
{
    /// <summary>
    /// 游戏全局配置
    /// 存储角色选择界面、倒计时、队伍等相关的配置数据
    /// 使用 ScriptableObject 便于在编辑器中配置和热更新
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "NTSD/Game Config")]
    public class GameConfig : ScriptableObject
    {
        #region 单例访问

        private static GameConfig _instance;

        /// <summary>
        /// 全局单例访问点
        /// 需要在游戏启动时由 AppManager 或其他初始化脚本设置
        /// </summary>
        public static GameConfig Instance
        {
            get => _instance;
            set
            {
                if (_instance == null)
                {
                    _instance = value;
                }
            }
        }

        #endregion

        #region 对象池配置

        [Header("对象池配置")]
        [Tooltip("阴影 Prefab（挂载 SpriteRenderer，所有对象生成时自动添加阴影子节点）")]
        public GameObject ShadowPrefab;
        [Tooltip("LF2Object Prefab（可选，不填则运行时动态创建 GameObject）")]
        public GameObject LF2ObjectPrefab;
        [Tooltip("对象池预热数量（0=完全懒加载）")]
        public int PoolInitialSize = 0;
        [Tooltip("对象池最大容量（对齐反汇编 SceneManager_Init 预分配 400 实体）")]
        public int PoolMaxSize = 400;
        [Tooltip("空闲对象超时销毁时间（秒）")]
        public float PoolExpireTimeSeconds = 120f;
        [Tooltip("超时检查间隔（秒）")]
        public float PoolCheckIntervalSeconds = 10f;
        [Tooltip("Spark SpriteRenderer 桶预热数量")]
        public int PoolInitialSpritePoolSize = 16;

        [Header("Battle Stage Runtime")]
        [Tooltip("Unity 战斗场景逻辑宽度（像素）。仅使用 Unity 配置，不读取 C++ 背景 dat。")]
        public int BattleStageWidthPx = 800;
        [Tooltip("Unity 战斗场景逻辑 Z 最小值（像素）。仅使用 Unity 配置，不读取 C++ 背景 dat。")]
        public int BattleStageZMinPx = 180;
        [Tooltip("Unity 战斗场景逻辑 Z 最大值（像素）。仅使用 Unity 配置，不读取 C++ 背景 dat。")]
        public int BattleStageZMaxPx = 350;
        [Tooltip("Unity 战斗场景透视近端参数。默认 0，后续按 Unity 场景表现调整。")]
        public int BattlePerspectiveNear = 0;
        [Tooltip("Unity 战斗场景透视远端参数。默认 0，后续按 Unity 场景表现调整。")]
        public int BattlePerspectiveFar = 0;

        #endregion

        #region 队伍配置

        /// <summary>
        /// 可选的队伍名称列表
        /// 最后一个选项通常是"独立"(Independent)，表示不属于任何队伍
        /// </summary>
        [Header("Team Options")]
        public string[] TeamOptions = { "Team 1", "Team 2", "Team 3", "Team 4", "Independent" };

        #endregion

        #region 角色选择UI - 空闲状态配置

        /// <summary>
        /// 空闲状态下闪烁的第一个图标（用于"按键加入"提示）
        /// </summary>
        [Header("Select Role UI - Idle State")]
        public Sprite JoinIcon1;

        /// <summary>
        /// 空闲状态下闪烁的第二个图标（与JoinIcon1交替显示）
        /// </summary>
        public Sprite JoinIcon2;

        /// <summary>
        /// 空闲状态下玩家名称显示的文本（如"Join?"）
        /// </summary>
        public string IdlePlayerText = "Join?";

        /// <summary>
        /// 空闲状态下角色名称显示的文本（通常为空）
        /// </summary>
        public string IdleFighterText = "";

        /// <summary>
        /// 空闲状态下队伍显示的文本（通常为空）
        /// </summary>
        public string IdleTeamText = "";

        /// <summary>
        /// 空闲状态下图标/文字闪烁的间隔时间（秒）
        /// </summary>
        public float IdleFlashInterval = 0.5f;

        /// <summary>
        /// 闪烁时的第一个颜色
        /// </summary>
        public Color IdleFlashColor1 = Color.white;

        /// <summary>
        /// 闪烁时的第二个颜色（与IdleFlashColor1交替）
        /// </summary>
        public Color IdleFlashColor2 = Color.gray;

        #endregion

        #region 角色选择UI - 激活状态配置

        /// <summary>
        /// 当前正在选择的项目的高亮颜色
        /// </summary>
        [Header("Select Role UI - Active State")]
        public Color SelectedColor = Color.yellow;

        /// <summary>
        /// 已确认选择的项目的颜色
        /// </summary>
        public Color ConfirmedColor = Color.green;

        #endregion

        #region 随机选项配置

        /// <summary>
        /// "随机"选项的显示名称
        /// </summary>
        [Header("Random Option")]
        public string RandomDisplayName = "Random";

        /// <summary>
        /// "随机"选项的图标
        /// </summary>
        public Sprite RandomIcon;

        #endregion

        #region 倒计时配置

        /// <summary>
        /// 倒计时精灵数组
        /// 索引0对应倒计时开始（如显示"5"），索引4对应倒计时结束（如显示"1"）
        /// 这些精灵会显示在未加入玩家的槽位上
        /// </summary>
        [Header("Countdown Display")]
        public Sprite[] CountdownSprites;

        /// <summary>
        /// 倒计时总时长（秒）
        /// 当所有已加入玩家都确认后开始倒计时
        /// </summary>
        public float CountdownDuration = 5f;

        /// <summary>
        /// 倒计时加速倍率
        /// 当有玩家在倒计时期间取消确认时，倒计时会以此倍率加速
        /// </summary>
        public float CountdownAcceleration = 2f;

        #endregion

        #region 常量定义

        /// <summary>
        /// 表示"随机角色"的特殊ID
        /// 在最终确定角色时会被替换为实际的随机角色ID
        /// </summary>
        public const int RandomCharacterId = -1;

        /// <summary>
        /// 表示"独立队伍"的特殊ID
        /// 选择此队伍的玩家不与任何人组队
        /// </summary>
        public const int TeamIndependent = -1;

        #endregion
    }
}
