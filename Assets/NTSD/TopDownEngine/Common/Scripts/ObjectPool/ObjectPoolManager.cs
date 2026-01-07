using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ObjectPoolManager : MMSingleton<ObjectPoolManager>
    {
        public GenerationConfig m_GenerationConfig;

        private int m_MaxCount = 5;

        /// <summary>
        /// 正在使用的游戏对象资源
        /// </summary>
        public Dictionary<int, Queue<object>> m_ActiveObjectList;
        /// <summary>
        /// 回收的游戏对象列表，也就是放到 ReleaseGameObject 下的游戏对象
        /// </summary>
        private Dictionary<int, Queue<ResourceInfo>> m_ReleaseObjectList;
        private Dictionary<int, Queue<ResourceInfo>> m_CompleteObjectList;
        private TaskPool<LoadResourceTaskBase> m_TaskPool;

        private GameObject ReleaseGameObject;
        private AssetBundle assetBundle;

        protected override void Awake()
        {
            base.Awake();

            m_ActiveObjectList = new Dictionary<int, Queue<object>>();
            m_ReleaseObjectList = new Dictionary<int, Queue<ResourceInfo>>();
            m_CompleteObjectList = new Dictionary<int, Queue<ResourceInfo>>();
            m_TaskPool = new TaskPool<LoadResourceTaskBase>();
        }

        public void OnEnable()
        {
            OnInitReleaseGameObject();
            m_GenerationConfig.OnInitialInfo();
            OnInititalDefaultAsset();
        }

        public void OnDisable()
        {
            m_ActiveObjectList.Clear();
            m_ReleaseObjectList.Clear();
            m_CompleteObjectList.Clear();
            m_TaskPool.OnRemoveAllAgent();
        }

        public void Start()
        {

        }


        public void Update() 
        {
            m_TaskPool.OnUpdate();
        }
        
        void OnInitReleaseGameObject()
        {
            ReleaseGameObject = GameObject.Find("ReleaseGameObject");
            if (ReleaseGameObject == null)
                ReleaseGameObject = new GameObject("ReleaseGameObject");

            ReleaseGameObject.SetActive(false);
        }

        void OnInititalDefaultAsset() 
        {
            for (int i = 0; i < m_GenerationConfig.LootTable.Count; i++)
            {
                var item = m_GenerationConfig.LootTable[i];
                OnLoadingAsset(item.ID, m_MaxCount);
            }
        }

        public void OnLoadingAsset(int ID,float loadingCount)
        {
            string prefabPath = m_GenerationConfig.OnGetObjectInfoByName(ID);
            ResourceInfo resourceInfo = ResourceInfo.Create(prefabPath);
            resourceInfo.ResourceID = ID;
            while (loadingCount > 0)
            {
                LoadResourceAgent loadResourceAgent = new LoadResourceAgent();
                m_TaskPool.OnAddDefautAgent(loadResourceAgent);

                LoadAssetTask task = LoadAssetTask.Create(prefabPath, resourceInfo, (LoadResourceTaskBase task) =>
                {
                    OnAddCompleteList(task);
                });

                m_TaskPool.AddTask(task);
                loadingCount--;
            }
        }

        public void OnLoadObject(int ID,int loadCount, LoadAssetFailureCallbackEX loadAssetCallbackEx)
        {
            ResourceInfo resourceInfo = OnGetAssetObjectByComplete(ID);
            if (resourceInfo != null)
            {
                loadAssetCallbackEx?.Invoke(resourceInfo);
                return;
            }

            OnLoadingAsset(ID, loadCount);
        }

        public GameObject OnInstantiateObject(ResourceInfo resourceInfo) 
        {
            GameObject gameObject = Instantiate(resourceInfo.Target as GameObject);
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            Queue<object> queue = null;
            if (!m_ActiveObjectList.TryGetValue(resourceInfo.ResourceID, out queue)) 
            {
                queue = new Queue<object>();
                queue.Enqueue(gameObject);
                m_ActiveObjectList[resourceInfo.ResourceID] = queue;
            }else
                queue.Enqueue(gameObject);

            return gameObject;
        }

        ResourceInfo OnGetAssetObjectByComplete(int resourceID)
        {
            ResourceInfo resourceInfo = null;
           
            if (m_CompleteObjectList.Count <= 0)
                return resourceInfo;

            Queue<ResourceInfo> info = null;
            if (!m_CompleteObjectList.TryGetValue(resourceID, out info))
                return resourceInfo;

            info.TryDequeue(out resourceInfo);
            return resourceInfo;
        }

        void OnAddCompleteList(LoadResourceTaskBase task) 
        {
            Queue<ResourceInfo> list = null;
            ResourceInfo info = task.ResourceInfo;
            if (!m_CompleteObjectList.TryGetValue(task.ResourceInfo.ResourceID, out list)) 
            {
                list = new Queue<ResourceInfo>();
                m_CompleteObjectList.Add(task.ResourceInfo.ResourceID, list);
            }

            info.Target = task.ResourceInfo.Target;
            list.Enqueue(info);
        }
    }
}