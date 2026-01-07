using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// An event triggered every time health values change, for other classes to listen to
    /// \每次健康值变化时触发的事件，供其他类监听
    /// </summary>
    public struct HealthChangeEvent
	{
		public Health AffectedHealth;
		public float NewHealth;
		
		public HealthChangeEvent(Health affectedHealth, float newHealth)
		{
			AffectedHealth = affectedHealth;
			NewHealth = newHealth;
		}

		static HealthChangeEvent e;
		public static void Trigger(Health affectedHealth, float newHealth)
		{
			e.AffectedHealth = affectedHealth;
			e.NewHealth = newHealth;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
    /// and what happens when it dies.
    /// 这个类管理一个对象的健康状态，控制其可能的健康条，处理受到伤害时的情况，
    /// 以及死亡时的情况。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")] 
	public class Health : TopDownMonoBehaviour,MMEventListener<TopDownEngineEvent>
	{
		[MMInspectorGroup("Bindings", true, 3)]

        /// the model to disable (if set so)
        [Tooltip("如果设置了，将禁用该模型")]
        public GameObject Model;

        [MMInspectorGroup("Status", true, 29)]

		/// the current health of the character
		[MMReadOnly]
        [Tooltip("字符当前的健康值")]
        public float CurrentHealth;
        /// 如果为真，此对象当前不能受到伤害
        [MMReadOnly]
        [Tooltip("如果为真，此对象当前不能受到伤害")]
        public bool Invulnerable = false;

        [MMInspectorGroup("Health", true, 5)]

		[MMInformation("将这个组件添加到一个对象中，它就会有生命值，可能会受到伤害甚至死亡。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 该对象的初始健康值
        [Tooltip("该对象的初始健康值")]
        public float InitialHealth = 10;
		public bool DynamicMaxHealth;

        /// 该对象的最大健康值
        [Tooltip("该对象的最大健康值")]
        public float MaximumHealth = 10;
        /// 如果为真，每次启用此角色时都会重置健康值（通常在场景开始时）
        [Tooltip("如果为真，每次启用此角色时都会重置健康值（通常在场景开始时）")]
        public bool ResetHealthOnEnable = true;

        [MMInspectorGroup("Damage", true, 6)]

		[MMInformation("在这里，你可以指定一个效果和声音特效来实例化当物体被损坏时，以及当物体被击中时应该闪烁多长时间（只适用于精灵）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 是否允许此健康对象受到伤害
        [Tooltip("是否允许此健康对象受到伤害")]
        public bool ImmuneToDamage = false;
        /// 受到伤害时播放的反馈
        [Tooltip("受到伤害时播放的反馈")]
        public MMFeedbacks DamageMMFeedbacks;
        /// 如果为真，伤害值将作为MMFeedbacks的强度参数传递，让你随着伤害增加触发更强烈的反馈
        [Tooltip("如果为真，伤害值将作为MMFeedbacks的强度参数传递，让你随着伤害增加触发更强烈的反馈")]
        public bool FeedbackIsProportionalToDamage = false;
        /// 如果设置为真，其他对象伤害此对象时不会受到任何自我伤害
        [Tooltip("如果设置为真，其他对象伤害此对象时不会受到任何自我伤害")]
        public bool PreventTakeSelfDamage = false;

        [MMInspectorGroup("Knockback", true, 63)]

        /// 是否对伤害击退免疫
        [Tooltip("是否对伤害击退免疫")]
        public bool ImmuneToKnockback = false;
        /// 如果受到的伤害为零，则是否对伤害击退免疫
        [Tooltip("如果受到的伤害为零，则是否对伤害击退免疫")]
        public bool ImmuneToKnockbackIfZeroDamage = false;
        /// 应用于传入击退力的乘数。0将取消所有击退，0.5将减半，1无影响，2将加倍击退力等
        [Tooltip("应用于传入击退力的乘数。0将取消所有击退，0.5将减半，1无影响，2将加倍击退力等")]
        public float KnockbackForceMultiplier = 1f;

        [MMInspectorGroup("Death", true, 53)]

		[MMInformation("在这里，你可以设置一个对象死亡时实例化的效果"+
					"施加于它的力量（需要自上而下的控制器），给游戏分数增加多少分，"+
					"以及角色应该在哪里重生（仅限非玩家角色）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 此对象死亡时是否应该被销毁
        [Tooltip("此对象死亡时是否应该被销毁")]
        public bool DestroyOnDeath = true;
        /// 角色被销毁或禁用前的时间（秒）
        [Tooltip("角色被销毁或禁用前的时间（秒）")]
        public float DelayBeforeDestruction = 0f;
        /// 对象健康值达到零时玩家获得的分数
        [Tooltip("对象健康值达到零时玩家获得的分数")]
        public int PointsWhenDestroyed;
        /// 如果设置为false，角色将在死亡地点复活，否则将移动到初始位置（场景开始时）
        [Tooltip("如果设置为false，角色将在死亡地点复活，否则将移动到初始位置（场景开始时）")]
        public bool RespawnAtInitialLocation = false;
        /// 如果为真，死亡时将禁用控制器
        [Tooltip("如果为真，死亡时将禁用控制器")]
        public bool DisableControllerOnDeath = true;
        /// 如果为真，死亡时将立即禁用模型（如果设置了模型）
        [Tooltip("如果为真，死亡时将立即禁用模型（如果设置了模型）")]
        public bool DisableModelOnDeath = true;
        /// 如果为真，角色死亡时将关闭碰撞
        [Tooltip("如果为真，角色死亡时将关闭碰撞")]
        public bool DisableCollisionsOnDeath = true;
        /// 如果为真，角色死亡时也将关闭子碰撞器的碰撞
        [Tooltip("如果为真，角色死亡时也将关闭子碰撞器的碰撞")]
        public bool DisableChildCollisionsOnDeath = false;
		//[Tooltip("")]

        /// 是否应在死亡时更改层
        [Tooltip("是否应在死亡时更改层")]
        public bool ChangeLayerOnDeath = false;
        /// 是否应在死亡时递归更改层
        [Tooltip("是否应在死亡时递归更改层")]
        public bool ChangeLayersRecursivelyOnDeath = false;
        /// 角色死亡时应移动到的层
        [Tooltip("角色死亡时应移动到的层")]
        public MMLayer LayerOnDeath;
        /// 死亡时播放的反馈
        [Tooltip("死亡时播放的反馈")]
        public MMFeedbacks DeathMMFeedbacks;

        /// 如果为真，复活时将重置颜色
        [Tooltip("如果为真，复活时将重置颜色")]
        public bool ResetColorOnRevive = true;
        /// 渲染器的着色器中定义颜色的属性名称
        [Tooltip("渲染器的着色器中定义颜色的属性名称")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";
        /// 如果为真，此组件将使用材质属性块而不是在材质实例上工作
        [Tooltip("如果为真，此组件将使用材质属性块而不是在材质实例上工作")]
        public bool UseMaterialPropertyBlocks = false;

        [MMInspectorGroup("Shared Health and Damage Resistance", true, 12)]
        /// 所有健康值将重定向到的另一个Health组件（通常在另一个角色上）
        [Tooltip("所有健康值将重定向到的另一个Health组件（通常在另一个角色上）")]
        public Health MasterHealth;
        /// 此Health在接收到伤害时将使用的伤害抗性处理器
        [Tooltip("此Health在接收到伤害时将使用的伤害抗性处理器")]
        public DamageResistanceProcessor TargetDamageResistanceProcessor;

        [MMInspectorGroup("Animator", true, 14)]
        /// 传递死亡动画参数的目标动画器。如果留空，Health组件将尝试自动绑定
        [Tooltip("传递死亡动画参数的目标动画器。如果留空，Health组件将尝试自动绑定")]
        public Animator TargetAnimator;
        /// 如果为真，将关闭关联动画器的动画器日志，以避免潜在的垃圾信息
        [Tooltip("如果为真，将关闭关联动画器的动画器日志，以避免潜在的垃圾信息")]
        public bool DisableAnimatorLogs = true;

        public virtual float LastDamage { get; set; }
		public virtual Vector3 LastDamageDirection { get; set; }
		public virtual bool Initialized => _initialized;

		// hit delegate
		public delegate void OnHitDelegate();
		public OnHitDelegate OnHit;

		// respawn delegate
		public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

        public delegate void OnFeignDeathDelegate();
        public OnFeignDeathDelegate OnFeign;

        // death delegate
        public delegate void OnDeathDelegate();
		public OnDeathDelegate OnDeath;

		protected Vector3 _initialPosition;
		protected Renderer _renderer;
		protected Character _character;
		protected TopDownController _controller;
		
		protected MMHealthBar _healthBar;
		protected Collider2D _collider2D;
		protected Collider _collider3D;
		protected CharacterController _characterController;
		protected bool _initialized = false;
		protected Color _initialColor;
		protected AutoRespawn _autoRespawn;
		protected int _initialLayer;
		protected MaterialPropertyBlock _propertyBlock;
		protected bool _hasColorProperty = false;

		protected const string _deathAnimatorParameterName = "Death";
		protected const string _healthAnimatorParameterName = "Health";
		protected const string _healthAsIntAnimatorParameterName = "HealthAsInt";
		protected int _deathAnimatorParameter;
		protected int _healthAnimatorParameter;
		protected int _healthAsIntAnimatorParameter;

		protected class InterruptiblesDamageOverTimeCoroutine
		{
			public Coroutine DamageOverTimeCoroutine;
			public DamageType DamageOverTimeType;
		}
		
		protected List<InterruptiblesDamageOverTimeCoroutine> _interruptiblesDamageOverTimeCoroutines;
		protected List<InterruptiblesDamageOverTimeCoroutine> _damageOverTimeCoroutines;

		#region Initialization
		
		/// <summary>
		/// On Awake, we initialize our health
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			InitializeCurrentHealth();
		}

		/// <summary>
		/// On Start we grab our animator
		/// </summary>
		protected virtual void Start()
		{
			GrabAnimator();
		}
		
		/// <summary>
		/// Grabs useful components, enables damage and gets the inital color
		/// </summary>
		public virtual void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>(); 

			if (Model != null)
			{
				Model.SetActive(true);
			}        
            
			if (gameObject.GetComponentInParent<Renderer>() != null)
			{
				_renderer = GetComponentInParent<Renderer>();				
			}
			
			if (_renderer != null)
			{
				if (UseMaterialPropertyBlocks && (_propertyBlock == null))
				{
					_propertyBlock = new MaterialPropertyBlock();
				}
	            
				if (ResetColorOnRevive)
				{
					if (UseMaterialPropertyBlocks)
					{
						if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true; 
							_initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
						}
					}
					else
					{
						if (_renderer.material.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true;
							_initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
						} 
					}
				}
			}

			_interruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_damageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_initialLayer = gameObject.layer;
			
			_deathAnimatorParameter = Animator.StringToHash(_deathAnimatorParameterName);
			_healthAnimatorParameter = Animator.StringToHash(_healthAnimatorParameterName);
			_healthAsIntAnimatorParameter = Animator.StringToHash(_healthAsIntAnimatorParameterName);

			_autoRespawn = this.gameObject.GetComponentInParent<AutoRespawn>();
			_healthBar = this.gameObject.GetComponentInParent<MMHealthBar>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_characterController = this.gameObject.GetComponentInParent<CharacterController>();
			_collider2D = this.gameObject.GetComponentInParent<Collider2D>();
			_collider3D = this.gameObject.GetComponentInParent<Collider>();

			DamageMMFeedbacks?.Initialization(this.gameObject);
			DeathMMFeedbacks?.Initialization(this.gameObject);

			StoreInitialPosition();
			_initialized = true;
			
			DamageEnabled();
		}

        public virtual void OnInitHealth(float InitHealth)
        {
            InitialHealth = InitHealth;
            MaximumHealth = InitHealth;
        }

        /// <summary>
        /// Grabs the target animator
        /// </summary>
        protected virtual void GrabAnimator()
		{
			if (TargetAnimator == null)
			{
				BindAnimator();
			}

			if ((TargetAnimator != null) && DisableAnimatorLogs)
			{
				TargetAnimator.logWarnings = false;
			}
			UpdateHealthAnimationParameters();
		}

		/// <summary>
		/// Finds and binds an animator if possible
		/// </summary>
		protected virtual void BindAnimator()
		{
			if (_character != null)
			{
				
			}
			else
			{
				TargetAnimator = GetComponent<Animator>();
			}    
		}

		/// <summary>
		/// Stores the initial position for further use
		/// </summary>
		public virtual void StoreInitialPosition()
		{
			_initialPosition = this.transform.position;
		}
		
		/// <summary>
		/// Initializes health to either initial or current values
		/// </summary>
		public virtual void InitializeCurrentHealth()
		{
			if (MasterHealth == null)
			{
				SetHealth(InitialHealth);	
			}
			else
			{
				if (MasterHealth.Initialized)
				{
					SetHealth(MasterHealth.CurrentHealth);
				}
				else
				{
					SetHealth(MasterHealth.InitialHealth);
				}
			}
		}

		/// <summary>
		/// When the object is enabled (on respawn for example), we restore its initial health levels
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ResetHealthOnEnable)
			{
				InitializeCurrentHealth();
			}
			if (Model != null)
			{
				Model.SetActive(true);
			}            
			DamageEnabled();

            this.MMEventStartListening<TopDownEngineEvent>();
        }

        /// <summary>
        /// On Disable, we prevent any delayed destruction from running
        /// </summary>
        protected virtual void OnDisable()
		{
			CancelInvoke();
            this.MMEventStopListening<TopDownEngineEvent>();

        }

        #endregion

        /// <summary>
        /// Returns true if this Health component can be damaged this frame, and false otherwise
        /// 如果这个健康组件在这帧可以受到伤害，则返回真，否则返回假。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTakeDamageThisFrame()
		{
			// if the object is invulnerable, we do nothing and exit 
			if (Invulnerable || ImmuneToDamage)
			{
				return false;
			}

			if (!this.enabled)
			{
				return false;
			}
			
			// if we're already below zero, we do nothing and exit
			if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the object takes damage
		/// </summary>
		/// <param name="damage">The amount of health points that will get lost.</param>
		/// <param name="instigator">The object that caused the damage.</param>
		/// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage - not used anymore, kept to not break retrocompatibility</param>
		/// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
		public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
		{
			if (!CanTakeDamageThisFrame())
			{
				return;
			}

			damage = ComputeDamageOutput(damage, typedDamages, true);
			
			// we decrease the character's health by the damage
			float previousHealth = CurrentHealth;
			if (MasterHealth != null)
			{
				previousHealth = MasterHealth.CurrentHealth;
				MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
			}
			else
			{
				SetHealth(CurrentHealth - damage);	
			}

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			if (OnHit != null)
			{
				OnHit();
			}

			// we prevent the character from colliding with Projectiles, Player and Enemies
			if (invincibilityDuration > 0)
			{
				DamageDisabled();
				StartCoroutine(DamageEnabled(invincibilityDuration));	
			}
            
			// we trigger a damage taken event
			MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth, typedDamages);

			// we update our animator
			if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger("Damage");
			}

			// we play our feedback
			if (FeedbackIsProportionalToDamage)
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);    
			}
			else
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
			}
            
			// we update the health bar
			UpdateHealthBar(true);
			
			// we process any condition state change
			ComputeCharacterConditionStateChanges(typedDamages);
			ComputeCharacterMovementMultipliers(typedDamages);

			// if health has reached zero we set its health to zero (useful for the healthbar)
			if (MasterHealth != null)
			{
				if (MasterHealth.CurrentHealth <= 0)
				{
					MasterHealth.CurrentHealth = 0;
					MasterHealth.Kill();
				}
			}
			else
			{
				if (CurrentHealth <= 0)
				{
					CurrentHealth = 0;
					Kill();
				}
					
			}
		}

		/// <summary>
		/// Interrupts all damage over time, regardless of type
		/// </summary>
		public virtual void InterruptAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_interruptiblesDamageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// Interrupts all damage over time, even the non interruptible ones (usually on death)
		/// </summary>
		public virtual void StopAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _damageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_damageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// Interrupts all damage over time of the specified type
		/// </summary>
		/// <param name="damageType"></param>
		public virtual void InterruptAllDamageOverTimeOfType(DamageType damageType)
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				if (coroutine.DamageOverTimeType == damageType)
				{
					StopCoroutine(coroutine.DamageOverTimeCoroutine);	
				}
			}
			TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
		}

        /// <summary>
        /// Applies damage over time, for the specified amount of repeats (which includes the first application of damage, makes it easier to do quick maths in the inspector, and at the specified interval).
        /// Optionally you can decide that your damage is interruptible, in which case, calling InterruptAllDamageOverTime() will stop these from being applied, useful to cure poison for example.
        /// 对指定的次数施加持续伤害（包括第一次伤害的应用，这样可以在检视器中更容易进行快速计算），并在指定的时间间隔内进行。
		/// 你可以选择让你的伤害是可中断的，如果是这样，调用  InterruptAllDamageOverTime()  将停止这些伤害的应用，例如可以用来解毒。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="instigator"></param>
        /// <param name="flickerDuration"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection"></param>
        /// <param name="typedDamages"></param>
        /// <param name="amountOfRepeats"></param>
        /// <param name="durationBetweenRepeats"></param>
        /// <param name="interruptible"></param>
        public virtual void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			if (ComputeDamageOutput(damage, typedDamages, false) == 0)
			{
				return;
			}
			
			InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
			damageOverTime.DamageOverTimeType = damageType;
			damageOverTime.DamageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damage, instigator, flickerDuration,
				invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats,
				interruptible));
			_damageOverTimeCoroutines.Add(damageOverTime);
			if (interruptible)
			{
				_interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
			}
		}

		/// <summary>
		/// A coroutine used to apply damage over time
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="instigator"></param>
		/// <param name="flickerDuration"></param>
		/// <param name="invincibilityDuration"></param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
		/// <param name="amountOfRepeats"></param>
		/// <param name="durationBetweenRepeats"></param>
		/// <param name="interruptible"></param>
		/// <param name="damageType"></param>
		/// <returns></returns>
		protected virtual IEnumerator DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			for (int i = 0; i < amountOfRepeats; i++)
			{
				Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
				yield return MMCoroutine.WaitFor(durationBetweenRepeats);
			}
		}

        /// <summary>
        /// Returns the damage this health should take after processing potential resistances
        /// 返回在处理潜在抗性后，这个健康值应该受到的伤害。
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public virtual float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
		{
			if (Invulnerable || ImmuneToDamage)
			{
				return 0;
			}
			
			float totalDamage = 0f;
			// we process our damage through our potential resistances
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);	
				}
			}
			else
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
			}
			return totalDamage;
		}

		/// <summary>
		/// Goes through resistances and applies condition state changes if needed
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ForceCharacterCondition)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);	
				}
			}
			
		}

		/// <summary>
		/// Goes through the resistance list and applies movement multipliers if needed
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ApplyMovementMultiplier)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Determines a new knockback force by processing it through resistances
		/// </summary>
		/// <param name="knockbackForce"></param>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
		{
			return (TargetDamageResistanceProcessor == null) ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages);;

		}

		/// <summary>
		/// Returns true if this Health can get knockbacked, false otherwise
		/// </summary>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual bool CanGetKnockback(List<TypedDamage> typedDamages) 
		{
			if (ImmuneToKnockback)
			{
				return false;
			}
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					bool checkResistance = TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages);
					if (checkResistance)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Kills the character, instantiates death effects, handles points, etc
		/// </summary>
		public virtual void Kill()
		{
			if (ImmuneToDamage)
			{
				return;
			}
	        
			if (_character != null)
			{
				// we set its dead state to true
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
				_character.Reset();

				if (_character.CharacterType == Character.CharacterTypes.Player)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
				}
			}
			SetHealth(0);

			// we prevent further damage
			StopAllDamageOverTime();
			DamageDisabled();

			DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);
            
			// Adds points if needed.
			if(PointsWhenDestroyed != 0)
			{
                // we send a new points event for the GameManager to catch (and other classes that may listen to it too)
                //我们向GameManager发送一个新的分数事件，以便它能够捕获（以及其他可能监听它的类也能捕获）。
                TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
			}

			if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger(_deathAnimatorParameter);
			}
			// we make it ignore the collisions from now on
			if (DisableCollisionsOnDeath)
			{
				if (_collider2D != null)
				{
					_collider2D.enabled = false;
				}
				if (_collider3D != null)
				{
					_collider3D.enabled = false;
				}

				// if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
				if (_controller != null)
				{				
					_controller.CollisionsOff();						
				}

				if (DisableChildCollisionsOnDeath)
				{
					foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
					{
						collider.enabled = false;
					}
					foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
					{
						collider.enabled = false;
					}
				}
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = LayerOnDeath.LayerIndex;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
				}
			}
            
			OnDeath?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

			if (DisableControllerOnDeath && (_controller != null))
			{
				_controller.enabled = false;
			}

			if (DisableControllerOnDeath && (_characterController != null))
			{
				_characterController.enabled = false;
			}

			if (DisableModelOnDeath && (Model != null))
			{
				Model.SetActive(false);
			}

			if (DelayBeforeDestruction > 0f)
			{
				Invoke ("DestroyObject", DelayBeforeDestruction);
			}
			else
			{
				// finally we destroy the object
				DestroyObject();	
			}
		}

		/// <summary>
		/// Revive this object.
		/// </summary>
		public virtual void Revive()
		{
			if (!_initialized)
			{
				return;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
			if (_collider3D != null)
			{
				_collider3D.enabled = true;
			}
			if (DisableChildCollisionsOnDeath)
			{
				foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
				{
					collider.enabled = true;
				}
				foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
				{
					collider.enabled = true;
				}
			}
			if (ChangeLayerOnDeath)
			{
				gameObject.layer = _initialLayer;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(_initialLayer);
				}
			}
			if (_characterController != null)
			{
				_characterController.enabled = true;
			}
			if (_controller != null)
			{
				_controller.enabled = true;
				_controller.CollisionsOn();
				_controller.Reset();
			}
			if (_character != null)
			{
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
			if (ResetColorOnRevive && (_renderer != null))
			{
				if (UseMaterialPropertyBlocks)
				{
					_renderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
					_renderer.SetPropertyBlock(_propertyBlock);    
				}
				else
				{
					_renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
				}
			}            

			if (RespawnAtInitialLocation)
			{
				transform.position = _initialPosition;
			}
			if (_healthBar != null)
			{
				_healthBar.Initialization();
			}

			Initialization();
			InitializeCurrentHealth();
			OnRevive?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
		}

        public void OnFeignDeath()
        {
            if (!_initialized)
            {
                return;
            }

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }
            if (_collider3D != null)
            {
                _collider3D.enabled = true;
            }
            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = true;
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = true;
                }
            }
            if (ChangeLayerOnDeath)
            {
                gameObject.layer = _initialLayer;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(_initialLayer);
                }
            }

            if (_characterController != null)
            {
                _characterController.enabled = true;
            }
            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }

            if (ResetColorOnRevive && (_renderer != null))
            {
                if (UseMaterialPropertyBlocks)
                {
                    _renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                    _renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
                }
            }


            if (_healthBar != null)
            {
                _healthBar.Initialization();
            }

            Initialization();
            InitializeCurrentHealth();
            OnFeign?.Invoke();
        }

        /// <summary>
        /// Destroys the object, or tries to, depending on the character's settings
        /// </summary>
        protected virtual void DestroyObject()
		{
			if (_autoRespawn == null)
			{
				if (DestroyOnDeath)
				{
					if (_character != null)
					{
						_character.gameObject.SetActive(false);
					}
					else
					{
						gameObject.SetActive(false);	
					}
				}                
			}
			else
			{
				_autoRespawn.Kill();
			}
		}

		#region HealthManipulationAPIs
		

		/// <summary>
		/// Sets the current health to the specified new value, and updates the health bar
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetHealth(float newValue)
		{
			CurrentHealth = newValue;
			if (_character != null && _character.CharacterType == Character.CharacterTypes.AI) 
			{
              

                UpdateHealthBar(false);
            }

            if (_character != null && _character.CharacterType == Character.CharacterTypes.Player)
            {
              
            }

			

            //HealthChangeEvent.Trigger(this, newValue);
        }

        /// <summary>
        /// Called when the character gets health (from a stimpack for example)
        /// 当角色获得健康值（例如从一个刺激包）时被调用。
        /// </summary>
        /// <param name="health">The health the character gets.</param>
        /// <param name="instigator">The thing that gives the character health.</param>
        public virtual void ReceiveHealth(float health,GameObject instigator = null)
		{
			// this function adds health to the character's Health and prevents it to go above MaxHealth.
			//这个函数为角色的Health添加健康值，并防止它超过MaxHealth。

			if (DynamicMaxHealth) 
			{
				if (CurrentHealth + health > MaximumHealth) 
				{
					MaximumHealth = CurrentHealth + health;
				}
			}

			float newValue = Mathf.Min(CurrentHealth + health, MaximumHealth);

            if (MasterHealth != null)
			{
				MasterHealth.SetHealth(newValue);	
			}
			else
			{
				SetHealth(newValue);	
			}
			UpdateHealthBar(true);


			//HealthChangeEvent.Trigger(this, newValue);
		}
		
		/// <summary>
		/// Resets the character's health to its max value
		/// </summary>
		public virtual void ResetHealthToMaxHealth()
		{
			SetHealth(MaximumHealth);
		}
		
		/// <summary>
		/// Forces a refresh of the character's health bar
		/// </summary>
		public virtual void UpdateHealthBar(bool show)
		{
			UpdateHealthAnimationParameters();

			if (MaximumHealth <= 0)
				return;

			if (_healthBar != null)
			{
				_healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
			}

			
		}

		protected virtual void UpdateHealthAnimationParameters()
		{
			if (TargetAnimator != null)
			{
				TargetAnimator.SetFloat(_healthAnimatorParameter, CurrentHealth);
				TargetAnimator.SetInteger(_healthAsIntAnimatorParameter, (int)CurrentHealth);
			}
		}

		#endregion
		
		#region DamageDisablingAPIs

		/// <summary>
		/// Prevents the character from taking any damage
		/// </summary>
		public virtual void DamageDisabled()
		{
			Invulnerable = true;
		}

        /// <summary>
        /// Allows the character to take damage
        /// 允许角色受到伤害
        /// </summary>
        public virtual void DamageEnabled()
		{
			Invulnerable = false;
		}

		/// <summary>
		/// makes the character able to take damage again after the specified delay
		/// </summary>
		/// <returns>The layer collision.</returns>
		public virtual IEnumerator DamageEnabled(float delay)
		{
			yield return new WaitForSeconds (delay);
			Invulnerable = false;
		}

        public void OnMMEvent(TopDownEngineEvent eventType)
        {
			switch (eventType.EventType) 
			{
				case TopDownEngineEventTypes.ShootAmo:
					if (_character != null && _character.CharacterType == Character.CharacterTypes.Player)
						ReceiveHealth(eventType.Value);
                    break;

            }
        }

        #endregion
    }
}