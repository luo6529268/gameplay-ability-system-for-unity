using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.Game
{
    /// <summary>
    /// 角色输入模块（纯 C#，不再需要挂载到预制体上）。
    ///
    /// 设计目标：
    /// - 只负责把 Unity InputSystem 的事件写入到 SimInputBuffer（按 tick 对齐）
    /// - 不直接驱动动画/状态，所有帧播放仍由 OnComboDetected → LF2CharacterAnimator 处理
    ///
    /// </summary>
    public sealed class CharacterInputModule : ICharacterModule, ILF2Controller
    {
        private Character _hub;
        private bool _inputBound;

        private InputActionMap _inputActionMap;

        public SimInputBuffer InputBuffer { get; private set; }

        public InputAction MoveAction { get; private set; }
        public InputAction AttackAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction DefendAction { get; private set; }

        private Vector2 _currentMoveInput = Vector2.zero;
        public Vector2 CurrentMoveInput => _currentMoveInput;

        // Step D5: 方向键追踪（支持同帧多方向）
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

        bool ILF2Controller.IsUp => _topPressed;

        bool ILF2Controller.IsDown => _downPressed;

        bool ILF2Controller.IsLeft => _leftPressed;

        bool ILF2Controller.IsRight => _rightPressed;

        bool ILF2Controller.IsAttack => _isAttacking;

        bool ILF2Controller.IsJump => _isJumping;

        bool ILF2Controller.IsDefend => _isDefending;

        public void ModuleSetup(Character character)
        {
            _hub = character;
        }

        public void ModuleInitialize()
        {
            InputBuffer ??= new SimInputBuffer();
        }

        public void ModuleBind()
        {
            // Player-only（AI 未来会有独立输入源/行为树等）
            if (_hub != null && _hub.CharacterType != Character.CharacterTypes.Player) return;
            if (_inputBound) return;

            BindActionMap();
            BindInputEvents();
            _inputBound = true;
        }

        public void ModuleUnbind()
        {
            if (!_inputBound) return;
            UnbindInputEvents();
            _inputBound = false;
            _currentMoveInput = Vector2.zero;
            _lastDirectionMask = FuncKeyMask.None;
            _leftPressed = false;
            _rightPressed = false;
            _topPressed = false;
            _downPressed = false;
        }

        private void BindActionMap()
        {
            int inputId = _hub != null ? _hub.InputID : 0;
            _inputActionMap = InputManager.Instance.GetActionMapByPlayerID(inputId);
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
                _isAttacking = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.att, down: true);
                return;
            }

            if (context.action == JumpAction)
            {
                _isJumping = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.jump, down: true);
                return;
            }

            if (context.action == DefendAction)
            {
                _isDefending = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.def, down: true);
            }
        }

        private void OnInputCanceled(InputAction.CallbackContext context)
        {
            if (context.action == AttackAction)
            {
                _isAttacking = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.att, down: false);
                return;
            }

            if (context.action == JumpAction)
            {
                _isJumping = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.jump, down: false);
                return;
            }

            if (context.action == DefendAction)
            {
                _isDefending = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.def, down: false);
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
        }
    }
}

