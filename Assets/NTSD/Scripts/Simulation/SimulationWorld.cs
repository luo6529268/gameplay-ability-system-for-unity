using System.Collections.Generic;

namespace NTSD.Simulation
{
    public readonly struct PendingSoundEvent
    {
        public PendingSoundEvent(string cue, int worldX, int tick)
        {
            Cue = cue;
            WorldX = worldX;
            Tick = tick;
        }

        public string Cue { get; }
        public int WorldX { get; }
        public int Tick { get; }
    }

    public interface ISimulationSoundPresentationSink
    {
        void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds);
    }

    /// <summary>
    /// NTSD 战斗对象的确定性模拟调度器，具体实现已拆分到各个 partial 文件中。
    /// </summary>
    public partial class SimulationWorld
    {
        internal int ActiveDataObjectTypeCacheTick { get; private set; } = -1;

        public bool PpMode => NTSDGlobal.MPEnabled;
        public List<PendingSoundEvent> PendingSounds { get; } = new List<PendingSoundEvent>();
        public long QueuedSoundEventCountForDiagnostics { get; private set; }

        public void QueueSound(string soundId, int worldX)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            PendingSounds.Add(new PendingSoundEvent(soundId, worldX, CurrentTickIndex));
            QueuedSoundEventCountForDiagnostics++;
        }

        internal void BeginDataObjectTypeTickCache(int tickIndex)
        {
            ActiveDataObjectTypeCacheTick = tickIndex;
        }

        internal void EndDataObjectTypeTickCache()
        {
            ActiveDataObjectTypeCacheTick = -1;
        }
    }
}
