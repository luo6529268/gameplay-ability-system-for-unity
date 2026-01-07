using System.IO;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2角色配置JSON加载器
    /// </summary>
    public static class LF2CharacterJsonLoader
    {
        /// <summary>
        /// 从文件路径加载角色数据
        /// </summary>
        /// <param name="jsonFilePath">JSON文件的完整路径</param>
        /// <returns>角色配置数据包装器</returns>
        public static LF2CharacterDataWrapper LoadFromFile(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                Debug.LogError($"JSON文件不存在: {jsonFilePath}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                return LoadFromJsonString(jsonContent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"读取JSON文件失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从JSON字符串加载角色数据
        /// </summary>
        /// <param name="jsonString">JSON字符串内容</param>
        /// <returns>角色配置数据包装器</returns>
        public static LF2CharacterDataWrapper LoadFromJsonString(string jsonString)
        {
            try
            {
                LF2CharacterDataWrapper wrapper = JsonUtility.FromJson<LF2CharacterDataWrapper>(jsonString);

                if (wrapper == null)
                {
                    Debug.LogError("JSON解析失败：返回null");
                    return null;
                }

                Debug.Log($"成功加载角色数据: ID={wrapper.characterId}, Name={wrapper.characterData.name}");
                return wrapper;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析JSON失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从Resources文件夹加载角色数据（文件需要放在Resources文件夹内）
        /// </summary>
        /// <param name="resourcePath">Resources相对路径（不含.json扩展名）</param>
        /// <returns>角色配置数据包装器</returns>
        public static LF2CharacterDataWrapper LoadFromResources(string resourcePath)
        {
            TextAsset jsonTextAsset = Resources.Load<TextAsset>(resourcePath);

            if (jsonTextAsset == null)
            {
                Debug.LogError($"无法从Resources加载: {resourcePath}");
                return null;
            }

            return LoadFromJsonString(jsonTextAsset.text);
        }

        /// <summary>
        /// 从StreamingAssets文件夹加载角色数据
        /// </summary>
        /// <param name="fileName">文件名（包含.json扩展名）</param>
        /// <returns>角色配置数据包装器</returns>
        public static LF2CharacterDataWrapper LoadFromStreamingAssets(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            return LoadFromFile(filePath);
        }

        /// <summary>
        /// 根据角色ID加载角色数据（从指定目录）
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="folderPath">JSON文件所在文件夹路径</param>
        /// <returns>角色配置数据包装器</returns>
        public static LF2CharacterDataWrapper LoadByCharacterId(int characterId, string folderPath)
        {
            string fileName = $"character_{characterId}_data.json";
            string filePath = Path.Combine(folderPath, fileName);
            return LoadFromFile(filePath);
        }

        /// <summary>
        /// 打印角色数据信息（用于调试）
        /// </summary>
        public static void PrintCharacterInfo(LF2CharacterDataWrapper wrapper)
        {
            if (wrapper == null || wrapper.characterData == null)
            {
                Debug.LogWarning("角色数据为空");
                return;
            }

            var data = wrapper.characterData;
            Debug.Log("===== 角色数据信息 =====");
            Debug.Log($"角色ID: {wrapper.characterId}");
            Debug.Log($"角色名称: {data.name}");
            Debug.Log($"头像: {data.head}");
            Debug.Log($"精灵图数量: {data.files.Count}");
            Debug.Log($"帧数量: {data.frames.Count}");
            Debug.Log($"行走速度: {data.walking_speed}");
            Debug.Log($"奔跑速度: {data.running_speed}");
            Debug.Log($"跳跃高度: {data.jump_height}");
        }

        /// <summary>
        /// 获取指定帧数据
        /// </summary>
        public static LF2FrameData GetFrameData(LF2CharacterDataWrapper wrapper, int frameId)
        {
            if (wrapper == null || wrapper.characterData == null)
            {
                Debug.LogError("角色数据为空");
                return null;
            }

            foreach (var frame in wrapper.characterData.frames)
            {
                if (frame.frameId == frameId)
                {
                    return frame;
                }
            }

            Debug.LogWarning($"未找到帧ID: {frameId}");
            return null;
        }
    }
}
