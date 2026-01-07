using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// List扩展类 - 包含列表操作的扩展方法
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// 交换列表中两个元素的位置
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">要操作的列表</param>
        /// <param name="i">第一个元素的索引</param>
        /// <param name="j">第二个元素的索引</param>
        public static void MMSwap<T>(this IList<T> list, int i, int j)
        {
            // 临时存储第一个元素的值
            T temporary = list[i];
            // 将第二个元素的值赋给第一个元素
            list[i] = list[j];
            // 将临时存储的值赋给第二个元素
            list[j] = temporary;
        }

        /// <summary>
        /// 随机打乱列表中元素的顺序
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">要打乱的列表</param>
        public static void MMShuffle<T>(this IList<T> list)
        {
            // 遍历列表中的每个元素
            for (int i = 0; i < list.Count; i++)
            {
                // 将当前元素与随机位置的元素交换
                // Random.Range(i, list.Count) 生成从当前位置到列表末尾的随机索引
                list.MMSwap(i, Random.Range(i, list.Count));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachFast<T>(this List<T> list, System.Action<T> action)
        {
            for (int i = 0; i < list.Count; i++)
                action(list[i]);
        }
    }

}