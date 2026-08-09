using System.Collections.Generic;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NTSD.Tools
{
    /// <summary>
    /// 在 Scene 视图中绘制战斗实体的 bdy / itr / wpoint 调试盒。
    /// 绿色 = bdy；红色 = 攻击 itr；橙色 = 抓取/拾取 itr；蓝色 = 其他 itr；黄色 = wpoint 拾取盒。
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

        private static readonly Color BdyColor = new Color(0.1f, 1f, 0.1f);
        private static readonly Color ItrColor0 = new Color(1f, 0.15f, 0.15f);
        private static readonly Color ItrColor123 = new Color(1f, 0.6f, 0.1f);
        private static readonly Color WPointColor = new Color(1f, 0.9f, 0.2f);
        private static readonly Color ItrColorOther = new Color(0.2f, 0.5f, 1f);

        private readonly List<LF2Entity> _entities = new List<LF2Entity>();

        private void OnDrawGizmos()
        {
            if (!showBdy && !showItr) return;

            var driver = SimulationTickDriver.Instance;
            if (driver == null) return;

#if UNITY_EDITOR
            if (showOnlySelected)
            {
                GameObject selectedObject = Selection.activeGameObject;
                LF2Entity selectedEntity = selectedObject != null
                    ? selectedObject.GetComponentInParent<LF2Entity>()
                    : null;
                if (selectedEntity != null)
                    DrawEntity(selectedEntity);
                return;
            }
#endif

            driver.World?.GetAllEntities(_entities);
            if (_entities.Count == 0) return;

            foreach (var entity in _entities)
                DrawEntity(entity);
        }

        private void DrawEntity(LF2Entity entity)
        {
            if (entity == null) return;

            LF2FrameData frameD = entity.Frame?.D;
            if (frameD == null) return;

            float spriteW = entity.GetSpriteWidthPxForCollision();
            if (spriteW <= 0f) return;

            if (showBdy)
                DrawBdyBoxes(entity, frameD, spriteW);

            if (!showItr) return;

            DrawItrBoxes(entity, frameD, spriteW);

            if (entity is LF2WeaponBase weaponBase)
                DrawWeaponPickupBoxes(weaponBase, frameD, spriteW);
        }

        private void DrawBdyBoxes(LF2Entity entity, LF2FrameData frameD, float spriteW)
        {
            if (frameD.bodies == null || frameD.bodies.Count == 0) return;

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
                InteractionArea itr = frameD.itrs[i];
                if (ShouldHideReleaseState18Itr(entity, frameD, itr)) continue;

                int kind = frameD.itrs[i].kind;
                Color baseColor = kind == 0 || kind == 4 || kind == 5
                    ? ItrColor0
                    : kind == 1 || kind == 2 || kind == 3
                        ? ItrColor123
                        : ItrColorOther;

                Color fill = new Color(baseColor.r, baseColor.g, baseColor.b, itrAlpha);
                Color wire = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                DrawVolume(vols[i], fill, wire);
            }
        }

        private static bool ShouldHideReleaseState18Itr(LF2Entity entity, LF2FrameData frameD, InteractionArea itr)
        {
            if (entity == null || frameD == null || itr == null) return false;
            if (frameD.state != LF2States.Burning) return false;
            if (itr.effect == 21 || itr.effect == 22) return false;
            return entity is LF2Character || entity is LF2WeaponBase || entity is LF2SpecialAttack;
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

                float localX = facingLeft ? spriteW - wp.x - wp.w : wp.x;
                var vol = new PhysicsState.BattleVolume(
                    weapon.PS.sx,
                    weapon.PS.sy,
                    weapon.PS.sz,
                    localX,
                    wp.y,
                    wp.w,
                    wp.h,
                    NTSDGlobal.Default.Itr.ZWidth
                );

                DrawVolume(vol, fill, wire);
            }
        }

        /// <summary>
        /// 将 NTSD 像素坐标的碰撞体积绘制到 Unity 世界坐标。
        /// BattleVolume.y 已经是屏幕 Y 原点；屏幕 Y 向下为正，Unity Y 向上为正。
        /// </summary>
        private static void DrawVolume(PhysicsState.BattleVolume vol, Color fill, Color wire)
        {
            float screenLeft = vol.x + vol.vx;
            float worldWidth = NTSDRenderSpace.PixelWidthToWorld(vol.w);

            float screenTop = vol.y + vol.vy;
            Vector3 worldTopLeft = NTSDRenderSpace.ScreenPixelToWorld(screenLeft, screenTop, 0f);
            float worldHeight = NTSDRenderSpace.PixelHeightToWorld(vol.h);

            float cx = worldTopLeft.x + worldWidth * 0.5f;
            float cy = worldTopLeft.y - worldHeight * 0.5f;
            float worldDepth = Mathf.Max(0.05f, NTSDRenderSpace.PixelHeightToWorld((vol.zwidth > 0f ? vol.zwidth : 1f) * 2f));

            Vector3 center = new Vector3(cx, cy, 0f);
            Vector3 size = new Vector3(worldWidth, worldHeight, worldDepth);

            Gizmos.color = fill;
            Gizmos.DrawCube(center, size);
            Gizmos.color = wire;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
