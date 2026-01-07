using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMountains.TopDownEngine
{
    public interface ITaskAgent<T> where T : TaskBase
    {

        /// <summary>
        /// 获取任务。
        /// </summary>
        T Task
        {
            get;
        }

        void Initialize();

        /// <summary>
        /// 开始处理任务。
        /// </summary>
        /// <param name="task">要处理的任务。</param>
        /// <returns>开始处理任务的状态。</returns>
        StartTaskStatus Start(T task);

        /// <summary>
        /// 停止正在处理的任务并重置任务代理。
        /// </summary>
        void Reset();

        void OnUpdate();
    }
}
