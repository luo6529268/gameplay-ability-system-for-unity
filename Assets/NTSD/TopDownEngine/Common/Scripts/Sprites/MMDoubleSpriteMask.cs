using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This setup uses two sprite masks, bound in the inspector, to enable one and then disable the other to mask specific parts of a level
    /// 该设置使用两个在检查器中绑定的精灵遮罩，启用一个然后禁用另一个，以遮罩关卡的特定部分
    /// </summary>
    public class MMDoubleSpriteMask : MonoBehaviour, MMEventListener<MMSpriteMaskEvent>
	{
		[Header("Masks")]

        /// 第一个精灵遮罩
        [Tooltip("第一个精灵遮罩")]
        public MMSpriteMask Mask1;
        /// 第二个精灵遮罩
        [Tooltip("第二个精灵遮罩")]
        public MMSpriteMask Mask2;

        protected MMSpriteMask _currentMask;
		protected MMSpriteMask _dormantMask;

		/// <summary>
		/// On awake we initialize our masks
		/// </summary>
		protected virtual void Awake()
		{
			Mask1.gameObject.SetActive(true);
			Mask2.gameObject.SetActive(false);
			_currentMask = Mask1;
			_dormantMask = Mask2;
		}

        /// <summary>
        /// Sets new values for current and dormant masks
        ///  设置当前遮罩和休眠遮罩的新值
        /// </summary>
        protected virtual void SwitchCurrentMask()
		{
			_currentMask = (_currentMask == Mask1) ? Mask2 : Mask1;
			_dormantMask = (_currentMask == Mask1) ? Mask2 : Mask1;
		}

        /// <summary>
        /// A coroutine designed to mask the first mask after having activated and moved the dormant one to the new position
        /// 一个协程，旨在在激活并移动休眠遮罩到新位置后遮罩第一个遮罩
        /// </summary>
        /// <param name="spriteMaskEvent"></param>
        /// <returns></returns>
        protected virtual IEnumerator DoubleMaskCo(MMSpriteMaskEvent spriteMaskEvent)
		{
			_dormantMask.transform.position = spriteMaskEvent.NewPosition;
			_dormantMask.transform.localScale = spriteMaskEvent.NewSize * _dormantMask.ScaleMultiplier;
			_dormantMask.gameObject.SetActive(true);
			yield return new WaitForSeconds(spriteMaskEvent.Duration);
			_currentMask.gameObject.SetActive(false);
			SwitchCurrentMask();
		}

        /// <summary>
        /// When we catch a double mask event, we handle it
        /// 当我们捕获到双遮罩事件时，我们处理它
        /// </summary>
        /// <param name="spriteMaskEvent"></param>
        public virtual void OnMMEvent(MMSpriteMaskEvent spriteMaskEvent)
		{
			switch (spriteMaskEvent.EventType)
			{
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.DoubleMask:
					StartCoroutine(DoubleMaskCo(spriteMaskEvent));
					break;
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMSpriteMaskEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMSpriteMaskEvent>();
		}
	}
}