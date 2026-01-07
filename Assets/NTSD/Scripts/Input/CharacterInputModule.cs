using BeatEmUpTemplate2D;
using Loxodon.Framework.Localizations;
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
    public sealed class CharacterInputModule : ICharacterModule, ISimObject
    {
        private Character _hub;
        private bool _inputBound;

        private InputActionMap _inputActionMap;

        public SimInputBuffer InputBuffer { get; private set; }

        public InputAction MoveAction { get; private set; }
        public InputAction AttackAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction DefendAction { get; private set; }

        // 按键状态（用于 CharacterStates 查询）
        private bool _isDefending;
        private bool _isAttacking;
        private bool _isJumping;
        private Vector2 _currentMoveInput = Vector2.zero;
        public Vector2 CurrentMoveInput => _currentMoveInput;

        // Step D5: 方向键追踪（支持同帧多方向）
        private FuncKeyMask _lastDirectionMask = FuncKeyMask.None;
        private const float DIRECTION_DEADZONE = 0.3f;
        private bool _leftPressed;
        private bool _rightPressed;
        public bool IsLeft => _leftPressed;
        public bool IsRight => _rightPressed;

        public int ModuleOrder => CharacterModuleOrder.Input;

        public int SimOrder => 40;

        public int StableId { get; private set; }
        public void SetStableId(int stableId) => StableId = stableId;

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
                Debug.LogError($"[CharacterInput] MoveAction comboKey: value={value}");
                FuncKeyMask newDirectionMask = FuncKeyMask.None;

                if (value.x < -DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.Left;
                    _leftPressed = true;
                }
                if (value.x > DIRECTION_DEADZONE)
                {
                    _rightPressed = true;
                    newDirectionMask |= FuncKeyMask.Right;
                }
                if (value.y > DIRECTION_DEADZONE) newDirectionMask |= FuncKeyMask.Up;
                if (value.y < -DIRECTION_DEADZONE) newDirectionMask |= FuncKeyMask.Down;

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
                if ((_lastDirectionMask & FuncKeyMask.Up) != 0) InputBuffer?.EnqueueForNextTick(FuncKeyMask.Up, down: false);
                if ((_lastDirectionMask & FuncKeyMask.Down) != 0) InputBuffer?.EnqueueForNextTick(FuncKeyMask.Down, down: false);

                _currentMoveInput = Vector2.zero;
                Debug.LogError("_currentMoveInput:" + _currentMoveInput);
                _lastDirectionMask = FuncKeyMask.None;
            }
        }

        private void CheckAndEnqueueDirectionChange(FuncKeyMask direction, FuncKeyMask oldMask, FuncKeyMask newMask)
        {
            bool wasPressed = (oldMask & direction) != 0;
            bool isPressed = (newMask & direction) != 0;

            if (!wasPressed && isPressed)
            {
                Debug.LogError("down: true:         " + direction);

                InputBuffer?.EnqueueForNextTick(direction, down: true);
            }
            else if (wasPressed && !isPressed)
            {
                Debug.LogError("down: false");

                InputBuffer?.EnqueueForNextTick(direction, down: false);
            }
        }

        public void OnAdded(SimContext ctx)
        {
        }

        public void OnRemoved(SimContext ctx)
        {
        }

        public void SimTick(int tickIndex)
        {
            if (InputBuffer == null) return;

            Debug.LogErrorFormat("MoveAction.IsPressed() : {0}", MoveAction.IsPressed());
            Debug.LogErrorFormat("MoveAction.IsInProgress() : {0}", MoveAction.IsInProgress());
            if (MoveAction.IsPressed() && !_currentMoveInput.Equals(Vector2.zero)) 
            {
                FuncKeyMask newDirectionMask = FuncKeyMask.None;
                if (_currentMoveInput.x < -DIRECTION_DEADZONE)
                {
                    newDirectionMask |= FuncKeyMask.Left;
                    _leftPressed = true;
                }
                if (_currentMoveInput.x > DIRECTION_DEADZONE)
                {
                    _rightPressed = true;
                    newDirectionMask |= FuncKeyMask.Right;
                }
                if (_currentMoveInput.y > DIRECTION_DEADZONE) newDirectionMask |= FuncKeyMask.Up;
                if (_currentMoveInput.y < -DIRECTION_DEADZONE) newDirectionMask |= FuncKeyMask.Down;

                if (InputBuffer.BufferedTickCount <= 0)
                {
                    _lastDirectionMask = FuncKeyMask.None;
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Left, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Right, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Up, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Down, _lastDirectionMask, newDirectionMask);
                    _lastDirectionMask = newDirectionMask;
                }
            }
            if (AttackAction.IsPressed()) 
            {
            
            }

            if (JumpAction.IsPressed()) 
            {
            
            }

            if (DefendAction.IsPressed()) 
            {
            
            }
        }
    }
}

