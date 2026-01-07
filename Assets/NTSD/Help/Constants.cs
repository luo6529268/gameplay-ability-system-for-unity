using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Help
{
    public static class Constants
    {
        public const int MaxFrameID = 999;

        /// <summary>
        /// LF2 像素单位到 Unity 世界单位的转换比例
        /// LF2 使用像素坐标，Unity 使用世界单位
        /// 根据项目实际情况调整此值
        /// </summary>
        public const float PIXEL_TO_UNIT = 0.01f;
    }
}
