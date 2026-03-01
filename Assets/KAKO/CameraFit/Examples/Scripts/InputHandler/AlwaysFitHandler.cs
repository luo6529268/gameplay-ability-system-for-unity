using UnityEngine;
using UnityEngine.UI;


namespace Kako.CameraFit.Examples
{
    public class AlwaysFitHandler : MonoBehaviour
    {
        [SerializeField] private CameraFitter fitter;
        [SerializeField] private Toggle fitAlwaysToggle;
        [SerializeField] private Button fitButton;

        private void Start()
        {
            FitCameraRotator.OnRotateClicked += HandleRotateCamera;
        }

        private void HandleRotateCamera(bool isRotating)
        {
            if (isRotating)
            {
                fitAlwaysToggle.isOn = true;
                fitAlwaysToggle.interactable = false;
            }
            else
            {
                fitAlwaysToggle.interactable = true;
            }
        }

        public void FitAlwaysClicked(Toggle toggle)
        {
            fitButton.interactable = !toggle.isOn;
        }

        private void Update()
        {
            if(!fitAlwaysToggle.isOn) return;
            fitter.Fit();
        }
    }
}
