using System;
using System.Collections;
using MoreMountains.Tools;
using NTSD.Simulation;
using UnityEngine;

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

        [SerializeField] private UnityEngine.UI.Image comboBackgroundImage;
        [SerializeField] private UnityEngine.UI.Image comboContentImage;
        [SerializeField] private UnityEngine.UI.Image comboArrowImage;
        [SerializeField] private KeyIcon[] keyIcons = Array.Empty<KeyIcon>();
        [SerializeField, Min(1f)] private float defaultWidth = 406f;
        [SerializeField, Min(0f)] private float horizontalPadding = 80f;
        [SerializeField, Min(0f)] private float iconGap = 32f;
        [SerializeField, Min(0f)] private float consumedDisplaySeconds = .5f;

        private static readonly BattleInputHistoryKey[] Keys =
        {
            BattleInputHistoryKey.Right, BattleInputHistoryKey.Left, BattleInputHistoryKey.Up,
            BattleInputHistoryKey.Down, BattleInputHistoryKey.Attack, BattleInputHistoryKey.Jump, BattleInputHistoryKey.Defend,
        };
        private readonly Sprite[] sprites = new Sprite[10];
        private readonly UnityEngine.UI.Image[] icons = new UnityEngine.UI.Image[BattleComboInputChangedEvent.Capacity];
        private readonly UnityEngine.UI.Image[] arrows = new UnityEngine.UI.Image[BattleComboInputChangedEvent.Capacity - 1];
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
            for (int i = 1; i < icons.Length; i++)
                if (icons[i] != null) Destroy(icons[i].gameObject);
            for (int i = 1; i < arrows.Length; i++)
                if (arrows[i] != null) Destroy(arrows[i].gameObject);
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
            initialized = true;
            BuildIconMap();
            Transform parent = comboContentImage.transform.parent;
            comboArrowImage.transform.SetParent(parent, true);
            contentY = comboContentImage.rectTransform.anchoredPosition.y;
            arrowY = comboArrowImage.rectTransform.anchoredPosition.y;
            icons[0] = comboContentImage;
            arrows[0] = comboArrowImage;
            for (int i = 1; i < icons.Length; i++) icons[i] = Instantiate(comboContentImage, parent);
            for (int i = 1; i < arrows.Length; i++) arrows[i] = Instantiate(comboArrowImage, parent);
            foreach (var icon in icons) { icon.raycastTarget = false; icon.enabled = true; }
            foreach (var arrow in arrows) { arrow.raycastTarget = false; arrow.enabled = true; }
            if (comboBackgroundImage != null) comboBackgroundImage.raycastTarget = false;
        }

        public void OnMMEvent(BattleComboInputChangedEvent value)
        {
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
            float iconWidth = comboContentImage.rectTransform.rect.width;
            float step = iconWidth + comboArrowImage.rectTransform.rect.width + iconGap;
            float rowWidth = count > 0 ? iconWidth + (count - 1) * step : 0f;
            float start = -(rowWidth - iconWidth) * .5f;
            for (int i = 0; i < icons.Length; i++)
            {
                int key = i < count ? (int)value.GetKey(i) : -1;
                Sprite sprite = key >= 0 && key < sprites.Length ? sprites[key] : null;
                icons[i].sprite = sprite;
                icons[i].gameObject.SetActive(sprite != null);
                icons[i].rectTransform.anchoredPosition = new Vector2(start + i * step, contentY);
            }
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].gameObject.SetActive(i + 1 < count && icons[i].gameObject.activeSelf && icons[i + 1].gameObject.activeSelf);
                arrows[i].rectTransform.anchoredPosition = new Vector2(start + (i + .5f) * step, arrowY);
            }
            SetBackgroundWidth(count > 0 ? Mathf.Max(defaultWidth, rowWidth + 2f * horizontalPadding) : defaultWidth);
        }

        private void SetBackgroundWidth(float width)
        {
            if (comboBackgroundImage == null) return;
            comboBackgroundImage.enabled = true;
            comboBackgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void ClearVisualState()
        {
            CancelPendingClear();
            if (comboContentImage != null) comboContentImage.gameObject.SetActive(false);
            if (comboArrowImage != null) comboArrowImage.gameObject.SetActive(false);
            foreach (var icon in icons) if (icon != null) icon.gameObject.SetActive(false);
            foreach (var arrow in arrows) if (arrow != null) arrow.gameObject.SetActive(false);
            SetBackgroundWidth(defaultWidth);
        }
    }
}
