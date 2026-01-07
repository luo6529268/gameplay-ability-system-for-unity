using MoreMountains.Tools;
using NTSD.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace BeatEmUpTemplate2D {

    //state Machine class
    // Legacy AI state machine (BeatEmUpTemplate2D).
    // NTSD/FLF-style gameplay uses `CharacterStates` + `LF2CharacterAnimator` instead.
    // Keep this component opt-in to avoid running unused Update/FixedUpdate loops on the unified Character prefab.
    public class StateMachine : UnitActions {
    
        [Header("Legacy Toggle")]
        [SerializeField]
        private bool _enableLegacyStateMachine = false;

        [SerializeField] private bool showStateInGame; // 在游戏运行时是否显示当前状态（显示在此单位下方的文本框中）
        [SerializeField] private string profileKey;

        [MMReadOnly] public string currentState; // 用于在Unity检视面板中显示当前状态的字符串（只读属性）

        private Type _currentStateType;
        private string _currentStateShortName = "";
        private StateNode state; // 当前状态对象
        private StatePool _statePool;

        [SerializeField] private int _stateHistorySize = 10;
        [ReadOnlyProperty] public Queue<string> _stateHistory; // 用于记录状态历史记录的队列（只读属性）

        private void Awake()
        {
            if (!_enableLegacyStateMachine)
            {
                enabled = false;
                return;
            }

            _statePool = new StatePool();
            _stateHistory = new Queue<string>();

            // 设置单位的初始状态
            if (isEnemy) SetState<EnemyIdle>(); // 如果单位是敌人，则切换到敌人待机状态
        }

        public void SetState(StateNode _state){
        
            // 退出当前状态
            // 检查当前状态是否存在，如果存在则调用其Exit()方法进行状态退出处理
            if (this.state != null) state.Exit();
       
            // 设置新状态
            // 将传入的新状态赋值给当前状态变量
            state = _state;
            // 将当前单元实例赋给新状态，使状态可以访问单元的相关信息
            state.unit = this;

            _currentStateType = state?.GetType();

            _currentStateShortName = GetShortNameFromType(_currentStateType);
            // 设置状态数据
            // 获取当前状态的短名称，用于调试信息显示
            currentState = _currentStateShortName; //debug info
            // 记录状态开始的时间，用于计算状态持续时间
            state.stateStartTime = Time.time;

            // 进入新状态
            // 调用新状态的Enter()方法，执行状态进入时的初始化操作
            state.Enter();

        }
        // 泛型方法用于其他状态
        public void SetState<T>() where T : StateNode, new()
        {
            SetStateInternal(_statePool.GetState<T>());
        }

        public StateNode GetState<T>() where T : StateNode, new() 
        {
            return _statePool.GetState<T>();
        }

        public StateNode GetCurrentState(){
            return state;;
        }

        /// <summary>
        /// 获取当前状态（泛型版本）
        /// </summary>
        /// <typeparam name="T">状态类型</typeparam>
        /// <returns>指定类型的当前状态，如果类型不匹配则返回null</returns>
        public T GetCurrentState<T>() where T : StateNode
        {
            return state as T;
        }

        /// <summary>
        /// 检查当前状态是否为指定类型
        /// </summary>
        /// <typeparam name="T">要检查的状态类型</typeparam>
        /// <returns>如果当前状态是指定类型则返回true</returns>
        public bool IsCurrentState<T>() where T : StateNode
        {
            return state is T;
        }

        void Update()
        {
            state?.Update();

        }

        void LateUpdate()
        {
            state?.LateUpdate();
        }

        void FixedUpdate()
        {
            state?.FixedUpdate();
        }

        private void SetStateInternal(StateNode newState)
        {
            if (state != null)
            {
                state.Exit();
                // 只有无参状态才能放入对象池
                if (state.GetType().GetConstructors().Any(c => c.GetParameters().Length == 0))
                {
                    _statePool.ReturnState(state);
                }
            }

            state = newState;
            state.unit = this;
            state.stateStartTime = Time.time;
            state.Enter();
            state.unit.animator.Update(0);

            _currentStateType = state?.GetType();
            _currentStateShortName = GetShortNameFromType(_currentStateType);

            currentState = _currentStateShortName;

            string stateEntry = $"{Time.time:F2}: {currentState}";
            _stateHistory.Enqueue(stateEntry);
            if (_stateHistory.Count > _stateHistorySize)
            {
                _stateHistory.Dequeue();
            }
        }

        /// <summary>
        /// 获取当前状态的短名称（不包含命名空间）
        /// </summary>
        /// <returns>返回状态的短名称，如果无法获取则返回空字符串</returns>
        string GetCurrentStateShortName(){
            // 获取当前状态的完整类型名称
            string currentState = stateMachine?.GetCurrentState().GetType().ToString();
            // 通过点号分割字符串，获取命名空间和类名
            string[] splitStrings = currentState.Split('.');                  
            // 如果分割后的数组长度大于等于2，返回第二个元素（类名）
            if(splitStrings.Length >= 2) return splitStrings[1];
            // 否则返回空字符串
            return "";
        }

        // 基于 Type 获取类名（不包含命名空间），无额外分配 ToString()/Split
        string GetShortNameFromType(Type t)
        {
            if (t == null) return "";
            return t.Name ?? "";
        }
    }
}
