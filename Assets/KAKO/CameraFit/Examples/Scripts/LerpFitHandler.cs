using UnityEngine;
using UnityEngine.UI;

namespace Kako.CameraFit.Examples
{
    public class LerpFitHandler : MonoBehaviour
    {
        [SerializeField] private CameraFitter fitter;

        private float _lerpSpeed = 5;
        public void OnLerpSpeedChanged(InputField input)
        {
            if (!float.TryParse(input.text, out float result)) result = 0;
            _lerpSpeed = result;
        }

        private void LateUpdate()
        {
            fitter.Fit().Lerp(Time.deltaTime * _lerpSpeed);
        }
    }
}