using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Input
{
    [System.Flags]
    public enum FuncKeyMask : uint
    {
        None = 0,
        att = 1 << 0, // J 默认
        jump = 1 << 1, // K 默认
        def = 1 << 2, // L 默认
        left = 1 << 3,
        right = 1 << 4,
        down = 1 << 5,
        up = 1 << 6
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
}
