#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace NTSD.Animation.Editor
{
    /// <summary>
    /// data.txt 查看和测试工具
    /// </summary>
    public class DataTxtToJsonConverter : OdinEditorWindow
    {
        [MenuItem("NTSD/Tools/Data.txt 查看器")]
        private static void OpenWindow()
        {
            GetWindow<DataTxtToJsonConverter>("Data.txt 查看器").Show();
        }

        #region 配置

        [Title("配置", "Configuration", TitleAlignment = TitleAlignments.Centered)]
        [LabelText("data.txt 路径")]
        [Sirenix.OdinInspector.FilePath(Extensions = "txt")]
        [PropertyOrder(0)]
        public string dataTxtPath = "Assets/NTSD/Config/data.txt";

        #endregion

        #region 解析结果

        [Title("解析结果", "Parse Result", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("@parsedConfig != null")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("对象数量")]
        [PropertyOrder(10)]
        private int ObjectCount => parsedConfig?.objects?.Count ?? 0;

        [ShowIf("@parsedConfig != null")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("背景数量")]
        [PropertyOrder(11)]
        private int BackgroundCount => parsedConfig?.backgrounds?.Count ?? 0;

        [ShowIf("@parsedConfig != null")]
        [FoldoutGroup("解析数据", Expanded = false)]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(12)]
        private GameDataConfig parsedConfig;

        #endregion

        #region 操作按钮

        [Title("操作", "Actions", TitleAlignment = TitleAlignments.Centered)]
        [Button("解析 data.txt", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [PropertyOrder(20)]
        private void ParseDataTxt()
        {
            if (!File.Exists(dataTxtPath))
            {
                EditorUtility.DisplayDialog("错误", $"文件不存在:\n{dataTxtPath}", "确定");
                return;
            }

            try
            {
                string content = File.ReadAllText(dataTxtPath, Encoding.UTF8);
                parsedConfig = ParseDataFile(content);

                Debug.Log($"<color=green>✅ 解析成功: 对象={ObjectCount}, 背景={BackgroundCount}</color>");
                EditorUtility.DisplayDialog("解析成功",
                    $"对象数量: {ObjectCount}\n背景数量: {BackgroundCount}",
                    "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>❌ 解析失败: {e.Message}\n{e.StackTrace}</color>");
                EditorUtility.DisplayDialog("解析失败", e.Message, "确定");
            }
        }

        #endregion

        #region 查询功能

        [Title("查询", "Query", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("@parsedConfig != null")]
        [HorizontalGroup("Query/Row1")]
        [LabelText("对象 ID")]
        [PropertyOrder(30)]
        public int searchObjectId = 0;

        [ShowIf("@parsedConfig != null")]
        [HorizontalGroup("Query/Row1")]
        [Button("查找对象", ButtonSizes.Medium)]
        [GUIColor(0.4f, 1f, 0.4f)]
        [PropertyOrder(30)]
        private void SearchObjectById()
        {
            if (parsedConfig == null || parsedConfig.objects == null)
            {
                Debug.LogWarning("请先解析 data.txt");
                return;
            }

            var obj = parsedConfig.objects.Find(o => o.id == searchObjectId);
            if (obj != null)
            {
                selectedObject = obj;
                Debug.Log($"<color=green>✅ 找到对象: ID={obj.id}, Type={obj.type}, File={obj.file}</color>");
            }
            else
            {
                selectedObject = null;
                Debug.LogWarning($"<color=yellow>⚠️ 未找到 ID={searchObjectId} 的对象</color>");
                EditorUtility.DisplayDialog("未找到", $"未找到 ID={searchObjectId} 的对象", "确定");
            }
        }

        [ShowIf("@parsedConfig != null")]
        [HorizontalGroup("Query/Row2")]
        [LabelText("对象类型")]
        [PropertyOrder(31)]
        [ValueDropdown("GetObjectTypes")]
        public int filterObjectType = -1;

        [ShowIf("@parsedConfig != null")]
        [HorizontalGroup("Query/Row2")]
        [Button("按类型筛选", ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [PropertyOrder(31)]
        private void FilterByType()
        {
            if (parsedConfig == null || parsedConfig.objects == null)
            {
                Debug.LogWarning("请先解析 data.txt");
                return;
            }

            if (filterObjectType < 0)
            {
                filteredObjects = new List<ObjectDefinition>(parsedConfig.objects);
                Debug.Log($"<color=cyan>显示所有对象: {filteredObjects.Count} 个</color>");
            }
            else
            {
                filteredObjects = parsedConfig.objects.FindAll(o => o.type == filterObjectType);
                Debug.Log($"<color=cyan>类型 {filterObjectType} 的对象: {filteredObjects.Count} 个</color>");
            }
        }

        private IEnumerable<ValueDropdownItem<int>> GetObjectTypes()
        {
            yield return new ValueDropdownItem<int>("全部类型", -1);
            yield return new ValueDropdownItem<int>("0 - 角色", 0);
            yield return new ValueDropdownItem<int>("1 - 未知1", 1);
            yield return new ValueDropdownItem<int>("2 - 未知2", 2);
            yield return new ValueDropdownItem<int>("3 - 技能特效", 3);
            yield return new ValueDropdownItem<int>("4 - 武器", 4);
            yield return new ValueDropdownItem<int>("5 - 未知5", 5);
            yield return new ValueDropdownItem<int>("6 - 未知6", 6);
        }

        [ShowIf("@selectedObject != null")]
        [Title("选中对象", "Selected Object", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(40)]
        private ObjectDefinition selectedObject;

        [ShowIf("@filteredObjects != null && filteredObjects.Count > 0")]
        [Title("筛选结果", "Filtered Results", TitleAlignment = TitleAlignments.Centered)]
        [FoldoutGroup("筛选列表", Expanded = false)]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(50)]
        private List<ObjectDefinition> filteredObjects;

        #endregion

        #region 解析逻辑

        /// <summary>
        /// 解析 data.txt 文件内容
        /// </summary>
        private GameDataConfig ParseDataFile(string content)
        {
            GameDataConfig config = new GameDataConfig();

            // 提取 <object> 区块
            Match objectMatch = Regex.Match(content, @"<object>(.*?)<object_end>", RegexOptions.Singleline);
            if (objectMatch.Success)
            {
                string objectBlock = objectMatch.Groups[1].Value;
                config.objects = ParseObjects(objectBlock);
            }

            // 提取 <background> 区块
            Match bgMatch = Regex.Match(content, @"<background>(.*?)<background_end>", RegexOptions.Singleline);
            if (bgMatch.Success)
            {
                string bgBlock = bgMatch.Groups[1].Value;
                config.backgrounds = ParseBackgrounds(bgBlock);
            }

            return config;
        }

        /// <summary>
        /// 解析对象列表
        /// </summary>
        private List<ObjectDefinition> ParseObjects(string block)
        {
            List<ObjectDefinition> objects = new List<ObjectDefinition>();

            // 正则表达式：匹配一行 object 定义
            // 格式：id: 50 type: 0 file: chars\pein.json
            string pattern = @"id:\s*(\d+)\s+type:\s*(\d+)\s+file:\s*([^\s#]+)\s*(?:#(.*))?";
            MatchCollection matches = Regex.Matches(block, pattern);

            foreach (Match match in matches)
            {
                int id = int.Parse(match.Groups[1].Value);
                int type = int.Parse(match.Groups[2].Value);
                string file = match.Groups[3].Value.Trim();

                objects.Add(new ObjectDefinition(id, type, file));
            }

            // 按 ID 排序
            objects.Sort((a, b) => a.id.CompareTo(b.id));

            return objects;
        }

        /// <summary>
        /// 解析背景列表
        /// </summary>
        private List<BackgroundDefinition> ParseBackgrounds(string block)
        {
            List<BackgroundDefinition> backgrounds = new List<BackgroundDefinition>();

            // 正则表达式：匹配一行 background 定义
            // 格式：id: 0 file: bg\sys\District\bg.json
            string pattern = @"id:\s*(\d+)\s+file:\s*([^\s#]+)";
            MatchCollection matches = Regex.Matches(block, pattern);

            foreach (Match match in matches)
            {
                int id = int.Parse(match.Groups[1].Value);
                string file = match.Groups[2].Value.Trim();

                backgrounds.Add(new BackgroundDefinition(id, file));
            }

            // 按 ID 排序
            backgrounds.Sort((a, b) => a.id.CompareTo(b.id));

            return backgrounds;
        }

        #endregion
    }
}
#endif
