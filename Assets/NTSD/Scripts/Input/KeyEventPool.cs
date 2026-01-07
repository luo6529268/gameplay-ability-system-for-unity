using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Input
{
    [System.Flags]
    public enum FuncKeyMask : uint
    {
        None = 0,
        Attack = 1 << 0, // J 默认
        Jump = 1 << 1, // K 默认
        Defend = 1 << 2, // L 默认
        Left = 1 << 3,
        Right = 1 << 4,
        Down = 1 << 5,
        Up = 1 << 6
    }

    public struct KeyEvent
    {
        public uint funcCode;
        public float time;

        public bool IsEmpty => funcCode == 0;
        
        /// <summary>
        /// 检查是否包含指定按键
        /// </summary>
        public bool HasKey(FuncKeyMask key)
        {
            return (funcCode & (uint)key) == (uint)key;
        }
    }

    public class KeyEventPool
    {
        public int Count => size;
        public KeyEvent this[int index] => pool[index];

        private KeyEvent[] pool;
        private int size;

        public KeyEventPool(int size = 5)
        {
            this.size = size;
            pool = new KeyEvent[size];
            // 初始化为 None time = 0
            for (int i = 0; i < size; i++) pool[i] = new KeyEvent { funcCode = 0, time = 0f };
        }

        // push latest to index 0, shift others to +1
        public void Push(KeyEvent item)
        {
            for (int i = size - 2; i >= 0; i--) pool[i + 1] = pool[i];
            pool[0] = item;
        }

        // 返回 OR 后的 mask （只 OR 时间窗口内的事件）
        public uint ComputeMaskWithinWindow(float windowSeconds)
        {
            float now = Time.time;
            uint result = 0;
            for (int i = 0; i < size; i++)
            {
                if (pool[i].funcCode == 0) continue;
                if (now - pool[i].time <= windowSeconds)
                {
                    result |= pool[i].funcCode;
                }
            }
            return result;
        }

        /// <summary>
        /// 检查序列是否匹配（类似FLF的序列检测）
        /// </summary>
        /// <param name="sequence">按键序列（从旧到新）</param>
        /// <param name="maxTime">序列首尾最大时间差（秒）</param>
        /// <returns>是否匹配</returns>
        public bool MatchSequence(FuncKeyMask[] sequence, float maxTime = float.MaxValue)
        {
            if (sequence == null || sequence.Length == 0) return false;
            if (sequence.Length > size) return false;

            float now = Time.time;
            int seqIndex = sequence.Length - 1; // 从序列末尾开始（最新按键）

            for (int i = 0; i < size && seqIndex >= 0; i++)
            {
                if (pool[i].IsEmpty) continue;

                // 检查按键是否匹配
                if (pool[i].HasKey(sequence[seqIndex]))
                {
                    // 检查时间限制
                    if (seqIndex == sequence.Length - 1)
                    {
                        // 最新按键必须在maxTime内
                        if (now - pool[i].time > maxTime) return false;
                    }
                    else
                    {
                        // 检查首尾时间差
                        float firstKeyTime = pool[i].time;
                        if (now - firstKeyTime > maxTime) return false;
                    }

                    seqIndex--;
                }
            }

            return seqIndex < 0; // 所有按键都匹配
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= size) return false;
            pool[index] = new KeyEvent { funcCode = 0, time = 0f };
            return true;
        }

        // 便捷：清空
        public void Clear()
        {
            for (int i = 0; i < size; i++) pool[i] = new KeyEvent { funcCode = 0, time = 0f };
        }
    }
}
