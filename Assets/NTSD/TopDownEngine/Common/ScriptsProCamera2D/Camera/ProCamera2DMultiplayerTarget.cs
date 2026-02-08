using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using Com.LuisPedroFonseca.ProCamera2D;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Automatically adds all LevelManager players as ProCamera2D targets on load.
	/// Drop this on the same GameObject as your ProCamera2D component.
	/// This is the ProCamera2D equivalent of MultiplayerCameraGroupTarget.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Camera/ProCamera2D Multiplayer Target")]
	public class ProCamera2DMultiplayerTarget : TopDownMonoBehaviour, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
	{
		[Header("ProCamera2D Multiplayer Target")]

		/// Optional manual reference to the ProCamera2D instance. If null, will auto-find on this GameObject.
		[Tooltip("Optional manual reference to the ProCamera2D instance. If null, will auto-find on this GameObject.")]
		public ProCamera2D ProCamera2DInstance;

		/// The default influence each player target has on the camera
		[Tooltip("The default influence each player target has on the camera")]
		[Range(0f, 1f)]
		public float DefaultTargetInfluence = 1f;

		/// The duration (in seconds) for influence transitions when a player dies or respawns. 0 = instant.
		[Tooltip("The duration (in seconds) for influence transitions when a player dies or respawns. 0 = instant.")]
		public float InfluenceTransitionDuration = 0.5f;

		/// A dictionary mapping player characters to their camera target transforms for quick lookup
		protected Dictionary<Character, Transform> _playerTargetMap = new Dictionary<Character, Transform>();

		/// <summary>
		/// On Awake we grab our ProCamera2D component
		/// </summary>
		protected virtual void Awake()
		{
			if (ProCamera2DInstance == null)
			{
				ProCamera2DInstance = GetComponent<ProCamera2D>();
			}

			if (ProCamera2DInstance == null)
			{
				Debug.LogError("ProCamera2DMultiplayerTarget: No ProCamera2D component found. Please add one to this GameObject or assign it manually.");
			}
		}

		/// <summary>
		/// On the "Load" game event, adds all players as ProCamera2D targets
		/// </summary>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == "Load")
			{
				SetupMultiplayerTargets();
			}
		}

		/// <summary>
		/// Adds all LevelManager players as camera targets
		/// </summary>
		protected virtual void SetupMultiplayerTargets()
		{
			if (ProCamera2DInstance == null) { return; }
			if (!LevelManager.HasInstance) { return; }
			if (LevelManager.Instance.Players == null || LevelManager.Instance.Players.Count == 0) { return; }

			// Clear existing targets
			ProCamera2DInstance.RemoveAllCameraTargets();
			_playerTargetMap.Clear();

			// Add each player as a camera target
			foreach (Character character in LevelManager.Instance.Players)
			{
				if (character == null || character.CameraTarget == null) { continue; }

				Transform targetTransform = character.CameraTarget.transform;
				ProCamera2DInstance.AddCameraTarget(targetTransform, DefaultTargetInfluence, DefaultTargetInfluence);
				_playerTargetMap[character] = targetTransform;
			}
		}

		/// <summary>
		/// On player death, sets that player's camera target influence to 0 so the camera
		/// focuses on the remaining alive players.
		/// On respawn complete, restores all player influences.
		/// </summary>
		public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			if (tdEvent.EventType == TopDownEngineEventTypes.PlayerDeath)
			{
				if (ProCamera2DInstance == null) { return; }
				if (!LevelManager.HasInstance) { return; }

				foreach (Character character in LevelManager.Instance.Players)
				{
					if (character == null) { continue; }

					if (character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
					{
						if (_playerTargetMap.TryGetValue(character, out Transform targetTransform))
						{
							// Set influence to 0 for dead players
							ProCamera2DInstance.AdjustCameraTargetInfluence(targetTransform, 0f, 0f, InfluenceTransitionDuration);
						}
					}
				}
			}

			// On respawn complete, restore all player influences
			if (tdEvent.EventType == TopDownEngineEventTypes.RespawnComplete)
			{
				if (ProCamera2DInstance == null) { return; }

				foreach (var kvp in _playerTargetMap)
				{
					if (kvp.Key != null && kvp.Value != null)
					{
						ProCamera2DInstance.AdjustCameraTargetInfluence(kvp.Value, DefaultTargetInfluence, DefaultTargetInfluence, InfluenceTransitionDuration);
					}
				}
			}
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
