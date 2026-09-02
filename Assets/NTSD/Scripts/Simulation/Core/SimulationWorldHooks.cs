using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// Per-world diagnostic and self-check hooks. Keeping these on the world avoids
    /// cross-match state leaking through mutable static fields.
    /// </summary>
    internal sealed class SimulationWorldHooks
    {
        internal Func<SimulationWorld, LF2Entity, LF2Entity>
            RespawnEffectSpawnOverride { get; set; }

#if UNITY_INCLUDE_TESTS
        internal Action<SimulationWorld, LF2Entity>
            CharacterInputPassMutationOverride { get; set; }
#endif
    }
}
