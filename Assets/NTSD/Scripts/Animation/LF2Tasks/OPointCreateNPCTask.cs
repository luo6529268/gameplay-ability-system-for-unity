using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation.LF2Tasks
{
    /// <summary>
    /// NPC 角色创建任务 (oid=5 分身)
    /// 对应 FLF match.js case 'create_non_player_characters'
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\match.js:394-395
    /// </summary>
    public class OPointCreateNPCTask : LF2TaskBase
    {
        public override LF2TaskType TaskType => LF2TaskType.CreateNPCCharacters;

        // ========== 任务数据 ==========
        public ILF2LivingObject parent;
        public int team;
        public int characterId;
        public int number;
        public Vector3 basePos;
    }
}
