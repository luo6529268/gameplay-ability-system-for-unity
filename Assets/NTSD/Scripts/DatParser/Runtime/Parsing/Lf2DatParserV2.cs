using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件解析器 V2
    /// 使用 Token 化方法，与 LF2.IDE 的实现保持一致
    /// </summary>
    public class Lf2DatParserV2
    {
        /// <summary>
        /// 解析 dat 文件文本
        /// </summary>
        public Lf2DatFile Parse(string text, string sourcePath = null)
        {
            Lf2DatFile dat = new Lf2DatFile
            {
                SourcePath = sourcePath,
                FileName = string.IsNullOrEmpty(sourcePath) ? null : System.IO.Path.GetFileName(sourcePath)
            };

            if (string.IsNullOrEmpty(text))
                return dat;

            // Token 化整个文本
            string[] tokens = Lf2DatTokenizer.Tokenize(text);

            // 使用栈来处理嵌套结构
            Stack<object> stack = new Stack<object>();
            stack.Push(dat);

            // 逐 token 解析
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                // 标签开始: <xxx>
                if (token.StartsWith("<") && token.EndsWith(">"))
                {
                    string tagName = token.Substring(1, token.Length - 2).Trim();

                    // 结束标签: <xxx_end>
                    if (tagName.EndsWith("_end", StringComparison.OrdinalIgnoreCase))
                    {
                        string openName = tagName.Substring(0, tagName.Length - 4);
                        CloseTag(stack, openName);
                        continue;
                    }

                    // bmp_begin
                    if (string.Equals(tagName, "bmp_begin", StringComparison.OrdinalIgnoreCase))
                    {
                        Lf2BmpSection bmp = new Lf2BmpSection();
                        dat.Bmp = bmp;
                        stack.Push(bmp);
                        continue;
                    }

                    // frame
                    if (string.Equals(tagName, "frame", StringComparison.OrdinalIgnoreCase))
                    {
                        Lf2FrameBlock frame = new Lf2FrameBlock();

                        // 解析 frame 索引和名称（紧跟在 <frame> 后面的 token）
                        if (i + 1 < tokens.Length)
                        {
                            i++;
                            int frameIndex;
                            if (int.TryParse(tokens[i], out frameIndex))
                            {
                                frame.FrameIndex = frameIndex;
                                // 如果还有名称
                                if (i + 1 < tokens.Length && !tokens[i + 1].Contains(":") && !tokens[i + 1].StartsWith("<"))
                                {
                                    i++;
                                    frame.FrameName = tokens[i];
                                }
                            }
                            else
                            {
                                frame.FrameName = tokens[i];
                            }
                        }

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

                // ⚠️ 特殊处理 file(x-y): 格式（BMP section）- 必须在普通 : 处理之前！
                if (token.StartsWith("file(") && token.EndsWith(":"))
                {
                    Lf2BmpSection bmpSection = stack.Peek() as Lf2BmpSection;
                    if (bmpSection != null)
                    {
                        Lf2SpriteFileDef fileDef = new Lf2SpriteFileDef();

                        // 解析 file(x-y):
                        string range = token.Substring(5, token.Length - 7); // 去掉 "file(" 和 "):"
                        string[] parts = range.Split('-');
                        if (parts.Length == 2)
                        {
                            int.TryParse(parts[0], out fileDef.StartIndex);
                            int.TryParse(parts[1], out fileDef.EndIndex);
                        }

                        // 读取文件路径（下一个token）
                        if (i + 1 < tokens.Length)
                        {
                            i++;
                            fileDef.Path = tokens[i];
                        }

                        // 读取后续属性 w: h: row: col:
                        while (i + 2 < tokens.Length && tokens[i + 1].EndsWith(":"))
                        {
                            // ⚠️ 检查下一个 token 是否是新的 file() 定义
                            // 如果是，跳出循环，让外层处理
                            if (tokens[i + 1].StartsWith("file("))
                            {
                                break;
                            }

                            i++;
                            string attrKey = tokens[i].TrimEnd(':');
                            i++;
                            int attrValue = 0;
                            int.TryParse(tokens[i], out attrValue);

                            if (string.Equals(attrKey, "w", StringComparison.OrdinalIgnoreCase))
                                fileDef.Width = attrValue;
                            else if (string.Equals(attrKey, "h", StringComparison.OrdinalIgnoreCase))
                                fileDef.Height = attrValue;
                            else if (string.Equals(attrKey, "row", StringComparison.OrdinalIgnoreCase))
                                fileDef.Row = attrValue;
                            else if (string.Equals(attrKey, "col", StringComparison.OrdinalIgnoreCase))
                                fileDef.Col = attrValue;
                        }

                        bmpSection.Files.Add(fileDef);

                        // 调试日志：记录解析到的 BMP 文件定义
                        UnityEngine.Debug.Log($"<color=cyan>[Parser] 解析 BMP 文件定义: {fileDef.Path}, 范围=[{fileDef.StartIndex}-{fileDef.EndIndex}], w={fileDef.Width}, h={fileDef.Height}, row={fileDef.Row}, col={fileDef.Col}</color>");
                    }
                    continue;
                }

                // 处理以 : 结尾的 token
                if (token.EndsWith(":"))
                {
                    string name = token.Substring(0, token.Length - 1).Trim();

                    // 子块结束: xxx_end:
                    if (name.EndsWith("_end", StringComparison.OrdinalIgnoreCase))
                    {
                        string openName = name.Substring(0, name.Length - 4);
                        CloseSubBlock(stack, openName);
                        continue;
                    }

                    // 判断是子块还是键值对
                    // 子块：opoint, bpoint, cpoint, wpoint, itr, bdy
                    bool isSubBlock = string.Equals(name, "opoint", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(name, "bpoint", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(name, "cpoint", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(name, "wpoint", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(name, "itr", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(name, "bdy", StringComparison.OrdinalIgnoreCase);

                    if (isSubBlock)
                    {
                        // 创建子块
                        Lf2DatSubBlock sub = new Lf2DatSubBlock { Name = name };
                        AddSubBlock(stack, sub);
                        stack.Push(sub);
                        continue;
                    }
                    else
                    {
                        // 键值对: key: 后跟 value
                        if (i + 1 < tokens.Length)
                        {
                            string key = name;
                            i++;
                            string value = tokens[i];

                            // 添加属性
                            Lf2DatProperty prop = new Lf2DatProperty(key, value);
                            AddProperty(stack.Peek(), prop);

                            // 特殊处理 BmpSection
                            object current = stack.Peek();
                            Lf2BmpSection bmpSection = current as Lf2BmpSection;
                            if (bmpSection != null)
                            {
                                if (string.Equals(key, "name", StringComparison.OrdinalIgnoreCase))
                                    bmpSection.Name = value;
                                else if (string.Equals(key, "head", StringComparison.OrdinalIgnoreCase))
                                    bmpSection.Head = value;
                                else if (string.Equals(key, "small", StringComparison.OrdinalIgnoreCase))
                                    bmpSection.Small = value;
                            }

                            continue;
                        }
                    }
                }
            }

            return dat;
        }

        private static void AddProperty(object target, Lf2DatProperty prop)
        {
            ILf2DatPropertyContainer container = target as ILf2DatPropertyContainer;
            if (container != null)
                container.AddProperty(prop);
        }

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
