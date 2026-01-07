using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    public class LoadAssetTask:LoadResourceTaskBase
    {
        private LoadAssetSuccessCallback m_LoadAssetCallbacks;
       


        public LoadAssetTask()
        {
            m_LoadAssetCallbacks = null;
        }


        public static LoadAssetTask Create(string assetName, ResourceInfo resourceInfo = null, LoadAssetSuccessCallback loadAssetSuccessCallback = null, object userData = null)
        {
            LoadAssetTask loadAssetTask = new LoadAssetTask();
            loadAssetTask.Initialize(assetName, resourceInfo, userData);
            loadAssetTask.m_LoadAssetCallbacks = loadAssetSuccessCallback;
            return loadAssetTask;
        }

        public override void OnLoadAssetSuccess(LoadResourceAgent agent)
        {
            base.OnLoadAssetSuccess(agent);
            if (m_LoadAssetCallbacks != null)
                m_LoadAssetCallbacks?.Invoke(agent.Task);
        }
    }
}