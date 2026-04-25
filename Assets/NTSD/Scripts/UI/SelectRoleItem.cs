using BeatEmUpTemplate2D;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.App;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NTSD.UI
{
    /// <summary>
    /// 角色选择槽位的状态枚举
    /// 状态流转: Idle -> SelectingCharacter -> SelectingTeam -> Confirmed
    /// </summary>
    public enum SelectRoleState
    {
        /// <summary>空闲状态，等待玩家按键加入</summary>
        Idle,
        /// <summary>正在选择角色</summary>
        SelectingCharacter,
        /// <summary>正在选择队伍</summary>
        SelectingTeam,
        /// <summary>已确认所有选择</summary>
        Confirmed
    }

    /// <summary>
    /// 角色选择槽位控制器
    /// 
    /// 职责:
    /// 1. 管理单个玩家槽位的状态机 (Idle -> SelectingCharacter -> SelectingTeam -> Confirmed)
    /// 2. 处理玩家输入 (方向键切换选项, Attack确认, Jump取消)
    /// 3. 更新UI显示 (角色图标、名称、队伍)
    /// 4. 支持倒计时显示 (在空闲槽位上显示倒计时精灵)
    /// 
    /// 输入绑定:
    /// - Move: 左右切换角色/队伍
    /// - Attack: 确认当前选择，进入下一状态
    /// - Jump: 取消，返回上一状态
    /// </summary>
    public class SelectRoleItem : MonoBehaviour
    {
        #region UI引用

        [Header("UI References")]
        public Image RoleIcon;              // 角色图标
        public TextMeshProUGUI PlayerNameTxt;   // 玩家名称文本
        public TextMeshProUGUI RoleNameTxt;     // 角色名称文本
        public TextMeshProUGUI TeamTxt;         // 队伍文本

        public AudioClip JoinSound;      // 加入声音
        public AudioClip LeaveSound;    // 离开声音

        #endregion

        #region 输入设置

        [Header("Input Settings")]
        [SerializeField] private float navigationCooldown = 0.15f;  // 导航输入冷却时间，防止过快切换

        #endregion

        #region 运行时状态

        [Header("Runtime State")]
        [SerializeField] private int itemIndex;                     // 槽位索引 (0-7)
        [SerializeField] private int playerId;                      // 绑定的玩家ID (1-4)
        [SerializeField] private SelectRoleState state = SelectRoleState.Idle;  // 当前状态
        [SerializeField] private int selectedCharacterId = GameConfig.RandomCharacterId;  // 选中的角色ID
        [SerializeField] private int selectedTeamIndex = 0;         // 选中的队伍索引

        private List<int> availableCharacterIds;    // 可选角色ID列表
        private int characterSelectionIndex = 0;    // 当前角色选择索引
        private float idleFlashTimer;               // 空闲状态闪烁计时器
        private bool idleFlashToggle;               // 空闲状态闪烁开关
        private bool CountdownStart;               // 倒计时开始
        #endregion

        #region 输入系统

        private InputActionMap inputActionMap;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction jumpAction;
        private float lastNavigationTime;           // 上次导航时间，用于冷却判断
        private bool inputBound;                    // 输入是否已绑定

        #endregion

        #region 公开属性

        public int ItemIndex => itemIndex;
        public int PlayerId => playerId;
        public SelectRoleState State => state;
        public int SelectedCharacterId => selectedCharacterId;
        public int SelectedTeamIndex => selectedTeamIndex;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化槽位
        /// </summary>
        /// <param name="index">槽位索引 (0-7)</param>
        public void Initialize(int index)
        {
            itemIndex = index;
            playerId = (index % 4) + 1;  // 槽位0-3对应玩家1-4，槽位4-7也对应玩家1-4

            state = SelectRoleState.Idle;
            selectedCharacterId = GameConfig.RandomCharacterId;
            selectedTeamIndex = 0;
            characterSelectionIndex = 0;
            RoleIcon.gameObject.SetActive(true);

            RefreshAvailableCharacters();
            UpdateDisplay();

            BindInput();
        }

        private void OnEnable()
        {
            BindInput();
        }

        private void OnDisable()
        {
            UnbindInput();
        }

        private void Update()
        {
            {
                UpdateIdleFlash();
            }
        }

        #endregion

        #region 输入绑定

        /// <summary>
        /// 绑定玩家输入
        /// 只有当槽位索引等于玩家ID时才绑定（确保每个玩家只控制自己的槽位）
        /// </summary>
        private void BindInput()
        {
            if (inputBound) return;
            if (itemIndex + 1 != playerId) return;  // 只绑定对应玩家的输入

            inputActionMap = AppManager.Instance.InputModule.GetActionMapByPlayerID(playerId);
            if (inputActionMap == null) return;

            inputActionMap.Enable();

            moveAction = inputActionMap.FindAction("Move");
            attackAction = inputActionMap.FindAction("Attack");
            jumpAction = inputActionMap.FindAction("Jump");

            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
            }
            if (attackAction != null)
            {
                attackAction.performed += OnConfirmPerformed;
            }
            if (jumpAction != null)
            {
                jumpAction.performed += OnCancelPerformed;
            }

            inputBound = true;
        }

        private void UnbindInput()
        {
            if (!inputBound) return;

            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
            }
            if (attackAction != null)
            {
                attackAction.performed -= OnConfirmPerformed;
            }
            if (jumpAction != null)
            {
                jumpAction.performed -= OnCancelPerformed;
            }

            inputActionMap?.Disable();
            inputActionMap = null;
            moveAction = null;
            attackAction = null;
            jumpAction = null;
            inputBound = false;
        }

        /// <summary>
        /// 处理移动输入（左右切换）
        /// </summary>
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            if (Time.unscaledTime - lastNavigationTime < navigationCooldown) return;

            Vector2 input = ctx.ReadValue<Vector2>();

            if (Mathf.Abs(input.x) != 0f)
            {
                int direction = input.x > 0 ? 1 : -1;
                HandleNavigate(direction);
                lastNavigationTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// 处理确认输入（Attack键）
        /// </summary>
        private void OnConfirmPerformed(InputAction.CallbackContext ctx)
        {
            MMSoundManagerSoundPlayEvent.Trigger(JoinSound, MMSoundManagerPlayOptions.Default);
            HandleConfirm();
        }

        /// <summary>
        /// 处理取消输入（Jump键）
        /// </summary>
        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (this.GetComponentInParent<CharacterSelectionController>().Step >= CharacterSelectionStep.ComputerCount)
                return;

            MMSoundManagerSoundPlayEvent.Trigger(LeaveSound, MMSoundManagerPlayOptions.Default);
            HandleCancel();
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 根据当前状态处理导航输入
        /// </summary>
        private void HandleNavigate(int direction)
        {
            switch (state)
            {
                case SelectRoleState.SelectingCharacter:
                    OnNavigateCharacter(direction);
                    break;
                case SelectRoleState.SelectingTeam:
                    OnNavigateTeam(direction);
                    break;
            }
        }

        /// <summary>
        /// 根据当前状态处理确认输入
        /// 状态流转: Idle -> SelectingCharacter -> SelectingTeam -> Confirmed
        /// </summary>
        private void HandleConfirm()
        {
            switch (state)
            {
                case SelectRoleState.Idle:
                    OnJoin();  // 加入游戏
                    break;
                case SelectRoleState.SelectingCharacter:
                    OnConfirmCharacter();  // 确认角色，进入队伍选择
                    break;
                case SelectRoleState.SelectingTeam:
                    OnConfirmTeam();  // 确认队伍，完成选择
                    break;
            }
        }

        /// <summary>
        /// 处理取消输入，返回上一状态
        /// </summary>
        private void HandleCancel()
        {
            OnCancel();
        }

        #endregion

        #region 状态操作

        /// <summary>
        /// 更新空闲状态的闪烁效果
        /// </summary>
        private void UpdateIdleFlash()
        {
            if (state == SelectRoleState.Confirmed)
                return;

            if (CountdownStart) return;

            var config = GameConfig.Instance;
            if (config == null) return;

            idleFlashTimer += Time.deltaTime;
            if (idleFlashTimer >= config.IdleFlashInterval)
            {
                idleFlashTimer = 0f;
                idleFlashToggle = !idleFlashToggle;

                // 空闲状态：闪烁"Join?"图标
                if (state == SelectRoleState.Idle)
                {
                    if (PlayerNameTxt != null)
                        PlayerNameTxt.color = idleFlashToggle ? config.IdleFlashColor1 : config.IdleFlashColor2;
                    if (RoleIcon != null)
                    {
                        RoleIcon.sprite = idleFlashToggle ? config.JoinIcon1 : config.JoinIcon2;
                        RoleIcon.SetNativeSize();
                    }
                }

                // 选择角色状态：角色名称闪烁
                if (RoleNameTxt != null && state == SelectRoleState.SelectingCharacter) 
                {
                    RoleNameTxt.color = idleFlashToggle ? config.IdleFlashColor1 : config.IdleFlashColor2;
                }

                // 选择队伍状态：队伍名称闪烁
                if (TeamTxt != null && state == SelectRoleState.SelectingTeam) 
                {
                    TeamTxt.color = idleFlashToggle ? config.IdleFlashColor1 : config.IdleFlashColor2;
                }

            }
        }

        /// <summary>
        /// 刷新可选角色列表
        /// 从 CharacterAnimtorManager 获取所有已加载的角色
        /// </summary>
        public void RefreshAvailableCharacters()
        {
            availableCharacterIds = new List<int> { GameConfig.RandomCharacterId };  // 第一个选项始终是"随机"

            if (CharacterAnimtorManager.Instance != null && CharacterAnimtorManager.Instance.IsPrewarmCompleted)
            {
                var loadedIds = CharacterAnimtorManager.Instance.GetAllLoadedCharacterIds();
                if (loadedIds != null)
                {
                    availableCharacterIds.AddRange(loadedIds);
                }
            }
        }

        /// <summary>
        /// 玩家加入游戏
        /// 从 Idle 状态进入 SelectingCharacter 状态
        /// </summary>
        public void OnJoin()
        {
            if (state != SelectRoleState.Idle) return;

            state = SelectRoleState.SelectingCharacter;
            characterSelectionIndex = 0;
            selectedCharacterId = GetCharacterIdAtIndex(characterSelectionIndex);
            UpdateDisplay();
        }

        /// <summary>
        /// 切换角色选择
        /// </summary>
        /// <param name="direction">方向 (1=右, -1=左)</param>
        public void OnNavigateCharacter(int direction)
        {
            if (state != SelectRoleState.SelectingCharacter) return;
            if (availableCharacterIds == null || availableCharacterIds.Count == 0) return;

            characterSelectionIndex += direction;
            // 循环选择
            if (characterSelectionIndex < 0)
                characterSelectionIndex = availableCharacterIds.Count - 1;
            else if (characterSelectionIndex >= availableCharacterIds.Count)
                characterSelectionIndex = 0;

            selectedCharacterId = GetCharacterIdAtIndex(characterSelectionIndex);
            UpdateDisplay();
        }

        /// <summary>
        /// 确认角色选择，进入队伍选择状态
        /// </summary>
        public void OnConfirmCharacter()
        {
            if (state != SelectRoleState.SelectingCharacter) return;

            state = SelectRoleState.SelectingTeam;
            UpdateDisplay();
        }

        /// <summary>
        /// 切换队伍选择
        /// </summary>
        /// <param name="direction">方向 (1=右, -1=左)</param>
        public void OnNavigateTeam(int direction)
        {
            if (state != SelectRoleState.SelectingTeam) return;

            var config = GameConfig.Instance;
            if (config == null || config.TeamOptions == null || config.TeamOptions.Length == 0) return;

            selectedTeamIndex += direction;
            // 循环选择
            if (selectedTeamIndex < 0)
                selectedTeamIndex = config.TeamOptions.Length - 1;
            else if (selectedTeamIndex >= config.TeamOptions.Length)
                selectedTeamIndex = 0;

            UpdateDisplay();
        }

        /// <summary>
        /// 确认队伍选择，完成所有选择
        /// </summary>
        public void OnConfirmTeam()
        {
            if (state != SelectRoleState.SelectingTeam) return;

            state = SelectRoleState.Confirmed;
            UpdateDisplay();
        }

        /// <summary>
        /// 取消当前选择，返回上一状态
        /// Confirmed -> SelectingTeam -> SelectingCharacter -> Idle
        /// </summary>
        public void OnCancel()
        {
            switch (state)
            {
                case SelectRoleState.SelectingCharacter:
                    state = SelectRoleState.Idle;
                    break;
                case SelectRoleState.SelectingTeam:
                    state = SelectRoleState.SelectingCharacter;
                    break;
                case SelectRoleState.Confirmed:
                    state = SelectRoleState.SelectingTeam;
                    break;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// 重置槽位到初始状态
        /// </summary>
        public void Reset()
        {
            state = SelectRoleState.Idle;
            selectedCharacterId = GameConfig.RandomCharacterId;
            selectedTeamIndex = 0;
            characterSelectionIndex = 0;
            UpdateDisplay();
        }

        #endregion

        #region 显示更新

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateDisplay()
        {
            var config = GameConfig.Instance;

            if (state == SelectRoleState.Idle)
            {
                UpdateIdleDisplay(config);
            }
            else
            {
                UpdateActiveDisplay(config);
            }

            UpdateStateColors(config);
        }

        /// <summary>
        /// 更新空闲状态的显示
        /// </summary>
        private void UpdateIdleDisplay(GameConfig config)
        {
            if (PlayerNameTxt != null)
                PlayerNameTxt.text = config != null ? config.IdlePlayerText : "Join?";

            if (RoleNameTxt != null)
                RoleNameTxt.text = config != null ? config.IdleFighterText : "";

            if (TeamTxt != null)
                TeamTxt.text = config != null ? config.IdleTeamText : "";

        }

        /// <summary>
        /// 更新激活状态的显示（选择角色/队伍）
        /// </summary>
        private void UpdateActiveDisplay(GameConfig config)
        {
            if (state == SelectRoleState.SelectingCharacter)
            {
                if (PlayerNameTxt != null)
                    PlayerNameTxt.text = GameLocalSettings.GetPlayerName(itemIndex % 4);

                UpdateCharacterDisplay(config);

                if (TeamTxt != null)
                    TeamTxt.text = config != null ? config.IdleTeamText : "";
            }

            if (state == SelectRoleState.SelectingTeam)
            {
                UpdateTeamDisplay(config);
            }
        }

        /// <summary>
        /// 更新角色显示（图标和名称）
        /// </summary>
        private void UpdateCharacterDisplay(GameConfig config)
        {
            if (selectedCharacterId == GameConfig.RandomCharacterId)
            {
                // 显示"随机"选项
                if (RoleNameTxt != null)
                    RoleNameTxt.text = config != null ? config.RandomDisplayName : "Random";

                if (RoleIcon != null && config != null && config.RandomIcon != null) 
                {
                    RoleIcon.sprite = config.RandomIcon;
                    RoleIcon.SetNativeSize();
                }
            }
            else
            {
                // 显示具体角色
                string characterName = "Unknown";
                Sprite characterIcon = null;

                // 从CharacterAnimtorManager获取角色名称
                if (CharacterAnimtorManager.Instance != null)
                {
                    characterName = CharacterAnimtorManager.Instance.GetCharacterName(selectedCharacterId);
                }

                // 从CharacterUIResourceManager获取角色头像
                if (CharacterUIResourceManager.Instance != null)
                {
                    characterIcon = CharacterUIResourceManager.Instance.GetHeadSprite(selectedCharacterId);
                }

                if (RoleNameTxt != null)
                    RoleNameTxt.text = characterName;

                if (RoleIcon != null && characterIcon != null) 
                {
                    RoleIcon.sprite = characterIcon;
                    RoleIcon.SetNativeSize();
                }
            }
        }

        /// <summary>
        /// 更新队伍显示
        /// </summary>
        private void UpdateTeamDisplay(GameConfig config)
        {
            if (TeamTxt == null) return;

            if (config != null && config.TeamOptions != null && selectedTeamIndex < config.TeamOptions.Length)
            {
                TeamTxt.text = config.TeamOptions[selectedTeamIndex];
            }
        }

        /// <summary>
        /// 根据状态更新颜色
        /// 已确认的项目显示为 ConfirmedColor
        /// </summary>
        private void UpdateStateColors(GameConfig config)
        {
            if (config == null) return;

            switch (state)
            {
                case SelectRoleState.Idle:
                    return;
                case SelectRoleState.SelectingCharacter:
                    // 玩家名已确认（已加入）
                    if (PlayerNameTxt != null)
                        PlayerNameTxt.color = config.ConfirmedColor;
                    break;
                case SelectRoleState.SelectingTeam:
                    // 角色已确认
                    if (RoleNameTxt != null)
                        RoleNameTxt.color = config.ConfirmedColor;
                    break;
                case SelectRoleState.Confirmed:
                    // 队伍已确认
                    if (TeamTxt != null)
                        TeamTxt.color = config.ConfirmedColor;
                    break;
            }

        }

        #endregion

        #region 公开获取方法

        /// <summary>
        /// 根据索引获取角色ID
        /// </summary>
        private int GetCharacterIdAtIndex(int index)
        {
            if (availableCharacterIds == null || availableCharacterIds.Count == 0)
                return GameConfig.RandomCharacterId;

            if (index < 0 || index >= availableCharacterIds.Count)
                return GameConfig.RandomCharacterId;

            return availableCharacterIds[index];
        }

        /// <summary>
        /// 获取最终确定的角色ID
        /// 如果选择了"随机"，则随机返回一个可用角色
        /// </summary>
        public int GetFinalCharacterId()
        {
            if (selectedCharacterId == GameConfig.RandomCharacterId)
            {
                if (availableCharacterIds != null && availableCharacterIds.Count > 1)
                {
                    // 从索引1开始随机（跳过索引0的"随机"选项）
                    int randomIndex = Random.Range(1, availableCharacterIds.Count);
                    return availableCharacterIds[randomIndex];
                }
                return GameConfig.RandomCharacterId;
            }
            return selectedCharacterId;
        }

        /// <summary>
        /// 获取最终确定的队伍ID
        /// 如果选择了最后一个选项（Independent），返回 TeamIndependent
        /// </summary>
        public int GetFinalTeam()
        {
            var config = GameConfig.Instance;
            if (config == null) return 0;

            if (selectedTeamIndex == config.TeamOptions.Length - 1)
                return GameConfig.TeamIndependent;

            return selectedTeamIndex;
        }

        #endregion

        #region 倒计时显示

        /// <summary>
        /// 显示倒计时精灵
        /// 在倒计时阶段，空闲槽位会显示倒计时数字
        /// </summary>
        /// <param name="countdownSprite">倒计时精灵（如数字5、4、3、2、1）</param>
        public void ShowCountdown(Sprite countdownSprite)
        {
            if (RoleIcon == null) return;

            RoleIcon.sprite = countdownSprite;
            RoleIcon.SetNativeSize();
            CountdownStart = true;
        }

        /// <summary>
        /// 隐藏倒计时，恢复原始图标
        /// </summary>
        public void HideCountdown()
        {
            if (RoleIcon == null) return;

            RoleIcon.gameObject.SetActive(false);
            PlayerNameTxt.text = "--";
            CountdownStart = false;
        }

        #endregion
    }
}
