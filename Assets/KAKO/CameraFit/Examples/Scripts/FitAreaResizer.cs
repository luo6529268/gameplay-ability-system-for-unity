using UnityEngine;
using UnityEngine.EventSystems;

namespace Kako.CameraFit.Examples
{
    public class FitAreaResizer : MonoBehaviour, IDragHandler
    {
        [SerializeField] private FitAreaController controller;
        
        [SerializeField] private Vector2Int direction;

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.delta;
            if (direction.x == 0) delta.x = 0;
            if (direction.y == 0) delta.y = 0;

            AdjustFitArea(delta / controller.Canvas.scaleFactor);
        }

        private void AdjustFitArea(Vector2 delta)
        {
            delta.x *= direction.x;
            delta.y *= direction.y;
            controller.SetSize(controller.RectTransform.sizeDelta + delta);
            delta.x *= direction.x;
            delta.y *= direction.y;
            controller.RectTransform.anchoredPosition += delta * 0.5f;
        }
    }   
}
