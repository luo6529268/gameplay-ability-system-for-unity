using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
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
    /// 参考：原 CharacterInput.cs（MonoBehaviour 版本）
    /// </summary>
    public sealed class CharacterInputModule : ICharacterModule
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

        public bool IsLeft => _leftPressed;
        public bool IsRight => _rightPressed;
        public bool IsDown => _downPressed;
        public bool IsTop => _topPressed;
        public bool IsDef => _isDefending;
        public bool IsAtt => _isAttacking;
        public bool IsJump => _isJumping;

        public int ModuleOrder => CharacterModuleOrder.Input;

        public int Dirv 
        {
            get
            {
                int dz = 0;
                if (_topPressed) dz += 1;
                if (_downPressed) dz -= 1;
                return dz;
            }
        }

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
                    newDirectionMask |= FuncKeyMask.Left;
                }
                if (value.x > DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.Right;
                }
                if (value.y > DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.Up;
                }
                if (value.y < -DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.Down;
                }

                _leftPressed = (newDirectionMask & FuncKeyMask.Left) != 0;
                _rightPressed = (newDirectionMask & FuncKeyMask.Right) != 0;
                _topPressed = (newDirectionMask & FuncKeyMask.Up) != 0;
                _downPressed = (newDirectionMask & FuncKeyMask.Down) != 0;

                if (newDirectionMask != _lastDirectionMask)
                {
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Left, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Right, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Up, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Down, _lastDirectionMask, newDirectionMask);
                    _lastDirectionMask = newDirectionMask;
                }

                return;
            }

            if (context.action == AttackAction)
            {
                _isAttacking = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Attack, down: true);
                return;
            }

            if (context.action == JumpAction)
            {
                _isJumping = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Jump, down: true);
                return;
            }

            if (context.action == DefendAction)
            {
                _isDefending = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Defend, down: true);
            }
        }

        private void OnInputCanceled(InputAction.CallbackContext context)
        {
            if (context.action == AttackAction)
            {
                _isAttacking = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Attack, down: false);
                return;
            }

            if (context.action == JumpAction)
            {
                _isJumping = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Jump, down: false);
                return;
            }

            if (context.action == DefendAction)
            {
                _isDefending = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Defend, down: false);
                return;
            }

            if (context.action == MoveAction)
            {
                if ((_lastDirectionMask & FuncKeyMask.Left) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Left, down: false);
                    _leftPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.Right) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Right, down: false);
                    _rightPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.Up) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Up, down: false);
                    _topPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.Down) != 0)
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Down, down: false);
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

    }
}

