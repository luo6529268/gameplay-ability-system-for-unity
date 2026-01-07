using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 多人游戏关卡管理器，用于处理多人游戏场景（特别是生成点和相机模式）
	/// 建议扩展此类以实现您自己的特定游戏规则
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/Multiplayer Level Manager")]
	public class MultiplayerLevelManager : LevelManager
	{
		/// <summary>
		/// 角色ID选择模式枚举
		/// 用于定义在不同场景下如何选择或覆盖角色ID的策略
		/// </summary>
		public enum CharacterIdSelectionModes
		{
			/// <summary>
			/// 使用预制体中的值
			/// 表示直接使用预设的角色ID，不进行任何覆盖
			/// </summary>
			UsePrefabValue = 0,

			/// <summary>
			/// 覆盖所有玩家
			/// 表示统一设置所有玩家的角色ID，忽略各自的设置
			/// </summary>
			OverrideAllPlayers = 1,

			/// <summary>
			/// 按玩家索引覆盖
			/// 表示根据玩家索引单独设置每个玩家的角色ID
			/// </summary>
			OverrideByPlayerIndex = 2
		}


		[Header("角色选择（CharacterID）")]
		[Tooltip("决定是否覆盖生成出来的 Character.CharacterID（用于选角）。\n" +
		         "Scheme C：CharacterID 的数据绑定由 Character 模块生命周期驱动（支持运行时变身）。")]
		public CharacterIdSelectionModes CharacterIdSelectionMode = CharacterIdSelectionModes.UsePrefabValue;

		[Tooltip("当模式为 OverrideAllPlayers 时使用的 CharacterID")]
		public int OverrideCharacterId = 0;

		[Tooltip("当模式为 OverrideByPlayerIndex 时使用：下标=玩家索引(0/1/2..)，值=-1 表示不覆盖，其他值表示强制 CharacterID")]
		public List<int> OverrideCharacterIdsByPlayerIndex = new List<int>();

		[Header("多人游戏生成点")]
		/// <summary>
		/// 用于生成角色的检查点列表（按顺序）
		/// 该列表决定了玩家在游戏中的出生点位置
		/// </summary>
		[Tooltip("用于生成角色的检查点列表（按顺序）")]
		public List<CheckPoint> SpawnPoints;
		
		/// <summary>
		/// 相机模式枚举类型
		/// 定义了游戏中可用的相机模式
		/// </summary>
		public enum CameraModes { Split, Group }

		[Header("相机设置")]
		/// <summary>
		/// 选定的相机模式
		/// 分组模式：所有目标在一个屏幕中显示
		/// 分屏模式：每个目标独立显示在各自的屏幕区域
		/// </summary>
		[Tooltip("选定的相机模式（分组模式：所有目标在一个屏幕中显示，或分屏模式：每个目标独立显示在各自的屏幕区域）")]
		public CameraModes CameraMode = CameraModes.Split;
		
		/// <summary>
		/// 分组相机装备
		/// 用于在分组模式下控制相机行为和显示
		/// </summary>
		[Tooltip("分组相机装备")]
		public GameObject GroupCameraRig;
		
		/// <summary>
		/// 分屏相机装备
		/// 用于在分屏模式下控制各个玩家的相机显示
		/// </summary>
		[Tooltip("分屏相机装备")]
		public GameObject SplitCameraRig;

		[Header("GUI管理器")]
		/// <summary>
		/// 多人游戏GUI管理器
		/// 负责管理多人游戏中的图形用户界面显示和交互
		/// </summary>
		[Tooltip("多人游戏GUI管理器")]
		public MultiplayerGUIManager MPGUIManager;



		/// <summary>
		/// 在Awake时处理不同的相机模式
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			HandleCameraModes();
		}

		/// <summary>
		/// 设置场景以匹配选定的相机模式
		/// </summary>
		protected virtual void HandleCameraModes()
		{
			// 处理分屏模式
			if (CameraMode == CameraModes.Split)
			{
				if (GroupCameraRig != null) { GroupCameraRig.SetActive(false); }
				if (SplitCameraRig != null) { SplitCameraRig.SetActive(true); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(true);
					MPGUIManager.GroupHUD?.SetActive(false);
					MPGUIManager.SplittersGUI?.SetActive(true);
				}
			}
			// 处理分组模式
			if (CameraMode == CameraModes.Group)
			{
				if (GroupCameraRig != null) { GroupCameraRig?.SetActive(true); }
				if (SplitCameraRig != null) { SplitCameraRig?.SetActive(false); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(false);
					MPGUIManager.GroupHUD?.SetActive(true);
					MPGUIManager.SplittersGUI?.SetActive(false);
				}
			}
		}

		/// <summary>
		/// 在指定的生成点生成所有角色
		/// </summary>
		protected override void SpawnMultipleCharacters()
		{
			// 遍历所有玩家并在对应的生成点生成
			for (int i = 0; i < Players.Count; i++)
			{
				SpawnPoints[i].SpawnPlayer(Players[i]);
				             
			}
			// 触发生成完成事件
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
		}

		/// <summary>
		/// 实例化可玩角色，并在需要时覆盖 CharacterID（选角）
		/// </summary>
		protected override void InstantiatePlayableCharacters()
		{
			base.InstantiatePlayableCharacters();

			if (Players == null || Players.Count == 0) return;

			for (int i = 0; i < Players.Count; i++)
			{
				Character player = Players[i];
				if (player == null) continue;

				int targetId = -1;
				if (CharacterIdSelectionMode != CharacterIdSelectionModes.UsePrefabValue)
				{
					switch (CharacterIdSelectionMode)
					{
						case CharacterIdSelectionModes.OverrideAllPlayers:
							targetId = OverrideCharacterId;
							break;

						case CharacterIdSelectionModes.OverrideByPlayerIndex:
							targetId = (OverrideCharacterIdsByPlayerIndex != null && i < OverrideCharacterIdsByPlayerIndex.Count)
								? OverrideCharacterIdsByPlayerIndex[i]
								: -1;
							break;
					}
				}

				if (targetId >= 0)
				{
					player.ApplyCharacterID(targetId);
				}

				// Scheme C: make sure CharacterID-driven data is bound before gameplay starts.
				player.EnsureCharacterDataBound();
			}
		}

		/// <summary>
		/// 处理指定玩家的死亡
		/// </summary>
		public override void PlayerDead(Character playerCharacter)
		{
			// 检查角色是否为空
			if (playerCharacter == null)
			{
				return;
			}
			// 获取角色的生命值组件
			Health characterHealth = playerCharacter.CharacterHealth;
			if (characterHealth == null)
			{
				return;
			}
			else
			{
				// 调用玩家死亡处理方法
				OnPlayerDeath(playerCharacter);
			}
		}
        
		/// <summary>
		/// 重写此方法以指定玩家死亡时发生的事件
		/// </summary>
		/// <param name="playerCharacter">死亡的角色</param>
		protected virtual void OnPlayerDeath(Character playerCharacter)
		{

		}
	}
}
