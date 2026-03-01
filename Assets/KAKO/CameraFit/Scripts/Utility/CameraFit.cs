using System.Collections.Generic;
using Kako.Utilities;
using UnityEngine;

namespace Kako.CameraFit
{
    public static class CameraFit
    {
        public static CameraFitData GetCameraFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, CameraFitOrientation orientation, Vector3? focus = null)
        {
            return cam.orthographic 
                ? GetOrthographicFitData(cam, positions, paddingX, paddingY, orientation, focus) 
                : GetPerspectiveFitData(cam, positions, paddingX, paddingY, orientation, focus);
        }
        
        public static CameraFitData GetCameraRectFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, RectTransform fitArea, Canvas canvas, CameraFitOrientation orientation, Vector3? focus = null)
        {
            return cam.orthographic 
                ? GetOrthographicRectFitData(cam, positions, paddingX, paddingY, fitArea, canvas, orientation, focus) 
                : GetPerspectiveRectFitData(cam, positions, paddingX, paddingY, fitArea, canvas, orientation, focus);
        }
        
        private static CameraFitData GetOrthographicFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, CameraFitOrientation orientation, Vector3? focus = null)
        {
            GetOrthographicFitValues(cam, positions, paddingX, paddingY, focus, out Vector3 focusedCameraPosition, out Vector2 size);
            float fitZoom = GetOrthographicFitZoom(cam, size, orientation);
            return new CameraFitData(cam, cam.transform.position, focusedCameraPosition, cam.orthographicSize, fitZoom);
        }
        
        private static CameraFitData GetOrthographicRectFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, RectTransform fitArea, Canvas canvas, CameraFitOrientation orientation, Vector3? focus = null)
        {
            GetOrthographicFitValues(cam, positions, paddingX, paddingY, focus, out Vector3 focusedCameraPosition, out Vector2 size);
            
            size.x *= canvas.Size().x / fitArea.rect.width;
            size.y *= canvas.Size().y / fitArea.rect.height;

            float fitZoom = GetOrthographicFitZoom(cam, size, orientation);

            Vector3 camPosition = cam.transform.position;
            Vector3 rectCenterScreenPoint = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, fitArea.TransformPoint(fitArea.rect.center));
            focusedCameraPosition -= (cam.ScreenToWorldPoint(rectCenterScreenPoint) - camPosition) * fitZoom / cam.orthographicSize;
            
            return new CameraFitData(cam, camPosition, focusedCameraPosition, cam.orthographicSize, fitZoom);
        }

        private static void GetOrthographicFitValues(Camera cam, Vector3[] positions, float paddingX, float paddingY, Vector3? focus, out Vector3 focusedCameraPosition, out Vector2 size)
        {
            ExtremePositions extremePositions = GetOrthographicExtremes(cam, positions);
            focusedCameraPosition = GetFocusedCameraPosition(cam, extremePositions, focus);
            size = CalculateSize(cam, extremePositions, paddingX, paddingY, focus);
        }
        private static float GetOrthographicFitZoom(Camera cam, Vector2 size, CameraFitOrientation orientation)
        {
            if (orientation == CameraFitOrientation.Automatic) orientation = size.x / cam.aspect > size.y ? CameraFitOrientation.Horizontal : CameraFitOrientation.Vertical;
            return orientation == CameraFitOrientation.Horizontal ? size.x * 0.5f / cam.aspect : size.y * 0.5f;
        }

        private static Vector2 CalculateSize(Camera cam, ExtremePositions extremePositions, float paddingX, float paddingY, Vector3? focus = null)
        {
            Transform camTransform = cam.transform;
            Vector3 camUp = camTransform.up;
            Vector3 camRight = camTransform.right;

            Vector2 size;

            if (focus == null)
            {
                size.x = Mathf.Abs(Vector3.Dot(extremePositions.RightMost - extremePositions.LeftMost, camRight));   
                size.y = Mathf.Abs(Vector3.Dot(extremePositions.UpMost - extremePositions.DownMost, camUp));
            }
            else
            {
                float leftDistance = Mathf.Abs(Vector3.Dot(extremePositions.LeftMost - focus.Value, camRight));
                float rightDistance = Mathf.Abs(Vector3.Dot(extremePositions.RightMost - focus.Value, camRight));
                float upDistance = Mathf.Abs(Vector3.Dot(extremePositions.UpMost - focus.Value, camUp));
                float downDistance = Mathf.Abs(Vector3.Dot(extremePositions.DownMost - focus.Value, camUp));

                float maxHorizontalDistance = leftDistance > rightDistance ? leftDistance : rightDistance;
                float maxVerticalDistance = upDistance > downDistance ? upDistance : downDistance;
                
                size.x = maxHorizontalDistance * 2f;
                size.y = maxVerticalDistance * 2f;
            }

            size.x += paddingX * 2f;
            size.y += paddingY * 2f;
            
            return size;
        }
        
        private static Vector3 GetFocusedCameraPosition(Camera cam, ExtremePositions positionData, Vector3? focus = null)
        {
            Vector3 focusedPosition;
            Transform camTransform = cam.transform;
            if (focus == null)
            {
                Vector3 horizontalCenter = (positionData.LeftMost + positionData.RightMost) * 0.5f;
                Vector3 verticalCenter = (positionData.UpMost + positionData.DownMost) * 0.5f;
                Vector3 center = horizontalCenter.GetProjectionPoint(verticalCenter, camTransform.up);
                focusedPosition = center.GetProjectionPoint(camTransform);
            }
            else
            {
                focusedPosition = focus.Value.GetProjectionPoint(camTransform);
            }

            float backMostDistance = cam.transform.InverseTransformPoint(positionData.BackMost).z-1;
            if(backMostDistance < 0)
                focusedPosition += camTransform.forward * (backMostDistance-1);
            return focusedPosition;
        }

        private static ExtremePositions GetOrthographicExtremes(Camera cam, Vector3[] positions)
        {
            Transform camTransform = cam.transform;
            Vector3 firstInversePoint = camTransform.InverseTransformPoint(positions[0]);
            Vector3 leftMost = firstInversePoint;
            Vector3 rightMost = firstInversePoint;
            Vector3 downMost = firstInversePoint;
            Vector3 upMost = firstInversePoint;
            Vector3 backMost = firstInversePoint;

            for (var i = 1; i < positions.Length; i++)
            {
                Vector3 inversePoint = camTransform.InverseTransformPoint(positions[i]);
                if (inversePoint.x < leftMost.x) leftMost = inversePoint;
                if (inversePoint.x > rightMost.x) rightMost = inversePoint;
                if (inversePoint.y < downMost.y) downMost = inversePoint;
                if (inversePoint.y > upMost.y) upMost = inversePoint;
                if (inversePoint.z < backMost.z) backMost = inversePoint;
            }
            leftMost = camTransform.TransformPoint(leftMost);
            rightMost = camTransform.TransformPoint(rightMost);
            downMost = camTransform.TransformPoint(downMost);
            upMost = camTransform.TransformPoint(upMost);
            backMost = camTransform.TransformPoint(backMost);
            
            return new ExtremePositions(leftMost, rightMost, upMost, downMost, backMost);
        }
        
        private static CameraFitData GetPerspectiveFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, CameraFitOrientation orientation, Vector3? focus = null)
        {
            Vector3 fitPoint;
            PerspectiveCameraFrustumVectors frustumVectors = new PerspectiveCameraFrustumVectors(cam);
            ExtremePositions extremePositions = GetPerspectiveExtremes(cam, positions, paddingX, paddingY, frustumVectors);
            GetFrustumRays(extremePositions, frustumVectors, out Ray rayUp, out Ray rayDown, out Ray rayLeft, out Ray rayRight);
            
            Transform camTransform = cam.transform;
            float fow = cam.fieldOfView;

            if (focus != null)
            {
                Ray rayCenter = new Ray(focus.Value, -camTransform.forward);
                List<Vector3> intersectionPoints = new List<Vector3>();
                if (orientation is CameraFitOrientation.Horizontal or CameraFitOrientation.Automatic)
                {
                    intersectionPoints.Add(GetIntersectionPoint(rayCenter, rayLeft));
                    intersectionPoints.Add(GetIntersectionPoint(rayCenter, rayRight));
                }
                if (orientation is CameraFitOrientation.Vertical or CameraFitOrientation.Automatic)
                {
                    intersectionPoints.Add(GetIntersectionPoint(rayCenter, rayUp));
                    intersectionPoints.Add(GetIntersectionPoint(rayCenter, rayDown));
                }
                fitPoint = intersectionPoints[0];
                for (var i = 1; i < intersectionPoints.Count; i++)
                {
                    if (IsOnBackward(intersectionPoints[i], fitPoint)) 
                        fitPoint = intersectionPoints[i];
                }

                return new CameraFitData(cam, camTransform.position, fitPoint, fow, fow);
            }
            
            Vector3 camUp = camTransform.up;
            Vector3 camRight = camTransform.right;

            Vector3 verticalIntersectionPoint = GetIntersectionPoint(rayUp, rayDown);
            Vector3 horizontalIntersectionPoint = GetIntersectionPoint(rayLeft, rayRight);

            if (orientation == CameraFitOrientation.Automatic) 
                orientation = IsOnBackward(horizontalIntersectionPoint, verticalIntersectionPoint) ? CameraFitOrientation.Horizontal : CameraFitOrientation.Vertical;
            
            if (orientation == CameraFitOrientation.Horizontal) 
                fitPoint = horizontalIntersectionPoint + camUp * Vector3.Dot(verticalIntersectionPoint - horizontalIntersectionPoint, camUp);
            else 
                fitPoint = verticalIntersectionPoint + camRight * Vector3.Dot(horizontalIntersectionPoint - verticalIntersectionPoint, camRight);

            return new CameraFitData(cam, camTransform.position, fitPoint, fow, fow);

            bool IsOnBackward(Vector3 point, Vector3 comparePoint) => cam.transform.InverseTransformPoint(point).z < cam.transform.InverseTransformPoint(comparePoint).z;
        }

        private static CameraFitData GetPerspectiveRectFitData(Camera cam, Vector3[] positions, float paddingX, float paddingY, RectTransform fitArea, Canvas canvas, CameraFitOrientation orientation, Vector3? focus = null)
        {
            PerspectiveCameraFrustumVectors frustumVectors = new PerspectiveCameraFrustumVectors(cam, fitArea, canvas);
            ExtremePositions extremePositions = GetPerspectiveExtremes(cam, positions, paddingX, paddingY, frustumVectors);
            GetFrustumRays(extremePositions, frustumVectors, out Ray rayUp, out Ray rayDown, out Ray rayLeft, out Ray rayRight);
            
            Transform camTransform = cam.transform;
            float fow = cam.fieldOfView;

            Vector3 verticalIntersectionPoint;
            Vector3 horizontalIntersectionPoint;
            
            if (focus != null)
            {
                Ray rayCenterX = new Ray(focus.Value, -frustumVectors.CenterXDirection);
                Ray rayCenterY = new Ray(focus.Value, -frustumVectors.CenterYDirection);
                
                Vector3 leftIntersection = GetIntersectionPoint(rayCenterX, rayLeft);
                Vector3 rightIntersection = GetIntersectionPoint(rayCenterX, rayRight);
                Vector3 upIntersection = GetIntersectionPoint(rayCenterY, rayUp);
                Vector3 downIntersection = GetIntersectionPoint(rayCenterY, rayDown);
                
                horizontalIntersectionPoint = IsOnBackward(leftIntersection, rightIntersection) ? leftIntersection : rightIntersection;
                verticalIntersectionPoint = IsOnBackward(upIntersection, downIntersection) ? upIntersection : downIntersection;
            }
            else
            {
                horizontalIntersectionPoint = GetIntersectionPoint(rayLeft, rayRight);
                verticalIntersectionPoint = GetIntersectionPoint(rayUp, rayDown);
            }
            
            if (orientation == CameraFitOrientation.Automatic) 
                orientation = IsOnBackward(horizontalIntersectionPoint, verticalIntersectionPoint) ? CameraFitOrientation.Horizontal : CameraFitOrientation.Vertical;
            
            if (orientation == CameraFitOrientation.Horizontal)
            {
                Ray ray1 = new Ray(horizontalIntersectionPoint, camTransform.up);
                Ray ray2 = new Ray(verticalIntersectionPoint, -frustumVectors.CenterYDirection);
                return new CameraFitData(cam, camTransform.position, GetIntersectionPoint(ray1, ray2), fow, fow);
            }
            else
            {
                Ray ray1 = new Ray(verticalIntersectionPoint, camTransform.right);
                Ray ray2 = new Ray(horizontalIntersectionPoint, -frustumVectors.CenterXDirection);
                return new CameraFitData(cam, camTransform.position, GetIntersectionPoint(ray1, ray2), fow, fow);
            }

            bool IsOnBackward(Vector3 point, Vector3 comparePoint) => camTransform.InverseTransformPoint(point).z < camTransform.InverseTransformPoint(comparePoint).z;
        }

        private static void GetFrustumRays(ExtremePositions extremePositions, PerspectiveCameraFrustumVectors frustumVectors, out Ray up, out Ray down, out Ray left, out Ray right)
        {
            up = new Ray(extremePositions.UpMost, -frustumVectors.UpperEdgeDirection);
            down = new Ray(extremePositions.DownMost, -frustumVectors.BottomEdgeDirection);
            left = new Ray(extremePositions.LeftMost, -frustumVectors.LeftEdgeDirection);
            right = new Ray(extremePositions.RightMost, -frustumVectors.RightEdgeDirection);
        }
        
        private static Vector3 GetIntersectionPoint(Ray ray1, Ray ray2)
        {
            Vector3 cross = Vector3.Cross(ray1.direction, ray2.direction);
            float denominator = cross.magnitude * cross.magnitude;
            Vector3 diff = ray2.origin - ray1.origin;
            float t1 = Vector3.Dot(Vector3.Cross(diff, ray2.direction), cross) / denominator;

            return ray1.origin + ray1.direction * t1;
        }
        
        private static ExtremePositions GetPerspectiveExtremes(Camera cam, Vector3[] positions, float paddingX, float paddingY, PerspectiveCameraFrustumVectors frustumVectors)
        {
            Transform camTransform = cam.transform;
            Vector3 cameraPosition = camTransform.position;
            
            Vector3 paddingLeftVector = frustumVectors.LeftEdgeNormal * paddingX;
            Vector3 paddingRightVector = frustumVectors.RightEdgeNormal * paddingX;
            Vector3 paddingDownVector = frustumVectors.BottomEdgeNormal * paddingY;
            Vector3 paddingUpVector = frustumVectors.UpperEdgeNormal * paddingY;

            Vector3 leftMost = positions[0] + paddingLeftVector;
            Vector3 rightMost = positions[0] + paddingRightVector;
            Vector3 downMost = positions[0] + paddingDownVector;
            Vector3 upMost = positions[0] + paddingUpVector;

            for (var i = 1; i < positions.Length; i++)
            {
                Vector3 position = positions[i];
                if (IsExtreme(position + paddingLeftVector, leftMost, frustumVectors.LeftEdgeNormal)) leftMost = position + paddingLeftVector;
                if (IsExtreme(position + paddingRightVector, rightMost, frustumVectors.RightEdgeNormal)) rightMost = position + paddingRightVector;
                if (IsExtreme(position + paddingDownVector, downMost, frustumVectors.BottomEdgeNormal)) downMost = position + paddingDownVector;
                if (IsExtreme(position + paddingUpVector, upMost, frustumVectors.UpperEdgeNormal)) upMost = position + paddingUpVector;
            }
            
            return new ExtremePositions(leftMost, rightMost, upMost, downMost, Vector3.zero);
            
            bool IsExtreme(Vector3 position, Vector3 extremePosition, Vector3 direction)
            {
                return Vector3.Dot(position - cameraPosition, direction) > Vector3.Dot(extremePosition - cameraPosition, direction);   
            }
        }

        private struct ExtremePositions
        {
            public readonly Vector3 LeftMost;
            public readonly Vector3 RightMost;
            public readonly Vector3 UpMost;
            public readonly Vector3 DownMost;
            public readonly Vector3 BackMost;

            public ExtremePositions(Vector3 leftMost, Vector3 rightMost, Vector3 upMost, Vector3 downMost, Vector3 backMost)
            {
                LeftMost = leftMost;
                RightMost = rightMost;
                UpMost = upMost;
                DownMost = downMost;
                BackMost = backMost;
            }
        }
    }
}