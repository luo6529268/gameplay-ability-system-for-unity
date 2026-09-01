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
        private NTSDEntityRuntime _runtime;
        private double _x;
        private double _y;
        private double _z;
        private double _vx;
        private double _vy;
        private double _vz;
        private float _sx;
        private float _sy;
        private float _sz;

        /// <summary>
        /// 绑定 C++ release 风格的运行时状态。绑定后公开属性读写 Runtime，避免多份状态互相漂移。
        /// </summary>
        public void BindRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null) return;

            runtime.X = x;
            runtime.Y = y;
            runtime.Z = z;
            runtime.Vx = vx;
            runtime.Vy = vy;
            runtime.Vz = vz;
            runtime.SpriteX = sx;
            runtime.SpriteY = sy;
            runtime.SpriteZ = sz;
            _runtime = runtime;
        }

        /// <summary>地面平面 X 坐标，单位为 NTSD 像素。</summary>
        public double x { get => _runtime?.X ?? _x; set { if (_runtime != null) _runtime.X = value; else _x = value; } }

        /// <summary>垂直偏移，单位为 NTSD 像素；负数表示在空中。</summary>
        public double y { get => _runtime?.Y ?? _y; set { if (_runtime != null) _runtime.Y = value; else _y = value; } }

        /// <summary>Unity 世界空间中的地面参考高度。</summary>
        public float groundY = 0f;

        /// <summary>地面平面深度坐标，单位为 NTSD 像素。</summary>
        public double z { get => _runtime?.Z ?? _z; set { if (_runtime != null) _runtime.Z = value; else _z = value; } }

        /// <summary>X 轴速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public double vx { get => _runtime?.Vx ?? _vx; set { if (_runtime != null) _runtime.Vx = value; else _vx = value; } }

        /// <summary>垂直速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public double vy { get => _runtime?.Vy ?? _vy; set { if (_runtime != null) _runtime.Vy = value; else _vy = value; } }

        /// <summary>Z 轴速度，单位为每个 30Hz 模拟 tick 的像素。</summary>
        public double vz { get => _runtime?.Vz ?? _vz; set { if (_runtime != null) _runtime.Vz = value; else _vz = value; } }

        // ==================== 渲染空间缓存坐标 ====================
        /// <summary>缓存的精灵原点 X，单位为 NTSD 像素。</summary>
        public float sx { get => _runtime?.SpriteX ?? _sx; set { if (_runtime != null) _runtime.SpriteX = value; else _sx = value; } }

        /// <summary>缓存的精灵原点 Y，单位为 NTSD 像素。</summary>
        public float sy { get => _runtime?.SpriteY ?? _sy; set { if (_runtime != null) _runtime.SpriteY = value; else _sy = value; } }

        /// <summary>缓存的精灵排序/深度值，单位为 NTSD 像素。</summary>
        public float sz { get => _runtime?.SpriteZ ?? _sz; set { if (_runtime != null) _runtime.SpriteZ = value; else _sz = value; } }

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
            sx = (float)(dir == "right"
                ? (x - centerx)
                : (x + centerx - spriteWidthPx));

            sy = (float)(y + z - centery);
            sz = (float)z;
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
        /// C++ release 中没有 bdy 的帧不会参与身体命中判定。
        /// </summary>
        public System.Collections.Generic.List<BattleVolume> GetBodyVolumes(
            System.Collections.Generic.List<BattleBodyBoxValue> bodies,
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
                return result;

            bool facingLeft = dir == "left";
            foreach (var body in bodies)
            {
                float localX = body.X;
                if (facingLeft)
                {
                    localX = spriteWidthPx - body.X - body.W;
                }

                result.Add(new BattleVolume(
                    originX, originY, originZ,
                    localX, body.Y,
                    body.W, body.H,
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
            System.Collections.Generic.List<BattleBodyBoxValue> bodies,
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
                return;

            bool facingLeft = dir == "left";
            for (int i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];

                float localX = body.X;
                if (facingLeft)
                {
                    localX = spriteWidthPx - body.X - body.W;
                }

                dst.Add(new BattleVolume(
                    originX, originY, originZ,
                    localX, body.Y,
                    body.W, body.H,
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

        /// <summary>将 NTSD 深度 z 转换为 Unity 屏幕 Y；NTSD z 越大表示越靠屏幕下方。</summary>
        public static float DepthToUnityY(float depthZ)
        {
            return NTSDRenderSpace.GroundPixelToWorld(0f, depthZ).y;
        }

        /// <summary>将 Unity 屏幕 Y 转换回 NTSD 深度 z。</summary>
        public static float UnityYToDepth(float unityY)
        {
            return NTSDRenderSpace.WorldToGroundPixel(new Vector3(0f, unityY, 0f)).y;
        }

        /// <summary>将 NTSD 地面点转换为 Unity X/Y 坐标。</summary>
        public static Vector2 ToUnityGroundPoint(float ntsdX, float ntsdZ)
        {
            return NTSDRenderSpace.GroundPixelToWorld(ntsdX, ntsdZ);
        }

        /// <summary>将 NTSD 屏幕像素 Y 转换为 Unity 屏幕 Y。</summary>
        public static float ScreenYToUnityY(float screenY)
        {
            return NTSDRenderSpace.ScreenPixelToWorld(0f, screenY, 0f).y;
        }

        /// <summary>将 NTSD tick 速度转换为 Unity 地面平面上的每秒单位速度。</summary>
        public Vector2 ToUnityVelocity()
        {
            float conversion = SimulationConstants.SIM_TICK_RATE / SimulationConstants.PIXELS_PER_UNIT;  // = 0.3
            return new Vector2((float)(vx * conversion), (float)(-vz * conversion));
        }

        /// <summary>将 Unity 每秒单位速度转换回 NTSD tick 速度。</summary>
        public void FromUnityVelocity(Vector2 unityVel)
        {
            float conversion = SimulationConstants.PIXELS_PER_UNIT / SimulationConstants.SIM_TICK_RATE;
            vx = unityVel.x * conversion;
            vz = -unityVel.y * conversion;
        }

        /// <summary>将 NTSD 地面平面位置转换为 Unity X/Y 坐标。</summary>
        public Vector3 ToUnityPosition()
        {
            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld((float)x, (float)z);
            return new Vector3(groundPoint.x, groundPoint.y, 0f);
        }

        /// <summary>根据 Unity 位置初始化 NTSD 地面平面位置。</summary>
        public void FromUnityPosition(Vector3 unityPos)
        {
            Vector2 pixel = NTSDRenderSpace.WorldToGroundPixel(unityPos);
            x = pixel.x;
            z = pixel.y;
            groundY = 0f;
            y = 0;
        }

        /// <summary>返回用于场景边界检测的 Unity 空间地面平面点。</summary>
        public Vector2 GetGroundPoint2D()
        {
            return ToUnityGroundPoint((float)x, (float)z);
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
        public Rect GetFootprintRect(System.Collections.Generic.List<BattleBodyBoxValue> bodies, int centerx, int centery)
        {
            if (bodies == null || bodies.Count == 0)
            {
                Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld((float)x, (float)z);
                return new Rect(groundPoint.x, groundPoint.y, 0f, 0f);
            }

            var body = bodies[0];

            // 朝右：left = ps.x - centerx + body.x
            // 朝左：left = ps.x + centerx - body.x - body.w
            double bodyLeftPx = dir == "left"
                ? (x + centerx - body.X - body.W)
                : (x - centerx + body.X);
            float bodyWorldX = NTSDRenderSpace.GroundPixelToWorld((float)bodyLeftPx, (float)z).x;
            float bodyWorldY = NTSDRenderSpace.ScreenPixelToWorld(0f, (float)(z + body.Y - centery), 0f).y;
            float bodyWidth = body.W / SimulationConstants.PIXELS_PER_UNIT;
            float bodyHeight = body.H / SimulationConstants.PIXELS_PER_UNIT;

            return new Rect(bodyWorldX, bodyWorldY, bodyWidth, bodyHeight);
        }
    }
}
