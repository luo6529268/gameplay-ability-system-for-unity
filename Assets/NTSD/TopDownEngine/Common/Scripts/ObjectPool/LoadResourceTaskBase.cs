using MoreMountains.TopDownEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMountains.TopDownEngine
{
    public abstract class LoadResourceTaskBase:TaskBase
    {
        private int s_Serial = 0;
        private string m_AssetName;
        public string AssetName
        {
            get
            {
                return m_AssetName;
            }
        }
        private ResourceInfo m_ResourceInfo;
        public ResourceInfo ResourceInfo
        {
            get
            {
                return m_ResourceInfo;
            }
        }

        private object m_UserData;
        public object UserData 
        {
            get { return m_UserData; }
        }

        private DateTime m_StartTime;
        public DateTime StartTime
        {
            get
            {
                return m_StartTime;
            }
            set
            {
                m_StartTime = value;
            }
        }

        public LoadResourceTaskBase() 
        {
            
        }

        public LoadResourceTaskBase(ResourceInfo resourceInfo) 
        {
            m_ResourceInfo = resourceInfo;
        }

        protected void Initialize(string assetName, ResourceInfo resourceInfo, object userData)
        {
            ++s_Serial;
            m_AssetName = assetName;
            m_ResourceInfo = resourceInfo;
            m_UserData = userData;
        }

        public virtual void OnLoadAssetSuccess(LoadResourceAgent agent)
        {

        }

        public void LoadMain(LoadResourceAgent agent, string resourceName)
        {
            agent.Helper.LoadAsset(agent,resourceName);
        }
    }
}
