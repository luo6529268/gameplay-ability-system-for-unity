using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public abstract class ObjectPoolBase
    {
        /// <summary>
        /// 获取或设置对象池对象过期秒数。
        /// </summary>
        public abstract float ExpireTime
        {
            get;
            set;
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public abstract void ReleaseAllUnused();
    }
}
