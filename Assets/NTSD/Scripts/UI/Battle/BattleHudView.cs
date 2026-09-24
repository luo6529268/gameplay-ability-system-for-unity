using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// NTSD_Battle 场景常驻的角色资源 HUD 绑定面。
    /// 对应当前场景的 HUD/HUDBg 区域，不管理角色列表，也不是 TEngine UIWindow。
    /// </summary>
    public sealed class BattleHudView : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private Image headImage;
        [SerializeField] private TMP_Text characterNameText;

        [Header("Resources")]
        [SerializeField] private Image hpImage;
        [SerializeField] private Image hpPreviewImage;
        [SerializeField] private Image mpImage;

        public void Apply(BattleHudState state)
        {
            if (state == null)
                return;

            gameObject.SetActive(state.IsVisible);
            if (!state.IsVisible)
                return;

            if (headImage != null)
                headImage.sprite = state.HeadSprite;
            if (characterNameText != null)
                characterNameText.text = state.DisplayName ?? string.Empty;
            if (hpImage != null)
                hpImage.fillAmount = Mathf.Clamp01(state.Hp01);
            if (hpPreviewImage != null)
                hpPreviewImage.fillAmount = Mathf.Clamp01(state.HpPreview01);
            if (mpImage != null)
                mpImage.fillAmount = Mathf.Clamp01(state.Mp01);
        }

        public void ClearVisualState()
        {
            gameObject.SetActive(false);
        }

        public void CollectMissingBindings(string path, List<string> missing)
        {
            if (missing == null)
                return;

            if (headImage == null)
                missing.Add(path + ".headImage");
            if (hpImage == null)
                missing.Add(path + ".hpImage");
            if (hpPreviewImage == null)
                missing.Add(path + ".hpPreviewImage");
            if (mpImage == null)
                missing.Add(path + ".mpImage");
        }
    }
}
