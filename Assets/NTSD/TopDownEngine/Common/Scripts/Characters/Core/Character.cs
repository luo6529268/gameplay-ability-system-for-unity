using BeatEmUpTemplate2D;
using GAS.Runtime;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
using NTSD.Game;
using NTSD.TimeWheel;
using NTSD.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NTSD.Input;

namespace MoreMountains.TopDownEngine
{
	// ==================== Step 1: UnitActions 依赖拆分 ====================
	// Target / Grounding 数据承接（替代 UnitActions.target / groundPos / isGrounded）
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

        public TimeWheel m_TimeWheel { get; private set; }

        public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;

		/// <summary>
		/// Plan B: 纯 C# 游戏逻辑模块（不继承 MonoBehaviour）
		/// 由 SimulationWorld 在 30Hz 驱动
		/// </summary>
		public CharacterSim _CharacterSim { get; private set; }

        /// <summary>
        /// 输入/连招检测模块（纯 C#）
        /// </summary>
        public CharacterInputModule _CharacterInput { get; private set; }
        public ActionSequenceDetectorModule _ActionSequenceDetector { get; private set; }

        /// <summary>
        /// Step D9: id_update 管理器（角色特定逻辑扩展点）
        /// 对应 FLF 的 $.id_update(...) 方法
        /// </summary>
        public CharacterIdUpdate _IdUpdate { get; private set; }
		public Transform _ModeTrans { get; private set; }
		public AbilitySystemComponent _AbilitySystemComponent { get; private set; }
        public UnitSettings _UnitSetting { get; private set; }
        /// <summary>
        /// 角色专用逻辑模块（纯 C#，对应 FLF character.js）
        /// </summary>
        public LF2Character _LF2Character { get; private set; }

		/// <summary>
		/// 精灵动画模块（纯 C#，对应 FLF sprite.js）
		/// </summary>
		public LF2Sprite _LF2Sprite { get; private set; }

		// ==================== Step 1: Target / Grounding 数据承接 ====================
		/// <summary>
		/// 当前目标对象（替代 UnitActions.target）
		/// UnitSettings/AI 从这里读取 target
		/// </summary>
		public GameObject Target { get; private set; }

		/// <summary>
		/// 地面世界 Y 坐标（替代 UnitActions.groundPos）
		/// 由 LF2DynamicsApplier 通过 SetGrounding 写入
		/// </summary>
		public float GroundWorldY { get; private set; }

		/// <summary>
		/// 是否在地面上（替代 UnitActions.isGrounded）
		/// 由 LF2DynamicsApplier 通过 SetGrounding 写入
		/// </summary>
		public bool IsGrounded { get; private set; } = true;

		/// <summary>
		/// 设置地面状态（由 LF2DynamicsApplier 调用）
		/// </summary>
		public void SetGrounding(float groundWorldY, bool isGrounded)
		{
			GroundWorldY = groundWorldY;
			IsGrounded = isGrounded;
		}

        /// an object to use as the camera's point of focus and follow target
        public virtual GameObject CameraTarget { get; set; }
		/// the direction of the camera associated to this character
		public virtual Vector3 CameraDirection { get; protected set; }


		protected TopDownController _controller;

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


			_CharacterInput = new CharacterInputModule();
			_ActionSequenceDetector = new ActionSequenceDetectorModule();

			// 初始化纯 C# 模块
			_LF2Character = new LF2Character(this);
			_LF2Sprite = new LF2Sprite();

            HandleInitModulesInternal();
        }

		private void HandleInitModulesInternal() 
		{
            _modules.Add(_CharacterInput);
            _modules.Add(_ActionSequenceDetector);


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
		}

		protected virtual void InitAttribute()
		{
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitHP(HpMax);
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitMP(MpMax);
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitPOSTURE(0);
			//_AbilitySystemComponent.AttrSet<AS_Fight>().InitATK(ATK);
			//if (_LF2CharacterAnimator != null && _LF2CharacterAnimator._FrameDataWrapper != null)
			//{
			//	var characterData = _LF2CharacterAnimator._FrameDataWrapper.characterData;
			//	_AbilitySystemComponent.AttrSet<AS_Fight>().InitSPEED(characterData.walking_speed);
			//}
			//else
			//{
			//	// 如果没有 LF2CharacterData，使用默认值
			//	_AbilitySystemComponent.AttrSet<AS_Fight>().InitSPEED(_UnitSetting.moveSpeed);
			//}
		}

		/// <summary>
		/// Scheme C: one-time initialization (no CharacterID-driven data binding).
		/// </summary>
		public virtual void EnsureModulesInitialized()
		{
			if (_modulesInitialized) return;

			_UnitSetting = this.GetComponent<UnitSettings>();
			_AbilitySystemComponent = this.gameObject.GetComponent<AbilitySystemComponent>();
			_controller = this.gameObject.GetComponent<TopDownController>();
            _ModeTrans = this.gameObject.GetComponentInChildren<SpriteRenderer>().transform;

            _AbilitySystemComponent.InitWithPreset(1);

			m_TimeWheel = TimeWheel.CreateSharedInstance();

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

			// 初始化 LF2Character 模块（在 ICharacterModule 之后）
			if (_LF2Character != null)
			{
				var sprites = CharacterAnimtorManager.Instance?.GetCharacterSpriteByID(CharacterID);
                SpriteRenderer _SpriteRenderer = this.gameObject.GetComponentInChildren<SpriteRenderer>();
                _LF2Character.ModuleInitialize(
					spriteRenderer: _SpriteRenderer,
					sprites: sprites,
					baseLocalPosition: _SpriteRenderer.transform.localPosition
				);
			}

			// Step D1: 只有在成功分配 StableId 后才创建 Sim 模块
			// Plan B: Create CharacterSim module (pure C# gameplay logic)
			_CharacterSim = new CharacterSim(this);

			// Bind stable id into input/combos (deterministic ordering)
			_ActionSequenceDetector?.SetStableId(StableIdRuntime);

            // Step D9R: Create CharacterIdUpdate module (id_update 机制)
            // Hub 注入规则：由 Character 统一缓存依赖并注入
			_IdUpdate = new CharacterIdUpdate(this);

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

			// 绑定 LF2Character 模块（在 ICharacterModule.ModuleBind 之后）
			if (_LF2Character != null)
			{
                var frameDataWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(CharacterID);
                if (frameDataWrapper != null)
				{
					_LF2Character.ModuleBind(frameDataWrapper, CharacterID);
				}
			}

			// 初始化 LF2Character 模块（在 ModuleBind 之后，确保 LF2CharacterAnimator 已绑定）
			if (_LF2Character != null)
			{
				_LF2Character.Initialize(NTSDConstants.DEFAULT_MAX_HP, NTSDConstants.DEFAULT_MAX_MP);
			}

			// Step D9R: 注册默认 handlers（对应 FLF character.js 的 id_updates 初始化）
			if (_IdUpdate != null)
			{
				_IdUpdate.RegisterDefaultHandlers(CharacterID);
			}

			// 在此处注册到 SimulationWorld（时序确定：SimulationTickDriver.Awake 先于 StartLevel）
			if (SimulationTickDriver.Instance != null)
			{
				if (_ActionSequenceDetector != null)
					SimulationTickDriver.Instance.World.Register(_ActionSequenceDetector);
				// _LF2Character 已在 ModuleBind 内部注册
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
		/// 对象启用时，注册复活事件
		/// </summary>
		protected virtual void OnEnable()
		{
			// 补充注册：角色被重新激活时（首次激活已由 Initialization 的 ModuleBind 阶段处理）
			if (SimulationTickDriver.Instance == null) return;

			// Plan B: Register CharacterSim to SimulationWorld
			if (_CharacterSim != null)
				SimulationTickDriver.Instance.World.Register(_CharacterSim);

			// Register combo detector (input consumer) to SimulationWorld
			if (_ActionSequenceDetector != null)
				SimulationTickDriver.Instance.World.Register(_ActionSequenceDetector);

			if (_LF2Character != null)
				SimulationTickDriver.Instance.World.Register(_LF2Character);
		}

		/// <summary>
		/// 对象禁用时，取消注册事件
		/// </summary>
		protected virtual void OnDisable()
		{
			// Plan B: Unregister CharacterSim from SimulationWorld
			if (_CharacterSim != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Unregister(_CharacterSim);
			}

			if (_ActionSequenceDetector != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Unregister(_ActionSequenceDetector);
            }

			if (_LF2Character != null && SimulationTickDriver.Instance != null)
			{
				SimulationTickDriver.Instance.World.Unregister(_LF2Character);
			}
		}

	}
}
