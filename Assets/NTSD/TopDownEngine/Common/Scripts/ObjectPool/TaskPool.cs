using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class TaskPool<T> where T : TaskBase
    {
        private readonly LinkedList<ITaskAgent<T>> m_WorkingAgents;
        private readonly LinkedList<T> m_WaitingTasks;
        private readonly Stack<ITaskAgent<T>> m_FreeAgents;

        /// <summary>
        /// 初始化任务池的新实例。
        /// </summary>
        public TaskPool()
        {
            m_WorkingAgents = new LinkedList<ITaskAgent<T>>();
            m_WaitingTasks = new LinkedList<T>();
            m_FreeAgents = new Stack<ITaskAgent<T>>();
        }

        public void OnUpdate() 
        {
            ProcessRunningTasks();
            ProcessWaitingTasks();
        }

        public void OnRemoveAllAgent() 
        {
            m_WorkingAgents.Clear();
            m_WaitingTasks.Clear();
            m_FreeAgents.Clear();
        }

        public void OnAddDefautAgent(ITaskAgent<T> agent) 
        {
            if (agent == null)
            {
                Debug.LogError("Task agent is invalid.");
            }


            agent.Initialize();
            m_FreeAgents.Push(agent);
        }

        /// <summary>
        /// 增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask(T task)
        {
            LinkedListNode<T> current = m_WaitingTasks.Last;
            while (current != null)
            {
                current = current.Previous;
            }

            if (current != null)
            {
                m_WaitingTasks.AddAfter(current, task);
            }
            else
            {
                m_WaitingTasks.AddFirst(task);
            }
        }

        private void ProcessRunningTasks()
        {
            LinkedListNode<ITaskAgent<T>> current = m_WorkingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done) 
                {
                    current.Value.OnUpdate();
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();
                m_FreeAgents.Push(current.Value);
                m_WorkingAgents.Remove(current);
                current = next;
               
            }
        }

        private void ProcessWaitingTasks()
        {
            LinkedListNode<T> current = m_WaitingTasks.First;
            while (current != null)
            {
                T task = current.Value;
                ITaskAgent<T> agent = m_FreeAgents.Pop();
                LinkedListNode<T> next = current.Next;
                StartTaskStatus status = agent.Start(task);
                if ( status == StartTaskStatus.Done)
                {
                    //loadResourceAgent.Reset();
                    agent.Reset();
                    m_WaitingTasks.Remove(current);
                    m_FreeAgents.Push(agent);
                }

                if (status == StartTaskStatus.CanResume)
                {
                    m_WaitingTasks.Remove(current);
                    m_WorkingAgents.AddFirst(agent);
                }

                current = next;
            }
        }
    }
}