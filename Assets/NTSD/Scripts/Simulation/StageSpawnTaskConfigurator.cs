using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the deterministic mapping from stage spawn data to an opoint task.
    /// Production callers supply a pooled task; CreateCold is reserved for setup
    /// and diagnostics before the battle allocation boundary is sealed.
    /// </summary>
    public sealed class StageSpawnTaskConfigurator
    {
        public OPointCreateTask CreateCold(
            BattleStageSpawnData spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int requiredRuntimeSlot = -1)
        {
            BattleStageSpawnValue value = spawn.ToValue();
            return CreateCold(
                value,
                spawnX,
                spawnY,
                spawnZ,
                facingDir,
                requiredRuntimeSlot);
        }

        public OPointCreateTask CreateCold(
            in BattleStageSpawnValue spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int requiredRuntimeSlot = -1)
        {
            var task = new OPointCreateTask();
            Configure(
                task,
                spawn,
                spawnX,
                spawnY,
                spawnZ,
                facingDir,
                requiredRuntimeSlot);
            return task;
        }

        public void Configure(
            OPointCreateTask task,
            BattleStageSpawnData spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int requiredRuntimeSlot = -1)
        {
            BattleStageSpawnValue value = spawn.ToValue();
            Configure(
                task,
                value,
                spawnX,
                spawnY,
                spawnZ,
                facingDir,
                requiredRuntimeSlot);
        }

        public void Configure(
            OPointCreateTask task,
            in BattleStageSpawnValue spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int requiredRuntimeSlot = -1)
        {
            if (task == null)
                return;

            task.Clear();
            task.opoint = new ObjectPoint
            {
                oid = spawn.Id,
                kind = 0,
                action = spawn.Act,
                x = spawnX,
                y = spawnY,
                facing = 0,
            };
            task.parent = null;
            task.team = 2;
            task.requiredRuntimeSlot = requiredRuntimeSlot;
            task.relationTeam = 2;
            task.holderCopySlot = -1;
            task.useExplicitRelationIdentity = true;
            task.pos = new Vector3(spawnX, spawnY, 0f);
            task.z = spawnZ;
            task.dir = facingDir;
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.frameDelay = 0;
            task.attackExempt = 0;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.StageSpawnAt;
        }
    }
}
