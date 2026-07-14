using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// OPoint 工厂接口。Unity 侧使用 Enqueue + Flush，把生成请求延迟到统一阶段处理。
    /// </summary>
    public interface ILF2ObjectPointFactory
    {
        /// <summary>入队单个对象创建任务。</summary>
        void EnqueueCreateObject(OPointCreateTask task);

        /// <summary>入队多个对象创建任务。</summary>
        void EnqueueCreateMultipleObjects(OPointCreateMultipleTask task);

        /// <summary>处理队列中的所有任务并清空。</summary>
        void FlushTasks();
    }

    /// <summary>
    /// 处理当前帧的 opoint。模块只负责生成请求入队，实际创建由工厂统一执行。
    /// 行为以 C++ release 的 Entity_FrameLogic/opoint 分支为基准。
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

        /// <summary>处理当前帧 opoint，只入队，不立即创建。</summary>
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

            bool hasList = frame.opoints != null && frame.opoints.Count > 0;
            if (!hasList && !frame.opoint.HasValue) return;

            ObjectPoint firstOp = hasList ? frame.opoints[0] : frame.opoint.Value;
            if (firstOp.kind <= 0 || animator.AttackingCounter != 0) return;

            if (animator.HitStun != 0) return;

            if (animator.FrameDelay != 0 && animator.ObjectType == 0) return;

            if (animator.OwnerId != -1 && animator.ShotCount >= 150) return;

            if (animator.ShotCount >= 500) return;

            if (hasList)
            {
                for (int i = 0; i < frame.opoints.Count; i++)
                    ProcessOneOpoint(animator, frame.opoints[i]);
            }
            else
            {
                ProcessOneOpoint(animator, frame.opoint.Value);
            }
        }

        private void ProcessOneOpoint(LF2LivingObject animator, ObjectPoint op)
        {
            if (op.oid <= 0 || op.kind <= 0) return;

            var world = SimulationTickDriver.Instance?.World;
            if (world != null)
            {
                int objectCount = world.ObjectCount;
                int def = GameDataManager.Instance?.GetObjectById(op.oid)?.type ?? -1;
                int limit = (def == 3 || def == 4) ? 250 : 500;
                if (objectCount >= limit) return;
            }

            {
                int count = animator.ShotCount < 500 ? animator.ShotCount : 500;
                if (animator.ObjectType == 3 || animator.ObjectType == 4) count >>= 1;
                int step = (500 - count) / 30;
                animator.ShotCount += step + 1;
            }

            if (op.facing > 10)
            {
                int number = op.facing / 10;
                EnqueueMultipleTask(animator, op, number);
                return;
            }

            EnqueueSingleTask(animator, op);
        }

        private void EnqueueSingleTask(LF2LivingObject animator, ObjectPoint op)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = animator;
            task.team = animator.Team;
            task.pos = pos;
            task.z = (float)animator.PS.z;
            task.dir = animator.PS.dir;
            task.dvz = 0f;

            Factory.EnqueueCreateObject(task);
        }

        private void EnqueueMultipleTask(LF2LivingObject animator, ObjectPoint op, int number)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = LF2ReferencePool.Instance.Fetch<OPointCreateMultipleTask>();
            task.opoint = op;
            task.parent = animator;
            task.team = animator.Team;
            task.pos = pos;
            task.z = (float)animator.PS.z;
            task.dir = animator.PS.dir;
            task.dvz = 0f;
            task.number = number;

            Factory.EnqueueCreateMultipleObjects(task);
        }

        /// <summary>
        /// 计算 opoint 生成点。C++ release 使用实体逻辑坐标和当前帧 center，不依赖渲染原点缓存。
        /// task.pos.y 仍按现有初始化约定传递 screenY（逻辑 y + z），初始化时会再减 task.z。
        /// </summary>
        private Vector3 MakePoint(LF2LivingObject animator, ObjectPoint op)
        {
            var ps = animator.PS;
            var frame = animator.Frame?.D;
            if (ps == null || frame == null)
                return Vector3.zero;

            double x = ps.dir == "right"
                ? ps.x - frame.centerx + op.x
                : ps.x + frame.centerx - op.x;

            double logicalY = ps.y - frame.centery + op.y;
            double screenY = logicalY + ps.z;

            return new Vector3((float)x, (float)screenY, (float)ps.z);
        }
    }
}
