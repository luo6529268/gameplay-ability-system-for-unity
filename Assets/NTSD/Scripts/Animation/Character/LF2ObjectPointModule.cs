using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
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

        /// <summary>入队 NPC 创建任务</summary>
        void EnqueueCreateNPCCharacters(OPointCreateNPCTask task);

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

        /// <summary>
        /// 处理当前帧的 OPoint - 只入队，不立即创建
        /// 对应 FLF character.prototype.opoint()
        /// </summary>
        public void ProcessFrame(LF2LivingObject animator)
        {
            if (animator == null) return;
            if (Factory == null) return;

            LF2FrameData frame = animator.Frame.D;
            if (frame == null) return;

            ObjectPoint op = frame.opoint;
            if (op == null) return;
            if (op.oid <= 0) return;

            // === FLF character.js opoint() 逻辑 ===

            // 1. oid=5: 生成 NPC 分身角色
            if (op.oid == 5)
            {
                EnqueueNPCTask(animator, op);
                return;
            }

            // 2. facing > 10: 批量生成多个对象
            if (Mathf.Abs(op.facing) > 10)
            {
                int number = Mathf.FloorToInt(Mathf.Abs(op.facing) / 10f);
                float vz = op.dvz != 0 ? op.dvz : 3f;
                EnqueueMultipleTask(animator, op, number, vz);
                return;
            }

            // 3. 普通: 单个对象生成
            EnqueueSingleTask(animator, op);
        }

        private void EnqueueNPCTask(LF2LivingObject animator, ObjectPoint op)
        {
            int numberOfCharacters = Mathf.FloorToInt(Mathf.Abs(op.facing) / 10f);
            if (numberOfCharacters <= 0) return;

            var task = new OPointCreateNPCTask
            {
                parent = animator,
                team = animator.Team,
                characterId = animator.ObjectId,
                number = numberOfCharacters,
                basePos = new Vector3(animator.PS.x, animator.PS.y, animator.PS.z)
            };

            Factory.EnqueueCreateNPCCharacters(task);
        }

        private void EnqueueSingleTask(LF2LivingObject animator, ObjectPoint op)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = new OPointCreateTask
            {
                opoint = op,
                parent = animator,
                team = animator.Team,
                pos = pos,
                z = animator.PS.z,
                dir = animator.PS.dir,
                dvz = animator.Controller.Dirv() * 2f
            };

            Factory.EnqueueCreateObject(task);
        }

        private void EnqueueMultipleTask(LF2LivingObject animator, ObjectPoint op, int number, float vz)
        {
            Vector3 pos = MakePoint(animator, op);

            var task = new OPointCreateMultipleTask
            {
                opoint = op,
                parent = animator,
                team = animator.Team,
                pos = pos,
                z = animator.PS.z,
                dir = animator.PS.dir,
                dvz = animator.Controller.Dirv() * 2f,
                number = number,
                vz = vz
            };

            Factory.EnqueueCreateMultipleObjects(task);
        }

        /// <summary>
        /// 计算 OPoint 的世界坐标
        /// P2 对齐 FLF mechanics.js mech.prototype.make_point()
        /// 使用 PS.sx/sy/sz（sprite origin）而非 PS.x/y/z
        /// </summary>
        private Vector3 MakePoint(LF2LivingObject animator, ObjectPoint op, string prefix = "")
        {
            var PS = animator.PS;
            var frame = animator.Frame.D;
            float spriteWidth = animator.GetSpriteWidthPxForCollision();

            if (op == null) 
            {
                Log.Info("mechanics: make point failed'");
                return new Vector3(PS.sx, PS.sy, PS.sz);
            }

            Vector3 objectPoint = Vector3.zero;
            if (!string.IsNullOrEmpty(prefix))
            {
                // 确保 sx/sy/sz 已更新
                int centerx = frame?.centerx ?? 0;
                int centery = frame?.centery ?? 0;
                objectPoint.x = (PS.dir == "right") ?
                    PS.sx + centerx : PS.sx + spriteWidth - centerx;
                objectPoint.y = PS.sy + centery;
                objectPoint.z = PS.sz + centery;
            }
            else
            {
                objectPoint.x = (PS.dir == "right")
                    ? PS.sx + op.x
                    : PS.sx + spriteWidth - op.x;

                objectPoint.y = PS.sy + op.y;
                objectPoint.z = PS.sz + op.y;
            }

            return objectPoint;
        }
    }
}
