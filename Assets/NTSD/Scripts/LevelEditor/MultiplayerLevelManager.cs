using NTSD.LevelEditor;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 兼容旧测试/启动代码使用的关卡管理器名称。
    /// 当前直接复用 BoundaryWallManager 提供的 SpawnPoints。
    /// </summary>
    public class MultiplayerLevelManager : BoundaryWallManager
    {
    }
}
