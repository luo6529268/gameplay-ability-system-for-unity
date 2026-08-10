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
        public static bool Verbose = false;

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

        // =====================================================================
        // State Logger — Ring Buffer
        // =====================================================================

        /// <summary>状态日志级别</summary>
        public enum StateLogLevel { Info, Warn, Error }

        /// <summary>单条状态日志条目</summary>
        public struct StateLogEntry
        {
            public int    FrameCount;   // Time.frameCount
            public string ObjectName;
            public string Category;     // "Combo" / "Trans" / "Frame" / "Lock"
            public string Message;
            public StateLogLevel Level;
        }

        /// <summary>全局状态日志开关</summary>
        public static bool StateLogEnabled = false;

        /// <summary>检测到 Error 级别日志时自动暂停（关闭 StateLogEnabled），防止关键帧被冲掉</summary>
        public static bool AutoPauseOnError = false;

        // 触发式捕获：仅记录指定帧数内的日志，-1 = 不限制
        private static int _captureUntilFrame = -1;

        /// <summary>剩余捕获帧数（-1 表示未启用触发式捕获）</summary>
        public static int CaptureFramesRemaining
            => _captureUntilFrame < 0 ? -1
             : UnityEngine.Mathf.Max(0, _captureUntilFrame - UnityEngine.Time.frameCount);

        /// <summary>
        /// 触发式捕获：从现在起，记录接下来 <paramref name="frames"/> 帧内的日志，然后自动停止。
        /// 同时重新打开 StateLogEnabled，方便从暂停状态一键启动。
        /// </summary>
        public static void CaptureFrames(int frames)
        {
            _captureUntilFrame = UnityEngine.Time.frameCount + frames;
            StateLogEnabled    = true;
        }

        /// <summary>取消触发式捕获，恢复持续记录模式</summary>
        public static void CancelCapture() => _captureUntilFrame = -1;

        private const int StateBufferSize = 2000;
        private static readonly StateLogEntry[] _stateBuffer = new StateLogEntry[StateBufferSize];
        private static int _stateHead  = 0;   // 下一个写入位置
        private static int _stateCount = 0;   // 已写入条目数（最多 StateBufferSize）
        private static int _stateVersion = 0; // 每次写入自增，EditorWindow 轮询用

        /// <summary>版本号：每次 LogState 写入后自增，供 EditorWindow 检测变化</summary>
        public static int StateVersion => _stateVersion;

        /// <summary>
        /// 写入一条状态日志（线程不安全，仅供主线程调用）
        /// </summary>
        public static void LogState(string objectName, string category, string message,
                                    StateLogLevel level = StateLogLevel.Info)
        {
            if (!StateLogEnabled) return;

            // 触发式捕获：超出窗口则自动停止
            if (_captureUntilFrame >= 0 && UnityEngine.Time.frameCount > _captureUntilFrame)
            {
                StateLogEnabled    = false;
                _captureUntilFrame = -1;
                return;
            }

            _stateBuffer[_stateHead] = new StateLogEntry
            {
                FrameCount  = UnityEngine.Time.frameCount,
                ObjectName  = objectName ?? "?",
                Category    = category   ?? "",
                Message     = message    ?? "",
                Level       = level,
            };

            _stateHead = (_stateHead + 1) % StateBufferSize;
            if (_stateCount < StateBufferSize) _stateCount++;
            _stateVersion++;

            // Error 出现时自动冻结日志，避免关键帧被后续刷新覆盖
            if (level == StateLogLevel.Error && AutoPauseOnError)
                StateLogEnabled = false;
        }

        /// <summary>清空状态日志缓冲区</summary>
        public static void ClearStateLog()
        {
            _stateHead    = 0;
            _stateCount   = 0;
            _stateVersion++;
        }

        /// <summary>
        /// 返回所有有效条目（最旧 → 最新顺序）
        /// </summary>
        public static StateLogEntry[] GetStateSnapshot()
        {
            if (_stateCount == 0)
                return System.Array.Empty<StateLogEntry>();

            var result = new StateLogEntry[_stateCount];
            if (_stateCount < StateBufferSize)
            {
                // 缓冲区未满，从 0 到 _stateHead-1
                System.Array.Copy(_stateBuffer, 0, result, 0, _stateCount);
            }
            else
            {
                // 缓冲区已满，_stateHead 是最旧条目的位置
                int tail = StateBufferSize - _stateHead;
                System.Array.Copy(_stateBuffer, _stateHead, result, 0,    tail);
                System.Array.Copy(_stateBuffer, 0,          result, tail, _stateHead);
            }
            return result;
        }
    }
}
