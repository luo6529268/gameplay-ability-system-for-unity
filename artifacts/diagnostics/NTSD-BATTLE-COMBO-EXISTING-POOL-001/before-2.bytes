using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// NTSD_Battle 场景常驻的角色资源 HUD 绑定面。
    /// 对应当前场景的 HUD/HUDBg 区域，不管理角色列表，也不是 TEngine UIWindow。
    /// </summary>
    public sealed class BattleHudView : MonoBehaviour, MMEventListener<BattleHudChangedEvent>
    {
        [Header("Character")]
        [SerializeField] private Image headImage;
        [SerializeField] private TMP_Text characterNameText;

        [Header("Resources")]
        [SerializeField] private Image hpImage;
        [SerializeField] private Image hpPreviewImage;
        [SerializeField] private Image mpImage;

        private void OnEnable()
        {
            this.MMEventStartListening<BattleHudChangedEvent>();
            ClearVisualState();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.TryGetCurrentBattleHud(out BattleHudChangedEvent hudEvent))
                ApplyEvent(hudEvent, true);
        }

        private void OnDisable()
        {
            this.MMEventStopListening<BattleHudChangedEvent>();
            ClearVisualState();
        }

        private void OnDestroy()
        {
            this.MMEventStopListening<BattleHudChangedEvent>();
        }

        public void OnMMEvent(BattleHudChangedEvent hudEvent)
        {
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.IsCurrentBattleHudEvent(hudEvent))
                ApplyEvent(hudEvent, false);
        }

        private void ApplyEvent(BattleHudChangedEvent hudEvent, bool forceFull)
        {
            BattleHudValues values = hudEvent.Values;
            if (!values.IsVisible)
            {
                ClearVisualState();
                return;
            }
            BattleHudChanges changes = forceFull ? BattleHudChanges.All : values.Changes;
            if ((changes & BattleHudChanges.Binding) != 0)
            {
                if (characterNameText != null)
                    characterNameText.text = hudEvent.DisplayName;
                if (headImage != null)
                {
                    headImage.sprite = hudEvent.HeadSprite;
                    headImage.enabled = hudEvent.HeadSprite != null;
                }
            }
            if ((changes & (BattleHudChanges.Hp | BattleHudChanges.HpMax)) != 0)
                SetFill(hpImage, Ratio(values.Hp, values.HpMax));
            if ((changes & (BattleHudChanges.HpBound | BattleHudChanges.HpMax)) != 0)
                SetFill(hpPreviewImage, Ratio(values.HpBound, values.HpMax));
            if ((changes & (BattleHudChanges.Mp | BattleHudChanges.MpMax)) != 0)
                SetFill(mpImage, Ratio(values.Mp, values.MpMax));
        }

        private float Ratio(int value, int maximum)
        {
            return maximum > 0 ? (float)value / maximum : 0f;
        }

        public void ClearVisualState()
        {
            // Keep this component active so a later character publication can restore the HUD.
            if (headImage != null)
            {
                headImage.sprite = null;
                headImage.enabled = false;
            }
            if (characterNameText != null)
                characterNameText.text = string.Empty;
            SetFill(hpImage, 0f);
            SetFill(hpPreviewImage, 0f);
            SetFill(mpImage, 0f);
        }

        private void SetFill(Image image, float value)
        {
            if (image != null)
                image.fillAmount = float.IsNaN(value) ? 0f : Mathf.Clamp01(value);
        }

    }
}
