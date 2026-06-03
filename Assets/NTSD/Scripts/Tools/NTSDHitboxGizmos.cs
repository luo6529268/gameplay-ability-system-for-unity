using System.Collections.Generic;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Tools
{
    /// <summary>
    /// 在 Scene 视图中实时绘制所有活跃实体的 bdy/itr 碰撞盒。
    /// 挂在场景任意 GameObject 上即可生效（Editor Only）。
    /// 绿色 = bdy，红色 = itr kind=0/4/5，橙色 = itr kind=1/2/3，蓝色 = itr 其他 kind
    /// </summary>
    [ExecuteAlways]
    public class NTSDHitboxGizmos : MonoBehaviour
    {
        [Header("显示控制")]
        public bool showBdy = true;
        public bool showItr = true;
        public bool showOnlySelected = false;

        [Header("透明度")]
        [Range(0f, 1f)] public float bdyAlpha = 0.35f;
        [Range(0f, 1f)] public float itrAlpha = 0.35f;

        private static readonly Color BdyColor    = new Color(0.1f, 1f, 0.1f);
        private static readonly Color ItrColor0   = new Color(1f, 0.15f, 0.15f);   // kind=0/4/5 攻击
        private static readonly Color ItrColor123 = new Color(1f, 0.6f, 0.1f);     // kind=1/2/3 抓取
        private static readonly Color WPointColor = new Color(1f, 0.9f, 0.2f);
        private static readonly Color ItrColorOther = new Color(0.2f, 0.5f, 1f);   // 其他

        private readonly List<LF2Entity> _entities = new List<LF2Entity>();

        private void OnDrawGizmos()
        {
            if (!showBdy && !showItr) return;

            var driver = SimulationTickDriver.Instance;
            if (driver == null) return;

            driver.World?.GetAllEntities(_entities);
            if (_entities.Count == 0) return;

            foreach (var entity in _entities)
            {
                if (entity == null) continue;

                var frameD = entity.Frame?.D;
                if (frameD == null) continue;

                float spriteW = entity.GetSpriteWidthPxForCollision();
                if (spriteW <= 0f) continue;

                if (entity is LF2WeaponBase weapon && frameD.wpoints != null && frameD.wpoints.Count > 0)
                {
                    Debug.LogError($"[HitboxGizmos][WeaponFrame] weapon={weapon.StableId} frame={weapon.Frame?.N ?? -1} state={weapon.GetState()} wpointCount={frameD.wpoints.Count} spriteW={spriteW}");
                }

                if (showBdy)
                    DrawBdyBoxes(entity, frameD, spriteW);

                if (showItr)
                {
                    DrawItrBoxes(entity, frameD, spriteW);

                    if (entity is LF2WeaponBase weaponBase)
                        DrawWeaponPickupBoxes(weaponBase, frameD, spriteW);
                }
            }
        }

        private void DrawBdyBoxes(LF2Entity entity, LF2FrameData frameD, float spriteW)
        {
            var vols = entity.PS.GetBodyVolumes(frameD.bodies, frameD.centerx, frameD.centery, spriteW);
            Color fill = new Color(BdyColor.r, BdyColor.g, BdyColor.b, bdyAlpha);
            Color wire = new Color(BdyColor.r, BdyColor.g, BdyColor.b, 1f);

            foreach (var vol in vols)
                DrawVolume(vol, fill, wire);
        }

        private void DrawItrBoxes(LF2Entity entity, LF2FrameData frameD, float spriteW)
        {
            if (frameD.itrs == null || frameD.itrs.Count == 0) return;

            var vols = entity.PS.GetItrVolumes(frameD.itrs, frameD.centerx, frameD.centery, spriteW);
            int count = Mathf.Min(frameD.itrs.Count, vols.Count);

            for (int i = 0; i < count; i++)
            {
                int kind = frameD.itrs[i].kind;
                Color baseColor = kind == 0 || kind == 4 || kind == 5
                    ? ItrColor0
                    : (kind == 1 || kind == 2 || kind == 3 ? ItrColor123 : ItrColorOther);

                Color fill = new Color(baseColor.r, baseColor.g, baseColor.b, itrAlpha);
                Color wire = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                DrawVolume(vols[i], fill, wire);
            }
        }

        private void DrawWeaponPickupBoxes(LF2WeaponBase weapon, LF2FrameData frameD, float spriteW)
        {
            if (frameD.wpoints == null || frameD.wpoints.Count == 0) return;
            if (weapon.PS == null) return;

            bool facingLeft = weapon.PS.dir == "left";
            Color fill = new Color(WPointColor.r, WPointColor.g, WPointColor.b, itrAlpha);
            Color wire = new Color(WPointColor.r, WPointColor.g, WPointColor.b, 1f);

            for (int i = 0; i < frameD.wpoints.Count; i++)
            {
                var wp = frameD.wpoints[i];
                if (wp == null) continue;
                if (wp.kind != 1 && wp.kind != 2 && wp.kind != 7) continue;
                if (wp.w <= 0 || wp.h <= 0) continue;

                float localX = facingLeft ? (spriteW - wp.x - wp.w) : wp.x;
                var vol = new PhysicsState.FlfVolume(
                    weapon.PS.sx, weapon.PS.sy, weapon.PS.sz,
                    localX, wp.y,
                    wp.w, wp.h,
                    NTSDGlobal.Default.Itr.ZWidth
                );

                DrawVolume(vol, fill, wire);
            }
        }

        /// <summary>
        /// 将 FlfVolume（像素坐标）转换为 Unity world space 并绘制。
        ///
        /// 坐标系映射（本项目）：
        ///   FLF ps.x  → Unity world X = ps.x / ppu
        ///   FLF ps.z  → Unity world Y = ps.z / ppu  （深度映射到 Unity Y）
        ///   FLF ps.y  → 视觉偏移 localY = -ps.y / ppu （跳跃高度，不影响 root 位置）
        ///   Unity Z   = 0（不用于角色位置）
        ///
        /// FlfVolume 字段：
        ///   vol.x = sx = ps.x ± centerx  （精灵原点屏幕 X）
        ///   vol.y = sy = ps.y + ps.z - centery  （精灵原点屏幕 Y，向下为正）
        ///   vol.z = sz = ps.z  （深度像素）
        ///   vol.vx = body.x（或镜像）, vol.vy = body.y
        ///
        /// 推导 Unity world Y 中心：
        ///   center_Y = (ps.z - ps.y + centery - body.y - body.h/2) / ppu
        ///   因为 ps.y - centery = vol.y - vol.z，代入得：
        ///   center_Y = (2*vol.z - vol.y - vol.vy - vol.h/2) / ppu
        /// </summary>
        private static void DrawVolume(PhysicsState.FlfVolume vol, Color fill, Color wire)
        {
            float ppu = SimulationConstants.PIXELS_PER_UNIT;

            // Unity world X（FLF 屏幕 X 直接除以 ppu）
            float worldLeft  = (vol.x + vol.vx) / ppu;
            float worldWidth = vol.w / ppu;

            // Unity world Y（深度 vol.z 映射到 Unity Y，屏幕 Y 向下需翻转）
            // top（Unity Y 较大）= (2*vol.z - vol.y - vol.vy) / ppu
            float worldTop    = (2f * vol.z - vol.y - vol.vy) / ppu;
            float worldHeight = vol.h / ppu;

            // 中心点
            float cx = worldLeft + worldWidth  * 0.5f;
            float cy = worldTop  - worldHeight * 0.5f;

            // Unity Z = 0（本项目深度编码在 Unity Y，不用 Unity Z）
            // 给一个小厚度让 Gizmo 可见
            float worldDepth = Mathf.Max(0.05f, (vol.zwidth > 0f ? vol.zwidth : 1f) * 2f / ppu);

            Vector3 center = new Vector3(cx, cy, 0f);
            Vector3 size   = new Vector3(worldWidth, worldHeight, worldDepth);

            Gizmos.color = fill;
            Gizmos.DrawCube(center, size);
            Gizmos.color = wire;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
