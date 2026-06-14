using UnityEngine;
using NTSD.Animation;

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
        }

        private void SetPresentationEnabled(bool enabled)
        {
            if (worldCamera != null) worldCamera.enabled = enabled;
            if (uiCamera != null) uiCamera.enabled = enabled;
            if (battleCanvas != null) battleCanvas.enabled = enabled;
        }

        private void EnsureBattleCanvasCamera()
        {
            if (battleCanvas == null || uiCamera == null) return;

            battleCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            battleCanvas.worldCamera = uiCamera;
        }
    }
}
