using BeatEmUpTemplate2D;
using GAS.Runtime;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Game;
using NTSD.TimeWheel;
using NTSD.Simulation;  // Plan B: SimulationWorld integration
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 这个类将控制你角色的TopDownController组件。
	/// 这是你将实现所有角色游戏规则的地方，比如跳跃、冲刺、射击等。
	/// 动画器参数：Grounded（布尔值）、xSpeed（浮点数）、ySpeed（浮点数），
	/// CollidingLeft（布尔值）、CollidingRight（布尔值）、CollidingBelow（布尔值）、CollidingAbove（布尔值）、Idle（布尔值）
	/// 随机：一个0到1之间的随机浮点数，每帧更新，有助于为你的状态入口转换添加变化，例如
	/// RandomConstant：一个在Start时生成的随机整数（0到1000之间），将在整个动画器的生命周期中保持不变，有助于使相同类型的不同角色
	/// </summary>
	[SelectionBase]
	[AddComponentMenu("TopDown Engine/Character/Core/Character")]
	public class Character : TopDownMonoBehaviour
	{
		/// the possible character types : player controller or AI (controlled by the computer)
		public enum CharacterTypes { Player, AI }

		[MMInformation("角色脚本是所有角色能力的必需基础。" +
			"你的角色可以是一个由AI控制的非玩家角色，或者是由玩家控制的玩家角色。" +
			"在这种情况下，你需要指定一个玩家ID，这个ID必须与你在输入管理器中指定的ID相匹配。" +
			"通常这些ID是Player1、Player2等。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		public CharacterTypes CharacterType = CharacterTypes.AI;

		public virtual CharacterStates CharacterState { get; protected set; }

        public int CharacterID;

		[Header("Input")]
		[Tooltip("本地玩家输入 ID（对应 InputActionMap: Player_{InputID}）。玩家选择界面可在运行时设置。")]
		public int InputID = 1;

        [Header("Plan B: Stable ID (Network/Replay)")]
		public bool HasStableIdOverride = false;
		public int StableIdOverride = 0;
		[MMReadOnly]
		public int StableIdRuntime = 0;

		/// 与此角色相关联的健康脚本，如果留空将自动获取
		[Tooltip("与此角色相关联的健康脚本，如果留空将自动获取")]
		public Health CharacterHealth;

		[Header("AI")]

		[Tooltip("如果这是一个高级AI，与此角色相关联的大脑。默认情况下，引擎会在此对象上选择一个，但如果你想的话，可以附加另一个")]
		public AIBrain CharacterBrain;

        [HideInInspector] public TimeWheel m_TimeWheel;

        public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;

		/// <summary>
		/// Plan B: 纯 C# 游戏逻辑模块（不继承 MonoBehaviour）
		/// 由 SimulationWorld 在 30Hz 驱动
		/// </summary>
		[HideInInspector] public CharacterSim _CharacterSim;

		/// <summary>
		/// 输入/连招检测模块（纯 C#）
		/// </summary>
		[HideInInspector] public NTSD.Game.CharacterInputModule _CharacterInput;
		[HideInInspector] public NTSD.Input.ActionSequenceDetectorModule _ActionSequenceDetector;

		/// <summary>
		/// Step D9: id_update 管理器（角色特定逻辑扩展点）
		/// 对应 FLF 的 $.id_update(...) 方法
		/// </summary>
		[HideInInspector] public CharacterIdUpdate _IdUpdate;

		[HideInInspector] public AbilitySystemComponent _AbilitySystemComponent;
		[HideInInspector] public UnitSettings _UnitSetting;
		[HideInInspector] public StateMachine _StateMachine;
		[HideInInspector] public CapsuleCollider2D col2D; // 2D碰撞体组件
		[HideInInspector] public LF2CharacterAnimator _LF2CharacterAnimator;
        [HideInInspector] public Rigidbody2D _Rigidbody2D; //刚体组件

        public DIRECTION _CharacterDirection;

        /// an object to use as the camera's point of focus and follow target
        public virtual GameObject CameraTarget { get; set; }
		/// the direction of the camera associated to this character
		public virtual Vector3 CameraDirection { get; protected set; }


		protected bool _abilitiesCachedOnce = false;
		protected TopDownController _controller;

		protected bool _animatorInitialized = false;
		protected bool _onReviveRegistered;
		protected Coroutine _conditionChangeCoroutine;
		protected CharacterStates.CharacterConditions _lastState;
		// Step D3: _transformVelocity 和 _thisPositionLastFrame 已移除（FixedUpdate 旁路已删除）

		// ==================== Scheme C: Module Lifecycle ====================
		protected readonly List<ICharacterModule> _modules = new List<ICharacterModule>(16);
		protected bool _modulesCollected = false;
		protected bool _modulesInitialized = false;
		protected bool _characterDataBound = false;
		protected int _boundCharacterId = int.MinValue;

		/// <summary>
		/// Collects Character modules from this GameObject and children, then calls ModuleSetup() on each module.
		/// </summary>
		protected virtual void BootstrapModules()
		{
			if (_modulesCollected) return;
			_modulesCollected = true;

			_modules.Clear();

			// Pure C# runtime modules (no prefab component required)
			_CharacterInput = new NTSD.Game.CharacterInputModule();
			_ActionSequenceDetector = new NTSD.Input.ActionSequenceDetectorModule();
			_modules.Add(_CharacterInput);
			_modules.Add(_ActionSequenceDetector);

			MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour == null) continue;
				if (ReferenceEquals(behaviour, this)) continue;
				if (behaviour is ICharacterModule module)
				{
					_modules.Add(module);
				}
			}

			_modules.Sort((a, b) => a.ModuleOrder.CompareTo(b.ModuleOrder));

			for (int i = 0; i < _modules.Count; i++)
			{
				try
				{
					_modules[i].ModuleSetup(this);
				}
				catch (Exception e)
				{
					Debug.LogError($"[Character] ModuleSetup failed: {_modules[i].GetType().Name} on {name}\n{e}", this);
				}
			}
		}


		/// <summary>
		/// Initializes this instance of the character
		/// </summary>
		protected virtual void Awake()
		{
			BootstrapModules();
			EnsureModulesInitialized();
		}

		protected virtual void InitAttribute()
		{
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitHP(HpMax);
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitMP(MpMax);
			_AbilitySystemComponent.AttrSet<AS_Fight>().InitPOSTURE(0);
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitATK(ATK);
			// ✅ 从 LF2CharacterData 读取初始速度（默认使用 walking_speed）
			if (_LF2CharacterAnimator != null && _LF2CharacterAnimator._FrameDataWrapper != null)
			{
				var characterData = _LF2CharacterAnimator._FrameDataWrapper.characterData;
				_AbilitySystemComponent.AttrSet<AS_Fight>().InitSPEED(characterData.walking_speed);
			}
			else
			{
				// 如果没有 LF2CharacterData，使用默认值
				_AbilitySystemComponent.AttrSet<AS_Fight>().InitSPEED(_UnitSetting.moveSpeed);
			}
		}

		/// <summary>
		/// Scheme C: one-time initialization (no CharacterID-driven data binding).
		/// </summary>
		public virtual void EnsureModulesInitialized()
		{
			if (_modulesInitialized) return;

			// we store our components for further use 
			CharacterState = new CharacterStates();

			_UnitSetting = this.GetComponent<UnitSettings>();
			_AbilitySystemComponent = this.gameObject.GetComponent<AbilitySystemComponent>();
			_AbilitySystemComponent.InitWithPreset(1);


			_controller = this.gameObject.GetComponent<TopDownController>();
			_StateMachine = this.gameObject.GetComponent<StateMachine>();
			// _CharacterInput / _ActionSequenceDetector are pure C# modules (created in BootstrapModules)
			_LF2CharacterAnimator = this.gameObject.GetComponentInChildren<LF2CharacterAnimator>();

			_CharacterDirection = DIRECTION.RIGHT;

			if(m_TimeWheel == null)
                m_TimeWheel = TimeWheel.CreateSharedInstance();

            if (CharacterHealth == null)
			{
				CharacterHealth = this.gameObject.GetComponent<Health>();
			}

			if (CharacterBrain == null)
			{
				CharacterBrain = this.gameObject.GetComponent<AIBrain>();
			}

			if (CharacterBrain != null)
			{
				CharacterBrain.Owner = this.gameObject;
			}

			// Step D1: 强制 StableId 在注册前稳定分配（禁止 fallback）
			if (HasStableIdOverride)
			{
				StableIdRuntime = StableIdOverride;
			}
			else
			{
				// Auto-allocate from World (for local AI)
				// Step D1: SimulationTickDriver 已通过 RuntimeInitializeOnLoadMethod 早期初始化
				// 如果这里仍然 null，说明出现严重错误，必须阻止继续执行
				if (SimulationTickDriver.Instance == null || SimulationTickDriver.Instance.World == null)
				{
					Debug.LogError($"[Character] CRITICAL: SimulationTickDriver.Instance not ready in Initialization()! " +
						$"Cannot allocate StableId for {gameObject.name}. " +
						$"This character will NOT be registered to SimulationWorld.", this);

					// Step D1: 禁止 fallback，直接返回，不创建 Sim 模块
					// 这会导致 _CharacterSim==null，OnEnable 会检测到并跳过注册
					return;
				}

				StableIdRuntime = SimulationTickDriver.Instance.World.AllocateStableId();
			}

			// Scheme C: initialize all modules (must not read CharacterID-driven data)
			for (int i = 0; i < _modules.Count; i++)
			{
				try
				{
					_modules[i].ModuleInitialize();
				}
				catch (Exception e)
				{
					Debug.LogError($"[Character] ModuleInitialize failed: {_modules[i].GetType().Name} on {name}\n{e}", this);
				}
			}

			// Step D1: 只有在成功分配 StableId 后才创建 Sim 模块
			// Plan B: Create CharacterSim module (pure C# gameplay logic)
			_CharacterSim = new CharacterSim(this);

			// Bind stable id into input/combos (deterministic ordering)
			_ActionSequenceDetector?.SetStableId(StableIdRuntime);
            _CharacterInput?.SetStableId(StableIdRuntime);

            // Step D9R: Create CharacterIdUpdate module (id_update 机制)
            // Hub 注入规则：由 Character 统一缓存依赖并注入
            var unitActions = this.GetComponent<BeatEmUpTemplate2D.UnitActions>();
			_IdUpdate = new CharacterIdUpdate(this, unitActions);

			// instantiate camera target
			if (CameraTarget == null)
			{
				CameraTarget = new GameObject();
			}
			CameraTarget.transform.SetParent(this.transform);
			CameraTarget.transform.localPosition = Vector3.zero;
			CameraTarget.name = "CameraTarget";

			_modulesInitialized = true;
		}

		protected virtual void Start()
		{
			// Default flow: bind CharacterID-driven data on Start (single player / non-custom spawners).
			// Multiplayer spawners can call ApplyCharacterID + EnsureCharacterDataBound() earlier.
			EnsureCharacterDataBound();
        }

		/// <summary>
		/// Scheme C: binds/rebinds all CharacterID-driven data across modules.
		/// This is safe to call multiple times; it only rebinds when CharacterID has changed or when not yet bound.
		/// </summary>
		public virtual void EnsureCharacterDataBound()
		{
			EnsureModulesInitialized();

			if (_characterDataBound && _boundCharacterId == CharacterID) return;

			// Rebind flow
			if (_characterDataBound)
			{
				for (int i = _modules.Count - 1; i >= 0; i--)
				{
					try
					{
						_modules[i].ModuleUnbind();
					}
					catch (Exception e)
					{
						Debug.LogError($"[Character] ModuleUnbind failed: {_modules[i].GetType().Name} on {name}\n{e}", this);
					}
				}
			}

			for (int i = 0; i < _modules.Count; i++)
			{
				try
				{
					_modules[i].ModuleBind();
				}
				catch (Exception e)
				{
					Debug.LogError($"[Character] ModuleBind failed: {_modules[i].GetType().Name} on {name}\n{e}", this);
				}
			}

			// Step D9R: 注册默认 handlers（对应 FLF character.js 的 id_updates 初始化）
			if (_IdUpdate != null)
			{
				_IdUpdate.RegisterDefaultHandlers(CharacterID);
			}

			// Bind-time attribute init (depends on CharacterID-driven data, e.g. LF2CharacterData)
			if (_AbilitySystemComponent != null)
			{
				InitAttribute();
			}

			_characterDataBound = true;
			_boundCharacterId = CharacterID;
		}

		/// <summary>
		/// 运行时切换角色配置 ID。
		/// Scheme C: setting CharacterID is cheap; data is re-bound via EnsureCharacterDataBound().
		/// </summary>
		public virtual void ApplyCharacterID(int newCharacterId)
		{
			if (newCharacterId == CharacterID) return;

			CharacterID = newCharacterId;

			// Runtime transform support: if already bound, immediately rebind CharacterID-driven data.
			if (_characterDataBound)
			{
				EnsureCharacterDataBound();
			}
		}

		/// <summary>
		/// Explicit transform API (alias to ApplyCharacterID + EnsureCharacterDataBound).
		/// </summary>
		public virtual void TransformToCharacterId(int newCharacterId)
		{
			ApplyCharacterID(newCharacterId);
			EnsureCharacterDataBound();
		}

		/// <summary>
		/// Forces a full rebind even if CharacterID didn't change (debug/reload).
		/// </summary>
		public virtual void ForceRebindCharacterData()
		{
			_characterDataBound = false;
			EnsureCharacterDataBound();
		}

		/// <summary>
		/// 临时改变角色状态并在指定时间后恢复原状态的方法。
		/// 也可以用于临时禁用重力，并可选择是否重置作用力。
		/// </summary>
		/// <param name="newCondition">要设置的新状态</param>
		/// <param name="duration">状态持续的时间</param>
		/// <param name="resetControllerForces">是否重置控制器作用力</param>
		/// <param name="disableGravity">是否禁用重力</param>
		public virtual void ChangeCharacterConditionTemporarily(CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			// 如果已有状态改变协程在运行，先停止它
			if (_conditionChangeCoroutine != null)
			{
				StopCoroutine(_conditionChangeCoroutine);
			}
			// 启动新的状态改变协程
			_conditionChangeCoroutine = StartCoroutine(ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces, disableGravity));
		}

		/// <summary>
		/// 处理临时状态改变的协程方法
		/// </summary>
		/// <param name="newCondition">新的角色状态</param>
		/// <param name="duration">状态持续时间</param>
		/// <param name="resetControllerForces">是否重置控制器作用力</param>
		/// <param name="disableGravity">是否禁用重力</param>
		/// <returns>协程迭代器</returns>
		protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
			CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			// 保存当前状态（如果新状态与当前状态不同）
			if (_lastState != newCondition) if ((_lastState != newCondition) && (this.ConditionState.CurrentState != newCondition))
				{
					_lastState = this.ConditionState.CurrentState;
				}

			// 改变到新状态
			this.ConditionState.ChangeState(newCondition);
			// 根据参数重置控制器作用力
			if (resetControllerForces) { _controller?.SetMovement(Vector2.zero); }
			// 根据参数禁用重力
			if (disableGravity && (_controller != null)) { _controller.GravityActive = false; }
			// 等待指定持续时间
			yield return MMCoroutine.WaitFor(duration);
			// 恢复到之前的状态
			this.ConditionState.ChangeState(_lastState);
			// 如果之前禁用了重力，重新启用
			if (disableGravity && (_controller != null)) { _controller.GravityActive = true; }
		}

		/// <summary>
		/// 存储相关的相机方向
		/// </summary>
		/// <param name="direction">相机方向向量</param>
		public virtual void SetCameraDirection(Vector3 direction)
		{
			CameraDirection = direction;
		}

		/// <summary>
		/// 冻结角色，使其无法移动
		/// </summary>
		public virtual void Freeze()
		{
			// 禁用重力
			_controller.SetGravityActive(false);
			// 停止移动
			_controller.SetMovement(Vector2.zero);
			// 将状态改为冻结
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
		}

		/// <summary>
		/// 解除角色的冻结状态
		/// </summary>
		public virtual void UnFreeze()
		{
			// 只有在当前状态是冻结状态时才执行解冻
			if (ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
			{
				// 重新启用重力
				_controller.SetGravityActive(true);
				// 恢复正常状态
				ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
		}

		/// <summary>
		/// 禁用角色（例如在关卡结束时使用）。
		/// 禁用后角色将无法移动和响应输入。
		/// </summary>
		public virtual void Disable()
		{
			// 禁用此组件
			this.enabled = false;
			// 禁用控制器
			_controller.enabled = false;
		}

		/// <summary>
		/// 当角色死亡时调用。
		/// 调用所有能力的Reset()方法，以便在需要时将设置恢复到原始值
		/// </summary>
		public virtual void Reset()
		{

		}

		/// <summary>
		/// 角色复活时，强制设置生成方向
		/// </summary>
		protected virtual void OnRevive()
		{
			// 如果角色大脑组件存在
			if (CharacterBrain != null)
			{
				// 启用大脑组件
				CharacterBrain.enabled = true;
				// 重置大脑状态
				CharacterBrain.ResetBrain();
			}
		}

		/// <summary>
		/// 角色死亡时的处理方法
		/// </summary>
		protected virtual void OnDeath()
		{
			// 如果角色大脑组件存在
			if (CharacterBrain != null)
			{
				// 清空大脑状态
				CharacterBrain.TransitionToState("");
				// 禁用大脑组件
				CharacterBrain.enabled = false;
			}
		}

		/// <summary>
		/// 角色受到伤害时的处理方法
		/// </summary>
		protected virtual void OnHit()
		{

		}

		/// <summary>
		/// 对象启用时，注册复活事件
		/// </summary>
		protected virtual void OnEnable()
		{
			// 如果角色生命值组件存在
			if (CharacterHealth != null)
			{
				// 如果复活事件还未注册
				if (!_onReviveRegistered)
				{
					// 注册复活事件
					CharacterHealth.OnRevive += OnRevive;
					_onReviveRegistered = true;
				}
				// 注册死亡事件
				CharacterHealth.OnDeath += OnDeath;
				// 注册受伤事件
				CharacterHealth.OnHit += OnHit;
			}

			// Plan B: Register CharacterSim to SimulationWorld
			if (_CharacterSim != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Register(_CharacterSim);
			}

			// Register combo detector (input consumer) to SimulationWorld
			if (_ActionSequenceDetector != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Register(_ActionSequenceDetector);
                SimulationTickDriver.Instance.World.Register(_CharacterInput);
            }
		}

		/// <summary>
		/// 对象禁用时，取消注册事件
		/// </summary>
		protected virtual void OnDisable()
		{
			// 如果角色生命值组件存在
			if (CharacterHealth != null)
			{
				// 取消注册死亡事件
				CharacterHealth.OnDeath -= OnDeath;
				// 取消注册受伤事件
				CharacterHealth.OnHit -= OnHit;
			}

			// Plan B: Unregister CharacterSim from SimulationWorld
			if (_CharacterSim != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Unregister(_CharacterSim);
			}

			if (_ActionSequenceDetector != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Unregister(_ActionSequenceDetector);
                SimulationTickDriver.Instance.World.Unregister(_CharacterInput);
            }
		}

	}
}
