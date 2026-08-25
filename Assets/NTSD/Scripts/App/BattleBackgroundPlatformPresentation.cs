using UnityEngine;

namespace NTSD.App
{
    public enum BattleBackgroundPresentationPlatform
    {
        Desktop = 0,
        Mobile = 1,
    }

    public enum BattleBackgroundEditorPreviewMode
    {
        Automatic = 0,
        Desktop = 1,
        Mobile = 2,
    }

    /// <summary>
    /// Owns the local visual frame for the real Bg (2) world Sprite. Scene View and Game
    /// therefore share one map coordinate system; only the final mobile black overlay is
    /// screen-local.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("NTSD/战斗背景平台表现")]
    public sealed class BattleBackgroundPlatformPresentation : MonoBehaviour
    {
        public const string BottomOverlayShaderResourcePath = "BattleBackgroundScreen";
        public const float DefaultAndroidBottomGapNormalized = 1f / 9f;
        public const float MaximumAndroidBottomGapNormalized = 0.5f;
        private const float MinimumAspect = 0.0001f;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer sourceRenderer;
        [SerializeField, InspectorName("编辑器平台预览")]
        [Tooltip("仅在 Unity Editor 中覆盖预览。Player 始终根据真实平台选择布局。")]
        private BattleBackgroundEditorPreviewMode editorPreviewMode =
            BattleBackgroundEditorPreviewMode.Automatic;
        [SerializeField, InspectorName("编辑器实时相机取景")]
        [Tooltip("开启后，Edit Mode 会根据 Bg (2) 当前 Sprite 的 world bounds 实时更新目标相机的位置和正交尺寸。关闭后恢复本次预览前的相机取景。")]
        private bool editorLiveCameraFrame = true;
        [SerializeField, Range(0f, MaximumAndroidBottomGapNormalized)]
        [InspectorName("Android 底部黑区比例")]
        [Tooltip("仅影响 Android/iOS 的本地视觉取景和最终黑色覆盖层；不会改 Bg Transform 或战斗实体逻辑。")]
        private float androidBottomGapNormalized = DefaultAndroidBottomGapNormalized;

        private BattleBackgroundBottomOverlayPresenter bottomOverlayPresenter;
        private Camera framedCamera;
        private Vector3 capturedCameraPosition;
        private float capturedOrthographicSize;
        private bool hasCapturedCameraFrame;

        public BattleBackgroundEditorPreviewMode EditorPreviewMode
        {
            get => editorPreviewMode;
            set
            {
                if (editorPreviewMode == value)
                    return;

                editorPreviewMode = value;
                RefreshPresentation();
            }
        }

        public bool EditorLiveCameraFrame
        {
            get => editorLiveCameraFrame;
            set
            {
                if (editorLiveCameraFrame == value)
                    return;

                editorLiveCameraFrame = value;
                RefreshPresentation();
            }
        }

        private void Reset()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            ResolveDependencies();
            RefreshPresentation();
        }

        private void OnValidate()
        {
            ResolveDependencies();
            RefreshPresentation();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && editorLiveCameraFrame)
                RefreshPresentation();
#endif
        }

        private void OnDisable()
        {
            ReleasePresentation();
        }

        private void OnDestroy()
        {
            ReleasePresentation();
        }

        public void RefreshPresentation()
        {
            ResolveDependencies();
            if (sourceRenderer == null || targetCamera == null)
            {
                ReleasePresentation();
                return;
            }

            if (!IsValidSourceRendererOwner(gameObject, sourceRenderer) ||
                sourceRenderer.sprite == null)
            {
                ReleasePresentation();
                return;
            }

            BattleBackgroundPresentationPlatform platform =
                ResolvePresentationPlatform(Application.platform, editorPreviewMode);
            float bottomGap = ResolveBottomGapNormalized(
                platform,
                androidBottomGapNormalized);

            if (ShouldApplyWorldCameraFrame(
                    Application.isPlaying,
                    editorLiveCameraFrame) &&
                !TryApplyWorldCameraFrame(bottomGap))
            {
                ReleasePresentation();
                return;
            }

            if (!ShouldApplyWorldCameraFrame(
                    Application.isPlaying,
                    editorLiveCameraFrame))
            {
                RestoreCapturedCameraFrame();
            }

            bottomOverlayPresenter ??= new BattleBackgroundBottomOverlayPresenter();
            bottomOverlayPresenter.Refresh(targetCamera, bottomGap);
        }

        public static bool ShouldApplyWorldCameraFrame(
            bool isPlaying,
            bool editorLiveCameraFrame)
        {
#if UNITY_EDITOR
            return isPlaying || editorLiveCameraFrame;
#else
            return true;
#endif
        }

        public static bool IsValidSourceRendererOwner(
            GameObject presentationObject,
            SpriteRenderer renderer)
        {
            return presentationObject != null &&
                   renderer != null &&
                   renderer.gameObject == presentationObject;
        }

        public static BattleBackgroundPresentationPlatform ResolvePresentationPlatform(
            RuntimePlatform platform,
            BattleBackgroundEditorPreviewMode previewMode =
                BattleBackgroundEditorPreviewMode.Automatic)
        {
#if UNITY_EDITOR
            switch (previewMode)
            {
                case BattleBackgroundEditorPreviewMode.Desktop:
                    return BattleBackgroundPresentationPlatform.Desktop;
                case BattleBackgroundEditorPreviewMode.Mobile:
                    return BattleBackgroundPresentationPlatform.Mobile;
            }
#endif

            return platform == RuntimePlatform.Android ||
                   platform == RuntimePlatform.IPhonePlayer
                ? BattleBackgroundPresentationPlatform.Mobile
                : BattleBackgroundPresentationPlatform.Desktop;
        }

        public static float ResolveBottomGapNormalized(
            BattleBackgroundPresentationPlatform platform,
            float configuredBottomGapNormalized)
        {
            if (platform != BattleBackgroundPresentationPlatform.Mobile)
                return 0f;

            return Mathf.Clamp(
                configuredBottomGapNormalized,
                0f,
                MaximumAndroidBottomGapNormalized);
        }

        /// <summary>
        /// Resolves the real orthographic world rect seen by the presentation camera.
        /// A mobile bottom gap moves the frame below the map so source loss occurs only
        /// above the map, while the lower map edge remains visible directly above black.
        /// </summary>
        public static Rect ResolveWorldCameraFrame(
            Bounds backgroundBounds,
            float outputAspect,
            float bottomGapNormalized)
        {
            if (backgroundBounds.size.x <= MinimumAspect ||
                backgroundBounds.size.y <= MinimumAspect ||
                outputAspect <= MinimumAspect)
            {
                return default;
            }

            float sourceWidth = backgroundBounds.size.x;
            float sourceHeight = backgroundBounds.size.y;
            float sourceAspect = sourceWidth / sourceHeight;
            float frameWidth;
            float frameHeight;

            if (outputAspect >= sourceAspect)
            {
                frameWidth = sourceWidth;
                frameHeight = sourceWidth / outputAspect;
            }
            else
            {
                frameHeight = sourceHeight;
                frameWidth = sourceHeight * outputAspect;
            }

            float bottomGap = Mathf.Clamp01(bottomGapNormalized);
            float frameBottom = backgroundBounds.min.y - frameHeight * bottomGap;
            return Rect.MinMaxRect(
                backgroundBounds.center.x - frameWidth * 0.5f,
                frameBottom,
                backgroundBounds.center.x + frameWidth * 0.5f,
                frameBottom + frameHeight);
        }

        public static float ResolveOutputAspect(Camera camera)
        {
            if (camera == null)
                return 16f / 9f;

            if (camera.pixelWidth > 0 && camera.pixelHeight > 0)
                return camera.pixelWidth / (float)camera.pixelHeight;

            return camera.aspect > MinimumAspect
                ? camera.aspect
                : 16f / 9f;
        }

        private void ResolveDependencies()
        {
            if (sourceRenderer == null)
                sourceRenderer = GetComponent<SpriteRenderer>();

            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void ReleasePresentation()
        {
            bottomOverlayPresenter?.Dispose();
            bottomOverlayPresenter = null;
            RestoreCapturedCameraFrame();
        }

        private bool TryApplyWorldCameraFrame(float bottomGap)
        {
            if (targetCamera == null ||
                sourceRenderer == null ||
                sourceRenderer.sprite == null ||
                !targetCamera.orthographic)
            {
                return false;
            }

            CaptureCameraFrame(targetCamera);
            Rect cameraFrame = ResolveWorldCameraFrame(
                sourceRenderer.bounds,
                ResolveOutputAspect(targetCamera),
                bottomGap);
            if (cameraFrame.width <= MinimumAspect || cameraFrame.height <= MinimumAspect)
                return false;

            Vector3 position = targetCamera.transform.position;
            position.x = cameraFrame.center.x;
            position.y = cameraFrame.center.y;
            if (targetCamera.transform.position != position)
                targetCamera.transform.position = position;

            float orthographicSize = cameraFrame.height * 0.5f;
            if (!Mathf.Approximately(targetCamera.orthographicSize, orthographicSize))
                targetCamera.orthographicSize = orthographicSize;
            return true;
        }

        private void CaptureCameraFrame(Camera camera)
        {
            if (hasCapturedCameraFrame && framedCamera == camera)
                return;

            RestoreCapturedCameraFrame();
            framedCamera = camera;
            capturedCameraPosition = camera.transform.position;
            capturedOrthographicSize = camera.orthographicSize;
            hasCapturedCameraFrame = true;
        }

        private void RestoreCapturedCameraFrame()
        {
            if (!hasCapturedCameraFrame)
                return;

            if (framedCamera != null)
            {
                if (framedCamera.transform.position != capturedCameraPosition)
                    framedCamera.transform.position = capturedCameraPosition;
                if (!Mathf.Approximately(
                        framedCamera.orthographicSize,
                        capturedOrthographicSize))
                {
                    framedCamera.orthographicSize = capturedOrthographicSize;
                }
            }

            framedCamera = null;
            capturedCameraPosition = default;
            capturedOrthographicSize = 0f;
            hasCapturedCameraFrame = false;
        }
    }
}
