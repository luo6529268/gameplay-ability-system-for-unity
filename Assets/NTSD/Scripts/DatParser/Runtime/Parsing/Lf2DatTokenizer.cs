using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件分词器
    /// 将 dat 文本按照 LF2 的规则分割成 token 序列
    /// </summary>
    public static class Lf2DatTokenizer
    {
        internal static void ScanLoganScalarFields(string fragment, List<Lf2DatProperty> output, string context = null)
        {
            int cursor = 0;
            while (cursor < fragment.Length)
            {
                if (!IsLoganIdentifierStart(fragment[cursor]) ||
                    (cursor > 0 && IsLoganAlphaNumericOrUnderscore(fragment[cursor - 1])))
                {
                    cursor++;
                    continue;
                }
                int keyBegin = cursor;
                while (cursor < fragment.Length && (IsLoganAlphaNumericOrUnderscore(fragment[cursor]) ||
                    fragment[cursor] == '(' || fragment[cursor] == ')' || fragment[cursor] == '-'))
                    cursor++;
                if (cursor >= fragment.Length || fragment[cursor] != ':')
                    continue;
                string key = fragment.Substring(keyBegin, cursor - keyBegin);
                cursor++;
                while (cursor < fragment.Length && IsLoganWhitespace(fragment[cursor]))
                    cursor++;
                int valueBegin = cursor;
                while (cursor < fragment.Length && !IsLoganWhitespace(fragment[cursor]) && fragment[cursor] != '<')
                    cursor++;
                bool pairedIntegerText = (context == "armor" && key == "frame") ||
                    (context == "itr" && (key == "caughtact" || key == "catchingact" || key == "pickedact" || key == "pickingact"));
                if (pairedIntegerText)
                {
                    int second = cursor;
                    while (second < fragment.Length && IsLoganWhitespace(fragment[second]))
                        second++;
                    int end = second;
                    if (end < fragment.Length && (fragment[end] == '-' || fragment[end] == '+'))
                        end++;
                    int digitsBegin = end;
                    while (end < fragment.Length && fragment[end] >= '0' && fragment[end] <= '9')
                        end++;
                    if (end > digitsBegin)
                        cursor = end;
                }
                string value = fragment.Substring(valueBegin, cursor - valueBegin);
                if (key.EndsWith("_end", StringComparison.Ordinal) || key == "layer" ||
                    (value.Length == 0 && (key == "itr" || key == "bdy" || key == "opoint" ||
                        key == "wpoint" || key == "bpoint" || key == "cpoint" || key == "ppoint")))
                    continue;
                output.Add(new Lf2DatProperty(key, value));
            }
        }

        private static bool IsLoganIdentifierStart(char value)
        {
            return (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z') || value == '_';
        }

        private static bool IsLoganAlphaNumericOrUnderscore(char value)
        {
            return IsLoganIdentifierStart(value) || (value >= '0' && value <= '9');
        }

        private static bool IsLoganWhitespace(char value)
        {
            return value == ' ' || value == '\t' || value == '\r' || value == '\n' || value == '\v' || value == '\f';
        }

        // 分隔符定义（与 LF2.IDE 保持一致）
        private static readonly char[] TokenDelimiters = { ' ', '\t', '\r', '\n' };
        private static readonly char[] TokenDelimiterEnd = { '>', ':' };
        private static readonly char[] TokenDelimiterBegin = { '<' };

        /// <summary>
        /// 将 dat 文本分词成 token 数组
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>token 数组</returns>
        public static string[] Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();

            List<string> tokens = new List<string>(128);
            bool inToken = false;
            int tokenStart = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // 遇到 # 注释，跳过到行尾
                if (c == '#')
                {
                    if (inToken)
                    {
                        tokens.Add(text.Substring(tokenStart, i - tokenStart));
                        inToken = false;
                    }

                    // 跳过到换行符
                    while (i < text.Length && text[i] != '\n')
                        i++;
                    continue;
                }

                // 开始分隔符 <
                if (Array.IndexOf(TokenDelimiterBegin, c) >= 0)
                {
                    if (inToken)
                    {
                        tokens.Add(text.Substring(tokenStart, i - tokenStart));
                    }
                    tokenStart = i;
                    inToken = true;
                    continue;
                }

                // 结束分隔符 > 或 :
                if (Array.IndexOf(TokenDelimiterEnd, c) >= 0)
                {
                    if (inToken)
                    {
                        tokens.Add(text.Substring(tokenStart, i - tokenStart + 1));
                        inToken = false;
                    }
                    else
                    {
                        // 单独的 : 或 >（语法错误，但尝试容错）
                        tokens.Add(c.ToString());
                    }
                    continue;
                }

                // 普通分隔符（空格、制表符、换行）
                if (Array.IndexOf(TokenDelimiters, c) >= 0)
                {
                    if (inToken)
                    {
                        tokens.Add(text.Substring(tokenStart, i - tokenStart));
                        inToken = false;
                    }
                    continue;
                }

                // 普通字符
                if (!inToken)
                {
                    tokenStart = i;
                    inToken = true;
                }
            }

            // 处理最后一个 token
            if (inToken)
            {
                tokens.Add(text.Substring(tokenStart));
            }

            return tokens.ToArray();
        }
    }
}
