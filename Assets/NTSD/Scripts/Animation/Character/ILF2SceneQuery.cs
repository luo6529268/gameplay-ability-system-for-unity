using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// Release battle scene query interface.
    /// </summary>
    public interface ILF2SceneQuery
    {
        /// <summary>
        /// Queries entities whose body volumes intersect the supplied volume.
        /// </summary>
        List<LF2Entity> QueryBodies(in PhysicsState.BattleVolume vol, LF2Entity exclude);

        /// <summary>
        /// Queries entities whose itr volumes intersect the supplied volume and match kind.
        /// </summary>
        List<LF2Entity> QueryItrs(in PhysicsState.BattleVolume vol, LF2Entity exclude, int itrKind, int excludeTeam = 0);
    }
}