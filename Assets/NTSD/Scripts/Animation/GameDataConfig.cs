using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 游戏全局数据配置（对应 data.txt）
    /// </summary>
    [Serializable]
    public class GameDataConfig
    {
        [Tooltip("所有游戏对象定义（角色、武器、技能特效等）")]
        public List<ObjectDefinition> objects = new List<ObjectDefinition>();

        [Tooltip("所有背景定义")]
        public List<BackgroundDefinition> backgrounds = new List<BackgroundDefinition>();
    }

    /// <summary>
    /// 对象定义（角色、武器、技能特效等）
    /// 对应 data.txt 中的 <object> 区块
    /// </summary>
    [Serializable]
    public class ObjectDefinition
    {
        [Tooltip("对象ID")]
        public int id;

        [Tooltip("对象类型\n0=角色\n1=?\n2=?\n3=技能特效\n4=武器\n5=?\n6=?")]
        public int type;

        [Tooltip("JSON 文件路径")]
        public string file;

        public ObjectDefinition() { }

        public ObjectDefinition(int id, int type, string file)
        {
            this.id = id;
            this.type = type;
            this.file = file;
        }

        /// <summary>
        /// 解析文件路径（处理不同的路径格式）
        /// </summary>
        public string GetResolvedPath(string baseDir = "Assets/NTSD/Config")
        {
            // 如果已经是 Assets/ 开头，直接返回
            if (file.StartsWith("Assets/") || file.StartsWith("assets/"))
            {
                return file;
            }

            // 处理 chars\ 或 chars/ 格式
            string normalizedFile = file.Replace('\\', '/');

            // 如果以 chars/ 或其他目录开头，添加基础路径
            if (normalizedFile.StartsWith("chars/") ||
                normalizedFile.StartsWith("jsona/") ||
                normalizedFile.StartsWith("bg/"))
            {
                // NTSD 原始路径不包含 Config，直接是相对路径
                return $"{baseDir}/{normalizedFile}";
            }

            return file;
        }
    }

    /// <summary>
    /// 背景定义
    /// 对应 data.txt 中的 <background> 区块
    /// </summary>
    [Serializable]
    public class BackgroundDefinition
    {
        [Tooltip("背景ID")]
        public int id;

        [Tooltip("JSON 文件路径")]
        public string file;

        public BackgroundDefinition() { }

        public BackgroundDefinition(int id, string file)
        {
            this.id = id;
            this.file = file;
        }

        /// <summary>
        /// 解析文件路径
        /// </summary>
        public string GetResolvedPath(string baseDir = "Assets/NTSD/Config")
        {
            if (file.StartsWith("Assets/") || file.StartsWith("assets/"))
            {
                return file;
            }

            string normalizedFile = file.Replace('\\', '/');
            return $"{baseDir}/{normalizedFile}";
        }
    }
}
