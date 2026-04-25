using MoreMountains.Tools;
using NTSD.App;
using NTSD.Tools;
using NTSD.UI;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeatEmUpTemplate2D
{

    /**
     * UIButton类 - 用于通过InputManager（手柄和键盘）导航UI按钮的类
     * 实现了ISelectHandler、IPointerDownHandler和ISubmitHandler接口
     * 提供按钮选择、点击和提交的功能
     */
    public class UIButton : MonoBehaviour, IPointerDownHandler
    {
        public bool SelectOnStart; // 是否在开始时自动选择此按钮

        [Header("选中时改变按钮文本")]
        public TextMeshProUGUI buttonText; // 按钮文本组件
        private Color buttonTextDefaultColor = Color.white; // 默认按钮文本颜色
        public Color buttonTextSelectedColor = Color.black; // 按钮选中时的文本颜色

        [Header("选中时显示/隐藏图像（可选）")]
        public Image imageTarget; // 目标图像组件

        [Header("按钮音效")]
        [SerializeField] private AudioClip sfxOnClickClip;

        public Action onClickCallback; // 按钮点击时的回调函数

        private RectTransform rectTransform; // 矩形变换组件
        private MMSoundManagerPlayOptions mMSoundManagerPlayOptions; // 音效播放选项

        void OnEnable()
        {
            mMSoundManagerPlayOptions = MMSoundManagerPlayOptions.Default;
            mMSoundManagerPlayOptions.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.UI;
        }

        void Start()
        {
            // 如果设置了在开始时选中，且按钮组件存在，则选中此按钮
            if (SelectOnStart && GetComponent<Button>() != null) GetComponent<Button>().Select();

            rectTransform = GetComponent<RectTransform>(); // 获取矩形变换组件

            // 保存默认文本颜色
            if (buttonText != null) buttonTextDefaultColor = buttonText.color;
        }

        void Update()
        {
            // 根据鼠标位置或EventSystem确定按钮是否被选中
            bool selected = IsMouseOverButton();

            // 设置按钮文本颜色
            if (buttonText != null) buttonText.color = selected ? buttonTextSelectedColor : buttonTextDefaultColor;

            // 显示/隐藏图像
            if (imageTarget != null) imageTarget.enabled = selected;
        }

        /// <summary>
        /// 检查鼠标是否在按钮范围内
        /// </summary>
        /// <returns>如果鼠标在按钮范围内返回true，否则返回false</returns>
        private bool IsMouseOverButton()
        {
            if (rectTransform == null || MenuUIController.Instance.menuUiCanvas == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, MenuUIController.Instance.menuUiCamera);
        }

        // 鼠标点击此按钮时调用
        public void OnPointerDown(PointerEventData eventData)
        {
            if (sfxOnClickClip != null)
                MMSoundManagerSoundPlayEvent.Trigger(sfxOnClickClip, mMSoundManagerPlayOptions);


            onClickCallback?.Invoke();
        }


    }
}
