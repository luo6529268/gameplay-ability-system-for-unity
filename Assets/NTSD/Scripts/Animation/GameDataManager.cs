using MoreMountains.Tools;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// data.txt 数据管理器（运行时访问）
    /// 提供静态 API 用于查询游戏对象定义
    /// </summary>
    public class GameDataManager:MMSingleton<GameDataManager>
    {
        private GameDataConfig cachedConfig;
        private Dictionary<int, ObjectDefinition> objectLookup;
        private readonly Dictionary<int, List<ObjectDefinition>> objectsByTypeLookup =
            new Dictionary<int, List<ObjectDefinition>>();
        private readonly Dictionary<int, BackgroundDefinition> backgroundLookup =
            new Dictionary<int, BackgroundDefinition>();
        private readonly List<ObjectDefinition> emptyObjectDefinitions =
            new List<ObjectDefinition>(0);

        public long UnloadedObjectQueryCountForDiagnostics { get; private set; }
        public long UnloadedTypeQueryCountForDiagnostics { get; private set; }
        public long UnloadedObjectListQueryCountForDiagnostics { get; private set; }
        public long UnloadedBackgroundQueryCountForDiagnostics { get; private set; }
        public int BackgroundCount => cachedConfig?.backgrounds?.Count ?? 0;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            LoadDataFile();
        }

        /// <summary>
        /// 加载 data.txt 文件
        /// </summary>
        public void LoadDataFile(string filePath = "Assets/NTSD/Config/data.txt")
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"<color=red>❌ data.txt 文件不存在: {filePath}</color>");
                return;
            }

            if (objectLookup?.Count > 0)
                return;

            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                cachedConfig = ParseDataFile(content);

                // 构建查找表
                objectLookup = new Dictionary<int, ObjectDefinition>(cachedConfig.objects.Count);
                objectsByTypeLookup.Clear();
                foreach (var obj in cachedConfig.objects)
                {
                    objectLookup[obj.id] = obj;

                    if (!objectsByTypeLookup.TryGetValue(
                            obj.type,
                            out List<ObjectDefinition> typedObjects))
                    {
                        typedObjects = new List<ObjectDefinition>();
                        objectsByTypeLookup.Add(obj.type, typedObjects);
                    }
                    typedObjects.Add(obj);
                }

                backgroundLookup.Clear();
                backgroundLookup.EnsureCapacity(cachedConfig.backgrounds.Count);
                foreach (BackgroundDefinition background in cachedConfig.backgrounds)
                    backgroundLookup[background.id] = background;

                Debug.Log($"<color=green>✅ GameDataManager 加载成功: 对象={cachedConfig.objects.Count}, 背景={cachedConfig.backgrounds.Count}</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>❌ GameDataManager 加载失败: {e.Message}\n{e.StackTrace}</color>");
            }
        }

        /// <summary>
        /// 根据 ID 获取对象定义
        /// </summary>
        public ObjectDefinition GetObjectById(int id)
        {
            if (cachedConfig == null || objectLookup == null)
            {
                UnloadedObjectQueryCountForDiagnostics++;
                return null;
            }

            if (objectLookup.TryGetValue(id, out ObjectDefinition obj))
            {
                return obj;
            }

            return null;
        }

        /// <summary>
        /// 根据类型获取对象列表
        /// </summary>
        public List<ObjectDefinition> GetObjectsByType(int type)
        {
            if (cachedConfig == null)
            {
                UnloadedTypeQueryCountForDiagnostics++;
                return emptyObjectDefinitions;
            }

            return objectsByTypeLookup.TryGetValue(
                type,
                out List<ObjectDefinition> objects)
                ? objects
                : emptyObjectDefinitions;
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        public List<ObjectDefinition> GetAllObjects()
        {
            if (cachedConfig == null)
            {
                UnloadedObjectListQueryCountForDiagnostics++;
                return emptyObjectDefinitions;
            }

            return cachedConfig.objects;
        }

        /// <summary>
        /// 根据 ID 获取背景定义
        /// </summary>
        public BackgroundDefinition GetBackgroundById(int id)
        {
            if (cachedConfig == null)
            {
                UnloadedBackgroundQueryCountForDiagnostics++;
                return null;
            }

            return backgroundLookup.TryGetValue(id, out BackgroundDefinition background)
                ? background
                : null;
        }

        /// <summary>
        /// 检查是否已加载
        /// </summary>
        public bool IsLoaded()
        {
            return cachedConfig != null;
        }

        /// <summary>
        /// 将 data.txt 中的相对路径解析为完整路径
        /// 对应原 DataFileParser.ResolveObjectFilePath
        /// </summary>
        public static string ResolveObjectFilePath(string dataFileDirectory, string relativePath)
        {
            if (relativePath.StartsWith("Assets"))
                return relativePath;
            string normalizedPath = relativePath.Replace("\\", "/");
            return Path.Combine(dataFileDirectory, normalizedPath);
        }

        #region 解析逻辑（与 DataTxtToJsonConverter 相同）

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

            return objects;
        }

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
