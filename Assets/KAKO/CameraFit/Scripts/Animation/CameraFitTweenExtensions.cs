using System;
using DG.Tweening;
using Kako.Utilities;
using UnityEngine;

namespace Kako.CameraFit
{
    public static class CameraFitTweenExtensions
    {
        /// <summary>Adjust the camera's position and size smoothly to fit.</summary>
        /// <param name="fitData">Desired camera fit action that is offered by Kako Camera Fit</param>
        /// <param name="duration">The duration of the sequence.</param>
        /// <param name="ease">If APPLIED to Sequences eases the whole sequence animation.</param>
        public static Sequence DOFit(this CameraFitData fitData, float duration, Ease ease = Ease.OutQuad)
        {
            fitData.Camera.transform.position = fitData.StartPosition;
            fitData.Camera.SetZoom(fitData.StartZoom);

            Sequence fitSequence = DOTween.Sequence();
            fitSequence.Join(fitData.Camera.transform.DOMove(fitData.FitPosition, duration).SetEase(ease));
            fitSequence.Join(fitData.Camera.orthographic
                ? fitData.Camera.DOOrthoSize(fitData.FitZoom, duration).SetEase(ease)
                : fitData.Camera.DOFieldOfView(fitData.FitZoom, duration).SetEase(ease));

            return fitSequence;
        }
    }
}