using System;
using NTSD.UI.Menu;
using UnityEngine;

namespace NTSD.UI
{
    public enum SettingBattleBgOption
    {
        Fight = 0,
        ResetAll = 1,
        ResetRandom = 2,
        Background = 3,
        Difficulty = 4,
        Exit = 5,
    }

    public sealed class SettingBattleBgController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private MenuOptionList optionList;

        [Header("Output")]
        [SerializeField] private int selectedMapId;
        [SerializeField] private int selectedDifficulty = 2;

        private bool inputBound;

        public event Action<int, int> OnConfirmed;
        public event Action OnExit;
        public event Action OnResetRandom;
        public event Action OnResetAll;

        public int SelectedMapId => selectedMapId;
        public int SelectedDifficulty => selectedDifficulty;

        public void Show()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }

            BindInput();
        }

        public void Hide()
        {
            UnbindInput();

            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            UnbindInput();
        }

        private void BindInput()
        {
            if (inputBound)
            {
                return;
            }

            if (optionList != null)
            {
                optionList.OnOptionConfirmed += ConfirmCurrentOption;
            }

            inputBound = true;
        }

        private void UnbindInput()
        {
            if (!inputBound)
            {
                return;
            }

            if (optionList != null)
            {
                optionList.OnOptionConfirmed -= ConfirmCurrentOption;
            }

            inputBound = false;
        }

        private void ConfirmCurrentOption(int selectedIndex)
        {
            SettingBattleBgOption option = (SettingBattleBgOption)selectedIndex;

            switch (option)
            {
                case SettingBattleBgOption.Fight:
                    OnConfirmed?.Invoke(selectedMapId, selectedDifficulty);
                    break;

                case SettingBattleBgOption.Exit:
                    OnExit?.Invoke();
                    break;

                case SettingBattleBgOption.ResetRandom:
                    OnResetRandom?.Invoke();
                    break;

                case SettingBattleBgOption.ResetAll:
                    OnResetAll?.Invoke();
                    break;

                case SettingBattleBgOption.Difficulty:
                case SettingBattleBgOption.Background:
                    break;
            }
        }
    }
}
