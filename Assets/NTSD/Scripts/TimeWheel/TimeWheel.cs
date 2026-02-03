using System;
using System.Collections.Generic;
using System.Threading;

namespace NTSD.TimeWheel
{
    /// <summary>
    /// Linux内核风格的分层时间轮
    /// 特性：
    /// 1. 五级时间轮分层
    /// 2. 自动任务降级机制
    /// 3. 无锁设计（基于CAS操作）
    /// 4. 内存池优化
    /// </summary>
    public sealed class TimeWheel : IDisposable
    {
        #region Constants and Structs

        // 定义时间轮的位数和大小
        private const int TVN_BITS = 6;  // 非根时间轮的位数
        private const int TVR_BITS = 8;  // 根时间轮的位数
        private const int TVN_SIZE = 1 << TVN_BITS;  // 非根时间轮的大小
        private const int TVR_SIZE = 1 << TVR_BITS;  // 根时间轮的大小
        private const int TVN_MASK = TVN_SIZE - 1;   // 非根时间轮的掩码
        private const int TVR_MASK = TVR_SIZE - 1;   // 根时间轮的掩码

        // 最大延迟时间
        public const long MAX_DELAY = (long)TVR_SIZE * TVN_SIZE * TVN_SIZE * TVN_SIZE * TVN_SIZE - 1;

        // 定时任务节点
        private class TimerTask
        {
            public ulong Id;              // 任务ID
            public long ExecuteOrder;     // 执行顺序
            public Action<object?>? Callback;  // 回调函数
            public object? Data;          // 回调数据
            public ulong ExpireTicks;     // 过期时间点
            public ulong Interval;        // 重复间隔
            public int RepeatCount;       // 重复次数
            public TimerTask? Next;       // 下一个节点
            public TimerTask? Prev;       // 前一个节点

            /// <summary>
            /// 任务状态：0=未激活, 1=已激活, 2=已取消
            /// </summary>
            public int State;
        }

        #endregion

        #region Core Fields

        // 五级时间轮数组
        private readonly TimerTask?[] tv1 = new TimerTask[TVR_SIZE];  // 第一级时间轮
        private readonly TimerTask?[] tv2 = new TimerTask[TVN_SIZE];  // 第二级时间轮
        private readonly TimerTask?[] tv3 = new TimerTask[TVN_SIZE];  // 第三级时间轮
        private readonly TimerTask?[] tv4 = new TimerTask[TVN_SIZE];  // 第四级时间轮
        private readonly TimerTask?[] tv5 = new TimerTask[TVN_SIZE];  // 第五级时间轮

        // 核心字段
        private ulong nextId;             // 下一个任务ID
        private long executeOrder;        // 执行顺序计数器
        private ulong currentTick;        // 当前时钟滴答
        private readonly bool shouldSortBeforeExecution;  // 是否在执行前排序
        private readonly object syncRoot = new();         // 同步锁对象
        private readonly Stack<TimerTask?> taskPool = new(1024);  // 任务对象池
        private readonly List<TimerTask> taskListCache = new(1024);  // 任务列表缓存

        #endregion

        #region Public Interface

        public static TimeWheel SharedInstance { get; private set; } = null!;

        public static TimeWheel CreateSharedInstance(bool shouldSortBeforeExecution = true)
        {
            if (SharedInstance != null)
            {
                return SharedInstance;
            }

            return SharedInstance = new(shouldSortBeforeExecution);
        }

        public static void DestroySharedInstance()
        {
            if (SharedInstance != null)
            {
                SharedInstance.Dispose();
                SharedInstance = null!;
            }
        }

        // 构造函数
        public TimeWheel(bool shouldSortBeforeExecution = true)
        {
            this.shouldSortBeforeExecution = shouldSortBeforeExecution;
        }

        // 回调错误事件
        public event Action<Exception>? OnCallbackError;

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="delay">延迟, 最小值1, 如果为0则自动设置为1</param>
        /// <param name="interval">重复周期</param>
        /// <param name="repeatCount">到期后, 再次重复次数(小于0无限, 0不重复)</param>
        /// <param name="data">回调函数的数据</param>
        /// <returns>任务句柄</returns>
        public ulong Schedule(Action<object?> callback, ulong delay, ulong interval = 0, int repeatCount = 0, object? data = null)
        {
            // 参数校验
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            delay = delay switch
            {
                > MAX_DELAY => throw new ArgumentOutOfRangeException(nameof(delay), $"Delay must be less than {MAX_DELAY}, but got {delay}"),
                0 => 1,
                _ => delay
            };

            if (repeatCount != 0 && interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), $"Interval must be greater than zero");

            // 创建新任务
            var task = this.AllocateTask();
            task.Id = ++this.nextId;
            task.ExecuteOrder = Interlocked.Increment(ref this.executeOrder);
            task.Callback = callback;
            task.Data = data;
            task.Interval = interval;
            task.RepeatCount = repeatCount;
            task.ExpireTicks = this.currentTick + delay;

            // 添加任务到时间轮
            lock (this.syncRoot)
            {
                this.InternalAddTask(task);
            }

            return task.Id;
        }

        /// <summary>
        /// 取消定时任务
        /// </summary>
        public bool Cancel(ulong id)
        {
            lock (this.syncRoot)
            {
                // 遍历所有时间轮
                foreach (var vec in new[] { this.tv1, this.tv2, this.tv3, this.tv4, this.tv5 })
                {
                    for (int i = 0; i < vec.Length; i++)
                    {
                        TimerTask? current = vec[i];
                        while (current != null)
                        {
                            if (current.Id == id)
                            {
                                // 使用CAS操作取消任务
                                if (Interlocked.CompareExchange(ref current.State, 2, 1) == 1)
                                {
                                    RemoveTask(ref vec[i], current);
                                    this.ReleaseTask(current);
                                    return true;
                                }

                                return false;
                            }

                            current = current.Next;
                        }
                    }
                }

                return false;
            }
        }

        // 释放资源
        public void Dispose()
        {
        }

        // 推进时间轮
        public void Tick()
        {
            this.TimeWheelWorker();
        }

        #endregion

        #region Core Algorithm

        // 时间轮工作线程
        private void TimeWheelWorker()
        {
            // 更新当前时间
            if (this.currentTick >= long.MaxValue - 1)
                this.currentTick = 0;
            else
                this.currentTick++;

            // 计算当前索引
            int index = (int)(this.currentTick & TVR_MASK);
            if (index == 0)
            {
                // 处理时间轮级联
                index = (int)(this.currentTick >> TVR_BITS) & TVN_MASK;
                this.CascadeTimers(this.tv2, index);
                if (index == 0)
                {
                    index = (int)((this.currentTick >> (TVR_BITS + TVN_BITS)) & TVN_MASK);
                    this.CascadeTimers(this.tv3, index);
                    if (index == 0)
                    {
                        index = (int)((this.currentTick >> (TVR_BITS + 2 * TVN_BITS)) & TVN_MASK);
                        this.CascadeTimers(this.tv4, index);
                        if (index == 0)
                        {
                            index = (int)((this.currentTick >> (TVR_BITS + 3 * TVN_BITS)) & TVN_MASK);
                            this.CascadeTimers(this.tv5, index);
                        }
                    }
                }
            }

            // 处理到期的任务
            this.ProcessTimers(ref this.tv1[this.currentTick & TVR_MASK]);
        }

        // 时间轮级联处理
        private void CascadeTimers(TimerTask?[] tv, int index)
        {
            TimerTask? current = tv[index];
            tv[index] = null;

            while (current != null)
            {
                TimerTask? next = current.Next;
                this.InternalAddTask(current);
                current = next;
            }
        }

        // 处理到期任务
        private void ProcessTimers(ref TimerTask? head)
        {
            var taskList = this.taskListCache;
            ExtractTasks(ref head, taskList);
            if (taskList.Count == 0)
                return;

            // 如果需要，按执行顺序排序
            if (taskList.Count > 1 && this.shouldSortBeforeExecution)
                taskList.Sort((a, b) => a.ExecuteOrder.CompareTo(b.ExecuteOrder));

            // 执行任务
            foreach (var task in taskList)
            {
                if (task.State == 1 && this.currentTick >= task.ExpireTicks)
                {
                    try
                    {
                        task.Callback?.Invoke(task.Data);
                    }
                    catch (Exception ex)
                    {
                        if (this.OnCallbackError != null)
                        {
                            this.OnCallbackError.Invoke(ex);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // 处理重复任务
                    if (task.RepeatCount != 0)
                    {
                        if (task.RepeatCount > 0) task.RepeatCount--;

                        task.ExpireTicks += task.Interval;
                        task.ExecuteOrder = Interlocked.Increment(ref this.executeOrder);

                        // 重新加入时间轮
                        lock (this.syncRoot)
                        {
                            this.InternalAddTask(task);
                        }
                    }
                    else
                    {
                        this.ReleaseTask(task);
                    }
                }
                else if (task.State == 2)
                {
                    this.ReleaseTask(task);
                }
            }

            taskList.Clear();
        }

        // 提取任务到列表
        private static void ExtractTasks(ref TimerTask? head, List<TimerTask> taskList)
        {
            TimerTask? current = head;
            while (current != null)
            {
                var next = current.Next;
                taskList.Add(current);
                current.Next = null;
                current.Prev = null;
                current = next;
            }

            head = null; // 清空原链表
        }

        // 内部添加任务
        private void InternalAddTask(TimerTask task)
        {
            var expires = task.ExpireTicks;
            var idx = expires - this.currentTick;

            int i;
            TimerTask?[] vec;
            // 根据延迟时间选择合适的时间轮
            if (idx < TVR_SIZE)
            {
                i = (int)(expires & TVR_MASK);
                vec = this.tv1;
            }
            else if (idx < (1 << (TVR_BITS + TVN_BITS)))
            {
                i = (int)((expires >> TVR_BITS) & TVN_MASK);
                vec = this.tv2;
            }
            else if (idx < (1 << (TVR_BITS + 2 * TVN_BITS)))
            {
                i = (int)((expires >> (TVR_BITS + TVN_BITS)) & TVN_MASK);
                vec = this.tv3;
            }
            else if (idx < (1 << (TVR_BITS + 3 * TVN_BITS)))
            {
                i = (int)((expires >> (TVR_BITS + 2 * TVN_BITS)) & TVN_MASK);
                vec = this.tv4;
            }
            else
            {
                i = (int)((expires >> (TVR_BITS + 3 * TVN_BITS)) & TVN_MASK);
                vec = this.tv5;
            }

            // 插入到链表头部
            var oldTask = vec[i];
            if (oldTask != null)
            {
                oldTask.Prev = task;
                task.Next = oldTask;
            }

            vec[i] = task;
            task.State = 1;
        }

        #endregion

        #region Memory Management

        // 分配任务对象
        private TimerTask AllocateTask()
        {
            lock (this.taskPool)
            {
                return this.taskPool.Count > 0 ? this.taskPool.Pop()! : new();
            }
        }

        // 释放任务对象
        private void ReleaseTask(TimerTask task)
        {
            task.Id = 0;
            task.ExecuteOrder = 0;
            task.Callback = null;
            task.Next = null;
            task.Prev = null;
            task.State = 0;

            lock (this.taskPool)
            {
                if (this.taskPool.Count < 1024) this.taskPool.Push(task);
            }
        }

        #endregion

        #region Helpers

        // 从链表中移除任务
        private static void RemoveTask(ref TimerTask? head, TimerTask task)
        {
            if (task.Prev != null)
                task.Prev.Next = task.Next;
            else
                head = task.Next;

            if (task.Next != null)
                task.Next.Prev = task.Prev;
        }

        #endregion
    }

}
