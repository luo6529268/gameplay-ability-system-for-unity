using NTSD.Tools;
using UnityEngine;

namespace NTSD.UI
{
    /// <summary>
    /// Menu UI root controller.
    /// - Only handles CanvasGroup show/hide and input blocking.
    /// - Does not contain business logic.
    /// </summary>
    public sealed class MenuUIController : SingletonBehaviour<MenuUIController>
    {
        [Header("Menu UI Camera")]
        public Camera menuUiCamera;
        public Canvas menuUiCanvas;

        [Header("Views (CanvasGroup)")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject loading;
        [SerializeField] private GameObject selectGameMode;
        [SerializeField] private GameObject selectCharacter;

        public void EnableMenuUiCamera(bool enabled)
        {
            if (menuUiCamera != null)
            {
                menuUiCamera.enabled = enabled;
            }
        }

        public void HideAll()
        {
            mainMenu?.SetActive(false);
            loading?.SetActive(false);
            selectGameMode?.SetActive(false);
            selectCharacter?.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAll();
            mainMenu?.SetActive(true);
        }

        /// <summary>
        /// Show loading overlay and block input.
        /// </summary>
        public void ShowLoading(bool blockInput = true)
        {
            HideAll();
            loading?.SetActive(blockInput);
        }

        public void ShowSelectGameMode()
        {
            HideAll();
            selectGameMode?.SetActive(true);
        }

        public void ShowSelectCharacter()
        {
            HideAll();
            selectCharacter?.SetActive(true);
        }
    }
}
