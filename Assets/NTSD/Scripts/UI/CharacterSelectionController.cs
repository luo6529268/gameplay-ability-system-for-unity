using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NTSD.App;
using BeatEmUpTemplate2D;

namespace NTSD.UI
{
    /// <summary>
    /// 角色选择流程的阶段枚举
    /// 
    /// 流程顺序:
    /// JoinAndSelect -> Countdown -> ComputerCount -> SettingBattleBg -> StartingBattle
    /// </summary>
    public enum CharacterSelectionStep
    {
        /// <summary>
        /// 阶段1: 玩家加入并选择角色/队伍
        /// 等待所有已加入玩家确认后自动进入倒计时
        /// </summary>
        JoinAndSelect = 0,

        /// <summary>
        /// 阶段2: 5秒倒计时
        /// 在未使用的槽位上显示倒计时精灵
        /// 如果有玩家取消确认，倒计时会加速
        /// </summary>
        Countdown = 1,

        /// <summary>
        /// 阶段3: 选择电脑玩家数量 (CMCRoot弹窗)
        /// 只有Player1可以操作
        /// </summary>
        ComputerCount = 2,

        /// <summary>
        /// 阶段4: 设置战斗参数 (SettingBattleBg弹窗)
        /// 设置难度、背景等，只有Player1可以操作
        /// </summary>
        SettingBattleBg = 3,

        /// <summary>
        /// 阶段5: 启动战斗
        /// 构建MatchConfig并加载战斗场景
        /// </summary>
        StartingBattle = 4,
    }

    /// <summary>
    /// 角色选择流程总控制器
    /// 
    /// 职责:
    /// 1. 管理整个角色选择流程的状态机
    /// 2. 协调各个子控制器 (SelectRoleItem, CMCRootController, SettingBattleBgController)
    /// 3. 处理倒计时逻辑
    /// 4. 构建最终的 MatchConfig 并启动战斗
    /// 
    /// 流程说明:
    /// 1. JoinAndSelect: 玩家通过各自的 SelectRoleItem 加入并选择角色/队伍
    /// 2. Countdown: 所有玩家确认后开始5秒倒计时，期间在空闲槽位显示倒计时
    /// 3. ComputerCount: 显示CMC弹窗，Player1选择电脑玩家数量
    /// 4. SettingBattleBg: 显示设置弹窗，Player1设置难度和背景
    /// 5. StartingBattle: 构建配置，调用AppManager启动战斗
    /// 
    /// 依赖:
    /// - GameConfig: 获取倒计时配置
    /// - AppManager: 启动战斗场景
    /// </summary>
    public sealed class CharacterSelectionController : MonoBehaviour
    {
        #region Inspector字段

        [Header("Player Slots")]
        [SerializeField] private List<SelectRoleItem> playerSlots;      // 8个玩家槽位

        [Header("Popup Controllers")]
        [SerializeField] private CMCRootController cmcRootController;               // CMC弹窗控制器
        [SerializeField] private SettingBattleBgController settingBattleBgController;   // 战斗设置弹窗控制器

        [Header("Runtime State")]
        [SerializeField] private CharacterSelectionStep step = CharacterSelectionStep.JoinAndSelect;  // 当前流程阶段
        [SerializeField] private GameModeConfig currentGameMode;        // 当前游戏模式配置
        [SerializeField] private int backgroundId = -1;                 // 选择的背景ID
        [SerializeField] private int difficulty = 2;                    // 选择的难度
        [SerializeField] private int seed = 0;                          // 随机种子
        [SerializeField] private List<PlayerSlotConfig> players = new List<PlayerSlotConfig>();  // 玩家配置列表

        [Header("Countdown")]
        [SerializeField] private float countdownTimer;                  // 倒计时剩余时间
        [SerializeField] private int lastDisplayedSecond = -1;          // 上次显示的秒数（用于优化更新）

        private InputActionMap player1ActionMap;                        // Player1的输入ActionMap
        private InputAction player1JumpAction;                          // Player1的Jump输入Action
        private bool countdownInputBound;                               // 倒计时输入是否已绑定

        #endregion

        #region 公开属性

        public CharacterSelectionStep Step => step;
        public IReadOnlyList<PlayerSlotConfig> Players => players;
        public IReadOnlyList<SelectRoleItem> PlayerSlots => playerSlots;

        #endregion

        #region 事件

        /// <summary>
        /// 比赛配置确认事件
        /// 当所有设置完成并准备开始战斗时触发
        /// </summary>
        public event Action<MatchConfig> MatchConfirmed;

        #endregion

        #region Unity生命周期

        private void OnEnable()
        {
            InitializePlayerSlots();
            SubscribePopupEvents();
            HideAllPopups();
        }

        private void OnDisable()
        {
            UnsubscribePopupEvents();
            UnbindCountdownInput();
        }

        private void Update()
        {
            switch (step)
            {
                case CharacterSelectionStep.JoinAndSelect:
                    UpdateJoinAndSelect();
                    break;
                case CharacterSelectionStep.Countdown:
                    UpdateCountdown();
                    break;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化所有玩家槽位
        /// </summary>
        private void InitializePlayerSlots()
        {
            for (int i = 0; i < playerSlots.Count; i++)
            {
                if (playerSlots[i] != null)
                {
                    playerSlots[i].Initialize(i);
                }
            }
        }

        /// <summary>
        /// 订阅弹窗事件
        /// </summary>
        private void SubscribePopupEvents()
        {
            if (cmcRootController != null)
                cmcRootController.OnConfirmed += OnCMCConfirmed;

            if (settingBattleBgController != null)
            {
                settingBattleBgController.OnConfirmed += OnSettingBattleBgConfirmed;
                settingBattleBgController.OnExit += OnSettingBattleBgExit;
                settingBattleBgController.OnResetRandom += OnSettingBattleBgResetRandom;
                settingBattleBgController.OnResetAll += OnSettingBattleBgResetAll;
            }
        }

        /// <summary>
        /// 取消订阅弹窗事件
        /// </summary>
        private void UnsubscribePopupEvents()
        {
            if (cmcRootController != null)
                cmcRootController.OnConfirmed -= OnCMCConfirmed;

            if (settingBattleBgController != null)
            {
                settingBattleBgController.OnConfirmed -= OnSettingBattleBgConfirmed;
                settingBattleBgController.OnExit -= OnSettingBattleBgExit;
                settingBattleBgController.OnResetRandom -= OnSettingBattleBgResetRandom;
                settingBattleBgController.OnResetAll -= OnSettingBattleBgResetAll;
            }
        }

        /// <summary>
        /// 隐藏所有弹窗
        /// </summary>
        private void HideAllPopups()
        {
            cmcRootController?.Hide();
            settingBattleBgController?.Hide();
        }

        #endregion

        #region 阶段1: JoinAndSelect

        /// <summary>
        /// 更新JoinAndSelect阶段
        /// 检测是否所有已加入玩家都已确认，如果是则开始倒计时
        /// </summary>
        private void UpdateJoinAndSelect()
        {
            if (AreAllPlayersConfirmed())
            {
                StartCountdown();
            }
        }

        #endregion

        #region 阶段2: Countdown

        /// <summary>
        /// 开始倒计时
        /// 从GameConfig获取倒计时时长，绑定Player1的Jump输入用于跳过
        /// </summary>
        private void StartCountdown()
        {
            var config = GameConfig.Instance;
            countdownTimer = config != null ? config.CountdownDuration : 5f;
            lastDisplayedSecond = -1;
            step = CharacterSelectionStep.Countdown;

            BindCountdownInput();
            UpdateCountdownDisplay();
        }

        /// <summary>
        /// 更新倒计时
        /// 每帧减少倒计时，到0时结束
        /// </summary>
        private void UpdateCountdown()
        {
            countdownTimer -= Time.deltaTime;

            UpdateCountdownDisplay();

            if (countdownTimer <= 0)
            {
                EndCountdown();
            }
        }

        /// <summary>
        /// 绑定倒计时期间的输入
        /// 监听Player1的Jump键，用于快速跳过倒计时
        /// </summary>
        private void BindCountdownInput()
        {
            if (countdownInputBound) return;

            player1ActionMap = AppManager.Instance.InputModule.GetActionMapByPlayerID(1);
            if (player1ActionMap == null) return;

            player1ActionMap.Enable();
            player1JumpAction = player1ActionMap.FindAction("Jump");

            if (player1JumpAction != null)
                player1JumpAction.performed += OnPlayer1JumpDuringCountdown;

            countdownInputBound = true;
        }

        /// <summary>
        /// 解绑倒计时输入
        /// </summary>
        private void UnbindCountdownInput()
        {
            if (!countdownInputBound) return;

            if (player1JumpAction != null)
                player1JumpAction.performed -= OnPlayer1JumpDuringCountdown;

            player1ActionMap = null;
            player1JumpAction = null;
            countdownInputBound = false;
        }

        /// <summary>
        /// Player1在倒计时期间按下Jump的回调
        /// 每次按下减少1秒倒计时
        /// </summary>
        private void OnPlayer1JumpDuringCountdown(InputAction.CallbackContext ctx)
        {
            if (step != CharacterSelectionStep.Countdown) return;

            countdownTimer--;

            if (countdownTimer <= 0)
            {
                EndCountdown();
            }
        }

        /// <summary>
        /// 更新倒计时显示
        /// 在空闲槽位上显示对应的倒计时精灵
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            int currentSecond = Mathf.CeilToInt(countdownTimer);
            if (currentSecond == lastDisplayedSecond) return;

            lastDisplayedSecond = currentSecond;

            var config = GameConfig.Instance;
            if (config == null || config.CountdownSprites == null || config.CountdownSprites.Length == 0)
                return;

            int spriteIndex = Mathf.Clamp((int)config.CountdownDuration - currentSecond, 0, config.CountdownSprites.Length - 1);
            Sprite countdownSprite = config.CountdownSprites[spriteIndex];

            foreach (var slot in playerSlots)
            {
                if (slot != null && slot.State == SelectRoleState.Idle)
                {
                    slot.ShowCountdown(countdownSprite);
                }
            }
        }

        /// <summary>
        /// 结束倒计时
        /// 解绑输入，隐藏倒计时显示，进入CMC阶段
        /// </summary>
        private void EndCountdown()
        {
            UnbindCountdownInput();
            HideCountdownOnUnusedSlots();
            ShowCMCPopup();
        }

        /// <summary>
        /// 隐藏空闲槽位上的倒计时显示
        /// </summary>
        private void HideCountdownOnUnusedSlots()
        {
            foreach (var slot in playerSlots)
            {
                if (slot != null && slot.State == SelectRoleState.Idle)
                {
                    slot.HideCountdown();
                }
            }
        }

        #endregion

        #region 阶段3: ComputerCount (CMCRoot)

        /// <summary>
        /// 显示CMC弹窗
        /// </summary>
        private void ShowCMCPopup()
        {
            step = CharacterSelectionStep.ComputerCount;
            int joinedCount = GetJoinedPlayerCount();
            cmcRootController?.Show(joinedCount);
        }

        /// <summary>
        /// CMC确认回调
        /// </summary>
        /// <param name="computerCount">选择的电脑玩家数量</param>
        private void OnCMCConfirmed(int computerCount)
        {
            cmcRootController?.Hide();
            ShowSettingBattleBg();
        }

        #endregion

        #region 阶段4: SettingBattleBg

        /// <summary>
        /// 显示战斗设置弹窗
        /// </summary>
        private void ShowSettingBattleBg()
        {
            step = CharacterSelectionStep.SettingBattleBg;
            settingBattleBgController?.Show();
        }

        /// <summary>
        /// 战斗设置确认回调 - 开始战斗
        /// </summary>
        private void OnSettingBattleBgConfirmed(int mapId, int difficultyValue)
        {
            //settingBattleBgController?.Hide();
            backgroundId = mapId;
            difficulty = difficultyValue;
            StartBattle();
        }

        /// <summary>
        /// 战斗设置退出回调 - 返回CMC
        /// </summary>
        private void OnSettingBattleBgExit()
        {
            settingBattleBgController?.Hide();
        }

        /// <summary>
        /// 重置随机选项回调
        /// </summary>
        private void OnSettingBattleBgResetRandom()
        {
            // TODO: 实现重置随机选项的逻辑
        }

        /// <summary>
        /// 重置所有选择回调 - 返回角色选择
        /// </summary>
        private void OnSettingBattleBgResetAll()
        {
            settingBattleBgController?.Hide();
            ResetAll();
        }

        #endregion

        #region 阶段5: StartingBattle

        /// <summary>
        /// 启动战斗
        /// 构建MatchConfig并调用AppManager加载战斗场景
        /// </summary>
        private void StartBattle()
        {
            step = CharacterSelectionStep.StartingBattle;

            SyncPlayersFromSlots();

            var cfg = new MatchConfig
            {
                gameMode = currentGameMode,
                backgroundId = backgroundId,
                difficulty = difficulty,
                seed = seed,
                players = new List<PlayerSlotConfig>(players),
            };

            MatchConfirmed?.Invoke(cfg);

            if (AppManager.Instance != null)
            {
                AppManager.Instance.SetMatchConfig(cfg);
                AppManager.Instance.LoadBattleAdditive();
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 重置所有状态，返回初始的角色选择阶段
        /// </summary>
        /// <param name="playerSlotCount">玩家槽位数量（默认8）</param>
        public void ResetAll(int playerSlotCount = 8)
        {
            step = CharacterSelectionStep.JoinAndSelect;
            backgroundId = -1;
            difficulty = 2;
            seed = 0;
            countdownTimer = 0;
            lastDisplayedSecond = -1;

            // 重置玩家配置列表
            players.Clear();
            for (int i = 0; i < playerSlotCount; i++)
            {
                players.Add(new PlayerSlotConfig
                {
                    use = false,
                    isHuman = true,
                    characterId = -1,
                    team = 0,
                    aiId = -1,
                });
            }

            // 重置所有槽位
            foreach (var slot in playerSlots)
            {
                slot?.Reset();
            }

            HideAllPopups();
        }

        /// <summary>
        /// 检查是否所有已加入的玩家都已确认
        /// </summary>
        /// <returns>如果所有已加入玩家都确认则返回true</returns>
        public bool AreAllPlayersConfirmed()
        {
            int confirmedCount = 0;
            int joinedCount = 0;

            foreach (var slot in playerSlots)
            {
                if (slot == null) continue;

                if (slot.State != SelectRoleState.Idle)
                {
                    joinedCount++;
                    if (slot.State == SelectRoleState.Confirmed)
                    {
                        confirmedCount++;
                    }
                }
            }

            // 至少有一个玩家加入，且所有加入的玩家都已确认
            return joinedCount > 0 && confirmedCount == joinedCount;
        }

        /// <summary>
        /// 获取已加入的玩家数量
        /// </summary>
        public int GetJoinedPlayerCount()
        {
            int count = 0;
            foreach (var slot in playerSlots)
            {
                if (slot != null && slot.State != SelectRoleState.Idle)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 从槽位同步玩家配置
        /// 将每个槽位的选择结果转换为PlayerSlotConfig
        /// </summary>
        public void SyncPlayersFromSlots()
        {
            players.Clear();

            foreach (var slot in playerSlots)
            {
                if (slot == null || slot.State == SelectRoleState.Idle)
                {
                    players.Add(new PlayerSlotConfig { use = false });
                    continue;
                }

                players.Add(new PlayerSlotConfig
                {
                    use = true,
                    isHuman = true,
                    characterId = slot.GetFinalCharacterId(),
                    team = slot.GetFinalTeam(),
                    aiId = -1
                });
            }
        }

        /// <summary>
        /// 确认比赛配置（外部调用）
        /// </summary>
        public void ConfirmMatch()
        {
            SyncPlayersFromSlots();

            var cfg = new MatchConfig
            {
                gameMode = currentGameMode,
                backgroundId = backgroundId,
                difficulty = difficulty,
                seed = seed,
                players = new List<PlayerSlotConfig>(players),
            };

            MatchConfirmed?.Invoke(cfg);
        }

        #endregion

        #region Setter方法

        public void SetGameMode(GameModeConfig config) => currentGameMode = config;
        public void SetStep(CharacterSelectionStep newStep) => step = newStep;
        public void SetBackgroundId(int id) => backgroundId = id;
        public void SetDifficulty(int value) => difficulty = value;
        public void SetSeed(int value) => seed = value;

        #endregion
    }
}
