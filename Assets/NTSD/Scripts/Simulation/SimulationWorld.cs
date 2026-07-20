using System.Collections.Generic;
using NTSD.Animation;
using NTSD.App;
using UnityEngine;

namespace NTSD.Simulation
{
    public sealed class PendingSoundEvent
    {
        public string Cue;
        public int WorldX;
        public int Tick;
    }

    /// <summary>
    /// NTSD 战斗对象的确定性模拟调度器，具体实现已拆分到各个 partial 文件中。
    /// </summary>
    public partial class SimulationWorld
    {
        public bool PpMode => NTSDGlobal.MPEnabled;
        public List<PendingSoundEvent> PendingSounds { get; } = new List<PendingSoundEvent>();

        public void QueueSound(string soundId, int worldX)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            PendingSounds.Add(new PendingSoundEvent
            {
                Cue = soundId,
                WorldX = worldX,
                Tick = CurrentTickIndex,
            });

            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld(worldX, 0f);
            AppManager.Instance?.SoundPlayer?.PlaySfx(
                soundId,
                new Vector3(groundPoint.x, groundPoint.y, 0f));
        }
    }
}
