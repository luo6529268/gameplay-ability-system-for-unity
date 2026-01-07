using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ObjectPool: ObjectPoolBase
    {
        public override float ExpireTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


        public override void Release()
        {
            throw new System.NotImplementedException();
        }

        public override void ReleaseAllUnused()
        {
            throw new System.NotImplementedException();
        }
    }
}
