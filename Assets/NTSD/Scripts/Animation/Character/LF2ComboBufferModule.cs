using NTSD.Input;
using NTSD.Tools;
using System;

namespace NTSD.Animation
{
    /// <summary>
    /// FLF/LF2 combo_buffer 的最小复刻（数据层，不继承 Mono）。
    /// 职责：
    /// - 接收 ActionSequenceDetector 的 combo_event 回调，写入 combo + timeout
    /// - 提供 combo_update 读取/清理规则
    /// - 处理 state_exit 清理规则（left-left/right-right 不能跨状态保留）
    /// </summary>
    public sealed class LF2ComboBufferModule
    {
        private string _combo;
        private int _timeout;

        public string Combo => string.IsNullOrEmpty(_combo) ? null : _combo;

        public void Reset()
        {
            _combo = null;
            _timeout = 0;
        }

        public void OnComboDetected(
            ComboConfig.ComboDefinition combo,
            bool allowSwitchDir,
            Action<string> setDirectionByString,
            int timeoutFrames,
            bool debugLog,
            int stableId)
        {
            string K = combo.name;

            // 1) 方向键切换（FLF character.js combo_event: left/right 且 allowSwitchDir）
            if (allowSwitchDir && (K == "left" || K == "right"))
            {
                setDirectionByString?.Invoke(K);
            }

            // 2) 同一窗口的优先级冲突处理（对齐现有实现）
            if (_timeout == timeoutFrames && !string.IsNullOrEmpty(_combo))
            {
                if (ComboConfig.GetComboPriority(K) < ComboConfig.GetComboPriority(_combo))
                    return;
            }

            // 3) 写入 buffer
            _combo = K;
            _timeout = timeoutFrames;
        }

        public void OnClearCombo() => _combo = null;

        /// <summary>
        /// 对齐 FLF: 每 TU 递减 combo_buffer.timeout；归零后清理部分指令。
        /// </summary>
        public void ReduceTimeout()
        {
            if (_timeout <= 0) return;

            _timeout--;
            if (_timeout != 0) return;

            switch (_combo)
            {
                case "def":
                case "jump":
                case "att":
                case "left-left":
                case "right-right":
                    _combo = null;
                    break;
            }
        }

        /// <summary>
        /// combo_update 结束后的清理规则（对齐现有实现，源自 FLF character.js:1832-1845）。
        /// </summary>
        public void AfterComboUpdate(bool curStateResult, bool generResult, string rawCombo, string mappedCombo)
        {
            if (rawCombo == "jump-att")
            {
                if (curStateResult|| generResult)
                {
                    _combo = "att";
                }
            }
            else
            {
                if (curStateResult || generResult ||mappedCombo == "left" || mappedCombo == "right" || mappedCombo == "up" || mappedCombo == "down")
                {
                    _combo = null;
                }
            }
        }
    }
}
