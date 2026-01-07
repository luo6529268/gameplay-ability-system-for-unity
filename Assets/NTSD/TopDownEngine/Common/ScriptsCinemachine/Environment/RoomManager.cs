using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class RoomManager : MMSingleton<RoomManager>
    {
        [MMReadOnly]
        public int CurrentLevelID = 0;

        private Dictionary<int,Room> _TotalRoom = new Dictionary<int,Room>();
        private Room CurrentRoom;

        protected override void Awake()
        {
            base.Awake();

            OnInitTotalRoomList();
        }

        protected virtual void OnEnable() 
        {
        }

        protected virtual void OnDisable() 
        {
        }

        void OnInitTotalRoomList() 
        {
            foreach (var room in transform.GetComponentsInChildren<Room>(true)) 
            {
                _TotalRoom.TryAdd(room.LevelID, room);
            }
        }

       

        void OnHandleRoomDisplayHide(Room room) 
        {
            int lastID = -1;
            int nextID = -1;

            OnHandleLastAndNextID(room, ref lastID, ref nextID);

            foreach (var item in _TotalRoom.Values)
            {
                if (item.LevelID == room.LevelID || item.LevelID == lastID || item.LevelID == nextID) 
                {
                    item.gameObject.SetActive(true);
                    continue;
                }

                item.gameObject.SetActive(false);
            }

            MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, LevelManager.Instance.Players[0]);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
        }

        void OnHandleLastAndNextID(Room room,ref int lastID, ref int nextID) 
        {
            DisplayRoom displayRoom = room.gameObject.GetComponent<DisplayRoom>();
            if (displayRoom == null)
                return;

            if (displayRoom.m_LastRoom != null)
                lastID = displayRoom.m_LastRoom.GetComponent<Room>().LevelID;

            if (displayRoom.m_NextRoom != null)
                nextID = displayRoom.m_NextRoom.GetComponent<Room>().LevelID;
        }
    }
}