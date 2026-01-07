using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    /// <summary>
    /// An event type used to set a new size for the mask from any class
    /// 一个事件类型，用于从任何类设置遮罩的新大小
    /// </summary>
    public struct MMSpriteMaskEvent
	{
		public enum MMSpriteMaskEventTypes { MoveToNewPosition, ExpandAndMoveToNewPosition, DoubleMask }

		public MMSpriteMaskEventTypes EventType;
		public Vector2 NewPosition;
		public Vector2 NewSize;
		public float Duration;
		public MMTween.MMTweenCurve Curve;

		public MMSpriteMaskEvent(MMSpriteMaskEventTypes eventType, Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			EventType = eventType;
			NewPosition = newPosition;
			NewSize = newSize;
			Duration = duration;
			Curve = curve;
		}

		static MMSpriteMaskEvent e;
		public static void Trigger(MMSpriteMaskEventTypes eventType, Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			e.EventType = eventType;
			e.NewPosition = newPosition;
			e.NewSize = newSize;
			e.Duration = duration;
			e.Curve = curve;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// This class will automatically look for sprite renderers, particle systems, tilemaps in the scene, and change their SpriteMaskInteraction settings according to the one set in the inspector
    /// Use the NoMask tag on objects you don't want automatically setup
    ///  这个类会自动寻找场景中的精灵渲染器、粒子系统和瓦片图，并根据检查器中设置的SpriteMaskInteraction进行更改
    /// 在你不希望自动设置的对象上使用NoMask标签
    /// </summary>
    public class MMSpriteMask : MonoBehaviour, MMEventListener<MMSpriteMaskEvent>
	{
        /// the possible timescales this mask can operate on
        //遮罩可能操作的时间尺度
        public enum Timescales { Scaled, Unscaled }

		[Header("Scale")]
        /// 应用到精灵遮罩的缩放倍数
        [Tooltip("应用到精灵遮罩的缩放倍数")]
        public float ScaleMultiplier = 100f;

        [Header("Auto setup")]
        /// 是否应该转换所有精灵渲染器
        [Tooltip("是否应该转换所有精灵渲染器")]
        public bool AutomaticallySetupSpriteRenderers = true;
        /// 是否应该转换所有粒子系统
        [Tooltip("是否应该转换所有粒子系统")]
        public bool AutomaticallySetupParticleSystems = true;
        /// 是否应该转换所有瓦片图
        [Tooltip("是否应该转换所有瓦片图")]
        public bool AutomaticallySetupTilemaps = true;

        [Header("Behaviour")]

        /// 如果为真，当捕获到精灵遮罩事件时，此遮罩将移动
        [Tooltip("如果为真，当捕获到精灵遮罩事件时，此遮罩将移动")]
        public bool CatchEvents = true;
        /// 此遮罩操作的时间尺度
        [Tooltip("此遮罩操作的时间尺度")]
        public Timescales Timescale = Timescales.Unscaled;
        /// 要应用于所有渲染器的交互类型
        [Tooltip("要应用于所有渲染器的交互类型")]
        public SpriteMaskInteraction MaskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        public virtual float MaskTime { get { float time = (Timescale == Timescales.Unscaled) ? Time.unscaledTime : Time.time; return time; } }

		/// <summary>
		/// On Awake we setup our objects
		/// </summary>
		protected virtual void Start()
		{
			SetupMaskSettingsAutomatically();
		}

        /// <summary>
        /// Looks for mask settings and updates them
        /// 查找遮罩设置并更新它们
        /// </summary>
        protected virtual void SetupMaskSettingsAutomatically()
		{
			if (AutomaticallySetupSpriteRenderers)
			{
				var foundSpriteRenderers = FindObjectsOfType<SpriteRenderer>();
				if (foundSpriteRenderers.Length > 0)
				{
					foreach (SpriteRenderer renderer in foundSpriteRenderers)
					{
						if (!renderer.gameObject.CompareTag("NoMask"))
						{
							renderer.maskInteraction = MaskInteraction;
						}                        
					}
				}                
			}

			if (AutomaticallySetupTilemaps)
			{
				var foundTilemapRenderers = FindObjectsOfType<TilemapRenderer>();
				if (foundTilemapRenderers.Length > 0)
				{
					foreach (TilemapRenderer renderer in foundTilemapRenderers)
					{
						if (!renderer.gameObject.CompareTag("NoMask"))
						{
							renderer.maskInteraction = MaskInteraction;
						}
					}
				}                
			}

			if (AutomaticallySetupParticleSystems)
			{
				var foundParticleSystems = FindObjectsOfType<ParticleSystem>();
				if (foundParticleSystems.Length > 0)
				{
					foreach (ParticleSystem system in foundParticleSystems)
					{
						if (!system.gameObject.CompareTag("NoMask"))
						{
							ParticleSystemRenderer pr = system.GetComponent<ParticleSystemRenderer>();
							pr.maskInteraction = MaskInteraction;
						}                        
					}
				}
			}
		}

        /// <summary>
        /// Moves the mask to a new size and position for a certain duration and along a certain curve
        /// 将遮罩移动到新的大小和位置，持续一定时间，并沿着某个曲线
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        public virtual void MoveMaskTo(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			StartCoroutine(MoveMaskToCoroutine(newPosition, newSize, duration, curve));            
		}

        /// <summary>
        /// Moves the mask to a new size and position after having expanded to encompass its origin size/position and
        /// the destination's size/position
        /// 先扩展遮罩以包含其原始大小/位置和目的地的大小/位置，然后调整大小以匹配目的地大小
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        public virtual void ExpandAndMoveMaskTo(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			StartCoroutine(ExpandAndMoveMaskToCoroutine(newPosition, newSize, duration, curve));
		}

		protected Vector3 _initialPosition;
		protected Vector3 _initialScale;
		protected Vector3 _newPosition;
		protected Vector3 _newScale;
		protected Vector3 _targetPosition;
		protected Vector3 _targetScale;

        /// <summary>
        /// Coroutine that moves the mask 
        /// 协程，移动遮罩
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        protected virtual IEnumerator MoveMaskToCoroutine(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			if (duration > 0)
			{
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;
				_targetPosition = ComputeTargetPosition(newPosition);
				_targetScale = ComputeTargetScale(newSize);
				float startedAt = MaskTime;

				while (MaskTime - startedAt <= duration)
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, duration, _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, duration, _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;

					yield return null;
				}
			}

			this.transform.position = ComputeTargetPosition(newPosition);
			this.transform.localScale = ComputeTargetScale(newSize);
		}

        /// <summary>
        /// A coroutine that expands the mask to cover both its current position and its destination area, then resizes itself to match the destination size
        ///  一个协程，先扩展遮罩以覆盖当前位置和目的地区域，然后调整自身大小以匹配目的地大小
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        protected virtual IEnumerator ExpandAndMoveMaskToCoroutine(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			if (duration > 0)
			{
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;

				float startedAt = MaskTime;

                // first we move to the total size and position
                //首先我们移动到总大小和位置
                _targetScale.x = this.transform.localScale.x / 2f + Mathf.Abs((this.transform.position - (Vector3)newPosition).x) * ScaleMultiplier + ComputeTargetScale(newSize).x / 2f;
				_targetScale.y = this.transform.localScale.y / 2f + Mathf.Abs((this.transform.position - (Vector3)newPosition).y) * ScaleMultiplier + ComputeTargetScale(newSize).y / 2f;
				_targetScale.z = 1f;

				_targetPosition = (
					(this.transform.position + (Vector3.up * this.transform.localScale.y/ ScaleMultiplier / 2f) + (Vector3.left * this.transform.localScale.x/ ScaleMultiplier / 2f))
					+
					((Vector3)newPosition + (Vector3.down * newSize.y / 2f) + (Vector3.right * newSize.x / 2f))
				) / 2f;


				while (MaskTime - startedAt <= (duration / 2f))
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;
                    
					yield return null;
				}
                
				// then we move to the final position
				startedAt = MaskTime;
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;
				_targetPosition = ComputeTargetPosition(newPosition);
				_targetScale = ComputeTargetScale(newSize);
                
				while (MaskTime - startedAt <= duration / 2f)
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;
                    
					yield return null;
				}
			}

			this.transform.position = ComputeTargetPosition(newPosition);
			this.transform.localScale = ComputeTargetScale(newSize);
		}

        /// <summary>
        /// Determines the new position of the mask
        /// 确定遮罩的新位置。
        /// </summary>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        protected virtual Vector3 ComputeTargetPosition(Vector3 newPosition)
		{
			return newPosition;
		}

        /// <summary>
        /// Determines the scale of the mask
        /// 确定遮罩的缩放比例。
        /// </summary>
        /// <param name="newScale"></param>
        /// <returns></returns>
        protected virtual Vector3 ComputeTargetScale(Vector3 newScale)
		{
			return ScaleMultiplier * newScale;
		}

		/// <summary>
		/// Catches sprite mask events
		/// </summary>
		/// <param name="spriteMaskEvent"></param>
		public virtual void OnMMEvent(MMSpriteMaskEvent spriteMaskEvent)
		{
			if (!CatchEvents)
			{
				return;
			}

			switch(spriteMaskEvent.EventType)
			{
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition:
					MoveMaskTo(spriteMaskEvent.NewPosition, spriteMaskEvent.NewSize, spriteMaskEvent.Duration, spriteMaskEvent.Curve);
					break;
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition:
					ExpandAndMoveMaskTo(spriteMaskEvent.NewPosition, spriteMaskEvent.NewSize, spriteMaskEvent.Duration, spriteMaskEvent.Curve);
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