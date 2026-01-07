using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace BeatEmUpTemplate2D
{

    /**
     * 现代Unity输入系统的输入管理器
     * 这个类实现了单例模式，用于管理游戏中的输入控制
     * 使用Unity的新输入系统(Input System)来处理玩家输入
     */
    public class InputManager : MMSingleton<InputManager>
    {
        [Header("MODERN INPUTMANAGER. v1.0")]
        // 只读属性，显示当前控制方案
        [ReadOnlyProperty] public string controlsScheme;

        // 玩家控制输入系统
        public NTSDInputConfig _NTSDInputConfig;

        private float lastJumpSprintPressTime;
        private float inputBufferTime = 0.2f; // 0.2秒的缓冲时间

        // 各种输入动作的定义
        private InputAction move;      // 移动
        private InputAction punch;     // 拳击
        private InputAction kick;      // 踢击
        private InputAction defend;    // 防御
        private InputAction grab;      // 抓取
        private InputAction jump;      // 跳跃

        /**
         * 唤醒时初始化
         * 设置输入系统并实现单例模式
         */
        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();

            // 初始化玩家控制
            _NTSDInputConfig = new NTSDInputConfig();
        }

        public InputActionMap GetActionMapByPlayerID(int playerID) 
        {
            return _NTSDInputConfig.asset.FindActionMap($"Player_{playerID}");
        }

        public Vector2 GetMoveInputVector(InputActionMap inputActions) 
        {
            return inputActions.FindAction("Move").ReadValue<Vector2>();
        }

        // 获取拳击按键状态
        public static bool PunchKeyDown()
        {
            if (Instance.punch == null)
                return false;

            return Instance.punch.WasPressedThisFrame();
        }

        // 获取踢击按键状态
        public static bool KickKeyDown()
        {
            if (Instance.kick == null)
                return false;
            return Instance.kick.WasPressedThisFrame();
        }

        // 获取防御按键状态
        public static bool DefendKeyDown()
        {
            if (Instance.defend == null)
                return false;
            return Instance.defend.IsPressed();
        }

        // 获取抓取按键状态
        public static bool GrabKeyDown()
        {
            if (Instance.grab == null)
                return false;
            return Instance.grab.WasPressedThisFrame();
        }

        // 获取跳跃按键状态
        public static bool JumpKeyDown(InputAction JumpInputAction = null)
        {
            if (JumpInputAction == null)
                return false;
            return JumpInputAction.WasPressedThisFrame();
        }

        public bool JumpSprintKeyDown(InputAction JumpInputAction = null)
        {
            if (JumpInputAction == null)
                return false;

            if (JumpInputAction.WasPressedThisFrame())
            {
                lastJumpSprintPressTime = Time.time;
                return true;
            }

            // 在缓冲时间内，如果其他按钮也被按下，仍然认为跳跃按钮被按下
            if (Time.time - lastJumpSprintPressTime < inputBufferTime)
            {
                return true;
            }

            return false;
        }

        // 获取方向输入，返回二维向量
        public static Vector2 GetInputVector(InputAction MoveInputAction = null)
        {
            if (MoveInputAction == null)
                return Vector2.zero;

            return MoveInputAction.ReadValue<Vector2>();
        }

        // 检测手柄方向输入
        public static bool JoypadDirInputDetected()
        {
            return (Instance.move.ReadValue<Vector2>().x != 0 || Instance.move.ReadValue<Vector2>().y != 0);
        }
    }

}