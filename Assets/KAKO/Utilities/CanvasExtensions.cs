using UnityEngine;

namespace Kako.Utilities
{
    public static class CanvasExtensions
    {
        public static Vector3 Size(this Canvas canvas)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            return canvasRect.rect.size;
        }
    }
}
