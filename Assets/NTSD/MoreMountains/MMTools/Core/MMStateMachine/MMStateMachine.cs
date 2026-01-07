using UnityEngine;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 状态变化事件结构体，用于存储状态变化的相关信息
    /// </summary>
    /// <typeparam name="T">状态类型，必须实现多个接口以确保基本功能</typeparam>
    public struct MMStateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        // 触发状态变化的目标对象
        public GameObject Target;
        // 目标状态机实例
        public MMStateMachine<T> TargetStateMachine;
        // 新的状态
        public T NewState;
        // 之前的状态
        public T PreviousState;

        /// <summary>
        /// 构造函数，初始化状态变化事件
        /// </summary>
        /// <param name="stateMachine">状态机实例</param>
        public MMStateChangeEvent(MMStateMachine<T> stateMachine)
        {
            Target = stateMachine.Target;
            TargetStateMachine = stateMachine;
            NewState = stateMachine.CurrentState;
            PreviousState = stateMachine.PreviousState;
        }
    }

    /// <summary>
    /// 状态机的公共接口，定义了基本功能
    /// </summary>
    public interface MMIStateMachine
    {
        // 是否触发事件的标志
        bool TriggerEvents { get; set; }
    }

    /// <summary>
    /// 状态机管理器，设计注重简单性
    /// 使用方法：
    /// 1. 定义一个枚举，例如：public enum CharacterConditions { Normal, ControlledMovement, Frozen, Paused, Dead }
    /// 2. 声明状态机：public StateMachine<CharacterConditions> ConditionStateMachine;
    /// 3. 初始化：ConditionStateMachine = new StateMachine<CharacterConditions>();
    /// 4. 改变状态：ConditionStateMachine.ChangeState(CharacterConditions.Dead);
    /// 状态机会自动维护当前状态和前一个状态，并可选择性地触发状态进入/退出事件
    /// </summary>
    /// <typeparam name="T">状态类型，必须实现多个接口以确保基本功能</typeparam>
    public class MMStateMachine<T> : MMIStateMachine where T : struct, IComparable, IConvertible, IFormattable
    {
        /// <summary>
        /// 是否触发事件的标志
        /// 如果设置为true，状态机会在进入和退出状态时触发事件
        /// 可以监听状态变化事件，无需硬绑定委托
        /// </summary>
        public virtual bool TriggerEvents { get; set; }

        // 目标游戏对象
        public GameObject Target;
        // 当前状态
        public virtual T CurrentState { get; protected set; }
        // 前一个状态
        public virtual T PreviousState { get; protected set; }

        // 状态变化委托
        public delegate void OnStateChangeDelegate();

        /// <summary>
        /// 状态变化事件，可以用于本地监听状态机变化
        /// 使用方法：
        /// 1. 在OnEnable中注册：yourReferenceToTheStateMachine.OnStateChange += OnStateChange;
        /// 2. 在OnDisable中取消注册：yourReferenceToTheStateMachine.OnStateChange -= OnStateChange;
        /// 3. 在OnStateChange方法中处理变化
        /// </summary>
        public OnStateChangeDelegate OnStateChange;

        /// <summary>
        /// 创建新的状态机实例
        /// </summary>
        /// <param name="target">目标游戏对象</param>
        /// <param name="triggerEvents">是否触发事件</param>
        public MMStateMachine(GameObject target, bool triggerEvents)
        {
            this.Target = target;
            this.TriggerEvents = triggerEvents;
        }

        /// <summary>
        /// 改变当前状态到指定状态，并在需要时触发退出和进入事件
        /// </summary>
        /// <param name="newState">新的状态</param>
        public virtual void ChangeState(T newState)
        {
            // 如果新状态与当前状态相同，直接返回
            if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
            {
                return;
            }

            // 保存前一个状态
            PreviousState = CurrentState;
            CurrentState = newState;

            // 触发状态变化事件
            OnStateChange?.Invoke();

            // 如果启用了事件触发，则广播状态变化事件
            if (TriggerEvents)
            {
                MMEventManager.TriggerEvent(new MMStateChangeEvent<T>(this));
            }
        }

        /// <summary>
        /// 将状态恢复到前一个状态
        /// </summary>
        public virtual void RestorePreviousState()
        {
            // 恢复前一个状态
            CurrentState = PreviousState;

            // 触发状态变化事件
            OnStateChange?.Invoke();

            // 如果启用了事件触发，则广播状态变化事件
            if (TriggerEvents)
            {
                MMEventManager.TriggerEvent(new MMStateChangeEvent<T>(this));
            }
        }
    }

}