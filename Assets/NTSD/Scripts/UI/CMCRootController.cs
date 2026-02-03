using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using BeatEmUpTemplate2D;
using NTSD.UI.Menu;

namespace NTSD.UI
{
    /// <summary>
    /// CMC (Choose Machine Count) 弹窗控制器
    /// 
    /// 功能:
    /// 用于选择电脑玩家数量的弹窗界面
    /// 
    /// 流程位置:
    /// CharacterSelectionStep.ComputerCount 阶段显示
    /// 在所有玩家确认并倒计时结束后出现
    /// 
    /// 交互规则:
    /// - 只有 Player1 可以控制此弹窗
    /// - 左右键切换选择的电脑数量
    /// - 已被真人玩家占用的槽位对应的选项会被禁用
    /// - Attack键确认选择
    /// 
    /// 禁用逻辑:
    /// 如果有N个真人玩家加入，则后N个选项被禁用
    /// 例如: 2个玩家加入，则只能选择0-6个电脑（7和8被禁用）
    /// </summary>
    public class CMCRootController : MonoBehaviour
    {
        #region UI引用

        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;                      // 弹窗根节点
        [SerializeField] private List<TextMeshProUGUI> selectComputerItems; // 选项文本列表 (0-8个电脑)
        [SerializeField] private MenuOptionList optionList;              // 选项列表
        #endregion

        #region 视觉设置

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;       // 正常选项颜色
        [SerializeField] private Color disabledColor = Color.gray;      // 禁用选项颜色

        #endregion

        #region 输入设置

        [Header("Input Settings")]
        [SerializeField] private int controlPlayerId = 1;               // 控制此弹窗的玩家ID（默认Player1）
        [SerializeField] private float navigationCooldown = 0.15f;      // 导航冷却时间

        #endregion

        #region 运行时状态

        [Header("Runtime")]
        [SerializeField] private int disabledCount = 0;                 // 禁用的选项数量（等于已加入玩家数）

        private bool inputBound;
        private bool isActive;

        #endregion

        #region 事件

        /// <summary>
        /// 确认选择事件
        /// 参数: 选择的电脑玩家数量
        /// </summary>
        public event Action<int> OnConfirmed;

        #endregion

        #region 公开方法

        /// <summary>
        /// 显示CMC弹窗
        /// </summary>
        /// <param name="joinedPlayerCount">已加入的真人玩家数量，用于禁用对应数量的选项</param>
        public void Show(int joinedPlayerCount)
        {
            disabledCount = joinedPlayerCount;

            if (rootPanel != null)
                rootPanel.SetActive(true);

            UpdateDisplay();
            BindInput();
            isActive = true;
        }

        /// <summary>
        /// 隐藏CMC弹窗
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
                optionList.OnOptionConfirmed += OnOptionConfirmed;

            inputBound = true;
        }

        private void UnbindInput()
        {
            if (!inputBound) return;

            if (optionList != null)
                optionList.OnOptionConfirmed -= OnOptionConfirmed;

            inputBound = false;
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理攻击输入（确认选择）
        /// </summary>
        private void OnOptionConfirmed(int optionIndex)
        {
            if (!isActive) return;

            OnConfirmed?.Invoke(optionIndex);
        }

        #endregion

        #region 显示更新

        /// <summary>
        /// 获取最大可选索引
        /// 总选项数 - 已加入玩家数 - 1
        /// </summary>
        private int GetMaxSelectableIndex()
        {
            int maxIndex = selectComputerItems.Count - 1 - disabledCount;
            return Mathf.Max(0, maxIndex);
        }

        /// <summary>
        /// 更新所有选项的显示状态
        /// - 超过最大可选索引的选项显示为禁用色
        /// - 当前选中的选项显示为选中色
        /// - 其他可选选项显示为正常色
        /// </summary>
        private void UpdateDisplay()
        {
            int maxSelectableIndex = GetMaxSelectableIndex();

            for (int i = 0; i < selectComputerItems.Count; i++)
            {
                if (selectComputerItems[i] == null) continue;

                if (i > maxSelectableIndex)
                {
                    selectComputerItems[i].color = disabledColor;
                }
                else
                {
                    selectComputerItems[i].color = normalColor;
                }
            }
        }

        #endregion
    }
}
