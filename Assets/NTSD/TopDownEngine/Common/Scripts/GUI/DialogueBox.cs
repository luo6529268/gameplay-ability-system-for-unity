using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using TMPro;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Dialogue box class. Don't add this directly to your game, look at DialogueZone instead.
    /// 对话框类。不要直接将其添加到游戏中，请查看 DialogueZone 代替。
    /// </summary>
    public class DialogueBox : TopDownMonoBehaviour
	{
		[Header("Dialogue Box")]
        /// 文本面板背景
        [Tooltip("文本面板背景")]
        public CanvasGroup TextPanelCanvasGroup;
        /// 要显示的文本
        [Tooltip("要显示的文本")]
        public TextMeshProUGUI DialogueText;
        /// 按钮A提示
        [Tooltip("按钮A提示")]
        public CanvasGroup Prompt;
        /// 要着色化的图片列表
        [Tooltip("要着色化的图片列表")]
        public List<Image> ColorImages;


        protected Color _backgroundColor;
		protected Color _textColor;

		/// <summary>
		/// Changes the text.
		/// </summary>
		/// <param name="newText">New text.</param>
		public virtual void ChangeText(string newText)
		{
			DialogueText.text = newText;
		}

		/// <summary>
		/// Activates the ButtonA prompt
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		public virtual void ButtonActive(bool state)
		{
			Prompt.gameObject.SetActive(state);
		}

		/// <summary>
		/// Changes the color of the dialogue box to the ones in parameters
		/// </summary>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="textColor">Text color.</param>
		public virtual void ChangeColor(Color backgroundColor, Color textColor)
		{
			_backgroundColor = backgroundColor;
			_textColor = textColor;

			foreach(Image image in ColorImages)
			{
				image.color = _backgroundColor;
			}
			DialogueText.color = _textColor;
		}

		/// <summary>
		/// Fades the dialogue box in.
		/// </summary>
		/// <param name="duration">Duration.</param>
		public virtual void FadeIn(float duration)
		{
			if (TextPanelCanvasGroup != null)
			{
				StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 1f));
			}
			if (DialogueText != null)
			{
				StartCoroutine(MMFade.FadeText(DialogueText, duration, _textColor));
			}
			if (Prompt != null)
			{
				StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 1f));
			}
		}

		/// <summary>
		/// Fades the dialogue box out.
		/// </summary>
		/// <param name="duration">Duration.</param>
		public virtual void FadeOut(float duration)
		{
			Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
			Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

			StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 0f));
			StartCoroutine(MMFade.FadeText(DialogueText, duration, newTextColor));
			StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 0f));
		}
	}
}