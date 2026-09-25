using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NTSD.UI.Battle
{
    /// <summary>
    /// BattleControls 的动作按键绑定面。
    ///
    /// 这里复用 AppManager 持有的 NTSDInputConfig action map，不创建第二份输入资产。
    /// Image 既是当前场景的视觉面，也是 PointerDown/Up 的输入面；这样不要求当前场景
    /// 立即增加 UnityEngine.UI.Button 组件，同时保留按住和松开两个输入边沿。
    /// </summary>
    public sealed class BattleControlsView : MonoBehaviour
    {
        private const string AttackActionName = "Attack";
        private const string JumpActionName = "Jump";
        private const string DefendActionName = "Defend";

        [Header("Input Binding")]
        [SerializeField, Min(1)] private int playerId = 1;

        [Header("Action Button Surfaces")]
        [SerializeField] private Image attackImage;
        [SerializeField] private Image jumpImage;
        [SerializeField] private Image defendImage;

        [Header("Optional Pressed Overlays")]
        [SerializeField] private GameObject attackPressedState;
        [SerializeField] private GameObject jumpPressedState;
        [SerializeField] private GameObject defendPressedState;

        [Header("Color Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.white;

        private InputActionMap actionMap;
        private InputAction attackAction;
        private InputAction jumpAction;
        private InputAction defendAction;

        private CharacterInputModule attackInputOwner;
        private CharacterInputModule jumpInputOwner;
        private CharacterInputModule defendInputOwner;

        private bool attackPressed;
        private bool jumpPressed;
        private bool defendPressed;
        private bool inputActionsResolved;
        private bool warnedInputBinding;
        private bool warnedTargetController;

        private void Awake()
        {
            BindPointerSurface(attackImage, BattleControlAction.Attack);
            BindPointerSurface(jumpImage, BattleControlAction.Jump);
            BindPointerSurface(defendImage, BattleControlAction.Defend);
        }

        private void OnEnable()
        {
            ResolveInputActions(logWarnings: false);
        }

        private void OnDisable()
        {
            ReleaseAllActions();
        }

        private void OnDestroy()
        {
            ClearPointerSurface(attackImage);
            ClearPointerSurface(jumpImage);
            ClearPointerSurface(defendImage);
        }

        public void Apply(BattleControlsState state)
        {
            if (state == null || !state.IsVisible)
            {
                ClearVisualState();
                return;
            }

            gameObject.SetActive(true);
            ApplyAction(attackImage, attackPressedState, state.IsAttackHeld);
            ApplyAction(jumpImage, jumpPressedState, state.IsJumpHeld);
            ApplyAction(defendImage, defendPressedState, state.IsDefendHeld);
        }

        public void ClearVisualState()
        {
            gameObject.SetActive(false);
            ApplyAction(attackImage, attackPressedState, false);
            ApplyAction(jumpImage, jumpPressedState, false);
            ApplyAction(defendImage, defendPressedState, false);
        }

        public void CollectMissingBindings(string path, List<string> missing)
        {
            if (missing == null)
                return;

            if (attackImage == null)
                missing.Add(path + ".attackImage");
            if (jumpImage == null)
                missing.Add(path + ".jumpImage");
            if (defendImage == null)
                missing.Add(path + ".defendImage");
        }

        internal void HandlePointerDown(BattleControlAction action)
        {
            SetActionPressed(action, true);
        }

        internal void HandlePointerUp(BattleControlAction action)
        {
            SetActionPressed(action, false);
        }

        private void SetActionPressed(BattleControlAction action, bool pressed)
        {
            if (IsActionPressed(action) == pressed)
                return;

            SetActionPressedState(action, pressed);
            ApplyAction(GetActionImage(action), GetPressedState(action), pressed);

            if (!ResolveInputActions(logWarnings: true))
                return;

            CharacterInputModule inputController = pressed
                ? ResolvePlayerInputController()
                : GetInputOwner(action);
            if (inputController == null)
            {
                if (!warnedTargetController)
                {
                    Debug.LogWarning(
                        $"[BattleControlsView] No human CharacterInputModule was found for Player_{playerId}.");
                    warnedTargetController = true;
                }
                return;
            }

            if (!TryDispatchAction(action, inputController, pressed))
                return;

            SetInputOwner(action, pressed ? inputController : null);
        }

        private bool ResolveInputActions(bool logWarnings)
        {
            if (inputActionsResolved)
                return true;

            AppManager appManager = AppManager.Instance;
            if (appManager == null || appManager.InputModule == null)
            {
                WarnInputBinding(
                    logWarnings,
                    "AppManager.InputModule is not ready; the battle action buttons are not bound yet.");
                return false;
            }

            actionMap = appManager.InputModule.GetActionMapByPlayerID(playerId);
            if (actionMap == null)
            {
                WarnInputBinding(
                    logWarnings,
                    $"The NTSDInputConfig action map Player_{playerId} was not found.");
                return false;
            }

            attackAction = actionMap.FindAction(AttackActionName, throwIfNotFound: false);
            jumpAction = actionMap.FindAction(JumpActionName, throwIfNotFound: false);
            defendAction = actionMap.FindAction(DefendActionName, throwIfNotFound: false);

            inputActionsResolved = attackAction != null &&
                                   jumpAction != null &&
                                   defendAction != null;
            if (!inputActionsResolved)
            {
                WarnInputBinding(
                    logWarnings,
                    $"Player_{playerId} must contain Attack, Jump and Defend actions.");
                return false;
            }

            // CharacterInputModule shares this map for keyboard input. The view only
            // enables it when necessary and never disables it during teardown.
            actionMap.Enable();
            return true;
        }

        private CharacterInputModule ResolvePlayerInputController()
        {
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            BattleSlotRuntimeState[] slots = world?.Runtime?.Roster?.Slots;
            if (world == null || slots == null)
                return null;

            for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
            {
                BattleSlotRuntimeState slot = slots[slotIndex];
                if (slot == null || !slot.Active || !slot.IsHuman || slot.InputId != playerId)
                    continue;

                if (slot.RuntimeSlotIndex < 0)
                    continue;

                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(
                    slot.RuntimeSlotIndex);
                if (entity is LF2LivingObject livingObject &&
                    livingObject.Controller is CharacterInputModule inputController)
                {
                    return inputController;
                }
            }

            return null;
        }

        private bool TryDispatchAction(
            BattleControlAction action,
            CharacterInputModule inputController,
            bool pressed)
        {
            InputAction inputAction = GetInputAction(action);
            if (inputAction == null)
                return false;

            // These three calls intentionally use the same entry points as the
            // generated InputAction performed/canceled callbacks. The controller
            // remains responsible for the NTSD logical-key cross mapping and tick buffer.
            if (inputAction == attackAction)
            {
                inputController.SetAttackActionPressed(pressed);
                return true;
            }

            if (inputAction == jumpAction)
            {
                inputController.SetJumpActionPressed(pressed);
                return true;
            }

            if (inputAction == defendAction)
            {
                inputController.SetDefendActionPressed(pressed);
                return true;
            }

            return false;
        }

        private void BindPointerSurface(Image image, BattleControlAction action)
        {
            if (image == null)
                return;

            BattleControlPointerRelay relay =
                image.GetComponent<BattleControlPointerRelay>() ??
                image.gameObject.AddComponent<BattleControlPointerRelay>();
            relay.Bind(this, action);
        }

        private void ClearPointerSurface(Image image)
        {
            if (image == null)
                return;

            image.GetComponent<BattleControlPointerRelay>()?.ClearOwner(this);
        }

        private void ReleaseAllActions()
        {
            SetActionPressed(BattleControlAction.Attack, false);
            SetActionPressed(BattleControlAction.Jump, false);
            SetActionPressed(BattleControlAction.Defend, false);
        }

        private void WarnInputBinding(bool logWarnings, string message)
        {
            if (!logWarnings || warnedInputBinding)
                return;

            warnedInputBinding = true;
            Debug.LogWarning($"[BattleControlsView] {message}");
        }

        private InputAction GetInputAction(BattleControlAction action)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    return attackAction;
                case BattleControlAction.Jump:
                    return jumpAction;
                case BattleControlAction.Defend:
                    return defendAction;
                default:
                    return null;
            }
        }

        private Image GetActionImage(BattleControlAction action)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    return attackImage;
                case BattleControlAction.Jump:
                    return jumpImage;
                case BattleControlAction.Defend:
                    return defendImage;
                default:
                    return null;
            }
        }

        private GameObject GetPressedState(BattleControlAction action)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    return attackPressedState;
                case BattleControlAction.Jump:
                    return jumpPressedState;
                case BattleControlAction.Defend:
                    return defendPressedState;
                default:
                    return null;
            }
        }

        private bool IsActionPressed(BattleControlAction action)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    return attackPressed;
                case BattleControlAction.Jump:
                    return jumpPressed;
                case BattleControlAction.Defend:
                    return defendPressed;
                default:
                    return false;
            }
        }

        private void SetActionPressedState(BattleControlAction action, bool pressed)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    attackPressed = pressed;
                    break;
                case BattleControlAction.Jump:
                    jumpPressed = pressed;
                    break;
                case BattleControlAction.Defend:
                    defendPressed = pressed;
                    break;
            }
        }

        private CharacterInputModule GetInputOwner(BattleControlAction action)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    return attackInputOwner;
                case BattleControlAction.Jump:
                    return jumpInputOwner;
                case BattleControlAction.Defend:
                    return defendInputOwner;
                default:
                    return null;
            }
        }

        private void SetInputOwner(
            BattleControlAction action,
            CharacterInputModule inputController)
        {
            switch (action)
            {
                case BattleControlAction.Attack:
                    attackInputOwner = inputController;
                    break;
                case BattleControlAction.Jump:
                    jumpInputOwner = inputController;
                    break;
                case BattleControlAction.Defend:
                    defendInputOwner = inputController;
                    break;
            }
        }

        private void ApplyAction(Image image, GameObject pressedState, bool pressed)
        {
            if (image != null)
                image.color = pressed ? pressedColor : normalColor;
            if (pressedState != null && pressedState.activeSelf != pressed)
                pressedState.SetActive(pressed);
        }
    }

    internal enum BattleControlAction
    {
        Attack,
        Jump,
        Defend,
    }

    /// <summary>
    /// Runtime-only pointer bridge for the scene's Image action surfaces.
    /// </summary>
    internal sealed class BattleControlPointerRelay : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {
        private BattleControlsView owner;
        private BattleControlAction action;

        public void Bind(BattleControlsView nextOwner, BattleControlAction nextAction)
        {
            owner = nextOwner;
            action = nextAction;
        }

        public void ClearOwner(BattleControlsView expectedOwner)
        {
            if (owner == expectedOwner)
                owner = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            owner?.HandlePointerDown(action);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            owner?.HandlePointerUp(action);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            owner?.HandlePointerUp(action);
        }

        private void OnDisable()
        {
            owner?.HandlePointerUp(action);
        }
    }
}
