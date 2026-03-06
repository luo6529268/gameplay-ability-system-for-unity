using System;
using System.Collections.Generic;
using Kako.CameraFit;
using MoreMountains.TopDownEngine;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Battle
{
    /// <summary>
    /// NTSD Battle 摄像机跟随控制器（基于 KAKO CameraFit）
    ///
    /// 工作模式（优先级由高到低）：
    ///   1. Simulation 活体  — 战斗运行时，自动追踪所有已注册的 LF2LivingObject
    ///   2. Manual Targets  — Inspector 手动指定的 Transform（测试 / 战斗未启动时使用）
    ///
    /// 取景逻辑：
    ///   自行计算所有 pivot 的 bounding box，然后分别加上上下左右留白，
    ///   得到非对称取景矩形，再换算为摄像机中心位置与 orthographicSize。
    ///   这样可以独立控制"上方视野"与"下方视野"，避免底部 HUD 遮挡角色。
    ///
    /// 边界限制：
    ///   启用 enableBoundsClamping 后，摄像机视口不会超出 mapBoundsWorld 定义的世界矩形。
    /// </summary>
    [AddComponentMenu("NTSD/Battle Camera Fit")]
    [RequireComponent(typeof(CameraFitter))]
    public class NTSDBattleCameraFit : MonoBehaviour
    {
        // ─── 总开关 ───────────────────────────────────────────────────────────
        [Header("Camera Fit")]
        [Tooltip("关闭后摄像机停止一切自动跟随与缩放适配，保持当前位置不变。")]
        [SerializeField] private bool enableCameraFit = true;

        // ─── 跟随设置 ────────────────────────────────────────────────────────
        [Header("Smooth Follow")]
        [Tooltip("诊断模式：开启后摄像机立即跳到目标，无任何平滑。")]
        [SerializeField] private bool snapMode = false;

        [Tooltip("摄像机到达目标所需时间（秒）。越小越紧跟，建议范围 0.05–0.15。")]
        [SerializeField] private float smoothTime = 0.08f;

        [Tooltip("摄像机最大移动速度（世界单位/秒）。防止位移突然过大。")]
        [SerializeField] private float maxFollowSpeed = 40f;

        // ─── 取景设置（非对称留白）────────────────────────────────────────────
        [Header("Framing — Asymmetric Padding（世界单位）")]
        [Tooltip("角色左右两侧的留白。")]
        [SerializeField] private float paddingHorizontal = 3f;

        [Tooltip("角色上方留白。增大可让上方视野更开阔，适合俯瞰感强的场景。")]
        [SerializeField] private float paddingTop = 4f;

        [Tooltip("角色下方留白。HUD 遮挡时可缩小此值，让角色在画面中更靠下。")]
        [SerializeField] private float paddingBottom = 1f;

        [Tooltip("最小 orthographicSize。防止角色叠在一起时视野过小。")]
        [SerializeField] private float minZoom = 4f;

        // ─── 地图边界限制 ─────────────────────────────────────────────────────
        [Header("Map Bounds（防止显示黑色背景）")]
        [Tooltip("启用后，摄像机视口不会超出 mapBoundsWorld 定义的地图范围。")]
        [SerializeField] private bool enableBoundsClamping = true;

        [Tooltip("地图世界坐标矩形（X=左边, Y=下边, Width=宽, Height=高）。")]
        [SerializeField] private Rect mapBoundsWorld = new Rect(-20f, -15f, 40f, 30f);

        // ─── 像素对齐 ────────────────────────────────────────────────────────
        [Header("Pixel Snap")]
        [Tooltip("像素每单位，与精灵导入设置一致（通常 100）。0 = 不启用像素对齐。")]
        [SerializeField] private float pixelsPerUnit = 100f;

        // ─── 手动目标（测试用）────────────────────────────────────────────────
        [Header("Manual Targets（测试 / 战斗未启动时）")]
        [Tooltip("手动指定追踪的 Transform。\n" +
                 "appendManualTargets=false：Simulation 无活体时作为回退使用。\n" +
                 "appendManualTargets=true：始终追加到 Simulation 活体列表（测试多目标）。")]
        [SerializeField] private Transform[] manualTargets = Array.Empty<Transform>();

        [Tooltip("始终将 Manual Targets 追加到追踪列表，即使 Simulation 已有活体。")]
        [SerializeField] private bool appendManualTargets = false;

        // ─── 运行时缓存 ──────────────────────────────────────────────────────
        private CameraFitter _fitter;
        private readonly List<LF2LivingObject> _living = new List<LF2LivingObject>(16);
        private Transform[] _pivotBuffer = Array.Empty<Transform>();
        private Vector3 _posVelocity;
        private float   _zoomVelocity;

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _fitter = GetComponent<CameraFitter>();
        }

        private void LateUpdate()
        {
            if (!enableCameraFit) return;

            Transform[] pivots = GatherPivots();
            if (pivots == null || pivots.Length == 0) return;

            Camera cam = _fitter.Camera;
            if (cam == null) return;

            // 保存本帧实际位置与缩放（SmoothDamp 的起点）
            Vector3 prevPos  = cam.transform.position;
            float   prevZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;

            // 用自定义非对称算法计算目标位置与缩放
            ComputeFit(pivots, cam, out Vector3 fitPos, out float fitZoom);

            if (snapMode)
            {
                // ── 诊断模式：瞬间跟随 ──
                cam.transform.position = new Vector3(fitPos.x, fitPos.y, prevPos.z);
                if (cam.orthographic) cam.orthographicSize = fitZoom;
                else                  cam.fieldOfView      = fitZoom;
                _posVelocity  = Vector3.zero;
                _zoomVelocity = 0f;
            }
            else
            {
                // ── 正常模式：SmoothDamp 平滑跟随 ──
                Vector3 newPos = Vector3.SmoothDamp(
                    prevPos, fitPos, ref _posVelocity, smoothTime, maxFollowSpeed);
                newPos.z = prevPos.z;
                cam.transform.position = newPos;

                float newZoom = Mathf.SmoothDamp(
                    prevZoom, fitZoom, ref _zoomVelocity, smoothTime * 1.5f, float.MaxValue);
                if (cam.orthographic) cam.orthographicSize = newZoom;
                else                  cam.fieldOfView      = newZoom;
            }

            EnforceMinZoom();

            if (enableBoundsClamping)
            {
                EnforceMaxZoom();
                ClampToBounds();
            }

            if (pixelsPerUnit > 0f)
                SnapToPixelGrid();
        }

        // ─── 核心：非对称取景计算 ─────────────────────────────────────────────

        /// <summary>
        /// 根据所有 pivot 的 bounding box 和非对称留白，计算目标相机位置与 orthographicSize。
        ///
        /// 取景矩形：
        ///   left   = xMin - paddingHorizontal
        ///   right  = xMax + paddingHorizontal
        ///   bottom = yMin - paddingBottom
        ///   top    = yMax + paddingTop
        ///
        /// 相机中心  = 取景矩形中心
        /// halfH     = 取景矩形高度 / 2
        /// halfW     = 取景矩形宽度 / 2
        /// zoom      = max(halfH, halfW / aspect)   确保宽和高都能放进视口
        /// </summary>
        private void ComputeFit(Transform[] pivots, Camera cam, out Vector3 fitPos, out float fitZoom)
        {
            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;

            foreach (var t in pivots)
            {
                if (t == null) continue;
                float px = t.position.x, py = t.position.y;
                if (px < xMin) xMin = px;
                if (px > xMax) xMax = px;
                if (py < yMin) yMin = py;
                if (py > yMax) yMax = py;
            }

            // 非对称取景矩形
            float left   = xMin - paddingHorizontal;
            float right  = xMax + paddingHorizontal;
            float bottom = yMin - paddingBottom;
            float top    = yMax + paddingTop;

            // 目标相机中心
            float centerX = (left + right)   * 0.5f;
            float centerY = (bottom + top)   * 0.5f;

            // 所需半高 / 半宽
            float halfH = (top - bottom) * 0.5f;
            float halfW = (right - left) * 0.5f;

            // orthographicSize = max(半高, 半宽/aspect) —— 确保取景矩形完整可见
            float zoom = cam.orthographic
                ? Mathf.Max(halfH, halfW / cam.aspect)
                : Mathf.Atan(halfH / Mathf.Abs(cam.transform.position.z)) * 2f * Mathf.Rad2Deg;

            fitPos  = new Vector3(centerX, centerY, cam.transform.position.z);
            fitZoom = zoom;
        }

        // ─── 像素网格对齐 ────────────────────────────────────────────────────

        private void SnapToPixelGrid()
        {
            Camera cam = _fitter.Camera;
            if (cam == null) return;

            Vector3 pos = cam.transform.position;
            pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
            pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
            cam.transform.position = pos;
        }

        private void ClampToBounds()
        {
            Camera cam = _fitter.Camera;
            if (cam == null) return;

            float halfH, halfW;
            if (cam.orthographic)
            {
                halfH = cam.orthographicSize;
                halfW = cam.orthographicSize * cam.aspect;
            }
            else
            {
                float dist = Mathf.Abs(cam.transform.position.z);
                halfH = dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                halfW = halfH * cam.aspect;
            }

            float xMin = mapBoundsWorld.xMin + halfW;
            float xMax = mapBoundsWorld.xMax - halfW;
            float yMin = mapBoundsWorld.yMin + halfH;
            float yMax = mapBoundsWorld.yMax - halfH;

            if (xMin > xMax) xMin = xMax = mapBoundsWorld.center.x;
            if (yMin > yMax) yMin = yMax = mapBoundsWorld.center.y;

            Vector3 pos = cam.transform.position;
            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
            pos.y = Mathf.Clamp(pos.y, yMin, yMax);
            cam.transform.position = pos;
        }

        // ─── Pivot 解析 ──────────────────────────────────────────────────────

        private Transform[] GatherPivots()
        {
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            if (world != null)
            {
                _living.Clear();
                world.GetAllLivingObjects(_living);

                if (_living.Count > 0)
                {
                    int extra = appendManualTargets ? manualTargets.Length : 0;
                    int need  = _living.Count + extra;
                    if (_pivotBuffer.Length < need)
                        _pivotBuffer = new Transform[need];

                    int count = 0;
                    for (int i = 0; i < _living.Count; i++)
                    {
                        Transform t = ResolvePivot(_living[i]);
                        if (t != null) _pivotBuffer[count++] = t;
                    }

                    for (int i = 0; i < extra; i++)
                    {
                        if (manualTargets[i] != null)
                            _pivotBuffer[count++] = manualTargets[i];
                    }

                    if (count > 0)
                    {
                        if (count == _pivotBuffer.Length) return _pivotBuffer;
                        Transform[] trimmed = new Transform[count];
                        Array.Copy(_pivotBuffer, trimmed, count);
                        return trimmed;
                    }
                }
            }

            return manualTargets;
        }

        private static Transform ResolvePivot(LF2LivingObject obj)
        {
            if (obj == null) return null;
            if (obj._CharacterHub != null && obj._CharacterHub.isActiveAndEnabled)
                return obj._CharacterHub.transform;
            if (obj.Renderer != null && obj.Renderer.isActiveAndEnabled)
                return obj.Renderer.transform;
            return null;
        }

        private void EnforceMinZoom()
        {
            Camera cam = _fitter.Camera;
            if (cam == null) return;
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Max(cam.orthographicSize, minZoom);
            else
                cam.fieldOfView = Mathf.Max(cam.fieldOfView, minZoom);
        }

        private void EnforceMaxZoom()
        {
            Camera cam = _fitter.Camera;
            if (cam == null) return;
            if (cam.orthographic)
            {
                float maxByH = mapBoundsWorld.height * 0.5f;
                float maxByW = mapBoundsWorld.width  * 0.5f / cam.aspect;
                cam.orthographicSize = Mathf.Min(cam.orthographicSize, Mathf.Min(maxByH, maxByW));
            }
        }

        // ─── Gizmos ──────────────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enableBoundsClamping) return;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Vector3 center = new Vector3(mapBoundsWorld.center.x, mapBoundsWorld.center.y,
                transform.position.z);
            Vector3 size = new Vector3(mapBoundsWorld.width, mapBoundsWorld.height, 0f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
