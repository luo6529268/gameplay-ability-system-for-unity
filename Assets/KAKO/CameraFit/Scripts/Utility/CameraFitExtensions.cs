using UnityEngine;
using Kako.Utilities;

namespace Kako.CameraFit
{
    public static class CameraFitExtensions
    {
        #region ViewFit
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="paddingX">Horizontal padding applied. It determines the additional empty space, in world units, to leave on the left and right sides of the framed area.</param>
        /// <param name="paddingY">Vertical padding applied. It determines the additional empty space, in world units, to leave on the up and down sides of the framed area.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData Fit(this Camera cam, Vector3[] pointList, float paddingX, float paddingY, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraFitData(cam, pointList, paddingX, paddingY, CameraFitOrientation.Automatic, focus));
        }
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points horizontally.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="padding">Horizontal padding applied. It determines the additional empty space, in world units, to leave on the left and right sides of the framed area.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData HorizontalFit(this Camera cam, Vector3[] pointList, float padding, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraFitData(cam, pointList, padding, padding, CameraFitOrientation.Horizontal, focus));
        }
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points vertically.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="padding">Vertical padding applied. It determines the additional empty space, in world units, to leave on the up and down sides of the framed area.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData VerticalFit(this Camera cam, Vector3[] pointList, float padding, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraFitData(cam, pointList, padding, padding, CameraFitOrientation.Vertical, focus));
        }
        #endregion
        #region RectFit
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="paddingX">Horizontal padding applied. It determines the additional empty space, in world units, to leave on the left and right sides of the framed area.</param>
        /// <param name="paddingY">Vertical padding applied. It determines the additional empty space, in world units, to leave on the up and down sides of the framed area.</param>
        /// <param name="fitArea">The camera fitting operation ensures that the specified pointList are visible within this RectTransform.</param>
        /// <param name="canvas">The Canvas component to which the fitArea belongs.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData Fit(this Camera cam, Vector3[] pointList, float paddingX, float paddingY, RectTransform fitArea, Canvas canvas, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraRectFitData(cam, pointList, paddingX, paddingY, fitArea, canvas, CameraFitOrientation.Automatic, focus));
        }
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points horizontally.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="padding">Horizontal padding applied. It determines the additional empty space, in world units, to leave on the left and right sides of the framed area.</param>
        /// <param name="fitArea">The camera fitting operation ensures that the specified pointList are visible within this RectTransform.</param>
        /// <param name="canvas">The Canvas component to which the fitArea belongs.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData HorizontalFit(this Camera cam, Vector3[] pointList, float padding, RectTransform fitArea, Canvas canvas, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraRectFitData(cam, pointList, padding, padding, fitArea, canvas, CameraFitOrientation.Horizontal, focus));
        }
        /// <summary>Adjust the camera's position and size to frame a list of specified 3D world-space points vertically.</summary>
        /// <param name="cam">Represents the camera to which you want to apply the fitting operation.</param>
        /// <param name="pointList">List of 3D world-space points that you want the camera to fit. The camera will adjust its position and size to encompass these points.</param>
        /// <param name="padding">Vertical padding applied. It determines the additional empty space, in world units, to leave on the up and down sides of the framed area.</param>
        /// <param name="fitArea">The camera fitting operation ensures that the specified pointList are visible within this RectTransform.</param>
        /// <param name="canvas">The Canvas component to which the fitArea belongs.</param>
        /// <param name="focus">If PROVIDED, centers the camera to specific point (in world space). If NOT PROVIDED, centers the camera to center of pointList.</param>
        public static CameraFitData VerticalFit(this Camera cam, Vector3[] pointList, float padding, RectTransform fitArea, Canvas canvas, Vector3? focus = null)
        {
            return ApplyData(cam, CameraFit.GetCameraRectFitData(cam, pointList, padding, padding, fitArea, canvas, CameraFitOrientation.Vertical, focus));
        }
        #endregion

        private static CameraFitData ApplyData(Camera cam, CameraFitData data)
        {
            cam.transform.position = data.FitPosition;
            cam.SetZoom(data.FitZoom);
            return data;
        }
        
        #region LerpFit
        /// <summary>Adjust the camera's position and size smoothly to fit.</summary>
        /// <param name="fitData">Desired camera fit action that is offered by Kako Camera Fit</param>
        /// <param name="t">Value used to interpolate between initial fit and final fit.</param>
        public static void Lerp(this CameraFitData fitData, float t)
        {
            fitData.Camera.transform.position = Vector3.Lerp(fitData.StartPosition, fitData.FitPosition, t);
            fitData.Camera.SetZoom(Mathf.Lerp(fitData.StartZoom, fitData.FitZoom, t));
        }
        #endregion
    }

    public enum CameraFitOrientation
    {
        Horizontal,
        Vertical,
        Automatic
    }
}