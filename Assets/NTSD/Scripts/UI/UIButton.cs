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
        [Header("选中时改变按钮文本")]
        public TextMeshProUGUI buttonText; // 按钮文本组件
        private Color buttonTextDefaultColor = Color.white; // 默认按钮文本颜色
        public Color buttonTextSelectedColor = Color.black; // 按钮选中时的文本颜色

        [Header("按钮音效")]
        [SerializeField] private AudioClip sfxOnClickClip;
        [SerializeField] private int defalutFontSize = 50;
        [SerializeField] private int selectFontSize = 80;


        public Action onClickCallback; // 按钮点击时的回调函数

        private RectTransform rectTransform; // 矩形变换组件
        private MMSoundManagerPlayOptions mMSoundManagerPlayOptions; // 音效播放选项

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>(); // 获取矩形变换组件

            // 保存默认文本颜色
            if (buttonText != null) buttonTextDefaultColor = buttonText.color;
        }

        void OnEnable()
        {
            mMSoundManagerPlayOptions = MMSoundManagerPlayOptions.Default;
            mMSoundManagerPlayOptions.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.UI;
        }

        void Update()
        {
            // 根据鼠标位置或EventSystem确定按钮是否被选中
            bool selected = IsMouseOverButton();

            // 设置按钮文本颜色
            if (buttonText != null) buttonText.color = selected ? buttonTextSelectedColor : buttonTextDefaultColor;

            if (buttonText != null) buttonText.fontSize = selected ? Mathf.Lerp(buttonText.fontSize, selectFontSize,0.2f) : Mathf.Lerp(buttonText.fontSize, defalutFontSize, 0.2f);
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
