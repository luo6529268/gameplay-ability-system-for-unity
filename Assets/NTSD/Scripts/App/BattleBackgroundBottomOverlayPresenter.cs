using UnityEngine;

namespace NTSD.App
{
    /// <summary>
    /// Supplies a target-camera-only final black overlay. It never redraws, hides, or
    /// transforms the world background; Scene View remains a direct view of Bg (2).
    /// </summary>
    internal sealed class BattleBackgroundBottomOverlayPresenter
    {
        private static readonly int BottomGapId = Shader.PropertyToID("_BottomGap");

        private static BattleBackgroundBottomOverlayPresenter activePresenter;

        private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private Mesh overlayMesh;
        private Material overlayMaterial;
        private Camera targetCamera;
        private bool isReady;

        public void Refresh(Camera camera, float bottomGapNormalized)
        {
            float bottomGap = Mathf.Clamp01(bottomGapNormalized);
            if (camera == null || bottomGap <= 0f || !EnsureResources())
            {
                DisablePresentation();
                return;
            }

            targetCamera = camera;
            propertyBlock.Clear();
            propertyBlock.SetFloat(BottomGapId, bottomGap);
            isReady = true;
            activePresenter = this;
        }

        public void Dispose()
        {
            isReady = false;
            if (activePresenter == this)
                activePresenter = null;

            DestroyObject(overlayMesh);
            DestroyObject(overlayMaterial);
            overlayMesh = null;
            overlayMaterial = null;
            targetCamera = null;
        }

        internal static bool TryGetDraw(
            Camera camera,
            out Mesh mesh,
            out Material material,
            out MaterialPropertyBlock properties)
        {
            BattleBackgroundBottomOverlayPresenter presenter = activePresenter;
            if (presenter == null ||
                !presenter.isReady ||
                presenter.targetCamera != camera ||
                presenter.overlayMesh == null ||
                presenter.overlayMaterial == null)
            {
                mesh = null;
                material = null;
                properties = null;
                return false;
            }

            mesh = presenter.overlayMesh;
            material = presenter.overlayMaterial;
            properties = presenter.propertyBlock;
            return true;
        }

        private bool EnsureResources()
        {
            if (overlayMaterial == null)
            {
                Shader shader = Resources.Load<Shader>(
                    BattleBackgroundPlatformPresentation.BottomOverlayShaderResourcePath);
                if (shader == null)
                    return false;

                overlayMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = "NTSD Battle Bottom Overlay (Runtime)",
                };
            }

            if (overlayMesh == null)
                overlayMesh = CreateOverlayMesh();
            return overlayMesh != null;
        }

        private void DisablePresentation()
        {
            isReady = false;
            if (activePresenter == this)
                activePresenter = null;
        }

        private static Mesh CreateOverlayMesh()
        {
            var mesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "NTSD Battle Bottom Overlay Quad",
                vertices = new[]
                {
                    new Vector3(-1f, -1f, 0f),
                    new Vector3(1f, -1f, 0f),
                    new Vector3(-1f, 1f, 0f),
                    new Vector3(1f, 1f, 0f),
                },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                },
                triangles = new[] { 0, 2, 1, 2, 3, 1 },
                bounds = new Bounds(Vector3.zero, Vector3.one * 100000f),
            };
            mesh.UploadMeshData(true);
            return mesh;
        }

        private static void DestroyObject(Object value)
        {
            if (value == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(value);
            else
                Object.DestroyImmediate(value);
        }
    }
}
