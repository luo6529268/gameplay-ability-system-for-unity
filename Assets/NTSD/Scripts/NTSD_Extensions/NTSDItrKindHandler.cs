namespace NTSD.Extensions
{
    public interface INTSDItrKindService
    {
        bool IsAttackKind(int kind);
        bool IsPreInteractionKind(int kind);
    }

    /// <summary>
    /// ITR kind 分类服务。
    /// C++ release 的碰撞分支按 itr.kind 精确判断；这里仅保留正式战斗流程需要的分类入口。
    /// </summary>
    public class NTSDItrKindService : INTSDItrKindService
    {
        public bool IsAttackKind(int kind)
        {
            return kind == 0 || kind == 4 || kind == 8 || kind == 9 || kind == 10 || kind == 11 || kind == 15 || kind == 16;
        }

        public bool IsPreInteractionKind(int kind)
        {
            return kind == 1 || kind == 2 || kind == 3 || kind == 7;
        }
    }
}
