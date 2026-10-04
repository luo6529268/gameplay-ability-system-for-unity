using System;
using System.Collections;
using MoreMountains.Tools;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace NTSD.UI.Battle
{
    public sealed class BattleComboView : MonoBehaviour, MMEventListener<BattleComboInputChangedEvent>
    {
        [Serializable]
        private struct KeyIcon
        {
            public BattleInputHistoryKey Key;
            public Sprite Sprite;
        }

        [SerializeField] private RectTransform comboBackgroundImage;
        [SerializeField] private RectTransform comboContentImage;
        [SerializeField] private RectTransform comboArrowImage;
        [SerializeField] private KeyIcon[] keyIcons = Array.Empty<KeyIcon>();
        [SerializeField] private float defaultWidth = 406f;
        [SerializeField] private float horizontalPadding = 80f;
        [SerializeField] private float consumedDisplaySeconds = .5f;

        private static readonly BattleInputHistoryKey[] Keys =
        {
            BattleInputHistoryKey.Right, BattleInputHistoryKey.Left, BattleInputHistoryKey.Up,
            BattleInputHistoryKey.Down, BattleInputHistoryKey.Attack, BattleInputHistoryKey.Jump, BattleInputHistoryKey.Defend,
        };
        private readonly Sprite[] sprites = new Sprite[10];
        [SerializeField] private RectTransform[] iconSlots = Array.Empty<RectTransform>();
        [SerializeField] private RectTransform[] arrowSlots = Array.Empty<RectTransform>();
        private Image[] icons;
        private Image[] arrows;
        private bool initialized;
        private bool listening;
        private float contentY;
        private float arrowY;
        private Coroutine pendingClear;

        private void OnEnable()
        {
            InitializeVisuals();
            if (!listening)
            {
                this.MMEventStartListening<BattleComboInputChangedEvent>();
                listening = true;
            }
            ClearVisualState();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.TryGetCurrentBattleComboInput(out var value)) Apply(value);
        }

        private void OnDisable()
        {
            StopListening();
            ClearVisualState();
        }

        private void OnDestroy()
        {
            StopListening();
            CancelPendingClear();
            ClearVisualState();
        }

        private void StopListening()
        {
            if (!listening) return;
            this.MMEventStopListening<BattleComboInputChangedEvent>();
            listening = false;
        }

        private bool BuildIconMap()
        {
            Array.Clear(sprites, 0, sprites.Length);
            bool valid = true;
            int assigned = 0;
            foreach (KeyIcon entry in keyIcons ?? Array.Empty<KeyIcon>())
            {
                int key = (int)entry.Key;
                if (Array.IndexOf(Keys, entry.Key) < 0 || (assigned & (1 << key)) != 0)
                {
                    Debug.LogWarning("[BattleComboView] Invalid or duplicate icon key: " + entry.Key, this);
                    valid = false;
                    continue;
                }
                assigned |= 1 << key;
                sprites[key] = entry.Sprite;
            }
            foreach (BattleInputHistoryKey key in Keys)
            {
                if (sprites[(int)key] != null) continue;
                Debug.LogWarning("[BattleComboView] Missing icon: " + key, this);
                valid = false;
            }
            return valid;
        }

        private void InitializeVisuals()
        {
            if (initialized || comboContentImage == null || comboArrowImage == null) return;
            BuildIconMap();
            icons = CacheImages(iconSlots);
            arrows = CacheImages(arrowSlots);
            contentY = comboContentImage.anchoredPosition.y;
            arrowY = comboArrowImage.anchoredPosition.y;
            initialized = true;
            if (icons.Length < BattleComboInputChangedEvent.Capacity ||
                arrows.Length < BattleComboInputChangedEvent.Capacity - 1)
                Debug.LogWarning("[BattleComboView] Bind at least five icons and four arrows.", this);
        }

        private static Image[] CacheImages(RectTransform[] slots)
        {
            var images = new Image[slots?.Length ?? 0];
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = slots[i] != null ? slots[i].GetComponent<Image>() : null;
                if (images[i] != null) images[i].raycastTarget = false;
            }
            return images;
        }

        public void OnMMEvent(BattleComboInputChangedEvent value)
        {
            if (!isActiveAndEnabled) return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.IsCurrentBattleComboInputEvent(value)) Apply(value);
        }

        private void Apply(BattleComboInputChangedEvent value)
        {
            CancelPendingClear();
            if (!initialized) { ClearVisualState(); return; }
            int count = value.Binding.IsVisible ? value.Count : 0;
            double remaining = consumedDisplaySeconds - (Time.unscaledTimeAsDouble - value.PublishedAt);
            if (value.Kind == BattleComboInputChangeKind.Consumed && remaining <= 0) count = 0;
            Render(value, count);
            if (count > 0 && value.Kind == BattleComboInputChangeKind.Consumed)
                pendingClear = StartCoroutine(ClearAfter((float)remaining));
        }

        private IEnumerator ClearAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            pendingClear = null;
            ClearVisualState();
        }

        private void CancelPendingClear()
        {
            if (pendingClear == null) return;
            StopCoroutine(pendingClear);
            pendingClear = null;
        }

        private void Render(BattleComboInputChangedEvent value, int count)
        {
            count = Mathf.Min(count, BattleComboInputChangedEvent.Capacity, icons.Length);
            float iconWidth = comboContentImage.rect.width;
            float step = iconWidth + comboArrowImage.rect.width;
            float start = -(Mathf.Max(0, count - 1) * step) * .5f;
            for (int i = 0; i < icons.Length; i++)
            {
                Image icon = icons[i];
                if (icon == null) continue;
                int key = i < count ? (int)value.GetKey(i) : -1;
                Sprite sprite = key >= 0 && key < sprites.Length ? sprites[key] : null;
                icon.overrideSprite = null;
                icon.sprite = sprite;
                icon.enabled = true;
                icon.gameObject.SetActive(sprite != null);
                if (sprite != null)
                    icon.rectTransform.anchoredPosition = new Vector2(start + i * step, contentY);
            }
            for (int i = 0; i < arrows.Length; i++)
            {
                Image arrow = arrows[i];
                if (arrow == null) continue;
                bool visible = i + 1 < count && icons[i] != null && icons[i + 1] != null &&
                    icons[i].gameObject.activeSelf && icons[i + 1].gameObject.activeSelf;
                arrow.enabled = true;
                arrow.gameObject.SetActive(visible);
                if (visible)
                    arrow.rectTransform.anchoredPosition = new Vector2(start + (i + .5f) * step, arrowY);
            }
            float singleKeyWidth = Mathf.Max(defaultWidth, iconWidth + 2f * horizontalPadding);
            SetBackgroundWidth(count > 0 ? singleKeyWidth + (count - 1) * step : defaultWidth);
        }

        private void SetBackgroundWidth(float width)
        {
            if (comboBackgroundImage == null) return;
            comboBackgroundImage.gameObject.SetActive(true);
            Image background = comboBackgroundImage.GetComponent<Image>();
            if (background != null) background.enabled = true;
            comboBackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void ClearVisualState()
        {
            CancelPendingClear();
            if (icons != null)
            {
                foreach (Image icon in icons)
                {
                    if (icon == null) continue;
                    icon.gameObject.SetActive(false);
                    icon.overrideSprite = null;
                    icon.sprite = null;
                }
            }
            if (arrows != null)
            {
                foreach (Image arrow in arrows)
                    if (arrow != null) arrow.gameObject.SetActive(false);
            }
            SetBackgroundWidth(defaultWidth);
        }
    }
}
