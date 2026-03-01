using UnityEngine;
using UnityEngine.UI;

namespace Kako.CameraFit.Examples
{
    public class FitterInputHandler : MonoBehaviour
    {
        [SerializeField] private CameraFitter fitter;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Text hideUIText;
        [SerializeField] private GameObject fitArea;
        [SerializeField] private GameObject focusObject;

        public void FitClicked()
        {
            fitter.Fit();
        }
        public void HideUIClicked()
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
            hideUIText.text = settingsPanel.activeSelf ? "Hide UI" : "Show UI";
        }

        public void RandomizePositionsClicked()
        {
            for (var i = 0; i < fitter.FitPositions.Length; i++)
            {
                fitter.FitPositions[i] = GetRandomPosition();
            }
            foreach (var t in fitter.FitPivots)
            {
                t.transform.position = GetRandomPosition();
            }
            Vector3 GetRandomPosition() => new (Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5));
        }

        public void OrthographicProjectionClicked(Toggle toggle) => mainCamera.orthographic = toggle.isOn;
        public void PerspectiveProjectionClicked(Toggle toggle) => mainCamera.orthographic = !toggle.isOn;
        public void OnIsRectFitClicked(Toggle toggle)
        {
            fitter.IsRectFit = toggle.isOn;
            fitArea.SetActive(toggle.isOn);
        }
        public void IsFocusOnClicked(Toggle toggle)
        {
            fitter.HasFocus = toggle.isOn;
            focusObject.SetActive(toggle.isOn);
        }

        public void OnOrientationChanged(Dropdown dropdown)
        {
            if (dropdown.captionText.text == dropdown.options[0].text)
                fitter.Orientation = CameraFitOrientation.Automatic;
            else if (dropdown.captionText.text == dropdown.options[1].text)
                fitter.Orientation = CameraFitOrientation.Horizontal;
            else if (dropdown.captionText.text == dropdown.options[2].text)
                fitter.Orientation = CameraFitOrientation.Vertical;
        }

        public void OnPaddingXChanged(InputField input)
        {
            if (!float.TryParse(input.text, out var result)) result = 0;
            fitter.Padding = new Vector2(result, fitter.Padding.y);
        }

        public void OnPaddingYChanged(InputField input)
        {
            if (!float.TryParse(input.text, out var result)) result = 0;
            fitter.Padding = new Vector2(fitter.Padding.x, result);
        }
    }
}