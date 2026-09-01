using UnityEngine;
using NTSD.Animation;
using NTSD.LevelEditor;

namespace NTSD.App
{
    /// <summary>
    /// Battle 场景表现层配置
    /// 
    /// 职责：
    /// - 启用/禁用 Battle Camera、UI Camera、Canvas
    /// - 配置 Canvas 的 RenderCamera
    /// 
    /// 注意：不再自动启动，由 AppManager 主动调用
    /// </summary>
    public sealed class BattleBootstrap : MonoBehaviour
    {
        [Header("Battle Cameras")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Camera uiCamera;

        [Header("Battle UI")]
        [SerializeField] private Canvas battleCanvas;

        [Header("Battle Map Configuration")]
        [Tooltip("Optional MapId catalog. Leave both Catalog and Map Id empty to keep the legacy Scene BoundaryWall fallback.")]
        [SerializeField] private BattleMapCatalog mapCatalog;
        [SerializeField] private string mapId = "";
        [SerializeField] private BoundaryWallManager boundaryManager;
        [SerializeField] private SpriteRenderer backgroundRenderer;

        private bool mapConfigurationPrepared;
        private string preparedMapId = string.Empty;
        private Sprite previousBackgroundSprite;
        private BoundaryWallManager preparedBoundaryManager;
        private BattleMapBoundaryDefinition preparedBoundaryDefinition;

        public bool IsMapConfigurationPrepared => mapConfigurationPrepared;
        public string PreparedMapId => preparedMapId;
        public bool IsRuntimeMapCleared =>
            !mapConfigurationPrepared && preparedBoundaryManager == null;

        public bool TryPrepareMapConfiguration(out string failure)
        {
            bool hasCatalog = mapCatalog != null;
            bool hasMapId = !string.IsNullOrWhiteSpace(mapId);
            if (!hasCatalog && !hasMapId)
            {
                failure = string.Empty;
                return true;
            }

            if (!hasCatalog || !hasMapId)
            {
                failure = "Map Catalog and Map Id must be configured together.";
                return false;
            }

            if (mapConfigurationPrepared)
            {
                if (string.Equals(preparedMapId, mapId, System.StringComparison.Ordinal))
                {
                    failure = string.Empty;
                    return true;
                }

                failure = "A different map is already prepared for this battle bootstrap.";
                return false;
            }

            if (!mapCatalog.TryResolve(mapId, out BattleMapCatalog.Entry entry, out failure))
                return false;

            if (boundaryManager == null)
            {
                failure = "Assign a BoundaryWallManager before preparing a configured map.";
                return false;
            }

            if (backgroundRenderer == null)
            {
                failure = "Assign the world background SpriteRenderer before preparing a configured map.";
                return false;
            }

            Sprite mapBackgroundSprite = entry.BoundaryDefinition.BackgroundSprite;
            if (mapBackgroundSprite == null)
            {
                failure = "The resolved map boundary has no background sprite.";
                return false;
            }

            if (!boundaryManager.TryLoadBoundaryDefinition(entry.BoundaryDefinition, out failure))
                return false;

            previousBackgroundSprite = backgroundRenderer.sprite;
            backgroundRenderer.sprite = mapBackgroundSprite;
            preparedBoundaryManager = boundaryManager;
            preparedBoundaryDefinition = entry.BoundaryDefinition;
            preparedMapId = mapId;
            mapConfigurationPrepared = true;
            failure = string.Empty;
            return true;
        }

        public void ClearPreparedMapConfiguration()
        {
            if (!mapConfigurationPrepared)
                return;

            if (preparedBoundaryManager != null &&
                preparedBoundaryManager.LoadedBoundaryDefinition == preparedBoundaryDefinition)
            {
                preparedBoundaryManager.ClearLoadedBoundaryDefinition();
            }

            if (backgroundRenderer != null)
                backgroundRenderer.sprite = previousBackgroundSprite;

            mapConfigurationPrepared = false;
            preparedMapId = string.Empty;
            previousBackgroundSprite = null;
            preparedBoundaryManager = null;
            preparedBoundaryDefinition = null;
        }

        public void EnablePresentation()
        {
            SetPresentationEnabled(true);
            NTSDRenderSpace.BindWorldCamera(worldCamera);
            EnsureBattleCanvasCamera();
        }

        public void DisablePresentation()
        {
            NTSDRenderSpace.ClearBoundWorldCamera(worldCamera);
            SetPresentationEnabled(false);
            ClearPreparedMapConfiguration();
        }

        private void OnDisable()
        {
            DisablePresentation();
        }

        private void SetPresentationEnabled(bool enabled)
        {
            if (worldCamera != null) worldCamera.enabled = enabled;
            if (uiCamera != null)
            {
                bool usesOverlayCanvas = battleCanvas != null &&
                    battleCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
                uiCamera.enabled = enabled && !usesOverlayCanvas;
            }
            if (battleCanvas != null) battleCanvas.enabled = enabled;
        }

        private void EnsureBattleCanvasCamera()
        {
            if (battleCanvas == null)
                return;
            if (battleCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                battleCanvas.worldCamera = null;
                return;
            }
            if (uiCamera == null)
                return;

            battleCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            battleCanvas.worldCamera = uiCamera;
        }
    }
}
