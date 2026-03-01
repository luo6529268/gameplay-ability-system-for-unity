using UnityEngine;

namespace Kako.CameraFit
{
    public struct CameraFitData
    {
        public readonly Camera Camera;
        public readonly Vector3 StartPosition, FitPosition;
        public readonly float StartZoom, FitZoom;

        public CameraFitData(Camera camera, Vector3 startPosition, Vector3 fitPosition, float startZoom, float fitZoom)
        {
            Camera = camera;
            StartPosition = startPosition;
            FitPosition = fitPosition;
            StartZoom = startZoom;
            FitZoom = fitZoom;
        }
    }
}