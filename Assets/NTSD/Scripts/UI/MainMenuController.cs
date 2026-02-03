using BeatEmUpTemplate2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public UIButton _GameStartButton;
        public UIButton _NetworkButton;
        public UIButton _SettingsButton;

        public GameObject _SettingsPanelObj;
        private void Awake()
        {
            _GameStartButton.onClickCallback = OnGameStartCallBack;
            _NetworkButton  .onClickCallback = OnNetworkOnlineCallBack;
            _SettingsButton .onClickCallback = OnControllerSettingsCallBack;
        }

        private void OnControllerSettingsCallBack()
        {
            if (_SettingsPanelObj == null)
                return;

            _SettingsPanelObj.gameObject.SetActive(true);
        }

        private void OnNetworkOnlineCallBack()
        {

        }

        private void OnGameStartCallBack()
        {
            MenuUIController.Instance.ShowLoading();
        }
    }
}