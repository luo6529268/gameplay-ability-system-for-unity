using BehaviorDesigner.Runtime.Tasks;
using NTSD.TimeWheel;
using NTSD.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BeatEmUpTemplate2D
{
    public abstract class Condition: IPoolable
    {
        public abstract bool Evaluate();
        public virtual void OnRecycled() { }
        public virtual void OnSpawned() { }
    }

    public class ConditionHelper : Condition
    {
        public override bool Evaluate(){return false;}

        protected static T Get<T>() where T : Condition, new()
        {
            return ReferencePoolManager.Spawn<T>();
        }

        public void Release()
        {
            ReferencePoolManager.Recycle(this);
        }
    }

    // 简单布尔包装器：用委托作为条件（最灵活）
    public class BoolCondition : ConditionHelper
    {
        private Func<bool> _func;

        public static BoolCondition Create(Func<bool> getter)
        {
            var c = Get<BoolCondition>();
            c._func = getter;
            return c;
        }

        public override bool Evaluate() => _func?.Invoke() ?? false;
        public override void OnRecycled()
        {
            _func = null; // 清除引用，防止闭包泄漏
        }
    }

    // 数值轴比较（例如 Horizontal）
    /// <summary>
    /// 比较模式枚举，用于定义不同的比较操作类型
    /// </summary>
    public enum CompareMode
    {
        /// <summary>
        /// 无比较模式
        /// </summary>
        None,           // 不进行任何比较

        Equal, //等于比较

        UnEqual, // 不等于比较

        /// <summary>
        /// 大于比较模式
        /// </summary>
        Greater,        // 执行大于比较

        /// <summary>
        /// 小于比较模式
        /// </summary>
        Less,           // 执行小于比较

        /// <summary>
        /// 大于等于比较模式
        /// </summary>
        GreaterOrEqual, // 执行大于等于比较

        /// <summary>
        /// 小于等于比较模式
        /// </summary>
        LessOrEqual     // 执行小于等于比较
    }
    public class AxisCondition : ConditionHelper
    {
        private Func<float> _valueSource;
        private float _threshold;
        private CompareMode _mode;

        // valueSource 可传 Input.GetAxisRaw("Horizontal") 的 wrapper，或 UnitActions 的 cached value
        public static AxisCondition Create(Func<float> valueSource, float threshold, CompareMode mode)
        {
            var c = Get<AxisCondition>();
            c._valueSource = valueSource;
            c._threshold = threshold;
            c._mode = mode;
            return c;
        }

        public override bool Evaluate()
        {
            float v = MathF.Abs(_valueSource?.Invoke() ?? 0f);
            return _mode switch
            {
                CompareMode.Equal => v == _threshold,
                CompareMode.UnEqual => v != _threshold,
                CompareMode.Greater => v > _threshold,
                CompareMode.Less => v < _threshold,
                CompareMode.GreaterOrEqual => v >= _threshold,
                CompareMode.LessOrEqual => v <= _threshold,
                _ => false
            };
        }

        public override void OnRecycled()
        {
            _valueSource = null;
            _threshold = 0;
            _mode = CompareMode.None;
        }
    }

    // 动画结束 / normalizedTime >= threshold
    public class AnimationFinishedCondition : ConditionHelper
    {
        private Func<float> _getNormalizedTime;
        private float _threshold;

        public static AnimationFinishedCondition Create(Func<float> func, float threshold) 
        {
            var c = Get<AnimationFinishedCondition>();
            c._getNormalizedTime = func;
            c._threshold = threshold;
            return c;
        }

        public override bool Evaluate() => (_getNormalizedTime?.Invoke() ?? 0f) >= _threshold;

        public override void OnRecycled()
        {
            _getNormalizedTime = null;
            _threshold = 0f;
        }
    }

    // 冷却/定时类
    public class CooldownCondition : ConditionHelper
    {
        private Func<float> _getLastTime;
        private float _cooldown;

        public static CooldownCondition Create(Func<float> _getLastTime, float _cooldown)
        {
            var c = Get<CooldownCondition>();
            c._getLastTime = _getLastTime;
            c._cooldown = _cooldown;
            return c;
        }

        public override bool Evaluate() => (Time.time - (_getLastTime?.Invoke() ?? -9999f)) >= _cooldown;
        public override void OnRecycled()
        {
            _getLastTime = null;
            _cooldown = 0f;
        }

    }

    // 组合条件
    public class AndCondition : ConditionHelper
    {
        private readonly List<ConditionHelper> _conditions = new List<ConditionHelper>(5);

        public static AndCondition Create(params ConditionHelper[] conds)
        {
            var c = Get<AndCondition>();
            for (int i = 0; i < conds.Length; i++)
                c._conditions.Add(conds[i]);
            return c;
        }

        public override bool Evaluate()
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].Evaluate())
                    return false;
            }
            return true;
        }

        public override void OnRecycled()
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                _conditions[i].Release();
            }
            _conditions.Clear();
        }
    }

    public class OrCondition : ConditionHelper
    {
        private readonly List<ConditionHelper> _conditions = new List<ConditionHelper>(5);


        public static OrCondition Create(params ConditionHelper[] conds) 
        {
            var c = Get<OrCondition>();
            for (int i = 0; i < conds.Length; i++)
                c._conditions.Add(conds[i]);

            return c;
        }

        public override bool Evaluate()
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].Evaluate())
                    return true;
            }
            return false;
        }

        public override void OnRecycled()
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                _conditions[i].Release();
            }
            _conditions.Clear();
        }
    }

    public class NotCondition : ConditionHelper
    {
        private ConditionHelper _inner;

        public static NotCondition Create(ConditionHelper inner) 
        {
           var c = Get<NotCondition>();
            c._inner = inner;
            return c;
        } 

        public override bool Evaluate() => !_inner.Evaluate();

        public override void OnRecycled()
        {
            _inner = null;
        }
    }

    public class TimeWheelDelayCondition : ConditionHelper
    {
        private static TimeWheel _timeWheel;
        private Func<bool> _callback;
        private bool _hasTriggered;
        private ulong _taskId;

        public static TimeWheelDelayCondition Create(int delayMilliseconds, System.Func<bool> callback)
        {
            var c = Get<TimeWheelDelayCondition>();
            c._hasTriggered = false;

            // 确保TimeWheel实例存在
            _timeWheel ??= TimeWheel.CreateSharedInstance();

            // 计算tick数（假设1ms = 1tick）
            ulong delayTicks = (ulong)delayMilliseconds;
            c._callback = callback;

            // 安排延迟任务
            c._taskId = _timeWheel.Schedule(
                callback: (data) => {
                    c._hasTriggered = true;
                },
                delay: delayTicks
            );

            return c;
        }

        public override bool Evaluate() => _hasTriggered && (_callback?.Invoke() ?? false);

        public override void OnRecycled()
        {
            if (_taskId != 0 && _timeWheel != null)
            {
                _timeWheel.Cancel(_taskId);
            }
            _hasTriggered = false;
            _callback = null;
            _taskId = 0;
        }
    }


}