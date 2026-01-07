using BeatEmUpTemplate2D;
using GAS.Runtime;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Input;
using NTSD.Simulation;  // Plan B: SimInputBuffer
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.Game
{
    /// <summary>
    /// 角色输入处理器 - 复刻 LF2 真实逻辑
    ///
    /// 核心逻辑（基于 LF2 源码分析）：
    /// 1. **所有按键都记录到 ActionSequenceDetector**（包括单键）
    /// 2. **ActionSequenceDetector 检测连招**（单键也作为"连招"）
    /// 3. **所有按键都通过 OnComboDetected 触发**（没有单独的基础帧处理）
    /// 4. **CharacterInput 只负责记录输入和播放帧**
    ///
    /// 关键差异（与之前错误实现的对比）：
    /// - ❌ 防御键不"立即播放"，而是通过 OnComboDetected 触发
    /// - ❌ 所有方向键都正常记录，不做防御期间的特殊处理
    /// - ❌ 没有 OnNoComboMatched 事件，单键本身就是"连招"
    ///
    /// 参考文件：
    /// - I:\C++Test\NTSD\F.LF-master\LF\character.js (combo_event, combo_update)
    /// - I:\C++Test\NTSD\F.LF-master\core\combodec.js (连招检测逻辑)
    /// - I:\C++Test\NTSD\LF2按键系统真实逻辑分析.md (详细分析文档)
    /// </summary>
    // Legacy: kept for reference/backward compatibility. New implementation is `CharacterInputModule` (pure C#).
    public class CharacterInput : InputBase
    {
        [SerializeField] private int ID;

        // Step D4: 移除 _Character 和 _ActionSequenceDetector 字段（Hub 注入规则）
        // ActionSequenceDetector 通过 Character.Initialization() 调用 SetCharacterInput() 注入
        // CharacterInput 不需要反向引用这些组件
        InputActionMap _InputActionMap;

        // Plan B: Tick-Aligned Input Buffer
        /// <summary>
        /// 输入缓冲区（按 tickIndex 存储输入事件）
        /// Unity InputSystem 回调写入，ActionSequenceDetector.SimTick 消费
        /// </summary>
        [HideInInspector] public SimInputBuffer InputBuffer;

        [HideInInspector]
        public InputAction MoveAction;
        [HideInInspector]
        public InputAction AttackAction;
        [HideInInspector]
        public InputAction JumpAction;
        [HideInInspector]
        public InputAction DefendAction;

        public bool IsLeft => leftPressed;
        public bool IsRight => rightPressed;

        // ==================== 按键状态（对应 LF2 的 con.state）====================

        /// <summary>
        /// 防御键是否按下
        /// 对应 LF2: $.con.state.def
        /// </summary>
        private bool _isDefending = false;

        /// <summary>
        /// 攻击键是否按下
        /// 对应 LF2: $.con.state.att
        /// </summary>
        private bool _isAttacking = false;

        /// <summary>
        /// 跳跃键是否按下
        /// 对应 LF2: $.con.state.jump
        /// </summary>
        private bool _isJumping = false;

        /// <summary>
        /// 当前方向键输入
        /// 对应 LF2: $.con.state.left/right/up/down
        /// </summary>
        private Vector2 _currentMoveInput = Vector2.zero;

        /// <summary>
        /// 公共属性：获取当前移动方向输入（供 CharacterStates 使用）
        /// </summary>
        public Vector2 CurrentMoveInput => _currentMoveInput;

        // Step D5: 方向键追踪（支持同时按下多个方向）
        /// <summary>
        /// 上一帧的方向键状态（bitmask，支持同时多个方向）
        /// 用于检测方向变化：只有变化时才 enqueue 到 buffer
        /// 避免 MoveAction.performed 高频 spam（Unity InputSystem 会持续触发 performed）
        ///
        /// FLF 语义：con.state.left/right/up/down 是四个独立 bool，可同时为 true
        /// </summary>
        private FuncKeyMask _lastDirectionMask = FuncKeyMask.None;

        /// <summary>
        /// 方向键检测死区（避免摇杆漂移触发方向输入）
        /// </summary>
        private const float DIRECTION_DEADZONE = 0.3f;

        bool leftPressed, rightPressed = false;

        // ==================== Scheme C: Character Module Lifecycle ====================
        private Character _hub;
        private bool _inputBound = false;

        public int ModuleOrder => CharacterModuleOrder.Input;

        public void ModuleSetup(Character character)
        {
            _hub = character;
        }

        public void ModuleInitialize()
        {
            // Initialize Input Buffer (independent of CharacterID)
            InputBuffer ??= new SimInputBuffer();
        }

        public void ModuleBind()
        {
            // Player-only (AI will have its own input source later)
            if (_hub != null && _hub.CharacterType != Character.CharacterTypes.Player) return;

            if (_inputBound) return;

            OnInitNTSDInputConfig();
            BindInputEvents();
            _inputBound = true;
        }

        public void ModuleUnbind()
        {
            if (!_inputBound) return;
            UnBindInputEvent();
            _inputBound = false;
        }

        // ==================== Unity 生命周期 ====================
        private void OnDestroy()
        {
            UnBindInputEvent();
        }

        // ==================== 初始化 ====================

        private void OnInitNTSDInputConfig()
        {
            _InputActionMap = InputManager.Instance.GetActionMapByPlayerID(ID);
            _InputActionMap?.Enable();

            MoveAction = _InputActionMap.FindAction("Move");
            AttackAction = _InputActionMap.FindAction("Attack");
            JumpAction = _InputActionMap.FindAction("Jump");
            DefendAction = _InputActionMap.FindAction("Defend");
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

        private void UnBindInputEvent()
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

            _InputActionMap?.Disable();
        }

        // ==================== 输入处理（对应 LF2 的 combo 事件）====================

        /// <summary>
        /// 统一处理所有输入的 performed 事件（Plan B: 改为写入 InputBuffer）
        ///
        /// 对应 LF2 逻辑：
        /// - 记录所有按键到 combodec（包括单键）
        /// - 不做任何特殊处理（如防御期间方向键）
        /// - 所有帧播放逻辑通过 OnComboDetected 触发
        ///
        /// Plan B 修改：
        /// - ❌ 禁止直接调用 ActionSequenceDetector.RecordAction()
        /// - ✅ 写入到 InputBuffer.EnqueueForNextTick()
        /// - ✅ 方向键转换为离散 left/right/up/down 按下事件（FLF con.state 语义）
        ///
        /// 参考：combodec.js:91-147
        /// </summary>
        private void OnInputStarted(InputAction.CallbackContext context)
        {
            // ================ 方向键处理（Step D5: 支持同时按下多个方向）================
            if (context.action == MoveAction)
            {
                Vector2 value = context.ReadValue<Vector2>();
                _currentMoveInput = value;
                Debug.LogError($"[CharacterInput] MoveAction comboKey: value={value}");
                // Step D5: 构建当前方向 mask（支持同时多个方向）
                // FLF 语义：con.state.left/right/up/down 是四个独立 bool
                FuncKeyMask newDirectionMask = FuncKeyMask.None;

                // 独立检测四个方向（不使用 else-if）
                if (value.x < -DIRECTION_DEADZONE) 
                {
                    newDirectionMask |= FuncKeyMask.Left;
                    leftPressed = true;
                }
                if (value.x > DIRECTION_DEADZONE) 
                {
                    rightPressed = true;
                    newDirectionMask |= FuncKeyMask.Right;
                }
                if (value.y > DIRECTION_DEADZONE)
                    newDirectionMask |= FuncKeyMask.Up;
                if (value.y < -DIRECTION_DEADZONE)
                    newDirectionMask |= FuncKeyMask.Down;

                // 检测每个方向位的变化，只在变化时 enqueue
                if (newDirectionMask != _lastDirectionMask)
                {
                    // 检测每个方向的变化
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Left, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Right, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Up, _lastDirectionMask, newDirectionMask);
                    CheckAndEnqueueDirectionChange(FuncKeyMask.Down, _lastDirectionMask, newDirectionMask);

                    _lastDirectionMask = newDirectionMask;
                }
            }
            // ================ 攻击键处理 ================
            else if (context.action == AttackAction)
            {
                _isAttacking = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Attack, down: true);
            }
            // ================ 跳跃键处理 ================
            else if (context.action == JumpAction)
            {
                _isJumping = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Jump, down: true);
            }
            // ================ 防御键处理 ================
            else if (context.action == DefendAction)
            {
                _isDefending = true;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Defend, down: true);
            }
        }

        /// <summary>
        /// 统一处理所有输入的 canceled 事件（Plan B: 改为写入 InputBuffer）
        ///
        /// Plan B 修改：
        /// - ❌ 禁止直接调用 ActionSequenceDetector.OnKeyUp()
        /// - ✅ 写入到 InputBuffer.EnqueueForNextTick(..., down: false)
        /// - ✅ MoveAction.canceled 时 enqueue 所有 4 个方向的 key-up
        /// </summary>
        private void OnInputCanceled(InputAction.CallbackContext context)
        {
            if (context.action == AttackAction)
            {
                _isAttacking = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Attack, down: false);
            }
            else if (context.action == JumpAction)
            {
                _isJumping = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Jump, down: false);
            }
            else if (context.action == DefendAction)
            {
                _isDefending = false;
                InputBuffer?.EnqueueForNextTick(FuncKeyMask.Defend, down: false);
            }
            else if (context.action == MoveAction)
            {
                // Step D5: MoveAction.canceled 时只释放当前 mask 中的方向
                // （不假设单方向，支持同时按下多个方向）
                if ((_lastDirectionMask & FuncKeyMask.Left) != 0) 
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Left, down: false);
                    leftPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.Right) != 0) 
                {
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Right, down: false);
                    rightPressed = false;
                }
                if ((_lastDirectionMask & FuncKeyMask.Up) != 0)
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Up, down: false);
                if ((_lastDirectionMask & FuncKeyMask.Down) != 0)
                    InputBuffer?.EnqueueForNextTick(FuncKeyMask.Down, down: false);

                _currentMoveInput = Vector2.zero;
                _lastDirectionMask = FuncKeyMask.None;  // 重置方向追踪
            }
        }

        // ==================== Step D5: 辅助方法 ====================

        /// <summary>
        /// 检测单个方向位的变化并 enqueue 对应事件
        /// </summary>
        /// <param name="direction">要检测的方向位</param>
        /// <param name="oldMask">旧的方向 mask</param>
        /// <param name="newMask">新的方向 mask</param>
        private void CheckAndEnqueueDirectionChange(FuncKeyMask direction, FuncKeyMask oldMask, FuncKeyMask newMask)
        {
            bool wasPressed = (oldMask & direction) != 0;
            bool isPressed = (newMask & direction) != 0;

            if (!wasPressed && isPressed)
            {
                // 由 false -> true: 按下事件
                InputBuffer?.EnqueueForNextTick(direction, down: true);
            }
            else if (wasPressed && !isPressed)
            {
                // 由 true -> false: 抬起事件
                InputBuffer?.EnqueueForNextTick(direction, down: false);
            }
            // 否则：状态未变化，不 enqueue
        }
    }
}
