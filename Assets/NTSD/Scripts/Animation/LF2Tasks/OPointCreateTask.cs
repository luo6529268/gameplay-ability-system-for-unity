using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation.LF2Tasks
{
    /// <summary>
    /// 单对象创建任务
    /// 对应 FLF match.js case 'create_object'
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\match.js:338-354
    /// </summary>
    public class OPointCreateTask : LF2TaskBase
    {
        public override LF2TaskType TaskType => LF2TaskType.CreateObject;

        // ========== 任务数据 ==========
        public ObjectPoint opoint;
        public LF2LivingObject parent;
        public int team;
        public Vector3 pos;      // 世界坐标 (make_point 结果)
        public float z;          // parent.ps.z
        public string dir;       // parent.ps.dir
        public float dvz;        // parent.dirv() * 2
    }
}
