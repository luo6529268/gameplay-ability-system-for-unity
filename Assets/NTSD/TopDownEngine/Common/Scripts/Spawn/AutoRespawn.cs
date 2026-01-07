using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this script to an object and it will automatically be reactivated and revived when the player respawns.
    /// 将此脚本添加到对象上，当玩家复活时，它将自动重新激活并复活。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Auto Respawn")]
	public class AutoRespawn : TopDownMonoBehaviour, Respawnable 
	{
		[Header("Respawn when the player respawns")]
        /// 如果为真，当玩家复活时，此对象将在其最后位置重生
        [Tooltip("如果为真，当玩家复活时，此对象将在其最后位置重生")]
        public bool RespawnOnPlayerRespawn = true;
        /// 如果为真，当玩家复活时，此对象将被重新定位到其初始位置
        [Tooltip("如果为真，当玩家复活时，此对象将被重新定位到其初始位置")]
        public bool RepositionToInitOnPlayerRespawn = false;
        /// 如果为真，在击杀时此对象上的所有组件将被禁用
        [Tooltip("如果为真，在击杀时此对象上的所有组件将被禁用")]
        public bool DisableAllComponentsOnKill = false;
        /// 如果为真，在击杀时此游戏对象将被禁用
        [Tooltip("如果为真，在击杀时此游戏对象将被禁用")]
        public bool DisableGameObjectOnKill = true;

        [Header("Checkpoints")]
        /// 如果为真，无论是否与检查点关联，对象都将始终重生
        [Tooltip("如果为真，无论是否与检查点关联，对象都将始终重生")]
        public bool IgnoreCheckpointsAlwaysRespawn = true;
        /// 如果玩家在这些检查点复活，对象将被重生
        [Tooltip("如果玩家在这些检查点复活，对象将被重生")]
        public List<CheckPoint> AssociatedCheckpoints;

        [Header("Auto respawn after X seconds")]
        /// 如果这个值大于0，此对象将在死亡后X秒在其最后位置重生
        [Tooltip("如果这个值大于0，此对象将在死亡后X秒在其最后位置重生")]
        public float AutoRespawnDuration = 0f;
        /// 此对象可以自动重生的次数
        [Tooltip("此对象可以自动重生的次数，负值：无限")]
        public int AutoRespawnAmount = 3;
        /// 剩余重生次数（只读，由类在运行时控制）
        [Tooltip("剩余重生次数（只读，由类在运行时控制）")]
        [MMReadOnly]
        public int AutoRespawnRemainingAmount = 3;
        /// 当玩家复活时实例化的特效
        [Tooltip("当玩家复活时实例化的特效")]
        public GameObject RespawnEffect;
        /// 当玩家复活时播放的音效
        [Tooltip("当玩家复活时播放的音效")]
        public AudioClip RespawnSfx;

        [FormerlySerializedAs("OnRespawn")]
		[Header("Events")]
        /// 重生时触发的Unity事件
        [Tooltip("重生时触发的Unity事件")]
        public UnityEvent OnReviveEvent;

        // respawn
        public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

		protected MonoBehaviour[] _otherComponents;
		protected Collider2D _collider2D;
		protected Renderer _renderer;
		protected Character _character;
		protected Health _health;
		protected bool _reviving = false;
		protected float _timeOfDeath = 0f;
		protected bool _firstRespawn = true;
		protected Vector3 _initialPosition;
		protected AIBrain _aiBrain;

		/// <summary>
		/// On Start we grab our various components
		/// </summary>
		protected virtual void Start()
		{
			AutoRespawnRemainingAmount = AutoRespawnAmount;
			_otherComponents = this.gameObject.GetComponents<MonoBehaviour>() ;
			_collider2D = this.gameObject.GetComponent<Collider2D> ();
			_renderer = this.gameObject.GetComponent<Renderer> ();
			_character = this.gameObject.GetComponent<Character>();
			if (_character != null)
			{
				_health = _character.CharacterHealth;
			}
			_aiBrain = this.gameObject.GetComponent<AIBrain>();
			if ((_aiBrain == null) && (_character != null))
			{
				_aiBrain = _character.CharacterBrain;
			}
			_initialPosition = this.transform.position;
		}

        /// <summary>
        /// When the player respawns, we reinstate this agent.
        /// 当玩家复活时，我们重新激活这个代理。
        /// </summary>
        /// <param name="checkpoint">Checkpoint.</param>
        /// <param name="player">Player.</param>
        public virtual void OnPlayerRespawn (CheckPoint checkpoint, Character player)
		{
			if (RepositionToInitOnPlayerRespawn)
			{
				this.transform.position = _initialPosition;				
			}

			if (RespawnOnPlayerRespawn)
			{
				if (_health != null)
				{
					_health.Revive();
				}
				Revive ();
			}
			AutoRespawnRemainingAmount = AutoRespawnAmount;
		}

		/// <summary>
		/// On Update we check whether we should be reviving this agent
		/// </summary>
		protected virtual void Update()
		{
			if (_reviving)
			{
				if (_timeOfDeath + AutoRespawnDuration < Time.time)
				{
					if (AutoRespawnAmount == 0)
					{
						return;
					}
					if (AutoRespawnAmount > 0)
					{
						if (AutoRespawnRemainingAmount <= 0)
						{
							return;
						}
						AutoRespawnRemainingAmount -= 1;
					}
					Revive ();
					_reviving = false;
				}
			}
		}

		/// <summary>
		/// Kills this object, turning its parts off based on the settings set in the inspector
		/// </summary>
		public virtual void Kill()
		{
			if (AutoRespawnDuration <= 0f)
			{
				// object is turned inactive to be able to reinstate it at respawn
				if (DisableGameObjectOnKill)
				{
					gameObject.SetActive(false);	
				}
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						if (component != this)
						{
							component.enabled = false;
						}
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = false;	}
				if (_renderer != null)	{ _renderer.enabled = false; }
				_reviving = true;
				_timeOfDeath = Time.time;
			}
		}

		/// <summary>
		/// Revives this object, turning its parts back on again
		/// </summary>
		public virtual void Revive()
		{
			if (AutoRespawnDuration <= 0f)
			{
				// object is turned inactive to be able to reinstate it at respawn
				gameObject.SetActive(true);
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						component.enabled = true;
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = true;	}
				if (_renderer != null)	{ _renderer.enabled = true; }
				InstantiateRespawnEffect ();
				PlayRespawnSound ();
			}
			if (_health != null)
			{
				_health.Revive();
			}
			if (_aiBrain != null)
			{
				_aiBrain.ResetBrain();
			}
			OnRevive?.Invoke();
			if (OnReviveEvent != null)
			{
				OnReviveEvent.Invoke();
			}
		}

		/// <summary>
		/// Instantiates the respawn effect at the object's position
		/// </summary>
		protected virtual void InstantiateRespawnEffect()
		{
			// instantiates the destroy effect
			if (RespawnEffect != null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(RespawnEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}

		/// <summary>
		/// Plays the respawn sound.
		/// </summary>
		protected virtual void PlayRespawnSound()
		{
			if (RespawnSfx != null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(RespawnSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}
	}
}