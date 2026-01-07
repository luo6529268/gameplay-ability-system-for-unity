using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文本解析工具类
    /// 提供文本解析的各种辅助方法
    /// </summary>
    public static class Lf2DatTextUtils
    {
        // 键值对匹配正则（key: value 格式）
        private static readonly Regex KeyValueRegex = new Regex(@"([A-Za-z_][A-Za-z0-9_\-]*\(?[0-9\-]*\)?)\s*:\s*([^\s]+)", RegexOptions.Compiled);

        /// <summary>
        /// 去除注释（# 开头）
        /// </summary>
        public static string TrimComment(string line)
        {
            if (line == null)
                return string.Empty;

            int hash = line.IndexOf('#');
            if (hash >= 0)
                return line.Substring(0, hash);

            return line;
        }

        /// <summary>
        /// 尝试解析标签行（<tag_name> 格式）
        /// </summary>
        /// <param name="line">输入行</param>
        /// <param name="tagName">标签名</param>
        /// <param name="tail">标签后面的内容</param>
        /// <returns>是否成功解析</returns>
        public static bool TryParseTagLine(string line, out string tagName, out string tail)
        {
            tagName = null;
            tail = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            line = line.Trim();
            if (!line.StartsWith("<"))
                return false;

            int close = line.IndexOf('>');
            if (close < 0)
                return false;

            tagName = line.Substring(1, close - 1).Trim();
            tail = line.Substring(close + 1).Trim();
            return tagName.Length > 0;
        }

        /// <summary>
        /// 尝试解析子块行（name: 或 name_end: 格式）
        /// </summary>
        /// <param name="line">输入行</param>
        /// <param name="name">子块名</param>
        /// <param name="isEnd">是否为结束标记</param>
        /// <returns>是否成功解析</returns>
        public static bool TryParseSubBlockLine(string line, out string name, out bool isEnd)
        {
            name = null;
            isEnd = false;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            string trimmed = line.Trim();
            if (!trimmed.EndsWith(":"))
                return false;

            string token = trimmed.Substring(0, trimmed.Length - 1).Trim();
            if (token.Length == 0)
                return false;

            if (token.EndsWith("_end", StringComparison.OrdinalIgnoreCase))
            {
                name = token.Substring(0, token.Length - 4);
                isEnd = true;
            }
            else
            {
                name = token;
                isEnd = false;
            }

            return true;
        }

        /// <summary>
        /// 解析键值对（key: value 或 key value 格式）
        /// </summary>
        public static List<Lf2DatProperty> ParseKeyValuePairs(string line)
        {
            List<Lf2DatProperty> list = new List<Lf2DatProperty>();
            if (string.IsNullOrWhiteSpace(line))
                return list;

            line = TrimComment(line);

            // 尝试使用正则匹配 key: value 格式
            if (line.IndexOf(':') >= 0)
            {
                MatchCollection matches = KeyValueRegex.Matches(line);
                foreach (Match m in matches)
                {
                    string key = m.Groups[1].Value;
                    string value = m.Groups[2].Value;
                    list.Add(new Lf2DatProperty(key, value));
                }
                return list;
            }

            // 尝试解析 key value 格式（空格分隔）
            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i + 1 < tokens.Length;)
            {
                string key = tokens[i];
                string value = tokens[i + 1];

                if (IsKeyCandidate(key) && !value.Contains(":"))
                {
                    list.Add(new Lf2DatProperty(key, value));
                    i += 2;
                }
                else
                {
                    i++;
                }
            }

            return list;
        }

        /// <summary>
        /// 尝试解析精灵文件定义（file(x-y): path w: h: row: col:）
        /// </summary>
        public static bool TryParseSpriteFileDef(string line, out Lf2SpriteFileDef def)
        {
            def = null;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            Match match = Regex.Match(line, @"file\((\d+)\-(\d+)\):\s*([^\s]+)");
            if (!match.Success)
                return false;

            def = new Lf2SpriteFileDef();
            def.StartIndex = ParseInt(match.Groups[1].Value);
            def.EndIndex = ParseInt(match.Groups[2].Value);
            def.Path = match.Groups[3].Value;

            def.Width = ParseInt(FindValue(line, "w"));
            def.Height = ParseInt(FindValue(line, "h"));
            def.Row = ParseInt(FindValue(line, "row"));
            def.Col = ParseInt(FindValue(line, "col"));

            return true;
        }

        /// <summary>
        /// 解析帧头（<frame> 0 standing）
        /// </summary>
        public static void ParseFrameHeader(string tail, Lf2FrameBlock frame)
        {
            if (frame == null || string.IsNullOrWhiteSpace(tail))
                return;

            string[] tokens = tail.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return;

            int idx;
            if (int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out idx))
            {
                frame.FrameIndex = idx;
                if (tokens.Length > 1)
                    frame.FrameName = tokens[1];
            }
            else
            {
                frame.FrameName = tokens[0];
            }
        }

        /// <summary>
        /// 检查是否为有效的键名
        /// </summary>
        private static bool IsKeyCandidate(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            // 键名不能以数字开头
            if (char.IsDigit(token[0]))
                return false;

            // 检查字符是否合法
            for (int i = 0; i < token.Length; i++)
            {
                char c = token[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 查找值（key: value 格式）
        /// </summary>
        private static string FindValue(string line, string key)
        {
            Match m = Regex.Match(line, key + @"\s*:\s*([-]?[0-9]+)");
            if (m.Success)
                return m.Groups[1].Value;

            return null;
        }

        /// <summary>
        /// 解析整数
        /// </summary>
        private static int ParseInt(string value)
        {
            int result;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }
    }
}
