using NTSD.Animation;
using NTSD.App;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// NTSD 战斗对象的确定性模拟调度器，具体实现已拆分到各个 partial 文件中。
    /// </summary>
    public partial class SimulationWorld
    {
        public bool PpMode => NTSDGlobal.MPEnabled;

        public void QueueSound(string soundId, int worldX)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld(worldX, 0f);
            AppManager.Instance?.SoundPlayer?.PlaySfx(
                soundId,
                new Vector3(groundPoint.x, groundPoint.y, 0f));
        }
    }
}
