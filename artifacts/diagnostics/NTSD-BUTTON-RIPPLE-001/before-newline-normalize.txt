using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NTSDButton))]
    [AddComponentMenu("NTSD/UI/NTSD Button Ripple")]
    public sealed class NTSDButtonRipple : MonoBehaviour, ISubmitHandler
    {
        [SerializeField] private UnityEngine.UI.Image ringImage;
        [SerializeField] private Color ringColor = new Color(0.1f, 0.85f, 1f, 0.85f);
        [SerializeField] private Vector2 startSize = new Vector2(90f, 90f);
        [SerializeField] private Vector2 endSize = new Vector2(175f, 175f);
        [SerializeField, Min(0.01f)] private float duration = 0.45f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private NTSDButton button;
        private Tween rippleTween;
        private float progress;
        private bool hasFocus = true;
        private bool paused;

        private void Awake()
        {
            button = GetComponent<NTSDButton>();
            if (ringImage != null)
            {
                ringImage.raycastTarget = false;
                RectTransform rect = ringImage.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                ringImage.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (button != null)
                button.PressedStateChanged += HandlePressedStateChanged;
        }

        private void OnDisable()
        {
            if (button != null)
                button.PressedStateChanged -= HandlePressedStateChanged;
            StopRipple();
        }

        private void OnDestroy()
        {
            if (button != null)
                button.PressedStateChanged -= HandlePressedStateChanged;
            StopRipple();
            rippleTween?.Kill(false);
            rippleTween = null;
        }

        private void HandlePressedStateChanged(NTSDButton source, bool pressed)
        {
            if (pressed)
                PlayRipple();
            else if (!source.isActiveAndEnabled || !source.IsInteractable())
                StopRipple();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            PlayRipple();
        }

        public void PlayRipple()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || !hasFocus || paused ||
                button == null || !button.isActiveAndEnabled || !button.IsInteractable() ||
                ringImage == null || ringImage.sprite == null)
            {
                return;
            }

            if (rippleTween == null || !rippleTween.IsActive())
            {
                rippleTween = DOTween.To(() => progress, value => progress = value, 1f,
                        1f)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .SetAutoKill(false)
                    .OnUpdate(ApplyProgress)
                    .OnComplete(HideRing);
            }

            // Restart one cached effect; the button's held state is never changed by animation.
            rippleTween.Pause();
            rippleTween.Rewind();
            rippleTween.timeScale = 1f / Mathf.Max(0.01f, duration);
            progress = 0f;
            ringImage.enabled = true;
            ApplyProgress();
            rippleTween.Restart();
        }

        private void ApplyProgress()
        {
            if (ringImage == null)
                return;
            ringImage.rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, progress);
            Color color = ringColor;
            color.a *= Mathf.Clamp01(fadeCurve == null ? 1f - progress : fadeCurve.Evaluate(progress));
            ringImage.color = color;
        }

        private void HideRing()
        {
            if (ringImage != null)
                ringImage.enabled = false;
        }

        private void StopRipple()
        {
            rippleTween?.Pause();
            HideRing();
        }

        private void OnApplicationFocus(bool focused)
        {
            hasFocus = focused;
            if (!focused)
                StopRipple();
        }

        private void OnApplicationPause(bool value)
        {
            paused = value;
            if (value)
                StopRipple();
        }
    }
}
