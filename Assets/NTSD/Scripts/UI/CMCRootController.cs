using System;
using NTSD.UI.Menu;
using UnityEngine;

namespace NTSD.UI
{
    public sealed class CMCRootController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private MenuOptionList optionList;

        [Header("Runtime")]
        [SerializeField] private int joinedPlayerCount;
        [SerializeField] private int selectedComputerCount;

        private bool inputBound;

        public event Action<int> OnConfirmed;

        public int JoinedPlayerCount => joinedPlayerCount;
        public int SelectedComputerCount => selectedComputerCount;

        public void Show(int joinedCount)
        {
            joinedPlayerCount = Mathf.Max(0, joinedCount);
            selectedComputerCount = 0;

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
            int maxComputerCount = Mathf.Max(0, 8 - joinedPlayerCount);
            selectedComputerCount = Mathf.Clamp(selectedIndex, 0, maxComputerCount);
            OnConfirmed?.Invoke(selectedComputerCount);
        }
    }
}
