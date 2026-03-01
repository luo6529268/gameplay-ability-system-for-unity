using System;
using UnityEngine;

namespace Kako.CameraFit
{
    [AddComponentMenu("KAKO/Camera Fitter")]
    [DisallowMultipleComponent]
    public class CameraFitter : MonoBehaviour
    {
        public Camera Camera { set => fitCamera = value; get => fitCamera; }
        public Vector2 Padding { set => padding = value; get => padding; }
        public CameraFitOrientation Orientation { set => orientation = value; get => orientation; }
        public bool HasFocus { set => enableFocus = value; get => enableFocus; }
        public FocusType FocusType { set => focusType = value; get => focusType; }
        public Vector3 FocusPosition { set => focusPosition = value; get => focusPosition; }
        public Transform FocusTransform { set => focusObject = value; get => focusObject; }
        public bool IsRectFit { set => enableRectFit = value; get => enableRectFit; }
        public Canvas FitAreaCanvas { set => fitAreaCanvas = value; get => fitAreaCanvas; }
        public RectTransform FitArea { set => fitArea = value; get => fitArea; }
        public Vector3[] FitPositions { set => fitPositions = value; get => fitPositions; }
        public Transform[] FitPivots { set => fitPivots = value; get => fitPivots; }
        public MeshFilter[] FitMeshes { set => fitMeshes = value; get => fitMeshes; }
        public bool IsDebugEnabled { set => enableDebug = value; get => enableDebug; }
        public bool DebugDrawFitFrustum { set => drawFitFrustum = value; get => drawFitFrustum; }
        public Color DebugFitFrustumColor { set => fitFrustumColor = value; get => fitFrustumColor; }
        public bool DebugFitFrustumCenterLine { set => drawFitFrustumCenterLine = value; get => drawFitFrustumCenterLine; }
        public Color DebugCenterLineColor { set => centerLineColor = value; get => centerLineColor; }
        public bool DebugDrawClipPlane { set => drawClipPlane = value; get => drawClipPlane; }
        public Color DebugClipPlaneColor { set => clipPlaneColor = value; get => clipPlaneColor; }
        public float DebugClipPlanePercent { set => clipPlanePercent = value; get => clipPlanePercent; }

        [Tooltip("Represents the camera to which you want to apply the fitting operation.")] 
        [SerializeField] private Camera fitCamera;
        
        [Tooltip("It determines the additional empty space, in world units, to leave on the sides of the framed area.")]
        [SerializeField] private Vector2 padding;

        [Tooltip("If automatic, it makes sure the fit points stay in the framed area. Otherwise, it performs the fit operation only in the selected orientation.")]
        [SerializeField] private CameraFitOrientation orientation = CameraFitOrientation.Automatic;

        [Tooltip("If enabled, centers the camera to specific point (in world space). If disabled, centers the camera to the center of pointList.")]
        [SerializeField] private bool enableFocus;
        [Tooltip("Determines the type of focus.")]
        [SerializeField] private FocusType focusType;
        [Tooltip("The position in world space to which the camera will be focused.")]
        [SerializeField] private Vector3 focusPosition;
        [Tooltip("The object position to which the camera will be focused.")]
        [SerializeField] private Transform focusObject;

        [Tooltip("If enabled, The camera fitting operation ensures that the specified pointList are visible within this RectTransform.")]
        [SerializeField] private bool enableRectFit;
        [Tooltip("The canvas that contains the fit area.")]
        [SerializeField] private Canvas fitAreaCanvas;
        [Tooltip("The RectTransform defining the area in which the camera should fit.")]
        [SerializeField] private RectTransform fitArea;

        [Tooltip("The positions to fit within the camera view.")]
        [SerializeField] private Vector3[] fitPositions;
        [Tooltip("The object pivots to fit within the camera view.")]
        [SerializeField] private Transform[] fitPivots;
        [Tooltip("The meshes to fit within the camera view.")]
        [SerializeField] private MeshFilter[] fitMeshes;

        [Tooltip("If enabled, displays debug information on gizmos.")]
        [SerializeField] private bool enableDebug;
        [SerializeField] private bool drawFitFrustum = true;
        [SerializeField] private Color fitFrustumColor = Color.green;
        [SerializeField] private bool drawFitFrustumCenterLine = true;
        [SerializeField] private Color centerLineColor = Color.yellow;
        [SerializeField] private bool drawClipPlane;
        [SerializeField] private Color clipPlaneColor = Color.yellow;
        [Range(0f, 1f)] [SerializeField] private float clipPlanePercent = 1f;

        public virtual CameraFitData Fit()
        {
            Vector3[] positions = GetFitPositions();
            if (positions.Length == 0) throw new Exception("Camera Fit Failed. There is no fit point.");

            if (enableRectFit)
            {
                // RECT FIT
                if (orientation == CameraFitOrientation.Horizontal) return fitCamera.HorizontalFit(positions, padding.x, fitArea, fitAreaCanvas, GetFocus());
                if (orientation == CameraFitOrientation.Vertical) return fitCamera.VerticalFit(positions, padding.y, fitArea, fitAreaCanvas, GetFocus());
                return fitCamera.Fit(positions, padding.x, padding.y, fitArea, fitAreaCanvas, GetFocus());
            }
            else
            {
                // SCREEN FIT
                if (orientation == CameraFitOrientation.Horizontal) return fitCamera.HorizontalFit(positions, padding.x, GetFocus());
                if (orientation == CameraFitOrientation.Vertical) return fitCamera.VerticalFit(positions, padding.y, GetFocus());
                return fitCamera.Fit(positions, padding.x, padding.y, GetFocus());
            }
        }

        protected virtual Vector3? GetFocus()
        {
            if (!enableFocus) return null;
            return focusType == FocusType.Position ? focusPosition : focusObject.transform.position;
        }
        protected virtual Vector3[] GetFitPositions() => GetMergedPositionArrays(fitPositions, GetAllPivotPositions(), GetAllVerticesFromMeshFilters());
        private Vector3[] GetMergedPositionArrays(params Vector3[][] arrays)
        {
            // Calculate the total length of the merged array
            int totalLength = 0;
            foreach (Vector3[] array in arrays)
            {
                totalLength += array.Length;
            }

            // Create the merged array
            Vector3[] mergedArray = new Vector3[totalLength];

            // Copy elements from each array to the merged array
            int currentIndex = 0;
            foreach (Vector3[] array in arrays)
            {
                Array.Copy(array, 0, mergedArray, currentIndex, array.Length);
                currentIndex += array.Length;
            }

            return mergedArray;
        }

        protected Vector3[] GetAllPivotPositions()
        {
            Vector3[] positions = new Vector3[fitPivots.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = fitPivots[i].position;
            }

            return positions;
        }

        protected Vector3[] GetAllVerticesFromMeshFilters()
        {
            // Calculate total vertices count
            int totalVerticesCount = 0;
            foreach (MeshFilter meshFilter in fitMeshes)
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                    totalVerticesCount += meshFilter.sharedMesh.vertexCount;
            }

            // Create the array to hold all vertices
            Vector3[] allVertices = new Vector3[totalVerticesCount];

            // Index to keep track of the next available position in allVertices array
            int currentIndex = 0;

            // Iterate through each MeshFilter
            foreach (MeshFilter meshFilter in fitMeshes)
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Get the vertices from the mesh
                    Vector3[] meshVertices = meshFilter.sharedMesh.vertices;

                    // Copy the vertices to the allVertices array
                    for (int i = 0; i < meshVertices.Length; i++)
                    {
                        allVertices[currentIndex] = meshFilter.transform.TransformPoint(meshVertices[i]);
                        currentIndex++;
                    }
                }
            }

            return allVertices;
        }

        private void Reset() => InitializeCamera();

        private void InitializeCamera()
        {
            if (fitCamera != null) return;

            // Try find camera
            fitCamera = GetComponent<Camera>();
            if (fitCamera == null)
                fitCamera = Camera.main;
            if (fitCamera == null)
                fitCamera = FindObjectOfType<Camera>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enableDebug || fitCamera == null) return;
            if (IsRectFit)
            {
                if (fitArea == null || fitAreaCanvas == null) return;

                if (fitCamera.orthographic)
                {
                    if(fitCamera.orthographicSize <= 0) return;
                    if(drawClipPlane)CameraFitDebugUtility.DrawRectOrthographicCameraClipPlane(fitCamera, fitArea, fitAreaCanvas, clipPlanePercent, clipPlaneColor);
                    if(drawFitFrustum) CameraFitDebugUtility.DrawRectOrthographicCameraFrustum(fitCamera, fitArea, fitAreaCanvas, fitFrustumColor);
                    if(drawFitFrustumCenterLine) CameraFitDebugUtility.DrawRectOrthographicCenterLine(fitCamera, fitArea, fitAreaCanvas, centerLineColor);
                }
                else
                {
                    if(fitCamera.fieldOfView <= 0 || fitCamera.fieldOfView >= 180) return;
                    if(drawClipPlane) CameraFitDebugUtility.DrawRectPerspectiveCameraClipPlane(fitCamera, fitArea, fitAreaCanvas, clipPlanePercent, clipPlaneColor);
                    if(drawFitFrustum) CameraFitDebugUtility.DrawRectPerspectiveCameraFrustum(fitCamera, fitArea, fitAreaCanvas, fitFrustumColor);
                    if(drawFitFrustumCenterLine) CameraFitDebugUtility.DrawRectPerspectiveCenterLine(fitCamera, fitArea, fitAreaCanvas, centerLineColor);
                }
            }
            else
            {
                if(drawClipPlane)CameraFitDebugUtility.DrawCameraClipPlane(fitCamera, clipPlanePercent, clipPlaneColor);
                if(drawFitFrustum) CameraFitDebugUtility.DrawCameraFrustum(fitCamera, fitFrustumColor);
                if(drawFitFrustumCenterLine) CameraFitDebugUtility.DrawCenterLine(fitCamera, centerLineColor);
            }
        }
#endif
    }

    public enum FocusType
    {
        Position,
        ObjectPivot
    }
}