using System;
using UnityEngine;

namespace NTSD.Tools
{
    /// <summary>
    /// 统一日志输出类
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 全局日志开关，关闭时不输出任何日志
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// 详细日志开关，关闭时只输出 Warning 和 Error
        /// </summary>
        public static bool Verbose = true;

        /// <summary>
        /// 日志前缀
        /// </summary>
        private const string Prefix = "[NTSD]";

        /// <summary>
        /// 输出信息日志
        /// </summary>
        /// <param name="message">日志消息或格式字符串</param>
        /// <param name="args">格式参数（可选）</param>
        public static void Info(string message, params object[] args)
        {
            if (!Enabled || !Verbose) return;

            string finalMessage = FormatMessage(message, args);
            Debug.Log($"{Prefix} {finalMessage}");
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="message">日志消息或格式字符串</param>
        /// <param name="args">格式参数（可选）</param>
        public static void Warn(string message, params object[] args)
        {
            if (!Enabled) return;

            string finalMessage = FormatMessage(message, args);
            Debug.LogWarning($"{Prefix} {finalMessage}");
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">日志消息或格式字符串</param>
        /// <param name="args">格式参数（可选）</param>
        public static void Error(string message, params object[] args)
        {
            if (!Enabled) return;

            string finalMessage = FormatMessage(message, args);
            Debug.LogError($"{Prefix} {finalMessage}");
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">附加消息或格式字符串（可选）</param>
        /// <param name="args">格式参数（可选）</param>
        public static void Exception(Exception ex, string message = null, params object[] args)
        {
            if (!Enabled) return;

            if (!string.IsNullOrEmpty(message))
            {
                string finalMessage = FormatMessage(message, args);
                Debug.LogError($"{Prefix} {finalMessage}");
            }

            Debug.LogException(ex);
        }

        /// <summary>
        /// 格式化消息
        /// 如果没有参数，直接返回原消息；否则使用 string.Format
        /// </summary>
        private static string FormatMessage(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return message;
            }

            return string.Format(message, args);
        }
    }
}
