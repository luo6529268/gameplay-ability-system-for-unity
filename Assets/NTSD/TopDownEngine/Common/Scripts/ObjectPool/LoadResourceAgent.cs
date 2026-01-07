using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class LoadResourceAgent : ITaskAgent<LoadResourceTaskBase>
    {
        private readonly LoadResourceAgentHelper m_Helper;
        public LoadResourceAgentHelper Helper
        {
            get
            {
                return m_Helper;
            }
        }

        private LoadResourceTaskBase m_Task;
        /// <summary>
        /// 获取加载资源任务。
        /// </summary>
        public LoadResourceTaskBase Task
        {
            get
            {
                return m_Task;
            }
        }

        private readonly ObjectPool m_ObjectPool;


        public LoadResourceAgent() 
        {
            m_Helper = new LoadResourceAgentHelper();
            m_ObjectPool = new ObjectPool();
            m_Task = null;
        }

        public void Initialize() 
        {
            m_Helper.m_LoadCompleteEvent.AddListener(OnLoadResourceAgentHelperLoadComplete);
        }

        public void OnUpdate() 
        {
            m_Helper.OnUpdate();
        }

        public StartTaskStatus Start(LoadResourceTaskBase task)
        {
            if (task == null)
            {
                Debug.LogError("Task is invalid.");
            }

            m_Task = task;
            m_Task.StartTime = DateTime.UtcNow;

            ResourceInfo resourceInfo = m_Task.ResourceInfo;
            if (!resourceInfo.Ready)
            {
                task.StartTime = default(DateTime);
                return StartTaskStatus.HasToWait;
            }

            string resourceName = resourceInfo.m_ResourceName;
            OnResourceObjectReady(resourceName);
            return StartTaskStatus.CanResume;
        }

        private void OnResourceObjectReady(string resourceName)
        {
            m_Task.LoadMain(this, resourceName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="target">加载完成的资源</param>
        private void OnLoadResourceAgentHelperLoadComplete(object sender, object target) 
        {
            m_Task.ResourceInfo.Target = target;
            m_Task.OnLoadAssetSuccess(this);
            m_Task.Done = true;
        }

        public void Reset()
        {
            m_Helper.Reset();
            m_Task = null;
        }
    }
}
