using NTSD.Simulation;
using NTSD.LevelEditor;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// C++ release 使用 794x550 的固定逻辑屏幕坐标绘制战斗层。
    /// 这里只做 C++ 屏幕像素到 Unity 世界坐标的映射；背景图和输出分辨率不能反向改变实体比例。
    /// </summary>
    public static class NTSDRenderSpace
    {
        public const int SourceScreenWidth = 794;
        public const int SourceScreenHeight = 550;

        public const int ScreenWidth = SourceScreenWidth;
        public const int ScreenHeight = SourceScreenHeight;

        private static Camera _boundWorldCamera;
        private static Camera _cachedWorldCamera;
        private static BoundaryWallManager _cachedBoundaryManager;

        public static Camera WorldCamera
        {
            get
            {
                if (_boundWorldCamera != null && _boundWorldCamera.isActiveAndEnabled)
                    return _boundWorldCamera;

                if (_cachedWorldCamera != null && _cachedWorldCamera.isActiveAndEnabled)
                    return _cachedWorldCamera;

                Camera[] cameras = Camera.allCameras;
                for (int i = 0; i < cameras.Length; i++)
                {
                    Camera camera = cameras[i];
                    if (camera != null && camera.isActiveAndEnabled && camera.name == "ScenesCamera")
                    {
                        _cachedWorldCamera = camera;
                        return _cachedWorldCamera;
                    }
                }

                int battleLayer = LayerMask.NameToLayer("Battle");
                if (battleLayer >= 0)
                {
                    int battleMask = 1 << battleLayer;
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        Camera camera = cameras[i];
                        if (camera != null && camera.isActiveAndEnabled && (camera.cullingMask & battleMask) != 0)
                        {
                            _cachedWorldCamera = camera;
                            return _cachedWorldCamera;
                        }
                    }
                }

                return null;
            }
        }

        public static float UnitsPerPixelX => 1f / SimulationConstants.PIXELS_PER_UNIT;
        public static float UnitsPerPixelY => 1f / SimulationConstants.PIXELS_PER_UNIT;
        public const float BattleVisualScale = 1.5f;
        public static Vector3 RenderScale => Vector3.one * BattleVisualScale;

        public static void BindWorldCamera(Camera camera)
        {
            _boundWorldCamera = camera;
            _cachedWorldCamera = camera;
        }

        public static void ClearBoundWorldCamera(Camera camera)
        {
            if (_boundWorldCamera == camera)
                _boundWorldCamera = null;
            if (_cachedWorldCamera == camera)
                _cachedWorldCamera = null;
        }

        public static Vector3 ScreenPixelToWorld(float screenX, float screenY, float z = 0f)
        {
            GetViewport(out float left, out float top);
            return new Vector3(
                left + screenX * UnitsPerPixelX,
                top - screenY * UnitsPerPixelY,
                z);
        }

        public static Vector2 WorldToScreenPixel(Vector3 worldPos)
        {
            GetViewport(out float left, out float top);
            return new Vector2(
                (worldPos.x - left) / UnitsPerPixelX,
                (top - worldPos.y) / UnitsPerPixelY);
        }

        public static Vector2 GroundPixelToWorld(float ntsdX, float ntsdZ)
        {
            GetViewport(out float left, out float top);
            return new Vector2(
                left + ntsdX * UnitsPerPixelX,
                top - ntsdZ * UnitsPerPixelY);
        }

        public static Vector2 WorldToGroundPixel(Vector3 worldPos)
        {
            return WorldToScreenPixel(worldPos);
        }

        public static bool TryGetStagePixelBounds(out Rect bounds)
        {
            BoundaryWallManager manager = _cachedBoundaryManager != null
                ? _cachedBoundaryManager
                : Object.FindObjectOfType<BoundaryWallManager>();

            if (manager != null)
            {
                _cachedBoundaryManager = manager;
                if (manager.TryGetBattleStagePixelBounds(out bounds))
                    return true;
            }

            if (TryGetBoundaryViewport(out Rect worldBounds))
            {
                bounds = new Rect(
                    0f,
                    0f,
                    worldBounds.width / UnitsPerPixelX,
                    worldBounds.height / UnitsPerPixelY);
                return bounds.width > 0f && bounds.height > 0f;
            }

            bounds = new Rect(0f, 0f, SourceScreenWidth, SourceScreenHeight);
            return false;
        }

        public static bool TryGetStageWorldBounds(out Rect bounds)
        {
            if (TryGetBoundaryViewport(out Rect worldBounds))
            {
                bounds = worldBounds;
                return true;
            }

            bounds = default;
            return false;
        }

        public static float PixelWidthToWorld(float pixels)
        {
            return pixels * UnitsPerPixelX;
        }

        public static float PixelHeightToWorld(float pixels)
        {
            return pixels * UnitsPerPixelY;
        }

        public static Vector3 SnapWorldPosition(Vector3 worldPos)
        {
            GetViewport(out float left, out float top);
            return new Vector3(
                left + Mathf.Round((worldPos.x - left) / UnitsPerPixelX) * UnitsPerPixelX,
                top - Mathf.Round((top - worldPos.y) / UnitsPerPixelY) * UnitsPerPixelY,
                worldPos.z);
        }

        private static void GetViewport(out float left, out float top)
        {
            Camera camera = WorldCamera;
            if (camera != null && camera.orthographic)
            {
                float canvasWidth = SourceScreenWidth * UnitsPerPixelX;
                float canvasHeight = SourceScreenHeight * UnitsPerPixelY;
                Vector3 cameraPos = camera.transform.position;
                left = cameraPos.x - canvasWidth * 0.5f;
                top = cameraPos.y + canvasHeight * 0.5f;
                return;
            }

            // 只有在场景相机还未绑定的兜底路径下，才退回到 Boundary 外接框。
            // 正式战斗中的屏幕像素坐标必须跟随 ScenesCamera 的固定 794x550 逻辑视口，
            // 不能直接把整张可走区域当成 draw_entity 的 camera viewport。
            if (TryGetBoundaryViewport(out Rect worldBounds))
            {
                left = worldBounds.xMin;
                top = worldBounds.yMax;
                return;
            }

            left = 0f;
            top = 0f;
        }

        private static bool TryGetBoundaryViewport(out Rect worldBounds)
        {
            worldBounds = default;

            BoundaryWallManager manager = _cachedBoundaryManager != null
                ? _cachedBoundaryManager
                : Object.FindObjectOfType<BoundaryWallManager>();

            if (manager == null)
                return false;

            _cachedBoundaryManager = manager;
            return manager.TryGetWalkableBounds(out worldBounds);
        }
    }
}
