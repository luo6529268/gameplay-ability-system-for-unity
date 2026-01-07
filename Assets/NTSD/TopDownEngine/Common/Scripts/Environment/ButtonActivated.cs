using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 扩展这个类以在某个区域按下按钮时激活某些内容。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Button Activated")]
	public class ButtonActivated : TopDownMonoBehaviour 
	{
		public enum ButtonActivatedRequirements { Character, ButtonActivator, Either, None }
		public enum InputTypes { Default, Button, Key }

		[MMInspectorGroup("Requirements", true, 10)]
        [MMInformation("在这里，你可以指定与这个区域交互所需的内容。是否需要角色具备按钮激活能力？是否只能由玩家交互？", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        [Tooltip("如果这是true，具有ButtonActivator类的对象将能够与这个区域交互")]
        public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;

        [Tooltip("如果这是true，这只能由玩家角色激活")]
        public bool RequiresPlayerType = true;

        [Tooltip("如果这是true，只有角色具备所需能力时，这个区域才能被激活")]
        public bool RequiresButtonActivationAbility = true;


        [MMInspectorGroup("Activation Conditions", true, 11)]

        [MMInformation("在这里，你可以指定该区域如何被交互。你可以设置它自动激活，仅在着地时激活，或者完全阻止其激活。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        [Tooltip("如果这是false，该区域将无法被激活")]
        public bool Activable = true;

        [Tooltip("如果为true，无论按钮是否被按下，该区域都将被激活")]
        public bool AutoActivation = false;

        [MMCondition("AutoActivation", true)]
        [Tooltip("角色需要在区域内停留的时长（以秒为单位），才能激活该区域")]
        public float AutoActivationDelay = 0f;

        [MMCondition("AutoActivation", true)]
        [Tooltip("如果这是true，离开该区域将重置自动激活延迟")]
        public bool AutoActivationDelayResetsOnExit = true;

        [Tooltip("如果此选项设置为false，则角色在非着地状态下无法激活该区域")]
        public bool CanOnlyActivateIfGrounded = false;


        [Tooltip("如果你希望角色行为状态在进入区域时得到通知，请将此设置为true。")]
        public bool ShouldUpdateState = true;
  
        [Tooltip("如果这是true，当有其他对象进入时，不会重新触发进入事件，只有当最后一个对象离开时，才会触发退出事件。")]
        public bool OnlyOneActivationAtOnce = true;

        [Tooltip("一个包含所有可以与这个特定按钮激活区域交互的图层的图层遮罩。")]
        public LayerMask TargetLayerMask = ~0;

        [MMInspectorGroup("激活次数", true, 12)]

        [MMInformation("你可以决定让这个区域永远可以交互，或者只允许有限次数的交互，并且可以指定使用之间的延迟（以秒为单位）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        [Tooltip("如果此选项设置为false，你的激活次数将是最大激活次数。")]
        public bool UnlimitedActivations = true;

        [Tooltip("该区域可以被交互的次数。")]
        public int MaxNumberOfActivations = 0;

        [Tooltip("该区域剩余的激活次数。")]
        [MMReadOnly]
        public int NumberOfActivationsLeft;

        [Tooltip("激活后区域在多少秒内无法再次被激活的延迟。")]
        public float DelayBetweenUses = 0f;

        [Tooltip("如果这是true，该区域在最后一次使用后将禁用自身（永久或直到你手动重新激活它）。")]
        public bool DisableAfterUse = false;


        [MMInspectorGroup("Input", true, 13)]

        [Tooltip("所选的输入类型（默认、按钮或按键）")]
        public InputTypes InputType = InputTypes.Default;
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the input action to use for this button activated object
			public InputActionProperty InputSystemAction = new InputActionProperty(
				new InputAction(
					name: "ButtonActivatedAction",
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
		#else
			/// the selected button string used to activate this zone
			[MMEnumCondition("InputType", (int)InputTypes.Button)]
			[Tooltip("the selected button string used to activate this zone")]
			public string InputButton = "Interact";
			/// the key used to activate this zone
			[MMEnumCondition("InputType", (int)InputTypes.Key)]
			[Tooltip("the key used to activate this zone")]
			public KeyCode InputKey = KeyCode.Space;
		#endif

		[MMInspectorGroup("Animation", true, 14)]

		/// an (absolutely optional) animation parameter that can be triggered on the character when activating the zone	
		[Tooltip("an (absolutely optional) animation parameter that can be triggered on the character when activating the zone	")]
		public string AnimationTriggerParameterName;

		[MMInspectorGroup("Visual Prompt", true, 15)]

        [MMInformation("你可以让这个区域显示一个视觉提示，以向玩家表明它是可交互的。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        [Tooltip("如果这是true，且设置正确，将显示一个提示")]
        public bool UseVisualPrompt = true;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("用于实例化以显示提示的游戏对象")]
        public ButtonPrompt ButtonPromptPrefab;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("要在按钮提示中显示的文本")]
        public string ButtonPromptText = "A";

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("按钮提示的颜色")]
        public Color ButtonPromptColor = MMColors.LawnGreen;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("提示文本的颜色")]
        public Color ButtonPromptTextColor = MMColors.White;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("如果为true，无论玩家是否在区域内，“按钮A”提示将始终显示。")]
        public bool AlwaysShowPrompt = true;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("如果为true，当玩家与区域碰撞时，“按钮A”提示将显示")]
        public bool ShowPromptWhenColliding = true;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("如果为true，使用后提示将隐藏")]
        public bool HidePromptAfterUse = false;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("实际按钮A提示相对于对象中心的位置")]
        public Vector3 PromptRelativePosition = Vector3.zero;

        [MMCondition("UseVisualPrompt", true)]
        [Tooltip("实际按钮A提示的旋转")]
        public Vector3 PromptRotation = Vector3.zero;


        [MMInspectorGroup("Feedbacks", true, 16)]

        /// 当区域被激活时播放的反馈
        [Tooltip("当区域被激活时播放的反馈")]
        public MMFeedbacks ActivationFeedback;
        /// 当区域尝试被激活但无法激活时播放的反馈
        [Tooltip("当区域尝试被激活但无法激活时播放的反馈")]
        public MMFeedbacks DeniedFeedback;
        /// 当进入区域时播放的反馈
        [Tooltip("当进入区域时播放的反馈")]
        public MMFeedbacks EnterFeedback;
        /// 当离开区域时播放的反馈
        [Tooltip("当离开区域时播放的反馈")]
        public MMFeedbacks ExitFeedback;

        [MMInspectorGroup("Actions", true, 17)]

        /// 当这个区域被激活时触发的UnityEvent
        [Tooltip("当这个区域被激活时触发的UnityEvent")]
        public UnityEvent OnActivation;
        /// 当离开这个区域时触发的UnityEvent
        [Tooltip("当离开这个区域时触发的UnityEvent")]
        public UnityEvent OnExit;
        /// 当角色处于这个区域内时触发的UnityEvent
        [Tooltip("当角色处于这个区域内时触发的UnityEvent")]
        public UnityEvent OnStay;


        protected Animator _buttonPromptAnimator;
		protected ButtonPrompt _buttonPrompt;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected bool _promptHiddenForever = false;
		protected float _lastActivationTimestamp;
		protected List<GameObject> _collidingObjects;
		protected Character _currentCharacter;
		protected bool _staying = false;
		protected Coroutine _autoActivationCoroutine;
        
		public virtual bool AutoActivationInProgress { get; set; }
		public virtual float AutoActivationStartedAt { get; set; }
		public bool InputActionPerformed
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					return InputSystemAction.action.WasPressedThisFrame();
				#else
					return false;
				#endif
			}
		}

		/// <summary>
		/// On Enable, we initialize our ButtonActivated zone
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs components and shows prompt if needed
		/// </summary>
		public virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			NumberOfActivationsLeft = MaxNumberOfActivations;
			_collidingObjects = new List<GameObject>();

			ActivationFeedback?.Initialization(this.gameObject);
			DeniedFeedback?.Initialization(this.gameObject);
			EnterFeedback?.Initialization(this.gameObject);
			ExitFeedback?.Initialization(this.gameObject);

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Enable();
			#endif
		}
		
		/// <summary>
		/// On disable we disable our input action if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Disable();
			#endif
		}

		protected virtual IEnumerator TriggerButtonActionCo()
		{
			if (AutoActivationDelay <= 0f)
			{
				TriggerButtonAction();
				yield break;
			}
			else
			{
				AutoActivationInProgress = true;
				AutoActivationStartedAt = Time.time;
				yield return MMCoroutine.WaitFor(AutoActivationDelay);
				AutoActivationInProgress = false;
				TriggerButtonAction();
				yield break;
			}
		}

		/// <summary>
		/// When the input button is pressed, we check whether or not the zone can be activated, and if yes, trigger ZoneActivated
		/// </summary>
		public virtual void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			_staying = true;
			ActivateZone();
		}

		public virtual void TriggerExitAction(GameObject collider)
		{
			_staying = false;
			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

		/// <summary>
		/// Makes the zone activable
		/// </summary>
		public virtual void MakeActivable()
		{
			Activable = true;
		}

		/// <summary>
		/// Makes the zone unactivable
		/// </summary>
		public virtual void MakeUnactivable()
		{
			Activable = false;
		}

		/// <summary>
		/// Makes the zone activable if it wasn't, unactivable if it was activable.
		/// </summary>
		public virtual void ToggleActivable()
		{
			Activable = !Activable;
		}

		protected virtual void Update()
		{
			if (_staying && (OnStay != null))
			{
				OnStay.Invoke();
			}
		}

		/// <summary>
		/// Activates the zone
		/// </summary>
		protected virtual void ActivateZone()
		{
			if (OnActivation != null)
			{
				OnActivation.Invoke();
			}

			_lastActivationTimestamp = Time.time;

			ActivationFeedback?.PlayFeedbacks(this.transform.position);

			if (HidePromptAfterUse)
			{
				_promptHiddenForever = true;
				HidePrompt();	
			}	
			NumberOfActivationsLeft--;

			if (DisableAfterUse && (NumberOfActivationsLeft <= 0))
			{
				DisableZone();
			}
		}

		/// <summary>
		/// Triggers an error 
		/// </summary>
		public virtual void PromptError()
		{
			if (_buttonPromptAnimator != null)
			{
				_buttonPromptAnimator.SetTrigger("Error");
			}
			DeniedFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Shows the button A prompt.
		/// </summary>
		public virtual void ShowPrompt()
		{
			if (!UseVisualPrompt || _promptHiddenForever || (ButtonPromptPrefab == null))
			{
				return;
			}
            
			// we add a blinking A prompt to the top of the zone
			if (_buttonPrompt == null)
			{
				_buttonPrompt = (ButtonPrompt)Instantiate(ButtonPromptPrefab);
				_buttonPrompt.Initialization();
				_buttonPromptAnimator = _buttonPrompt.gameObject.MMGetComponentNoAlloc<Animator>();
			}
			
			if (_collider != null)
			{
				_buttonPrompt.transform.position = _collider.bounds.center + PromptRelativePosition;
			}
			if (_collider2D != null)
			{
				_buttonPrompt.transform.position = _collider2D.bounds.center + PromptRelativePosition;
			}

			if (_buttonPrompt != null)
			{
				_buttonPrompt.transform.parent = transform;
				_buttonPrompt.transform.localEulerAngles = PromptRotation;
				_buttonPrompt.SetText(ButtonPromptText);
				_buttonPrompt.SetBackgroundColor(ButtonPromptColor);
				_buttonPrompt.SetTextColor(ButtonPromptTextColor);
				_buttonPrompt.Show();
			}
		}

		/// <summary>
		/// Hides the button A prompt.
		/// </summary>
		public virtual void HidePrompt()
		{
			if (_buttonPrompt != null)
			{
				_buttonPrompt.Hide();
			}
		}

		/// <summary>
		/// Disables the button activated zone
		/// </summary>
		public virtual void DisableZone()
		{
			Activable = false;
            
			if (_collider != null)
			{
				_collider.enabled = false;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
	            
			
		}

		/// <summary>
		/// Enables the button activated zone
		/// </summary>
		public virtual void EnableZone()
		{
			Activable = true;
            
			if (_collider != null)
			{
				_collider.enabled = true;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter2D (Collider2D collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit2D (Collider2D collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter (Collider collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit (Collider collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
        
		/// <summary>
		/// Triggered when something collides with the button activated zone
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void TriggerEnter(GameObject collider)
		{            
			if (!CheckConditions(collider))
			{
				return;
			}

			// if we can only activate this zone when grounded, we check if we have a controller and if it's not grounded,
			// we do nothing and exit
			if (CanOnlyActivateIfGrounded)
			{
				if (collider != null)
				{
					TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
					if (controller != null)
					{
						if (!controller.Grounded)
						{
							return;
						}
					}
				}
			}

			// at this point the object is colliding and authorized, we add it to our list
			_collidingObjects.Add(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			EnterFeedback?.PlayFeedbacks(this.transform.position);

			if (ShouldUpdateState)
			{
				
			}

			if (AutoActivation)
			{
				_autoActivationCoroutine = StartCoroutine(TriggerButtonActionCo());
			}	

			// if we're not already showing the prompt and if the zone can be activated, we show it
			if (ShowPromptWhenColliding)
			{
				ShowPrompt();	
			}
		}

		/// <summary>
		/// Triggered when something exits the water
		/// </summary>
		/// <param name="collider">Something colliding with the dialogue zone.</param>
		protected virtual void TriggerExit(GameObject collider)
		{
			if (!CheckConditions(collider))
			{
				return;
			}

			_collidingObjects.Remove(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			AutoActivationInProgress = false;
			if (_autoActivationCoroutine != null)
			{
				StopCoroutine(_autoActivationCoroutine);
			}

			if (ShouldUpdateState)
			{
				
			}

			ExitFeedback?.PlayFeedbacks(this.transform.position);

			if ((_buttonPrompt!=null) && !AlwaysShowPrompt)
			{
				HidePrompt();	
			}

			TriggerExitAction(collider);
		}

		/// <summary>
		/// Tests if the object exiting our zone is the last remaining one
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestForLastObject(GameObject collider)
		{
			if (OnlyOneActivationAtOnce)
			{
				if (_collidingObjects.Count > 0)
				{
					bool lastObject = true;
					foreach (GameObject obj in _collidingObjects)
					{
						if ((obj != null) && (obj != collider))
						{
							lastObject = false;
						}
					}
					return lastObject;
				}                    
			}
			return true;            
		}

		/// <summary>
		/// Checks the remaining number of uses and eventual delay between uses and returns true if the zone can be activated.
		/// </summary>
		/// <returns><c>true</c>, if number of uses was checked, <c>false</c> otherwise.</returns>
		public virtual bool CheckNumberOfUses()
		{
			if (!Activable)
			{
				return false;
			}

			if (Time.time - _lastActivationTimestamp < DelayBetweenUses)
			{
				return false;
			}

			if (UnlimitedActivations)
			{
				return true;
			}

			if (NumberOfActivationsLeft == 0)
			{
				return false;
			}

			if (NumberOfActivationsLeft > 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether or not this zone should be activated
		/// </summary>
		/// <returns><c>true</c>, if conditions was checked, <c>false</c> otherwise.</returns>
		/// <param name="character">Character.</param>
		/// <param name="characterButtonActivation">Character button activation.</param>
		protected virtual bool CheckConditions(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return false;
			}
			
			Character character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			switch (ButtonActivatedRequirement)
			{
				case ButtonActivatedRequirements.Character:
					if (character == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.ButtonActivator:
					if (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.Either:
					if ((character == null) && (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null))
					{
						return false;
					}
					break;
			}

			if (RequiresPlayerType)
			{
				if (character == null)
				{
					return false;
				}
				if (character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}

			if (RequiresButtonActivationAbility)
			{
				
			}

			return true;
		}
	}
}