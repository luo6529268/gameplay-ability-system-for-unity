using NTSD.UI.Menu;
using UnityEngine;

namespace NTSD.UI
{
    public class SelectGameModeController : MonoBehaviour
    {
        [SerializeField] private MenuOptionList optionList;

        private void OnEnable()
        {
            if (optionList != null) 
            {
                optionList.OnOptionConfirmed += HandleOptionConfirmed;
                optionList.OnFocusEnter();
            }
        }

        private void OnDisable()
        {
            if (optionList != null)
                optionList.OnOptionConfirmed -= HandleOptionConfirmed;
        }

        private void HandleOptionConfirmed(int index)
        {
            switch (index)
            {
                case 0: OnVSMode(); break;
                case 1: OnStageMode(); break;
                case 2: OnBattleMode(); break;
                case 3: OnTrainingMode(); break;
                case 4: OnOptions(); break;
                case 5: OnExit(); break;
            }
        }

        private void OnVSMode() 
        {
            MenuUIController.Instance.ShowSelectCharacter();
            this.gameObject.SetActive(false);
        
        }

        private void OnStageMode() { /* 加载关卡模式 */ }
        private void OnBattleMode() { /* 加载乱斗模式 */ }
        private void OnTrainingMode() { /* 加载训练模式 */ }
        private void OnOptions() { /* 打开设置 */ }
        private void OnExit() { Application.Quit(); }
    }
}