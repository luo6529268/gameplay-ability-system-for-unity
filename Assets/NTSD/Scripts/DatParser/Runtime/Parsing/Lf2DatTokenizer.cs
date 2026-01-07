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
