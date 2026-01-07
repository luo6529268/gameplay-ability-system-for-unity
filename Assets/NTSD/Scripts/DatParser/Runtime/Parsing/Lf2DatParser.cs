using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件解析器
    /// 将 dat 文件文本解析为结构化数据
    /// </summary>
    public class Lf2DatParser
    {
        /// <summary>
        /// 解析 dat 文件文本
        /// </summary>
        /// <param name="text">文件文本内容</param>
        /// <param name="sourcePath">源文件路径（可选）</param>
        /// <returns>解析后的 Lf2DatFile 对象</returns>
        public Lf2DatFile Parse(string text, string sourcePath = null)
        {
            Lf2DatFile dat = new Lf2DatFile
            {
                SourcePath = sourcePath,
                FileName = string.IsNullOrEmpty(sourcePath) ? null : System.IO.Path.GetFileName(sourcePath)
            };

            // 使用栈来处理嵌套结构
            Stack<object> stack = new Stack<object>();
            stack.Push(dat);

            if (string.IsNullOrEmpty(text))
                return dat;

            // 按行解析
            string[] lines = text.Replace("\r", "").Split('\n');
            foreach (string rawLine in lines)
            {
                // 去除注释和首尾空格
                string line = Lf2DatTextUtils.TrimComment(rawLine).Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // 尝试解析标签行（<xxx> 或 <xxx_end>）
                string tagName;
                string tail;
                if (Lf2DatTextUtils.TryParseTagLine(line, out tagName, out tail))
                {
                    // 处理结束标签
                    if (tagName.EndsWith("_end", StringComparison.OrdinalIgnoreCase))
                    {
                        string openName = tagName.Substring(0, tagName.Length - 4);
                        CloseTag(stack, openName);
                        continue;
                    }

                    // 处理 bmp_begin
                    if (string.Equals(tagName, "bmp_begin", StringComparison.OrdinalIgnoreCase))
                    {
                        Lf2BmpSection bmp = new Lf2BmpSection();
                        dat.Bmp = bmp;
                        stack.Push(bmp);
                        continue;
                    }

                    // 处理 frame
                    if (string.Equals(tagName, "frame", StringComparison.OrdinalIgnoreCase))
                    {
                        Lf2FrameBlock frame = new Lf2FrameBlock();
                        Lf2DatTextUtils.ParseFrameHeader(tail, frame);
                        dat.Frames.Add(frame);
                        stack.Push(frame);
                        continue;
                    }

                    // 检测文件类型
                    if (string.Equals(tagName, "object", StringComparison.OrdinalIgnoreCase))
                        dat.FileType = Lf2DatFileType.Object;
                    else if (string.Equals(tagName, "stage", StringComparison.OrdinalIgnoreCase))
                        dat.FileType = Lf2DatFileType.Stage;
                    else if (string.Equals(tagName, "background", StringComparison.OrdinalIgnoreCase))
                        dat.FileType = Lf2DatFileType.Background;

                    // 创建普通块
                    Lf2DatBlock block = new Lf2DatBlock { Name = tagName };
                    dat.Blocks.Add(block);
                    stack.Push(block);
                    continue;
                }

                // 尝试解析子块行（xxx: 或 xxx_end:）
                string subName;
                bool isEnd;
                if (Lf2DatTextUtils.TryParseSubBlockLine(line, out subName, out isEnd))
                {
                    if (isEnd)
                    {
                        CloseSubBlock(stack, subName);
                    }
                    else
                    {
                        Lf2DatSubBlock sub = new Lf2DatSubBlock { Name = subName };
                        AddSubBlock(stack, sub);
                        stack.Push(sub);
                    }
                    continue;
                }

                // 解析键值对
                List<Lf2DatProperty> props = Lf2DatTextUtils.ParseKeyValuePairs(line);
                if (props.Count == 0)
                    continue;

                // 添加属性到当前容器
                foreach (Lf2DatProperty prop in props)
                {
                    AddProperty(stack.Peek(), prop);
                }

                // 特殊处理 BmpSection
                object current = stack.Peek();
                Lf2BmpSection bmpSection = current as Lf2BmpSection;
                if (bmpSection != null)
                {
                    foreach (Lf2DatProperty prop in props)
                    {
                        if (string.Equals(prop.Key, "name", StringComparison.OrdinalIgnoreCase))
                            bmpSection.Name = prop.Value;
                        else if (string.Equals(prop.Key, "head", StringComparison.OrdinalIgnoreCase))
                            bmpSection.Head = prop.Value;
                        else if (string.Equals(prop.Key, "small", StringComparison.OrdinalIgnoreCase))
                            bmpSection.Small = prop.Value;
                    }

                    // 尝试解析精灵文件定义
                    Lf2SpriteFileDef fileDef;
                    if (Lf2DatTextUtils.TryParseSpriteFileDef(line, out fileDef))
                        bmpSection.Files.Add(fileDef);
                }
            }

            return dat;
        }

        /// <summary>
        /// 添加属性到容器
        /// </summary>
        private static void AddProperty(object target, Lf2DatProperty prop)
        {
            ILf2DatPropertyContainer container = target as ILf2DatPropertyContainer;
            if (container != null)
                container.AddProperty(prop);
        }

        /// <summary>
        /// 添加子块到当前块或帧
        /// </summary>
        private static void AddSubBlock(Stack<object> stack, Lf2DatSubBlock sub)
        {
            if (stack.Count == 0 || sub == null)
                return;

            object top = stack.Peek();
            Lf2FrameBlock frame = top as Lf2FrameBlock;
            if (frame != null)
            {
                frame.SubBlocks.Add(sub);
                return;
            }

            Lf2DatBlock block = top as Lf2DatBlock;
            if (block != null)
            {
                block.SubBlocks.Add(sub);
            }
        }

        /// <summary>
        /// 关闭标签（从栈中弹出）
        /// </summary>
        private static void CloseTag(Stack<object> stack, string openName)
        {
            if (stack.Count <= 1)
                return;

            object top = stack.Peek();
            if (top is Lf2BmpSection && string.Equals(openName, "bmp", StringComparison.OrdinalIgnoreCase))
            {
                stack.Pop();
                return;
            }

            if (top is Lf2FrameBlock && string.Equals(openName, "frame", StringComparison.OrdinalIgnoreCase))
            {
                stack.Pop();
                return;
            }

            Lf2DatBlock block = top as Lf2DatBlock;
            if (block != null && string.Equals(block.Name, openName, StringComparison.OrdinalIgnoreCase))
            {
                stack.Pop();
            }
        }

        /// <summary>
        /// 关闭子块（从栈中弹出）
        /// </summary>
        private static void CloseSubBlock(Stack<object> stack, string name)
        {
            if (stack.Count <= 1)
                return;

            object top = stack.Peek();
            Lf2DatSubBlock sub = top as Lf2DatSubBlock;
            if (sub != null && string.Equals(sub.Name, name, StringComparison.OrdinalIgnoreCase))
                stack.Pop();
        }
    }
}
