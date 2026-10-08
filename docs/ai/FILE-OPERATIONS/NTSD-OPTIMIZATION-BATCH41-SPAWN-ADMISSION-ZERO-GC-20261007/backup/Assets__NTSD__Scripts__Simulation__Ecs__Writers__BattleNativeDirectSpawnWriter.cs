using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeDirectSpawnWriter
    {
        internal static bool IsInitialActionAdmitted(LF2CharacterDataWrapper wrapper, int action)
        {
            // Alignment contract: NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001.
            return wrapper?.characterData != null && action >= 0 &&
                action < LF2FrameCache.NativeMaxFrameIdExclusive &&
                (action != 999 || wrapper.characterData.frames.Exists(frame => frame.frameId == action));
        }

        internal static void InitializeBirth(LF2Entity entity, OPointCreateTask task)
        {
            var data = entity.FrameCache.Wrapper.characterData;
            var runtime = entity.Runtime;
            entity.RelationTeam = task.relationTeam;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Health.PP = 500;
            entity.Health.MaxMP = data.NativeMetadata?.Stats.Int32OrDefault("max_mp", 500) ?? 500;
            runtime.HP2Orig = 1;
            runtime.HPOrig = 0;
            runtime.RespawnCount = 0;
            BattleNativeDisplayWriter.InitializeBirth(runtime, 500);
            runtime.WeaponFlightCounter = data.NativeMetadata?.Bmp.Int32OrDefault("weapon_hp", 0) ?? data.weapon_hp;
            entity.InitializeNativeDefinitionIdentityForSpawn();
            entity.InitializeNativeArmorRuntimeFromCurrentDefinitionForSpawn();
            runtime.IncomingDamageScale340 = 0;
            runtime.ModeDamageScalePercent = 100;
            runtime.HitCount = 0;
            runtime.KnockbackVx = 0;
            runtime.KnockbackVy = 0;
            runtime.KnockbackVz = 0;
            entity.WriteCurrentFrameId(task.opoint.action);
            entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(task.opoint.action);
            entity.Trans.SyncDirectFrameData(entity.Frame.D.wait, entity.Frame.D.next, task.opoint.action);
            entity.Frame.Prev = task.opoint.action;
            entity.AttackingCounter = 0;
            entity.SyncCollisionSnapshotToCurrentFrame();
            runtime.NativeSoundActionLatch = -1;
            runtime.NativeLifecycleResolutionPending = false;
            runtime.NativeLifecycleCode = 0;
            runtime.NativeRuntimeStateCode = 0;
        }

    }
}
