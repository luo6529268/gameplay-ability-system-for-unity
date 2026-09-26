using MoreMountains.TopDownEngine;
using NTSD.App;
using NTSD.Simulation;
using UnityEngine;
using NTSD.Input;
using NTSD.Animation.LF2Objects;
using UnityEngine.InputSystem;

namespace NTSD.Game
{
    /// <summary>
    /// 角色输入模块（纯 C#，不再需要挂载到预制体上）。
    ///
    /// 设计目标：
    /// - 只负责把 Unity InputSystem 的事件写入到 SimInputBuffer（按 tick 对齐）
    /// - 不直接驱动动作、状态或帧切换，连招和帧播放由角色逻辑处理。
    ///
    /// </summary>
    public sealed class CharacterInputModule:
        ILF2Controller,
        ILocalFrameInputSource,
        IPlayerActionInputSink
    {
        private int _explicitInputId = -1;
        private bool _inputBound;

        private NTSD.App.InputModule _inputModule;
        private InputActionMap _inputActionMap;

        public InputAction MoveAction { get; private set; }
        public InputAction AttackAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction DefendAction { get; private set; }

        private Vector2 _currentMoveInput = Vector2.zero;
        public Vector2 CurrentMoveInput => _currentMoveInput;

        // 方向键追踪：支持同帧多方向变化，并写入 tick 对齐输入缓冲。
        private FuncKeyMask _lastDirectionMask = FuncKeyMask.None;
        private const float DIRECTION_DEADZONE = 0.3f;
        private bool _leftPressed;
        private bool _rightPressed;
        private bool _downPressed;
        private bool _topPressed;

        // 按键状态（用于 CharacterStates 查询）
        private bool _isDefending;
        private bool _isAttacking;
        private bool _isJumping;
        private bool _uiDefending;
        private bool _uiAttacking;
        private bool _uiJumping;
        private bool _deviceDefending;
        private bool _deviceAttacking;
        private bool _deviceJumping;

        bool ILF2Controller.IsUp => _topPressed;

        bool ILF2Controller.IsDown => _downPressed;

        bool ILF2Controller.IsLeft => _leftPressed;

        bool ILF2Controller.IsRight => _rightPressed;

        bool ILF2Controller.IsAttack => _isAttacking;

        bool ILF2Controller.IsJump => _isJumping;

        bool ILF2Controller.IsDefend => _isDefending;

        public SimInputBuffer InputBuffer { get ; set; }

        SimulationInputButtons ILocalFrameInputSource.CaptureHeldSimulationButtons()
        {
            SimulationInputButtons buttons = SimulationInputButtons.None;
            if (_rightPressed) buttons |= SimulationInputButtons.Right;
            if (_leftPressed) buttons |= SimulationInputButtons.Left;
            if (_topPressed) buttons |= SimulationInputButtons.Up;
            if (_downPressed) buttons |= SimulationInputButtons.Down;

            // Unity action names describe the physical layout. The existing NTSD input
            // contract crosses these three actions when they enter the logical key buffer.
            if (_isDefending) buttons |= SimulationInputButtons.Attack;
            if (_isAttacking) buttons |= SimulationInputButtons.Jump;
            if (_isJumping) buttons |= SimulationInputButtons.Defend;
            return buttons;
        }

        public CharacterInputModule() 
        {
            InputBuffer ??= new SimInputBuffer();
        }

        /// <summary>
        /// 设置显式输入 ID，用于战斗对象池中的角色绑定玩家输入。
        /// </summary>
        public void SetInputID(int inputId)
        {
            _explicitInputId = inputId;
            ModuleBind();
        }

        public void ModuleBind()
        {
            // 已绑定时直接返回，避免重复注册输入回调。
            if (_inputBound) return;

            BindActionMap();
            BindInputEvents();
            _inputBound = true;
            _inputModule?.RegisterPlayerActionInput(_explicitInputId, this);
        }

        public void ModuleUnbind()
        {
            _inputModule?.UnregisterPlayerActionInput(_explicitInputId, this);

            if (_inputBound)
                UnbindInputEvents();

            _inputBound = false;
            _inputModule = null;
            _currentMoveInput = Vector2.zero;
            _lastDirectionMask = FuncKeyMask.None;
            _leftPressed = false;
            _rightPressed = false;
            _topPressed = false;
            _downPressed = false;
            _isAttacking = false;
            _isJumping = false;
            _isDefending = false;
            _uiAttacking = false;
            _uiJumping = false;
            _uiDefending = false;
            _deviceAttacking = false;
            _deviceJumping = false;
            _deviceDefending = false;
        }

        public void ResetForPoolReuse()
        {
            ModuleUnbind();
            _explicitInputId = -1;
            _inputActionMap = null;
            MoveAction = null;
            AttackAction = null;
            JumpAction = null;
            DefendAction = null;
        }

        private void BindActionMap()
        {
            int inputId = _explicitInputId >= 0 ? _explicitInputId : 1;
            _inputModule = AppManager.Instance.InputModule;
            _inputActionMap = _inputModule.GetActionMapByPlayerID(inputId);
            _inputActionMap?.Enable();

            MoveAction = _inputActionMap?.FindAction("Move");
            AttackAction = _inputActionMap?.FindAction("Attack");
            JumpAction = _inputActionMap?.FindAction("Jump");
            DefendAction = _inputActionMap?.FindAction("Defend");
        }

        private void BindInputEvents()
        {
            if (MoveAction != null)
            {
                MoveAction.performed += OnInputStarted;
                MoveAction.canceled += OnInputCanceled;
            }

            if (AttackAction != null)
            {
                AttackAction.performed += OnInputStarted;
                AttackAction.canceled += OnInputCanceled;
            }

            if (JumpAction != null)
            {
                JumpAction.performed += OnInputStarted;
                JumpAction.canceled += OnInputCanceled;
            }

            if (DefendAction != null)
            {
                DefendAction.performed += OnInputStarted;
                DefendAction.canceled += OnInputCanceled;
            }
        }

        private void UnbindInputEvents()
        {
            if (MoveAction != null)
            {
                MoveAction.performed -= OnInputStarted;
                MoveAction.canceled -= OnInputCanceled;
            }

            if (AttackAction != null)
            {
                AttackAction.performed -= OnInputStarted;
                AttackAction.canceled -= OnInputCanceled;
            }

            if (JumpAction != null)
            {
                JumpAction.performed -= OnInputStarted;
                JumpAction.canceled -= OnInputCanceled;
            }

            if (DefendAction != null)
            {
                DefendAction.performed -= OnInputStarted;
                DefendAction.canceled -= OnInputCanceled;
            }

            _inputActionMap?.Disable();
            _inputActionMap = null;
        }

        private void OnInputStarted(InputAction.CallbackContext context)
        {
            if (context.action == MoveAction)
            {
                Vector2 value = context.ReadValue<Vector2>();
                _currentMoveInput = value;
                FuncKeyMask newDirectionMask = FuncKeyMask.None;

                if (value.x < -DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.left;
                }
                if (value.x > DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.right;
                }
                if (value.y > DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.up;
                }
                if (value.y < -DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.down;
                }

                _leftPressed = (newDirectionMask & FuncKeyMask.left) != 0;
                _rightPressed = (newDirectionMask & FuncKeyMask.right) != 0;
                _topPressed = (newDirectionMask & FuncKeyMask.up) != 0;
                _downPressed = (newDirectionMask & FuncKeyMask.down) != 0;

                if (newDirectionMask != _lastDirectionMask)
                {
                    CheckAndEnqueueDirectionChange(FuncKeyMask.left, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.right, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.up, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.down, _lastDirectionMask, newDirectionMask);
                    _lastDirectionMask = newDirectionMask;
                }

                return;
            }

            if (context.action == AttackAction)
            {
                SetAttackActionPressed(true);
                return;
            }

            if (context.action == JumpAction)
            {
                SetJumpActionPressed(true);
                return;
            }

            if (context.action == DefendAction)
            {
                SetDefendActionPressed(true);
            }
        }

        private void OnInputCanceled(InputAction.CallbackContext context)
        {
            if (context.action == AttackAction)
            {
                SetAttackActionPressed(false);
                return;
            }

            if (context.action == JumpAction)
            {
                SetJumpActionPressed(false);
                return;
            }

            if (context.action == DefendAction)
            {
                SetDefendActionPressed(false);
                return;
            }

            if (context.action == MoveAction)
            {
                if ((_lastDirectionMask & FuncKeyMask.left) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.left, down: false);
                    _leftPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.right) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.right, down: false);
                    _rightPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.up) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.up, down: false);
                    _topPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.down) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.down, down: false);
                    _downPressed = false;
                }

                _currentMoveInput = Vector2.zero;
                _lastDirectionMask = FuncKeyMask.None;
            }
        }

        // Unity action names describe the physical layout; NTSD uses the crossed internal fields below.
        internal void SetAttackActionPressed(bool pressed)
        {
            _deviceAttacking = pressed;
            SetEffectiveActionPressed(
                ref _isAttacking,
                _deviceAttacking || _uiAttacking,
                FuncKeyMask.jump);
        }

        internal void SetJumpActionPressed(bool pressed)
        {
            _deviceJumping = pressed;
            SetEffectiveActionPressed(
                ref _isJumping,
                _deviceJumping || _uiJumping,
                FuncKeyMask.def);
        }

        internal void SetDefendActionPressed(bool pressed)
        {
            _deviceDefending = pressed;
            SetEffectiveActionPressed(
                ref _isDefending,
                _deviceDefending || _uiDefending,
                FuncKeyMask.att);
        }

        void IPlayerActionInputSink.SetActionPressed(
            BattleInputAction action,
            bool pressed)
        {
            switch (action)
            {
                case BattleInputAction.Attack:
                    _uiAttacking = pressed;
                    SetEffectiveActionPressed(
                        ref _isAttacking,
                        _deviceAttacking || _uiAttacking,
                        FuncKeyMask.jump);
                    break;
                case BattleInputAction.Jump:
                    _uiJumping = pressed;
                    SetEffectiveActionPressed(
                        ref _isJumping,
                        _deviceJumping || _uiJumping,
                        FuncKeyMask.def);
                    break;
                case BattleInputAction.Defend:
                    _uiDefending = pressed;
                    SetEffectiveActionPressed(
                        ref _isDefending,
                        _deviceDefending || _uiDefending,
                        FuncKeyMask.att);
                    break;
            }
        }

        private void SetEffectiveActionPressed(
            ref bool currentState,
            bool nextState,
            FuncKeyMask logicalKey)
        {
            if (currentState == nextState)
                return;

            currentState = nextState;
            InputBuffer?.EnqueueForNextTick(logicalKey, nextState);
        }

        private void CheckAndEnqueueDirectionChange(FuncKeyMask direction, FuncKeyMask oldMask, FuncKeyMask newMask)
        {
            bool wasPressed = (oldMask & direction) != 0;
            bool isPressed = (newMask & direction) != 0;

            if (!wasPressed && isPressed)
            {
                InputBuffer?.EnqueueForNextTick(direction, down: true);
            }
            else if (wasPressed && !isPressed)
            {
                InputBuffer?.EnqueueForNextTick(direction, down: false);
            }
        }

        int ILF2Controller.Dirv()
        {
            int dz = 0;
            if (_topPressed) dz += 1;
            if (_downPressed) dz -= 1;
            return dz;
        }

        (int dx, int dz) ILF2Controller.GetMoveInput()
        {
            int dx = 0, dz = 0;
            {
                if (CurrentMoveInput.x < -0.1f) dx -= 1;
                if (CurrentMoveInput.x > 0.1f) dx += 1;
                if (CurrentMoveInput.y < -0.1f) dz -= 1;
                if (CurrentMoveInput.y > 0.1f) dz += 1;
            }
            return (dx, dz);
        }    }
}
