using BeatEmUpTemplate2D;
using NTSD.UI.Menu;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.UI
{
    /// <summary>
    /// 战斗设置弹窗的选项枚举
    /// 对应UI中从上到下的6个选项
    /// </summary>
    public enum SettingBattleBgOption
    {
        /// <summary>开始战斗</summary>
        Fight = 0,
        /// <summary>重置所有选择</summary>
        ResetAll = 1,
        /// <summary>重置随机选项</summary>
        ResetRandom = 2,
        /// <summary>背景/地图选择</summary>
        Background = 3,
        /// <summary>难度设置 (0-4)</summary>
        Difficulty = 4,
        /// <summary>退出返回CMC</summary>
        Exit = 5,
    }

    /// <summary>
    /// 战斗设置弹窗控制器
    /// 
    /// 功能:
    /// 用于设置战斗参数（难度、背景）并启动战斗的弹窗界面
    /// 
    /// 流程位置:
    /// CharacterSelectionStep.SettingBattleBg 阶段显示
    /// 在CMC选择完成后出现
    /// 
    /// 交互规则:
    /// - 只有 Player1 可以控制此弹窗
    /// - 上下键切换选项
    /// - 左右键调整 Difficulty/Background 的值
    /// - Attack键确认当前选项的操作
    /// 
    /// 选项说明:
    /// - Difficulty: 左右调整难度值 (0-4)
    /// - Background: 左右调整地图ID
    /// - ResetRandom: 确认后触发 OnResetRandom 事件
    /// - ResetAll: 确认后触发 OnResetAll 事件，返回角色选择
    /// - Fight: 确认后触发 OnConfirmed 事件，开始战斗
    /// - Exit: 确认后触发 OnExit 事件，返回CMC弹窗
    /// </summary>
    public class SettingBattleBgController : MonoBehaviour
    {
        #region UI引用

        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;                  // 弹窗根节点
        [SerializeField] private List<TextMeshProUGUI> optionItems;     // 选项文本列表 (6个选项)
        [SerializeField] private MenuOptionList optionList;              // 选项列表

        #endregion

        #region 运行时状态

        [Header("Runtime")]
        [SerializeField] private int selectedIndex = 0;                 // 当前选中的选项索引

        [Header("Output")]
        [SerializeField] private int selectedMapId = 0;                 // 选择的地图ID
        [SerializeField] private int selectedDifficulty = 2;            // 选择的难度 (0-4, 默认2=中等)

        private bool inputBound;
        private bool isActive;

        #endregion

        #region 事件

        /// <summary>
        /// 确认开始战斗事件
        /// 参数1: 地图ID
        /// 参数2: 难度值
        /// </summary>
        public event Action<int, int> OnConfirmed;

        /// <summary>退出事件，返回CMC弹窗</summary>
        public event Action OnExit;

        /// <summary>重置随机选项事件</summary>
        public event Action OnResetRandom;

        /// <summary>重置所有选择事件，返回角色选择</summary>
        public event Action OnResetAll;

        #endregion

        #region 公开属性

        public int SelectedMapId => selectedMapId;
        public int SelectedDifficulty => selectedDifficulty;

        #endregion

        #region 公开方法

        /// <summary>
        /// 显示战斗设置弹窗
        /// </summary>
        public void Show()
        {
            selectedIndex = 0;

            if (rootPanel != null)
                rootPanel.SetActive(true);

            BindInput();
            isActive = true;
        }

        /// <summary>
        /// 隐藏战斗设置弹窗
        /// </summary>
        public void Hide()
        {
            UnbindInput();
            isActive = false;

            if (rootPanel != null)
                rootPanel.SetActive(false);
        }

        #endregion

        #region 生命周期

        private void OnDisable()
        {
            UnbindInput();
        }

        #endregion

        #region 输入绑定

        private void BindInput()
        {
            if (inputBound) return;
            
            if(optionList != null)
                optionList.OnOptionConfirmed += ConfirmCurrentOption;

            inputBound = true;
        }

        private void UnbindInput()
        {
            if (!inputBound) return;

            if (optionList != null)
                optionList.OnOptionConfirmed -= ConfirmCurrentOption;

            inputBound = false;
        }

        #endregion

        #region 输入处理


        /// <summary>
        /// 确认当前选项
        /// 根据选项类型触发不同的事件
        /// </summary>
        private void ConfirmCurrentOption(int selectedIndex)
        {
            SettingBattleBgOption option = (SettingBattleBgOption)selectedIndex;

            switch (option)
            {
                case SettingBattleBgOption.Fight:
                    // 开始战斗
                    OnConfirmed?.Invoke(selectedMapId, selectedDifficulty);
                    break;
                case SettingBattleBgOption.Exit:
                    // 返回CMC弹窗
                    OnExit?.Invoke();
                    break;
                case SettingBattleBgOption.ResetRandom:
                    // 重置随机选项
                    OnResetRandom?.Invoke();
                    break;
                case SettingBattleBgOption.ResetAll:
                    // 重置所有，返回角色选择
                    OnResetAll?.Invoke();
                    break;
                case SettingBattleBgOption.Difficulty:
                case SettingBattleBgOption.Background:
                    

                    break;
            }
        }

        #endregion

    }
}
