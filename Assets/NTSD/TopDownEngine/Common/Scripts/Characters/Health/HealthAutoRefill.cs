using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this class to a character or object with a Health class, and its health will auto refill based on the settings here
    /// 这句话的中文翻译是：将这个类添加到具有Health类的角色或对象上，它的健康值将根据这里的设置自动回填。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Health Auto Refill")]
	public class HealthAutoRefill : TopDownMonoBehaviour
	{
		/// the possible refill modes :
		/// - linear : constant health refill at a certain rate per second
		/// - bursts : periodic bursts of health
		public enum RefillModes { Linear, Bursts }

		[Header("Mode")]
        /// 选择的回填模式
        [Tooltip("选择的回填模式")]
        public RefillModes RefillMode;
        /// 一个可选的目标Health组件来补充
        [Tooltip("一个可选的目标Health组件来补充")]
        public Health TargetHealth;

        [Header("Cooldown")]
        /// 在补充启动之前需要经过多少时间（秒）
        [Tooltip("在补充启动之前需要经过多少时间（秒）")]
        public float CooldownAfterHit = 1f;

        [Header("Refill Settings")]
        /// 如果这个为真，当健康值不是满值时将自动补充
        [Tooltip("如果这个为真，当健康值不是满值时将自动补充")]
        public bool RefillHealth = true;
        /// 在线性模式下每秒恢复的健康值
        [MMEnumCondition("RefillMode", (int)RefillModes.Linear)]
        [Tooltip("在线性模式下每秒恢复的健康值")]
        public float HealthPerSecond;
        /// 在脉冲模式下每次脉冲恢复的健康值
        [MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
        [Tooltip("在脉冲模式下每次脉冲恢复的健康值")]
        public float HealthPerBurst = 5;
        /// 两次健康脉冲之间的持续时间（秒）
        [MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
        [Tooltip("两次健康脉冲之间的持续时间（秒）")]
        public float DurationBetweenBursts = 2f;

        protected Health _health;
		protected float _lastHitTime = 0f;
		protected float _healthToGive = 0f;
		protected float _lastBurstTimestamp;

		/// <summary>
		/// On Awake we do our init
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init we grab our Health component
		/// </summary>
		protected virtual void Initialization()
		{
			_health = TargetHealth == null ? this.gameObject.GetComponent<Health>() : TargetHealth;
		}

		/// <summary>
		/// On Update we refill
		/// </summary>
		protected virtual void Update()
		{
			ProcessRefillHealth();
		}  

		/// <summary>
		/// Tests if a refill is needed and processes it
		/// </summary>
		protected virtual void ProcessRefillHealth()
		{
			if (!RefillHealth)
			{
				return;
			}

			if (Time.time - _lastHitTime < CooldownAfterHit)
			{
				return;
			}

			if (_health.CurrentHealth < _health.MaximumHealth)
			{
				switch (RefillMode)
				{
					case RefillModes.Bursts:
						if (Time.time - _lastBurstTimestamp > DurationBetweenBursts)
						{
							_health.ReceiveHealth(HealthPerBurst, this.gameObject);
							_lastBurstTimestamp = Time.time;
						}
						break;

					case RefillModes.Linear:
						_healthToGive += HealthPerSecond * Time.deltaTime;
						if (_healthToGive > 1f)
						{
							float givenHealth = _healthToGive;
							_healthToGive -= givenHealth;
							_health.ReceiveHealth(givenHealth, this.gameObject);
						}
						break;
				}
			}
		}

		/// <summary>
		/// On hit we store our time
		/// </summary>
		public virtual void OnHit()
		{
			_lastHitTime = Time.time;
		}
        
		/// <summary>
		/// On enable we start listening for hits
		/// </summary>
		protected virtual void OnEnable()
		{
			_health.OnHit += OnHit;
		}

		/// <summary>
		/// On disable we stop listening for hits
		/// </summary>
		protected virtual void OnDisable()
		{
			_health.OnHit -= OnHit;
		}
	}
}