using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ResourceInfo
    {
        private string ResourceName;
        public string m_ResourceName
        {
            get
            {
                return ResourceName;
            }
        }
        private bool m_Ready;
        /// <summary>
        /// 获取资源是否准备完毕。
        /// </summary>
        public bool Ready
        {
            get
            {
                return m_Ready;
            }
        }

        private float m_StartTime;
        public float StartTime 
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        private int m_ResourceID;

        public int ResourceID
        {
            get { return m_ResourceID; }
            set { m_ResourceID = value; }
        }

        private object m_Target;

        public object Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        public static ResourceInfo Create(string name) 
        {
            ResourceInfo resourceInfo = new ResourceInfo();
            resourceInfo.ResourceName = name;
            resourceInfo.m_Ready = true;

            return resourceInfo;
        }
    }
}
