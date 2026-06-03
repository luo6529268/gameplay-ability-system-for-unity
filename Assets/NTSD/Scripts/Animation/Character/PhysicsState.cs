using UnityEngine;
using NTSD.Simulation;

namespace NTSD.Animation
{
    /// <summary>
    /// NTSD 对象的正式版物理状态。
    /// 坐标和速度使用 NTSD 像素单位；y == 0 表示地面，y &lt; 0 表示空中，z 表示地面深度轴。
    /// </summary>
    public class PhysicsState
    {
        /// <summary>地面平面 X 坐标，单位为 NTSD 像素。</summary>
        public float x;

        /// <summary>垂直偏移，单位为 NTSD 像素；负数表示在空中。</summary>
        public float y;

        /// <summary>Unity 世界空间中的地面参考高度。</summary>
        public float groundY = 0f;

        /// <summary>地面平面深度坐标，单位为 NTSD 像素。</summary>
        public float z;

        /// <summary>X 轴速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public float vx;

        /// <summary>垂直速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public float vy;

        /// <summary>Z 轴速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public float vz;

        // ==================== 渲染空间缓存坐标 ====================
        /// <summary>缓存的精灵原点 X，单位为 NTSD 像素。</summary>
        public float sx;

        /// <summary>缓存的精灵原点 Y，单位为 NTSD 像素。</summary>
        public float sy;

        /// <summary>缓存的精灵排序/深度值，单位为 NTSD 像素。</summary>
        public float sz;

        /// <summary>朝向，当前使用 "right" 或 "left"。</summary>
        public string dir = "right";

        /// <summary>每 tick 的地面摩擦系数，TU 逻辑前会重置为 1。</summary>
        public float fric = 1f;

        /// <summary>额外深度偏移，用于抓取和渲染层级调整。</summary>
        public float zz = 0f;

        /// <summary>当前物理 tick 内阻止正 Z 方向移动。</summary>
        public bool zBoundPositive;

        /// <summary>当前物理 tick 内阻止负 Z 方向移动。</summary>
        public bool zBoundNegative;

        /// <summary>当前物理 tick 内阻止正 X 方向移动。</summary>
        public bool xBoundPositive;

        /// <summary>当前物理 tick 内阻止负 X 方向移动。</summary>
        public bool xBoundNegative;

        /// <summary>将所有可变物理状态重置为对象池复用时的默认值。</summary>
        public void Reset()
        {
            x = 0;
            y = 0;
            z = 0;
            groundY = 0;
            vx = 0;
            vy = 0;
            vz = 0;
            sx = 0;
            sy = 0;
            sz = 0;
            dir = "right";
            fric = 1f;
            zz = 0;
            zBoundPositive = false;
            zBoundNegative = false;
            xBoundPositive = false;
            xBoundNegative = false;
        }

        /// <summary>
        /// 根据 DAT 帧中心点更新精灵空间原点缓存。
        /// 朝左时会按精灵宽度镜像本地 X 原点。
        /// </summary>
        public void UpdateSpriteOrigin(int centerx, int centery, float spriteWidthPx)
        {
            sx = dir == "right"
                ? (x - centerx)
                : (x + centerx - spriteWidthPx);

            sy = y + z - centery;
            sz = z;
        }

        /// <summary>
        /// NTSD 像素空间中的碰撞体积。
        /// x/y/z 是帧原点，vx/vy 是本地盒子偏移，w/h/zwidth 是盒子尺寸和深度。
        /// </summary>
        public readonly struct BattleVolume
        {
            public readonly float x;
            public readonly float y;
            public readonly float z;
            public readonly float vx;
            public readonly float vy;
            public readonly float w;
            public readonly float h;
            public readonly float zwidth;

            public BattleVolume(float x, float y, float z, float vx, float vy, float w, float h, float zwidth)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.vx = vx;
                this.vy = vy;
                this.w = w;
                this.h = h;
                this.zwidth = zwidth;
            }
        }

        /// <summary>
        /// 根据当前帧的 bdy 盒子生成身体碰撞体积。
        /// 如果没有 bdy，则返回位于原点的零尺寸点体积。
        /// </summary>
        public System.Collections.Generic.List<BattleVolume> GetBodyVolumes(
            System.Collections.Generic.List<BodyBox> bodies,
            int centerx,
            int centery,
            float spriteWidthPx,
            float offsetX = 0f,
            float offsetY = 0f,
            float offsetZ = 0f)
        {
            float originX = sx + offsetX;
            float originY = sy + offsetY;
            float originZ = sz + offsetZ;

            var result = new System.Collections.Generic.List<BattleVolume>();

            if (bodies == null || bodies.Count == 0)
            {
                result.Add(new BattleVolume(originX, originY, originZ, 0f, 0f, 0f, 0f, 0f));
                return result;
            }

            bool facingLeft = dir == "left";
            foreach (var body in bodies)
            {
                float localX = body.x;
                if (facingLeft)
                {
                    localX = spriteWidthPx - body.x - body.w;
                }

                result.Add(new BattleVolume(
                    originX, originY, originZ,
                    localX, body.y,
                    body.w, body.h,
                    NTSDGlobal.Default.Itr.ZWidth
                ));
            }

            return result;
        }

        /// <summary>
        /// 将身体碰撞体积写入调用方提供的列表，避免高频碰撞路径中每 tick 分配内存。
        /// </summary>
        public void FillBodyVolumes(
            System.Collections.Generic.List<BattleVolume> dst,
            System.Collections.Generic.List<BodyBox> bodies,
            int centerx,
            int centery,
            float spriteWidthPx,
            float zwidthPx,
            float offsetX = 0f,
            float offsetY = 0f,
            float offsetZ = 0f)
        {
            if (dst == null) return;
            dst.Clear();

            float originX = sx + offsetX;
            float originY = sy + offsetY;
            float originZ = sz + offsetZ;

            if (bodies == null || bodies.Count == 0)
            {
                dst.Add(new BattleVolume(originX, originY, originZ, 0f, 0f, 0f, 0f, 0f));
                return;
            }

            bool facingLeft = dir == "left";
            for (int i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                if (body == null) continue;

                float localX = body.x;
                if (facingLeft)
                {
                    localX = spriteWidthPx - body.x - body.w;
                }

                dst.Add(new BattleVolume(
                    originX, originY, originZ,
                    localX, body.y,
                    body.w, body.h,
                    zwidthPx
                ));
            }
        }

        /// <summary>
        /// 根据当前帧 itr 盒子生成攻击/交互体积。
        /// 没有 itr 数据时返回空列表。
        /// </summary>
        public System.Collections.Generic.List<BattleVolume> GetItrVolumes(
            System.Collections.Generic.List<InteractionArea> itrs,
            int centerx,
            int centery,
            float spriteWidthPx,
            float itrZWidthPx = 0f,
            float offsetX = 0f,
            float offsetY = 0f,
            float offsetZ = 0f)
        {
            float originX = sx + offsetX;
            float originY = sy + offsetY;
            float originZ = sz + offsetZ;

            var result = new System.Collections.Generic.List<BattleVolume>();
            if (itrs == null || itrs.Count == 0)
                return result;

            bool facingLeft = dir == "left";
            foreach (var itr in itrs)
            {
                float localX = itr.x;
                if (facingLeft)
                {
                    localX = spriteWidthPx - itr.x - itr.w;
                }

                result.Add(new BattleVolume(
                    originX, originY, originZ,
                    localX, itr.y,
                    itr.w, itr.h,
                    itr.zwidth != 0 ? itr.zwidth : itrZWidthPx
                ));
            }

            return result;
        }

        /// <summary>根据单个 itr 盒子生成一个交互体积。</summary>
        public BattleVolume GetItrVolume(
            InteractionArea itr,
            int centerx,
            int centery,
            float spriteWidthPx,
            float itrZWidthPx = 0f)
        {
            bool facingLeft = dir == "left";
            float localX = itr.x;
            if (facingLeft)
            {
                localX = spriteWidthPx - itr.x - itr.w;
            }

            return new BattleVolume(
                sx, sy, sz,
                localX, itr.y,
                itr.w, itr.h,
                itr.zwidth != 0 ? itr.zwidth : itrZWidthPx
            );
        }

        /// <summary>恢复默认摩擦系数，供下一次 TU 物理流程使用。</summary>
        public void ResetFriction()
        {
            fric = 1f;
        }

        /// <summary>将 NTSD tick 速度转换为 Unity 地面平面上的每秒单位速度。</summary>
        public Vector2 ToUnityVelocity()
        {
            float conversion = SimulationConstants.SIM_TICK_RATE / SimulationConstants.PIXELS_PER_UNIT;  // = 0.3
            return new Vector2(vx * conversion, vz * conversion);
        }

        /// <summary>将 Unity 每秒单位速度转换回 NTSD tick 速度。</summary>
        public void FromUnityVelocity(Vector2 unityVel)
        {
            float conversion = SimulationConstants.PIXELS_PER_UNIT / SimulationConstants.SIM_TICK_RATE;
            vx = unityVel.x * conversion;
            vz = unityVel.y * conversion;
        }

        /// <summary>将 NTSD 地面平面位置转换为 Unity X/Y 坐标。</summary>
        public Vector3 ToUnityPosition()
        {
            return new Vector3(
                x / SimulationConstants.PIXELS_PER_UNIT,
                z / SimulationConstants.PIXELS_PER_UNIT,
                0f
            );
        }

        /// <summary>根据 Unity 位置初始化 NTSD 地面平面位置。</summary>
        public void FromUnityPosition(Vector3 unityPos)
        {
            x = unityPos.x * SimulationConstants.PIXELS_PER_UNIT;
            z = unityPos.y * SimulationConstants.PIXELS_PER_UNIT;
            groundY = 0f;
            y = 0;
        }

        /// <summary>返回用于场景边界检测的 Unity 空间地面平面点。</summary>
        public Vector2 GetGroundPoint2D()
        {
            return new Vector2(
                x / SimulationConstants.PIXELS_PER_UNIT,
                z / SimulationConstants.PIXELS_PER_UNIT
            );
        }

        /// <summary>朝左返回 -1，朝右返回 1。</summary>
        public int GetDir() 
        {
            return dir == "left" ? -1 : 1;
        }

        /// <summary>
        /// 根据第一个 body box 计算 Unity 空间中的脚底占用矩形。
        /// 该矩形用于简单可走边界检测，不用于战斗重叠检测。
        /// </summary>
        public Rect GetFootprintRect(System.Collections.Generic.List<BodyBox> bodies, int centerx, int centery)
        {
            if (bodies == null || bodies.Count == 0)
            {
                float worldX = x / SimulationConstants.PIXELS_PER_UNIT;
                float worldY = z / SimulationConstants.PIXELS_PER_UNIT;
                return new Rect(worldX, worldY, 0f, 0f);
            }

            var body = bodies[0];

            // 朝右：left = ps.x - centerx + body.x
            // 朝左：left = ps.x + centerx - body.x - body.w
            float bodyLeftPx = dir == "left"
                ? (x + centerx - body.x - body.w)
                : (x - centerx + body.x);
            float bodyWorldX = bodyLeftPx / SimulationConstants.PIXELS_PER_UNIT;
            float bodyWorldY = (z + body.y - centery) / SimulationConstants.PIXELS_PER_UNIT;
            float bodyWidth = body.w / SimulationConstants.PIXELS_PER_UNIT;
            float bodyHeight = body.h / SimulationConstants.PIXELS_PER_UNIT;

            return new Rect(bodyWorldX, bodyWorldY, bodyWidth, bodyHeight);
        }
    }
}
