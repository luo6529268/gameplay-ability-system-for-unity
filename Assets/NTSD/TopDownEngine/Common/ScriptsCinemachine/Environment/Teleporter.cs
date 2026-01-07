using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{	
	[AddComponentMenu("TopDown Engine/Environment/Teleporter")]
    /// <summary>
    /// Add this script to a trigger collider2D or collider to teleport objects from that object to its destination
    /// 将此脚本添加到触发器碰撞器2D或碰撞器上，以将对象从该对象传送到其目的地
    /// </summary>
    public class Teleporter : ButtonActivated 
	{
        /// the possible modes the teleporter can interact with the camera system on activation, either doing nothing, teleporting the camera to a new position, or blending between Cinemachine virtual cameras
        /// 当激活时，传送器可以与相机系统交互的可能模式，要么什么都不做，要么将相机传送到新位置，或者在Cinemachine虚拟相机之间混合
        public enum CameraModes { DoNothing, TeleportCamera, CinemachinePriority }
        /// the possible teleportation modes (either 1-frame instant teleportation, or tween between this teleporter and its destination)
        /// 可能的传送模式（要么是1帧即时传送，或者是在此传送器和其目的地之间进行渐变）
        public enum TeleportationModes { Instant, Tween }
        /// the possible time modes 
        /// 可能的时间模式
        public enum TimeModes { Unscaled, Scaled }

		[MMInspectorGroup("Teleporter", true, 18)]

        /// 如果为真，则不会传送非玩家角色
        [Tooltip("如果为真，则不会传送非玩家角色")]
        public bool OnlyAffectsPlayer = true;
        /// 退出此传送器时应用的偏移量
        [Tooltip("退出此传送器时应用的偏移量")]
        public Vector3 ExitOffset;
        /// 选定的传送模式
        [Tooltip("选定的传送模式")]
        public TeleportationModes TeleportationMode = TeleportationModes.Instant;
        /// 应用到传送渐变的曲线
        [MMEnumCondition("TeleportationMode", (int)TeleportationModes.Tween)]
        [Tooltip("应用到传送渐变的曲线")]
        public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInCubic;
        /// 是否在退出时保持被传送对象的x值
        [Tooltip("是否在退出时保持被传送对象的x值")]
        public bool MaintainXEntryPositionOnExit = false;
        /// 是否在退出时保持被传送对象的y值
        [Tooltip("是否在退出时保持被传送对象的y值")]
        public bool MaintainYEntryPositionOnExit = false;
        /// 是否在退出时保持被传送对象的z值
        [Tooltip("是否在退出时保持被传送对象的z值")]
        public bool MaintainZEntryPositionOnExit = false;

		[MMInspectorGroup("Destination", true, 19)]

        [Tooltip("传送器的目的地")]
        public Teleporter Destination;
        /// 如果为真，被传送对象将被放置在目的地的忽略列表上，以防止立即重新进入。如果你的目的地偏移量足够远离其中心，你可以将其设置为false
        [Tooltip("如果为真，被传送对象将被放置在目的地的忽略列表上，以防止立即重新进入。如果你的目的地偏移量足够远离其中心，你可以将其设置为false")]
        public bool AddToDestinationIgnoreList = true;


        [MMInspectorGroup("Rooms", true, 20)]

        /// 选择的相机模式
        [Tooltip("选择的相机模式")]
        public CameraModes CameraMode = CameraModes.TeleportCamera;
        /// 此传送器所属的房间
        [Tooltip("此传送器所属的房间")]
        public Room CurrentRoom;
        /// 目标房间
        [Tooltip("目标房间")]
        public Room TargetRoom;

        [MMInspectorGroup("MMFader Transtitions", true, 21)]

        /// 如果为真，传送时将发生淡入黑色
        [Tooltip("如果为真，传送时将发生淡入黑色")]
        public bool TriggerFade = false;
        /// 要定位的淡入淡出ID
        [MMCondition("TriggerFade", true)]
        [Tooltip("要定位的淡入淡出ID")]
        public int FaderID = 0;
        /// 用于淡入黑色的曲线
        [Tooltip("用于淡入黑色的曲线")]
        public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        /// 如果为真，淡入淡出事件将忽略时间缩放
        [Tooltip("如果为真，淡入淡出事件将忽略时间缩放")]
        public bool FadeIgnoresTimescale = false;

        [MMInspectorGroup("Mask", true, 22)]

        /// 是否应在激活时请求移动MMSpriteMask
        [Tooltip("是否应在激活时请求移动MMSpriteMask")]
        public bool MoveMask = true;
        /// 移动遮罩的曲线
        [MMCondition("MoveMask", true)]
        [Tooltip("移动遮罩的曲线")]
        public MMTween.MMTweenCurve MoveMaskCurve = MMTween.MMTweenCurve.EaseInCubic;
        /// 移动遮罩的方法
        [MMCondition("MoveMask", true)]
        [Tooltip("移动遮罩的方法")]
        public MMSpriteMaskEvent.MMSpriteMaskEventTypes MoveMaskMethod = MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition;
        /// 遮罩移动的持续时间（通常与DelayBetweenFades相同）
        [MMCondition("MoveMask", true)]
        [Tooltip("遮罩移动的持续时间（通常与DelayBetweenFades相同）")]
        public float MoveMaskDuration = 0.2f;

        [MMInspectorGroup("Freeze", true, 23)]
        /// 是否在过渡期间冻结时间
        [Tooltip("是否在过渡期间冻结时间")]
        public bool FreezeTime = false;
        /// 是否在过渡期间冻结角色（阻止输入）
        [Tooltip("是否在过渡期间冻结角色（阻止输入）")]
        public bool FreezeCharacter = true;

        [MMInspectorGroup("Teleport Sequence", true, 24)]

        /// 用于传送序列的时间缩放
        [Tooltip("用于传送序列的时间缩放")]
        public TimeModes TimeMode = TimeModes.Unscaled;
        /// 在运行序列前应用的延迟（以秒为单位）
        [Tooltip("在运行序列前应用的延迟（以秒为单位）")]
        public float InitialDelay = 0.1f;
        /// 在初始延迟后覆盖场景淡出的持续时间（以秒为单位）
        [Tooltip("在初始延迟后覆盖场景淡出的持续时间（以秒为单位）")]
        public float FadeOutDuration = 0.2f;
        /// 在淡出后和淡入前等待的持续时间（以秒为单位）
        [Tooltip("在淡出后和淡入前等待的持续时间（以秒为单位）")]
        public float DelayBetweenFades = 0.3f;
        /// 在初始延迟后覆盖场景淡入的持续时间（以秒为单位）
        [Tooltip("在初始延迟后覆盖场景淡入的持续时间（以秒为单位）")]
        public float FadeInDuration = 0.2f;
        /// 在场景淡入后的持续时间（以秒为单位）
        [Tooltip("在场景淡入后的持续时间（以秒为单位）")]
        public float FinalDelay = 0.1f;

        public virtual float LocalTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledTime : Time.time;
		public virtual float LocalDeltaTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledDeltaTime : Time.deltaTime;

		protected Character _player;
		protected Character _characterTester;
		protected List<Transform> _ignoreList;

		protected Vector3 _entryPosition;
		protected Vector3 _newPosition;

		/// <summary>
		/// On start we initialize our ignore list
		/// </summary>
		protected virtual void Awake()
		{
			InitializeTeleporter();
		}

        /// <summary>
        /// Grabs the current room in the parent if needed
        /// 如果需要，在父对象中获取当前房间
        /// </summary>
        protected virtual void InitializeTeleporter()
		{
			_ignoreList = new List<Transform>();
			if (CurrentRoom == null)
			{
				CurrentRoom = this.gameObject.GetComponentInParent<Room>();
			}
		}

        /// <summary>
        /// Triggered when something enters the teleporter
        /// 当有物体进入传送器时触发
        /// </summary>
        /// <param name="collider">Collider.</param>
        protected override void TriggerEnter(GameObject collider)
		{
            // if the object that collides with the teleporter is on its ignore list, we do nothing and exit.
            // 如果与传送器碰撞的对象在其忽略列表上，我们不做任何事情并退出。
            if (_ignoreList.Contains(collider.transform))
			{
				return;
			}

			_characterTester = collider.GetComponent<Character>();

			if (_characterTester != null)
			{
				if (RequiresPlayerType)
				{
					if (_characterTester.CharacterType != Character.CharacterTypes.Player)
					{
						return;
					}
				}

				_player = _characterTester;
			}

            // if the teleporter is supposed to only affect the player, we do nothing and exit
            //如果传送器应该只影响玩家，我们不做任何事情并退出
            if (OnlyAffectsPlayer || !AutoActivation)
			{
				base.TriggerEnter(collider);
			}
			else
			{
				base.TriggerButtonAction();
				Teleport(collider);
			}
		}

        /// <summary>
        /// If we're button activated and if the button is pressed, we teleport
        /// 如果我们是按钮激活的，并且按钮被按下，我们将进行传送
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			Teleport(_player.gameObject);
		}

        /// <summary>
        /// Teleports whatever enters the portal to a new destination
        /// 将进入传送门的物体传送到新目的地
        /// </summary>
        protected virtual void Teleport(GameObject collider)
		{
            if (collider.MMGetComponentNoAlloc<Character>() == null)
                return;

			_entryPosition = collider.transform.position;
            // if the teleporter has a destination, we move the colliding object to that destination
            //如果传送器有目的地，我们将碰撞物体移动到该目的地
            if (Destination != null)
			{
				StartCoroutine(TeleportSequence(collider));         
			}
		}

        /// <summary>
        /// Handles the teleport sequence (fade in, pause, fade out)
        ///  处理传送序列（淡入，暂停，淡出）
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual IEnumerator TeleportSequence(GameObject collider)
		{
			SequenceStart(collider);

			for (float timer = 0, duration = InitialDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }
            
			AfterInitialDelay(collider);

			for (float timer = 0, duration = FadeOutDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeOut(collider);
            
			for (float timer = 0, duration = DelayBetweenFades; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterDelayBetweenFades(collider);

			for (float timer = 0, duration = FadeInDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeIn(collider);

			for (float timer = 0, duration = FinalDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }

			SequenceEnd(collider);
		}

        /// <summary>
        /// Describes the events happening before the initial fade in
        /// 描述初始淡入前发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void SequenceStart(GameObject collider)
		{
			if (CameraMode == CameraModes.TeleportCamera)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			}

			if (FreezeCharacter && (_player != null))
			{
				_player.Freeze();
			}
		}

        /// <summary>
        /// Describes the events happening after the initial delay has passed
        /// 描述初始延迟过后发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void AfterInitialDelay(GameObject collider)
		{            
			if (TriggerFade)
			{
				MMFadeInEvent.Trigger(FadeOutDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

        /// <summary>
        /// Describes the events happening once the initial fade in is complete
        /// 描述初始淡入完成后发生的事件
        /// </summary>
        protected virtual void AfterFadeOut(GameObject collider)
		{   
			#if MM_CINEMACHINE || MM_CINEMACHINE3         
			TeleportCollider(collider);

			if (AddToDestinationIgnoreList)
			{
				Destination.AddToIgnoreList(collider.transform);
			}            
            
			if (CameraMode == CameraModes.CinemachinePriority)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
				MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, DelayBetweenFades);
			}

			if (CurrentRoom != null)
			{
				CurrentRoom.PlayerExitsRoom();
			}
            
			if (TargetRoom != null)
			{
				TargetRoom.PlayerEntersRoom();
				#if MM_CINEMACHINE || MM_CINEMACHINE3 
				if (TargetRoom.VirtualCamera != null)
				{
					TargetRoom.VirtualCamera.Priority = 10;	
				}
				#endif
				MMSpriteMaskEvent.Trigger(MoveMaskMethod, (Vector2)TargetRoom.RoomColliderCenter, TargetRoom.RoomColliderSize, MoveMaskDuration, MoveMaskCurve);
			}
			#endif
		}

        /// <summary>
        /// Teleports the object going through the teleporter, either instantly or by tween
        /// 传送通过传送器的对象，无论是立即传送还是渐变传送
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void TeleportCollider(GameObject collider)
		{
			_newPosition = Destination.transform.position + Destination.ExitOffset;
			if (MaintainXEntryPositionOnExit)
			{
				_newPosition.x = _entryPosition.x;
			}
			if (MaintainYEntryPositionOnExit)
			{
				_newPosition.y = _entryPosition.y;
			}
			if (MaintainZEntryPositionOnExit)
			{
				_newPosition.z = _entryPosition.z;
			}

			switch (TeleportationMode)
			{
				case TeleportationModes.Instant:
					collider.transform.position = _newPosition;
					_ignoreList.Remove(collider.transform);
					break;
				case TeleportationModes.Tween:
					StartCoroutine(TeleportTweenCo(collider, collider.transform.position, _newPosition));
					break;
			}
		}

        /// <summary>
        /// Tweens the object from origin to destination
        /// 从原点到目的地渐变对象
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected virtual IEnumerator TeleportTweenCo(GameObject collider, Vector3 origin, Vector3 destination)
		{
			float startedAt = LocalTime;
			while (LocalTime - startedAt < DelayBetweenFades)
			{
				float elapsedTime = LocalTime - startedAt;
				collider.transform.position = MMTween.Tween(elapsedTime, 0f, DelayBetweenFades, origin, destination, TweenCurve);
				yield return null;
			}
			_ignoreList.Remove(collider.transform);
		}

        /// <summary>
        /// Describes the events happening after the pause between the fade in and the fade out
        /// 描述淡入和淡出之间的暂停后发生的事件
        /// </summary>
        protected virtual void AfterDelayBetweenFades(GameObject collider)
		{
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			if (TriggerFade)
			{
				MMFadeOutEvent.Trigger(FadeInDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

        /// <summary>
        /// Describes the events happening after the fade in of the scene\
        /// 描述场景淡入后发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void AfterFadeIn(GameObject collider)
		{

		}

        /// <summary>
        /// Describes the events happening after the fade out is complete, so at the end of the teleport sequence
        /// 描述淡出完成后发生的事件，即传送序列结束时
        /// </summary>
        protected virtual void SequenceEnd(GameObject collider)
		{
			if (FreezeCharacter && (_player != null))
			{
				_player.UnFreeze();
			}

			
			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

        /// <summary>
        /// When something exits the teleporter, if it's on the ignore list, we remove it from it, so it'll be considered next time it enters.
        /// 当物体退出传送器时，如果它在忽略列表上，我们将其从列表中移除，以便下次进入时会考虑它。
        /// </summary>
        /// <param name="collider">Collider.</param>
        public override void TriggerExitAction(GameObject collider)
		{
			if (_ignoreList.Contains(collider.transform))
			{
				_ignoreList.Remove(collider.transform);
			}
			base.TriggerExitAction(collider);
		}

        /// <summary>
        /// Adds an object to the ignore list, which will prevent that object to be moved by the teleporter while it's in that list
        /// 将对象添加到忽略列表，这将阻止该对象在列表上时被传送器移动
        /// </summary>
        /// <param name="objectToIgnore">Object to ignore.</param>
        public virtual void AddToIgnoreList(Transform objectToIgnore)
		{
			if (!_ignoreList.Contains(objectToIgnore))
			{
				_ignoreList.Add(objectToIgnore);
			}            
		}

        /// <summary>
        /// On draw gizmos, we draw arrows to the target destination and target room if there are any
        /// 在绘制gizmos时，如果有任何目标目的地和目标房间，我们绘制箭头指向它们
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (Destination != null)
			{
                // draws an arrow from this teleporter to its destination
                //从这个传送器到其目的地绘制箭头
                MMDebug.DrawGizmoArrow(this.transform.position, (Destination.transform.position + Destination.ExitOffset) - this.transform.position, Color.cyan, 1f, 25f);
                // draws a point at the exit position 
                //在退出位置绘制点 
                MMDebug.DebugDrawCross(this.transform.position + ExitOffset, 0.5f, Color.yellow);
				MMDebug.DrawPoint(this.transform.position + ExitOffset, Color.yellow, 0.5f);
			}

			if (TargetRoom != null)
			{
                // draws an arrow to the destination room
                //绘制指向目标房间的箭头
                MMDebug.DrawGizmoArrow(this.transform.position, TargetRoom.transform.position - this.transform.position, MMColors.Pink, 1f, 25f);
			}
		}
	}
}