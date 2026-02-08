using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using Com.LuisPedroFonseca.ProCamera2D;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A class that handles camera follow using ProCamera2D.
	/// Drop this on your camera's GameObject (alongside ProCamera2D) as a replacement for CinemachineCameraController.
	/// It listens to the same MMCameraEvent / TopDownEngineEvent events that the rest of the TopDown Engine fires.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Camera/ProCamera2D Camera Controller")]
	public class ProCamera2DCameraController : TopDownMonoBehaviour, MMEventListener<MMCameraEvent>, MMEventListener<TopDownEngineEvent>
	{
		/// Whether this camera is currently following the player
		public virtual bool FollowsPlayer { get; set; }

		[Header("ProCamera2D Camera Controller")]

		/// Whether this camera should follow a player
		[Tooltip("Whether this camera should follow a player")]
		public bool FollowsAPlayer = true;

		/// Whether to confine the camera to level bounds using ProCamera2D NumericBoundaries
		[Tooltip("Whether to confine the camera to level bounds via ProCamera2D NumericBoundaries")]
		public bool ConfineCameraToLevelBounds = true;

		/// If true, this controller will listen to SetConfiner events and update NumericBoundaries accordingly
		[Tooltip("If true, this controller will listen to SetConfiner events and update NumericBoundaries accordingly")]
		public bool ListenToSetConfinerEvents = true;

		/// Optional manual reference to the ProCamera2D instance. If null, will auto-find on this GameObject.
		[Tooltip("Optional manual reference to the ProCamera2D instance. If null, will auto-find on this GameObject.")]
		public ProCamera2D ProCamera2DInstance;

		/// The target character this camera should follow
		[MMReadOnly]
		[Tooltip("The target character this camera should follow")]
		public Character TargetCharacter;

		protected ProCamera2DNumericBoundaries _numericBoundaries;
		protected int _lastStopFollow = -1;
		protected bool _isFirstFollow = true;

		/// <summary>
		/// On Awake we grab our ProCamera2D components
		/// </summary>
		protected virtual void Awake()
		{
			if (ProCamera2DInstance == null)
			{
				ProCamera2DInstance = GetComponent<ProCamera2D>();
			}

			if (ProCamera2DInstance == null)
			{
				Debug.LogError("ProCamera2DCameraController: No ProCamera2D component found. Please add one to this GameObject or assign it manually.");
				return;
			}

			_numericBoundaries = GetComponent<ProCamera2DNumericBoundaries>();
		}

		/// <summary>
		/// On Start we apply level bounds to NumericBoundaries if configured
		/// </summary>
		protected virtual void Start()
		{
			if (ConfineCameraToLevelBounds)
			{
				ApplyLevelBounds();
			}
		}

		/// <summary>
		/// Finds the LevelLimits in the scene and applies them to ProCamera2D NumericBoundaries
		/// </summary>
		protected virtual void ApplyLevelBounds()
		{
			if (_numericBoundaries == null)
			{
				return;
			}

			LevelLimits limits = FindObjectOfType<LevelLimits>();
			if (limits != null)
			{
				ApplyBoundsToNumericBoundaries(limits.LeftLimit, limits.RightLimit, limits.BottomLimit, limits.TopLimit);
			}
		}

		/// <summary>
		/// Applies the given boundary values to the NumericBoundaries extension
		/// </summary>
		protected virtual void ApplyBoundsToNumericBoundaries(float left, float right, float bottom, float top)
		{
			if (_numericBoundaries == null)
			{
				return;
			}

			_numericBoundaries.Settings = new NumericBoundariesSettings
			{
				UseNumericBoundaries = true,
				UseLeftBoundary = true,
				LeftBoundary = left,
				UseRightBoundary = true,
				RightBoundary = right,
				UseBottomBoundary = true,
				BottomBoundary = bottom,
				UseTopBoundary = true,
				TopBoundary = top,
			};
		}

		/// <summary>
		/// Applies a Collider2D's bounds to the NumericBoundaries extension
		/// </summary>
		protected virtual void ApplyCollider2DBounds(Collider2D collider2D)
		{
			if (collider2D == null || _numericBoundaries == null)
			{
				return;
			}

			Bounds bounds = collider2D.bounds;
			ApplyBoundsToNumericBoundaries(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
		}

		/// <summary>
		/// Applies a 3D Collider's bounds to the NumericBoundaries extension (projected to 2D)
		/// </summary>
		protected virtual void ApplyCollider3DBounds(Collider collider)
		{
			if (collider == null || _numericBoundaries == null)
			{
				return;
			}

			Bounds bounds = collider.bounds;
			ApplyBoundsToNumericBoundaries(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
		}

		/// <summary>
		/// Sets the target character for this camera to follow
		/// </summary>
		public virtual void SetTarget(Character character)
		{
			TargetCharacter = character;
		}

		/// <summary>
		/// Starts following the target character
		/// </summary>
		public virtual void StartFollowing()
		{
			StartCoroutine(StartFollowingCo());
		}

		/// <summary>
		/// Coroutine that handles the start following logic, with a frame delay if stop was called on the same frame
		/// </summary>
		protected virtual IEnumerator StartFollowingCo()
		{
			if (_lastStopFollow > 0 && _lastStopFollow == Time.frameCount)
			{
				yield return null;
			}

			if (!FollowsAPlayer) { yield break; }
			if (TargetCharacter == null) { yield break; }
			if (ProCamera2DInstance == null) { yield break; }

			FollowsPlayer = true;

			// Remove existing targets to avoid duplicates
			ProCamera2DInstance.RemoveAllCameraTargets();

			// Add the character's camera target as a ProCamera2D target
			Transform targetTransform = TargetCharacter.CameraTarget.transform;
			ProCamera2DInstance.AddCameraTarget(targetTransform);

			// On first follow or after respawn, snap the camera instantly to the target
			if (_isFirstFollow)
			{
				ProCamera2DInstance.MoveCameraInstantlyToPosition(
					new Vector2(targetTransform.position.x, targetTransform.position.y)
				);
				_isFirstFollow = false;
			}

			ProCamera2DInstance.enabled = true;
		}

		/// <summary>
		/// Stops following any target
		/// </summary>
		public virtual void StopFollowing()
		{
			if (!FollowsAPlayer) { return; }
			if (ProCamera2DInstance == null) { return; }

			FollowsPlayer = false;
			ProCamera2DInstance.RemoveAllCameraTargets();
			_lastStopFollow = Time.frameCount;
		}

		/// <summary>
		/// Handles MMCameraEvent: SetTargetCharacter, StartFollowing, StopFollowing, RefreshPosition, SetConfiner, ResetPriorities
		/// </summary>
		public virtual void OnMMEvent(MMCameraEvent cameraEvent)
		{
			switch (cameraEvent.EventType)
			{
				case MMCameraEventTypes.SetTargetCharacter:
					SetTarget(cameraEvent.TargetCharacter);
					break;

				case MMCameraEventTypes.SetConfiner:
					if (ListenToSetConfinerEvents)
					{
						if (cameraEvent.Bounds2D != null)
						{
							ApplyCollider2DBounds(cameraEvent.Bounds2D);
						}
						else if (cameraEvent.Bounds != null)
						{
							ApplyCollider3DBounds(cameraEvent.Bounds);
						}
					}
					break;

				case MMCameraEventTypes.StartFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StartFollowing();
					break;

				case MMCameraEventTypes.StopFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StopFollowing();
					break;

				case MMCameraEventTypes.RefreshPosition:
					StartCoroutine(RefreshPosition());
					break;

				case MMCameraEventTypes.ResetPriorities:
					// ProCamera2D doesn't use priorities like Cinemachine.
					// No action needed.
					break;
			}
		}

		/// <summary>
		/// Refreshes the camera position by briefly disabling and re-enabling following
		/// </summary>
		protected virtual IEnumerator RefreshPosition()
		{
			if (ProCamera2DInstance != null)
			{
				ProCamera2DInstance.enabled = false;
			}
			yield return null;
			_isFirstFollow = true; // Force instant snap on next follow
			StartFollowing();
		}

		/// <summary>
		/// Handles TopDownEngineEvent: CharacterSwitch and CharacterSwap
		/// </summary>
		public virtual void OnMMEvent(TopDownEngineEvent topdownEngineEvent)
		{
			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwitch)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				StartFollowing();
			}

			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwap)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				MMCameraEvent.Trigger(MMCameraEventTypes.RefreshPosition);
			}
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMCameraEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMCameraEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
