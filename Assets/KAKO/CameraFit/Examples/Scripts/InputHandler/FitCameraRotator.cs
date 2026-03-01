using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kako.CameraFit.Examples
{
    public class FitCameraRotator : MonoBehaviour
    {
        [SerializeField] protected Camera fitCamera;
        [SerializeField] private Text rotateText;
        [SerializeField] private float rotateSpeed = 1f;
        
        private bool _isRotating;

        public static Action<bool> OnRotateClicked;

        public void CameraRotateClicked()
        {
            _isRotating = !_isRotating;
            rotateText.text = _isRotating ? "Stop\nRotation" : "Rotate\nCamera";
            OnRotateClicked?.Invoke(_isRotating);
        }
        private void FixedUpdate()
        {
            if(!_isRotating) return;

            fitCamera.transform.localEulerAngles += rotateSpeed * Vector3.up;
        }
    }
}