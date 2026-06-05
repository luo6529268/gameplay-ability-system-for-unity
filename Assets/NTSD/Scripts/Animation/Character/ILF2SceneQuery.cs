using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// C++ release 语义下的战斗场景查询接口。
    /// </summary>
    public interface ILF2SceneQuery
    {
        /// <summary>
        /// 查询 body 体积与指定体积相交的实体。
        /// </summary>
        List<LF2Entity> QueryBodies(in PhysicsState.BattleVolume vol, LF2Entity exclude);
    }
}
