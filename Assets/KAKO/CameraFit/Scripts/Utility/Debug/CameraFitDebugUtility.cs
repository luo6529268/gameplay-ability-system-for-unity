using Kako.CameraFit;
using Kako.Utilities;
using UnityEngine;

public static class CameraFitDebugUtility
{
    // 0 => Bottom Left Corner
    // 1 => Top Left Corner
    // 2 => Top Right Corner
    // 3 => Bottom Right Corner
    // 4 => Center
    
    public static void DrawCameraFrustum(Camera camera, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetClipPlanePoints(camera, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetClipPlanePoints(camera, camera.farClipPlane);
        DrawRectFrustum(nearRectClipPlanePoints, farRectClipPlanePoints, color);
    }

    public static void DrawCameraClipPlane(Camera camera, float percent, Color color)
    {
        float distance = camera.nearClipPlane + (camera.farClipPlane - camera.nearClipPlane) * percent;
        Vector3[] clipPlanePoints = GetClipPlanePoints(camera, distance);
        DrawRectClipPlane(clipPlanePoints, color);
    }

    public static void DrawCenterLine(Camera camera, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetClipPlanePoints(camera, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetClipPlanePoints(camera, camera.farClipPlane);
        
        Gizmos.color = color;
        Gizmos.DrawLine(nearRectClipPlanePoints[4], farRectClipPlanePoints[4]);
        Gizmos.color = Color.white;
    }


    public static void DrawRectPerspectiveCameraFrustum(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetPerspectiveRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetPerspectiveRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.farClipPlane);
        DrawRectFrustum(nearRectClipPlanePoints, farRectClipPlanePoints, color);
    }
    
    public static void DrawRectPerspectiveCenterLine(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetPerspectiveRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetPerspectiveRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.farClipPlane);
        
        Gizmos.color = color;
        Gizmos.DrawLine(nearRectClipPlanePoints[4], farRectClipPlanePoints[4]);
        Gizmos.color = Color.white;
    }

    public static void DrawRectOrthographicCameraFrustum(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetOrthographicRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetOrthographicRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.farClipPlane);
        DrawRectFrustum(nearRectClipPlanePoints, farRectClipPlanePoints, color);
    }
    
    public static void DrawRectOrthographicCenterLine(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, Color color)
    {
        Vector3[] nearRectClipPlanePoints = GetOrthographicRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.nearClipPlane);
        Vector3[] farRectClipPlanePoints = GetOrthographicRectClipPlanePoints(camera, fitArea, fitAreaCanvas, camera.farClipPlane);
        
        Gizmos.color = color;
        Gizmos.DrawLine(nearRectClipPlanePoints[4], farRectClipPlanePoints[4]);
        Gizmos.color = Color.white;
    }

    public static void DrawRectPerspectiveCameraClipPlane(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, float percent, Color color)
    {
        float distance = camera.nearClipPlane + (camera.farClipPlane - camera.nearClipPlane) * percent;
        Vector3[] clipPlanePoints = GetPerspectiveRectClipPlanePoints(camera, fitArea, fitAreaCanvas, distance);
        DrawRectClipPlane(clipPlanePoints, color);
    }
    public static void DrawRectOrthographicCameraClipPlane(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, float percent, Color color)
    {
        float distance = camera.nearClipPlane + (camera.farClipPlane - camera.nearClipPlane) * percent;
        Vector3[] clipPlanePoints = GetOrthographicRectClipPlanePoints(camera, fitArea, fitAreaCanvas, distance);
        DrawRectClipPlane(clipPlanePoints, color);
    }

    private static void DrawRectFrustum(Vector3[] nearRectClipPlanePoints, Vector3[] farRectClipPlanePoints, Color color)
    {
        // Clip PLane Cross
        Gizmos.color = Color.black;
        Gizmos.DrawLine(nearRectClipPlanePoints[0], nearRectClipPlanePoints[2]);
        Gizmos.DrawLine(nearRectClipPlanePoints[1], nearRectClipPlanePoints[3]);
        Gizmos.DrawLine(farRectClipPlanePoints[0], farRectClipPlanePoints[2]);
        Gizmos.DrawLine(farRectClipPlanePoints[1], farRectClipPlanePoints[3]);

        // Near Clip Plane Edges
        DrawClipPlaneEdges(nearRectClipPlanePoints, color);
        // Far Clip Plane Edges
        DrawClipPlaneEdges(farRectClipPlanePoints, color);
        
        // Lines From Near Plane To Far Plane
        Gizmos.color = color;
        Gizmos.DrawLine(nearRectClipPlanePoints[0], farRectClipPlanePoints[0]);
        Gizmos.DrawLine(nearRectClipPlanePoints[1], farRectClipPlanePoints[1]);
        Gizmos.DrawLine(nearRectClipPlanePoints[2], farRectClipPlanePoints[2]);
        Gizmos.DrawLine(nearRectClipPlanePoints[3], farRectClipPlanePoints[3]);
        Gizmos.color = Color.white;
    }

    private static void DrawClipPlaneEdges(Vector3[] clipPlanePoints, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(clipPlanePoints[0], clipPlanePoints[1]);
        Gizmos.DrawLine(clipPlanePoints[1], clipPlanePoints[2]);
        Gizmos.DrawLine(clipPlanePoints[2], clipPlanePoints[3]);
        Gizmos.DrawLine(clipPlanePoints[3], clipPlanePoints[0]);
        Gizmos.color = Color.white;
    }

    private static void DrawRectClipPlane(Vector3[] clipPlanePoints, Color color)
    {
        // Clip Plane Edges
        DrawClipPlaneEdges(clipPlanePoints, color);

        // +
        Gizmos.color = color;
        Gizmos.DrawLine((clipPlanePoints[0] + clipPlanePoints[1]) * 0.5f, (clipPlanePoints[2] + clipPlanePoints[3]) * 0.5f);
        Gizmos.DrawLine((clipPlanePoints[1] + clipPlanePoints[2]) * 0.5f, (clipPlanePoints[0] + clipPlanePoints[3]) * 0.5f);
        Gizmos.color = Color.white;
    }

    private static Vector3[] GetClipPlanePoints(Camera camera, float distance)
    {
        Transform cameraTransform = camera.transform;
        Vector3 cameraPosition = cameraTransform.position;

        float nearClipPlaneHeight = camera.orthographic ? camera.orthographicSize: Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
        float nearClipPlaneWidth = nearClipPlaneHeight * camera.aspect;
        
        Vector3 cameraUp = cameraTransform.up;
        Vector3 cameraRight = cameraTransform.right;
        
        Vector3 center = cameraPosition + cameraTransform.forward * distance;
        Vector3 topLeft = center - cameraRight * nearClipPlaneWidth + cameraUp * nearClipPlaneHeight;
        Vector3 topRight = center + cameraRight * nearClipPlaneWidth + cameraUp * nearClipPlaneHeight;
        Vector3 bottomLeft = center - cameraRight * nearClipPlaneWidth - cameraUp * nearClipPlaneHeight;
        Vector3 bottomRight = center + cameraRight * nearClipPlaneWidth - cameraUp * nearClipPlaneHeight;

        return new[] { bottomLeft, topLeft, topRight, bottomRight, center };
    }
    
    private static Vector3[] GetPerspectiveRectClipPlanePoints(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, float distance)
    {
        Transform cameraTransform = camera.transform;
        Vector3 cameraRight = cameraTransform.right;
        Vector3 cameraUp = cameraTransform.up;

        PerspectiveCameraFrustumVectors directionData = new PerspectiveCameraFrustumVectors(camera, fitArea, fitAreaCanvas);
        
        Vector3 center = GetRayPointOnPlane(camera, directionData.CenterDirection, distance);
        
        Vector3 left = GetRayPointOnPlane(camera, directionData.LeftEdgeDirection, distance);
        Vector3 right = GetRayPointOnPlane(camera, directionData.RightEdgeDirection, distance);
        Vector3 top = GetRayPointOnPlane(camera, directionData.UpperEdgeDirection, distance);
        Vector3 bottom = GetRayPointOnPlane(camera, directionData.BottomEdgeDirection, distance);

        float farRectHalfWidth = Vector3.Distance(left, right) * 0.5f;
        float farRectHalfHeight = Vector3.Distance(bottom, top) * 0.5f;

        Vector3 topLeft = center - farRectHalfWidth * cameraRight + farRectHalfHeight * cameraUp;
        Vector3 topRight = center + farRectHalfWidth * cameraRight + farRectHalfHeight * cameraUp;
        Vector3 bottomLeft = center - farRectHalfWidth * cameraRight - farRectHalfHeight * cameraUp;
        Vector3 bottomRight = center + farRectHalfWidth * cameraRight - farRectHalfHeight * cameraUp;

        return new[] { bottomLeft, topLeft, topRight, bottomRight, center };
    }
    
    private static Vector3[] GetOrthographicRectClipPlanePoints(Camera camera, RectTransform fitArea, Canvas fitAreaCanvas, float distance)
    {
        Transform cameraTransform = camera.transform;
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraRight = cameraTransform.right;
        Vector3 cameraUp = cameraTransform.up;

        Vector3 rectCenterScreenPoint = RectTransformUtility.WorldToScreenPoint(fitAreaCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, fitArea.TransformPoint(fitArea.rect.center));
        Vector3 rectCenterOffset = camera.ScreenToWorldPoint(rectCenterScreenPoint) - cameraPosition;
        Vector3 center = cameraPosition + cameraTransform.forward * distance + rectCenterOffset;

        float halfHeight = camera.orthographicSize * fitArea.rect.height / fitAreaCanvas.Size().y;
        float halfWidth = camera.orthographicSize * camera.aspect * fitArea.rect.width / fitAreaCanvas.Size().x;

        Vector3 topLeft = center - halfWidth * cameraRight + halfHeight * cameraUp;
        Vector3 topRight = center + halfWidth * cameraRight + halfHeight * cameraUp;
        Vector3 bottomLeft = center - halfWidth * cameraRight - halfHeight * cameraUp;
        Vector3 bottomRight = center + halfWidth * cameraRight - halfHeight * cameraUp;

        return new[] { bottomLeft, topLeft, topRight, bottomRight, center };
    }
    
    private static Vector3 GetRayPointOnPlane(Camera camera, Vector3 direction, float distance)
    {
        float d = distance / Vector3.Dot(direction, camera.transform.forward);
        return camera.transform.position + direction * d;
    }
}
