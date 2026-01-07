using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件完整结构
    /// 表示一个完整的 .dat 文件
    /// </summary>
    [Serializable]
    public class Lf2DatFile : ILf2DatPropertyContainer
    {
        public string SourcePath;                               // 源文件路径
        public string FileName;                                 // 文件名
        public Lf2DatFileType FileType = Lf2DatFileType.Unknown; // 文件类型
        public Lf2BmpSection Bmp;                               // BMP 精灵定义节
        public List<Lf2FrameBlock> Frames = new List<Lf2FrameBlock>();    // 帧列表
        public List<Lf2DatBlock> Blocks = new List<Lf2DatBlock>();        // 其他块

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties; // 根属性

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        /// <summary>
        /// 转换为 JSON 格式
        /// </summary>
        public string ToJson(bool prettyPrint = true)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public override string ToString()
        {
            return $"{FileName} ({FileType}) - {Frames.Count} frames, {Blocks.Count} blocks";
        }
    }
}
