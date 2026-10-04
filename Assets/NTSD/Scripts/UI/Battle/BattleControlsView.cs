using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// Routes scene action buttons to the selected human player's published input binding.
    /// </summary>
    public sealed class BattleControlsView : MonoBehaviour, MMEventListener<BattleHudChangedEvent>
    {
        private const int ControlCount = 3;

        private static readonly string[] ActionNames =
        {
            "Attack",
            "Jump",
            "Defend",
        };

        [Header("Action Buttons")]
        [SerializeField] private NTSDButton attackButton;
        [SerializeField] private NTSDButton jumpButton;
        [SerializeField] private NTSDButton defendButton;

        private readonly ControlState[] controls = new ControlState[ControlCount];
        private InputModule inputModule;
        private InputActionMap actionMap;
        private int playerId = -1;
        private bool warnedMissingBinding;
        private bool listening;
        private BattleHudValues binding;

        public int PlayerId => playerId;
        public bool IsPlayerBound => playerId > 0 && actionMap != null;

        private void Awake()
        {
            controls[(int)BattleInputAction.Attack] =
                CreateControl(attackButton, BattleInputAction.Attack);
            controls[(int)BattleInputAction.Jump] =
                CreateControl(jumpButton, BattleInputAction.Jump);
            controls[(int)BattleInputAction.Defend] =
                CreateControl(defendButton, BattleInputAction.Defend);
        }

        private void OnEnable()
        {
            SetListening(true);
            this.MMEventStartListening<BattleHudChangedEvent>();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.TryGetCurrentBattleHud(out BattleHudChangedEvent value))
                ApplyBinding(value.Values);
            else
                UnbindPlayer();
        }

        private void OnDisable()
        {
            SetListening(false);
            this.MMEventStopListening<BattleHudChangedEvent>();
            UnbindPlayer();
        }

        private void OnDestroy()
        {
            SetListening(false);
            this.MMEventStopListening<BattleHudChangedEvent>();
            UnbindPlayer();
        }

        public void OnMMEvent(BattleHudChangedEvent value)
        {
            if (!isActiveAndEnabled) return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.IsCurrentBattleHudEvent(value))
                ApplyBinding(value.Values);
        }

        private void ApplyBinding(BattleHudValues value)
        {
            if (!value.IsVisible || value.InputId < 1)
            {
                UnbindPlayer();
                return;
            }
            if (IsPlayerBound && playerId == value.InputId && binding.Session == value.Session &&
                binding.Handle.Equals(value.Handle) && binding.StableId == value.StableId)
                return;
            if (BindPlayer(value.InputId)) binding = value;
        }

        private void SetListening(bool value)
        {
            if (listening == value)
                return;

            listening = value;
            for (int index = 0; index < controls.Length; index++)
            {
                ControlState control = controls[index];
                if (control?.Button == null)
                    continue;

                if (value)
                    control.Button.PressedStateChanged += HandlePressedStateChanged;
                else
                    control.Button.PressedStateChanged -= HandlePressedStateChanged;
            }
        }

        /// <summary>
        /// 由进入战斗后的 UI/玩家分配流程调用。
        /// </summary>
        public bool BindPlayer(int assignedPlayerId)
        {
            ReleaseAllActions();
            ClearPlayerBinding();

            if (assignedPlayerId < 1)
            {
                Debug.LogWarning(
                    $"[BattleControlsView] Invalid player ID: {assignedPlayerId}.");
                return false;
            }

            AppManager appManager = AppManager.Instance;
            inputModule = appManager != null ? appManager.InputModule : null;
            if (inputModule == null)
            {
                Debug.LogWarning(
                    "[BattleControlsView] AppManager.InputModule is not ready.");
                return false;
            }

            InputActionMap resolvedMap =
                inputModule.GetActionMapByPlayerID(assignedPlayerId);
            if (resolvedMap == null)
            {
                Debug.LogWarning(
                    $"[BattleControlsView] Player_{assignedPlayerId} action map was not found.");
                ClearPlayerBinding();
                return false;
            }

            for (int index = 0; index < controls.Length; index++)
            {
                InputAction action = resolvedMap.FindAction(
                    ActionNames[index],
                    throwIfNotFound: false);
                if (action == null)
                {
                    Debug.LogWarning(
                        $"[BattleControlsView] Player_{assignedPlayerId}/{ActionNames[index]} was not found.");
                    ClearPlayerBinding();
                    return false;
                }

                controls[index].Action = action;
            }

            playerId = assignedPlayerId;
            actionMap = resolvedMap;
            warnedMissingBinding = false;
            return true;
        }

        public void UnbindPlayer()
        {
            ReleaseAllActions();
            ClearPlayerBinding();
        }

        public void Apply(BattleControlsState state)
        {
            bool visible = state != null && state.IsVisible;
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }

        public void ClearVisualState()
        {
            ReleaseAllActions();
            gameObject.SetActive(false);
        }

        public void CollectMissingBindings(string path, List<string> missing)
        {
            if (missing == null)
                return;

            if (attackButton == null)
                missing.Add(path + ".attackButton");
            if (jumpButton == null)
                missing.Add(path + ".jumpButton");
            if (defendButton == null)
                missing.Add(path + ".defendButton");
        }

        internal void SetActionPressed(BattleInputAction action, bool pressed)
        {
            ControlState control = controls[(int)action];
            if (control == null || control.IsPressed == pressed)
                return;

            if (!pressed)
            {
                // Release the old binding now; never carry a failed release into a later binding.
                control.IsPressed = false;
                if (IsPlayerBound && control.Action != null && inputModule != null)
                    inputModule.TrySetActionPressed(playerId, control.Action, false);
                return;
            }

            if (!isActiveAndEnabled ||
                control.Button == null ||
                !control.Button.isActiveAndEnabled ||
                !control.Button.IsInteractable())
            {
                return;
            }

            if (!IsPlayerBound || control.Action == null || inputModule == null)
            {
                WarnMissingBinding();
                return;
            }

            if (!inputModule.TrySetActionPressed(playerId, control.Action, pressed))
            {
                WarnMissingBinding();
                return;
            }

            control.IsPressed = pressed;
        }

        private ControlState CreateControl(NTSDButton button, BattleInputAction action)
        {
            return new ControlState(button, action);
        }

        private void HandlePressedStateChanged(NTSDButton button, bool pressed)
        {
            for (int index = 0; index < controls.Length; index++)
            {
                ControlState control = controls[index];
                if (control?.Button == button)
                {
                    SetActionPressed(control.ActionType, pressed);
                    return;
                }
            }
        }

        private void ReleaseAllActions()
        {
            for (int index = 0; index < controls.Length; index++)
            {
                if (controls[index]?.IsPressed == true)
                    SetActionPressed((BattleInputAction)index, pressed: false);
            }
        }

        private void ClearPlayerBinding()
        {
            binding = default;
            playerId = -1;
            actionMap = null;
            inputModule = null;
            warnedMissingBinding = false;

            for (int index = 0; index < controls.Length; index++)
            {
                if (controls[index] != null)
                {
                    controls[index].Action = null;
                    controls[index].IsPressed = false;
                }
            }
        }

        private void WarnMissingBinding()
        {
            if (warnedMissingBinding)
                return;

            warnedMissingBinding = true;
            Debug.LogWarning(
                "[BattleControlsView] BindPlayer must succeed before using the battle buttons.");
        }

        private sealed class ControlState
        {
            public ControlState(NTSDButton button, BattleInputAction actionType)
            {
                Button = button;
                ActionType = actionType;
            }

            public NTSDButton Button { get; }
            public BattleInputAction ActionType { get; }
            public InputAction Action { get; set; }
            public bool IsPressed { get; set; }
        }
    }
}
