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
    /// 边界限制：
    ///   启用 enableBoundsClamping 后，摄像机视口不会超出 mapBoundsWorld 定义的世界矩形，
    ///   从而避免显示地图外的黑色背景。在 Inspector 中直接填写矩形即可（X=左边, Y=下边）。
    /// </summary>
    [AddComponentMenu("NTSD/Battle Camera Fit")]
    [RequireComponent(typeof(CameraFitter))]
    public class NTSDBattleCameraFit : MonoBehaviour
    {
        // ─── 跟随设置 ────────────────────────────────────────────────────────
        [Header("Smooth Follow")]
        [Tooltip("诊断模式：开启后摄像机立即跳到目标，无任何平滑。\n" +
                 "若残影消失 → 是摄像机滞后问题；若仍有残影 → 是渲染级别问题（TAA/MotionBlur）。")]
        [SerializeField] private bool snapMode = false;

        [Tooltip("摄像机到达目标所需时间（秒）。越小越紧跟，建议范围 0.05–0.15。")]
        [SerializeField] private float smoothTime = 0.08f;

        [Tooltip("摄像机最大移动速度（世界单位/秒）。防止位移突然过大。")]
        [SerializeField] private float maxFollowSpeed = 40f;

        // ─── 取景设置 ────────────────────────────────────────────────────────
        [Header("Framing")]
        [Tooltip("目标周围的世界单位留白（X = 水平，Y = 垂直）。")]
        [SerializeField] private Vector2 padding = new Vector2(3f, 2f);

        [Tooltip("最小 orthographicSize（透视为最小 FOV）。防止角色叠在一起时视野过小。")]
        [SerializeField] private float minZoom = 4f;

        [Tooltip("固定视野模式：开启后缩放锁定在 minZoom，摄像机只跟随位置不动态缩放。\n" +
                 "用于测试固定视野是否消除角色拉伸问题。")]
        [SerializeField] private bool fixedViewport = false;

        // ─── 地图边界限制 ─────────────────────────────────────────────────────
        [Header("Map Bounds（防止显示黑色背景）")]
        [Tooltip("启用后，摄像机视口不会超出 mapBoundsWorld 定义的地图范围。")]
        [SerializeField] private bool enableBoundsClamping = true;

        [Tooltip("地图世界坐标矩形（X=左边, Y=下边, Width=宽, Height=高）。\n" +
                 "根据实际地图尺寸在此处直接填写。")]
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

        [Tooltip("始终将 Manual Targets 追加到追踪列表，即使 Simulation 已有活体。\n测试用，生产环境保持 false。")]
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
            Transform[] pivots = GatherPivots();
            if (pivots == null || pivots.Length == 0) return;

            _fitter.Padding      = padding;
            _fitter.FitPivots    = pivots;
            _fitter.FitPositions = Array.Empty<Vector3>();
            _fitter.FitMeshes    = Array.Empty<MeshFilter>();

            Camera cam = _fitter.Camera;
            if (cam == null) return;

            // 保存本帧实际位置与缩放（SmoothDamp 的起点）
            Vector3 prevPos  = cam.transform.position;
            float   prevZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;

            // Fit() 计算目标位置并写入摄像机，我们从返回值里取目标值
            CameraFitData fitData = _fitter.Fit();

            if (snapMode)
            {
                // ── 诊断模式：瞬间跟随，无任何延迟 ──
                // 摄像机已由 Fit() 写入目标位置，无需额外操作
                _posVelocity  = Vector3.zero;
                _zoomVelocity = 0f;
            }
            else
            {
                // ── 正常模式：SmoothDamp 平滑跟随 ──
                Vector3 newPos = Vector3.SmoothDamp(
                    prevPos, fitData.FitPosition, ref _posVelocity, smoothTime, maxFollowSpeed);
                newPos.z = prevPos.z;               // 保持摄像机深度不变
                cam.transform.position = newPos;

                if (fixedViewport)
                {
                    _zoomVelocity = 0f;
                    // 自动对齐到最近的整数像素比，消除非整数缩放引起的拉伸
                    // 例：1080p + PPU=100 + minZoom=4 → ratio=1 → size=5.4（1:1 完美像素）
                    if (cam.orthographic && pixelsPerUnit > 0f)
                    {
                        float screenH = cam.pixelHeight;
                        float ratio   = Mathf.Round(screenH / (2f * pixelsPerUnit * minZoom));
                        if (ratio < 1f) ratio = 1f;
                        cam.orthographicSize = screenH / (2f * pixelsPerUnit * ratio);
                    }
                }
                else
                {
                    float newZoom = Mathf.SmoothDamp(
                        prevZoom, fitData.FitZoom, ref _zoomVelocity, smoothTime * 1.5f, float.MaxValue);
                    if (cam.orthographic)
                        cam.orthographicSize = newZoom;
                    else
                        cam.fieldOfView = newZoom;
                }
            }

            EnforceMinZoom();

            if (enableBoundsClamping)
            {
                EnforceMaxZoom();   // 先限制最大缩放，再 clamp 位置
                ClampToBounds();
            }

            if (pixelsPerUnit > 0f)
                SnapToPixelGrid();
        }

        // ─── 像素网格对齐 ────────────────────────────────────────────────────

        /// <summary>
        /// 将摄像机 XY 坐标对齐到像素网格，消除精灵双线性采样导致的帧模糊。
        /// pixelsPerUnit 应与精灵导入设置一致（默认 100）。
        /// </summary>
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
                // 透视摄像机：以某个参考深度估算可视范围（取相机到 z=0 的距离）
                float dist = Mathf.Abs(cam.transform.position.z);
                halfH = dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                halfW = halfH * cam.aspect;
            }

            // 允许的摄像机中心范围
            float xMin = mapBoundsWorld.xMin + halfW;
            float xMax = mapBoundsWorld.xMax - halfW;
            float yMin = mapBoundsWorld.yMin + halfH;
            float yMax = mapBoundsWorld.yMax - halfH;

            // 当摄像机视口比地图还大时（缩放太远），居中处理
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
            // 模式 1：Simulation 活体
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

                    // appendManualTargets=true 时将手动目标一并追加
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

            // 模式 2：手动目标（测试，Simulation 无活体时）
            return manualTargets;
        }

        /// <summary>
        /// LF2Character → _CharacterHub.transform（MM Character MonoBehaviour）
        /// 其余 LivingObject → Renderer.transform（LF2ObjectRenderer）
        /// </summary>
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

        /// <summary>
        /// 限制最大缩放，确保视口不超出 mapBoundsWorld。
        /// 正交摄像机：视口刚好填满地图时对应的 orthographicSize 即为上限。
        ///   最大半高 = mapHeight / 2
        ///   最大半宽 = mapWidth  / 2  → 对应 orthographicSize = mapWidth / (2 * aspect)
        /// 取两者中较小值（更严格的约束）。
        /// </summary>
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
            // 透视摄像机暂不处理（NTSD 使用正交摄像机）
        }

        // ─── Gizmos（编辑器可视化）─────────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enableBoundsClamping) return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Vector3 center = new Vector3(mapBoundsWorld.center.x, mapBoundsWorld.center.y,
                transform.position.z);
            Vector3 size   = new Vector3(mapBoundsWorld.width, mapBoundsWorld.height, 0f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
