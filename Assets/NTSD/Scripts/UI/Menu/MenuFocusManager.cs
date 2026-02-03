using NTSD.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.UI.Menu
{
    public class MenuFocusManager : SingletonBehaviour<MenuFocusManager>
    {
        public static bool IsUIActive => Instance != null && Instance.HasFocus;

        [Header("Settings")]
        [SerializeField] private float navigationCooldown = 0.15f;

        private readonly Stack<IMenuFocusable> focusStack = new Stack<IMenuFocusable>();
        private NTSDInputConfig inputConfig;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction jumpAction;

        private float lastNavigationTime;

        public IMenuFocusable Current => focusStack.Count > 0 ? focusStack.Peek() : null;
        public bool HasFocus => focusStack.Count > 0;

        protected override void OnSingletonAwake()
        {
            inputConfig = new NTSDInputConfig();
            var player1 = inputConfig.Player_1;

            moveAction = player1.Move;
            attackAction = player1.Attack;
            jumpAction = player1.Jump;
        }

        private void OnEnable()
        {
            if (inputConfig == null) return;

            inputConfig.Player_1.Enable();

            moveAction.performed += OnNavigatePerformed;
            attackAction.performed += OnConfirmPerformed;
            jumpAction.performed += OnCancelPerformed;
        }

        private void OnDisable()
        {
            if (inputConfig == null) return;

            moveAction.performed -= OnNavigatePerformed;
            attackAction.performed -= OnConfirmPerformed;
            jumpAction.performed -= OnCancelPerformed;

            inputConfig.Player_1.Disable();
        }

        protected override void OnSingletonDestroyed()
        {
            if (inputConfig != null)
            {
                inputConfig.Dispose();
                inputConfig = null;
            }
        }

        private void OnConfirmPerformed(InputAction.CallbackContext ctx)
        {
            if (Current == null) return;
            Current.OnConfirm();
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (Current == null) return;
            Current.OnCancel();
        }

        private void OnNavigatePerformed(InputAction.CallbackContext ctx)
        {
            if (Current == null) return;

            if (Time.unscaledTime - lastNavigationTime < navigationCooldown) return;

            Vector2 input = ctx.ReadValue<Vector2>();
            if (input.y != 0f || input.x != 0f)
            {
                Current.OnNavigate(input);
                lastNavigationTime = Time.unscaledTime;
            }
        }

        public void Push(IMenuFocusable focusable)
        {
            if (focusable == null) return;

            Current?.OnFocusExit();
            focusStack.Push(focusable);
            focusable.OnFocusEnter();
        }

        public void Pop()
        {
            if (focusStack.Count == 0) return;

            var popped = focusStack.Pop();
            popped?.OnFocusExit();
            Current?.OnFocusEnter();
        }

        public void Clear()
        {
            while (focusStack.Count > 0)
            {
                var popped = focusStack.Pop();
                popped?.OnFocusExit();
            }
        }
    }
}
