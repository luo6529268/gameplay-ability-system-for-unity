using NTSD.Simulation;
using NTSD.LevelEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Animation
{
    /// <summary>
    /// C# authority 使用 794x550 的固定逻辑屏幕坐标绘制战斗层。
    /// 这里只做权威 C# 渲染坐标到 Unity 世界坐标的映射；背景图和输出分辨率不能反向改变实体比例。
    /// </summary>
    public static class NTSDRenderSpace
    {
        public readonly struct ViewportTransformSnapshot
        {
            internal ViewportTransformSnapshot(
                float left,
                float top,
                float unitsPerPixelX,
                float unitsPerPixelY)
            {
                Left = left;
                Top = top;
                UnitsPerPixelX = unitsPerPixelX;
                UnitsPerPixelY = unitsPerPixelY;
            }

            public float Left { get; }
            public float Top { get; }
            public float UnitsPerPixelX { get; }
            public float UnitsPerPixelY { get; }

            public Vector3 ScreenPixelToWorld(float screenX, float screenY, float z = 0f)
            {
                return new Vector3(
                    Left + screenX * UnitsPerPixelX,
                    Top - screenY * UnitsPerPixelY,
                    z);
            }

            public Vector3 SnapWorldPosition(Vector3 worldPos)
            {
                return new Vector3(
                    Left + Mathf.Round((worldPos.x - Left) / UnitsPerPixelX) * UnitsPerPixelX,
                    Top - Mathf.Round((Top - worldPos.y) / UnitsPerPixelY) * UnitsPerPixelY,
                    worldPos.z);
            }
        }

        public const int SourceScreenWidth = 794;
        public const int SourceScreenHeight = 550;

        public const int ScreenWidth = SourceScreenWidth;
        public const int ScreenHeight = SourceScreenHeight;

        private static Camera _boundWorldCamera;
        private static Camera _cachedWorldCamera;
        private static BoundaryWallManager _cachedBoundaryManager;
        private static BoundaryWallManager _boundaryViewportManager;
        private static Camera _boundaryViewportCamera;
        private static int _boundaryViewportSceneHandle = int.MinValue;
        private static int _boundaryViewportFrame = int.MinValue;
        private static bool _boundaryViewportResolved;
        private static bool _hasBoundaryViewport;
        private static Rect _cachedBoundaryViewport;
        private static bool _boundaryViewportOverrideEnabledForSelfCheck;
        private static bool _boundaryViewportOverrideHasBoundsForSelfCheck;
        private static Rect _boundaryViewportOverrideForSelfCheck;
        private static Object _presentationCameraOffsetOwner;
        private static Vector3 _presentationCameraOffset;
        private static readonly Camera[] WorldCameraSearchBuffer = new Camera[16];

        public static Camera WorldCamera
        {
            get
            {
                if (_boundWorldCamera != null && _boundWorldCamera.isActiveAndEnabled)
                    return _boundWorldCamera;

                if (_cachedWorldCamera != null && _cachedWorldCamera.isActiveAndEnabled)
                    return _cachedWorldCamera;

                int cameraCount = Camera.allCamerasCount;
                if (cameraCount > WorldCameraSearchBuffer.Length)
                    return null;

                cameraCount = Camera.GetAllCameras(WorldCameraSearchBuffer);
                for (int i = 0; i < cameraCount; i++)
                {
                    Camera camera = WorldCameraSearchBuffer[i];
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
                    for (int i = 0; i < cameraCount; i++)
                    {
                        Camera camera = WorldCameraSearchBuffer[i];
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
        internal static Camera BoundWorldCameraForSelfCheck => _boundWorldCamera;
        public static Vector3 PresentationCameraOffset => _presentationCameraOffset;

        public static void SetPresentationCameraOffset(Object owner, Vector3 cameraOffset)
        {
            if (owner == null)
                return;

            _presentationCameraOffsetOwner = owner;
            _presentationCameraOffset = cameraOffset;
        }

        public static void ClearPresentationCameraOffset(Object owner)
        {
            if (owner == null || _presentationCameraOffsetOwner != owner)
                return;

            _presentationCameraOffsetOwner = null;
            _presentationCameraOffset = Vector3.zero;
        }

        public static void BindWorldCamera(Camera camera)
        {
            _boundWorldCamera = camera;
            _cachedWorldCamera = camera;
            InvalidateBoundaryViewportCache();
        }

        public static void ClearBoundWorldCamera(Camera camera)
        {
            if (_boundWorldCamera == camera)
                _boundWorldCamera = null;
            if (_cachedWorldCamera == camera)
                _cachedWorldCamera = null;
            InvalidateBoundaryViewportCache();
        }

        internal static void InvalidateBoundaryViewportCache()
        {
            _boundaryViewportResolved = false;
            _boundaryViewportFrame = int.MinValue;
        }

        internal static void SetBoundaryViewportOverrideForSelfCheck(bool hasBounds, Rect bounds)
        {
            _boundaryViewportOverrideEnabledForSelfCheck = true;
            _boundaryViewportOverrideHasBoundsForSelfCheck = hasBounds;
            _boundaryViewportOverrideForSelfCheck = bounds;
            InvalidateBoundaryViewportCache();
        }

        internal static void ClearBoundaryViewportOverrideForSelfCheck()
        {
            _boundaryViewportOverrideEnabledForSelfCheck = false;
            _boundaryViewportOverrideHasBoundsForSelfCheck = false;
            _boundaryViewportOverrideForSelfCheck = default;
            InvalidateBoundaryViewportCache();
        }

        public static ViewportTransformSnapshot CaptureViewportTransform()
        {
            GetViewport(out float left, out float top);
            if (!TryGetBoundaryViewport(out _))
                left -= _presentationCameraOffset.x;
            top -= _presentationCameraOffset.y;
            return new ViewportTransformSnapshot(
                left,
                top,
                UnitsPerPixelX,
                UnitsPerPixelY);
        }

        public static Vector3 ScreenPixelToWorld(float screenX, float screenY, float z = 0f)
        {
            GetViewport(out float left, out float top);
            return new Vector3(
                left + screenX * UnitsPerPixelX,
                top - screenY * UnitsPerPixelY,
                z);
        }

        public static Vector3 ScreenPixelToPresentationWorld(
            float screenX,
            float screenY,
            float z = 0f)
        {
            Vector3 worldPosition = ScreenPixelToWorld(screenX, screenY, z);
            if (!TryGetBoundaryViewport(out _))
                worldPosition.x -= _presentationCameraOffset.x;
            worldPosition.y -= _presentationCameraOffset.y;
            return worldPosition;
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

        public static Vector3 SnapPresentationWorldPosition(Vector3 worldPos)
        {
            ViewportTransformSnapshot viewport = CaptureViewportTransform();
            return viewport.SnapWorldPosition(worldPos);
        }

        private static void GetViewport(out float left, out float top)
        {
            Camera camera = WorldCamera;
            bool hasBoundaryViewport = TryGetBoundaryViewport(camera, out Rect worldBounds);

            if (hasBoundaryViewport)
            {
                left = worldBounds.xMin;
            }
            else if (camera != null && camera.orthographic)
            {
                float canvasWidth = SourceScreenWidth * UnitsPerPixelX;
                left = camera.transform.position.x - canvasWidth * 0.5f;
            }
            else
            {
                left = 0f;
            }

            if (camera != null && camera.orthographic)
            {
                float canvasHeight = SourceScreenHeight * UnitsPerPixelY;
                Vector3 cameraPos = camera.transform.position;
                top = cameraPos.y + canvasHeight * 0.5f;
                return;
            }

            // Horizontal logical pixels start at the walkable boundary; vertical screen pixels
            // continue to use the bound camera's fixed 550px viewport when one is available.
            if (hasBoundaryViewport)
            {
                top = worldBounds.yMax;
                return;
            }

            top = 0f;
        }

        private static bool TryGetBoundaryViewport(out Rect worldBounds)
        {
            return TryGetBoundaryViewport(WorldCamera, out worldBounds);
        }

        private static bool TryGetBoundaryViewport(Camera camera, out Rect worldBounds)
        {
            if (_boundaryViewportOverrideEnabledForSelfCheck)
            {
                worldBounds = _boundaryViewportOverrideForSelfCheck;
                return _boundaryViewportOverrideHasBoundsForSelfCheck &&
                       worldBounds.width > 0f && worldBounds.height > 0f;
            }

            int frame = Time.frameCount;
            int sceneHandle = SceneManager.GetActiveScene().handle;
            BoundaryWallManager knownManager = _cachedBoundaryManager;
            bool cachedManagerStillValid = !_hasBoundaryViewport || _boundaryViewportManager != null;
            if (_boundaryViewportResolved &&
                _boundaryViewportFrame == frame &&
                _boundaryViewportSceneHandle == sceneHandle &&
                _boundaryViewportCamera == camera &&
                _boundaryViewportManager == knownManager &&
                cachedManagerStillValid)
            {
                worldBounds = _cachedBoundaryViewport;
                return _hasBoundaryViewport;
            }

            BoundaryWallManager manager = knownManager != null
                ? knownManager
                : Object.FindObjectOfType<BoundaryWallManager>();

            _cachedBoundaryManager = manager;
            _boundaryViewportManager = manager;
            _boundaryViewportCamera = camera;
            _boundaryViewportSceneHandle = sceneHandle;
            _boundaryViewportFrame = frame;
            _boundaryViewportResolved = true;
            _hasBoundaryViewport = manager != null &&
                                   manager.TryGetWalkableBounds(out _cachedBoundaryViewport) &&
                                   _cachedBoundaryViewport.width > 0f &&
                                   _cachedBoundaryViewport.height > 0f;

            worldBounds = _cachedBoundaryViewport;
            return _hasBoundaryViewport;
        }
    }
}
