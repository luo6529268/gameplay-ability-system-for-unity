using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace NTSD.App
{
    /// <summary>
    /// 战斗世界相机安全区域。
    ///
    /// 此组件只修改表现层状态，不会修改运行时实体位置、camera_x
    /// 或其他确定性战斗字段。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("NTSD/战斗相机安全区域")]
    public sealed class BattleCameraSafeArea : MonoBehaviour
    {
        private enum ViewportLayout
        {
            Standard,
            Ultrawide
        }

        private enum EditorViewportPreview
        {
            [InspectorName("自动（按屏幕类型）")]
            RuntimeAutomatic,
            [InspectorName("16:9 标准")]
            Standard,
            [InspectorName("20:9 超宽")]
            Ultrawide
        }

        private const float MinimumNormalizedSize = 0.05f;
        private const float MaximumNormalizedSize = 0.95f;
        private const float MinimumFollowDistance = 0.0001f;
        private const float DefaultOrthographicSize = 5.6f;
        private const float ReferenceViewportAspect = 16f / 9f;
        private const float ReferenceScreenHeight = 1080f;
        private const float UltrawideAspectThreshold = 20f / 9f;
        private const string ReservedAreaRootName = "__BattleLayoutReservedAreas";
        private const string ReservedCanvasName = "__BattleLayoutReservedCanvas";
        private const string BottomReservedAreaName = "__Bottom";
        private const string LeftReservedAreaName = "__Left";
        private const string RightReservedAreaName = "__Right";
        private static Sprite reservedAreaSprite;

        [Header("相机")]
        [InspectorName("目标相机")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("用于确定世界相机完整取景范围的背景。")]
        [InspectorName("背景渲染器")]
        [SerializeField] private SpriteRenderer backgroundRenderer;

        [Header("布局镂空")]
        [Min(0f)]
        [InspectorName("16:9 底部镂空高度（像素）")]
        [SerializeField] private float bottomReservedPixels = 260f;

        [Range(0f, 0.5f)]
        [InspectorName("20:9 单侧镂空宽度（屏幕比例）")]
        [SerializeField] private float ultrawideReservedSideFraction = 0.1f;

        [Header("编辑器预览")]
        [InspectorName("编辑器预览布局")]
        [SerializeField] private EditorViewportPreview editorViewportPreview;

        [Header("安全区域")]
        [Range(MinimumNormalizedSize, MaximumNormalizedSize)]
        [InspectorName("安全区域宽度（屏幕比例）")]
        [SerializeField] private float safeAreaWidth = 0.65f;

        [Range(MinimumNormalizedSize, MaximumNormalizedSize)]
        [InspectorName("安全区域高度（屏幕比例）")]
        [SerializeField] private float safeAreaHeight = 0.65f;

        [Range(0f, 1f)]
        [InspectorName("安全区域中心 X（0-1）")]
        [SerializeField] private float safeAreaCenterX = 0.5f;

        [Range(0f, 1f)]
        [InspectorName("安全区域中心 Y（0-1）")]
        [SerializeField] private float safeAreaCenterY = 0.5f;

        [Header("视野")]
        [Min(MinimumFollowDistance)]
        [InspectorName("16:9 参考正交尺寸")]
        [SerializeField] private float referenceOrthographicSize = DefaultOrthographicSize;

        [Range(0.1f, 1f)]
        [InspectorName("可见宽度（可行走区域比例）")]
        [SerializeField] private float visibleWidthFraction = 0.5f;

        [Header("跟随设置")]
        [InspectorName("跟随水平方向")]
        [SerializeField] private bool followHorizontal = true;
        [InspectorName("跟随垂直方向")]
        [SerializeField] private bool followVertical;
        [Min(0f)]
        [InspectorName("跟随平滑时间（秒）")]
        [SerializeField] private float followSmoothTime = 0.08f;
        [InspectorName("限制在舞台边界内")]
        [SerializeField] private bool constrainToStageBounds = true;

        [Header("跟随目标")]
        [Tooltip("启用后，将所有活动的 LF2 角色作为镜头取景目标。")]
        [InspectorName("跟随活动角色")]
        [SerializeField] private bool followActiveCharacters = true;

        [Tooltip("可选的显式跟随目标，会与活动战斗角色合并。")]
        [InspectorName("手动跟随目标")]
        [SerializeField] private Transform[] manualTargets;

        [SerializeField, HideInInspector] private bool hasSerializedBaseCameraState;
        [SerializeField, HideInInspector] private Rect serializedBaseCameraRect =
            new Rect(0f, 0f, 1f, 1f);
        [SerializeField, HideInInspector] private Vector3 serializedBaseCameraPosition;
        [SerializeField, HideInInspector] private float serializedBaseOrthographicSize =
            DefaultOrthographicSize;

        [Header("调试叠加层")]
        [InspectorName("显示安全区域叠加层")]
        [SerializeField] private bool showSafeAreaOverlay;
        [InspectorName("安全区域边框颜色")]
        [SerializeField] private Color safeAreaColor = new Color(0.15f, 1f, 0.35f, 1f);
        [InspectorName("非安全区域遮罩颜色")]
        [SerializeField] private Color unsafeAreaColor = new Color(1f, 0.2f, 0.15f, 0.12f);
        [Min(0.5f)]
        [InspectorName("边框厚度")]
        [SerializeField] private float borderThickness = 2f;

        private readonly List<Vector2> targetWorldScratch = new List<Vector2>(16);

        private Vector3 followVelocity;
        private Vector3 baseCameraPosition;
        private Rect baseCameraRect = new Rect(0f, 0f, 1f, 1f);
        private float baseOrthographicSize = DefaultOrthographicSize;
        private bool hasBaseCameraState;
        private Rect safeAreaScreenRect;
        private Rect safeAreaWorldRect;
        private bool hasSafeAreaWorldRect;
        private bool hasCameraBounds;
        private Rect cameraBounds;
        private bool hasBackgroundBounds;
        private Bounds backgroundBounds;
        private ViewportLayout activeLayout;
        private Canvas battleCanvas;
        private bool battleCanvasResolutionAttempted;
        private RectTransform reservedAreaRoot;
        private Image bottomReservedArea;
        private Image leftReservedArea;
        private Image rightReservedArea;
        private bool hasAppliedReservedAreaLayout;
        private ViewportLayout appliedReservedAreaLayout;
        private float appliedBottomReservedFraction;
        private float appliedUltrawideReservedSideFraction;
#if UNITY_EDITOR
        private bool reservedAreaRefreshScheduled;
        private int editorPreviewScreenWidth = -1;
        private int editorPreviewScreenHeight = -1;
#endif

        public Camera TargetCamera => targetCamera;
        public Rect SafeAreaScreenRect => safeAreaScreenRect;
        public Rect SafeAreaWorldRect => safeAreaWorldRect;
        public bool HasSafeAreaWorldRect => hasSafeAreaWorldRect;

        private void Reset()
        {
            targetCamera = GetComponent<Camera>();
            CaptureSerializedBaseCameraState(force: true);
        }

        private void Awake()
        {
            ResolveCamera();
            NormalizeSettings();
            CaptureBaseCameraState(force: true);
        }

        private void OnEnable()
        {
            ResolveCamera();
            battleCanvasResolutionAttempted = false;
            ResolveBattleCanvas(force: true);
            NormalizeSettings();
            CaptureBaseCameraState(force: true);
            if (Application.isPlaying)
            {
                ApplyRuntimeLayout();
                RefreshCameraBounds();
                RefreshBackgroundBounds();
                UpdateOrthographicSize();
                ClampCameraToBounds();
                UpdateCameraView();
            }
            else
            {
                ApplyEditorLayoutPreview();
                ScheduleReservedAreaRefresh();
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= ApplyScheduledReservedAreaRefresh;
            reservedAreaRefreshScheduled = false;
#endif
            followVelocity = Vector3.zero;
            SetReservedAreasEnabled(false);
            RestoreBaseCameraState();
            NTSDRenderSpace.ClearPresentationCameraOffset(this);
        }

        private void OnDestroy()
        {
            DestroyReservedAreaRoot();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                RefreshAutomaticEditorLayoutIfNeeded();
#endif
                return;
            }

            if (targetCamera == null || !targetCamera.isActiveAndEnabled || !targetCamera.orthographic)
            {
                NTSDRenderSpace.ClearPresentationCameraOffset(this);
                return;
            }

            NormalizeSettings();
            ApplyRuntimeLayout();
            RefreshCameraBounds();
            RefreshBackgroundBounds();
            UpdateOrthographicSize();
            UpdateCameraView();
            if (hasBackgroundBounds)
            {
                targetWorldScratch.Clear();
                followVelocity = Vector3.zero;
            }
            else
            {
                UpdateTargetWorldPositions();
                ApplyCameraFollow();
            }
            ClampCameraToBounds();
            UpdateCameraView();
            NTSDRenderSpace.SetPresentationCameraOffset(
                this,
                new Vector3(
                    targetCamera.transform.position.x - baseCameraPosition.x,
                    0f,
                    0f));
        }

        private void OnValidate()
        {
            ResolveCamera();
            ResolveBattleCanvas(force: true, createIfMissing: false);
            NormalizeSettings();
            if (!Application.isPlaying && isActiveAndEnabled)
            {
                ApplyEditorLayoutPreview();
#if UNITY_EDITOR
                ScheduleReservedAreaRefresh();
                RepaintEditorPreview();
#endif
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !showSafeAreaOverlay || !hasSafeAreaWorldRect ||
                Event.current.type != EventType.Repaint)
                return;

            if (targetCamera == null || !targetCamera.isActiveAndEnabled)
                return;

            DrawSafeAreaOverlay();
        }

        private void ResolveCamera()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
        }

        private void CaptureBaseCameraState(bool force)
        {
            if (targetCamera == null || (!force && hasBaseCameraState))
                return;

            CaptureSerializedBaseCameraState(force: false);
            baseCameraPosition = serializedBaseCameraPosition;
            baseCameraRect = serializedBaseCameraRect;
            baseOrthographicSize = serializedBaseOrthographicSize;
            hasBaseCameraState = true;
        }

        private void CaptureSerializedBaseCameraState(bool force)
        {
            if (targetCamera == null || (!force && hasSerializedBaseCameraState))
                return;

            serializedBaseCameraPosition = targetCamera.transform.position;
            serializedBaseCameraRect = targetCamera.rect;
            serializedBaseOrthographicSize = targetCamera.orthographicSize;
            hasSerializedBaseCameraState = true;
        }

        private void RestoreBaseCameraState()
        {
            if (targetCamera != null && hasBaseCameraState)
            {
                SetCameraViewportIfChanged(baseCameraRect);

                if (targetCamera.transform.position != baseCameraPosition)
                    targetCamera.transform.position = baseCameraPosition;

                if (!Mathf.Approximately(
                        targetCamera.orthographicSize,
                        baseOrthographicSize))
                    targetCamera.orthographicSize = baseOrthographicSize;
            }

            hasBaseCameraState = false;
        }

        private void ApplyRuntimeLayout()
        {
            activeLayout = ResolveRuntimeViewportLayout();
            SetCameraViewportIfChanged(ResolveCameraViewport(activeLayout));
            UpdateReservedAreas(activeLayout);
        }

        private void ApplyCameraLayout(ViewportLayout layout)
        {
            activeLayout = layout;
            SetCameraViewportIfChanged(ResolveCameraViewport(layout));
        }

        private Rect ResolveCameraViewport(ViewportLayout layout)
        {
            if (layout == ViewportLayout.Standard)
            {
                float bottomReservedFraction = ResolveBottomReservedFraction();
                return new Rect(
                    0f,
                    bottomReservedFraction,
                    1f,
                    1f - bottomReservedFraction);
            }

            return new Rect(
                ultrawideReservedSideFraction,
                0f,
                1f - ultrawideReservedSideFraction * 2f,
                1f);
        }

        private ViewportLayout ResolveRuntimeViewportLayout()
        {
            return ResolveScreenAspect() >= UltrawideAspectThreshold
                ? ViewportLayout.Ultrawide
                : ViewportLayout.Standard;
        }

        private void ScheduleReservedAreaRefresh()
        {
#if UNITY_EDITOR
            if (reservedAreaRefreshScheduled)
                return;

            reservedAreaRefreshScheduled = true;
            EditorApplication.delayCall += ApplyScheduledReservedAreaRefresh;
#endif
        }

#if UNITY_EDITOR
        private void RefreshAutomaticEditorLayoutIfNeeded()
        {
            if (editorViewportPreview != EditorViewportPreview.RuntimeAutomatic ||
                (editorPreviewScreenWidth == Screen.width &&
                 editorPreviewScreenHeight == Screen.height))
            {
                return;
            }

            ApplyEditorLayoutPreview();
            ScheduleReservedAreaRefresh();
            RepaintEditorPreview();
        }

        private void ApplyScheduledReservedAreaRefresh()
        {
            reservedAreaRefreshScheduled = false;
            if (this == null || Application.isPlaying || !isActiveAndEnabled)
                return;

            ResolveBattleCanvas(force: true);
            UpdateReservedAreas(ResolveEditorViewportLayout());
            RepaintEditorPreview();
        }

        private static void RepaintEditorPreview()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
#endif

        private void ApplyEditorLayoutPreview()
        {
            if (targetCamera == null || !targetCamera.orthographic)
                return;

            CaptureBaseCameraState(force: false);
            ViewportLayout layout = ResolveEditorViewportLayout();
            ApplyCameraLayout(layout);
            RefreshCameraBounds();
            RefreshBackgroundBounds();
            UpdateOrthographicSize();
            UpdateSafeAreaRect();
#if UNITY_EDITOR
            editorPreviewScreenWidth = Screen.width;
            editorPreviewScreenHeight = Screen.height;
#endif
        }

        private ViewportLayout ResolveEditorViewportLayout()
        {
            if (editorViewportPreview == EditorViewportPreview.Standard)
                return ViewportLayout.Standard;
            if (editorViewportPreview == EditorViewportPreview.Ultrawide)
                return ViewportLayout.Ultrawide;
            return ResolveRuntimeViewportLayout();
        }

        private float ResolveScreenAspect()
        {
            if (Screen.height > 0)
                return Screen.width / (float)Screen.height;

            if (targetCamera != null && targetCamera.aspect > MinimumFollowDistance)
                return targetCamera.aspect;

            return 16f / 9f;
        }

        private void SetCameraViewportIfChanged(Rect viewport)
        {
            if (targetCamera.rect == viewport)
                return;

            targetCamera.rect = viewport;
        }

        private void NormalizeSettings()
        {
            safeAreaWidth = Mathf.Clamp(
                safeAreaWidth,
                MinimumNormalizedSize,
                MaximumNormalizedSize);
            safeAreaHeight = Mathf.Clamp(
                safeAreaHeight,
                MinimumNormalizedSize,
                MaximumNormalizedSize);
            safeAreaCenterX = Mathf.Clamp01(safeAreaCenterX);
            safeAreaCenterY = Mathf.Clamp01(safeAreaCenterY);
            bottomReservedPixels = Mathf.Max(0f, bottomReservedPixels);
            ultrawideReservedSideFraction = Mathf.Clamp(
                ultrawideReservedSideFraction,
                0f,
                0.5f);
            referenceOrthographicSize = Mathf.Max(
                MinimumFollowDistance,
                referenceOrthographicSize);
            visibleWidthFraction = Mathf.Clamp(visibleWidthFraction, 0.1f, 1f);
            borderThickness = Mathf.Max(0.5f, borderThickness);
        }

        private void UpdateReservedAreas(ViewportLayout layout)
        {
            ResolveBattleCanvas(force: false);
            if (battleCanvas == null ||
                battleCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                DestroyReservedAreaRoot();
                return;
            }

            if (reservedAreaRoot != null &&
                reservedAreaRoot.parent != battleCanvas.transform)
            {
                DestroyReservedAreaRoot();
            }

            EnsureReservedAreas();
            if (reservedAreaRoot == null ||
                bottomReservedArea == null ||
                leftReservedArea == null ||
                rightReservedArea == null)
            {
                return;
            }

            bool standard = layout == ViewportLayout.Standard;
            float bottomReservedFraction = ResolveBottomReservedFraction();
            bool bottomEnabled = standard && bottomReservedFraction > 0f;
            bool sidesEnabled = !standard && ultrawideReservedSideFraction > 0f;
            bool layoutUnchanged =
                hasAppliedReservedAreaLayout &&
                appliedReservedAreaLayout == layout &&
                Mathf.Approximately(
                    appliedBottomReservedFraction,
                    bottomReservedFraction) &&
                Mathf.Approximately(
                    appliedUltrawideReservedSideFraction,
                    ultrawideReservedSideFraction) &&
                reservedAreaRoot.gameObject.activeSelf &&
                reservedAreaRoot.GetSiblingIndex() == 0 &&
                bottomReservedArea.gameObject.activeSelf == bottomEnabled &&
                leftReservedArea.gameObject.activeSelf == sidesEnabled &&
                rightReservedArea.gameObject.activeSelf == sidesEnabled;
            if (layoutUnchanged)
                return;

            reservedAreaRoot.SetAsFirstSibling();
            reservedAreaRoot.gameObject.SetActive(true);

            ConfigureReservedRect(
                bottomReservedArea.rectTransform,
                Vector2.zero,
                new Vector2(1f, bottomReservedFraction));
            ConfigureReservedRect(
                leftReservedArea.rectTransform,
                Vector2.zero,
                new Vector2(ultrawideReservedSideFraction, 1f));
            ConfigureReservedRect(
                rightReservedArea.rectTransform,
                new Vector2(1f - ultrawideReservedSideFraction, 0f),
                Vector2.one);

            bottomReservedArea.gameObject.SetActive(
                bottomEnabled);
            leftReservedArea.gameObject.SetActive(
                sidesEnabled);
            rightReservedArea.gameObject.SetActive(
                sidesEnabled);

            hasAppliedReservedAreaLayout = true;
            appliedReservedAreaLayout = layout;
            appliedBottomReservedFraction = bottomReservedFraction;
            appliedUltrawideReservedSideFraction = ultrawideReservedSideFraction;
        }

        private float ResolveBottomReservedFraction()
        {
            float screenHeight = Screen.height > 0
                ? Screen.height
                : ReferenceScreenHeight;
            return Mathf.Clamp(
                bottomReservedPixels / screenHeight,
                0f,
                0.95f);
        }

        private void ResolveBattleCanvas(bool force, bool createIfMissing = true)
        {
            if (!force &&
                battleCanvas != null &&
                battleCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return;
            }

            if (!force && battleCanvasResolutionAttempted)
                return;

            battleCanvasResolutionAttempted = true;
            battleCanvas = null;
            GameObject canvasObject = GameObject.Find(ReservedCanvasName);
            if (canvasObject == null)
            {
                if (!createIfMissing)
                    return;

                canvasObject = new GameObject(
                    ReservedCanvasName,
                    typeof(RectTransform),
                    typeof(Canvas));
                canvasObject.layer = 5;
                canvasObject.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            battleCanvas = canvasObject.GetComponent<Canvas>();
            if (battleCanvas == null)
                battleCanvas = canvasObject.AddComponent<Canvas>();

            battleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            battleCanvas.overrideSorting = true;
            battleCanvas.sortingOrder = -100;
            battleCanvas.pixelPerfect = false;
            ConfigureReservedRoot((RectTransform)canvasObject.transform);
            RemoveLegacyReservedAreaRoots();
        }

        private void RemoveLegacyReservedAreaRoots()
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(includeInactive: true);
            for (int index = 0; index < canvases.Length; index++)
            {
                Canvas canvas = canvases[index];
                if (canvas == battleCanvas)
                    continue;

                Transform legacyRoot = canvas.transform.Find(ReservedAreaRootName);
                if (legacyRoot == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(legacyRoot.gameObject);
                else
                    DestroyImmediate(legacyRoot.gameObject);
            }
        }

        private void EnsureReservedAreas()
        {
            if (reservedAreaRoot == null)
            {
                Transform existingRoot = battleCanvas.transform.Find(ReservedAreaRootName);
                reservedAreaRoot = existingRoot as RectTransform;
                if (reservedAreaRoot == null && existingRoot == null)
                {
                    var root = new GameObject(
                        ReservedAreaRootName,
                        typeof(RectTransform));
                    root.layer = battleCanvas.gameObject.layer;
                    root.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    reservedAreaRoot = (RectTransform)root.transform;
                    reservedAreaRoot.SetParent(battleCanvas.transform, false);
                }
            }

            if (reservedAreaRoot == null)
                return;

            ConfigureReservedRoot(reservedAreaRoot);
            bottomReservedArea = EnsureReservedImage(
                BottomReservedAreaName,
                bottomReservedArea);
            leftReservedArea = EnsureReservedImage(
                LeftReservedAreaName,
                leftReservedArea);
            rightReservedArea = EnsureReservedImage(
                RightReservedAreaName,
                rightReservedArea);
        }

        private Image EnsureReservedImage(string objectName, Image cachedImage)
        {
            if (cachedImage != null)
            {
                ConfigureReservedImage(cachedImage);
                return cachedImage;
            }

            Transform existing = reservedAreaRoot.Find(objectName);
            Image image = existing != null ? existing.GetComponent<Image>() : null;
            if (image == null && existing == null)
            {
                var mask = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                mask.layer = battleCanvas.gameObject.layer;
                mask.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                mask.transform.SetParent(reservedAreaRoot, false);
                image = mask.GetComponent<Image>();
            }
            else if (image == null)
            {
                image = existing.gameObject.AddComponent<Image>();
            }

            if (image != null)
                ConfigureReservedImage(image);

            return image;
        }

        private static void ConfigureReservedImage(Image image)
        {
            Sprite sprite = GetReservedAreaSprite();
            if (image.sprite != sprite)
                image.sprite = sprite;
            if (image.type != Image.Type.Simple)
                image.type = Image.Type.Simple;
            if (image.preserveAspect)
                image.preserveAspect = false;
            if (image.color != Color.black)
                image.color = Color.black;
            if (image.raycastTarget)
                image.raycastTarget = false;
        }

        private static Sprite GetReservedAreaSprite()
        {
            if (reservedAreaSprite == null)
            {
                reservedAreaSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                reservedAreaSprite.name = "__BattleLayoutReservedAreaSprite";
                reservedAreaSprite.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            return reservedAreaSprite;
        }

        private static void ConfigureReservedRoot(RectTransform root)
        {
            if (root.anchorMin != Vector2.zero)
                root.anchorMin = Vector2.zero;
            if (root.anchorMax != Vector2.one)
                root.anchorMax = Vector2.one;
            if (root.anchoredPosition != Vector2.zero)
                root.anchoredPosition = Vector2.zero;
            if (root.sizeDelta != Vector2.zero)
                root.sizeDelta = Vector2.zero;

            Vector2 centeredPivot = new Vector2(0.5f, 0.5f);
            if (root.pivot != centeredPivot)
                root.pivot = centeredPivot;
            if (root.localScale != Vector3.one)
                root.localScale = Vector3.one;
        }

        private void SetReservedAreasEnabled(bool enabled)
        {
            if (!enabled)
                hasAppliedReservedAreaLayout = false;

            if (reservedAreaRoot != null)
            {
                reservedAreaRoot.gameObject.SetActive(enabled);
                return;
            }

            if (battleCanvas == null)
                return;

            Transform existingRoot = battleCanvas.transform.Find(ReservedAreaRootName);
            if (existingRoot != null)
                existingRoot.gameObject.SetActive(enabled);
        }

        private void DestroyReservedAreaRoot()
        {
            bool ownsReservedCanvas =
                battleCanvas != null &&
                battleCanvas.gameObject.name == ReservedCanvasName;

            if (reservedAreaRoot != null)
            {
                GameObject rootObject = reservedAreaRoot.gameObject;
                if (Application.isPlaying)
                    Destroy(rootObject);
                else
                    DestroyImmediate(rootObject);
            }

            if (ownsReservedCanvas)
            {
                GameObject canvasObject = battleCanvas.gameObject;
                if (Application.isPlaying)
                    Destroy(canvasObject);
                else
                    DestroyImmediate(canvasObject);
            }

            reservedAreaRoot = null;
            bottomReservedArea = null;
            leftReservedArea = null;
            rightReservedArea = null;
            hasAppliedReservedAreaLayout = false;
            battleCanvas = null;
            battleCanvasResolutionAttempted = false;
        }

        private static void ConfigureReservedRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private void UpdateTargetWorldPositions()
        {
            targetWorldScratch.Clear();

            AddManualTargets();
            if (followActiveCharacters)
                AddActiveCharacterTargets();
        }

        private void AddManualTargets()
        {
            if (manualTargets == null)
                return;

            for (int index = 0; index < manualTargets.Length; index++)
            {
                Transform target = manualTargets[index];
                if (target == null)
                    continue;

                targetWorldScratch.Add(target.position);
            }
        }

        private void AddActiveCharacterTargets()
        {
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            if (world == null)
                return;

            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            if (ReferenceEquals(plan.World, world) &&
                plan.Submission != null &&
                plan.Submission.TryAcquire(
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease))
            {
                try
                {
                    AddActiveCharacterTargets(lease.Submission.CapturedFrame);
                }
                finally
                {
                    lease.Dispose();
                }

                return;
            }

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.DedicatedSimulationWorkerActiveForDiagnostics)
                return;

            AddActiveCharacterTargets(world.BattlePresentation.PublishedFrame);
        }

        private void AddActiveCharacterTargets(BattlePresentationFrame frame)
        {
            if (frame == null)
                return;

            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(index);
                if (entity.CurrentDatObjType != (int)LF2ObjectType.Character ||
                    !entity.EntityVisible ||
                    entity.State < 0 ||
                    entity.LinkState < 0)
                {
                    continue;
                }

                Vector3 worldPosition = NTSDRenderSpace.ScreenPixelToPresentationWorld(
                    entity.XInt + (int)entity.RenderOffsetX - entity.CameraX,
                    entity.ZInt);
                targetWorldScratch.Add(worldPosition);
            }
        }

        private void RefreshCameraBounds()
        {
            hasCameraBounds = TryResolveCameraBounds(out cameraBounds);
        }

        private void RefreshBackgroundBounds()
        {
            hasBackgroundBounds = TryResolveBackgroundBounds(out backgroundBounds);
        }

        private void UpdateCameraView()
        {
            if (targetCamera == null)
                return;

            UpdateSafeAreaRect();
        }

        private void UpdateSafeAreaRect()
        {
            Rect cameraRect = targetCamera.pixelRect;
            if (cameraRect.width <= 0f || cameraRect.height <= 0f)
                cameraRect = new Rect(0f, 0f, Screen.width, Screen.height);

            Rect gameplayRect = cameraRect;
            float normalizedSafeX = Mathf.Clamp(
                safeAreaCenterX - safeAreaWidth * 0.5f,
                0f,
                1f - safeAreaWidth);
            float normalizedSafeY = Mathf.Clamp(
                safeAreaCenterY - safeAreaHeight * 0.5f,
                0f,
                1f - safeAreaHeight);
            safeAreaScreenRect = new Rect(
                gameplayRect.x + gameplayRect.width *
                    normalizedSafeX,
                gameplayRect.y + gameplayRect.height *
                    normalizedSafeY,
                gameplayRect.width * safeAreaWidth,
                gameplayRect.height * safeAreaHeight);

            Vector3 bottomLeft = targetCamera.ScreenToWorldPoint(
                new Vector3(safeAreaScreenRect.xMin, safeAreaScreenRect.yMin, -targetCamera.transform.position.z));
            Vector3 topRight = targetCamera.ScreenToWorldPoint(
                new Vector3(safeAreaScreenRect.xMax, safeAreaScreenRect.yMax, -targetCamera.transform.position.z));
            safeAreaWorldRect = Rect.MinMaxRect(
                bottomLeft.x,
                bottomLeft.y,
                topRight.x,
                topRight.y);
            hasSafeAreaWorldRect = safeAreaWorldRect.width > 0f && safeAreaWorldRect.height > 0f;
        }

        private void UpdateOrthographicSize()
        {
            if (targetCamera == null)
                return;

            float viewportAspect = ResolveViewportAspect();
            if (hasBackgroundBounds)
            {
                float containSize = Mathf.Max(
                    backgroundBounds.extents.y,
                    backgroundBounds.extents.x / viewportAspect);
                SetOrthographicSize(containSize);
                AlignCameraToBackground(backgroundBounds);
                return;
            }

            float referenceSize = ResolveBoundedOrthographicSize(ReferenceViewportAspect);
            float orthographicSize = referenceSize *
                Mathf.Max(1f, ReferenceViewportAspect / viewportAspect);
            SetOrthographicSize(orthographicSize);
        }

        private float ResolveBoundedOrthographicSize(float aspect)
        {
            float orthographicSize = referenceOrthographicSize;
            if (!hasCameraBounds)
                return orthographicSize;

            float maximumBoundedSize =
                cameraBounds.width * visibleWidthFraction / (aspect * 2f);
            return Mathf.Min(referenceOrthographicSize, maximumBoundedSize);
        }

        private float ResolveViewportAspect()
        {
            if (targetCamera.pixelHeight > 0)
            {
                float viewportAspect =
                    targetCamera.pixelWidth / (float)targetCamera.pixelHeight;
                if (viewportAspect > MinimumFollowDistance)
                    return viewportAspect;
            }

            if (targetCamera.aspect > MinimumFollowDistance)
                return targetCamera.aspect;

            return ReferenceViewportAspect;
        }

        private bool TryResolveBackgroundBounds(out Bounds bounds)
        {
            if (backgroundRenderer != null && backgroundRenderer.sprite != null)
            {
                bounds = backgroundRenderer.bounds;
                return bounds.size.x > 0f && bounds.size.y > 0f;
            }

            bounds = default;
            return false;
        }

        private void SetOrthographicSize(float orthographicSize)
        {
            if (!Mathf.Approximately(targetCamera.orthographicSize, orthographicSize))
                targetCamera.orthographicSize = orthographicSize;
        }

        private void AlignCameraToBackground(Bounds backgroundBounds)
        {
            Vector3 position = targetCamera.transform.position;
            position.x = backgroundBounds.center.x;
            position.y = backgroundBounds.center.y;
            if (targetCamera.transform.position != position)
                targetCamera.transform.position = position;
        }

        private void ApplyCameraFollow()
        {
            if (targetWorldScratch.Count == 0 || !hasSafeAreaWorldRect)
                return;

            Vector3 desiredPosition = targetCamera.transform.position;
            ResolveFollowDeltas(out float horizontalDelta, out float verticalDelta);
            desiredPosition.x += horizontalDelta;
            desiredPosition.y += verticalDelta;

            if (hasCameraBounds)
                desiredPosition = ClampCameraPosition(desiredPosition);

            float smoothTime = Mathf.Max(MinimumFollowDistance, followSmoothTime);
            if (followSmoothTime <= MinimumFollowDistance)
            {
                targetCamera.transform.position = desiredPosition;
                followVelocity = Vector3.zero;
                return;
            }

            Vector3 currentPosition = targetCamera.transform.position;
            targetCamera.transform.position = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref followVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        private void ResolveFollowDeltas(out float horizontalDelta, out float verticalDelta)
        {
            horizontalDelta = 0f;
            verticalDelta = 0f;
            if (!followHorizontal && !followVertical)
                return;

            float minX = targetWorldScratch[0].x;
            float maxX = minX;
            float minY = targetWorldScratch[0].y;
            float maxY = minY;
            for (int index = 1; index < targetWorldScratch.Count; index++)
            {
                Vector2 position = targetWorldScratch[index];
                if (followHorizontal)
                {
                    minX = Mathf.Min(minX, position.x);
                    maxX = Mathf.Max(maxX, position.x);
                }

                if (followVertical)
                {
                    minY = Mathf.Min(minY, position.y);
                    maxY = Mathf.Max(maxY, position.y);
                }
            }

            if (followHorizontal)
            {
                if (minX < safeAreaWorldRect.xMin)
                    horizontalDelta = minX - safeAreaWorldRect.xMin;
                else if (maxX > safeAreaWorldRect.xMax)
                    horizontalDelta = maxX - safeAreaWorldRect.xMax;
            }

            if (followVertical)
            {
                if (minY < safeAreaWorldRect.yMin)
                    verticalDelta = minY - safeAreaWorldRect.yMin;
                else if (maxY > safeAreaWorldRect.yMax)
                    verticalDelta = maxY - safeAreaWorldRect.yMax;
            }
        }

        private bool TryResolveCameraBounds(out Rect resolvedBounds)
        {
            if (constrainToStageBounds &&
                NTSDRenderSpace.TryGetStageWorldBounds(out resolvedBounds))
            {
                return true;
            }

            resolvedBounds = default;
            return false;
        }

        private void ClampCameraToBounds()
        {
            if (targetCamera == null)
                return;

            if (hasBackgroundBounds)
            {
                AlignCameraToBackground(backgroundBounds);
                return;
            }

            if (hasCameraBounds)
                targetCamera.transform.position = ClampCameraPosition(targetCamera.transform.position);
        }

        private Vector3 ClampCameraPosition(Vector3 desiredPosition)
        {
            float halfWidth = targetCamera.orthographicSize * ResolveViewportAspect();
            float halfHeight = targetCamera.orthographicSize;
            float minX = cameraBounds.xMin + halfWidth;
            float maxX = cameraBounds.xMax - halfWidth;
            float minY = cameraBounds.yMin + halfHeight;
            float maxY = cameraBounds.yMax - halfHeight;

            if (minX <= maxX)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            }
            else
                desiredPosition.x = (cameraBounds.xMin + cameraBounds.xMax) * 0.5f;

            if (minY <= maxY)
            {
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }
            else
                desiredPosition.y = (cameraBounds.yMin + cameraBounds.yMax) * 0.5f;

            return desiredPosition;
        }

        private void OnDrawGizmosSelected()
        {
            if (hasSafeAreaWorldRect)
            {
                Vector3 safeAreaCenter = new Vector3(
                    safeAreaWorldRect.center.x,
                    safeAreaWorldRect.center.y,
                    0f);
                Vector3 safeAreaSize = new Vector3(
                    safeAreaWorldRect.width,
                    safeAreaWorldRect.height,
                    0.01f);
                Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.12f);
                Gizmos.DrawCube(safeAreaCenter, safeAreaSize);
                Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.9f);
                Gizmos.DrawWireCube(safeAreaCenter, safeAreaSize);
            }

        }

        private void DrawSafeAreaOverlay()
        {
            Rect screenRect = safeAreaScreenRect;
            Rect cameraRect = targetCamera.pixelRect;

            DrawScreenRect(
                new Rect(cameraRect.xMin, cameraRect.yMin, cameraRect.width, screenRect.yMin - cameraRect.yMin),
                unsafeAreaColor);
            DrawScreenRect(
                new Rect(cameraRect.xMin, screenRect.yMax, cameraRect.width, cameraRect.yMax - screenRect.yMax),
                unsafeAreaColor);
            DrawScreenRect(
                new Rect(cameraRect.xMin, screenRect.yMin, screenRect.xMin - cameraRect.xMin, screenRect.height),
                unsafeAreaColor);
            DrawScreenRect(
                new Rect(screenRect.xMax, screenRect.yMin, cameraRect.xMax - screenRect.xMax, screenRect.height),
                unsafeAreaColor);

            DrawScreenBorder(screenRect, safeAreaColor);
        }

        private void DrawScreenBorder(Rect rect, Color color)
        {
            DrawScreenLine(
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                color,
                borderThickness);
            DrawScreenLine(
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                color,
                borderThickness);
            DrawScreenLine(
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMin, rect.yMax),
                color,
                borderThickness);
            DrawScreenLine(
                new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMin, rect.yMin),
                color,
                borderThickness);
        }

        private void DrawScreenRect(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            Rect normalizedRect = new Rect(
                rect.x / Screen.width,
                rect.y / Screen.height,
                rect.width / Screen.width,
                rect.height / Screen.height);
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(
                new Rect(
                    normalizedRect.x * Screen.width,
                    (1f - normalizedRect.y - normalizedRect.height) * Screen.height,
                    normalizedRect.width * Screen.width,
                    normalizedRect.height * Screen.height),
                Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawScreenLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 guiStart = new Vector2(start.x, Screen.height - start.y);
            Vector2 guiEnd = new Vector2(end.x, Screen.height - end.y);
            Vector2 delta = guiEnd - guiStart;
            float length = delta.magnitude;
            if (length <= 0f)
                return;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, guiStart);
            GUI.DrawTexture(
                new Rect(guiStart.x, guiStart.y, length, thickness),
                Texture2D.whiteTexture);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

    }
}
