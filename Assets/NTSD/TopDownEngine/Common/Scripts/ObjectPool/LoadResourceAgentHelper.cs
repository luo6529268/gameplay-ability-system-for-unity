using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    public class LoadResourceAgentHelper
    {
        private AssetBundleRequest m_AssetBundleRequest = null;
        private UnityEngine.Object assetObject = null;
        private UnityEvent<object, object> LoadCompleteEvent;
        public UnityEvent<object, object> m_LoadCompleteEvent
        {
            get { return LoadCompleteEvent; }
        }

        public LoadResourceAgentHelper()
        {
            LoadCompleteEvent = new UnityEvent<object, object>();
        }

        public void OnUpdate()
        {
            UpdateAssetBundleRequest();
        }

        private void UpdateAssetBundleRequest()
        {
//#if UNITY_EDITOR
            if (assetObject == null)
                return;

            LoadCompleteEvent?.Invoke(this, assetObject);
            assetObject = null;
//#else
            //if (m_AssetBundleRequest == null)
            //    return;

            //if (!m_AssetBundleRequest.isDone)
            //    return;


            //if (m_AssetBundleRequest.asset != null)
            //{
            //    LoadCompleteEvent?.Invoke(this, m_AssetBundleRequest.asset);
            //    m_AssetBundleRequest = null;
            //}
            //else
            //{ 
            //    Debug.LogError("资源加载完成，但是Asset 文件并没有加载出来");
            //}
//#endif
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步加载资源。
        /// </summary>
        /// <param name="resource">资源。</param>
        /// <param name="assetName">要加载的资源名称。</param>
        public void LoadAsset(object resource, string assetName)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                return;

            assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetName);

#else
            //AssetBundle assetBundle = resource as AssetBundle;
            //if (assetBundle == null || string.IsNullOrEmpty(assetName))
            //{
            //    return;
            //}
            //m_AssetBundleRequest = assetBundle.LoadAssetAsync(assetName);

              LoadResourceAgent loadResourceAgent = resource as LoadResourceAgent;
            int ID = loadResourceAgent.Task.ResourceInfo.ResourceID;
            assetObject = ObjectPoolManager.Instance.m_GenerationConfig.OnGetObjectByID(ID);
#endif
        }

        /// <summary>
        /// 通过加载资源代理辅助器开始异步加载资源。
        /// </summary>
        /// <param name="resource">资源。</param>
        /// <param name="assetName">要加载的资源名称。</param>
        public void LoadAsset(string assetName)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                return;

            assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetName);
#endif

        }

        /// <summary>
        /// 重置加载资源代理辅助器。
        /// </summary>
        public void Reset()
        {
#if UNITY_EDITOR
            assetObject = null;
#else
            m_AssetBundleRequest = null;
#endif
        }
    }
}
