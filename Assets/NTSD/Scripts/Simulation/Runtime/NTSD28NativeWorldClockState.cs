using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Native NTSD 2.8 world clocks committed at C23/C24. These are distinct
    /// from Unity host tick and the earlier input/teleport cadence carriers.
    /// </summary>
    [Serializable]
    public sealed class NTSD28NativeWorldClockState
    {
        public int ResourcePhase12;
        public int ResourcePhase3;
        public ulong FrameSequence;

        public void Reset()
        {
            ResourcePhase12 = 0;
            ResourcePhase3 = 0;
            FrameSequence = 0UL;
        }

        internal void AdvanceResourcePhases()
        {
            ResourcePhase12 = (ResourcePhase12 + 1) % 12;
            ResourcePhase3 = (ResourcePhase3 + 1) % 3;
        }

        internal void AdvanceFrameSequence()
        {
            FrameSequence++;
        }
    }
}
