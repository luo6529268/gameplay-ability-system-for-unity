using Kako.Utilities;
using UnityEngine;

namespace Kako.CameraFit.Examples
{
    public class FitAreaController : MonoBehaviour
    {
        public Canvas Canvas => canvas;
        public RectTransform RectTransform => rectTransform;
        
        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RectTransform rectTransform;
        
        private Vector3 _mouseStart;
        private Vector3 _startPosition;
        private bool _isDragging;
        
        public void StartMove()
        {
            _isDragging = true;
            _mouseStart = Input.mousePosition;
            _startPosition = rectTransform.anchoredPosition;
        }

        public void OnDrag()
        {
            if(!_isDragging) return;
            Vector3 delta = Input.mousePosition - _mouseStart;
            rectTransform.anchoredPosition = _startPosition + new Vector3(delta.x * canvas.Size().x / Screen.width, delta.y * canvas.Size().y / Screen.height);
        }

        public void EndMove()
        {
            _isDragging = false;
        }

        public void SetSize(Vector2 size) => rectTransform.sizeDelta = size;
    }
}
