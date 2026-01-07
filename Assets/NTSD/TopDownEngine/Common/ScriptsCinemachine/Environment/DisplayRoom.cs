using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class DisplayRoom : TopDownMonoBehaviour
    {
        public GameObject m_LastRoom;
        public GameObject m_NextRoom;

        /// <summary>
        /// 控制指定关卡的显隐
        /// </summary>
        public void OnHandleRoomDisplay() 
        {
            if (m_LastRoom != null)
                m_LastRoom.SetActive(true);

            if(m_NextRoom != null)
                m_NextRoom.SetActive(true);
        }
    }

}