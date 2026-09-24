using System.Collections.Generic;
using UnityEngine;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// BattleControls 的动作按键表现绑定面。
    /// 这里只显示按下状态，不直接向 CharacterInputModule 写入输入。
    /// </summary>
    public sealed class BattleControlsView : MonoBehaviour
    {
        [Header("Action Images")]
        [SerializeField] private UnityEngine.UI.Image attackImage;
        [SerializeField] private UnityEngine.UI.Image jumpImage;
        [SerializeField] private UnityEngine.UI.Image defendImage;

        [Header("Optional Pressed Overlays")]
        [SerializeField] private GameObject attackPressedState;
        [SerializeField] private GameObject jumpPressedState;
        [SerializeField] private GameObject defendPressedState;

        [Header("Color Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.white;

        public void Apply(BattleControlsState state)
        {
            if (state == null || !state.IsVisible)
            {
                ClearVisualState();
                return;
            }

            gameObject.SetActive(true);
            ApplyAction(attackImage, attackPressedState, state.IsAttackHeld);
            ApplyAction(jumpImage, jumpPressedState, state.IsJumpHeld);
            ApplyAction(defendImage, defendPressedState, state.IsDefendHeld);
        }

        public void ClearVisualState()
        {
            gameObject.SetActive(false);
            ApplyAction(attackImage, attackPressedState, false);
            ApplyAction(jumpImage, jumpPressedState, false);
            ApplyAction(defendImage, defendPressedState, false);
        }

        public void CollectMissingBindings(string path, List<string> missing)
        {
            if (missing == null)
                return;

            if (attackImage == null)
                missing.Add(path + ".attackImage");
            if (jumpImage == null)
                missing.Add(path + ".jumpImage");
            if (defendImage == null)
                missing.Add(path + ".defendImage");
        }

        private void ApplyAction(
            UnityEngine.UI.Image image,
            GameObject pressedState,
            bool pressed)
        {
            if (image != null)
                image.color = pressed ? pressedColor : normalColor;
            if (pressedState != null && pressedState.activeSelf != pressed)
                pressedState.SetActive(pressed);
        }
    }
}
