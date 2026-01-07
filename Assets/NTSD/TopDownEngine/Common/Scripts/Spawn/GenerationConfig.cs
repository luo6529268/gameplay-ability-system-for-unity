using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/GenerationConfig", fileName = "GenerationConfig")]
    public class GenerationConfig : ScriptableObject
    {
        [System.Serializable]
        public class LootOption
        {
            public int ID;         // 项目名称（如奖励或怪物）
            public string PrefabPath;   // 生成的物体
            public GameObject PrefabObj;
            public float BaseWeight;    // 初始权重
            public int MaxQuantity;     // 最大生成数量
            [Range(1, 15)]                 // Rarity 范围为 1 到 5（1为普通，5为稀有）
            public int Rarity = 1;        // 稀有度
            [HideInInspector] public float CumulativeWeight;  // 累积权重（用于分布式随机）
            [HideInInspector] public int CurrentQuantity = 0; // 当前生成数量（内部使用）
        }

        /// <summary>
        /// 表示随机生成的结果
        /// </summary>
        public struct LootResult
        {
            public LootOption LootOption; // 生成的物品
            public int Quantity;                   // 生成的数量

            public LootResult(LootOption lootOption, int quantity)
            {
                LootOption = lootOption;
                Quantity = quantity;
            }
        }

        [Header("战利品选项")]
        public List<LootOption> LootTable = new List<LootOption>(); // 随机池

        private Dictionary<int,LootOption> m_TotalDiction = new Dictionary<int,LootOption>();

        [Header("Settings")]
        [Tooltip("是否动态调整权重，防止频繁出现相同结果")]
        public bool AdjustWeightsOverTime = false;
        [Tooltip("权重衰减系数，例如 0.1 表示减少 10% 的权重")]
        public float WeightDecayFactor = 0.5f;

        public void OnInitialInfo() 
        {
            for (int i = 0; i < LootTable.Count; i++) 
            {
                LootOption lootOption = LootTable[i];
                m_TotalDiction.TryAdd(lootOption.ID, lootOption);
            }
        }

        public string OnGetObjectInfoByName(int nameID) 
        {
            string path = string.Empty;

            if (m_TotalDiction.Count <= 0)
                return path;

            LootOption lootOption = null;
            m_TotalDiction.TryGetValue(nameID, out lootOption);
            if(lootOption == null)
                return path;

            return lootOption.PrefabPath;
        }

        public GameObject OnGetObjectByID(int nameID)
        {
            GameObject obj = null;

            if (m_TotalDiction.Count <= 0)
                return obj;

            LootOption lootOption = null;
            m_TotalDiction.TryGetValue(nameID, out lootOption);
            if (lootOption == null)
                return obj;

            return lootOption.PrefabObj;
        }

        public void OnGetLootOptionListByID(int min,int max,ref List<LootOption> options) 
        {
            if (m_TotalDiction.Count <= 0)
                return;

            for (int i = min; i <= max; i++) 
            {
                LootOption lootOption = null;
                if (m_TotalDiction.TryGetValue(i, out lootOption))
                    options.Add(lootOption);
            }
        }

        /// <summary>
        /// 随机生成一个物品或怪物
        /// </summary>
        public LootResult GenerateLoot(List<LootOption> lootOptions)
        {
            if (lootOptions == null || lootOptions.Count == 0)
            {
                Debug.LogError("Loot table is empty! Please configure loot options.");
                return new LootResult(null, 0);
            }

            // 随机选取一个项目
            LootOption selectedLoot = SelectRandomLoot(lootOptions);

            // 递归生成数量
            int quantity = GenerateLootQuantity(selectedLoot);

            // 返回生成结果
            return new LootResult(selectedLoot, quantity);
        }


        /// <summary>
        /// 随机选择一个物品或怪物
        /// </summary>
        private LootOption SelectRandomLoot(List<LootOption> lootOptions)
        {
            float totalWeight = 0f;

            foreach (var loot in lootOptions)
            {
                totalWeight += loot.BaseWeight * loot.Rarity; // 按稀有度加权
            }

            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var loot in lootOptions)
            {
                cumulativeWeight += loot.BaseWeight * loot.Rarity;
                if (randomValue <= cumulativeWeight)
                {
                    return loot;
                }
            }

            return null; // 理论上不会到达这里
        }

        /// <summary>
        /// 根据选中的物品递归生成数量
        /// </summary>
        private int GenerateLootQuantity(LootOption loot)
        {
            int generatedCount = 0;
            int attempt = 1;

            while (attempt <= loot.MaxQuantity)
            {
                // 动态计算本次概率
                float adjustedProbability = CalculateProbability(loot, attempt);
                float randomValue = UnityEngine.Random.value;

                if (randomValue <= adjustedProbability)
                {
                    // 增加生成数量
                    generatedCount++;
                }
                else
                {
                    // 未选中，结束当前递归
                    break;
                }

                attempt++;
            }

            return generatedCount;
        }

        /// <summary>
        /// 动态计算当前筛选概率
        /// </summary>
        private float CalculateProbability(LootOption loot, int attempt)
        {
            // 基于尝试次数和稀有度调整概率
            return loot.BaseWeight * (1f - (float)attempt / loot.MaxQuantity) / 100f * loot.Rarity;
        }

    }
}