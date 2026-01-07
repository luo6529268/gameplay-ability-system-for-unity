using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// data.txt 文件解析器
    /// 解析 LF2/NTSD 的 data.txt 文件，提取对象定义
    /// 格式：&lt;object&gt; id: 50 type: 0 file: chars\pein.json &lt;object_end&gt;
    /// </summary>
    public class DataFileParser
    {
        /// <summary>
        /// 对象数据定义
        /// </summary>
        [System.Serializable]
        public class ObjectData
        {
            public int id;
            public int type;
            public string file;
        }

        /// <summary>
        /// 解析 data.txt 文件
        /// </summary>
        /// <param name="filePath">data.txt 的完整路径</param>
        /// <returns>对象 ID 到对象数据的映射</returns>
        public static Dictionary<int, ObjectData> ParseDataFile(string filePath)
        {
            Dictionary<int, ObjectData> objectMap = new Dictionary<int, ObjectData>();

            if (!File.Exists(filePath))
            {
                Debug.LogError($"<color=red>data.txt 文件不存在: {filePath}</color>");
                return objectMap;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // 提取 <object> ... <object_end> 之间的内容
                Match objectMatch = Regex.Match(content, @"<object>(.*?)<object_end>", RegexOptions.Singleline);

                if (!objectMatch.Success)
                {
                    Debug.LogWarning("<color=yellow>data.txt 中没有找到 <object> 块</color>");
                    return objectMap;
                }

                string objectBlock = objectMatch.Groups[1].Value;

                // 解析每一行对象定义
                // 格式：id: 50 type: 0 file: chars\pein.json
                // 或带注释：id: 100 type: 4 file: chars\weapon6.json #heal_scroll
                string pattern = @"id:\s*(\d+)\s+type:\s*(\d+)\s+file:\s*([^\s#]+)";
                MatchCollection matches = Regex.Matches(objectBlock, pattern);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        ObjectData data = new ObjectData
                        {
                            id = int.Parse(match.Groups[1].Value),
                            type = int.Parse(match.Groups[2].Value),
                            file = match.Groups[3].Value.Trim()
                        };

                        objectMap[data.id] = data;
                    }
                }

                Debug.Log($"<color=green>✅ data.txt 解析成功：共 {objectMap.Count} 个对象定义</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>❌ 解析 data.txt 失败: {e.Message}</color>");
            }

            return objectMap;
        }

        /// <summary>
        /// 根据 oid 查找对象数据
        /// </summary>
        public static ObjectData GetObjectData(Dictionary<int, ObjectData> dataMap, int oid)
        {
            if (dataMap.TryGetValue(oid, out ObjectData data))
            {
                return data;
            }

            Debug.LogWarning($"<color=yellow>⚠️ 未找到 oid={oid} 的对象定义</color>");
            return null;
        }

        /// <summary>
        /// 将相对路径转换为完整路径
        /// data.txt 中的路径格式：chars\pein.json 或 Assets/NTSD/Config/FrameConfig/naruto_clone.json
        /// </summary>
        public static string ResolveObjectFilePath(string dataFileDirectory, string relativePath)
        {
            // 如果是绝对路径（以 Assets/ 开头），直接使用
            if (relativePath.StartsWith("Assets"))
            {
                return relativePath;
            }

            // 否则，相对于 data.txt 所在目录
            // chars\pein.json -> I:\...\Config\chars\pein.json
            string normalizedPath = relativePath.Replace("\\", "/");
            return Path.Combine(dataFileDirectory, normalizedPath);
        }
    }
}
