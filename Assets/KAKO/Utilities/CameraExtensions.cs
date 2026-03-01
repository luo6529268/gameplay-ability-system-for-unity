using System;
using UnityEngine;

namespace Kako.Utilities
{
    public static class CameraExtensions
    {
        public static void Focus(this Camera cam, Vector3 worldPosition) => cam.transform.position = worldPosition.GetProjectionPoint(cam.transform);
        public static float GetZoom(this Camera cam) => cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        public static void SetZoom(this Camera cam, float amount)
        {
            if (cam.orthographic)
                cam.orthographicSize = amount;
            else
                cam.fieldOfView = amount;
        }
        public static float GetHorizontalFow(this Camera cam)
        {
            var radAngle = cam.fieldOfView * Mathf.Deg2Rad;
            var radFow = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * cam.aspect);
            return Mathf.Rad2Deg * radFow;
        }

        public static Vector2 GetOrthographicWorldSize(this Camera cam)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            return new Vector2(width, height);
        }

        public static Vector3 GetPointOnEdge(this Camera cam, Vector3 worldPosition, CameraEdges edge)
        {
            Transform camTransform = cam.transform;
            Vector3 normal = edge switch
            {
                CameraEdges.Left => -camTransform.right,
                CameraEdges.Right => camTransform.right,
                CameraEdges.Top => camTransform.up,
                CameraEdges.Bottom => -camTransform.up,
                _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };

            if (cam.orthographic)
            {
                Vector2 viewportPoint = edge is CameraEdges.Left or CameraEdges.Bottom
                    ? new Vector2(0, 0)
                    : new Vector2(1, 1);
                Vector3 edgePoint = worldPosition.GetProjectionPoint(cam.ViewportToWorldPoint(viewportPoint), normal);
                return edgePoint;
            }
            else
            {
                Vector3 projection = worldPosition.GetProjectionPoint(camTransform.position, normal);
                float distanceToCamera = Vector3.Distance(worldPosition.GetProjectionPoint(camTransform), worldPosition);

                Vector3 edgePoint = projection + normal * (distanceToCamera * Mathf.Tan((edge is CameraEdges.Left or CameraEdges.Right
                                                                  ? cam.GetHorizontalFow()
                                                                  : cam.fieldOfView) * 0.5f * Mathf.Deg2Rad));
                return edgePoint;
            }
        }
    }
    
    public enum CameraEdges
    {
        Left,
        Right,
        Top,
        Bottom
    }
}