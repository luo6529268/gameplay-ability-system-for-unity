using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NTSD.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NTSDButton))]
    [AddComponentMenu("NTSD/UI/NTSD Button Ripple")]
    public sealed class NTSDButtonRipple : MonoBehaviour, ISubmitHandler
    {
        [SerializeField] private UnityEngine.UI.Image ringImage;
        [SerializeField] private Color ringColor = new Color(0.1f, 0.85f, 1f, 0.85f);
        [SerializeField] private Vector2 startSize = new Vector2(145f, 145f);
        [SerializeField] private Vector2 endSize = new Vector2(230f, 230f);
        [SerializeField, Min(0.01f)] private float duration = 0.45f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private NTSDButton button;
        [SerializeField, Range(1, 16)] private int initialPoolSize = 4;

        private sealed class RingSlot
        {
            internal UnityEngine.UI.Image Image;
            internal Tween Tween;
            internal float Progress;
            internal bool Active;
            internal bool Owned;
            internal Vector2 StartSize;
            internal Vector2 EndSize;
            internal Color Color;
            internal AnimationCurve Fade;
        }

        private readonly List<RingSlot> rings = new List<RingSlot>();
        private bool hasFocus = true;
        private bool paused;

        private void Awake()
        {
            button = GetComponent<NTSDButton>();
            ringImage = this.transform.GetChild(0).GetComponent<Image>();
            if (ringImage == null)
                return;

            AddSlot(ringImage, false);
            for (int index = 1; index < Mathf.Clamp(initialPoolSize, 1, 16); index++)
                GrowPool();
        }

        private RingSlot AddSlot(UnityEngine.UI.Image image, bool owned)
        {
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            image.enabled = false;
            var slot = new RingSlot { Image = image, Owned = owned };
            rings.Add(slot);
            return slot;
        }

        private RingSlot GrowPool()
        {
            var child = new GameObject("RippleRing " + rings.Count,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            child.transform.SetParent(ringImage.transform.parent, false);
            var image = child.GetComponent<UnityEngine.UI.Image>();
            image.sprite = ringImage.sprite;
            image.material = ringImage.material;
            image.maskable = ringImage.maskable;
            image.preserveAspect = ringImage.preserveAspect;
            return AddSlot(image, true);
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
            foreach (RingSlot slot in rings)
            {
                slot.Tween?.Kill(false);
                if (slot.Owned && slot.Image != null)
                    Destroy(slot.Image.gameObject);
            }
            rings.Clear();
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

            RingSlot available = null;
            foreach (RingSlot slot in rings)
            {
                if (!slot.Active && slot.Image != null)
                {
                    available = slot;
                    break;
                }
            }

            // Grow at peak demand; a new press never steals an in-flight ring.
            RingSlot ring = available ?? GrowPool();
            if (ring.Tween == null || !ring.Tween.IsActive())
            {
                ring.Tween = DOTween.To(() => ring.Progress, value => ring.Progress = value, 1f, 1f)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .SetAutoKill(false)
                    .OnUpdate(() => ApplyProgress(ring))
                    .OnComplete(() => Recycle(ring))
                    .Pause();
            }

            ring.Tween.Rewind();
            ring.Tween.timeScale = 1f / Mathf.Max(0.01f, duration);
            ring.Progress = 0f;
            ring.StartSize = startSize;
            ring.EndSize = endSize;
            ring.Color = ringColor;
            ring.Fade = fadeCurve;
            ring.Active = true;
            ring.Image.enabled = true;
            ApplyProgress(ring);
            ring.Tween.Restart();
        }

        private void ApplyProgress(RingSlot ring)
        {
            if (!ring.Active)
                return;
            if (ring.Image == null || button == null || !button.isActiveAndEnabled ||
                !button.IsInteractable())
            {
                StopRipple();
                return;
            }
            ring.Image.rectTransform.sizeDelta = Vector2.Lerp(ring.StartSize, ring.EndSize, ring.Progress);
            Color color = ring.Color;
            color.a *= Mathf.Clamp01(ring.Fade == null ? 1f - ring.Progress : ring.Fade.Evaluate(ring.Progress));
            ring.Image.color = color;
        }

        private static void Recycle(RingSlot ring)
        {
            ring.Active = false;
            if (ring.Image != null)
                ring.Image.enabled = false;
        }

        private void StopRipple()
        {
            foreach (RingSlot ring in rings)
            {
                ring.Tween?.Pause();
                Recycle(ring);
            }
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
