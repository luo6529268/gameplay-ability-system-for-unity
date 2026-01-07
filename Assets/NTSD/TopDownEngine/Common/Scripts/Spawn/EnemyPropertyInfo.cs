using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MoreMountains.TopDownEngine
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/EnemyPropertyInfo", fileName = "EnemyPropertyInfo")]
    public class EnemyPropertyInfo : ScriptableObject
    {
        [Serializable]
        public struct PropertyInfo 
        {
            public int Health;
            public int Damage;
            public int Defend;
            public int RewardGold;
            public int RewardExp;
        }

        [Serializable]
        public struct EnemyInfo 
        {
            public string ID;
            public PropertyInfo propertyInfo;

            public static implicit operator List<object>(EnemyInfo v)
            {
                throw new NotImplementedException();
            }
        }


        public List<EnemyInfo> EnemyPropertieList = new List<EnemyInfo>();
        
        private Dictionary<string,PropertyInfo> EnemyPropertyDic = new Dictionary<string,PropertyInfo>();

        public void InitialInfo() 
        {
            for (int i = 0;i < EnemyPropertieList.Count;i++) 
            {
                EnemyInfo enemyInfo = EnemyPropertieList[i];
                if (!EnemyPropertyDic.ContainsKey(enemyInfo.ID))
                    EnemyPropertyDic[enemyInfo.ID] = enemyInfo.propertyInfo;
            }

            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.InitEnemyInfo);
        }

        public bool OnEnemyInfoDicIsEmpty() 
        {
            return (EnemyPropertyDic.Count <= 0);
        }

        public PropertyInfo OnGetEnemyPropertyInfo(string ID) 
        {
            PropertyInfo propertyInfo = new PropertyInfo();

            if (EnemyPropertyDic.Count <= 0)
                return propertyInfo;

            EnemyPropertyDic.TryGetValue(ID, out propertyInfo);
            return propertyInfo;
        }
    }
}
