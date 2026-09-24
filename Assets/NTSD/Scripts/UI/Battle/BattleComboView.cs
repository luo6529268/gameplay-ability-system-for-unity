using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// NTSD_Battle 场景常驻的 ComboPanel 绑定面。
    /// Combo 图形节点属于当前场景 Canvas，不通过 TEngine UIWindow 动态加载。
    /// </summary>
    public sealed class BattleComboView : MonoBehaviour
    {
        [Header("Combo Visuals")]
        [SerializeField] private Image comboBackgroundImage;
        [SerializeField] private Image comboContentImage;
        [SerializeField] private Image comboArrowImage;
        [SerializeField] private TMP_Text comboText;

        public void Apply(BattleComboState state)
        {
            if (state == null)
                return;

            if (!state.IsVisible)
            {
                ClearVisualState();
                return;
            }

            gameObject.SetActive(true);

            if (comboText != null)
            {
                comboText.text = string.IsNullOrEmpty(state.DisplayText)
                    ? state.HitCount.ToString()
                    : state.DisplayText;
            }
        }

        public void ClearVisualState()
        {
            gameObject.SetActive(false);

            if (comboText != null)
                comboText.text = string.Empty;
        }

        public void CollectMissingBindings(string path, List<string> missing)
        {
            if (missing == null)
                return;

            if (comboBackgroundImage == null)
                missing.Add(path + ".comboBackgroundImage");
            if (comboContentImage == null)
                missing.Add(path + ".comboContentImage");
            if (comboArrowImage == null)
                missing.Add(path + ".comboArrowImage");
        }
    }
}
