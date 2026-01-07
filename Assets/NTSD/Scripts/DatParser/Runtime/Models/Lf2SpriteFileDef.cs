using System;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 精灵文件定义
    /// 对应 bmp_begin 节中的 file(x-y): path w: h: row: col:
    /// </summary>
    [Serializable]
    public class Lf2SpriteFileDef
    {
        public int StartIndex;  // 起始帧索引
        public int EndIndex;    // 结束帧索引
        public string Path;     // 文件路径
        public int Width;       // 单个精灵宽度
        public int Height;      // 单个精灵高度
        public int Row;         // 行数
        public int Col;         // 列数

        public override string ToString()
        {
            return $"file({StartIndex}-{EndIndex}): {Path} w:{Width} h:{Height} row:{Row} col:{Col}";
        }
    }
}
