using DG.Tweening;
using UnityEngine;

namespace Kako.CameraFit.Examples
{
    public class AnimatedFitHandler : MonoBehaviour
    {
        public float Duration { get => duration; set => duration = value; }
        public Ease Ease { get => ease; set => ease = value; }
        
        [SerializeField] private CameraFitter fitter;
        
        [Header("Animation Settings")] 
        [SerializeField] private float duration = 1;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private Sequence _fitSequence;

        public void Fit()
        {
            _fitSequence.Kill();
            _fitSequence = fitter.Fit().DOFit(duration, ease);
        }
    }
}