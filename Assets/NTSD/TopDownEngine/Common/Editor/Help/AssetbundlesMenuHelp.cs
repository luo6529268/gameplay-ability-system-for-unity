using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AssetbundlesMenuHelp
{
    [MenuItem("Assets/AssetBundle/Copy AssetBundle Path")]
    static void CopyAssetBundles()
    {
        Object[] selectedAsset = Selection.objects;
        CopyMultipleAssetPathsToClipboard(selectedAsset);
    }

    public static void CopyMultipleAssetPathsToClipboard(Object[] assets)
    {
        if (assets == null || assets.Length == 0)
        {
            Debug.LogError("No assets selected.");
            return;
        }

        string paths = string.Join("\n", System.Array.ConvertAll(assets, AssetDatabase.GetAssetPath));
        GUIUtility.systemCopyBuffer = paths.ToLower();
        Debug.Log($"Asset paths copied to clipboard:\n{paths}");
    }

    [MenuItem("Assets/Enemy/修改属性值")]
    static void OnModificationProperty() 
    {
        Object selectedAsset = Selection.activeObject;
        EnemyPropertyInfo enemyPropertyInfo = selectedAsset as EnemyPropertyInfo;
        EnemyPropertyInfo.EnemyInfo EnemyPropertieList;
        for (int i = 0; i < enemyPropertyInfo.EnemyPropertieList.Count; i++)
        {
            EnemyPropertieList = enemyPropertyInfo.EnemyPropertieList[i];
            EnemyPropertieList.propertyInfo.Health = Mathf.CeilToInt(EnemyPropertieList.propertyInfo.Health * 1.5f);
            enemyPropertyInfo.EnemyPropertieList[i] = EnemyPropertieList;
            EditorUtility.SetDirty(enemyPropertyInfo);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    // 通过 MenuItem 来修改属性
    [MenuItem("Assets/GenerationConfig/自动设置PrefabObj")]
    static void OnAutoSetPrefabObj()
    {
        // 获取当前选中的资产
        Object selectedAsset = Selection.activeObject;

        // 确保选择的是 GenerationConfig 类型
        GenerationConfig config = selectedAsset as GenerationConfig;

        if (config != null)
        {
            // 遍历所有 LootOption 并更新 PrefabObj
            foreach (var lootOption in config.LootTable)
            {
                // 使用 PrefabPath 加载对应的预制体并设置 PrefabObj
                if (!string.IsNullOrEmpty(lootOption.PrefabPath))
                {
                    // 加载预制体
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(lootOption.PrefabPath);

                    // 如果路径有效且找到预制体，设置 PrefabObj
                    if (prefab != null)
                    {
                        lootOption.PrefabObj = prefab;
                    }
                    else
                    {
                        Debug.LogWarning($"无法找到预制体：{lootOption.PrefabPath}");
                    }
                }
                else
                {
                    Debug.LogWarning("PrefabPath 为空，无法设置 PrefabObj");
                }
            }

            // 标记修改
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("PrefabObj 已成功更新！");
        }
        else
        {
            Debug.LogError("选择的资产不是 GenerationConfig 类型！");
        }
    }
}
