using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace NTSD.UI
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        public Button OkBtn;
        public Button CancelBtn;

        public List<SettingsItem> settingsItems;

        private void Awake()
        {
            OkBtn.onClick.AddListener(OnOkClicked);
            CancelBtn.onClick.AddListener(OnCancelClicked);
        }

        private void OnCancelClicked()
        {
            foreach (var item in settingsItems)
            {
                item.CancelChanges();
            }

            this.gameObject.SetActive(false);
        }

        private void OnOkClicked()
        {
            foreach (var item in settingsItems)
            {
                item.ApplyChanges(); 
            }

            this.gameObject.SetActive(false);
        }

    }
}