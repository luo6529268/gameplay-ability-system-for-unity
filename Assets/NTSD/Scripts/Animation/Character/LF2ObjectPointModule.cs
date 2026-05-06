using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// OPoint 工厂接口 - Enqueue + Flush 模式
    /// 对应 FLF match.js tasks 队列机制
    /// </summary>
    public interface ILF2ObjectPointFactory
    {
        /// <summary>入队单个对象创建任务</summary>
        void EnqueueCreateObject(OPointCreateTask task);

        /// <summary>入队多对象创建任务</summary>
        void EnqueueCreateMultipleObjects(OPointCreateMultipleTask task);

        /// <summary>
        /// 处理所有队列中的任务并清空
        /// 对应 FLF match.js process_tasks() + tasks.length = 0
        /// </summary>
        void FlushTasks();
    }

    /// <summary>
    /// FLF 对齐：character.prototype.opoint() 的完整实现
    /// 只负责 enqueue，不负责实际创建
    /// 
    /// 参考：
    /// - I:\C++Test\NTSD\F.LF-master\LF\character.js:2341 (character.prototype.opoint)
    /// - I:\C++Test\NTSD\F.LF-master\LF\match.js:194 (create_object/create_multiple_objects)
    /// </summary>
    public sealed class LF2ObjectPointModule
    {
        public ILF2ObjectPointFactory Factory { get; private set; }

        public void SetFactory(ILF2ObjectPointFactory factory)
        {
            Factory = factory;
        }

        public void Reset()
        {
            Factory = null;
        }

        /// <summary>
        /// 处理当前帧的 OPoint - 只入队，不立即创建
        /// 对应 FLF character.prototype.opoint()
        /// </summary>
        public void ProcessFrame(LF2LivingObject animator)
        {
            if (animator == null) return;
            if (Factory == null)
            {
                Debug.LogWarning($"[OPointModule] Factory is null for {animator?.Name}");
                return;
            }

            LF2FrameData frame = animator.Frame.D;
            if (frame == null) return;

            if (!frame.opoint.HasValue) return;
            ObjectPoint op = frame.opoint.Value;
            if (op.oid <= 0) return;

            Debug.Log($"[OPointModule] ProcessFrame: char={animator.Name}, frame={frame.frameId}, oid={op.oid}, action={op.action}, facing={op.facing}");

            // 对应反汇编 0x0042216F：被击中锁定期间不生成 opoint
            if (animator.HitStun != 0) return;

            // 对应反汇编 0x0042217D：帧延迟不为 0 且实体有类型时跳过 opoint
            if (animator.FrameDelay != 0 && animator.ObjectType != 0) return;

            // 对应反汇编 0x00421F11：自身是子对象（OwnerId != -1）且 ShotCount >= 150 → skip
            if (animator.OwnerId != -1 && animator.ShotCount >= 150) return;

            // 对应反汇编 0x00421F2A：ShotCount >= 500 → skip
            if (animator.ShotCount >= 500) return;

            // 对应反汇编 0x00421F57-F84：场景实体总数上限 500，type==3/4 时上限减半为 250
            var world = SimulationTickDriver.Instance?.World;
            if (world != null)
            {
                int objectCount = world.ObjectCount;
                int def = GameDataManager.Instance?.GetObjectById(op.oid)?.type ?? -1;
                int limit = (def == 3 || def == 4) ? 250 : 500;
                if (objectCount >= limit) return;
            }

            // 对应反汇编 0x00421F9C-FA6：ShotCount 递增
            // step = (500 - min(ShotCount,500)) / 30；对 type3/4 先将 count 减半
            {
                int count = animator.ShotCount < 500 ? animator.ShotCount : 500;
                if (animator.ObjectType == 3 || animator.ObjectType == 4) count >>= 1;
                int step = (500 - count) / 30;
                animator.ShotCount += step + 1;
            }

            // facing > 10: 批量生成（对应反汇编 0x0042219D: cmp ecx,0Ah; jle — 有符号比较，负值不进入）
            if (op.facing > 10)
            {
                int number = op.facing / 10;
                EnqueueMultipleTask(animator, op, number);
                return;
            }

            // 普通：单个对象生成
            EnqueueSingleTask(animator, op);
        }

        private void EnqueueSingleTask(LF2LivingObject animator, ObjectPoint op)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint  = op;
            task.parent  = animator;
            task.team    = animator.Team;
            task.pos     = pos;
            task.z       = animator.PS.z;
            task.dir     = animator.PS.dir;
            task.dvz     = 0f;

            Factory.EnqueueCreateObject(task);
        }

        private void EnqueueMultipleTask(LF2LivingObject animator, ObjectPoint op, int number)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = LF2ReferencePool.Instance.Fetch<OPointCreateMultipleTask>();
            task.opoint  = op;
            task.parent  = animator;
            task.team    = animator.Team;
            task.pos     = pos;
            task.z       = animator.PS.z;
            task.dir     = animator.PS.dir;
            task.dvz     = 0f;
            task.number  = number;

            Factory.EnqueueCreateMultipleObjects(task);
        }

        /// <summary>
        /// 计算 OPoint 的世界坐标
        /// P2 对齐 FLF mechanics.js mech.prototype.make_point()
        /// 使用 PS.sx/sy/sz（sprite origin）而非 PS.x/y/z
        /// </summary>
        private Vector3 MakePoint(LF2LivingObject animator, ObjectPoint op)
        {
            var PS = animator.PS;
            float spriteWidth = animator.GetSpriteWidthPxForCollision();

            Vector3 objectPoint = Vector3.zero;
            objectPoint.x = (PS.dir == "right")
                ? PS.sx + op.x
                : PS.sx + spriteWidth - op.x;

            objectPoint.y = PS.sy + op.y;
            objectPoint.z = PS.sz + op.y;

            return objectPoint;
        }
    }
}
