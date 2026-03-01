using UnityEngine;
using Kako.Utilities;

namespace Kako.CameraFit
{
    public readonly struct PerspectiveCameraFrustumVectors
    {
        public readonly Vector3 CenterDirection;
        public readonly Vector3 CenterXDirection;
        public readonly Vector3 CenterYDirection;
        
        public readonly Vector3 UpperEdgeDirection;
        public readonly Vector3 BottomEdgeDirection;
        public readonly Vector3 RightEdgeDirection;
        public readonly Vector3 LeftEdgeDirection;
        
        public readonly Vector3 UpperEdgeNormal;
        public readonly Vector3 BottomEdgeNormal;
        public readonly Vector3 RightEdgeNormal;
        public readonly Vector3 LeftEdgeNormal;

        public PerspectiveCameraFrustumVectors(Camera cam)
        {
            Transform camTransform = cam.transform;
            Vector3 camUp = camTransform.up;
            Vector3 camRight = camTransform.right;
            Vector3 camForward = camTransform.forward;
            
            UpperEdgeDirection = GetDirection(camUp * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
            BottomEdgeDirection = GetDirection(-camUp * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
            RightEdgeDirection = GetDirection(camRight * Mathf.Tan(cam.GetHorizontalFow() * 0.5f * Mathf.Deg2Rad));
            LeftEdgeDirection = GetDirection(-camRight * Mathf.Tan(cam.GetHorizontalFow() * 0.5f * Mathf.Deg2Rad));
            
            UpperEdgeNormal = Vector3.Cross(UpperEdgeDirection, camRight);
            BottomEdgeNormal = Vector3.Cross(BottomEdgeDirection, -camRight);
            RightEdgeNormal = Vector3.Cross(RightEdgeDirection, -camUp);
            LeftEdgeNormal = Vector3.Cross(LeftEdgeDirection, camUp);

            CenterDirection = camForward;
            CenterXDirection = camForward;
            CenterYDirection = camForward;
            
            Vector3 GetDirection(Vector3 edgeDirection) => (camForward + edgeDirection).normalized;
        }
        
        public PerspectiveCameraFrustumVectors(Camera cam, RectTransform rect, Canvas canvas)
        {
            Transform camTransform = cam.transform;
            Vector3 camPosition = camTransform.position;
            Vector3 camUp = camTransform.up;
            Vector3 camRight = camTransform.right;
            Vector3 camForward = camTransform.forward;

            RectTransform canvasRectTransform = canvas.transform as RectTransform;
            Vector3[] canvasWorldCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasWorldCorners);

            Vector3 canvasRight = (canvasWorldCorners[3] - canvasWorldCorners[0]).normalized;
            Vector3 canvasUp = (canvasWorldCorners[1] - canvasWorldCorners[0]).normalized;
            float canvasWorldWidth = Vector3.Distance(canvasWorldCorners[0], canvasWorldCorners[3]);
            float canvasWorldHeight = Vector3.Distance(canvasWorldCorners[0], canvasWorldCorners[1]);
            
            Vector3[] rectWorldCorners = new Vector3[4];
            rect.GetWorldCorners(rectWorldCorners);
            Vector3 rectCenterWorldPoint = (rectWorldCorners[0] + rectWorldCorners[1] + rectWorldCorners[2] + rectWorldCorners[3]) * 0.25f;

            Vector3 referenceCenter = camPosition + camForward;
            Vector3 referenceVerticalVector = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * camUp;
            Vector3 referenceHorizontalVector = Mathf.Tan(cam.GetHorizontalFow() * 0.5f * Mathf.Deg2Rad) * camRight;

            Vector3 centerViewport = GetViewportPoint(rectCenterWorldPoint);
            CenterDirection = GetDirection(centerViewport);
            CenterYDirection = GetDirection(new Vector2(0.5f, centerViewport.y));
            CenterXDirection = GetDirection(new Vector2(centerViewport.x, 0.5f));
            
            UpperEdgeDirection = GetDirection(new Vector2(0.5f, GetViewportPoint((rectWorldCorners[1] + rectWorldCorners[2]) * 0.5f).y));
            BottomEdgeDirection = GetDirection(new Vector2(0.5f, GetViewportPoint((rectWorldCorners[0] + rectWorldCorners[3]) * 0.5f).y));
            RightEdgeDirection = GetDirection(new Vector2(GetViewportPoint((rectWorldCorners[2] + rectWorldCorners[3]) * 0.5f).x, 0.5f));
            LeftEdgeDirection = GetDirection(new Vector2(GetViewportPoint((rectWorldCorners[0] + rectWorldCorners[1]) * 0.5f).x, 0.5f));

            UpperEdgeNormal = Vector3.Cross(UpperEdgeDirection, camRight);
            BottomEdgeNormal = Vector3.Cross(BottomEdgeDirection, -camRight);
            RightEdgeNormal = Vector3.Cross(RightEdgeDirection, -camUp);
            LeftEdgeNormal = Vector3.Cross(LeftEdgeDirection, camUp);

            Vector2 GetViewportPoint(Vector3 position)
            {
                return new Vector2(Vector3.Dot(position - canvasWorldCorners[0], canvasRight) / canvasWorldWidth, Vector3.Dot(position - canvasWorldCorners[0], canvasUp) / canvasWorldHeight);
            }

            Vector3 GetDirection(Vector2 viewportPosition)
            {
                Vector3 directionEnd = referenceCenter + (viewportPosition.x-0.5f) * 2f * referenceHorizontalVector + (viewportPosition.y-0.5f) * 2f * referenceVerticalVector;
                Vector3 direction = directionEnd - camPosition;
                return direction.normalized;
            }
        }
    }
}